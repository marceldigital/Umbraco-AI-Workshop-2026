using System.Net;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Umbraco.AI.Core.Chat;
using Umbraco.AI.Core.SpeechToText;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace TheRabbitHole.Core;

// Background service that processes podcast episodes from the queue: transcribes the audio, generates show notes
// using AI, and saves the results back to Umbraco.
public class PodcastEpisodeTranscriber(PodcastEpisodeQueue queue, IServiceScopeFactory scopeFactory,
    IHubContext<PodcastHub> hub, ILogger<PodcastEpisodeTranscriber> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Continuously read from the queue until the service is stopped.
        // Each content key represents a podcast episode to process.
        await foreach (var contentKey in queue.Reader.ReadAllAsync(stoppingToken))
        {
            // Mark the episode as in-flight so the save-notification handler ignores any saves
            // that happen during processing (otherwise every save we make would re-enqueue it).
            if (!queue.BeginProcessing(contentKey))
                continue;

            try
            {
                // Run the processing logic in an isolated context to avoid any AsyncLocal state
                // (like logging scopes) from flowing into the background work.
                await RunIsolated(() => ProcessAsync(contentKey, stoppingToken));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process podcast episode {Id}", contentKey);
            }
            finally
            {
                queue.EndProcessing(contentKey);
            }
        }
    }

    // Isolate the worker from producer-thread AsyncLocal state (e.g. ambient scopes).
    private static Task RunIsolated(Func<Task> action)
    {
        using (ExecutionContext.SuppressFlow())
            return Task.Run(action);
    }

    // The main processing logic: transcribe the audio, generate show notes, save to Umbraco, and notify clients via SignalR.
    private async Task ProcessAsync(Guid contentKey, CancellationToken ct)
    {
        await ProcessManualAsync(contentKey, ct);
    }

    private async Task ProcessManualAsync(Guid contentKey, CancellationToken ct)
    {
        // Create a new scope to resolve services for this unit of work, ensuring we have a clean context
        // for Umbraco services and AI clients.
        using var scope = scopeFactory.CreateScope();
        var sp = scope.ServiceProvider;
        var contentService = sp.GetRequiredService<IContentService>();
        var mediaFileManager = sp.GetRequiredService<MediaFileManager>();
        var stt = sp.GetRequiredService<IAISpeechToTextService>();
        var chat = sp.GetRequiredService<IAIChatService>();
        var jsonSerializer = sp.GetRequiredService<IJsonSerializer>();

        // Load the content item to process. We have the content key from the queue, but we need to load the full item
        var content = contentService.GetById(contentKey);
        if (content is null)
            return;

        bool contentHasChanged = false;

        // Check if the episode has already been processed (e.g. if a transcript already exists) to avoid duplicate work.
        string transcript = content.GetValue<string>("transcript") ?? string.Empty;
        if (string.IsNullOrWhiteSpace(transcript))
        {
            var audioPath = content.GetValue<string>("audioFile");
            if (string.IsNullOrWhiteSpace(audioPath))
                return;

            logger.LogInformation("Transcribing podcast episode {Id}", contentKey);

            await using var audio = mediaFileManager.FileSystem.OpenFile(audioPath);
            var sttResponse = await stt.TranscribeAsync(
                b => b.WithAlias("podcast-episode-transcription"),
                audio, ct);
            transcript = sttResponse.Text;

            content.SetValue("transcript", transcript);
            contentHasChanged = true;
        }

        if (string.IsNullOrWhiteSpace(transcript))
            return;

        var summary = content.GetValue<string>("summary") ?? string.Empty;
        if (string.IsNullOrWhiteSpace(summary))
        {
            string summaryPrompt =
                "You are a podcast producer. Given a raw transcript, write a concise 1–2 sentence summary of the episode " +
                "suitable for a listing card. Respond with plain text only — no markdown, no HTML, no quotes.\n" +
                "\n" +
                "## Episode Context\n" +
                $"Episode Key: {contentKey}";

            var summaryResponse = await chat.GetChatResponseAsync(
                b => b.WithAlias("podcast-episode-summary")
                    .WithProfile("podcast-profile"),
                [
                    new ChatMessage(ChatRole.System, summaryPrompt),
                    new ChatMessage(ChatRole.User, transcript)
                ], ct);

            summary = summaryResponse.Text;
            content.SetValue("summary", summary);
            contentHasChanged = true;
        }

        var showNotes = content.GetValue<string>("showNotes") ?? string.Empty;
        if (IsRichTextEmpty(showNotes, jsonSerializer))
        {
            string showNotesPrompt =
                "You are a podcast producer. Given a raw transcript, write HTML show notes for publication: " +
                "a short overview paragraph, a bulleted list of key topics, and any resources or guests mentioned. " +
                "Respond with valid HTML only — no markdown, no code fences, no <html>/<body> wrappers.\n" +
                "\n" +
                "If a previous episode is mentioned, include it and link it to the show, but only if you know the URL.\n" +
                "\n" +
                "## Episode Context\n" +
                $"Episode Key: {contentKey}";

            var showNotesResponse = await chat.GetChatResponseAsync(
                b => b.WithAlias("podcast-episode-show-notes")
                    .WithProfile("podcast-profile"),
                [
                    new ChatMessage(ChatRole.System, showNotesPrompt),
                    new ChatMessage(ChatRole.User, transcript)
                ], ct);

            showNotes = showNotesResponse.Text;
            content.SetValue("showNotes", showNotes);
            contentHasChanged = true;
        }

        // Save the transcript, summary and show notes back to the content item.
        if (contentHasChanged)
        {
            contentService.Save(content);
            logger.LogInformation("Podcast episode {Id} transcribed, summarised and show notes saved", contentKey);

            // Notify any connected backoffice clients that this episode has been processed,
            // so they can update the UI in real time if needed.
            await hub.Clients.All.SendAsync("episodeProcessed", contentKey, content.Name, ct);
        }
    }

    // The Tiptap rich-text editor persists even a manually cleared field as a JSON envelope
    // with "<p></p>" markup and empty block collections, so a plain IsNullOrWhiteSpace check
    // will report it as non-empty.
    private bool IsRichTextEmpty(string? value, IJsonSerializer jsonSerializer)
    {
        if (string.IsNullOrWhiteSpace(value))
            return true;

        if (!RichTextPropertyEditorHelper.TryParseRichTextEditorValue(value, jsonSerializer, logger, out var rte))
            return false;

        if (rte.Blocks?.ContentData is { Count: > 0 })
            return false;

        var stripped = WebUtility.HtmlDecode(rte.Markup?.StripHtml() ?? string.Empty);
        return string.IsNullOrWhiteSpace(stripped);
    }
}

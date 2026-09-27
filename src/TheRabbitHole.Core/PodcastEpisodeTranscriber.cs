using System.Net;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
        // Lesson 2: transcribe the episode, write a summary and show notes, then save and notify.
        await Task.CompletedTask;
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

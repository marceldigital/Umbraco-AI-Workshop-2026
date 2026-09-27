using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Umbraco.AI.Core.SpeechToText;
using Umbraco.AI.Core.Tools;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace TheRabbitHole.Core.Tools;

public sealed record TranscribeEpisodeArgs(
    [property: Description("The Umbraco content key (GUID) of the podcast episode to transcribe.")]
    Guid EpisodeKey);

[AITool("transcribe_episode", "Transcribe Episode", ScopeId = "podcast")]
public sealed class TranscribeEpisodeTool : EpisodeToolBase<TranscribeEpisodeArgs>
{
    private const int PreviewLength = 300;

    private readonly MediaFileManager _mediaFileManager;
    private readonly IAISpeechToTextService _stt;
    private readonly IBackOfficeSecurityAccessor _securityAccessor;
    private readonly ILogger<TranscribeEpisodeTool> _logger;

    public TranscribeEpisodeTool(
        IContentService contentService,
        MediaFileManager mediaFileManager,
        IAISpeechToTextService stt,
        IBackOfficeSecurityAccessor securityAccessor,
        ILogger<TranscribeEpisodeTool> logger) : base(contentService)
    {
        _mediaFileManager = mediaFileManager;
        _stt = stt;
        _securityAccessor = securityAccessor;
        _logger = logger;
    }

    public override string Description =>
        "Transcribes the audio file attached to a podcast episode and saves the transcript onto the episode. " +
        "Returns an acknowledgement with the character count and a short preview — the full text is persisted " +
        "directly (podcast transcripts are too large to pass around as tool arguments). " +
        "Call get_episode afterwards if you need the full transcript to produce summary or show notes.";

    protected override async Task<object> ExecuteAsync(
        TranscribeEpisodeArgs args,
        CancellationToken cancellationToken = default)
    {
        var (episode, error) = GetEpisodeOrError(args.EpisodeKey);
        if (episode is null)
            return error!;

        var audioPath = episode.GetValue<string>("audioFile");
        if (string.IsNullOrWhiteSpace(audioPath))
            return new { Success = false, Message = "Episode has no audio file to transcribe." };

        _logger.LogInformation("Transcribing podcast episode {Id}", args.EpisodeKey);

        var currentUser = _securityAccessor.BackOfficeSecurity?.CurrentUser;
        
        await using var audio = _mediaFileManager.FileSystem.OpenFile(audioPath);
        var sttResponse = await _stt.TranscribeAsync(
            b => b.WithAlias("podcast-episode-transcription"),
            audio, cancellationToken);

        var transcript = sttResponse.Text ?? string.Empty;
        episode.SetValue("transcript", transcript);
        ContentService.Save(episode, currentUser?.Id);

        var preview = transcript.Length <= PreviewLength
            ? transcript
            : transcript[..PreviewLength] + "…";

        return new
        {
            Success = true,
            CharacterCount = transcript.Length,
            Preview = preview,
        };
    }
}

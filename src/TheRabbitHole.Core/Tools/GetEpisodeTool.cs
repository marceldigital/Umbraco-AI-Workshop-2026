using System.ComponentModel;
using Umbraco.AI.Core.Tools;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace TheRabbitHole.Core.Tools;

public sealed record GetEpisodeArgs(
    [property: Description("The Umbraco content key (GUID) of the podcast episode.")]
    Guid EpisodeKey);

[AITool("get_episode", "Get Episode", ScopeId = "podcast")]
public sealed class GetEpisodeTool(IContentService contentService)
    : EpisodeToolBase<GetEpisodeArgs>(contentService)
{
    public override string Description =>
        "Return the current state of a podcast episode: name, whether audio is attached, and the current transcript, " +
        "summary, show notes, and guest emails. Call at the start of a run to see which fields still need work.";

    protected override Task<object> ExecuteAsync(
        GetEpisodeArgs args,
        CancellationToken cancellationToken = default)
    {
        var (episode, error) = GetEpisodeOrError(args.EpisodeKey);
        if (episode is null)
            return Task.FromResult(error!);

        var guestEmails = (episode.GetValue<string>("guests")?.Split('\n') ?? [])
            .Select(e => e.Trim())
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .ToArray();

        return Task.FromResult<object>(new
        {
            Success = true,
            Name = episode.Name,
            HasAudio = !string.IsNullOrWhiteSpace(episode.GetValue<string>("audioFile")),
            Transcript = episode.GetValue<string>("transcript"),
            Summary = episode.GetValue<string>("summary"),
            ShowNotes = episode.GetValue<string>("showNotes"),
            GuestEmails = guestEmails,
        });
    }
}

using System.ComponentModel;
using Umbraco.AI.Core.Tools;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace TheRabbitHole.Core.Tools;

public sealed record SaveEpisodeSummaryArgs(
    [property: Description("The Umbraco content key (GUID) of the podcast episode.")]
    Guid EpisodeKey,
    [property: Description("A plain-text summary suitable for a listing card. No markdown, no HTML, no quotes.")]
    string Summary);

[AITool("save_episode_summary", "Save Episode Summary", ScopeId = "podcast")]
public sealed class SaveEpisodeSummaryTool(IContentService contentService, IBackOfficeSecurityAccessor securityAccessor)
    : SaveEpisodePropertyToolBase<SaveEpisodeSummaryArgs>(contentService, securityAccessor)
{
    public override string Description =>
        "Persist a plain-text episode summary onto a podcast episode. The value must be plain text with no markdown, HTML, or surrounding quotes.";

    protected override string PropertyAlias => "summary";

    protected override (Guid EpisodeKey, object? Value) Extract(SaveEpisodeSummaryArgs args)
        => (args.EpisodeKey, args.Summary);
}

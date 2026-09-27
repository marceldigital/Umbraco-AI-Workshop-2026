using System.ComponentModel;
using Umbraco.AI.Core.Tools;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace TheRabbitHole.Core.Tools;

public sealed record SaveEpisodeShowNotesArgs(
    [property: Description("The Umbraco content key (GUID) of the podcast episode.")]
    Guid EpisodeKey,
    [property: Description("A valid HTML body fragment. No markdown, no code fences, no <html>/<body> wrappers.")]
    string ShowNotes);

[AITool("save_episode_show_notes", "Save Episode Show Notes", ScopeId = "podcast")]
public sealed class SaveEpisodeShowNotesTool(IContentService contentService, IBackOfficeSecurityAccessor securityAccessor)
    : SaveEpisodePropertyToolBase<SaveEpisodeShowNotesArgs>(contentService, securityAccessor)
{
    public override string Description =>
        "Persist HTML show notes onto a podcast episode. Input must be a valid HTML body fragment with no markdown, no code fences, and no <html>/<body> wrappers.";

    protected override string PropertyAlias => "showNotes";

    protected override (Guid EpisodeKey, object? Value) Extract(SaveEpisodeShowNotesArgs args)
        => (args.EpisodeKey, args.ShowNotes);
}

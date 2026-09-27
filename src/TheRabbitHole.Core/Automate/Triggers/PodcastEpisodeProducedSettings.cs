using Umbraco.Automate.Core.Settings;

namespace TheRabbitHole.Core.Automate.Triggers;

public sealed class PodcastEpisodeProducedSettings
{
    [Field(
        Label = "Only episodes with guests",
        Description = "Only fire when at least one guest was credited in the show notes.",
        EditorUiAlias = "Umb.PropertyEditorUi.Toggle")]
    public bool OnlyWithGuests { get; set; }
}

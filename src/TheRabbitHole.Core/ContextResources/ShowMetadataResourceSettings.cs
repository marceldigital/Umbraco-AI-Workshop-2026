using Umbraco.AI.Core.EditableModels;

namespace TheRabbitHole.Core.ContextResources;

public sealed class ShowMetadataResourceSettings
{
    [AIField(
        Label = "Site Root",
        Description = "Select the podcast site root (Home) node. Show metadata and the last 3 published episodes will be resolved from its descendants.",
        EditorUiAlias = "Umb.PropertyEditorUi.DocumentPicker",
        SortOrder = 10)]
    public Guid? SiteRootId { get; set; }
}

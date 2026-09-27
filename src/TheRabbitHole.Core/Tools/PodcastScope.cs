using Umbraco.AI.Core.Tools.Scopes;

namespace TheRabbitHole.Core.Tools;

// A tool scope groups related tools so permissions can be granted to the whole group at once.
[AIToolScope("podcast", Icon = "icon-mic", Domain = "Podcast")]
public sealed class PodcastScope : AIToolScopeBase;

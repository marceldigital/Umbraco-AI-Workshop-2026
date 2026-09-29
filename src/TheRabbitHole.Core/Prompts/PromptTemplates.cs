using System.Collections.Concurrent;
using Fluid;
using TheRabbitHole.Core.ContextResources;

namespace TheRabbitHole.Core.Prompts;

// Renders the Liquid prompt templates in the Prompts folder. They're embedded in this assembly at build time, so they
// ship with the code and load by name. Fluid is the same Liquid engine Semantic Kernel's Liquid templates use.
public sealed class PromptTemplates
{
    private readonly FluidParser _parser = new();
    private readonly TemplateOptions _options = new();
    private readonly ConcurrentDictionary<string, IFluidTemplate> _cache = new();

    public PromptTemplates()
    {
        // Liquid templates use camelCase names ({{ episode.name }}) for our PascalCase properties.
        _options.MemberAccessStrategy.MemberNameStrategy = MemberNameStrategies.CamelCase;

        // A template can only read the types registered here.
        _options.MemberAccessStrategy.Register<ShowMetadataResourceOutput>();
        _options.MemberAccessStrategy.Register<EpisodeSummary>();
    }

    public string Render(string name, object model)
    {
        IFluidTemplate template = _cache.GetOrAdd(name, Load);

        return template.Render(new TemplateContext(model, _options)).Trim();
    }

    private IFluidTemplate Load(string name)
    {
        string resourceName = $"Prompts/{name}.liquid";

        using Stream stream = typeof(PromptTemplates).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Prompt template '{resourceName}' wasn't found. Is it an EmbeddedResource?");
        using var reader = new StreamReader(stream);

        if (!_parser.TryParse(reader.ReadToEnd(), out IFluidTemplate template, out string error))
            throw new InvalidOperationException($"Prompt template '{resourceName}' isn't valid Liquid: {error}");

        return template;
    }
}

# Lesson 7 (bonus): Prompt templates

> ⏱️ **Take-home bonus · ~30 minutes** · Starting branch: `lesson-6-end` · Finished branch: `lesson-7-end`

> [!TRACKER]
> ✅ Transcribe podcast audio · ✅ Generate a summary and show notes · ✅ Cross-link mentioned episodes · ✅ Validate guest details

## Objective

Every requirement is ticked, so this lesson is about keeping the code healthy as the prompts grow. So far, prompt text
lives in C#: string concatenation in Lesson 2, and a `StringBuilder` loop in Lesson 3's `FormatDataForLlm`. That's fine
for a few lines, but it gets hard to read and review once a prompt has loops and conditions.

You'll move the Show Metadata text (the block every agent run sees) into a **Liquid template**, rendered by
[Fluid](https://github.com/sebastienros/fluid). Then you'll change what the model sees by editing only the template.

Remember Lecture 3's "Where should prompts live?" slide: in code, in template files, or in the backoffice. This lesson is
the middle option.

> [!NOTE]
> Used Semantic Kernel's `Microsoft.SemanticKernel.PromptTemplates.Liquid`? Fluid is the Liquid engine inside it, so the
> syntax is the same: `{{ variable }}`, `{% for %}`, `{% if %}` and filters. Microsoft.Extensions.AI, which Umbraco AI is
> built on, has no templating of its own, and Fluid doesn't need Semantic Kernel.

## What you'll change

- `src/TheRabbitHole.Core/TheRabbitHole.Core.csproj`: add the `Fluid.Core` package and embed `Prompts/*.liquid`
- New: `src/TheRabbitHole.Core/Prompts/show-metadata.liquid`, the template
- New: `src/TheRabbitHole.Core/Prompts/PromptTemplates.cs`, a small service that loads and renders templates
- `src/TheRabbitHole.Core/UmbracoBuilderExtensions.cs`: register `PromptTemplates`
- `src/TheRabbitHole.Core/ContextResources/ShowMetadataResourceType.cs`: `FormatDataForLlm` renders the template

No backoffice changes: the Show Metadata context, the agent and both automations stay exactly as Lesson 6 left them.

## Steps

### Step 1: Add Fluid

Stop the site (**Ctrl+C**). Then add the Liquid engine to the class library:

```bash
dotnet add src/TheRabbitHole.Core package Fluid.Core --version 2.40.0
```

### Step 2: Embed the templates

Templates will live in a new `Prompts` folder. Embedding them compiles them into the assembly, so they ship with the code
and there's no file path to get wrong at runtime.

Open **`src/TheRabbitHole.Core/TheRabbitHole.Core.csproj`** and add this `ItemGroup` directly below the one with the
package references:

```xml
    <ItemGroup>
        <!-- Prompt templates are compiled into the assembly and loaded by name, e.g. "Prompts/show-metadata.liquid". -->
        <EmbeddedResource Include="Prompts\*.liquid" LogicalName="Prompts/%(Filename)%(Extension)" />
    </ItemGroup>
```

`LogicalName` gives each template a predictable resource name, `Prompts/show-metadata.liquid`, instead of the
namespace-mangled default.

### Step 3: Write the template

First, reproduce exactly what `FormatDataForLlm` builds today. Create a new file
**`src/TheRabbitHole.Core/Prompts/show-metadata.liquid`** and add:

```liquid
{%- comment -%}
  The Show Metadata context resource (Lesson 3), as the model sees it.
  The model is a ShowMetadataResourceOutput: showName, showDescription and latestEpisodes.
{%- endcomment -%}
{%- if showName != blank %}
Show: {{ showName }}
{%- endif %}
{%- if showDescription != blank %}
Description: {{ showDescription }}
{%- endif %}
{%- if latestEpisodes != empty %}
Latest episodes:
{%- for episode in latestEpisodes %}
- #{{ episode.number }} "{{ episode.name }}" — {{ episode.url }}
{%- endfor %}
{%- endif %}
```

What's happening:

- **`{{ … }}`** outputs a value. **`{% … %}`** is logic: `if`, `for`, `comment`.
- The template reads the model's properties in **camelCase** (`showName` for `ShowName`). You'll switch that on in the
  next step.
- **`blank`** means null, empty or whitespace, and **`empty`** means an empty list. Together they replace the three
  `IsNullOrWhiteSpace` and `Count > 0` checks.
- **`{%-`** trims the whitespace just before the tag, including the line break. That's what stops every tag from leaving
  a blank line in the output. Without it, the model would see the gaps.
- The **loop** is the whole reason for Liquid here. The `foreach` and its string interpolation become one readable line.

### Step 4: Create the template renderer

This service loads a template by name, parses it once, and renders it with a model.

**4a. The scaffold.** Create a new file **`src/TheRabbitHole.Core/Prompts/PromptTemplates.cs`** and add:

```csharp
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
    }

    public string Render(string name, object model)
    {
        throw new NotImplementedException();
    }

    private IFluidTemplate Load(string name)
    {
        throw new NotImplementedException();
    }
}
```

What's happening:

- **`FluidParser`** turns template text into an **`IFluidTemplate`**. Parsing is the expensive part, so each template is
  parsed once and kept in **`_cache`**.
- **`TemplateOptions`** controls what templates are allowed to do. You'll configure it next.

**4b. Configure the options.** Replace the empty constructor with:

```csharp
    public PromptTemplates()
    {
        // Liquid templates use camelCase names ({{ episode.name }}) for our PascalCase properties.
        _options.MemberAccessStrategy.MemberNameStrategy = MemberNameStrategies.CamelCase;

        // A template can only read the types registered here.
        _options.MemberAccessStrategy.Register<ShowMetadataResourceOutput>();
        _options.MemberAccessStrategy.Register<EpisodeSummary>();
    }
```

What's happening:

- **`MemberNameStrategies.CamelCase`** maps `{{ episode.name }}` to `EpisodeSummary.Name`, so templates read like
  Liquid rather than C#.
- **`Register<T>()`** is an allow-list. A template can only read properties of registered types, so it can't wander into
  anything else you pass it. That matters more once templates are edited by people other than you.

**4c. Load a template.** Replace the `Load` method with:

```csharp
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
```

What's happening:

- The resource name is the `LogicalName` from Step 2.
- Both failure modes (a missing template and a Liquid syntax error) throw with a message that says exactly what's wrong.
  The Liquid error includes the line and column.

**4d. Render.** Replace the `Render` method with:

```csharp
    public string Render(string name, object model)
    {
        IFluidTemplate template = _cache.GetOrAdd(name, Load);

        return template.Render(new TemplateContext(model, _options)).Trim();
    }
```

What's happening:

- **`GetOrAdd`** loads and parses the template on first use, then reuses it.
- **`new TemplateContext(model, _options)`** makes the model's properties the template's top-level variables:
  `showName`, `showDescription`, `latestEpisodes`.
- **`Render`** is synchronous, because `FormatDataForLlm` is. **`Trim()`** removes the line break the template file
  ends with.

<details>
<summary>Full file so far</summary>

```csharp
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
```

</details>

### Step 5: Register the renderer

Open **`src/TheRabbitHole.Core/UmbracoBuilderExtensions.cs`**. Add `using TheRabbitHole.Core.Prompts;` directly below
`using TheRabbitHole.Core.Integrations.HubSpot;`. Then add the registration directly below the `AddHttpClient` line:

```csharp
        builder.Services.AddHttpClient<HubSpotGuestClient>();

        // Liquid prompt templates (Prompts/*.liquid)
        builder.Services.AddSingleton<PromptTemplates>();
```

It's a singleton, so the parsed templates are shared by every call.

### Step 6: Use it in the Show Metadata resource type

Open **`src/TheRabbitHole.Core/ContextResources/ShowMetadataResourceType.cs`**.

**At the top**, remove `using System.Text;` (the `StringBuilder` is going) and add the prompts namespace, so the first two
usings read:

```csharp
using TheRabbitHole.Core.Models;
using TheRabbitHole.Core.Prompts;
```

**Inject the renderer.** Replace the field and constructor with:

```csharp
    private readonly IUmbracoContextFactory _umbracoContextFactory;
    private readonly PromptTemplates _promptTemplates;

    public ShowMetadataResourceType(
        IAIContextResourceTypeInfrastructure infrastructure,
        IUmbracoContextFactory umbracoContextFactory,
        PromptTemplates promptTemplates)
        : base(infrastructure)
    {
        _umbracoContextFactory = umbracoContextFactory;
        _promptTemplates = promptTemplates;
    }
```

**Render the template.** Replace the whole `FormatDataForLlm` method with:

```csharp
    protected override string FormatDataForLlm(ShowMetadataResourceOutput data)
        => _promptTemplates.Render("show-metadata", data);
```

What's happening:

- Resource types are created by dependency injection (Lesson 3), so asking for `PromptTemplates` in the constructor is
  all it takes.
- The split is now clean. `ResolveDataAsync` decides **what** the model knows, and the template decides **how it reads**.

### Step 7: Check nothing changed

Run the site:

```bash
dotnet run --project src/TheRabbitHole.Web
```

Then produce the episode through Lesson 6's pipeline:

1. Open episode 5, clear the **Summary**, and click **Save and publish**.
2. Wait for the green **Episode produced** toast.
3. Go to **AI → Logs**, open the newest **Agent** entry, and find the **`### Show Metadata`** block in the **Prompt**.

It should read exactly as it did before this lesson:

```text
### Show Metadata
Show: The Rabbit Hole
Description: The latest AI news in Umbraco
Latest episodes:
- #5 "Community, AI, and the Umbraco Way" — https://localhost:44339/episodes/community-ai-and-the-umbraco-way/
- #4 "The 500th Question" — https://localhost:44339/episodes/the-500th-question/
- #3 "The Great Migration" — https://localhost:44339/episodes/the-great-migration/
```

That's the point of this step: a refactor that the model can't tell happened.

### Step 8: Change the prompt, not the code

Now improve what the model sees, as Lesson 3's stretch goal suggested: give it each episode's summary, so it can link
related episodes by topic and not just by name. This time you only edit the template.

In **`show-metadata.liquid`**, add three lines inside the loop, directly below the episode line. The snippet includes the
existing episode line and `{%- endfor %}`, so you can see where the new lines go:

```liquid
- #{{ episode.number }} "{{ episode.name }}" — {{ episode.url }}
{%- if episode.summary != blank %}
  {{ episode.summary | truncate: 160 }}
{%- endif %}
{%- endfor %}
```

What's happening:

- **`if … != blank`** skips episodes without a summary, so there's no empty indented line.
- **`| truncate: 160`** is a **filter**. It cuts long summaries to 160 characters and adds `...`, which keeps the
  context block, and its token cost, under control.

Templates are embedded, so stop the site (**Ctrl+C**) and start it again to rebuild. Then clear the **Summary** of
episode 5 again, click **Save and publish**, and look at the newest Agent entry's Show Metadata block:

```text
Latest episodes:
- #5 "Community, AI, and the Umbraco Way" — https://localhost:44339/episodes/community-ai-and-the-umbraco-way/
- #4 "The 500th Question" — https://localhost:44339/episodes/the-500th-question/
  What happens when an AI bot answers the same Discord question for the 500th time? We look at the weekends it hands back to the DevRel team — and the quiet co...
- #3 "The Great Migration" — https://localhost:44339/episodes/the-great-migration/
  Upgrading a decade-old Umbraco site used to be a six-month death march. We dig into where AI-assisted migrations genuinely save time, where they quietly intr...
```

Episode 5 has no summary line. You'd just cleared it, so the `if` skipped it. That's the condition doing its job.

## ✅ Checkpoint

- ⬜ The solution builds, and `src/TheRabbitHole.Core/Prompts/` contains `show-metadata.liquid` and `PromptTemplates.cs`.
- ⬜ `FormatDataForLlm` is one line.
- ⬜ In Step 7, the Show Metadata block in the logs was identical to Lesson 6's.
- ⬜ After Step 8, episodes with a summary show it, truncated, under their line.
- ⬜ One **Save and publish** still gives one **Episode produced** toast.

## 🔍 Under the hood

**Code decides what, templates decide how.** `ResolveDataAsync` still fetches the data and `FormatDataForLlm` still hands
text to Umbraco AI, so nothing else in the pipeline changed. What moved is the wording and layout, which is the part you
tune most often. Step 8 changed what every agent run sees without touching a line of C#.

**Where the text goes.** Umbraco AI calls `FormatDataForLlm` while it builds each prompt, then puts the result under the
resource's heading in the `## Context` section, exactly as in Lesson 3. The template only produces the text.

**Why not Semantic Kernel's Liquid package?** It would work: render to a string and pass it to `IAIChatService`. But it
brings in Semantic Kernel just for templating. And its `<message role="…">` blocks, which turn one template into a list
of chat messages, are a Semantic Kernel feature that Umbraco AI doesn't read. With Umbraco AI you build the message list
yourself, as in Lesson 2.

**And the Umbraco AI Prompt add-on?** It keeps prompt templates in the backoffice, editable without a deploy. But its
`{{variable}}` syntax only fills in values, with no loops or conditions. It's built for editor-facing actions on
properties, and its prompts can't use tools. It's a good home for editor tools, not for text like this.

## 🧯 Troubleshooting

<details>
<summary><strong>"Prompt template 'Prompts/show-metadata.liquid' wasn't found"</strong></summary>

The template isn't embedded under that name. Check that the file is in `src/TheRabbitHole.Core/Prompts/`, that its name
matches the one you pass to `Render` (without `.liquid`), and that the `EmbeddedResource` line from Step 2, including
`LogicalName`, is in the class library's `.csproj`. Then stop the site and run it again, so it rebuilds.

</details>

<details>
<summary><strong>"… isn't valid Liquid: …"</strong></summary>

The message includes the line and column. The usual causes are a missing `{%- endif %}` or `{%- endfor %}`, or `{{ }}`
where you meant `{% %}`.

</details>

<details>
<summary><strong>A value is blank, or the episode list is missing</strong></summary>

Liquid shows nothing for a value it can't read. Check that the name in the template is camelCase (`showName`, not
`ShowName`), and that both types are registered in the `PromptTemplates` constructor. Without
`Register<EpisodeSummary>()`, the loop runs but every episode line is empty.

</details>

<details>
<summary><strong>My template change doesn't show up</strong></summary>

Templates are embedded and cached, so edits only take effect after a rebuild. Stop the site (**Ctrl+C**) and run it
again. The second stretch goal removes that step.

</details>

## 🆘 Stuck?

Jump to the finished state of this lesson. Stop the site first (**Ctrl+C**), then:

```bash
git stash -u
```

```bash
git checkout lesson-7-end
```

```bash
dotnet run --project src/TheRabbitHole.Web
```

The branch has the finished code. There's no backoffice change in this lesson, so the database is the same as Lesson 6's.

## 🚀 Stretch goals

- **Template the Lesson 2 prompts too.** Move `summaryPrompt` and `showNotesPrompt` from `ProcessManualAsync` into
  `summary.liquid` and `show-notes.liquid`. *Hint: they only need the episode key, so add a `Render` overload that takes
  a dictionary and calls `context.SetValue(key, value)` for each entry. Lesson 6 switched that code path off, so switch
  `ProcessAsync` back to `ProcessManualAsync` while you test, and remember that Automate runs the agent as well.*
- **Edit templates without a restart.** Load them from a folder with a `PhysicalFileProvider` instead of embedding them,
  and clear a template's cache entry when the provider's `Watch` token fires. *Hint: copy the files to the output with
  `CopyToOutputDirectory` instead of `EmbeddedResource`.*
- **Unit-test a template.** Create a small xUnit project that references `TheRabbitHole.Core`, renders `show-metadata`
  with a hand-built `ShowMetadataResourceOutput`, and asserts on the text. *Hint: `PromptTemplates` has no Umbraco
  dependencies, so it's easy to new up in a test.*

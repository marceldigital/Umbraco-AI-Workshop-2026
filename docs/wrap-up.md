# Wrap-up

> [!TRACKER]
> ✅ Transcribe podcast audio · ✅ Generate a summary and show notes · ✅ Cross-link mentioned episodes · ✅ Validate guest details

## What we built

We started with a podcast site where every episode was loaded by hand, and finished with a pipeline that transcribes,
writes, fact-checks, cross-links and notifies on its own. Each requirement maps to an Umbraco AI building block:

| Requirement | Built with | Lesson |
|---|---|---|
| Transcribe podcast audio | Speech-to-text: `IAISpeechToTextService` and a default Speech-to-Text profile | 2 |
| Generate a summary and show notes | Chat: `IAIChatService`, the Podcast Profile and the Brand Voice context | 2 |
| Validate and cross-link mentioned episodes | A custom **context resource type** (Show Metadata) layered onto the profile | 3 |
| Validate and link guest details | A custom **tool** (`get_episode_guests`) backed by the CRM | 4 |
| *All of the above, goal-oriented* | An **agent** (Podcast Producer) with governed tools, run through `IAIAgentService` | 5 |
| *All of the above, durable and editor-configurable* | **Umbraco Automate**: Run AI Agent, a custom trigger, and the built-in Notify Editor | 6 |

The takeaway: Umbraco AI isn't just the copilot and prompt features you see out of the box. It's a foundation you build on,
in the same places you already extend Umbraco: services, notifications, attributes and the backoffice.

## What's next

Start with the bonus lesson: [**Lesson 7: Prompt templates**](lessons/lesson-7.md) moves prompt text out of C# into
Liquid templates, with loops and conditions. It takes about 30 minutes and picks up where Lesson 6 left off.

A few more extension points we didn't have time for:

- **Custom providers.** Plug in any model service by implementing a provider and its capabilities.
- **Guardrails.** Block, warn or redact on the way in or out, including your own evaluators.
- **Chat middleware.** Wrap every chat call for logging, caching or policy.
- **Tests (evals).** Grade prompts and agents automatically so changes don't quietly make things worse.
- **Prompts and Copilot.** Editor-facing AI features, driven by the same profiles, contexts and agents.
- **"Umbraco in AI".** The Developer MCP and the hosted Editor MCP bring your Umbraco site to tools like Claude, ChatGPT and
  Copilot.

## Resources

- This workshop: [marceldigital/Umbraco-AI-Workshop-2026](https://github.com/marceldigital/Umbraco-AI-Workshop-2026)
- Matt Brailsford's original demo: [mattbrailsford/TheRabbitHole](https://github.com/mattbrailsford/TheRabbitHole)
- Umbraco AI documentation: [docs.umbraco.com/ai-in-umbraco](https://docs.umbraco.com/ai-in-umbraco)
- Umbraco Automate documentation: [docs.umbraco.com/umbraco-automate](https://docs.umbraco.com/umbraco-automate)
- Umbraco Automate AI add-on: [docs.umbraco.com/umbraco-automate/add-ons/ai](https://docs.umbraco.com/umbraco-automate/add-ons/ai)

## Share what you build

Built something with Umbraco AI? Write it up, even if it's rough. As Matt put it at Codegarden, it doesn't have to be
perfect to be amazing. A blog post, a gist or a small package might be exactly the spark someone else needs.

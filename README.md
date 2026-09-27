# Beyond the Chatbot: Building Extensible AI Workflows in Umbraco

Hands-on workshop repository for the **Umbraco US Festival 2026** (Chicago, September 30), presented by Marcel Digital.

Over the afternoon you'll take an existing Umbraco 17 podcast site, **The Rabbit Hole**, and extend it with Umbraco AI and
Umbraco Automate. You'll transcribe episode audio, generate summaries and show notes, give the model custom context and tools,
hand the whole job to an agent, and finally orchestrate it with Automate.

## 👉 Workshop instructions

**https://marceldigital.github.io/Umbraco-AI-Workshop-2026/**

Start with **Lesson 0: Setup**.

## Quick start

Prerequisites: [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0), Git, and an editor
([VS Code](https://code.visualstudio.com/) with the C# Dev Kit is assumed).

```bash
git clone https://github.com/marceldigital/Umbraco-AI-Workshop-2026.git
cd Umbraco-AI-Workshop-2026
git checkout main
dotnet build
git checkout init
dotnet run --project src/TheRabbitHole.Web
```

Browse to https://localhost:44339. The backoffice at https://localhost:44339/umbraco signs you in automatically
(a development-only workshop convenience). If you ever need the login, it's `admin@example.com` / `password1234`.

## Branches

| Branch | What's in it |
|---|---|
| `init` | The starting point: the podcast site with no AI features yet |
| `lesson-1-end` … `lesson-6-end` | The finished state of each lesson. Check one out if you get stuck |
| `main` | The finished project plus the workshop docs |

## Credits

The Rabbit Hole demo site and the ideas behind this workshop come from
[Matt Brailsford](https://github.com/mattbrailsford)'s Codegarden 2026 talk *AI in Umbraco* and its companion repository,
[TheRabbitHole](https://github.com/mattbrailsford/TheRabbitHole). They're reused here with his permission. Thank you, Matt!

# Lesson 0: Setup

> ⏱️ **12:55–1:22** · Start as soon as you sit down, keep going while Lecture 1 runs, and finish at the setup checkpoint
> · Branch: `init`

## Objective

Get The Rabbit Hole running on your machine, open the Umbraco backoffice, and save your AI key where the site can
find it. Along the way, download every NuGet package the workshop needs in one go, so the lessons don't wait on
conference Wi-Fi.

## What you'll need

| Tool | Version | Check with |
|---|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download/dotnet/10.0) | 10.0 or later | `dotnet --version` |
| [Git](https://git-scm.com/downloads) | any recent | `git --version` |
| [VS Code](https://code.visualstudio.com/) + the [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit) extension | latest | — |

> [!NOTE]
> The lessons use `dotnet` CLI commands so they work in any editor on any OS. If you prefer Visual Studio or Rider, go for
> it. You'll just translate the commands into your IDE.

You'll also need an **AI key**: your own **OpenAI** or **Microsoft Foundry** key. [Bring your AI key](../ai-key.md) shows
how to get one, and Step 8 covers saving it.

> [!IMPORTANT]
> Missing the .NET 10 SDK? Start that download first, right now. It's the biggest thing on this page.

## Steps

### 1. Clone the workshop repository

```bash
git clone https://github.com/marceldigital/Umbraco-AI-Workshop-2026.git
```

```bash
cd Umbraco-AI-Workshop-2026
```

### 2. Download everything in one go

The `main` branch uses every package you'll add during the lessons. Restoring it now downloads them all into your local
NuGet cache while Lecture 1 runs, so each lesson's `dotnet add package` is instant.

```bash
git checkout main
```

```bash
dotnet restore
```

### 3. Switch to the starting point

```bash
git checkout init
```

`init` is the podcast site as a client might hand it to you: a working Umbraco 17 site with no AI features yet.

### 4. Trust the .NET development certificate

The site runs on HTTPS locally. Trust the development certificate so your browser doesn't complain:

```bash
dotnet dev-certs https --trust
```

Accept the prompt your OS shows you.

### 5. Run the site

```bash
dotnet run --project src/TheRabbitHole.Web
```

The first start takes a minute: it builds the solution and boots Umbraco. When you see `Now listening on:
https://localhost:44339` it's ready.

> [!NOTE]
> The build shows two warnings: `Parameter 'scopeFactory' is unread` and `Parameter 'hub' is unread`. They're expected.
> Those are the services you'll put to work in Lesson 2.

> [!TIP]
> To stop the site, press **Ctrl+C** in the terminal. Most lessons ask you to stop the site, make changes, and start it again.

### 6. Visit the site and the backoffice

1. Open **https://localhost:44339**. You should see The Rabbit Hole homepage listing four episodes.
2. Open **https://localhost:44339/umbraco**. You're signed in straight away as the **Administrator**, with no login
   screen.

> [!NOTE]
> That's a **development-only** convenience built into the workshop site, so nobody has to type a password after
> switching branches. If you ever do see the login screen (after logging out, or a long break), click **Sign in with
> Developer login**, or log in with `admin@example.com` / `password1234`.

<details>
<summary>Deep dive: how the developer login works</summary>

The code is in `src/TheRabbitHole.Web/AutoLogin/`, wired up by `.AddAutoLogin(builder.Environment)` in `Program.cs`.

- It registers a backoffice **external login provider**, the same mechanism you'd use for Microsoft Entra ID or Google.
  But its "remote" sign-in never leaves the site: it signs in the user named in `Autologin:Backoffice:Email` in
  `appsettings.Development.json`.
- It also tells the backoffice login page to redirect to that provider automatically, which is why you never see the
  page.
- It only switches on in the **Development** environment. Never enable anything like it on a real site: anyone who
  could reach the backoffice would be signed in as that user.
- `appsettings.Development.json` also raises the backoffice session timeout (`Umbraco:CMS:Global:TimeOut`) to a day,
  so you aren't timed out while you're busy in your editor.

</details>

### 7. Find your AI key

You need three values: an **API key**, a **chat endpoint** and a **transcription endpoint**.

- **Already have an OpenAI or Microsoft Foundry key?** Use it with the endpoints in the tabs below.
- **No key yet?** [Bring your AI key](../ai-key.md) has the steps for both providers. Adding billing or deploying models
  can take a while, so leave the site running and come back here when you have your key.

### 8. Save your key and endpoints as user secrets

Pick your path and run the three commands from the repo root. Replace the key placeholder with your own key. You can run
them while the site is running; it picks them up the next time it starts, which is the first thing Lesson 1 does.

# [OpenAI](#tab/openai)

```bash
dotnet user-secrets set "Umbraco:AI:Secrets:ApiKey" "<your-openai-key>" --project src/TheRabbitHole.Web
```

```bash
dotnet user-secrets set "Umbraco:AI:Variables:ChatEndpoint" "https://api.openai.com/v1/" --project src/TheRabbitHole.Web
```

```bash
dotnet user-secrets set "Umbraco:AI:Variables:TranscriptionEndpoint" "https://api.openai.com/v1/" --project src/TheRabbitHole.Web
```

On OpenAI, one endpoint serves both chat and transcription, so both variables get the same value.

# [Microsoft Foundry](#tab/foundry)

Replace `<your-resource>` with your Foundry resource name.

```bash
dotnet user-secrets set "Umbraco:AI:Secrets:ApiKey" "<your-foundry-key>" --project src/TheRabbitHole.Web
```

```bash
dotnet user-secrets set "Umbraco:AI:Variables:ChatEndpoint" "https://<your-resource>.services.ai.azure.com/openai/v1/" --project src/TheRabbitHole.Web
```

```bash
dotnet user-secrets set "Umbraco:AI:Variables:TranscriptionEndpoint" "https://<your-resource>.services.ai.azure.com/openai/deployments/gpt-4o-transcribe?api-version=2025-03-01-preview" --project src/TheRabbitHole.Web
```

Foundry serves chat on its OpenAI-compatible `/openai/v1/` endpoint, but transcription only on the deployment's own URL.
That's why the two variables differ. Lesson 1 shows how the connections handle it.

---

User secrets live in your user profile, not in the repository, so they can't be committed by accident, and they stay put
when you switch branches. [Lesson 1, Step 4](lesson-1.md#step-4-check-your-key-and-endpoints) explains how Umbraco AI
reads them.

## ✅ Checkpoint

- ⬜ The homepage lists 4 episodes, including **The 500th Question**.
- ⬜ The backoffice opens without asking you to log in, and **Content → Home → Episodes** shows the same 4 episodes.
- ⬜ The top navigation has *no* **AI** section yet. That's what we'll add in Lesson 1.
- ⬜ You've saved your three user secrets.

## 🧯 Troubleshooting

<details>
<summary><strong>I see the Umbraco login screen</strong></summary>

Click **Sign in with Developer login**. The automatic sign-in skips the login screen, except straight after you've logged
out or been timed out. If there's no Developer login button, make sure you started the site with `dotnet run` (which uses
the Development environment), and log in with `admin@example.com` / `password1234` meanwhile.

</details>

<details>
<summary><strong><code>dotnet --version</code> shows 8.x or 9.x</strong></summary>

Install the .NET 10 SDK from the link above. Multiple SDKs can live side by side, and the repo automatically uses the newest
installed SDK.

</details>

<details>
<summary><strong>The restore is crawling</strong></summary>

That's the conference Wi-Fi. Leave it running while Lecture 1 is on. If it fails partway through, run `dotnet restore`
again: it keeps what it already downloaded.

</details>

<details>
<summary><strong>"Address already in use" / port 44339 is taken</strong></summary>

Something else (often another Umbraco site) is using the port. Stop it, or run on different ports:

```bash
dotnet run --project src/TheRabbitHole.Web --urls "https://localhost:44349;http://localhost:42048"
```

Use the new port in your browser. Lesson 6 assumes 44339, so switch back when you can.

</details>

<details>
<summary><strong>The browser warns the connection isn't private</strong></summary>

Re-run `dotnet dev-certs https --trust` and restart your browser. On Linux, follow the
[distro-specific instructions](https://learn.microsoft.com/aspnet/core/security/enforcing-ssl#trust-https-certificate-on-linux).

</details>

<details>
<summary><strong><code>git checkout</code> complains about local changes to <code>Umbraco.sqlite.db</code></strong></summary>

The site's SQLite database is part of the repo, so running the site changes it. If you've only been exploring, discard the
changes:

```bash
git restore .
```

See [Troubleshooting](../troubleshooting.md) for more on switching branches.

</details>

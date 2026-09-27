# Lesson 0: Before the workshop

> [!IMPORTANT]
> Please do this **before** the workshop. It takes about 15 minutes, most of it downloading. Doing it at home means the
> conference Wi-Fi only has to carry AI requests on the day, not gigabytes of packages.

## Objective

Arrive with The Rabbit Hole running on your machine, logged in to the Umbraco backoffice, and with every NuGet package the
workshop needs already in your local cache.

## What you'll need

| Tool | Version | Check with |
|---|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download/dotnet/10.0) | 10.0 or later | `dotnet --version` |
| [Git](https://git-scm.com/downloads) | any recent | `git --version` |
| [VS Code](https://code.visualstudio.com/) + the [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit) extension | latest | — |

> [!NOTE]
> The lessons use `dotnet` CLI commands so they work in any editor on any OS. If you prefer Visual Studio or Rider, go for
> it. You'll just translate the commands into your IDE.

You **don't** need an AI provider account. We'll hand out an API key on the day.

## Steps

### 1. Trust the .NET development certificate

The site runs on HTTPS locally. Trust the development certificate once so your browser doesn't complain:

```bash
dotnet dev-certs https --trust
```

Accept the prompt your OS shows you.

### 2. Clone the workshop repository

```bash
git clone https://github.com/marceldigital/Umbraco-AI-Workshop-2026.git
cd Umbraco-AI-Workshop-2026
```

### 3. Warm your NuGet cache

The `main` branch contains the finished workshop, which uses every package you'll add during the lessons. Restoring it
downloads them all into your local NuGet cache now, so adding them on the day is instant and works even on poor Wi-Fi.

```bash
git checkout main
dotnet restore
```

### 4. Switch to the starting point

```bash
git checkout init
```

`init` is the podcast site as a client might hand it to you: a working Umbraco 17 site with no AI features yet.

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
2. Open **https://localhost:44339/umbraco** and log in:
   - **Email:** `admin@example.com`
   - **Password:** `password1234`

## ✅ Checkpoint

- ⬜ The homepage lists 4 episodes, including **The 500th Question**.
- ⬜ You can log in to the backoffice, and **Content → Home → Episodes** shows the same 4 episodes.
- ⬜ The top navigation has *no* **AI** section yet. That's what we'll add in Lesson 1.

You're ready. See you at the workshop! 🐇

## 🧯 Troubleshooting

<details>
<summary><strong><code>dotnet --version</code> shows 8.x or 9.x</strong></summary>

Install the .NET 10 SDK from the link above. Multiple SDKs can live side by side, and the repo automatically uses the newest
installed SDK.

</details>

<details>
<summary><strong>"Address already in use" / port 44339 is taken</strong></summary>

Something else (often another Umbraco site) is using the port. Stop it, or run on different ports:

```bash
dotnet run --project src/TheRabbitHole.Web --urls "https://localhost:44349;http://localhost:42048"
```

Use the new port in your browser. Some later lessons assume 44339, so switching back on the day is best.

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

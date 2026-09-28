# Troubleshooting

## How to jump to a lesson's end branch

Use this when a lesson's time is up, or when something is broken and you want a known-good starting point. Every
`lesson-N-end` branch contains the finished code for that lesson **and** a database snapshot with its backoffice
configuration, so checking it out really does catch you up. Your API key and endpoints are stored in user secrets,
outside the repository, so they come with you. The snapshots only hold references to them, so they work whichever AI
provider you use.

The short version (replace `3` with the lesson you want):

```bash
# Stop the site first: Ctrl+C in the terminal that's running it
git stash -u
git fetch origin
git checkout lesson-3-end
dotnet run --project src/TheRabbitHole.Web
```

Here's what each step does, and what to do if you'd rather throw your changes away.

### 1. Stop the site

Press **Ctrl+C** in the terminal where `dotnet run` is running. If you started it from your editor's debugger, stop
debugging instead.

The running site keeps the SQLite database open. On Windows, git can't replace a file that's in use, and stopping the
site cleanly lets SQLite write everything back into the main database file.

> [!NOTE]
> If you still see `Umbraco.sqlite.db-wal` or `Umbraco.sqlite.db-shm` next to the database after stopping the site
> (for example, after a crash), start the site and stop it again with **Ctrl+C**. Those are SQLite's temporary files.
> Git ignores them, so they won't be stashed or restored with the database.

### 2. Put your local changes aside

The database, `src/TheRabbitHole.Web/umbraco/Data/Umbraco.sqlite.db`, is tracked in git. That's how every lesson
branch carries its own backoffice configuration. It also means the file changes as soon as you run the site, so git
refuses to switch branches until you deal with it:

```text
error: Your local changes to the following files would be overwritten by checkout:
        src/TheRabbitHole.Web/umbraco/Data/Umbraco.sqlite.db
Please commit your changes or stash them before you switch branches.
```

Run one of these options from the repository root.

**Option A: keep your changes (recommended)**

```bash
git stash -u
```

`git stash` saves your modified tracked files (your code *and* the database) and resets them to the last commit. The
`-u` flag (`--include-untracked`) also saves the **new** files you created, such as a new class. Without it, those
files would stay behind and could clash with the same files on the branch you're switching to. Nothing is lost:
`git stash list` shows your saved work, and you can bring it back later (see
[Going back to your own work](#going-back-to-your-own-work)).

**Option B: throw your changes away**

```bash
git restore .
git clean -fd
```

`git restore .` puts every tracked file, including the database, back to how it was at the last commit.
`git clean -fd` deletes untracked files (`-f`) and directories (`-d`), which means the new files you created. **This
can't be undone.** To preview what `git clean` would delete, run `git clean -nd` first.

> [!WARNING]
> Don't add `-x` to `git clean`. It also deletes git-ignored files, including `bin/` and `obj/`, which forces a slow
> full rebuild.

### 3. Check out the lesson's end branch

```bash
git fetch origin
git checkout lesson-3-end
```

`git fetch` makes sure you have every lesson branch, even if you cloned a while ago. `git checkout lesson-3-end`
creates a local branch that tracks `origin/lesson-3-end` and switches to it, database included.

### 4. Run the site

```bash
dotnet run --project src/TheRabbitHole.Web
```

The backoffice now has that lesson's configuration, and it signs you in again automatically. If you land on a sign-in
screen instead, click **Sign in with Developer login**.
You can keep working on this branch. If you need to jump again later, follow the same steps.

### Going back to your own work

Your stash belongs to the branch you were on when you created it (usually `init`, or an earlier `lesson-N-end`). To get
it back, stop the site, set aside any changes on the current branch (Option A or B above), then:

```bash
git checkout init
git stash pop
```

> [!IMPORTANT]
> Only run `git stash pop` on the branch where you created the stash. If you pop it somewhere else, the database will
> conflict, and git can't merge a binary file.

## The site won't start

### The port is already in use

Usually a previous `dotnet run` is still running in another terminal, or another Umbraco site is using port 44339. Stop it.
If you can't, run on different ports for now:

```bash
dotnet run --project src/TheRabbitHole.Web --urls "https://localhost:44349;http://localhost:42048"
```

Switch back to 44339 when you can. The site's configured application URL (which Umbraco Automate relies on in Lesson 6)
points at that port.

### The browser warns about the HTTPS certificate

Trust the development certificate, then restart the browser:

```bash
dotnet dev-certs https --trust
```

### The wrong .NET SDK is used

The workshop needs the .NET 10 SDK. Check what you have installed:

```bash
dotnet --list-sdks
```

### The build fails with `MEAI001`

Microsoft.Extensions.AI marks its speech-to-text types as experimental. Make sure `TheRabbitHole.Core.csproj` contains
`<NoWarn>$(NoWarn);MEAI001</NoWarn>` (Lesson 1, Step 3).

### I see a `NU1903` warning about `Microsoft.OpenApi`

That's a security advisory on a package the Umbraco AI packages depend on. It's a warning, not an error, and it's safe to
ignore for the workshop.

## Nothing happens after I save an episode

Work through these in order:

1. **Does the episode have an audio file?** The pipeline skips episodes without one.
2. **Is there anything left to do?** The code only generates what's missing. If the transcript, summary and show notes are
   all filled in, clear the ones you want regenerated and save again.
3. **Look at the terminal** running the site. Search for `Failed to process podcast episode` and read the exception
   underneath. Common causes: a missing or wrong user secret (the key or either endpoint), the Transcriber Profile not
   set as the default Speech-to-Text profile, or a profile alias typo (`podcast-profile`). See
   [AI provider errors](#ai-provider-errors).
4. **From Lesson 6 on:** Automate only sees *published* content, so use **Save and publish**. Then check the automation's
   **Runs** tab in the Automation section.

## AI provider errors

Every AI call reads three user secrets: `Umbraco:AI:Secrets:ApiKey`, `Umbraco:AI:Variables:ChatEndpoint` and
`Umbraco:AI:Variables:TranscriptionEndpoint`. Most provider errors come down to one of them. Check they're all there:

```bash
dotnet user-secrets list --project src/TheRabbitHole.Web
```

User secrets are read at startup, so **restart the site** after changing one.

| Error | Likely cause |
|---|---|
| `401 Unauthorized` | The key is wrong, or it's for a different provider than your endpoints. An OpenAI key won't work on Foundry. |
| `429` with *"You exceeded your current quota"* | Your OpenAI account has no credit. Add some under **Settings → Billing**. |
| `429` otherwise | A rate limit. Wait a few seconds and try again. |
| `DeploymentNotFound` / "deployment not found" | On Foundry, the deployments must be named exactly `gpt-4.1` and `gpt-4o-transcribe`, and the Podcast Profile must use **GPT 4.1**. |
| `Configuration key '…' is not permitted` | A connection references something outside `Umbraco:AI:Secrets` or `Umbraco:AI:Variables`. |

The [Bring your AI key](ai-key.md) page has the exact values for each path.

## Where are the logs?

- **AI calls** (prompts, injected context, tool calls, responses): the **AI** section's logs, under **Monitoring**.
- **Automations** (each run, each step's resolved settings and output): the automation's **Runs** tab in the
  **Automation** section.
- **Everything else:** the terminal running the site, and **Settings → Log Viewer** in the backoffice.

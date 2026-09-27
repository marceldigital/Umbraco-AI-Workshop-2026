# Workshop docs site (DocFX)

This folder is the source for the workshop's documentation site, published at
<https://marceldigital.github.io/Umbraco-AI-Workshop-2026/>. It's a conceptual-only DocFX site that uses the
**modern** template, with no API reference. This file is excluded from the build.

## Build and preview locally

You need the .NET SDK (8 or later; the workshop uses 10). DocFX is installed as a *local* dotnet tool, pinned in
`dotnet-tools.json`, so run everything **from this folder** (`docs/`):

```bash
dotnet tool restore                           # once per clone: installs the pinned docfx
dotnet docfx docfx.json                       # build into _site/
dotnet docfx docfx.json --serve --port 8089   # build, then serve at http://localhost:8089
```

To serve an existing build without rebuilding, run `dotnet docfx serve _site --port 8089`. Stop the server with
**Ctrl+C**. The server sends `Cache-Control: max-age=60`, so hard-refresh (Ctrl+F5) after a rebuild.

`_site/` and `obj/` are build output and are git-ignored.

## Deployment

`.github/workflows/docs.yml` (at the repository root) builds this folder and deploys it to GitHub Pages on every push to
`main` that touches `docs/**` or the workflow. You can also run it manually (**Actions → Docs → Run workflow**).
One-time setup: **Settings → Pages → Build and deployment → Source = GitHub Actions**.

## How the structure maps to the workshop

| File | Page |
|---|---|
| `index.md` | Landing page: premise, agenda, requirements, how the branches work, and a link to Lesson 0 |
| `lessons/lesson-0.md` | Lesson 0: Before the workshop (pre-work) |
| `lessons/lesson-1.md` … `lessons/lesson-6.md` | Lessons 1–6. The branch at the end of lesson *N* is `lesson-N-end` |
| `wrap-up.md` | Wrap-up |
| `troubleshooting.md` | Troubleshooting, including *How to jump to a lesson's end branch* |
| `toc.yml` | Sidebar navigation **and** the Previous / Next order |
| `images/` | Logo, favicon, and screenshots (`images/lesson-N/…`) |
| `template/` | A tiny custom layer on top of the modern template: `public/main.css`, `public/main.js` (GitHub icon), `token.json` (the tracker label) |
| `docfx.json` | DocFX config: site title, logo, search, the custom `TRACKER` alert |

## Previous / Next navigation

The modern template renders **Previous / Next** links at the bottom of every page. It works them out in the browser from
`toc.yml`, in top-to-bottom order, skipping group headings that have no `href`. To add or reorder pages, edit `toc.yml`.
A page that isn't in `toc.yml` gets no Previous / Next links.

There's only one `toc.yml`, so DocFX would normally show it in the top navbar and hide the sidebar. `docfx.json` sets
`_disableToc: false` to force the sidebar on. `main.css` hides the duplicate navbar links, and `_disableBreadcrumb` is
set because the breadcrumb gets confused when the navbar and the sidebar share one TOC.

## Authoring conventions

`lessons/lesson-1.md` is the living style reference. Copy its patterns.

### The lesson template

Every lesson page has these H2 sections, in this order:

1. `## Objective`
2. `## What you'll change`
3. `## Steps`. Each step is an H3, `### 1. Do the thing`, rather than an ordered list. Tab groups are unreliable
   inside list items, and H3s show up in the "In this article" panel.
4. `## ✅ Checkpoint`
5. `## 🔍 Under the hood`
6. `## 🧯 Troubleshooting`
7. `## 🆘 Stuck?`: the ready-made `git stash -u` / `git checkout lesson-N-end` / `dotnet run` block
8. `## 🚀 Stretch goals`

Put a **Requirements tracker** directly under the H1, and a `**Time:** … · **Starts from:** … · **Ends at:** …` line
under that.

### DocFX quirks to know

- **Alerts:** `> [!NOTE]`, `> [!TIP]`, `> [!IMPORTANT]`, `> [!CAUTION]`, `> [!WARNING]`. The marker goes on its own line,
  and every following line of the alert starts with `>`. Colors: NOTE and TIP are blue, WARNING is yellow, IMPORTANT
  and CAUTION are red.
- **Requirements tracker:** a custom alert, `> [!TRACKER]`. It's defined in `docfx.json`
  (`markdownEngineProperties.alerts`), styled in `main.css`, and labeled in `template/token.json`. Use ✅ and ⬜.
  **GitHub-style task lists (`- [x]`) aren't supported** and render as literal text.
- **Tabs:** a heading whose only content is a link to `#tab/<id>`, then the tab content, and the group closes with a
  line containing only `---`:

  ```markdown
  # [Windows](#tab/windows)

  Windows content

  # [macOS](#tab/macos)

  macOS content

  ---
  ```

  Keep tab groups at the top level of the page. Inside a list item, the closing `---` leaks out as a horizontal rule.
  Groups that share tab ids stay in sync across the page, and the choice is kept in the URL (`?tabs=macos`). Always
  use the ids `windows` and `macos`.
- **Code blocks:** always add a language (`bash`, `powershell`, `csharp`, `xml`, `json`). Every fenced block gets a copy
  button on hover automatically. Keep lines short, because long lines scroll sideways.
- **Images:** store them in `images/lesson-N/` and link them with a path relative to the page, for example
  `![Alt text](../images/lesson-N/screenshot.png)` from a lesson. Always write alt text.
- **Collapsible deep dives:** use `<details>` and `<summary>`. Leave a **blank line** after `</summary>` and before
  `</details>`, or the Markdown inside won't render.
- **Links:** link to `.md` files (`../troubleshooting.md#anchor`). DocFX rewrites them to `.html` and warns about a
  missing file (`InvalidFileLink`) or a missing anchor (`InvalidBookmark`). The build should stay at **0 warnings**. The
  CI build runs on Linux, so **paths are case-sensitive**.
- **Heading anchors** are generated automatically. Emoji and punctuation are dropped, so `## 🆘 Stuck?` becomes `#-stuck`
  and "How to jump to a lesson's end branch" becomes `#how-to-jump-to-a-lessons-end-branch`.
- **Placeholders:** lesson bodies are stubs marked *Placeholder.* Search for that word before the event.

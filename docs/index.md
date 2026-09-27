---
title: Beyond the Chatbot
description: Step-by-step lessons for the "Beyond the Chatbot - Building Extensible AI Workflows in Umbraco" workshop at Umbraco US Festival 2026.
---

# Beyond the Chatbot: Building Extensible AI Workflows in Umbraco

A hands-on workshop at **Umbraco US Festival 2026**, Wednesday, September 30.
Part 1 runs 1:00–3:00, then a break, then Part 2 runs 3:20–4:40.

## The premise

**The Rabbit Hole** is a podcast website built on Umbraco 17 LTS. It's based on
[Matt Brailsford's](https://github.com/mattbrailsford) Codegarden 2026 demo
([mattbrailsford/TheRabbitHole](https://github.com/mattbrailsford/TheRabbitHole)), and all credit for the original site
and demo goes to him. Today, getting a new episode online is manual work. Someone listens to the audio, types up a transcript, writes a summary
and show notes, links earlier episodes that get mentioned, and looks up each guest's details. This afternoon you'll hand
that work to **Umbraco AI**. You'll start with direct service calls, then add a custom context and a tool, replace the
hand-written calls with an agent, and finally let **Umbraco Automate** run the whole pipeline.

## Agenda

| Time | Segment | Min |
|---|---|---|
| **Part 1** | | |
| 1:00 | Lecture 1: Welcome & the landscape *(boot the `init` branch while we talk)* | 12 |
| 1:12 | Lecture 2: Umbraco AI building blocks | 8 |
| 1:20 | [**Lesson 1:** Install & configure Umbraco AI](lessons/lesson-1.md) | 20 |
| 1:40 | Lecture 3: The pipeline & the core services | 8 |
| 1:48 | [**Lesson 2:** Transcribe, summarise, write show notes](lessons/lesson-2.md) | 27 |
| 2:15 | Lecture 4: Contexts & custom resource types | 8 |
| 2:23 | [**Lesson 3:** Show Metadata context](lessons/lesson-3.md) | 25 |
| 2:48 | Buffer / Q&A | 12 |
| 3:00 | *Break. If you're behind, catch up with `git checkout lesson-3-end`* | 20 |
| **Part 2** | | |
| 3:20 | Lecture 5: Tools | 6 |
| 3:26 | [**Lesson 4:** Episode guests tool](lessons/lesson-4.md) | 16 |
| 3:42 | Lecture 6: Agents | 7 |
| 3:49 | [**Lesson 5:** The Podcast Producer agent](lessons/lesson-5.md) | 20 |
| 4:09 | Lecture 7: Umbraco Automate + AI | 6 |
| 4:15 | [**Lesson 6:** Hand the pipeline to Automate](lessons/lesson-6.md) | 20 |
| 4:35 | [Wrap-up](wrap-up.md) | 5 |

Every lesson is timeboxed. When time's up, anyone who hasn't finished checks out that lesson's end branch, and we all
move on together. Finished early? Try the 🚀 stretch goals at the bottom of each lesson.

## The requirements

The client wants four things. Each lesson page has a **Requirements tracker** so you can see how far along we are.

- ⬜ Transcribe podcast audio
- ⬜ Generate a summary and show notes
- ⬜ Validate and cross-link mentioned episodes
- ⬜ Validate and link guest details

## How the branches work

The repository has one branch for the starting point and one for the end of every lesson:

`init` → `lesson-1-end` → `lesson-2-end` → `lesson-3-end` → `lesson-4-end` → `lesson-5-end` → `lesson-6-end` → `main`

| Branch | What's in it |
|---|---|
| `init` | The client's existing site. All the non-AI plumbing is built, but there are no AI packages, config or code yet. **You start here.** |
| `lesson-N-end` | The finished state of lesson *N*: the code **and** a database snapshot with that lesson's backoffice configuration. |
| `main` | The same as `lesson-6-end`, plus this documentation site. |

Fell behind, or something broke? Checking out a lesson's end branch catches you up completely. Follow
[How to jump to a lesson's end branch](troubleshooting.md#how-to-jump-to-a-lessons-end-branch). It takes about a minute.

## Start here

Before the day, set up your machine and warm your NuGet cache, so conference Wi-Fi can't slow you down.

<p><a class="btn btn-primary btn-lg" href="lessons/lesson-0.md">Start with Lesson 0: Before the workshop →</a></p>

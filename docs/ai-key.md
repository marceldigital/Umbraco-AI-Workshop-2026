---
title: Bring your AI key
description: How to set up an OpenAI or Microsoft Foundry key for the workshop.
---

# Bring your AI key

The workshop calls two AI models:

- **GPT-4.1** for chat: summaries, show notes, tool calls and the agent
- **GPT-4o Transcribe** for speech-to-text: turning the episode audio into a transcript

You'll need your own key from one of two providers:

| Path | Choose it if… | What you'll need |
|---|---|---|
| **OpenAI** | You have, or can create, an OpenAI API account | An API key and a few dollars of prepaid credit |
| **Microsoft Foundry** | Your company already uses Azure | An Azure subscription where you can create resources |

Whichever path you pick, you end up with **three values**: an **API key**, a **chat endpoint** and a **transcription
endpoint**. In [Lesson 0, Step 8](lessons/lesson-0.md#8-save-your-key-and-endpoints-as-user-secrets) you store them as
.NET user secrets, and every lesson branch picks them up from there.

> [!NOTE]
> Adding billing (OpenAI) or deploying models (Foundry) can take a while, so set up your key before you start Lesson 0.

> [!TIP]
> **What will it cost?** Very little. Transcribing the four-minute episode costs a few cents each time, and a summary, the
> show notes or a full agent run costs a few cents more. Expect well under US$1 for the whole afternoon.

## Set up your provider

Pick the tab for your path.

# [OpenAI](#tab/openai)

About 10 minutes.

1. **Sign in** at [platform.openai.com](https://platform.openai.com), or create an account there. A ChatGPT subscription
   doesn't include API access. The API is billed separately.
2. **Add credit.** Open **Settings → Billing** and add a payment method, then buy some credit. The minimum top-up
   (US$5 at the time of writing) is far more than the workshop needs. Without credit, every call fails with
   *"You exceeded your current quota"*.
3. **Create a project** (recommended). Use the project picker at the top left to create a project called
   `Umbraco Workshop`. It keeps the workshop's usage separate and lets you revoke everything in one place afterwards. In
   the project's settings you can also set a budget.
4. **Create an API key.** Go to [API keys](https://platform.openai.com/api-keys) and choose **Create new secret key**.
   Pick your `Umbraco Workshop` project, leave **Permissions** on **All**, and create it. **Copy the key straight away**,
   because OpenAI only shows it once. Keep it in a password manager.
5. **Check it works.** Open the [Playground](https://platform.openai.com/playground) in the same project, choose the
   **gpt-4.1** model and send "Hello". A reply means your billing and model access are sorted.

Your three values:

| Value | What to use |
|---|---|
| API key | Your new key. It starts with `sk-`. |
| Chat endpoint | `https://api.openai.com/v1/` |
| Transcription endpoint | `https://api.openai.com/v1/` (the same one) |

> [!NOTE]
> New OpenAI accounts start on the lowest usage tier. Its rate limits are fine for one person working through the
> workshop.

# [Microsoft Foundry](#tab/foundry)

About 15–20 minutes. You need an Azure subscription where you're allowed to create resources. Some free-trial and
sponsored subscriptions can't deploy these models. If yours can't, use the OpenAI path.

1. **Create a Foundry project.** Sign in to the [Microsoft Foundry portal](https://ai.azure.com) and create a new
   project. This also creates a Foundry resource to hold it. Choose the **East US 2** region, which has both models. Make
   a note of the **resource name** you choose.
2. **Deploy GPT-4.1.** Open the model catalog, find **gpt-4.1** and deploy it. Choose the **Global Standard** deployment
   type and **keep the deployment name as `gpt-4.1`**. If the portal says you're out of quota, lower the tokens-per-minute
   limit (50K is plenty) and try again.
3. **Deploy GPT-4o Transcribe.** Do the same for **gpt-4o-transcribe**, and keep the deployment name
   `gpt-4o-transcribe`.
4. **Copy the key.** The project's **Overview** page shows an **API key** and your endpoints. Copy the key. The
   endpoints start with `https://<your-resource>.services.ai.azure.com/`, where `<your-resource>` is the resource name
   from step 1.
5. **Check it works.** Open the `gpt-4.1` deployment in the playground and send "Hello".

Your three values (replace `<your-resource>`):

| Value | What to use |
|---|---|
| API key | The key from the Overview page |
| Chat endpoint | `https://<your-resource>.services.ai.azure.com/openai/v1/` |
| Transcription endpoint | `https://<your-resource>.services.ai.azure.com/openai/deployments/gpt-4o-transcribe?api-version=2025-03-01-preview` |

> [!IMPORTANT]
> **Keep the default deployment names.** When Umbraco asks for the model `gpt-4.1`, Foundry looks for a *deployment*
> with that name. If you named yours differently, calls fail with "deployment not found".

<details>
<summary><strong>Why does Foundry need two endpoints?</strong></summary>

Foundry's OpenAI-compatible `/openai/v1/` endpoint serves chat, but it doesn't serve audio transcription. We tested this in
September 2026. So transcription calls go straight to the transcription deployment's own URL instead. You'll see how
Umbraco AI absorbs that difference in Lesson 1, and it's a nice example of why connections exist.

Using a classic **Azure OpenAI** resource instead of a Foundry resource? That works too. Use
`https://<your-resource>.openai.azure.com` as the start of both endpoints.

</details>

---

## Next: save them as user secrets

Head to [Lesson 0, Step 8](lessons/lesson-0.md#8-save-your-key-and-endpoints-as-user-secrets) and pick the tab for your
path. User secrets live in your user profile, not in the repository, so they can't be committed by accident, and they stay
put when you switch branches.

> [!WARNING]
> Treat the key like a password. Don't paste it into chat, a screenshot or a commit. When the workshop's over, revoke it
> in your provider's portal (or delete the `Umbraco Workshop` project) unless you want to keep experimenting.

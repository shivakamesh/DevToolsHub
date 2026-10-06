---
title: "Stop pasting production tokens into random websites 🔐 — I built 50+ dev tools that never leave your browser"
published: false
description: JSON, JWT, Regex, Base64, cron, QR codes and even electrical engineering calculators. Free, no sign-up, 100% client-side with Blazor WebAssembly.
tags: showdev, dotnet, blazor, webdev
cover_image: https://www.itsalldevtools.com/logo.png
---

> **TL;DR** — I built **[It's All Dev Tools](https://www.itsalldevtools.com)**: 50+ free developer utilities that run **entirely in your browser**. No uploads. No sign-up. Works offline. Built with **Blazor WebAssembly + .NET 8**. 🚀

---

## 😬 Be honest… have you ever done this?

It's 6 PM. Something's broken in production. You grab a JWT from the logs, Google **"jwt decoder"**, click the first result and paste it in.

Then it hits you:

> *"Wait… where did that token just go?"*

Most online tools quietly send your input to **their server**. That's fine for lorem ipsum. It's **not** fine for:

- 🔑 Access tokens and API keys
- 📜 Certificates
- 📦 API payloads with customer data
- 🧾 Internal config files

So I built the toolbox I actually wanted to use.

---

## ✨ Meet It's All Dev Tools

| | |
|---|---|
| 🔒 **Private by design** | Everything runs client-side. Your data never leaves the tab. |
| 🚫 **No sign-up** | Open the page, use the tool. That's it. |
| ✈️ **Works offline** | Once a page loads, you can pull the network cable. |
| ⚡ **Fast** | Pages are prerendered to static HTML, then come alive with Blazor. |
| 💸 **Free** | No paywalls, no "upgrade to Pro" pop-ups. |

👉 **Try it now: [itsalldevtools.com](https://www.itsalldevtools.com)**

---

## 🧰 What's in the toolbox?

### 🧹 Format & validate
[JSON Formatter](https://www.itsalldevtools.com/json-formatter) · [JSON → C#](https://www.itsalldevtools.com/json-to-csharp) · [JSON ↔ YAML ↔ XML ↔ CSV](https://www.itsalldevtools.com/format-converter) · [SQL Formatter](https://www.itsalldevtools.com/sql-formatter) · [Regex Tester](https://www.itsalldevtools.com/regex-tester) · [Diff Checker](https://www.itsalldevtools.com/diff-checker) · [JSONPath Tester](https://www.itsalldevtools.com/json-path) · [Markdown Preview](https://www.itsalldevtools.com/markdown-preview)

### 🔐 Encode & decode
[Base64](https://www.itsalldevtools.com/base64) · [URL Encoder](https://www.itsalldevtools.com/url-encoder) · [JWT Decoder](https://www.itsalldevtools.com/jwt-decoder) · [JWT Generator](https://www.itsalldevtools.com/jwt-generator) · [HMAC](https://www.itsalldevtools.com/hmac-generator) · [X.509 Certificate Decoder](https://www.itsalldevtools.com/certificate-decoder)

### 🎲 Generate
[Hashes](https://www.itsalldevtools.com/hash-generator) · [GUIDs](https://www.itsalldevtools.com/guid-generator) · [Passwords](https://www.itsalldevtools.com/password-generator) · [QR Codes](https://www.itsalldevtools.com/qr-code) · [Fake Data](https://www.itsalldevtools.com/fake-data) · [Cron Builder](https://www.itsalldevtools.com/cron-builder) · [.gitignore](https://www.itsalldevtools.com/gitignore-generator) · [Invoices](https://www.itsalldevtools.com/invoice-generator)

### 🔄 Convert
[Unix Timestamps](https://www.itsalldevtools.com/timestamp-converter) · [Number Bases](https://www.itsalldevtools.com/number-base) · [Colors & Contrast](https://www.itsalldevtools.com/color-converter) · [Time Zones](https://www.itsalldevtools.com/time-zone-converter) · [Byte Sizes](https://www.itsalldevtools.com/byte-size) · [Chmod](https://www.itsalldevtools.com/chmod-calculator)

### ⚡ Bonus: Electrical engineering
Because why not? 😄
[Cable Size & Voltage Drop (IEC/NEC)](https://www.itsalldevtools.com/cable-size-calculator) · [Ohm's Law](https://www.itsalldevtools.com/ohms-law-calculator) · [Power Factor Correction](https://www.itsalldevtools.com/power-factor-correction) · [kW / kVA / Amps](https://www.itsalldevtools.com/power-converter) · [Resistor Color Code](https://www.itsalldevtools.com/resistor-color-code) · [Transformer Fault Current](https://www.itsalldevtools.com/transformer-calculator)

---

## 🛠️ Under the hood

```text
┌───────────────────────────────┐
│   Prerendered static HTML     │  ← fast first paint + SEO
├───────────────────────────────┤
│  Blazor WebAssembly (.NET 8)  │  ← all tool logic in C#
├───────────────────────────────┤
│         Your browser          │  ← the only place your data lives
└───────────────────────────────┘
```

**My favorite perk of using .NET:** the [Regex Tester](https://www.itsalldevtools.com/regex-tester) runs the **real .NET regex engine**. What matches here matches in your C# code. No more "works in the JS tester, fails in production" surprises. 🎯

---

## 💡 3 things I learned building it

**1. Prerendering is a must for Blazor WASM.**
Without it, users stare at a loading spinner and search engines see an empty page. With it, content appears instantly and Google can index every tool.

**2. Privacy turned out to be the real selling point.**
"Runs in your browser" sounds like a tech detail. For developers handling real tokens and data, it's the reason to use the site.

**3. Small tools add up.**
Each tool is tiny on its own. Together they replace a dozen ad-filled bookmarks.

---

## 🙌 Your turn

I'm actively adding new tools, and I'd love your input:

- 💬 **What tool do you Google every week?** Tell me in the comments.
- 🐛 **Found a bug?** [Open an issue on GitHub](https://github.com/shivakamesh/DevToolsHub/issues).
- ⭐ **Like the idea?** [Star the repo](https://github.com/shivakamesh/DevToolsHub). It really helps!

👉 **[itsalldevtools.com](https://www.itsalldevtools.com)**

Thanks for reading my first DEV post! 💜
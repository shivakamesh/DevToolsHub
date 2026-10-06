---
title: I built 50+ free developer tools that run 100% in your browser (Blazor WebAssembly)
published: false
description: No sign-up, no uploads, works offline. Here's why I built It's All Dev Tools and what I learned shipping a prerendered Blazor WASM site.
tags: showdev, dotnet, blazor, webdev
cover_image: https://www.itsalldevtools.com/logo.png
canonical_url: https://www.itsalldevtools.com
---

Hi DEV 👋 — this is my first post here!

I want to share a side project I've been building: **[It's All Dev Tools](https://www.itsalldevtools.com)**, a collection of 50+ free developer utilities and electrical engineering calculators.

## The problem

You know the drill: you need to pretty-print some JSON, decode a JWT, or check a cron expression. You Google it, land on a site full of ads, and paste in... a production token. 😬

Most online tools send your input to a server. That's fine for lorem ipsum, but not for JWTs, certificates, API payloads, or customer data.

## The idea

Build tools that:

- 🔒 **Run 100% in the browser** — nothing you type is uploaded
- 🚫 **Need no sign-up**
- ✈️ **Work offline** once a page has loaded
- ⚡ **Load fast** and are easy to find via search

## What's inside

**Format & validate** — JSON formatter, JSON → C# classes, JSON ↔ YAML ↔ XML ↔ CSV, SQL formatter, .NET regex tester, diff checker, JSONPath tester, Markdown preview

**Encode & decode** — Base64, URL, HTML entities, JWT decoder/generator, HMAC, X.509 certificate decoder

**Generate** — hashes, GUIDs, passwords, QR codes, fake data, cron builder, `.gitignore` generator, invoices

**Convert** — Unix timestamps, number bases, colors + contrast checker, time zones, byte sizes, chmod

**Electrical engineering** ⚡ — cable size & voltage drop (IEC/NEC), Ohm's law, power factor correction, kW/kVA/amps, resistor color codes, transformer fault current

## The tech stack

- **Blazor WebAssembly on .NET 8** — all the logic is C#, running client-side
- **Prerendered to static HTML** — so pages load instantly and are indexable by search engines, then Blazor takes over for interactivity
- **Static hosting** — no backend needed for the core tools

A nice bonus of using .NET: the [Regex Tester](https://www.itsalldevtools.com/regex-tester) uses the *actual* .NET regex engine, so what you test is exactly what your C# code will do. Same for [JSON to C#](https://www.itsalldevtools.com/json-to-csharp).

## Lessons learned

1. **Prerendering matters.** A plain Blazor WASM app shows a loading spinner and is hard for crawlers. Prerendering fixed both first-load speed and SEO.
2. **Keep the payload lean.** Trimming and lazy-loading make a big difference to WASM download size.
3. **Privacy is a feature.** "Nothing leaves your browser" turned out to be the thing people care about most.

## Try it & tell me what's missing

👉 **https://www.itsalldevtools.com**

The source is on GitHub: [shivakamesh/DevToolsHub](https://github.com/shivakamesh/DevToolsHub)

What tool do you wish existed (without ads or uploads)? Drop it in the comments — I'm actively adding new ones. 🙌

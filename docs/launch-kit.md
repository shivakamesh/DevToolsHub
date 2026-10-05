# Launch kit: ready-to-post text for backlinks

Post one or two per day rather than all at once. Reply to every comment, since engagement keeps posts visible.

---

## 1. Hacker News (Show HN)
Post at https://news.ycombinator.com/submit, Tuesday–Thursday, 8–10 AM US Eastern.

**Title:** Show HN: 50+ developer tools that run 100% in the browser (Blazor WASM)

**URL:** https://www.itsalldevtools.com

**First comment (post right after submitting):**
> I kept pasting JWTs, certificates and customer JSON into random online formatters and wondering where that data went. So I built a set of tools where everything runs locally in WebAssembly: nothing is uploaded and they work offline once loaded.
>
> It has JSON/YAML/XML/SQL formatters, a regex tester, cron explainer, JWT decoder/generator, hash/HMAC, certificate decoder and more. It also has a section of electrical engineering calculators (cable sizing per IEC 60364/NEC, power factor correction, transformer fault current) because I needed them too.
>
> Stack: .NET 8 Blazor WebAssembly, prerendered to static HTML, hosted on Azure Static Web Apps. Feedback is welcome, especially on tools you'd want added.

---

## 2. Reddit

### r/webdev, r/dotnet, r/Blazor
**Title:** I built 50+ privacy-first dev tools with Blazor WebAssembly. Everything runs in the browser.

**Body:**
> Free, no sign-up, nothing uploaded: https://www.itsalldevtools.com
>
> Includes JSON formatter, JSON to C#, regex tester (.NET engine), cron explainer with time zones, JWT decoder/generator, certificate decoder, SQL formatter and more.
>
> Tech notes for the Blazor crowd: lazy-loaded assemblies for heavy libraries (Markdig, YamlDotNet, QRCoder), static prerendering for SEO, Azure Static Web Apps hosting. Happy to answer questions about the setup.

### r/ElectricalEngineering, r/electricians, r/AskElectricians
**Title:** Free browser-based calculators: cable size & voltage drop (IEC/NEC), PF correction, transformer fault current

**Body:**
> I made a set of free calculators that run entirely in the browser (no ads, no sign-up):
> - Cable size & voltage drop (IEC 60364 / NEC 310.16): https://www.itsalldevtools.com/cable-size-calculator
> - Power factor correction (kVAR and µF): https://www.itsalldevtools.com/power-factor-correction
> - Transformer full-load & fault current (kVA, %Z): https://www.itsalldevtools.com/transformer-calculator
> - Ohm's law, kW/kVA/amps, resistor color code, LED resistor
>
> I'd appreciate it if someone could sanity-check the results against their own calculations.

*Check each subreddit's self-promotion rules before posting.*

---

## 3. Product Hunt
https://www.producthunt.com/posts/new. Launch at 12:01 AM Pacific, Tuesday–Thursday.

- **Name:** It's All Dev Tools
- **Tagline:** 50+ developer tools that never upload your data
- **Description:** Free JSON, regex, cron, JWT, hash, QR and SQL tools plus electrical engineering calculators. Everything runs locally in your browser with WebAssembly. No sign-up, works offline.
- **Topics:** Developer Tools, Privacy, Productivity
- **Images:** `wwwroot/og-image.png` plus 3–4 screenshots of tools

---

## 4. Dev.to / Hashnode / Medium article
**Title:** How I built 50 privacy-first developer tools with Blazor WebAssembly

**Outline:**
1. The problem: pasting secrets into unknown online tools
2. Why Blazor WASM: C# everywhere, runs client-side
3. Keeping it fast: lazy-loaded assemblies, trimming, Webcil
4. SEO for a SPA: prerendering with BlazorWasmPreRendering.Build, per-page meta and JSON-LD
5. Deployment: GitHub Actions to Azure Static Web Apps, plus IndexNow
6. Link to the site and the 5 most useful tools

Tags: `blazor`, `dotnet`, `webassembly`, `webdev`, `showdev`

---

## 5. Directories (one-time submissions)
| Site | URL | Tip |
|---|---|---|
| AlternativeTo | https://alternativeto.net | Add as an alternative to jsonformatter.org, regex101, crontab.guru, jwt.io |
| SaaSHub | https://www.saashub.com/submit | Category: Developer Tools |
| Uneed | https://www.uneed.best | Free listing |
| DevHunt | https://devhunt.org | For developer tools |
| Toolify / Futurepedia | — | Only if relevant |

---

## 6. GitHub awesome lists (pull requests)
Open a PR adding a one-line entry in the right section:
- https://github.com/AdrienTorris/awesome-blazor: *Showcases* section
  `- [It's All Dev Tools](https://www.itsalldevtools.com) - 50+ developer tools and engineering calculators running fully client-side. ([source](https://github.com/shivakamesh/DevToolsHub))`
- https://github.com/quozd/awesome-dotnet: *Sample projects* section
- https://github.com/moimikey/awesome-devtools: *Web tools* section

---

## 7. GitHub repository settings
- **About → Website:** https://www.itsalldevtools.com
- **Topics:** `developer-tools`, `blazor`, `blazor-webassembly`, `dotnet`, `json-formatter`, `regex-tester`, `jwt`, `electrical-engineering`, `calculator`, `privacy`

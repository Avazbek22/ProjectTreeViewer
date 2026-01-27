# DevProjex 📁🌳

A lightweight **Windows desktop tool** for quickly visualizing a folder/project structure as a **TreeView**, selecting files with checkboxes, and copying:

* the **full tree**,
* a **tree of selected items**,
* the **content of selected text files**,
* or **tree + content** in one shot.

Designed for developers who frequently need to share project structure (e.g., for code reviews, documentation, support, mentoring, or AI-assisted debugging) without manually assembling lists.

> ✅ **Read‑only by design**: DevProjex does **not** modify your projects. It scans and reads files/folders and copies text to clipboard.

---

## Download 🚀

Get the newest build here:

* **Latest Release:** [https://github.com/Avazbek22/DevProjex/releases/latest](https://github.com/Avazbek22/DevProjex/releases/latest)

> If you need a specific version, open the Releases page and select the tag you want.

---

## Highlights ✨

* **Fast TreeView rendering** with checkboxes
* **Layer‑by‑layer expand behavior** (no forced “Expand All” on load)
* **Live settings panel**: lists (extensions / root folders / ignore options) react instantly to changes
* **Copy to clipboard** with clean formatting
* **Search in tree** (Ctrl+F + menu)
* **File‑type icons** for common dev stacks and formats
* **Localization** (multi-language UI)
* Architecture prepared for future **UI swaps** (now running on Avalonia UI)

---

## Who is this for? 🎯

DevProjex is useful when you want to:

* quickly send someone your project structure
* prepare bug reports with a clean tree snapshot
* extract selected file contents (only text formats)
* share minimal reproducible context for an AI / mentor / teammate
* teach Clean Architecture / DDD style structures by visualizing layers

Typical users:

* .NET developers
* students/mentors
* open-source maintainers
* teams doing code review / refactoring

---

## What it does (and does NOT do) ✅

### It **does**

* scan a folder, build a hierarchical tree
* show file/folder nodes with icons
* let you check/uncheck nodes (propagates to children/parents)
* copy tree and/or selected text content to clipboard
* allow filtering via settings (extensions, root folders, ignore rules)

### It **does NOT**

* edit your files
* rename/move/delete anything
* run project code
* change git state
* install dependencies

---

## Screenshots 🖼️

> <img width="1203" height="835" alt="image" src="https://github.com/user-attachments/assets/14f78897-5947-4000-ac19-56e13da7c937" />



---

## Features ✅

### 1) Tree view

* Visualize the folder structure in a classic **TreeView**.
* **Checkbox selection** for files/folders.
* Expand/collapse behavior optimized to keep navigation comfortable.

### 2) Copy actions 📋

From the **Copy** menu you can:

* **Copy full tree**
* **Copy selected tree**
* **Copy selected content** (text files only)
* **Copy tree + content** (single clipboard payload)

> Binary formats (images, videos, executables, archives, many Office formats) are skipped for content export.

### 3) Live settings panel ⚙️

Settings panel controls:

* **Ignore options** (filtering logic)
* **File types** (extensions)
* **Top-level folders**
* **Tree font**

Important behavior:

* Settings lists update **immediately** to reflect what is available.
* The tree itself updates **only when you apply settings** (to keep UI predictable).

### 4) Search 🔎

* Open search using **Ctrl+F** or the menu item.
* Designed to feel like a browser-style find widget:

  * type → matches highlight/select
  * navigation via up/down actions
  * close with a close button

---

## Tech Stack 🧩

* **.NET 10**
* **Avalonia UI** (dark theme by default)
* Cleanly separated codebase (Core/Services/Infrastructure approach)
* JSON-based resources (localization, icon mappings, etc.)

---

## Localization 🌍

DevProjex supports multiple UI languages.

### How language is chosen

* By default the app **detects the system UI culture** and selects the most appropriate language.
* If a language is selected manually (menu), it should be used consistently afterwards (depending on your settings persistence implementation).

> Implementation detail may differ by build. See the code in `LocalizationService` (e.g., `DetectSystemLanguage()`), plus `Program.cs` startup flow.

---

## Quick Start ⚡

### Option A — Use the prebuilt .exe

1. Download from:

   * [https://github.com/Avazbek22/DevProjex/releases/latest](https://github.com/Avazbek22/DevProjex/releases/latest)
2. Run the application
3. **File → Open folder…**
4. Use the **Copy** menu to export tree/content

### Option B — Build from source

**Requirements**

* Windows
* .NET SDK **10**

**Build & run**

```bash
# From the solution folder

dotnet restore

dotnet build -c Release

dotnet run --project Apps/Avalonia/DevProjex.Avalonia.csproj
```

> Project/paths may vary slightly depending on how the solution is structured.

---

## How to use (workflow examples) 🧠

### Example 1: Share project structure with a teammate

1. Open folder
2. (Optionally) uncheck irrelevant folders/files
3. Copy → **Copy selected tree**
4. Paste into chat or issue

### Example 2: Share structure + specific file contents

1. Check only the files you want
2. Copy → **Copy selected content**
3. Paste into chat or documentation

### Example 3: Prepare context for AI assistance

1. Select key folders + relevant files
2. Copy → **Copy tree + content**
3. Paste into your AI prompt

---

## FAQ ❓

### Does it modify my project files?

No. DevProjex is **read-only**. It scans folders and reads files only to display structure or copy text.

### Can it break my repository or git state?

No. It does not run git commands or change tracked files.

### Why are some file contents not copied?

Binary formats are intentionally skipped (e.g., `.dll`, `.png`, `.zip`, `.pdf`, many Office files). The app focuses on *text content*.

### Why are icons missing sometimes?

Ensure the icon pack/resources are included correctly in the build/publish pipeline. The intended approach is to package icons with the app so they are available after publish.

### Can I use it on very large folders?

Yes, but very large trees may still take time depending on disk speed and folder complexity.

### Will there be a WPF/Web/CLI version?

That’s part of the roadmap. The architecture is prepared for future UI layers.

---

## Roadmap 🗺️

Planned directions (high level):

* smarter ignore detection (stack-aware candidates)
* improved search UX (fast, stable, browser-like)
* more export formats (Markdown, file, etc.)
* alternative UIs (WPF / WebHost / CLI)

---

## Contributing 🤝

PRs and issues are welcome.

Suggested contributions:

* Bug fixes
* Documentation improvements (README, screenshots, store descriptions)
* Performance improvements
* Tests (unit and integration)
* UI/UX improvements
* Icon mappings
* Localization

PRs and issues are welcome.  
For details, see [CONTRIBUTING.md](CONTRIBUTING.md).

---

## License 📄

DevProjex is source-available under the Business Source License (BSL) 1.1.

Non-commercial use and contributions are allowed, while commercial use is restricted until 2030-01-01. On that date, the license automatically changes to MIT.

See [LICENSE](LICENSE) for the current terms and [LICENSE-MIT](LICENSE-MIT) for the future change license.

---

## Keywords (for GitHub search) 🔎

project tree viewer, folder structure, tree export, clipboard export, avalonia, .net 10, codebase visualization, clean architecture, DDD, developer tool, repository structure, file selection, treeview search

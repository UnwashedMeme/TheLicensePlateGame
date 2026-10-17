# Implementation Plan: The License Plate Game (F# Fable + Elmish PWA)

This document provides a complete, step-by-step implementation plan with checkable task boxes for building **The License Plate Game** using the **Model-View-Update (MVU)** architecture in **F#** with **Fable** and **Feliz**.

> **Architecture Philosophy**:
> - **Pure Functional MVU**: Uses Elmish (The Elm Architecture) for predictable, immutable state transitions.
> - **UI-First Phasing**: We build the visual Game Board and interaction loops with in-memory Elmish state first, then layer on IndexedDB persistence, startup lifecycle, and background location services.
> - **Shared Domain Foundation**: A dedicated `Shared` F# library encapsulates domain models and event-sourcing types so future multi-client server synchronization is completely seamless.

---

## 1. Architecture & Tech Stack

- **Client Language & Framework**: **F#** compiled to modern JavaScript via **Fable 5** (v5.19+).
- **State Management & Pattern**: **Elmish** (The Elm Architecture / Model-View-Update).
- **View DSL**: **Feliz** (Type-safe React/HTML DSL for F#).
- **Bundler & Dev Server**: **Vite** with instant Hot Module Replacement (HMR).
- **Local Persistence**: Browser-native **IndexedDB** wired through Elmish `Cmd`.
- **Location Services**: Browser Geolocation API (`navigator.geolocation`) running asynchronously in the background.
- **PWA & Offline**: Web App Manifest (`manifest.json`) and Service Worker (`sw.js`) with Cache API for 100% offline usability.
- **Future Server**: F# backend (Giraffe / ASP.NET Core) consuming the `Shared` domain project.

---

## 2. Project Directory Structure

```text
TheLicensePlateGame/
├── src/
│   ├── Shared/
│   │   ├── Shared.fsproj       # .NET / Fable compatible shared library
│   │   ├── Domain.fs           # Jurisdiction, Game, Observation models & static data
│   │   └── Events.fs           # Event-sourcing sync events for future server sync
│   └── Client/
│       ├── Client.fsproj       # Fable application project
│       ├── Types.fs            # Elmish Model & Msg union types
│       ├── State.fs            # init and update functions (pure MVU)
│       ├── Search.fs           # Fuzzy and postal-code matching utility
│       ├── Storage.fs          # IndexedDB interop via Elmish Cmd
│       ├── Location.fs         # Async GPS capture via Elmish Cmd
│       ├── View.fs             # Feliz UI components (Board, Buttons, Modals)
│       └── App.fs              # Elmish program entrypoint (Program.mkProgram)
├── public/
│   ├── manifest.json           # Web App Manifest for mobile installation
│   ├── sw.js                   # Service Worker for offline asset caching
│   ├── css/
│   │   └── styles.css          # Mobile-first styles, CSS variables, touch layouts
│   └── icons/
│       ├── icon-192.png        # PWA application icon (192x192)
│       └── icon-512.png        # PWA application icon (512x512)
├── index.html                  # HTML host shell
├── package.json                # Project dependencies (vite, @vitejs/plugin-react, react)
├── pnpm-lock.yaml              # Deterministic pnpm dependency lockfile
├── pnpm-workspace.yaml         # Build permissions configuration (esbuild)
├── vite.config.js              # Vite configuration for Fable output
├── .config/
│   └── dotnet-tools.json       # Local dotnet tool manifest (fable)
├── .gitignore                  # Git exclusions for node, .fable, dist, and .NET
├── userexperience.md           # Approved UX specification
└── implementation-plan.md      # Step-by-step task checklist
```


---

## 3. Step-by-Step Task Checklist

### Phase 1: Toolchain, Project Scaffold & Shared Domain

- [x] **1.1. Toolchain Setup**
  - Verify Node.js and pnpm availability (`node -v` for Node v22/v24 LTS, `pnpm -v` >= 9/10).
  - Initialize local dotnet tool manifest: `dotnet new tool-manifest`.
  - Install local Fable tool: `dotnet tool install fable` (installs latest Fable 5.19+).
  - Initialize `package.json` and install modern tooling via pnpm: `vite` (^6.0+), `@vitejs/plugin-react` (^4.3+), `react` (^19.0+), `react-dom` (^19.0+).
  - Configure `package.json` workflow scripts: `dev`, `build`, `preview`, `test`, and `clean`.
  - Create `vite.config.js` pointing to compiled client entrypoint.
- [x] **1.2. Shared Domain Library (`src/Shared/`)**
  - Create `Shared.fsproj` targeting `net10.0`.
  - Implement `Domain.fs`:
    - `Jurisdiction` record: `{ Id: string; Code: string; Name: string; Tags: string list }`.
    - Complete dataset of 61 jurisdictions (50 US States, DC, 10 Canadian Provinces).
    - `GameStatus` union (`InProgress | Completed of EndDate: DateTimeOffset`).
    - `Game` record: `{ Id: Guid; Name: string; Notes: string option; StartDate: DateTimeOffset; Status: GameStatus }`.
    - `Observation` record: `{ Id: Guid; GameId: Guid; JurisdictionId: string; Timestamp: DateTimeOffset; Latitude: float option; Longitude: float option }`.
  - Implement `Events.fs`:
    - `GameEvent` union type (`PlateObserved of Observation`, `ObservationDeleted of Guid`, `GameStarted of Game`, `GameUpdated of Game`, `GameEnded of Guid`).
    - Ready for serialization and server sync.
- [x] **1.3. Base HTML Shell & Stylesheet Setup**
  - Configure `index.html` with mobile meta tags (`viewport-fit=cover`, mobile responsiveness, web app capable).
  - Create `public/css/styles.css` with CSS variables, safe-area insets, and mobile grid layouts.

---

### Phase 2: Visual Game Board & UI Shell (Elmish In-Memory)

- [x] **2.1. Client F# Project Setup (`src/Client/Client.fsproj`)**
  - Create `Client.fsproj` targeting `net10.0` and reference `src/Shared/Shared.fsproj`.
  - Add NuGet packages: `Fable.Core` (>= 4.3.0), `Fable.Elmish` (>= 4.2.0), `Fable.Elmish.React` (>= 4.0.0), `Feliz` (>= 2.9.0), `Fable.Browser.Dom` (>= 2.18.0), `Fable.Browser.IndexedDB` (>= 2.0.0).
  - Verify live development loop: `dotnet fable watch src/Client -o src/Client --run pnpm exec vite`.
- [x] **2.2. Elmish Core Types & In-Memory State (`Types.fs`, `State.fs`)**
  - Define `Model`:
    - `ActiveGame: Game`
    - `Observations: Observation list`
    - `SelectedTab: Scope` (`US | Canada | All`)
    - `SearchQuery: string`
    - `HideObserved: bool`
    - `ActiveModal: ModalType option`
  - Define `Msg`:
    - `PlateTapped of jurisdictionId: string`
    - `SearchChanged of string`
    - `ClearSearch`
    - `TabSelected of Scope`
    - `ToggleHideObserved`
    - `OpenModal of ModalType`
    - `CloseModal`
  - Implement pure `init` and `update` functions handling all state changes in memory.
- [x] **2.3. Header & Live Progress Counter (`View.fs`)**
  - Render sticky top header displaying:
    - Active game title (e.g. `Trip - Oct 3, 2026`).
    - Live progress counter (e.g. `18 / 61` or dynamic tab count `15 / 51 US`).
    - Hamburger menu button.
- [x] **2.4. Scope Filter Tabs & Search Box (`Search.fs`, `View.fs`)**
  - Render scope pills (`US States 51`, `Canadian Provinces 10`, `All 61`).
  - Render search input with real-time matching on full names and 2-letter postal codes (e.g. "la" $\rightarrow$ Louisiana).
  - Include inline clear ('X') button.
- [x] **2.5. State Button Grid & Tap Feedback (`View.fs`)**
  - Render buttons according to approved layout:
    - **Top-left**: Small 2-letter postal code (`TX`, `CA`, `ON`).
    - **Top-right**: Small count badge showing observations.
    - **Lower two-thirds**: Centered jurisdiction full name.
  - Implement visual states:
    - Neutral styling for unobserved states (count = 0).
    - Distinct accent/highlight background & border when observed (count $\ge 1$).
  - **Single-tap action**:
    - Dispatches `PlateTapped`.
    - Increments count badge and applies accent styling immediately.
    - Auto-clears search input and blurs keyboard so full board is visible.

---

### Phase 3: Interactive UI Modals & Menus (Elmish In-Memory)

- [x] **3.1. Long-Press Detection**
  - Implement touch/pointer long-press timer (~500ms):
    - Suppress standard browser context menu.
    - Cancel timer if finger moves/scrolls beyond 10px.
  - If count = 0: no-op.
  - If count $\ge 1$: dispatch `OpenObservationDetails(jurisdictionId)`.
- [x] **3.2. Observation Details & Deletion Modal**
  - Modal header: State Name & Postal Code.
  - List observations recorded this game with formatted timestamps (e.g., `Oct 3, 2:45 PM`).
  - Provide an **X** button next to each entry:
    - Dispatches `ObservationDeleted(id)`.
    - Decrements count badge in memory.
    - Reverts button to neutral styling when count reaches 0.
    - Auto-closes modal when last entry is deleted.
- [ ] **3.3. Hamburger Menu & Actions**
  - Slide-out/dropdown menu component.
  - **"Hide / Show Observed" Toggle**:
    - Dispatches `ToggleHideObserved`.
    - Filters observed buttons from the board.
  - **"Edit Game Info" Modal**:
    - Inputs for Game Name and Notes.
    - Dispatches `UpdateGameInfo(name, notes)` to update header title.
  - **"New Game" Confirmation Modal**:
    - Warning that active game will conclude.
    - Input for New Game Name (prefilled with date).
    - Notes textarea.
    - On confirm: resets in-memory counts to clean slate (all 0s).

---

### Phase 4: Persistence Layer with IndexedDB (`Storage.fs`)

- [ ] **4.1. IndexedDB Interop Module**
  - Open `LicensePlateGameDB` with object stores:
    - `games` (key: `id`, index: `isActive`).
    - `observations` (key: `id`, indexes: `gameId`, `jurisdictionId`).
- [ ] **4.2. Storage Commands in Elmish**
  - Wrap database operations into Elmish `Cmd`:
    - `loadActiveSession`: loads active game and its observations on startup.
    - `saveObservation`: persists new observation record.
    - `removeObservation`: deletes observation by ID.
    - `saveGame`: creates or updates game record.
    - `finishGame`: sets `isActive = false` and `endDate = now`.
- [ ] **4.3. Connect Elmish Update to Storage Commands**
  - Ensure UI updates optimistically (instant badge update), while database writes occur asynchronously in the background via `Cmd`.

---

### Phase 5: App Startup Flow & Lifecycle Management

- [ ] **5.1. First-Launch Auto-Start**
  - On app launch, query IndexedDB for an active game.
  - If none exists, automatically create a default session (e.g. `Trip - Oct 3, 2026`) and persist it.
- [ ] **5.2. Session Restoration**
  - On reload, hydrate Elmish model with the active game and all existing observations.
  - Restore all button badges, highlight colors, and header progress counter.
- [ ] **5.3. Full "New Game" Transition**
  - When user confirms "New Game":
    - Conclude existing game in IndexedDB (`endDate = now`, `isActive = false`).
    - Create new game record with entered name/notes.
    - Reset active board to clean slate without altering historical data.

---

### Phase 6: Add-on — Location Services & GPS Capture (`Location.fs`)

- [ ] **6.1. Explanation Banner & Permission Request**
  - Display non-intrusive explanation banner on first launch explaining GPS coordinate logging.
  - Request permission via `navigator.geolocation.getCurrentPosition()`.
- [ ] **6.2. Background GPS Resolution via Elmish Cmd**
  - On `PlateTapped`:
    - Record observation immediately with timestamp.
    - Dispatch background `Cmd` to query GPS coordinates (with 5000ms timeout).
    - When GPS resolves, dispatch `GpsCoordinatesAcquired(obsId, lat, lon)` to attach coordinates to the observation in IndexedDB.
  - Graceful fallback: observations log normally if GPS is denied, timed out, or unavailable.

---

### Phase 7: PWA, Service Worker & Offline Capability

- [ ] **7.1. Web App Manifest (`public/manifest.json`)**
  - Configure `name`, `short_name`, `start_url: "/"`, `display: "standalone"`, `theme_color`, `background_color`.
- [ ] **7.2. PWA Icons**
  - Generate and link 192x192 and 512x512 icons in `public/icons/`.
- [ ] **7.3. Service Worker (`public/sw.js`)**
  - Cache-first strategy for all static assets (`index.html`, compiled JS bundle, CSS, icons).
  - Offline fallback guaranteeing 100% functionality without internet.
- [ ] **7.4. Register Service Worker**
  - Register `sw.js` in `App.fs` on page load.

---

### Phase 8: End-to-End Verification & Future Server Sync Readiness

- [ ] **8.1. UI & Interaction Verification**
  - Verify state button layout (top-left abbreviation, top-right count, centered name).
  - Verify single-tap responsiveness and immediate badge count update.
  - Verify search filtering (name & abbreviation), auto-clear, and keyboard blur.
  - Verify tab switching and dynamic progress counter.
  - Verify long-press popup and individual observation deletion.
- [ ] **8.2. Persistence & Offline Verification**
  - Verify game state persists across browser page refreshes.
  - Verify full offline operation with DevTools Network set to Offline.
  - Verify "New Game" reset and clean slate behavior.
- [ ] **8.3. Future Server Sync Validation**
  - Validate that every state change maps cleanly to a `GameEvent` in `src/Shared/Events.fs`.
  - Ensure domain models are cleanly decoupled and ready for a future F# backend sync server.

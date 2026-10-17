# Implementation Plan: The License Plate Game (F# Fable + Elmish PWA)

This document provides a complete, step-by-step implementation plan with checkable task boxes for building **The License Plate Game** using the **Model-View-Update (MVU)** architecture in **F#** with **Fable 5**, **Feliz 3**, and **`Feliz.UseElmish`**.

> **Architecture Philosophy**:
> - **Pure Functional MVU with `Feliz.UseElmish`**: Uses Elmish (`React.useElmish`) across both the root application component and self-contained stateful sub-components (like `GameModal`) for predictable, immutable state transitions with clean component boundaries.
> - **UI-First Phasing**: We build the visual Game Board and interaction loops with in-memory Elmish state first, then layer on event-sourced IndexedDB persistence, startup lifecycle, and background location services.
> - **Unified Event-Sourced Domain Foundation**: A dedicated `Shared` F# library encapsulates domain models (`Domain.fs`) and event-sourcing types (`Events.fs`). Local IndexedDB persistence stores `StampedGameEvent` records serialized via `Thoth.Json` and folds them into the active model—making local storage and future multi-client server synchronization 100% unified.

---

## 1. Architecture & Tech Stack

- **Client Language & Compiler**: **F#** (.NET 10) compiled to modern JavaScript ES modules via **Fable 5** (v5.22+) with source maps (`-s`) outputting to an out-of-source `.fable-build/` directory.
- **State Management & Component Pattern**: **Elmish 5** + **`Feliz.UseElmish` 5** (`React.useElmish` at both the root app component and stateful child modals, eliminating `Fable.Elmish.React` and `OutMsg` parent-plumbing boilerplate).
- **View DSL**: **Feliz 3** (v3.3+) on **React 19**.
- **Bundler & Dev Server**: **Vite 8** with instant Hot Module Replacement (HMR).
- **Local Persistence & Serialization**: Browser-native **IndexedDB** (`Fable.Browser.IndexedDB`) + **`Thoth.Json`** storing `StampedGameEvent` records in an `events` object store and folding them into domain state.
- **Location Services**: Browser Geolocation API (`navigator.geolocation`) running asynchronously in the background via Elmish `Cmd`.
- **PWA & Offline**: **`vite-plugin-pwa`** (Workbox) for automatic precaching of Vite's content-hashed production bundles and Web App Manifest (`manifest.webmanifest`) management.
- **Testing & Formatting**: **Expecto** on .NET 10 (`dotnet test`) for testing `Shared` domain logic, event folding, search matching, and pure Elmish `update` transitions; **Fantomas 8** for F# code formatting.
- **Future Server**: F# backend (Giraffe / ASP.NET Core) consuming the `Shared` domain and `StampedGameEvent` stream directly.

---

## 2. Project Directory Structure

```text
TheLicensePlateGame/
├── src/
│   ├── Shared/
│   │   ├── Shared.fsproj           # .NET 10 / Fable compatible shared library
│   │   ├── Domain.fs               # Jurisdiction, Tag, Game, Observation models & static data
│   │   └── Events.fs               # StampedGameEvent union & pure event-folding functions
│   └── Client/
│       ├── Client.fsproj           # Fable application project
│       ├── Icons.fs                # SVG icon components
│       ├── Scopes.fs               # Scope filter model & toggle helpers
│       ├── GameModal.fs            # Self-contained Edit/New Game modal (React.useElmish)
│       ├── ObservationModal.fs     # Stateless observation list & deletion modal
│       ├── Types.fs                # Root Elmish Model & Msg union types
│       ├── Search.fs               # Fuzzy and postal-code matching utility
│       ├── Storage.fs              # IndexedDB event-store interop + Thoth.Json codecs
│       ├── Location.fs             # Async GPS capture via Elmish Cmd
│       ├── State.fs                # Root init and update functions (pure MVU)
│       ├── View.fs                 # Feliz UI components (Header, Controls, PlateGrid, Drawer)
│       └── App.fs                  # ReactDOM.createRoot entrypoint mounting View.App
├── tests/
│   ├── Tests.fsproj                # .NET 10 Expecto test project
│   └── Main.fs                     # Domain, event-folding, search & Elmish state unit tests
├── public/
│   ├── css/
│   │   └── styles.css              # Mobile-first styles, CSS variables, touch layouts
│   ├── icons/
│   │   ├── icon-192.png            # PWA application icon (192x192)
│   │   └── icon-512.png            # PWA application icon (512x512)
│   └── favicon.svg                 # Vector app icon
├── index.html                      # HTML host shell pointing to /.fable-build/App.js
├── package.json                    # Scripts & JS dependencies (vite, react, vite-plugin-pwa)
├── pnpm-lock.yaml                  # Deterministic pnpm dependency lockfile
├── pnpm-workspace.yaml             # Build permissions configuration
├── vite.config.js                  # Vite + React + VitePWA configuration
├── .config/
│   └── dotnet-tools.json           # Local dotnet tool manifest (fable, fantomas)
├── .gitignore                      # Git exclusions for node_modules, .fable-build, dist, bin/obj
├── dev-setup.md                    # Ubuntu developer setup & tooling architecture notes
├── userexperience.md               # Approved UX specification
└── implementation-plan.md          # Step-by-step task checklist
```

---

## 3. Step-by-Step Task Checklist

### Phase 1: Toolchain, Project Scaffold & Shared Domain

- [x] **1.1. Toolchain Setup**
  - Verify Node.js and pnpm availability (`node -v` for Node v22/v24 LTS, `pnpm -v`).
  - Initialize local dotnet tool manifest (`.config/dotnet-tools.json`) with `fable` (5.22+) and `fantomas` (8.0+).
  - Initialize `package.json` and install modern tooling via pnpm: `vite` (^8.3+), `@vitejs/plugin-react` (^6.1+), `react` (^19.3+), `react-dom` (^19.3+).
  - Create `vite.config.js` and `.gitignore`.
- [x] **1.2. Refine Build Pipeline for Out-of-Source `.fable-build` & Source Maps**
  - Update `package.json` scripts (`dev` and `build`) to compile Fable with `-s -o .fable-build` so source maps are enabled and `src/` stays 100% pure F#.
  - Add `format` script (`dotnet tool run fantomas src`) and `clean` script removing `.fable-build/` and any legacy in-place `.fs.js` artifacts.
  - Update `index.html` script entrypoint to `/.fable-build/App.js`.
  - Clean up `.gitignore` to ignore `.fable-build/` instead of blanket `*.js`.
- [x] **1.3. Shared Domain Library (`src/Shared/`)**
  - Create `Shared.fsproj` targeting `net10.0`.
  - Implement `Domain.fs`:
    - `Tag` union with regional/country categories and `UiFriendlyName`.
    - `Jurisdiction` record: `{ Id: string; Code: string; Name: string; Tags: Tag list }`.
    - Complete dataset of 61 jurisdictions (50 US States, DC, 10 Canadian Provinces).
    - `GameStatus` union (`InProgress | Completed of EndDate: DateTimeOffset`).
    - `Game` record: `{ Id: Guid; Name: string; Notes: string option; StartDate: DateTimeOffset; Status: GameStatus }`.
    - `Observation` record: `{ Id: Guid; GameId: Guid; JurisdictionId: string; Timestamp: DateTimeOffset; Latitude: float option; Longitude: float option }`.
  - Implement initial `Events.fs` (`GameEvent` and `StampedGameEvent`).
- [x] **1.4. Complete Event-Sourcing Foundation in `src/Shared/Events.fs`**
  - Add `ObservationLocationRecorded of observationId: Guid * latitude: float * longitude: float` to `GameEvent` to support asynchronous GPS resolution.
  - Implement pure `Events.apply` and `Events.fold` functions in `Shared/Events.fs` so any sequence of `StampedGameEvent`s deterministically reconstructs active and historical game state.
- [x] **1.5. Base HTML Shell & Stylesheet Setup**
  - Configure `index.html` with mobile meta tags (`viewport-fit=cover`, mobile responsiveness, web app capable).
  - Create `public/css/styles.css` with CSS variables, safe-area insets, and mobile grid layouts.

---

### Phase 2: Visual Game Board & UI Shell (`Feliz.UseElmish` In-Memory)

- [x] **2.1. Client F# Project Setup (`src/Client/Client.fsproj`)**
  - Create `Client.fsproj` targeting `net10.0` and reference `src/Shared/Shared.fsproj`.
  - Add core NuGet packages: `Fable.Core` (5.4+), `Fable.Elmish` (5.0+), `Feliz` (3.3+), `Fable.Browser.Dom` (2.20+), `Fable.Browser.IndexedDB` (2.2+).
- [ ] **2.2. Migrate Client Architecture to Full `Feliz.UseElmish`**
  - Replace `Fable.Elmish.React` with `Feliz.UseElmish` (5.0+) in `Client.fsproj`.
  - Update `App.fs` to mount a `[<ReactComponent>]` root (`View.App`) using `ReactDOM.createRoot` and `React.useElmish(State.init, State.update)`.
  - Refactor `GameModal.fs` into a self-contained `[<ReactComponent>]` using `React.useElmish` internally for draft form inputs (`Name`, `Notes`), invoking `onSave` / `onCancel` callbacks on completion instead of routing `GameModalMsg` and `OutMsg` through the root `Model`/`State`.
  - Remove unused `Msg` and `viewWithMsg` scaffolding from `ObservationModal.fs`.
- [x] **2.3. Elmish Core Types & In-Memory State (`Types.fs`, `State.fs`)**
  - Define `Model` (`ActiveGame`, `Observations`, `Scopes`, `SearchQuery`, `HideObserved`, `ActiveModal`).
  - Define `Msg` and pure `init` / `update` functions handling all in-memory state transitions.
- [x] **2.4. Header & Live Progress Counter (`View.fs`)**
  - Render sticky top header displaying active game title (tappable to edit), live progress counter (`18 / 61`), and hamburger menu button.
- [x] **2.5. Scope Filter Tabs & Search Box (`Scopes.fs`, `Search.fs`, `View.fs`)**
  - Render scope toggle pills (`US States 51`, `Canadian Provinces 10`).
  - Render search input with real-time matching on full names and 2-letter postal codes (e.g. "la" $\rightarrow$ Louisiana) plus inline clear ('X') button.
- [x] **2.6. State Button Grid & Tap Feedback (`View.fs`)**
  - Render `PlateCard` buttons with top-left postal code, top-right count badge, and centered jurisdiction name.
  - Apply neutral styling when count = 0 and distinct accent highlight when count $\ge 1$.
  - Single-tap dispatches `PlateTapped`, increments count immediately, clears search input, and blurs the mobile keyboard.

---

### Phase 3: Interactive UI Modals & Menus (`Feliz.UseElmish` In-Memory)

- [x] **3.1. Long-Press Detection (`View.fs`)**
  - Implement touch/pointer long-press timer (~500ms) on `PlateCard`:
    - Suppress standard browser context menu.
    - Cancel timer if pointer moves/scrolls beyond 10px.
  - If count $\ge 1$, dispatch `OpenObservationDetails(jurisdictionId)`.
- [x] **3.2. Observation Details & Deletion Modal (`ObservationModal.fs`)**
  - Display jurisdiction name, postal code, and recorded observations with local timestamps (`MMM d, h:mm tt`).
  - Provide an **X** button next to each entry that dispatches `DeleteObservation(id)`, decrements the badge count, and auto-closes the modal when the last observation for that state is removed.
- [x] **3.3. Hamburger Menu & Game Modals (`View.fs`, `GameModal.fs`)**
  - Slide-out navigation drawer with:
    - **"Hide / Show Observed States"** toggle (`ToggleHideObserved`).
    - **"Edit Game Info"** modal (`GameModal` in edit mode).
    - **"New Game"** confirmation modal (`GameModal` in new-game mode, resetting board counts for the new session).

---

### Phase 4: Event-Sourced Persistence Layer (`Storage.fs`)

- [ ] **4.1. Add `Thoth.Json` & Event Codecs**
  - Add `Thoth.Json` NuGet package to `src/Client/Client.fsproj`.
  - Implement encoders and decoders in `Storage.fs` for `Game`, `Observation`, `GameEvent`, and `StampedGameEvent` so F# unions, `option`, `Guid`, and `DateTimeOffset` round-trip losslessly.
- [ ] **4.2. IndexedDB Event Store Interop (`Storage.fs`)**
  - Open `LicensePlateGameDB` with an `events` object store (keyPath: `id`, indexes: `gameId`, `timestamp`).
  - Expose clean Elmish `Cmd` helpers:
    - `loadAllEvents`: reads all persisted `StampedGameEvent` records ordered by timestamp on startup.
    - `appendEvents`: asynchronously persists newly emitted `StampedGameEvent` records whenever user actions occur.
- [ ] **4.3. Connect Elmish `State.update` to Event Sourcing**
  - Update `State.update` transitions (`PlateTapped`, `DeleteObservation`, `SaveGame`, `StartNewGame`) to construct `StampedGameEvent` values, fold them into the in-memory `Model` immediately (optimistic UI update), and dispatch `Storage.appendEvents` in the background.

---

### Phase 5: App Startup Flow & Lifecycle Management

- [ ] **5.1. First-Launch Auto-Start & Event Replay**
  - On app launch (`State.init`), dispatch `Storage.loadAllEvents`.
  - When `EventsLoaded` arrives:
    - If events exist, fold them via `Events.fold` to restore the active `Game` and its `Observations`.
    - If no active game exists (first launch), automatically emit and persist a `GameStarted` event for a default session (e.g. `Trip - Oct 17, 2026`).
- [ ] **5.2. Full "New Game" Event Transition**
  - When user confirms "New Game":
    - Emit both `GameEnded(oldGame.Id)` and `GameStarted(newGame)` events in a single atomic batch.
    - Persist both events to IndexedDB while resetting the active board to a clean slate without losing historical game events.

---

### Phase 6: Add-on — Location Services & GPS Capture (`Location.fs`)

- [ ] **6.1. Explanation Banner & Permission Request**
  - Display a non-intrusive explanation banner on first launch explaining GPS coordinate logging.
  - Request permission via `navigator.geolocation.getCurrentPosition()`.
- [ ] **6.2. Background GPS Resolution via Elmish Cmd**
  - On `PlateTapped`:
    - Emit `PlateObserved` immediately with `Latitude = None, Longitude = None`.
    - Dispatch a background `Cmd` to query GPS coordinates (with a 5000ms timeout).
    - When GPS resolves, dispatch `GpsCoordinatesAcquired(obsId, lat, lon)`, which emits and persists `ObservationLocationRecorded(obsId, lat, lon)` to attach coordinates to the observation.
  - Graceful fallback: observations remain valid if GPS is denied, timed out, or offline.

---

### Phase 7: PWA, Service Worker & Offline Capability (`vite-plugin-pwa`)

- [ ] **7.1. Install & Configure `vite-plugin-pwa`**
  - Add `vite-plugin-pwa` as a devDependency in `package.json`.
  - Configure `VitePWA` in `vite.config.js` with `registerType: 'autoUpdate'`, `injectRegister: 'auto'`, Workbox glob patterns (`**/*.{js,css,html,ico,png,svg}`), and Web App Manifest metadata (`name`, `short_name`, `display: "standalone"`, `theme_color`, `background_color`, icons).
- [ ] **7.2. PWA Icons**
  - Ensure 192x192 and 512x512 icons are present in `public/icons/` and linked in the manifest configuration.

---

### Phase 8: Expecto Test Suite & End-to-End Verification

- [ ] **8.1. Scaffold Expecto Test Project (`tests/Tests.fsproj`)**
  - Create `tests/Tests.fsproj` targeting `net10.0` with `Expecto` (and `YoloDev.Expecto.TestSdk` / `Microsoft.NET.Test.Sdk` so `dotnet test` and `pnpm test` work seamlessly).
  - Write unit tests covering:
    - `Shared/Domain.fs` jurisdiction lookups and regional tag filtering.
    - `Shared/Events.fs` event folding (`GameStarted`, `PlateObserved`, `ObservationLocationRecorded`, `ObservationDeleted`, `GameUpdated`, `GameEnded`).
    - `Client/Search.fs` fuzzy and postal-code matching (`"la"` $\rightarrow$ Louisiana).
    - `Client/State.fs` pure Elmish `update` state transitions.
- [ ] **8.2. UI, Persistence & Offline Verification**
  - Verify single-tap responsiveness, search auto-clear + keyboard blur, long-press observation popup, and individual observation deletion.
  - Verify game state and observations persist across page reloads via IndexedDB event replay.
  - Verify full offline operation in production preview (`pnpm build && pnpm preview`) with DevTools Network set to Offline.

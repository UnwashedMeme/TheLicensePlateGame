# The License Plate Game

## Broad Idea

While driving around, especially on road trips, we see license plates and like to keep a list of the ones we've seen: "Collect them all".

## Game Design

### First Launch & Lifecycle

- **Instant Start**: On initial launch,
  1. Prompts for GPS permission with an explanation of use.
  2. Drops straight into the Game Board with an automatically created and named game (e.g., `Trip - Oct 3, 2026`) so the player can immediately begin tapping with zero friction or setup screens.
- **Active Game Header**: The active game's name is displayed at the top of the screen in the app header alongside a progress counter (e.g., `18 / 61`, dynamic to the selected tab). Tapping the game name (or selecting "Edit Game Info" from the hamburger menu) opens a modal where the player can edit the game's **Name** and **Notes**.
- **Game Metadata**:
  - Name (e.g., "2027 Summer roadtrip")
  - Notes
  - Start Date (recorded automatically at creation)
  - End Date (recorded automatically when ended)

### The Game Board

- **Jurisdiction Tabs**: Toggle pills/tabs at the top allow filtering the board between:
  - **US States** (50 US states + DC)
  - **Canadian Provinces** (10 Canadian provinces)
  - **All** (all 61 jurisdictions)
- **Search Box**:
  - Always visible near the top of the Game Board.
  - Supports fuzzy matching on full jurisdiction names as well as 2-letter postal abbreviations (e.g., typing "la" matches Louisiana).
  - Hides buttons that do not match the query.
  - **Post-Tap Behavior**: Tapping a filtered jurisdiction button immediately logs the observation, auto-clears the search input, and dismisses (blurs) the mobile keyboard so the full board is immediately visible for the next sighting.
- **State Buttons**:
  - Sized generously in a touch-friendly mobile grid.
  - **Button Layout**:
    - **Top-left corner**: Two-letter postal abbreviation in small text (e.g., `TX`, `CA`, `ON`).
    - **Top-right corner**: Small count badge showing the number of observations recorded in the current game.
    - **Lower two-thirds**: State/province name as the main centered text.
  - **Visual Styling**:
    - Unobserved states (count = 0) display standard neutral button styling.
    - Observed states (count >= 1) display a distinct active accent/highlight background and border so collected states stand out clearly at a glance.

### Observation Logging & Tap Handling

- **Instant Capture**: Tapping a state button immediately increments the count and logs an entry containing the jurisdiction, timestamp, and GPS coordinates (if permission has been granted). Tapping is instant and never blocked by prompts or network latency.
- **Location Permissions & Asynchronous GPS**:
  - Location permission is requested on first launch accompanied by a brief explanation banner.
  - GPS resolution runs asynchronously in the background so button taps record instantaneously even if GPS lock is delayed, denied, or offline.
- **Accidental Taps & Deletion**:
  - No floating toast/snackbar banner is shown on tap to keep the UI clean and unobtrusive.
  - If a button was tapped by mistake, the player long-presses the state button to open the observation list and deletes the erroneous entry.

### Long-Press Observation Details

- **Empty State**: Long-pressing a button with 0 observations does nothing.
- **Observation Popup**: Long-pressing an observed state (count >= 1) opens a popup listing each observation recorded during the current game.
  - Each entry displays its date and time timestamp (e.g., `Oct 3, 2:45 PM`).
  - Each entry has an **X** button to delete that specific observation.

### Header & Menu Actions

A hamburger menu at the top right of the screen contains:
- **Hide/Show Observed Toggle**: Toggles visibility of states that have already been collected in the current game, allowing players to focus solely on uncollected plates.
- **Edit Game Info**: Opens a modal to review and edit the active game's Name and Notes.
- **New Game**:
  - Prompts with a confirmation dialog to prevent accidental resets.
  - Confirmed action automatically marks the active game as ended (setting `End Date` to now).
  - Presents fields to enter the new game's Name (prefilled with a default date-based title) and optional Notes.
  - Resets the Game Board to a clean slate start without deleting historical observation logs.

---

## Future Ideas

These features are planned for future phases and are not part of the initial MVP:

- **History View**: Mode displaying aggregate stats across selectable time periods (All Time, Last 1 Year, This Game) with buttons styled in a color gradient from "most collected" to "least collected".
- **Past Game History View**: Browse, inspect, and review details and statistics from previous game sessions.
- **Map Mode**: Interactive map visualization of recorded GPS coordinates, filterable by time ranges and specific jurisdictions (e.g., "Show where I was when I collected Vermont over the last year").
- **Multiplayer**: Share collections with friends or compete in real-time road trip challenges.
- **Server Sync**: Cloud synchronization and backup across multiple devices.
- **Accessibility Support**: Work with Screen reader, haptic feedback
- different color themes

---

## Tech Notes

- **Platform**: Progressive Web App (PWA) optimized for mobile devices, installable to the home screen, and fully operational offline.
- **Jurisdiction Scope**: 61 total jurisdictions — 50 US states, District of Columbia (DC), and 10 Canadian provinces.
- **Observation Storage**: Local storage via IndexedDB; observation records capture jurisdiction ID, game ID, ISO timestamp, and latitude/longitude coordinates (when available).
- **Offline First**: All core game features (tapping, searching, filtering, editing, and game resets) work seamlessly without an active internet connection.

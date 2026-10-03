# The License Plate Game


## Broad idea

While driving around, especially on road trips, we see license plates and like to keep the list of the ones we've seen. "Collect them all".

## Game Design

As a player, I want to be able to start a game on my mobile phone. This records the start date and presents me with the "Game Board": a list of the 50 states, DC, and the Canadian provinces. I'll collectively refer to this as "states" henceforth.

### The Game Board

The main body of the Game Board is a set of buttons, one for each state. These should be big enough that they are easy to tap.

At the top of the Game Board there is search box that should always be visible. This should fuzzy match and match state abbreviations (e.g. "la" should find Louisiana). Hide any button that doesn't match.

As I recognize a license plate I tap its entry on the Game Board. This should record a log entry of the datetime, the state, and my current location (if permissions have been granted) with no further prompt or confirmation. Tapping doesn't require any further prompt.

I may tap on a state I've already seen this "game" to record a second observation. Each state button should have a small count badge in the upper right corner of the button with the number of observations in the current game.

Long pressing on a state brings up a popup of the observation times this game, each with an X button that causes that entry to be deleted.

A small hamburger menu at the top right of the screen has a menu that contains:

- Hide/Show observed toggle. When "Hide Observed" is selected it removes the buttons for states that have been observed this game.
- "New Game" button; clicking this confirms before Continuing.
- History button

### New Game

Hitting "New Game" allows me to start a fresh game that shows every state as unobserved.  This should not clear or delete any data, just create a DB entry with the new datetime that is used to filter observations.

This should have a confirmation prompt.


## History View

I want a mode where I can see stats about all the observations for the time periods: 1. All time 2. Last 1y 3. This game.  A toggle set at the top of the screen would be nice.

When I select a given time period it should show the list of buttons with color gradient of "most collected" to "least collected".


### Future Ideas

These are other ideas we want to consider, but not part of the initial implementation.

- Multiplayer - share my collections with friends or compete to collect
- Map mode to show my recorded locations. This should also be filteratble on the same set of time ranges, as well as set of states. E.g. "Show me where I was for the last 1y when I collected 'Vermont'".
- Past Game History View - browse and review previous game sessions
- Server sync - cloud synchronization and online backup across devices




## Tech notes

- **Platform**: Progressive Web App (PWA) optimized for mobile, installable to the home screen, and fully operational offline.
- **Jurisdiction Scope**: 64 total entries — 50 US states + DC, 10 Canadian provinces, and 3 Canadian territories.
- **Observation Capture**: Instant single-tap logging capturing jurisdiction, timestamp, and GPS location (if permitted) without requiring text entry.
- **Safety & Lifecycle**: Quick undo option for accidental taps, and a confirmation prompt before ending an active game to start a new one.
- **Auth & Export**: Current version has no accounts/authentication and no data export; all data is stored locally on the device (with future server sync planned).

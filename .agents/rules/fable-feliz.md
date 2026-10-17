# Fable & Feliz Interop Guidelines and Patterns

This document captures verified patterns and nuances when working with **F# (Fable 5.19+)** and **Feliz (2.9+)** in this codebase. Refer to these rules to avoid re-researching or using deprecated APIs.

---

## 1. React Components & Hooks

### Component Declaration
- Use the `[<ReactComponent>]` attribute for any React component that uses hooks or local state. The compiler plugin (`Feliz.CompilerPlugins.dll`) transforms these into idiomatic React functional components compatible with Fast Refresh.
- **Do NOT use `React.functionComponent`**: It is obsolete in Fable 3+ / Feliz 2.x and logs runtime deprecation warnings in the browser console.
- Components can accept tupled or curried arguments, e.g.:
  ```fsharp
  [<ReactComponent>]
  let PlateCard (jurisdiction: Jurisdiction) (count: int) (dispatch: Msg -> unit) =
      // Component body with hooks
  ```

### Refs (`React.useRef`)
- `React.useRef<'T>(initialValue)` returns `IRefValue<'T>` (defined in `Fable.React.IRefValue<'T>`).
- Read and update the value directly using the mutable `.current` property:
  ```fsharp
  let timerRef = React.useRef<float option>(None)
  
  // Read:
  match timerRef.current with
  | Some id -> ...
  | None -> ...

  // Write:
  timerRef.current <- Some newId
  ```

### Effects & Disposables (`React.useEffect`)
- To register effect cleanups, return a disposable via `React.createDisposable`:
  ```fsharp
  React.useEffect(fun () ->
      React.createDisposable(fun () ->
          match timerRef.current with
          | Some t -> Browser.Dom.window.clearTimeout t
          | None -> ()
      )
  , [| |])
  ```

---

## 2. Browser Timers & Pointer Events

### Timers (`Browser.Dom.window`)
- `Browser.Dom.window.setTimeout(callback: unit -> unit, ms: int)` returns a `float` (not an `int` or Node `Timeout` object).
- `Browser.Dom.window.clearTimeout(handle: float)` takes the `float` handle.

### Pointer Events in Feliz
- Feliz exposes standard pointer event properties:
  - `prop.onPointerDown (handler: Browser.Types.PointerEvent -> unit)`
  - `prop.onPointerMove (handler: Browser.Types.PointerEvent -> unit)`
  - `prop.onPointerUp (handler: Browser.Types.PointerEvent -> unit)`
  - `prop.onPointerCancel (handler: Browser.Types.PointerEvent -> unit)`
- Pointer events provide client coordinates via `e.clientX` and `e.clientY` (`float`).
- Suppress browser context menus (e.g., on mobile long-press) using:
  ```fsharp
  prop.onContextMenu (fun e -> e.preventDefault ())
  ```

---

## 3. Date & Timestamp Formatting

- Fable 5 compiles `DateTimeOffset.ToLocalTime().ToString("format")` natively via `Date.js` and `DateOffset.js`.
- Standard custom format strings such as `"MMM d, h:mm tt"` format correctly into strings like `"Oct 3, 2:45 PM"`.
- Do not import external JavaScript date packages (date-fns, dayjs, moment) when standard F#/.NET format strings are sufficient.

---

## 4. Build & Verification Pipeline

- Compilation pipeline: `dotnet fable src/Client -o src/Client -e .fs.js` followed by Vite bundling (`pnpm run build` or `pnpm run dev`).
- Rely on `dotnet fable` and `pnpm build` to verify compilation and bundle output rather than running ad-hoc Node evaluation scripts against internal fable modules.

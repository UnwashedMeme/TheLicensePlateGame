# Developer Environment Setup (Ubuntu)

This guide covers installing all prerequisites for developing **The License Plate Game** on Ubuntu (including Ubuntu 22.04, 24.04, and 26.04 LTS).

---

## 1. System Packages & Build Tools

First, ensure your package index is up to date and standard utilities are installed:

```bash
sudo apt update
sudo apt install -y curl wget git build-essential
```

---

## 2. .NET SDK (with F# support)

F# is included directly with the .NET SDK.

### Option A: Install from Ubuntu Repositories (Recommended on Ubuntu 26.04)

```bash
sudo apt install -y dotnet-sdk-10.0
```

### Option B: Official Microsoft dotnet-install script

If your repository doesn't have the 10.0 package directly:

```bash
curl -sSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 10.0
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$DOTNET_ROOT:$DOTNET_ROOT/tools:$PATH"
```

### Verify .NET & F# Installation

```bash
dotnet --version
dotnet fsi --help | head -n 5
```

---

## 3. Node.js & pnpm (for Vite & Bundling)

We target **Node.js LTS** (v22 or v24 LTS in late 2026) alongside **pnpm** (fast, disk-efficient package manager).

### Step 3.1: Install Node.js

Choose either **NodeSource** (system-wide via apt) or **nvm** (per-user version manager):

#### Option A: NodeSource LTS (System-wide via apt)

```bash
# Set up NodeSource repository for current LTS (tracks Active LTS v22/v24)
curl -fsSL https://deb.nodesource.com/setup_lts.x | sudo -E bash -

# Install Node.js (includes npm & corepack)
sudo apt install -y nodejs
```

#### Option B: NVM (Node Version Manager — No sudo needed)

```bash
# Install NVM
curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/HEAD/install.sh | bash

# Load NVM into current shell (or restart terminal)
export NVM_DIR="$HOME/.nvm"
[ -s "$NVM_DIR/nvm.sh" ] && \. "$NVM_DIR/nvm.sh"

# Install and use the latest LTS release (v22 or v24)
nvm install --lts
nvm use --lts
```

### Step 3.2: Enable or Install pnpm

You can enable pnpm via Node's built-in `corepack`, install via npm, or install using the official standalone script:

```bash
# Option 1: Corepack (Built into Node.js)
corepack enable

# Option 2: Via npm global install
npm install -g pnpm

# Option 3: Standalone shell script (no Node prerequisites required)
curl -fsSL https://get.pnpm.io/install.sh | sh -
```

### Verify Node & pnpm Installation

```bash
node -v   # Should report v22.x or v24.x LTS
pnpm -v   # Should report v12.x
```

---

## 4. .NET Local Tools (Fable & Fantomas)

We use **Fable 5** to compile F# to modern JavaScript ES modules (with source maps) and **Fantomas 8** for F# code formatting. Both are pinned in [`.config/dotnet-tools.json`](file:///home/nathan/projects/TheLicensePlateGame/.config/dotnet-tools.json).

In the project root directory:

```bash
# Restore tools from .config/dotnet-tools.json (recommended when cloning)
dotnet tool restore

# Or, if initializing from scratch:
dotnet new tool-manifest
dotnet tool install fable
dotnet tool install fantomas
```

### Verify Tool Installations

```bash
dotnet tool run fable --version
dotnet tool run fantomas --version
```

### Tooling Architecture Notes

* **Why `dotnet fable` CLI (with `-s -o .fable-build`) vs. `vite-plugin-fable`?**
  We currently run `dotnet fable` outputting to an out-of-source `.fable-build/` directory with source maps enabled (`-s`). We may re-evaluate [`vite-plugin-fable`](https://github.com/fable-compiler/vite-plugin-fable) in the future:
  * **`vite-plugin-fable` Pros**: Single-process Vite workflow (`vite` / `vite build`); compiles `.fs` files on-demand in memory via a background JSON-RPC daemon with zero intermediate `.js` files on disk.
  * **`vite-plugin-fable` Cons**: Bundles its own prebuilt .NET daemon linked against a specific `Fable.Compiler` version inside the npm package (making it non-trivial to bump Fable independently for compiler bug fixes), and the repository is currently seeking a new maintainer.
  * **`dotnet fable -s -o .fable-build` Pros**: Gives us full `.fs` source maps in browser DevTools, keeps `src/` 100% free of generated `.fs.js` clutter, lets us inspect emitted JS in `.fable-build/` if ever needed, and lets us bump Fable versions immediately in `.config/dotnet-tools.json`.
* **Why we don't use `Femto`**:
  `Femto` inspects `<NpmDependencies>` metadata inside Fable NuGet packages to sync npm packages. Because our Fable dependencies bind to native browser APIs (`Fable.Browser.*`), pure F# (`Fable.Elmish`, `Thoth.Json`), or React (`Feliz`, `Feliz.UseElmish`), and many NuGet packages lag on updating their internal peer-dependency ceilings for **React 19** and **Vite 8**, `Femto` is unnecessary here and can trigger false-positive downgrade warnings.

---

## 5. Quick Healthcheck Summary

Run this single snippet to verify that everything is ready:

```bash
echo "=== Environment Check ==="
echo "Dotnet:   $(dotnet --version 2>/dev/null || echo 'NOT FOUND')"
echo "Node:     $(node -v 2>/dev/null || echo 'NOT FOUND')"
echo "pnpm:     $(pnpm -v 2>/dev/null || echo 'NOT FOUND')"
echo "Fable:    $(dotnet tool run fable --version 2>/dev/null || echo 'NOT INSTALLED')"
echo "Fantomas: $(dotnet tool run fantomas --version 2>/dev/null || echo 'NOT INSTALLED')"
echo "========================="
```

---

## 6. Developer Workflow Commands

Once dependencies are installed (`dotnet tool restore && pnpm install`), use the following `pnpm` scripts:

```bash
# Install JS dependencies using pnpm
pnpm install

# Start development server with live reload & source maps (Fable watch -> .fable-build/ + Vite dev server)
pnpm dev

# Build the complete production bundle (dotnet build + Fable compile + Vite PWA bundle into dist/)
pnpm build

# Preview the production PWA bundle locally
pnpm preview

# Run the Expecto test suite on .NET 10 (Shared domain, event-sourcing, search, and Elmish state tests)
pnpm test

# Format F# source files with Fantomas
pnpm format

# Clean build artifacts (dist, .fable-build, bin, and obj)
pnpm clean
```

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

## 4. Fable .NET Tool

We use **Fable 5** (v5.19+) to compile F# to modern JavaScript ES modules.

In the project root directory:

```bash
# Initialize a local dotnet tool manifest if not already present
dotnet new tool-manifest

# Install Fable locally in the repository (installs latest Fable 5.x)
dotnet tool install fable
```

### Verify Fable Installation

```bash
dotnet tool run fable --version
```

---

## 5. Quick Healthcheck Summary

Run this single snippet to verify that everything is ready:

```bash
echo "=== Environment Check ==="
echo "Dotnet: $(dotnet --version 2>/dev/null || echo 'NOT FOUND')"
echo "Node:   $(node -v 2>/dev/null || echo 'NOT FOUND')"
echo "pnpm:   $(pnpm -v 2>/dev/null || echo 'NOT FOUND')"
echo "Fable:  $(dotnet tool run fable --version 2>/dev/null || echo 'NOT INSTALLED')"
echo "========================="
```

---

## 6. Running the Development Server

Once dependencies are installed, you will start development with:

```bash
# Install dependencies using pnpm (creates hard links to global store)
pnpm install

# Start development server with live reload (Vite + Fable)
pnpm dev
```

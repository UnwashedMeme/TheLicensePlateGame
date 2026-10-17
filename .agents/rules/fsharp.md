# FSharp development

This doc provides general guidance for how AI agents should interact with the FSharp and Fable environments.

## API Research & Binding Discovery (Source-First Policy)
- **Inspect Source First**: When working with Fable bindings (e.g., `Fable.Browser.*`, `Feliz.*`) or open-source libraries whose exact signatures, overloads, or namespaces are uncertain, ALWAYS read the original source code directly from GitHub (via `read_url_content` on raw files or repositories) or official documentation first.
- **Never Delve into DLLs with Ad-hoc Reflection**: Do NOT attempt ad-hoc FSI scripts, binary string dumps, or .NET reflection (`GetMembers()`) to discover bindings. Fable/interop assemblies contain `jsNative` stubs and compile-time attributes (`[<Global>]`, `[<AutoOpen>]`, `[<Emit>]`) that cannot be properly understood through runtime reflection.
- **Rely on Compiler Diagnostics**: Run `dotnet build` and `dotnet fable` for authoritative compiler feedback. The compiler explicitly details candidate overloads, parameter shapes, and missing namespaces when a mismatch occurs.

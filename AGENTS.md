# Agent Instructions

## Post-Implementation Formatting (Mandatory)

After completing any code implementation or modification in this repository, you **must** invoke `./format.ps1` from the repository root (`D:\GitHub\AdaskoTheBeAsT.Dapper.Oracle`) before finishing the task:

```powershell
.\format.ps1
```

### What `format.ps1` does
1. Restores local .NET tools (`dotnet tool restore` from `.config/dotnet-tools.json`).
2. Runs `dotnet format AdaskoTheBeAsT.Dapper.Oracle.sln` to apply Roslyn code-style (`IDE*`) and analyzer (`SA*`, `RCS*`, `MA*`) fixes.
3. Runs `dotnet jb cleanupcode AdaskoTheBeAsT.Dapper.Oracle.sln --profile="Built-in: Reformat Code"` to apply ReSharper layout and line-wrapping rules from `.editorconfig`.
4. Runs `dotnet format analyzers AdaskoTheBeAsT.Dapper.Oracle.sln --diagnostics SA1413 SA1500 SA1134` to ensure trailing commas and brace/attribute placement satisfy StyleCop.

After running `.\format.ps1`, verify that the solution builds cleanly with `dotnet build AdaskoTheBeAsT.Dapper.Oracle.sln --no-incremental`.

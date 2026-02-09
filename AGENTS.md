# AGENTS Instructions for `DotnetDebug`

## MCP Debugging (Killer Bug)
- Always use MCP server `killer-bug-dotnetdebug` at `http://localhost:3101/mcp`.
- Prefer launching debug with config name: `C#: Dotnet Debug (MCP CoreCLR)`.
- Do **not** modify `launch.json` for routine debugging; the project is preconfigured.

## Required Debug Workflow
1. Check MCP health: `Invoke-WebRequest http://localhost:3101/health -UseBasicParsing`.
2. Start with config via MCP tool `debug_startWithConfig` and `configName = "C#: Dotnet Debug (MCP CoreCLR)"`.
3. Inspect runtime data (`debug_getStackTrace`, `debug_getVariables`, `debug_evaluate`).
4. End with `debug_stop`.

## If MCP Tool Handshake Fails
- Use direct JSON-RPC calls to `http://localhost:3101/mcp` from PowerShell (`tools/list`, `tools/call`).
- Keep using the same config name `C#: Dotnet Debug (MCP CoreCLR)`.

## Notes
- `stopAtEntry: true` is enabled in the MCP profile to inspect `args` immediately at startup.
- Build happens automatically via VS Code task `build-dotnetdebug` (`preLaunchTask` in launch config).

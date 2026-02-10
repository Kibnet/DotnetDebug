# Debug Runbook (MCP + CoreCLR)

## Goal
Fast, reproducible debugging in `DotnetDebug` via MCP with minimal friction.

## Default Profile
- Use MCP server: `killer-bug-dotnetdebug` (`http://localhost:3101/mcp`)
- Use config: `C#: Dotnet Debug (MCP CoreCLR)`

## Quick Start Checklist
1. Check MCP health.
2. Ensure clean debug state (`debug_stop`, no stale breakpoints).
3. Start with `debug_startWithConfig`.
4. Confirm entry location via `debug_getStackTrace`.
5. Inspect only key values (`debug_evaluate`) before stepping.

## Productive Stepping Strategy
1. Prefer breakpoints + `debug_continue` over line-by-line stepping.
2. Use `debug_stepOver` only around suspicious logic.
3. In loops, avoid stepping every iteration:
   - set a breakpoint after the loop, or
   - set a conditional breakpoint for a specific iteration/value.
4. After any step/continue, verify actual location with `debug_getStackTrace`.

## Minimal Inspection Set
- Entry: `args.Length`, `string.Join(" ", args)`
- Branch flags: feature flags / mode booleans (for this app: `interactiveMode`)
- Loop checks: current item (`arg`), parsed value (`value`), counters (`numbers.Count`)
- Pre-output checks: computed result (`gcd`)

## Recovery for `ConnectionLostException` on `step/continue`
Symptoms:
- `debug_stepOver` / `debug_continue` fails with `ConnectionLostException`.

Actions:
1. Stop debug session.
2. Restart C# Dev Kit backend processes (`Microsoft.VisualStudio.Code.Server`, `Microsoft.VisualStudio.Code.ServiceHost`, `Microsoft.VisualStudio.Code.ServiceController`).
3. Start debug session again.
4. Re-run step/continue.

## Stable Workspace Settings
These settings improved stability in this repo:

```json
{
  "dotnet.useLegacyDotnetResolution": true,
  "csharp.experimental.debug.hotReload": false,
  "csharp.debug.hotReloadOnSave": false
}
```

## Session Hygiene
1. Remove temporary breakpoints when finished.
2. End with `debug_stop`.
3. Confirm `debug_listBreakpoints` returns empty.

## Reporting Template (Short)
1. Start position.
2. Key values observed.
3. Branches taken/skipped.
4. End state/result.
5. Any debugger anomalies and workaround used.

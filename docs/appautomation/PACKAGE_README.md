# AppAutomation

`AppAutomation` is a reusable desktop UI automation framework extracted from this repository.

Packages:

- `AppAutomation.Abstractions`: automation contracts, page model primitives, waits and diagnostics.
- `AppAutomation.Authoring`: source generator/analyzers for `[UiControl]`-based page objects.
- `AppAutomation.TUnit`: `UiTestBase` and shared test helpers for `TUnit`.
- `AppAutomation.Avalonia.Headless`: in-process Avalonia Headless runtime.
- `AppAutomation.FlaUI`: Windows desktop runtime on top of FlaUI.

Recommended test-solution topology:

- `<MyApp>.UiTests.Authoring`: page objects and shared scenarios.
- `<MyApp>.UiTests.Headless`: optional headless runtime tests.
- `<MyApp>.UiTests.FlaUI`: optional Windows desktop runtime tests.

Full setup guide:

- Quickstart: https://github.com/Kibnet/AppAutomation/blob/main/docs/appautomation/quickstart.md
- Project topology: https://github.com/Kibnet/AppAutomation/blob/main/docs/appautomation/project-topology.md
- Publishing: https://github.com/Kibnet/AppAutomation/blob/main/docs/appautomation/publishing.md

# AppAutomation

[English](#appautomation) | [Русский](#-русская-версия)

[![NuGet Version](https://img.shields.io/nuget/v/AppAutomation.Abstractions?label=NuGet%20(AppAutomation.Abstractions))](https://www.nuget.org/packages/AppAutomation.Abstractions)

`AppAutomation` is a .NET framework for UI automation of Avalonia desktop applications. The target consumer flow is:

1. You integrate the framework via NuGet, not by downloading source code;
2. You create the canonical test topology with a single command;
3. You write page objects and shared scenarios once;
4. You run the same scenarios in both `Headless` and `FlaUI`.

The resulting structure of a consumer repository should look like this:

```text
tests/
  MyApp.UiTests.Authoring/
  MyApp.UiTests.Headless/
  MyApp.UiTests.FlaUI/
  MyApp.AppAutomation.TestHost/
```

`Authoring` owns page objects and shared tests. `Headless` and `FlaUI` only run these scenarios through different runtime adapters.

## Compatibility

Supported baseline:

| Component | Support |
| --- | --- |
| `AppAutomation.Abstractions` | `net8.0+` |
| `AppAutomation.Session.Contracts` | `net8.0+` |
| `AppAutomation.TUnit` | `net8.0+` |
| `AppAutomation.TestHost.Avalonia` | `net8.0+` |
| `AppAutomation.Avalonia.Headless` | `net8.0`, `net10.0` |
| `AppAutomation.FlaUI` | `net8.0-windows7.0`, `net10.0-windows7.0` |
| `FlaUI` runtime | Windows only |
| Template package | `dotnet new` |
| CLI tool | `.NET tool`, command `appautomation` |

Full matrix: [docs/appautomation/compatibility.md](docs/appautomation/compatibility.md)

## Fast Path

Replace `1.1.0` with the desired package version.

### 1. Install template package

```powershell
dotnet new install AppAutomation.Templates::1.1.0
```

### 2. Install CLI tool

Locally in repo:

```powershell
dotnet tool install --tool-path .\.tools AppAutomation.Tooling --version 1.1.0
```

Or globally:

```powershell
dotnet tool install --global AppAutomation.Tooling --version 1.1.0
```

### 3. Generate canonical topology

From the root of your consumer repository:

```powershell
dotnet new appauto-avalonia --name MyApp --AppAutomationVersion 1.1.0
```

The template will create:

- `tests/MyApp.UiTests.Authoring`
- `tests/MyApp.UiTests.Headless`
- `tests/MyApp.UiTests.FlaUI`
- `tests/MyApp.AppAutomation.TestHost`
- `APPAUTOMATION_NEXT_STEPS.md`

### 4. Run doctor immediately

If the tool is installed locally:

```powershell
.\.tools\appautomation doctor --repo-root .
```

If the tool is installed globally:

```powershell
appautomation doctor --repo-root .
```

`doctor` checks:

- whether canonical topology exists;
- whether you've switched to source dependency instead of `PackageReference`;
- whether `TargetFramework` is compatible;
- whether `NuGet.Config` exists;
- whether SDK is pinned via `global.json`.

## What to do in consumer repo after generation

The template creates the correct topology, but cannot know your AUT-specific bootstrap. Next, you need to do exactly the following things.

### 1. Fill in the real launch/bootstrap in `TestHost`

File:

```text
tests/MyApp.AppAutomation.TestHost/MyAppAppLaunchHost.cs
```

You need to replace placeholder values:

- solution file name;
- relative path to desktop `.csproj`;
- `TargetFramework` of AUT;
- desktop executable name;
- `CreateHeadlessLaunchOptions()` with real `Window` creation.

Framework helpers that are already available out of the box:

- `AppAutomation.TestHost.Avalonia.AvaloniaDesktopLaunchHost`
- `AppAutomation.TestHost.Avalonia.AvaloniaHeadlessLaunchHost`
- `AppAutomation.TestHost.Avalonia.TemporaryDirectory`

### 2. Set `AutomationId` in the application

Minimum for the first iteration:

- root window;
- main tabs / navigation anchors;
- critical input/button/result controls;
- key child controls for composite widgets.

Example:

```xml
<TabControl automation:AutomationProperties.AutomationId="MainTabs">
  <TabItem automation:AutomationProperties.AutomationId="SmokeTabItem" />
</TabControl>
```

### 3. Connect Headless session hooks

File:

```text
tests/MyApp.UiTests.Headless/Infrastructure/HeadlessSessionHooks.cs
```

There you need to start your `Avalonia.Headless` session and register it via `HeadlessRuntime.SetSession(...)`.

### 4. Describe page objects and shared scenarios

In the `Authoring` project you:

- declare `[UiControl(...)]` for simple controls;
- manually add composite abstractions if necessary;
- write shared scenarios once.

### 5. Stabilize `Headless` first, then enable `FlaUI`

Commands:

```powershell
dotnet test tests/MyApp.UiTests.Headless/MyApp.UiTests.Headless.csproj -c Debug
dotnet test tests/MyApp.UiTests.FlaUI/MyApp.UiTests.FlaUI.csproj -c Debug
```

## What's already available out of the box

`AppAutomation` now covers typical integration gaps that consumers used to write manually:

- `dotnet new` template for canonical Avalonia topology;
- `appautomation doctor`;
- reusable `AppAutomation.TestHost.Avalonia`;
- desktop launch helpers with repo-root / project-path / build-before-launch;
- headless launch helpers on top of `BeforeLaunchAsync`, `CreateMainWindow`, `CreateMainWindowAsync`;
- adapter registration API via `WithAdapters(...)`;
- built-in composite abstraction `ISearchPickerControl` and `WithSearchPicker(...)`;
- package-based smoke path via `eng/smoke-consumer.ps1`.

## What remains consumer responsibility

The framework cannot automate these honestly and completely, so these things remain on the consumer side:

- domain-specific test data and permissions;
- auth bypass / login story;
- exact startup semantics of AUT;
- decision on which secondary controls are better simplified with data;
- adding `AutomationId` in the AUT itself.

## Do Not

- Do not pull `src/AppAutomation.*` into your consumer repo as source dependency unless there's an extreme reason.
- Do not duplicate tests from `Authoring` in runtime projects.
- Do not start with a complex end-to-end path. Start with one critical smoke scenario.
- Do not automate all controls before login / startup / settings path is stabilized.
- Do not hide repo-specific bootstrap inside a reusable framework package.

## Reference Implementation

Working reference in this repository:

- [sample/DotnetDebug.AppAutomation.Authoring](sample/DotnetDebug.AppAutomation.Authoring)
- [sample/DotnetDebug.AppAutomation.Avalonia.Headless.Tests](sample/DotnetDebug.AppAutomation.Avalonia.Headless.Tests)
- [sample/DotnetDebug.AppAutomation.FlaUI.Tests](sample/DotnetDebug.AppAutomation.FlaUI.Tests)
- [sample/DotnetDebug.AppAutomation.TestHost](sample/DotnetDebug.AppAutomation.TestHost)

## Next Steps

- Step-by-step consumer flow: [docs/appautomation/quickstart.md](docs/appautomation/quickstart.md)
- Pre-flight checklist: [docs/appautomation/adoption-checklist.md](docs/appautomation/adoption-checklist.md)
- Canonical project responsibilities: [docs/appautomation/project-topology.md](docs/appautomation/project-topology.md)
- Advanced bootstrap and composite controls: [docs/appautomation/advanced-integration.md](docs/appautomation/advanced-integration.md)
- Packaging and release flow: [docs/appautomation/publishing.md](docs/appautomation/publishing.md)

---

# 🇷🇺 Русская версия

[English](#appautomation) | [Русский](#-русская-версия)

`AppAutomation` это .NET framework для UI automation desktop-приложений на Avalonia. Его целевой consumer flow такой:

1. вы подключаете framework через NuGet, а не через скачивание исходников;
2. одной командой создаёте canonical test topology;
3. пишете page objects и shared scenarios один раз;
4. запускаете те же самые сценарии и в `Headless`, и в `FlaUI`.

Итоговая структура consumer-репозитория должна выглядеть так:

```text
tests/
  MyApp.UiTests.Authoring/
  MyApp.UiTests.Headless/
  MyApp.UiTests.FlaUI/
  MyApp.AppAutomation.TestHost/
```

`Authoring` владеет page objects и shared tests. `Headless` и `FlaUI` только запускают эти же сценарии через разные runtime adapters.

## Compatibility

Поддерживаемый базовый путь:

| Компонент | Поддержка |
| --- | --- |
| `AppAutomation.Abstractions` | `net8.0+` |
| `AppAutomation.Session.Contracts` | `net8.0+` |
| `AppAutomation.TUnit` | `net8.0+` |
| `AppAutomation.TestHost.Avalonia` | `net8.0+` |
| `AppAutomation.Avalonia.Headless` | `net8.0`, `net10.0` |
| `AppAutomation.FlaUI` | `net8.0-windows7.0`, `net10.0-windows7.0` |
| `FlaUI` runtime | только Windows |
| Template package | `dotnet new` |
| CLI tool | `.NET tool`, команда `appautomation` |

Полная матрица: [docs/appautomation/compatibility.md](docs/appautomation/compatibility.md)

## Fast Path

Замените `1.1.0` на нужную версию пакетов.

### 1. Установите template package

```powershell
dotnet new install AppAutomation.Templates::1.1.0
```

### 2. Установите CLI tool

Локально в repo:

```powershell
dotnet tool install --tool-path .\.tools AppAutomation.Tooling --version 1.1.0
```

Или глобально:

```powershell
dotnet tool install --global AppAutomation.Tooling --version 1.1.0
```

### 3. Сгенерируйте canonical topology

Из корня вашего consumer-репозитория:

```powershell
dotnet new appauto-avalonia --name MyApp --AppAutomationVersion 1.1.0
```

Шаблон создаст:

- `tests/MyApp.UiTests.Authoring`
- `tests/MyApp.UiTests.Headless`
- `tests/MyApp.UiTests.FlaUI`
- `tests/MyApp.AppAutomation.TestHost`
- `APPAUTOMATION_NEXT_STEPS.md`

### 4. Сразу прогоните doctor

Если tool установлен в локальную папку:

```powershell
.\.tools\appautomation doctor --repo-root .
```

Если tool установлен глобально:

```powershell
appautomation doctor --repo-root .
```

`doctor` проверяет:

- есть ли canonical topology;
- не ушли ли вы в source dependency вместо `PackageReference`;
- совместимы ли `TargetFramework`;
- есть ли `NuGet.Config`;
- закреплён ли SDK через `global.json`.

## Что сделать в consumer repo после генерации

Шаблон создаёт правильную topology, но не может знать ваш AUT-specific bootstrap. Дальше нужно сделать ровно следующие вещи.

### 1. Вписать реальный launch/bootstrap в `TestHost`

Файл:

```text
tests/MyApp.AppAutomation.TestHost/MyAppAppLaunchHost.cs
```

Нужно заменить placeholder values:

- solution file name;
- relative path до desktop `.csproj`;
- `TargetFramework` AUT;
- имя desktop executable;
- `CreateHeadlessLaunchOptions()` с реальным созданием `Window`.

Framework helpers, которые для этого уже есть из коробки:

- `AppAutomation.TestHost.Avalonia.AvaloniaDesktopLaunchHost`
- `AppAutomation.TestHost.Avalonia.AvaloniaHeadlessLaunchHost`
- `AppAutomation.TestHost.Avalonia.TemporaryDirectory`

### 2. Проставить `AutomationId` в приложении

Минимум для первой итерации:

- root window;
- main tabs / navigation anchors;
- критичные input/button/result controls;
- ключевые child controls у composite widgets.

Пример:

```xml
<TabControl automation:AutomationProperties.AutomationId="MainTabs">
  <TabItem automation:AutomationProperties.AutomationId="SmokeTabItem" />
</TabControl>
```

### 3. Подключить Headless session hooks

Файл:

```text
tests/MyApp.UiTests.Headless/Infrastructure/HeadlessSessionHooks.cs
```

Там нужно запустить ваш `Avalonia.Headless` session и зарегистрировать его через `HeadlessRuntime.SetSession(...)`.

### 4. Описать page objects и shared scenarios

В `Authoring` проекте вы:

- объявляете `[UiControl(...)]` для простых controls;
- при необходимости вручную добавляете composite abstractions;
- пишете shared scenarios один раз.

### 5. Сначала стабилизировать `Headless`, потом включать `FlaUI`

Команды:

```powershell
dotnet test tests/MyApp.UiTests.Headless/MyApp.UiTests.Headless.csproj -c Debug
dotnet test tests/MyApp.UiTests.FlaUI/MyApp.UiTests.FlaUI.csproj -c Debug
```

## Что уже есть из коробки

`AppAutomation` теперь закрывает типовые integration gaps, которые раньше consumer писал вручную:

- `dotnet new` template для canonical Avalonia topology;
- `appautomation doctor`;
- reusable `AppAutomation.TestHost.Avalonia`;
- desktop launch helpers с repo-root / project-path / build-before-launch;
- headless launch helpers поверх `BeforeLaunchAsync`, `CreateMainWindow`, `CreateMainWindowAsync`;
- adapter registration API через `WithAdapters(...)`;
- built-in composite abstraction `ISearchPickerControl` и `WithSearchPicker(...)`;
- package-based smoke path через `eng/smoke-consumer.ps1`.

## Что остаётся consumer responsibility

Framework не может автоматизировать это честно и полностью, поэтому эти вещи остаются на стороне consumer-а:

- domain-specific test data и permissions;
- auth bypass / login story;
- точная startup semantics AUT;
- решение, какие secondary controls лучше упростить данными;
- добавление `AutomationId` в самом AUT.

## Do Not

- Не тяните `src/AppAutomation.*` в consumer repo как source dependency, если нет совсем крайней причины.
- Не дублируйте tests из `Authoring` в runtime projects.
- Не начинайте со сложного end-to-end path. Сначала один критичный smoke scenario.
- Не автоматизируйте все controls подряд до того, как стабилизирован login / startup / settings path.
- Не прячьте repo-specific bootstrap внутрь reusable framework package.

## Reference Implementation

Working reference в этом репозитории:

- [sample/DotnetDebug.AppAutomation.Authoring](sample/DotnetDebug.AppAutomation.Authoring)
- [sample/DotnetDebug.AppAutomation.Avalonia.Headless.Tests](sample/DotnetDebug.AppAutomation.Avalonia.Headless.Tests)
- [sample/DotnetDebug.AppAutomation.FlaUI.Tests](sample/DotnetDebug.AppAutomation.FlaUI.Tests)
- [sample/DotnetDebug.AppAutomation.TestHost](sample/DotnetDebug.AppAutomation.TestHost)

## Дальше

- Пошаговый consumer flow: [docs/appautomation/quickstart.md](docs/appautomation/quickstart.md)
- Pre-flight checklist: [docs/appautomation/adoption-checklist.md](docs/appautomation/adoption-checklist.md)
- Canonical project responsibilities: [docs/appautomation/project-topology.md](docs/appautomation/project-topology.md)
- Advanced bootstrap and composite controls: [docs/appautomation/advanced-integration.md](docs/appautomation/advanced-integration.md)
- Packaging and release flow: [docs/appautomation/publishing.md](docs/appautomation/publishing.md)

# DotnetDebug

Учебно-практический репозиторий про отладку и тестирование .NET. Внутри:
- консольное приложение;
- Avalonia desktop UI;
- unit/UI тесты на `TUnit`;
- reusable framework для UI automation под именем `AppAutomation`.

## Состав решения

| Проект | Назначение |
| --- | --- |
| `src/DotnetDebug.Core` | Доменная логика вычисления НОД и трассировка шагов алгоритма. |
| `src/DotnetDebug` | CLI-приложение. |
| `src/DotnetDebug.Avalonia` | Desktop UI на Avalonia. |
| `src/AppAutomation.Abstractions` | Канонические automation contracts, locator manifest, waits и diagnostics. |
| `src/AppAutomation.Authoring` | Source generator и authoring diagnostics для page objects. |
| `src/AppAutomation.Session.Contracts` | Launch/session contracts. |
| `src/AppAutomation.TUnit` | Runtime-agnostic test base и polling assertions для `TUnit`. |
| `src/AppAutomation.FlaUI` | Runtime adapter поверх FlaUI. |
| `src/AppAutomation.Avalonia.Headless` | Headless runtime adapter поверх Avalonia Headless. |
| `src/DotnetDebug.AppAutomation.TestHost` | Repo-only host для запуска demo AUT в тестах. |
| `tests/DotnetDebug.Tests` | Unit-тесты доменной логики и базовых contracts. |
| `tests/DotnetDebug.AppAutomation.Authoring` | Shared authoring assembly с page objects и shared UI-сценариями. |
| `tests/DotnetDebug.AppAutomation.Avalonia.Headless.Tests` | Headless UI runtime tests. |
| `tests/DotnetDebug.AppAutomation.FlaUI.Tests` | Desktop UI runtime tests на FlaUI. |
| `tests/AppAutomation.Abstractions.Tests` | Contract/regression tests для abstraction layer. |
| `tests/AppAutomation.Authoring.Tests` | Generator/analyzer regression tests. |

## Требования к окружению

- stable `.NET 10` SDK, совместимый с `global.json`;
- Windows для `FlaUI` тестов (`net10.0-windows7.0`);
- любая ОС для unit-тестов и Avalonia headless tests.

## Быстрый старт

```powershell
dotnet restore
dotnet build DotnetDebug.sln -c Debug
```

### Запуск CLI

```powershell
dotnet run --project src/DotnetDebug/DotnetDebug.csproj -- 48 18 30
dotnet run --project src/DotnetDebug/DotnetDebug.csproj -- -i
```

### Запуск Avalonia UI

```powershell
dotnet run --project src/DotnetDebug.Avalonia/DotnetDebug.Avalonia.csproj
```

## Тесты

### Unit и contract tests

```powershell
dotnet test tests/DotnetDebug.Tests/DotnetDebug.Tests.csproj -c Debug
dotnet test tests/AppAutomation.Abstractions.Tests/AppAutomation.Abstractions.Tests.csproj -c Debug
dotnet test tests/AppAutomation.Authoring.Tests/AppAutomation.Authoring.Tests.csproj -c Debug
```

### Headless UI

```powershell
dotnet test tests/DotnetDebug.AppAutomation.Avalonia.Headless.Tests/DotnetDebug.AppAutomation.Avalonia.Headless.Tests.csproj -c Debug
```

### Desktop UI (FlaUI, только Windows)

```powershell
dotnet test tests/DotnetDebug.AppAutomation.FlaUI.Tests/DotnetDebug.AppAutomation.FlaUI.Tests.csproj -c Debug
```

## VS Code / MCP debug

В репозитории уже есть `.vscode` конфиги для:
- запуска CLI;
- запуска Avalonia UI;
- запуска unit tests;
- запуска `DotnetDebug.AppAutomation.FlaUI.Tests`.

При необходимости обновляйте `--filter-uid` в launch profile под конкретный тест.

## Что здесь полезно как reference

- `AppAutomation.Abstractions` показывает, как отделить runtime-neutral contracts от adapter-specific logic.
- `AppAutomation.Authoring` показывает, как собирать page-object authoring через source generator.
- `DotnetDebug.AppAutomation.Authoring` показывает shared authoring project вместо `Compile Include`.
- `DotnetDebug.AppAutomation.Avalonia.Headless.Tests` и `DotnetDebug.AppAutomation.FlaUI.Tests` показывают один набор сценариев на двух runtime.

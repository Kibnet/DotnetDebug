# DotnetDebug

Учебно-практический репозиторий про отладку и тестирование .NET:
- консольное приложение;
- Avalonia desktop UI;
- unit/UI тесты на `TUnit`;
- две переиспользуемые библиотеки для UI-автоматизации: `FlaUI.EasyUse` и `FlaUI.EasyUse.TUnit`.

Главная ценность репозитория для разработчика: здесь есть не только демо, но и готовые паттерны, которые можно унести в свой проект почти без изменений.

## Что внутри

| Проект | Назначение |
|---|---|
| `src/DotnetDebug.Core` | Доменная логика вычисления НОД (включая трассировку шагов алгоритма). |
| `src/DotnetDebug` | CLI-приложение для вычисления НОД. |
| `src/DotnetDebug.Avalonia` | Desktop UI (Avalonia) для визуализации результата и шагов. |
| `src/FlaUI.EasyUse` | Утилиты запуска desktop-приложения и удобные extension-методы поверх FlaUI. |
| `src/FlaUI.EasyUse.TUnit` | Ассерты с polling-ожиданиями для UI-проверок в TUnit. |
| `tests/DotnetDebug.Tests` | Unit-тесты доменной логики. |
| `tests/DotnetDebug.UiTests.Avalonia.Headless` | Headless UI-тесты для Avalonia (без реального окна ОС). |
| `tests/DotnetDebug.UiTests.FlaUI` | Прямые desktop UI-тесты на FlaUI (более низкоуровневый стиль). |
| `tests/DotnetDebug.UiTests.FlaUI.EasyUse` | Те же desktop-сценарии, но в более удобной архитектуре (`Client + Locators + Controller`). |

## Требования к окружению

- .NET SDK с поддержкой `net10.0` (для части проектов может требоваться preview SDK).
- Windows для desktop UI-тестов на FlaUI (`net10.0-windows7.0`).
- Любая ОС для unit-тестов и headless Avalonia тестов.

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

## Отладка через MCP и ИИ-агентов (VS Code)

Проект уже настроен так, чтобы ИИ-агент мог отлаживать код через MCP-сервер в VS Code:
- запускать под отладкой сами приложения (CLI и Avalonia);
- запускать под отладкой тестовые раннеры;
- останавливать выполнение на breakpoint'ах, смотреть stack/variables, вычислять выражения и идти по шагам (`step into/over/out`) для поиска глубинной причины бага.

### Что уже сделано в этом репозитории

- Подключение MCP-сервера описано в `.vscode/mcp.json` (HTTP endpoint для debug tooling).
- Рекомендованное расширение VS Code указано в `.vscode/extensions.json`:
  - `fellowabhi.killer-bug-ai-debugger`
- Добавлены CoreCLR launch-конфиги в `.vscode/launch.json`:
  - для приложений: `C#: Dotnet Console Debug (MCP CoreCLR)`, `C#: Dotnet Avalonia Debug (MCP CoreCLR)`;
  - для тестов: `C#: Dotnet Unit Tests Debug (MCP CoreCLR)`, `C#: Dotnet UI Tests FlaUI Debug (MCP CoreCLR)`, `C#: Dotnet UI Tests FlaUI.EasyUse Debug (MCP CoreCLR)`.
- Для каждого debug-профиля есть `preLaunchTask`, чтобы проект автоматически собирался перед запуском (`.vscode/tasks.json`).
- Добавлены настройки стабильности отладки в `.vscode/settings.json` (legacy resolution, hot reload off).
- Подробный пошаговый runbook вынесен в `DEBUG_RUNBOOK.md`.

### Как повторить у себя в проекте

1. Установите VS Code расширение для MCP debug-agent (в этом репозитории используется `fellowabhi.killer-bug-ai-debugger`).
2. Добавьте `.vscode/mcp.json` с MCP endpoint вашего debug-сервера (`http://<host>:<port>/mcp`).
3. Добавьте в `.vscode/launch.json` CoreCLR-профили:
   - минимум один профиль для приложения;
   - минимум один профиль для тестов (`program` = путь к test exe/dll, `args` с `--filter-uid`).
4. Добавьте `preLaunchTask` в каждый профиль и соответствующие build tasks в `.vscode/tasks.json`.
5. Для точечного запуска теста под отладкой обновляйте UID в `args`:
   - `dotnet test <project> --list-tests`;
   - получите UID через diagnostics (см. точные команды в `DEBUG_RUNBOOK.md`);
   - подставьте UID в `--filter-uid`.
6. Проверьте MCP health, запустите профиль через агент и ведите расследование через breakpoints + stepping.

### Важно про "любой тест из любого проекта"

ИИ-агент может запускать под отладкой любой конкретный тест, если:
- существует launch-профиль, указывающий на exe/dll нужного test-проекта;
- в аргументах передан корректный `--filter-uid` для выбранного теста.

Практически это означает, что вы масштабируете подход на любое число test-проектов: либо отдельными profile entry, либо одним переиспользуемым шаблоном с заменой `program` и `UID`.

## Тесты

### Unit

```powershell
dotnet test tests/DotnetDebug.Tests/DotnetDebug.Tests.csproj -c Debug
```

### Headless UI (Avalonia)

```powershell
dotnet test tests/DotnetDebug.UiTests.Avalonia.Headless/DotnetDebug.UiTests.Avalonia.Headless.csproj -c Debug
```

### Desktop UI (FlaUI, только Windows)

```powershell
dotnet test tests/DotnetDebug.UiTests.FlaUI/DotnetDebug.UiTests.FlaUI.csproj -c Debug
dotnet test tests/DotnetDebug.UiTests.FlaUI.EasyUse/DotnetDebug.UiTests.FlaUI.EasyUse.csproj -c Debug
```

## Полезные наработки, которые можно утащить к себе

Ниже перечислены самые практичные куски кода, которые уже можно посмотреть в действии в этом репозитории.

### 1) Доменная логика с полной трассировкой вычислений

Где смотреть:
- `src/DotnetDebug.Core/GcdCalculator.cs`
- `tests/DotnetDebug.Tests/GcdCalculatorTests.cs`

Что полезного:
- помимо `Result` возвращается пошаговый трейс (`GcdComputationResult`, `GcdPairComputation`, `GcdDivisionStep`);
- такой формат отлично подходит для UI, логирования и диагностики;
- тесты проверяют и корректность результата, и структуру трассы.

Как переиспользовать:
- оставить "плоский" API для бизнеса (`ComputeGcd`);
- добавить "диагностический" API (`ComputeGcdWithSteps`) для дебага, визуализации и richer telemetry.

### 2) UI, изначально подготовленный к автоматизации

Где смотреть:
- `src/DotnetDebug.Avalonia/MainWindow.axaml`

Что полезного:
- элементы размечены через `AutomationId` (`NumbersInput`, `CalculateButton`, `ResultText`, `StepsList`, `ErrorText`);
- это резко снижает хрупкость UI-тестов и упрощает поддержку.

Как переиспользовать:
- сразу фиксировать стабильные AutomationId на ключевых контролах;
- использовать единый нейминг (`<Screen><Control><Role>` или аналог).

### 3) Безопасный lifecycle-объект для desktop UI-сессии

Где смотреть:
- `src/FlaUI.EasyUse/Session/DesktopAppSession.cs`
- `src/FlaUI.EasyUse/Session/DesktopAppLaunchOptions.cs`

Что полезного:
- централизованный запуск приложения и ожидание главного окна;
- параметризация таймаутов/интервалов;
- корректная очистка ресурсов (best-effort `Close` + `Kill`).

Как переиспользовать:
- вынести запуск/остановку AUT (application under test) в отдельный класс-сессию;
- запретить прямой `Application.Launch(...)` в тестах, оставить один вход через session API.

### 4) Мини-DSL для действий и ожиданий в FlaUI

Где смотреть:
- `src/FlaUI.EasyUse/Extensions/TextBoxExtensions.cs`
- `src/FlaUI.EasyUse/Extensions/ButtonExtensions.cs`
- `src/FlaUI.EasyUse/Extensions/AutomationElementWaitExtensions.cs`

Что полезного:
- человекочитаемые операции (`EnterText`, `ClickButton`);
- wait-паттерны инкапсулированы в extension-методах;
- меньше дублирования `Retry`-логики по тестам.

Как переиспользовать:
- добавить 5-10 extension-методов для самых частых операций;
- в ошибках сразу печатать контекст (`AutomationId`, expected state, timeout).

### 5) Тестовая архитектура `Client + Locators + Controller`

Где смотреть:
- `tests/DotnetDebug.UiTests.FlaUI.EasyUse/Clients/AutomationTestClient.cs`
- `tests/DotnetDebug.UiTests.FlaUI.EasyUse/Locators/MainWindowLocators.cs`
- `tests/DotnetDebug.UiTests.FlaUI.EasyUse/Controllers/MainWindowController.cs`
- `tests/DotnetDebug.UiTests.FlaUI.EasyUse/Tests/UIAutomationTests/MainWindowFlaUI.EasyUseTests.cs`

Что полезного:
- локаторы изолированы от тестовых сценариев;
- тесты читаются как сценарии бизнес-поведения;
- поддержка fallback-поиска элементов (по `AutomationId`, затем по `Name`).

Как переиспользовать:
- разделить уровни:
  - `Locators`: только поиск элементов;
  - `Controller`: действия и ожидания;
  - `Tests`: только сценарии и ассерты.

### 6) Headless UI-тесты как быстрый слой обратной связи

Где смотреть:
- `tests/DotnetDebug.UiTests.Avalonia.Headless/Infrastructure/HeadlessSessionHooks.cs`
- `tests/DotnetDebug.UiTests.Avalonia.Headless/Tests/UIAutomationTests/MainWindowHeadlessTests.cs`

Что полезного:
- единая headless-сессия на `TestSession`;
- UI-сценарии выполняются через `Dispatch`, без зависимостей от оконного окружения;
- быстрые и стабильные проверки логики формы.

Как переиспользовать:
- держать headless-тесты как "быстрый UI smoke";
- оставлять desktop FlaUI-тесты для сквозных, реально-оконных сценариев.

### 7) Ассерты с polling-ожиданием для eventual consistency UI

Где смотреть:
- `src/FlaUI.EasyUse.TUnit/UiAssert.cs`

Что полезного:
- унифицированный retry/polling в одном месте;
- меньше flaky-проверок;
- API заточен под UI (`TextEqualsAsync`, `TextContainsAsync`, `NumberAtLeastAsync`).

Как переиспользовать:
- собрать в общий test-utils пакет;
- использовать один timeout/poll strategy для всей команды.

## Где посмотреть сравнение подходов "вживую"

Если хотите быстро увидеть, что именно улучшает `FlaUI.EasyUse`, сравните:
- `tests/DotnetDebug.UiTests.FlaUI/DesktopAppUiTests.cs` (низкоуровневый подход);
- `tests/DotnetDebug.UiTests.FlaUI.EasyUse/Tests/UIAutomationTests/MainWindowFlaUI.EasyUseTests.cs` (слоистый и читабельный подход).

Это хороший референс, как эволюционировать от "всё в одном тесте" к поддерживаемой архитектуре UI-автотестов.

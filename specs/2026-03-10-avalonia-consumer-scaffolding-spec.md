# AppAutomation Avalonia Consumer Scaffolding

## 0. Метаданные
- Тип (профиль): `dotnet-desktop-client` + `refactor-architecture`
- Владелец: Framework Maintainers
- Масштаб: medium
- Целевая ветка: текущая рабочая ветка
- Ограничения:
  - Не ломать текущий manual-first path: handwritten `[UiControl(...)]` + `AppAutomation.Authoring` generator.
  - Не переносить repo-specific launch/build knowledge внутрь reusable framework packages.
  - Не обещать “полную автогенерацию тестов” или авто-правку `AutomationId` в AUT.
  - Первая итерация должна быть Avalonia-specific и headless-first для scaffold flow.
  - Existing `AppAutomation.AppAutomation.*` reference tests и текущий `NuGet`/`pack` path должны остаться рабочими.
  - Перед завершением реализации должны оставаться зелёными `dotnet build`, `dotnet test`, `dotnet pack`.
- Связанные ссылки:
  - `docs/appautomation/quickstart.md`
  - `docs/appautomation/project-topology.md`
  - `docs/appautomation/advanced-integration.md`
  - `src/AppAutomation.Authoring/UiControlSourceGenerator.cs`
  - `src/AppAutomation.Abstractions/UiLocatorManifestContracts.cs`
  - `src/AppAutomation.Abstractions/UiControlType.cs`
  - `src/AppAutomation.Session.Contracts/HeadlessAppLaunchOptions.cs`
  - `src/AppAutomation.Avalonia.Headless/Automation/HeadlessControlResolver.cs`
  - `src/AppAutomation.AppAutomation.TestHost`
  - `tests/AppAutomation.AppAutomation.Authoring/Pages/MainWindowPage.cs`
  - `specs/2026-03-09-consumer-integration-hardening-spec.md`
  - user-provided desired consumer flow from 2026-03-10
  - `C:\Projects\My\Agents\instructions\core\quest-governance.md`
  - `C:\Projects\My\Agents\instructions\profiles\dotnet-desktop-client.md`
  - `C:\Projects\My\Agents\instructions\profiles\refactor-architecture.md`
  - `C:\Projects\My\Agents\instructions\governance\spec-linter.md`
  - `C:\Projects\My\Agents\instructions\governance\spec-rubric.md`

## 1. Overview / Цель
Довести consumer onboarding flow для существующего Avalonia-приложения до такого состояния, где framework поддерживает не только ручное описание page objects, но и быстрый scaffold path:

1. consumer создаёт `Authoring`, `Headless`, `FlaUI` и optional `TestHost`;
2. consumer подключает `AppAutomation.*` NuGet packages и настраивает launch/bootstrap;
3. в AUT проставляются `AutomationId`;
4. одной командой снимается control inventory из running headless app и генерится scaffold для authoring layer;
5. shared scenarios пишутся в одном общем проекте;
6. один и тот же набор shared tests запускается из `Headless` и `FlaUI` wrappers.

Практический результат:
- самый дорогой ручной этап “выписать все `[UiControl(...)]` руками” сокращается до scaffold + review;
- consumer получает machine-readable inventory и отчёт по missing/duplicate `AutomationId`;
- framework сохраняет текущую architecture split `Authoring` / runtime / `TestHost`;
- test execution story по-прежнему строится на shared scenarios и двух runtime-specific wrappers.

## 2. Текущее состояние (AS-IS)
Фактическое состояние репозитория после предыдущей итерации:

1. `Quickstart` и advanced docs уже покрывают:
   - topology из `Authoring` / `Headless` / `FlaUI` / `TestHost`;
   - repo-specific launch/bootstrap;
   - stateful headless apps;
   - shared scenarios для двух runtime projects.

2. Current authoring flow остаётся manual-first:
   - consumer должен вручную создать `UiPage`;
   - вручную перечислить `[UiControl(...)]`;
   - вручную решить `PropertyName`, `UiControlType`, page boundaries и namespace layout.

3. `AppAutomation.Authoring` уже хорошо делает вторую половину работы:
   - из `[UiControl(...)]` генерирует strongly typed properties;
   - генерирует `UiControlDefinition` set;
   - генерирует manifest provider.

4. Но первой половины pipeline пока нет:
   - нет команды, которая снимает tree snapshot из existing Avalonia app;
   - нет стандартизованного inventory format для consumer scaffold;
   - нет scaffold file generation для partial page definitions;
   - нет отчёта по missing `AutomationId`.

5. Внутренне необходимые building blocks уже partly существуют:
   - `Headless` runtime умеет создавать window из consumer-owned bootstrap;
   - internal automation model умеет перечислять descendants и знает native control types;
   - `UiControlType` и manifest contracts уже задают target vocabulary.

6. Today pain point consumer-а выглядит так:
   - тестовые проекты и launch wiring поднимаются по docs;
   - потом adoption останавливается на tedious/manual mapping UI tree -> `[UiControl(...)]`;
   - это замедляет первую полезную автоматизацию и повышает шанс ошибок в locator naming/type mapping.

## 3. Проблема
Самый медленный и наименее повторяемый этап внедрения `AppAutomation` в существующее Avalonia-приложение сегодня находится между “в приложении уже есть `AutomationId`” и “shared authoring project уже готов к написанию тестов”. Framework умеет использовать описанные controls, но не умеет быстро и стандартизованно получить их из AUT и подготовить authoring scaffold. Из-за этого consumers тратят много ручного времени на однотипную инфраструктурную работу вместо написания сценариев.

## 4. Цели дизайна
- Дать headless-first one-command scaffold path для existing Avalonia app.
- Сохранить `TestHost` как owner repo-specific launch/bootstrap knowledge.
- Генерировать одновременно raw inventory и human-reviewable C# scaffold.
- Помогать consumer-у находить missing/duplicate `AutomationId`, а не только молча игнорировать их.
- Сохранить current `AppAutomation.Authoring` generator contract и не ломать ручной path.
- Зафиксировать end-to-end flow, который естественно приводит к shared tests + dual-runtime execution.

## 5. Non-Goals
- Не добавлять автоматическую расстановку `AutomationId` в XAML/C# AUT.
- Не делать AI-driven semantic page modeling или auto-generated test scenarios.
- Не делать visual recorder / inspector IDE plugin.
- Не делать в этой итерации полноценный второй scanner backend поверх `FlaUI`; desktop runtime остаётся execution target для shared tests, а scaffold flow в v1 строится через `Headless`.
- Не убирать manual authoring path и не заставлять всех consumers использовать scaffold tool.
- Не пытаться полностью автоматически решать page boundaries или бизнес-смысл контролов без review.

## 6. Предлагаемое решение (TO-BE)

### 6.1 Целевой consumer flow
Framework formally supports следующий path:

1. Consumer создаёт проекты по existing topology docs.
2. Consumer подключает `AppAutomation.*` packages и делает repo-specific `TestHost`.
3. Consumer вручную или с помощью внешних средств добавляет в AUT стабильные `AutomationId`.
4. Consumer запускает scaffold command.
5. Command:
   - поднимает AUT через consumer-owned headless bootstrap;
   - снимает control inventory;
   - формирует JSON inventory;
   - формирует Markdown/text report;
   - формирует `*.scaffold.cs` partial page file с `[UiControl(...)]`.
6. Consumer ревьюит scaffold, правит имена/границы page objects при необходимости и добавляет shared tests.
7. Shared tests выполняются из `Headless` и `FlaUI` runtime wrappers.

Норматив:
- scaffold command ускоряет authoring, но не заменяет инженерный review;
- source of truth для финальных тестов остаётся authoring project;
- existing generator продолжает работать поверх scaffolded или handwritten partial pages одинаково.

### 6.2 Новый tooling layer
Добавляется новый tooling entrypoint семейства `AppAutomation.*`.

Предпочтительная форма первой итерации:
- новый проект `src/AppAutomation.Tooling`;
- CLI/console entrypoint, пригодный для `dotnet run --project ...` в repo и подготовленный к `PackAsTool` path;
- primary command:

```text
appautomation scaffold avalonia --host <path-to-TestHost.dll> --target <PageName> --output <directory>
```

Допустимые опции v1:
- `--host <path>`: путь до consumer `TestHost` assembly;
- `--target <PageName>`: один target для scaffold;
- `--all`: scaffold всех target-ов, объявленных host-ом;
- `--output <directory>`: корень для generated artifacts;
- `--namespace <value>`: override namespace для scaffold file;
- `--report-only`: только inventory/report без `*.scaffold.cs`.

Норматив:
- command должен быть детерминированным и headless-friendly;
- output files должны быть регенерируемыми;
- command не должен искать `.sln`, билдить AUT или угадывать repo layout без участия consumer `TestHost`.

### 6.3 Consumer-owned scaffold host contract
Чтобы не уносить repo-specific knowledge в framework, вводится explicit consumer contract для scaffold bootstrap.

Предпочтительный target shape в `AppAutomation.Session.Contracts`:
- `IAvaloniaAppAutomationScaffoldHost`
- `UiScaffoldTarget`

Минимальная форма:

```csharp
public interface IAvaloniaAppAutomationScaffoldHost
{
    HeadlessAppLaunchOptions CreateHeadlessLaunchOptions();
    IReadOnlyList<UiScaffoldTarget> GetTargets();
}

public sealed record UiScaffoldTarget(
    string PageName,
    string Namespace,
    string? RootAutomationId = null);
```

Норматив:
- host реализуется в consumer `TestHost` assembly;
- tool reflection-load-ит host implementation из указанной сборки;
- one host may expose multiple page targets;
- `RootAutomationId` ограничивает scope inventory/scaffold subtree; если он не задан, target scope = root window.

Это держит launch/bootstrap policy у consumer-а и не создаёт в reusable package knowledge про конкретный repo.

### 6.4 Inventory model
Tool генерирует machine-readable inventory artifact для review и возможной последующей автоматизации.

Предпочтительный shape:
- `UiScaffoldInventory`
- `UiScaffoldNode`

Минимальные поля node:
- `AutomationId`
- `Name`
- `SuggestedPropertyName`
- `SuggestedControlType` (`UiControlType`)
- `NativeControlType`
- `Path`
- `IncludeInScaffold`
- `ExclusionReason`
- `Children`

Artifact path convention:

```text
artifacts/appautomation/scaffold/<PageName>.inventory.json
```

Норматив:
- nodes без `AutomationId` должны попадать в report, но не в `[UiControl(...)]` scaffold by default;
- дубликаты `AutomationId` должны явно помечаться;
- если runtime type не удаётся уверенно смэппить, tool использует `UiControlType.AutomationElement` и помечает это в report.

### 6.5 Scaffolded C# output
Tool генерирует отдельный partial file для authoring layer, а не переписывает hand-authored page class.

Artifact path convention:

```text
<output>/Pages/<PageName>.scaffold.cs
```

Target shape:

```csharp
using AppAutomation.Abstractions;

namespace MyApp.UiTests.Authoring.Pages;

[UiControl("LoginButton", UiControlType.Button, "LoginButton")]
[UiControl("MainTabs", UiControlType.Tab, "MainTabs")]
public sealed partial class MainWindowPage
{
}
```

Норматив:
- scaffold file содержит только attribute declarations и partial type declaration;
- constructor/runtime logic остаются в hand-authored companion file;
- property names derive from sanitized `AutomationId` with deterministic duplicate handling;
- manual edits в `*.scaffold.cs` не предполагаются; файл считается regenerable artifact.

### 6.6 Missing/duplicate AutomationId report
Tool параллельно генерирует human-readable report:

```text
artifacts/appautomation/scaffold/<PageName>.report.md
```

Report должен содержать:
- controls included in scaffold;
- controls skipped because `AutomationId` missing;
- duplicate `AutomationId`;
- nodes, для которых пришлось упасть в `UiControlType.AutomationElement`;
- short remediation guidance.

Это делает шаг “проставить `AutomationId`” измеримым и проверяемым, а не purely manual guesswork.

### 6.7 Ответственность компонентов (до / после)
Текущее состояние:

| Компонент | Ответственность today |
| --- | --- |
| `AppAutomation.Authoring` | Генерация accessors/manifest из уже написанных `[UiControl(...)]` |
| Consumer `TestHost` | Launch/bootstrap для AUT |
| Consumer authoring project | Полностью ручное описание page object controls |

Целевое состояние:

| Компонент | Ответственность after change |
| --- | --- |
| `AppAutomation.Tooling` | Headless scan, inventory/report/scaffold generation |
| `AppAutomation.Session.Contracts` | Scaffold host contract + target metadata |
| `AppAutomation.Authoring` | По-прежнему генерация accessors/manifest из scaffolded or handwritten `[UiControl(...)]` |
| Consumer `TestHost` | Реализация scaffold host + repo-specific launch/bootstrap |
| Consumer authoring project | Review scaffold + shared tests + hand-authored page companion files |

### 6.8 Documentation and reference example
Documentation обновляется так, чтобы end-to-end flow был виден без чтения кода:

1. `quickstart.md` получает раздел “Scaffold existing Avalonia UI”.
2. `advanced-integration.md` получает раздел про scaffold host и review workflow.
3. `README.md` получает ссылку на scaffold flow.
4. `AppAutomation.AppAutomation.TestHost` получает reference implementation scaffold host.
5. `AppAutomation` используется как validation target для scaffold command.

## 7. Compatibility, Rollout, Rollback

### 7.1 Совместимость
- Existing consumers, которые продолжают вручную писать `[UiControl(...)]`, не ломаются.
- Existing `AppAutomation.Authoring` source generation contract сохраняется.
- Existing headless/FlaUI shared-test execution path сохраняется.
- New scaffold host contract и tooling layer additive и optional.

### 7.2 Rollout
1. Ввести scaffold host contract и inventory model.
2. Реализовать headless inventory export и CLI scaffold command.
3. Добавить reference implementation в `AppAutomation.AppAutomation.TestHost`.
4. Обновить docs под полный consumer flow.
5. Добавить tests и reference validation command.

### 7.3 Rollback
- Если tool ergonomics окажется плохой, rollback ограничивается removal tooling layer без отката authoring/runtime APIs.
- Если inventory contract окажется избыточным, rollback ограничивается tooling-specific models с сохранением manual authoring path.
- Если scaffolded C# output не оправдает себя, можно откатиться к report-only inventory без затрагивания runtime test execution.

## 8. Acceptance Criteria
1. В repo появляется `AppAutomation` scaffold command для Avalonia headless flow.
2. Command может использовать consumer-owned `TestHost` assembly вместо repo layout guessing.
3. Command генерирует `inventory.json` и `report.md` для target page/window.
4. Command генерирует `*.scaffold.cs` partial file с `[UiControl(...)]` для controls с валидным `AutomationId`.
5. Controls без `AutomationId` и дубликаты не теряются, а явно отражаются в report.
6. Generated scaffold успешно потребляется существующим `AppAutomation.Authoring` generator.
7. Docs описывают полный flow:
   - проекты;
   - NuGet packages;
   - `AutomationId`;
   - scaffold command;
   - shared tests;
   - dual-runtime execution.
8. Reference consumer flow на `AppAutomation` демонстрирует, что после scaffold/review shared tests продолжают запускаться из `Headless` и `FlaUI`.
9. Полные проверки зелёные:
   - `dotnet build AppAutomation.sln`
   - `dotnet test --solution AppAutomation.sln`
   - `dotnet pack AppAutomation.sln`

## 9. Проверки и команды
Точечные проверки:

```powershell
dotnet test tests/AppAutomation.Authoring.Tests/AppAutomation.Authoring.Tests.csproj -c Release
dotnet test tests/AppAutomation.AppAutomation.Avalonia.Headless.Tests/AppAutomation.AppAutomation.Avalonia.Headless.Tests.csproj -c Release
dotnet test tests/AppAutomation.AppAutomation.FlaUI.Tests/AppAutomation.AppAutomation.FlaUI.Tests.csproj -c Release
```

Tooling validation:

```powershell
dotnet build AppAutomation.sln -c Release
dotnet run --project src/AppAutomation.Tooling/AppAutomation.Tooling.csproj -- scaffold avalonia `
  --host src/AppAutomation.AppAutomation.TestHost/bin/Release/net10.0/AppAutomation.AppAutomation.TestHost.dll `
  --target MainWindowPage `
  --output artifacts/appautomation/scaffold-demo
```

Структурные проверки:

```powershell
rg -n "IAvaloniaAppAutomationScaffoldHost|UiScaffoldTarget|scaffold" src tests docs -g "*.cs" -g "*.md"
rg -n "scaffold|AutomationId|UiControl" docs -g "*.md"
```

Полный прогон:

```powershell
dotnet build AppAutomation.sln -c Release
dotnet test --solution AppAutomation.sln -c Release
dotnet pack AppAutomation.sln -c Release
```

## 10. Открытые вопросы
- Стоит ли inventory model жить в `AppAutomation.Abstractions` или лучше держать его внутри tooling assembly до появления второго consumer-а этих контрактов. Предварительно: начать с `AppAutomation.Abstractions`, потому что vocabulary уже там.
- Нужен ли сразу `PackAsTool` + publish automation для CLI, или достаточно сначала repo-local `dotnet run --project` path с последующим выносом в отдельную release story. Предварительно: подготовить tool к packaging, но не делать отдельную release automation blocking requirement для этой спеки.
- Нужно ли поддерживать несколько root target-ов на один page file или только один subtree на один `UiScaffoldTarget`. Предварительно: один target = один root scope = один page file.

## 11. Результат прогона линтера
### 11.1 SPEC Linter Result

| Блок | Пункты | Статус | Комментарий |
| --- | --- | --- | --- |
| A. Полнота спеки | 1-5 | PASS | Цель, AS-IS, проблема, цели и non-goals зафиксированы |
| B. Качество дизайна | 6-10 | PASS | Tooling layer, host contract, inventory/scaffold outputs и docs responsibilities описаны конкретно |
| C. Безопасность изменений | 11-13 | PASS | Manual path сохраняется, rollout additive, rollback локализован |
| D. Проверяемость | 14-16 | PASS | Acceptance criteria и validation commands измеримы |
| E. Готовность к автономной реализации | 17-19 | PASS | Scope ограничен Avalonia headless scaffold, открытые вопросы неблокирующие |
| F. Соответствие профилю | 20 | PASS | Спека соответствует `dotnet-desktop-client` + `refactor-architecture` |

Итог: `ГОТОВО`

### 11.2 SPEC Rubric Result

| Критерий | Балл (0/2/5) | Обоснование |
| --- | --- | --- |
| 1. Ясность цели и границ | 5 | Инициатива сфокусирована на одном пробеле: scaffold flow между `AutomationId` и authoring layer |
| 2. Понимание текущего состояния | 5 | AS-IS опирается на текущие docs, generator и runtime capabilities |
| 3. Конкретность целевого дизайна | 5 | Зафиксированы command surface, host contract, inventory/report/scaffold outputs |
| 4. Безопасность (миграция, откат) | 5 | Existing manual path и test execution path сохраняются без forced migration |
| 5. Тестируемость | 5 | Есть acceptance criteria, команды и reference validation target |
| 6. Готовность к автономной реализации | 5 | Scope ограничен Avalonia headless-first tooling и не расползается в AI/full recorder story |

Итоговый балл: `30 / 30`
Зона: `готово к автономному выполнению`

Слабые места:
- quality scaffold output сильно зависит от качества consumer `AutomationId` strategy;
- inventory model легко сделать слишком общим, поэтому важно удержать scope на Avalonia scaffold v1;
- packaging tool как отдельного release artifact может потребовать дополнительную release-story после базовой реализации.

## 12. Approval
Ожидается фраза: **"Спеку подтверждаю"**

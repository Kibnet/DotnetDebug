# Industrialization `EasyUse` framework: архитектура, DX, диагностика и release discipline

## 0. Метаданные
- Тип (профиль): `dotnet-desktop-client` + `refactor-architecture`
- Владелец: QA Automation / Platform Engineering / Framework Maintainers
- Масштаб: large
- Целевой релиз / ветка: текущая рабочая ветка, с последующим выделением подэтапов в отдельные PR
- Ограничения:
  - Не менять пользовательское поведение demo-приложения `DotnetDebug.Avalonia`.
  - Не ломать текущий DSL сценариев без явной карты миграции и major-version bump.
  - Не расширять матрицу поддерживаемых контролов сверх текущего showcase, кроме случаев, когда это требуется для выпрямления существующего API.
  - Не менять `automation-id`/селекторы и не ухудшать стабильность текущих UI-тестов.
  - Сохранить работоспособность обоих runtime: `FlaUI` и `Avalonia.Headless`.
- Связанные ссылки:
  - `README.md`
  - `ControlSupportMatrix.md`
  - `specs/2026-03-05-ui-tests-shared-refactor-spec.md`
  - `specs/2026-03-05-phase2-core-contracts-spec.md`
  - `C:\Projects\My\Agents\instructions\core\quest-governance.md`
  - `C:\Projects\My\Agents\instructions\profiles\dotnet-desktop-client.md`
  - `C:\Projects\My\Agents\instructions\profiles\refactor-architecture.md`
  - `C:\Projects\My\Agents\instructions\contexts\testing-dotnet.md`
  - `C:\Projects\My\Agents\instructions\governance\spec-linter.md`
  - `C:\Projects\My\Agents\instructions\governance\spec-rubric.md`

## 1. Overview / Цель
Довести текущий набор библиотек `EasyUse` до промышленного уровня по четырём направлениям:
- чёткие архитектурные границы между reusable API и repo-specific harness;
- предсказуемый и чистый public API без namespace ambiguity и скрытого дублирования;
- операционная зрелость: диагностика падений, packaging, CI, versioning, migration;
- тестовая зрелость: contract tests, parity checks, regression coverage по runtime и generator-поведению.

Результат: framework остаётся удобным для автора UI-тестов, но при этом становится пригодным для долгосрочного сопровождения, публикации пакетов и безопасной эволюции API.

## 2. Текущее состояние (AS-IS)
- В решении уже есть полезная декомпозиция:
  - `src/EasyUse.Session.Contracts`
  - `src/EasyUse.TUnit.Core`
  - `src/FlaUI.EasyUse`
  - `src/Avalonia.Headless.EasyUse`
  - `src/FlaUI.EasyUse.Generators`
  - `src/Avalonia.Headless.EasyUse.Generators`
- DX для автора тестов уже сильный:
  - source-generated page object;
  - fluent DSL действий и ожиданий;
  - shared UI-сценарии для двух runtime;
  - стабильные `AutomationId` в demo UI.
- Но текущая repo-topology остаётся transitional:
  - shared page/scenario source не оформлен как отдельная сборка-владелец и подключается через `Compile Include` сразу в оба runtime test project;
  - generated page API привязан к adapter-specific concrete types, а не к adapter-neutral descriptor contract.
- Текущие проверки подтверждают базовую жизнеспособность:
  - solution build проходит;
  - все текущие unit/headless/FlaUI тесты проходят;
  - parity shared-сценариев между headless и FlaUI подтверждён отдельным скриптом.

Ключевые архитектурные проблемы текущего состояния:
1. Public runtime API всё ещё содержит repo-specific bootstrap:
- поиск solution root;
- прямой вызов `dotnet build`;
- вычисление путей в `bin/<config>/<tfm>`;
- reflection-запуск `MainWindow` для headless-mode.

2. Границы package/namespace ещё не выпрямлены:
- headless runtime экспортирует типы и extensions в пространствах `FlaUI.*`;
- consumer видит неочевидную картину «какой пакет является source of truth для какого namespace».

3. Базовые примитивы и часть glue logic дублируются:
- `UiWait`/`UiWaitOptions` существуют в нескольких сборках;
- lifecycle/build bootstrap повторяется между runtime;
- generator governance выровнена не во всех generator-проектах.

4. Capability contract не выражен явно:
- часть API формально объявлена, но на одном из runtime не поддерживается полностью;
- runtime compatibility приходится понимать по исходникам и manual matrix.

5. Release engineering остаётся учебно-демонстрационной:
- смешанные target frameworks (`net8`, `net9`, `net10`);
- SDK version не pinned в `global.json`;
- package metadata и SourceLink discipline неполные;
- не все generator-проекты проходят без analyzer governance warnings.

6. Failure diagnostics ещё слабые:
- timeout-ошибки малоинформативны;
- нет стандартизованного snapshot/tree dump/log artifact bundle;
- нет единого failure contract для runtime/test base.

## 3. Проблема
Framework уже удобен для локальной разработки и demo-репозитория, но ещё не имеет строгих архитектурных, контрактных и release-границ, необходимых для промышленного использования и безопасной публикации как reusable продукта.

## 4. Цели дизайна
- Чётко разделить reusable framework, adapter-specific runtime и repo-specific test harness.
- Установить один канонический public API на пакет без перекрёстных namespace ambiguity.
- Явно зафиксировать capability model для runtime-адаптеров.
- Сделать падения тестов и runtime-операций диагностируемыми без ручного дебага.
- Поднять packaging/CI/versioning до уровня publishable framework.
- Сохранить текущую ergonomics сценариев и успешность текущего regression-набора.

## 5. Non-Goals (чего НЕ делаем)
- Не превращаем demo-приложение в production product.
- Не переписываем весь DSL с нуля.
- Не добавляем web/mobile/runtime beyond desktop-headless в рамках этой инициативы.
- Не меняем доменную/визуальную бизнес-логику `DotnetDebug.Avalonia`.
- Не решаем все будущие расширения control support за одну фазу.
- Не оставляем долгоживущую двойную поддержку двух параллельных namespace-моделей внутри core-пакетов.

## 6. Предлагаемое решение (TO-BE)
### 6.1 Целевая схема модулей
- `src/EasyUse.Automation.Abstractions`
  - Canonical adapter-neutral automation surface.
  - `UiPage`, `UiControlAttribute`, `UiControlType`, `UiLocatorKind`.
  - Typed control contracts / automation element contracts.
  - `UiWait`, `UiWaitOptions`, `UiWaitResult`.
  - Shared diagnostics and capability contracts for automation operations.
  - Descriptor contracts for blackbox runners:
    - `UiLocatorManifest`
    - `UiPageDefinition`
    - `UiControlDefinition`
    - `IUiLocatorManifestProvider`
- `src/EasyUse.Automation.Authoring`
  - Packable compile-time authoring package.
  - Source generators + analyzer diagnostics для page/scenario authoring.
  - Явная зависимость проектов, которые объявляют page objects и shared scenarios.
  - Генерация abstraction-based page accessors и generated C# implementation of `UiLocatorManifest` для blackbox runners.
  - Канонический output v1 это generated C# contract в authoring assembly, а не JSON/resource/external artifact.
  - Генерация provider/registry entry point, реализующего `IUiLocatorManifestProvider`, для типобезопасного доступа blackbox runner-ов к manifest.
- `src/EasyUse.Session.Contracts`
  - Только launch/session contracts.
  - Контракты prepared artifact launch и runtime session lifecycle.
- `src/EasyUse.TUnit.Core`
  - Runtime-agnostic test base, polling asserts, artifact integration hooks.
  - Не владеет automation surface и waiting primitives.
- `src/FlaUI.EasyUse`
  - Только FlaUI adapter/runtime logic.
  - Реализация canonical automation abstractions поверх FlaUI.
- `src/Avalonia.Headless.EasyUse`
  - Только headless adapter/runtime logic в собственных canonical namespaces.
  - Whitebox / in-process runtime по design contract.
  - Реализация canonical automation abstractions поверх Avalonia Headless.
- `src/EasyUse.TestHost` (новый repo-oriented слой)
  - `IsPackable = false`.
  - Внутренний repo-only helper, не являющийся public framework contract.
  - Поиск solution root;
  - build/resolve project output;
  - wiring demo/локального AUT к public launch contracts.
- `tests/*`
  - `tests/DotnetDebug.UiTests.Authoring` (новый repo-only SDK-style class library project)
    - единственный владелец shared page objects, shared scenario base classes и generated locator manifest;
    - явные зависимости на `EasyUse.Automation.Abstractions`, `EasyUse.Automation.Authoring`, `EasyUse.TUnit.Core` и test authoring packages;
    - `IsPackable = false`.
  - runtime test projects больше не включают shared source через `Compile Include`;
  - они ссылаются на `DotnetDebug.UiTests.Authoring` и содержат только runtime-specific bootstrap/session/wrapper logic;
  - Contract/regression/integration tests, отдельные от demo-specific AUT tests там, где это даёт ценность.

### 6.2 Принципиальные архитектурные правила
1. Public reusable package не знает про `.sln`, `dotnet build` и локальную структуру `bin/`.
2. `LaunchFromProject` не является public package contract; это только repo-only удобство внутри `EasyUse.TestHost`.
3. Один package = один canonical namespace family.
4. Shared page/scenario code не зависит напрямую от `FlaUI.*` или `Avalonia.Headless.*`; он зависит только от `EasyUse.Automation.Abstractions`.
5. Один тип ответственности = одна canonical implementation.
6. Unsupported capability не маскируется общим API без явного capability contract.
7. Любой runtime failure должен уметь отдать диагностический контекст по фиксированной minimal artifact matrix.
8. Compile-time authoring tooling подключается явно через `EasyUse.Automation.Authoring`; runtime packages не тащат generators транзитивно как public contract.
9. Generated page object surface возвращает только abstraction-layer types и locator metadata; adapter-specific concrete types не являются частью generated public API.
10. `UiLocatorManifest` v1 является generated C# contract, собранным в authoring assembly; embedded/external manifest formats не являются canonical contract на этом этапе.
11. В репозитории используется отдельный shared authoring project вместо `Compile Include` shared source в runtime test projects.
12. Blackbox runner получает manifest через canonical provider contract (`IUiLocatorManifestProvider`), а не через ad-hoc reflection по generated types.

### 6.3 Phased implementation plan
#### Phase 1. API boundaries and hosting split
- Добавить новый repo-specific слой `EasyUse.TestHost`.
- Зафиксировать `EasyUse.TestHost` как `IsPackable = false` и internal-only dependency для этого репозитория.
- Вынести из runtime-пакетов:
  - поиск solution root;
  - build-on-launch;
  - project-path bootstrap;
  - assembly reflection как public launch story.
- Зафиксировать public launch contracts:
  - `FlaUI.EasyUse`: только `Launch(DesktopAppLaunchOptions)` по уже подготовленному executable path;
  - `Avalonia.Headless.EasyUse`: только `Launch(HeadlessAppLaunchOptions)` по явной app/window factory, без `Assembly.LoadFrom` как public contract.
- Оставить в runtime packages только:
  - adapter launch по prepared artifact/runtime context;
  - runtime session lifecycle;
  - automation/page/control logic.
- Явно зафиксировать позиционирование headless runtime:
  - `Avalonia.Headless.EasyUse` является whitebox/in-process adapter;
  - blackbox launch story для headless runtime не проектируется как public contract этой инициативы.
- `EasyUse.TestHost` получает право:
  - резолвить project output;
  - строить `DesktopAppLaunchOptions`/`HeadlessAppLaunchOptions`;
  - связывать demo AUT с public launch contracts.
- В `EasyUse.Session.Contracts` фиксируются только launch/session contracts, без automation surface и waiting primitives.

#### Phase 2. Adapter-neutral automation surface and namespace normalization
- Добавить новый проект `EasyUse.Automation.Abstractions`.
- Добавить новый проект `EasyUse.Automation.Authoring`.
- Добавить новый repo-only проект `tests/DotnetDebug.UiTests.Authoring` и заменить текущую `Compile Include`-модель shared test source на project reference.
- Перенести туда canonical surface:
  - typed automation contracts;
  - page object contracts and attributes;
  - waiting primitives;
  - common automation diagnostics contracts.
- Shared page objects, generators и scenario DSL переводятся на `EasyUse.Automation.Abstractions` + `EasyUse.Automation.Authoring`.
- Сконсолидировать source generators из runtime-specific пакетов в `EasyUse.Automation.Authoring`.
- Зафиксировать authoring contract:
  - проекты, которые объявляют page objects/shared scenarios, явно подключают `EasyUse.Automation.Authoring`;
  - runtime packages не являются способом доставки generators;
  - generated accessors возвращают abstraction-layer control/page contracts, а не adapter-specific concrete types;
  - generator дополнительно эмитит generated C# implementation of `UiLocatorManifest`, пригодную для blackbox runners (`FlaUI` и будущие аналоги);
  - canonical manifest contract lives in `EasyUse.Automation.Abstractions`;
  - canonical manifest delivery is assembly-local generated C# code in `DotnetDebug.UiTests.Authoring`, not JSON/resource/external file.
  - generator эмитит canonical provider/registry entry point, реализующий `IUiLocatorManifestProvider`.
- `Avalonia.Headless.EasyUse` получает только собственные canonical namespaces:
  - `Avalonia.Headless.EasyUse.AutomationElements`
  - `Avalonia.Headless.EasyUse.PageObjects`
  - `Avalonia.Headless.EasyUse.Extensions`
- Убирается долговременный экспорт headless runtime в `FlaUI.*` namespaces.
- Generator strategy:
  - source generators работают против abstraction-layer и публикуются через `EasyUse.Automation.Authoring`;
  - runtime-specific generator packages не остаются долгоживущим public contract;
  - dual-flavor migration допускается только как краткоживущий internal transition, но не как долгосрочный public contract.
- Миграция:
  - shared page/scenario code внутри репозитория напрямую переводится на abstraction-layer;
  - для publishable packages подготавливаются migration guide, API migration map и major-version bump.

#### Phase 3. Capability model and diagnostics
- Ввести hybrid capability model:
  - compile-time: специализированные интерфейсы/контракты для non-universal операций;
  - runtime: обязательный immutable descriptor `UiRuntimeCapabilities`.
- Убрать ситуацию, когда API формально присутствует, но фактически сразу падает без предсказуемого контракта.
- Зафиксировать authoring model для capability-specific API:
  - generated page object members возвращают базовые abstraction contracts для universal operations;
  - capability-specific операции доступны через explicit capability interfaces / adapters поверх abstraction-layer;
  - shared scenarios используют только universal contract либо явно проверяют `UiRuntimeCapabilities` перед capability-specific branch.
- Ввести стандартизованную failure model:
  - `UiOperationException`
  - `UiFailureArtifact`
  - `UiFailureContext`
  - `IUiArtifactCollector`
- Зафиксировать mandatory artifact matrix:
  - для всех runtime: operation name, selector/property context, timeout, adapter id, exception, timestamp, logical/tree dump, last observed values;
  - для `FlaUI`: screenshot, process info, window handle, если доступны;
  - для `Avalonia.Headless`: logical tree snapshot и serialized control state, screenshot не требуется.

#### Phase 4. Release engineering and quality gates
- Ввести единые репозиторные build governance файлы:
  - `global.json` с pinned stable SDK;
  - `Directory.Build.props`;
  - `Directory.Packages.props`;
  - `.editorconfig` для code style/analyzer policy.
  - `eng/Versions.props` как single source of truth для lockstep-versioning.
- Зафиксировать repo support policy:
  - preview SDK запрещён в CI;
  - базовый SDK для репозитория: stable `.NET 10.x`;
  - public package TFM policy документируется явно, а любое отклонение от неё требует обоснования.
- Зафиксировать versioning policy пакетов семейства:
  - все publishable packages `EasyUse` versioned lockstep;
  - поддерживаемой считается только same-version combination пакетов семейства;
  - generators/authoring package и runtime packages публикуются синхронно одним release train.
  - все packable `csproj` импортируют версию из `eng/Versions.props`, а не задают её вручную по отдельности.
- Зафиксировать package TFM policy:
  - runtime-neutral библиотеки (`EasyUse.Automation.Abstractions`, `EasyUse.Session.Contracts`, `EasyUse.TUnit.Core`) по умолчанию таргетят `net8.0`;
  - `EasyUse.Automation.Authoring` таргетит analyzer-compatible `netstandard2.0`;
  - desktop/runtime-specific пакеты могут multi-target `net8.0-windows7.0` и `net10.0-windows7.0`, если это реально нужно для зависимостей или API;
  - `net10.0`-only допускается только с явным техническим обоснованием в changelog/spec.
- Для всех publishable packages добавить:
  - корректный `RepositoryUrl`;
  - `PackageReadmeFile`;
  - `PackageLicenseExpression` или `PackageLicenseFile`;
  - `Description`, `Authors`, tags;
  - SourceLink/symbol/package metadata.
- Для generator-проектов:
  - включить `EnforceExtendedAnalyzerRules`;
  - завести `AnalyzerReleases.Shipped.md`/`AnalyzerReleases.Unshipped.md`;
  - добиться build без RS1036/RS2008 warning.
- Добавить CI matrix:
  - build + unit/headless на любой поддерживаемой ОС;
  - FlaUI runs на Windows;
  - pack smoke;
  - API compatibility checks;
  - parity checks.

#### Phase 5. Test strategy hardening
- Сохранить текущий regression suite как baseline.
- Добавить недостающие типы проверок:
  - abstraction-layer contract tests для shared page/scenario compile compatibility;
  - authoring-project topology tests:
    - `DotnetDebug.UiTests.Authoring` собирается как единый owner shared source;
    - runtime test projects не используют `Compile Include` для shared source;
    - inherited test discovery и parity сохраняются после перехода на project reference topology;
  - contract tests для capability model;
  - generator diagnostics tests;
  - blackbox manifest contract/smoke tests:
    - `FlaUI` является первым обязательным consumer generated `UiLocatorManifest`;
    - smoke coverage проверяет lookup по `AutomationId`, fallback по `Name`, минимум один composite control и failure-path diagnostics;
  - failure-diagnostics tests;
  - package smoke tests (`dotnet pack`, restore/use from temp consumer);
  - compatibility tests на migration map.
- Для shared scenario parity сделать проверку обязательным CI gate.

### 6.4 Target dependency graph
#### До
- `tests` -> runtime packages + shared sources
- runtime packages -> contracts + собственный bootstrap/build logic
- headless package -> частично `FlaUI.*` namespace surface
- waiting/diagnostics/governance частично размазаны по нескольким пакетам

#### После
- `tests/DotnetDebug.UiTests.Authoring` -> `EasyUse.Automation.Abstractions` + `EasyUse.Automation.Authoring` + `EasyUse.TUnit.Core`
- runtime test projects -> `tests/DotnetDebug.UiTests.Authoring` + runtime adapters + repo `EasyUse.TestHost`
- `EasyUse.TUnit.Core` -> `EasyUse.Automation.Abstractions` + `EasyUse.Session.Contracts`
- runtime adapters -> `EasyUse.Automation.Abstractions` + `EasyUse.Session.Contracts`
- repo bootstrap/build helpers -> только `EasyUse.TestHost`
- каждый runtime-пакет экспортирует только свой canonical namespace
- shared page/scenario code компилируется против abstraction-layer, а не против adapter-specific namespaces
- blackbox runners используют generated C# `UiLocatorManifest`, а не runtime-specific generator surface
- blackbox runners получают manifest через `IUiLocatorManifestProvider`
- diagnostics/capabilities описаны контрактно, а не имплицитно

### 6.5 Component responsibility table
| Компонент | Ответственность | Не должен делать |
| --- | --- | --- |
| `EasyUse.Automation.Abstractions` | canonical automation/page/waiting surface и descriptor/provider contracts для blackbox consumption | знать про launch/build/session bootstrap |
| `EasyUse.Automation.Authoring` | compile-time generators, analyzer diagnostics, generated C# `UiLocatorManifest` emission, provider generation | становиться неявной transitive runtime dependency |
| `EasyUse.Session.Contracts` | launch/session contracts | знать про локальный solution layout и automation DSL |
| `EasyUse.TUnit.Core` | test lifecycle, polling assertions, artifact hooks | владеть canonical automation contracts |
| `FlaUI.EasyUse` | real desktop adapter | билдить project path и искать `.sln` |
| `Avalonia.Headless.EasyUse` | whitebox/in-process headless adapter | экспортировать `FlaUI.*` public API, reflection-based public launch contract или позиционироваться как blackbox runner |
| `EasyUse.TestHost` | repo-specific bootstrap and local AUT hosting | становиться обязательной зависимостью внешних consumers |
| `tests/DotnetDebug.UiTests.Authoring` | единый repo-owner shared pages/scenarios/manifest | дублироваться через `Compile Include` в runtime test projects |

## 7. Бизнес-правила / Алгоритмы (если есть)
- Инвариант 1: текущие shared UI-сценарии продолжают исполняться на обоих runtime через abstraction-layer.
- Инвариант 2: runtime package не содержит repo-specific build/solution discovery logic.
- Инвариант 3: `LaunchFromProject` не входит в publishable framework surface.
- Инвариант 4: один canonical namespace family на один package.
- Инвариант 5: capability support выражается hybrid contract model, а не knowledge-by-source.
- Инвариант 6: `UiWait` и waiting primitives существуют только в `EasyUse.Automation.Abstractions`.
- Инвариант 7: каждое неуспешное ожидание/операция оставляет диагностический след, пригодный для QA-разбора, по mandatory artifact matrix.
- Инвариант 8: industrialization не должна ухудшить текущий DSL usage pattern в тестах без formal migration map.
- Инвариант 9: `Avalonia.Headless.EasyUse` является whitebox runtime и не претендует на blackbox launch model.
- Инвариант 10: generated page object surface опирается только на abstraction-layer types; blackbox runners потребляют generated C# `UiLocatorManifest`.
- Инвариант 11: authoring/generator tooling подключается явно, а не через transitive runtime dependency.
- Инвариант 12: все publishable packages семейства `EasyUse` versioned lockstep.
- Инвариант 13: canonical owner shared test authoring в репозитории это `tests/DotnetDebug.UiTests.Authoring`, а не `Compile Include` в runtime projects.
- Инвариант 14: blackbox runner потребляет manifest через `IUiLocatorManifestProvider`, а не через reflection-scanning generated classes.

## 8. Точки интеграции и триггеры
- Запуск AUT:
  - repo/demo path через `EasyUse.TestHost`;
  - reusable runtime API через prepared artifact launch (`FlaUI`) или explicit factory launch (`Headless`, whitebox/in-process).
- Test lifecycle:
  - `EasyUse.TUnit.Core.UiTestBase`
  - runtime-specific hooks/session factories
- Generator integration:
  - compile-time generation page object accessors и generated C# `UiLocatorManifest` поверх `EasyUse.Automation.Abstractions` через явный `EasyUse.Automation.Authoring`
- Blackbox runner integration:
  - `FlaUI` как первый mandatory consumer generated `UiLocatorManifest` через `IUiLocatorManifestProvider` на contract/smoke уровне
- Packaging:
  - `dotnet pack`
  - CI publish/pack smoke

## 9. Изменения модели данных / состояния
- Persisted business data: нет.
- Runtime/test state:
  - добавляются artifact bundles и diagnostic context;
  - добавляется capability metadata;
  - меняется только framework-infrastructure state, не доменная модель AUT.

## 10. Миграция / Rollout / Rollback
### 10.1 Rollout
1. Phase 1: вынести repo-specific hosting/build bootstrap в `EasyUse.TestHost`.
2. Phase 2: ввести `EasyUse.Automation.Abstractions` и `EasyUse.Automation.Authoring`, создать `tests/DotnetDebug.UiTests.Authoring`, перевести shared page/scenario/generator surface на project-based authoring model и убрать `FlaUI.*` surface из headless package.
3. Phase 3: ввести hybrid capability/diagnostic contracts и мигрировать runtime/UiTestBase на новый failure contract.
4. Phase 4: включить release engineering governance и CI gates.
5. Phase 5: расширить regression/contract/package test suite и закрепить `FlaUI` как первый mandatory consumer generated `UiLocatorManifest`.

### 10.2 Backward compatibility policy
- Для внутренних потребителей репозитория: direct migration в одной ветке.
- Для publishable packages: major-version bump.
- Для publishable packages семейства: lockstep versioning одним release train.
- Обязательные артефакты major migration:
  - migration guide;
  - API migration map;
  - release notes;
  - перечень removed/renamed namespaces and types.
- Явное решение:
  - отдельный compatibility package не создаётся;
  - после публикации нового major rollback на старый public surface не выполняется, возможен только forward-fix.

### 10.3 Rollback
- Rollback выполняется пофазно:
  - если срывается Phase 1, возвращаем bootstrap в текущие runtime-пакеты;
  - если срывается Phase 2 до публикации, возвращаемся к предыдущему surface внутри ветки;
  - если срывается Phase 3, capability/diagnostic layer может быть откатан без rollback package split;
  - release engineering and CI changes откатываются независимо от runtime API.
- Обязательное правило rollback:
  - не публиковать partial-breaking state без migration artifacts и green CI.
- No-return point:
  - после публикации breaking major откат старого namespace/API не выполняется; допускаются только patch/fix releases вперёд.

## 11. Тестирование и критерии приёмки
- Acceptance Criteria:
  - В publishable runtime packages отсутствует repo-specific logic поиска `.sln`, локального `dotnet build` и project-path bootstrap.
  - `EasyUse.TestHost` существует как `IsPackable = false` и является единственным местом, где допустим `LaunchFromProject`-подобный workflow.
  - `Avalonia.Headless.EasyUse` явно позиционирован как whitebox/in-process runtime.
  - Shared page/scenario surface зависит только от `EasyUse.Automation.Abstractions`, а compile-time generation подключается через явный `EasyUse.Automation.Authoring`.
  - В репозитории существует отдельный SDK-style project `tests/DotnetDebug.UiTests.Authoring`; runtime test projects больше не используют `Compile Include` shared source.
  - Headless package больше не экспортирует reusable API в `FlaUI.*` namespaces.
  - В кодовой базе остаётся один canonical `UiWait` source of truth в `EasyUse.Automation.Abstractions`.
  - `UiLocatorManifest`, `UiPageDefinition`, `UiControlDefinition` и `IUiLocatorManifestProvider` зафиксированы как canonical abstraction contracts.
  - Generated page object members возвращают только abstraction-layer contracts; blackbox runner path использует generated C# `UiLocatorManifest`, встроенный в authoring assembly, и получает его через `IUiLocatorManifestProvider`, без JSON/resource/external-file canonical contract.
  - Capability support выражен hybrid contract model и покрыт contract/regression tests.
  - Любой timeout/error в UI runtime/test layer отдаёт диагностический контекст и artifact references по mandatory artifact matrix.
  - `EasyUse.Automation.Authoring` и любые временные migration generator projects собираются без RS1036/RS2008 warning.
  - `FlaUI` имеет обязательный manifest contract/smoke suite как первый blackbox consumer.
  - `dotnet build`, full `dotnet test`, parity check и `dotnet pack` проходят без preview SDK warnings.
  - В репозитории зафиксирована поддерживаемая SDK/TFM policy, и она соблюдается всеми publishable packages.
  - Все publishable packages семейства публикуются и поддерживаются в lockstep-versioning policy.
  - Lockstep version source of truth централизован в `eng/Versions.props` и используется всеми publishable packages.
  - Есть migration guide, release notes и API migration map для breaking-переходов.
- Какие тесты добавить/изменить:
  - обновить существующие regression tests под новый package boundary;
  - добавить abstraction-layer compile/runtime tests для shared page/scenario source;
  - добавить topology tests на `DotnetDebug.UiTests.Authoring` и отсутствие `Compile Include` shared source в runtime projects;
  - добавить tests на inherited test discovery/parity после перехода на project-reference topology;
  - добавить tests на explicit authoring package wiring и отсутствие transitive generator delivery через runtime packages;
  - добавить contract tests на capability matrix;
  - добавить blackbox manifest contract/smoke tests, где `FlaUI` является первым consumer generated `UiLocatorManifest` через `IUiLocatorManifestProvider`;
  - добавить tests на diagnostics/failure artifacts;
  - добавить generator diagnostics tests;
  - добавить pack smoke tests через temp consumer;
  - оставить parity script обязательным gate.
- Команды для проверки:
  - `dotnet build DotnetDebug.sln -c Debug`
  - `dotnet test DotnetDebug.sln -c Debug`
  - `pwsh -File tests/Verify-UiScenarioDiscoveryParity.ps1`
  - `dotnet pack src/FlaUI.EasyUse/FlaUI.EasyUse.csproj -c Release`
  - `dotnet pack src/Avalonia.Headless.EasyUse/Avalonia.Headless.EasyUse.csproj -c Release`
  - `dotnet pack src/EasyUse.Automation.Abstractions/EasyUse.Automation.Abstractions.csproj -c Release`
  - `dotnet pack src/EasyUse.Automation.Authoring/EasyUse.Automation.Authoring.csproj -c Release`
  - `dotnet pack src/EasyUse.TUnit.Core/EasyUse.TUnit.Core.csproj -c Release`
  - `rg -n "FindSolutionRoot|ArgumentList.Add\\(\"build\"\\)|Assembly.LoadFrom|Path.Combine\\(solutionRoot" src`
  - `rg -n "^namespace\\s+FlaUI\\." src/Avalonia.Headless.EasyUse`
  - `rg -n "class UiWait|record UiWaitOptions" src`
  - `rg -n "<Compile Include=\"\\.\\.\\\\DotnetDebug\\.UiTests\\.Shared" tests`
  - `rg -n "UiLocatorManifest|UiPageDefinition|UiControlDefinition|IUiLocatorManifestProvider" src tests`
  - `rg -n "EasyUseVersion|PackageVersion|VersionPrefix" eng src`
  - `rg -n "AnalyzerReleases\\.(Shipped|Unshipped)\\.md|EnforceExtendedAnalyzerRules" src/EasyUse.Automation.Authoring src/*Generators`

## 12. Риски и edge cases
- Риск: namespace cleanup даёт жёсткое breaking impact для существующих consumers.
  - Митигация: major-version bump + migration artifacts + direct repo migration в одном PR.
- Риск: выделение `EasyUse.TestHost` может вскрыть скрытые зависимости runtime на текущий demo layout.
  - Митигация: пофазный перенос с contract tests на launch behavior.
- Риск: abstraction-layer увеличит объём Phase 2.
  - Митигация: переносить только canonical surface, уже реально используемый shared page/scenario code, без немедленного расширения всей control matrix.
- Риск: явный `EasyUse.Automation.Authoring` добавит ещё одну зависимость в authoring-проекты и ухудшит onboarding.
  - Митигация: шаблоны подключения, migration guide и analyzer diagnostic с явным сообщением о недостающем authoring package.
- Риск: generated `UiLocatorManifest` как новый межпакетный контракт может быть слишком детализированным и зацементировать лишние implementation details.
  - Митигация: в v1 фиксировать только locator/page/control definitions, без leakage runtime-specific behavior и без внешнего serialization contract.
- Риск: перенос shared scenarios в отдельную authoring assembly повлияет на test discovery semantics.
  - Митигация: отдельный gate на inherited test discovery/parity после перехода на project-reference topology.
- Риск: capability model получится слишком абстрактной и не облегчит жизнь тестам.
  - Митигация: использовать hybrid model, а не один только flat flags; начинать с реально рассыпающихся capability gaps (`GridCells`, `Calendar`, `Tree`, `DatePicker`).
- Риск: диагностический слой увеличит шум и стоимость поддержки.
  - Митигация: включить фиксированную minimal artifact matrix и ограничить расширенные artefacts только adapter-specific failure-path.
- Риск: CI matrix усложнит обслуживание проекта.
  - Митигация: разделить mandatory gates и optional/nightly jobs.
- Риск: pack smoke вскроет package metadata и dependency issues в середине рефакторинга.
  - Митигация: сделать pack smoke обязательным уже с первой фазы release engineering.
- Риск: после публикации breaking major команда попытается откатывать namespace/API назад.
  - Митигация: явно зафиксировать no-return point и использовать только forward-fix release strategy.

## 13. План выполнения
1. Подготовить архитектурный PR для `EasyUse.TestHost` и выноса repo bootstrap из runtime packages.
2. Добавить `EasyUse.Automation.Abstractions` и `EasyUse.Automation.Authoring`, затем зафиксировать contracts `UiLocatorManifest`/`UiPageDefinition`/`UiControlDefinition`/`IUiLocatorManifestProvider`.
3. Создать `tests/DotnetDebug.UiTests.Authoring`, перенести туда shared pages/scenarios и убрать `Compile Include` shared source из runtime test projects.
4. Сконсолидировать runtime-specific generators в `EasyUse.Automation.Authoring` и зафиксировать explicit authoring dependency.
5. Реализовать generated C# `UiLocatorManifest` и `IUiLocatorManifestProvider`.
6. Перевести `FlaUI` smoke/contract path на потребление manifest через `IUiLocatorManifestProvider` как первого blackbox consumer.
7. Сформировать migration map для canonical namespace cleanup headless runtime и выполнить direct repo migration.
8. Реализовать hybrid capability model минимального объёма и убрать `NotSupported`-контракт как implicit behavior.
9. Внедрить unified diagnostics/artifact model в `EasyUse.Automation.Abstractions`, `EasyUse.TUnit.Core` и runtime packages.
10. Выровнять generator governance, packaging metadata, lockstep versioning и SDK/TFM policy.
11. Добавить build/test/pack/parity CI gates.
12. Обновить README, support matrix и migration documentation.

## 14. Открытые вопросы
- Блокирующих открытых вопросов нет.
- Зафиксированные решения для EXEC:
  - `EasyUse.TestHost` является internal-only и `IsPackable = false`;
  - `LaunchFromProject` выводится из publishable packages;
  - shared page/scenario/generator surface переводится на `EasyUse.Automation.Abstractions`;
  - compile-time authoring/generator contract выделяется в `EasyUse.Automation.Authoring` и подключается явно;
  - `UiWait` и waiting primitives живут только в `EasyUse.Automation.Abstractions`;
  - canonical blackbox descriptor contract фиксируется как `UiLocatorManifest`/`UiPageDefinition`/`UiControlDefinition`/`IUiLocatorManifestProvider` в abstraction layer;
  - canonical manifest delivery это generated C# code в authoring assembly;
  - в репозитории вводится отдельный `tests/DotnetDebug.UiTests.Authoring` как owner shared pages/scenarios/manifest;
  - capability model реализуется как hybrid model: compile-time interfaces + runtime descriptor;
  - `Avalonia.Headless.EasyUse` позиционируется как whitebox/in-process runtime;
  - `FlaUI` является первым mandatory consumer generated `UiLocatorManifest` на contract/smoke уровне;
  - отдельный compatibility package не создаётся;
  - все publishable packages семейства versioned lockstep;
  - после публикации breaking major действует only-forward-fix release policy;
  - отдельный `EasyUse.Diagnostics` проект не вводится на этом этапе.

## 15. Соответствие профилю
- Профиль:
  - `dotnet-desktop-client`
  - `refactor-architecture`
- Выполненные требования профиля:
  - схема зависимостей до/после зафиксирована;
  - список публичных API и точки миграции описаны;
  - compatibility/migration policy зафиксирована;
  - policy delivery/versioning генераторов и runtime пакетов зафиксирована;
  - rollout и rollback описаны;
  - `dotnet build`/`dotnet test`/`dotnet pack` включены в validation plan;
  - поддерживаемая SDK/TFM policy зафиксирована.

## 16. Таблица изменений файлов
| Файл | Изменения | Причина |
| --- | --- | --- |
| `src/EasyUse.Automation.Abstractions/*` | new | Canonical adapter-neutral automation/page/waiting surface |
| `src/EasyUse.Automation.Authoring/*` | new | Compile-time authoring package, source generators, locator manifest generation |
| `src/EasyUse.TestHost/*` | new | Вынесение repo-specific bootstrap и local AUT hosting |
| `tests/DotnetDebug.UiTests.Authoring/*` | new | Отдельный shared authoring project для pages/scenarios/generated manifest |
| `src/FlaUI.EasyUse/Session/*` | update | Удаление solution/build bootstrap из runtime package |
| `src/Avalonia.Headless.EasyUse/Session/*` | update | Удаление repo bootstrap и переход на canonical runtime contracts |
| `src/Avalonia.Headless.EasyUse/AutomationElements/*` | update | Выпрямление namespaces и adapter boundaries |
| `src/Avalonia.Headless.EasyUse/Extensions/*` | update | Выпрямление namespaces и diagnostic hooks |
| `src/Avalonia.Headless.EasyUse/Waiting/*` | update/delete | Удаление дублирования и canonical namespace story |
| `src/EasyUse.Session.Contracts/*` | update | Только launch/session contracts |
| `src/EasyUse.TUnit.Core/*` | update | Artifact hooks и test lifecycle hardening |
| `src/FlaUI.EasyUse.Generators/*` | delete/migrate | Сведение runtime-specific generators в authoring package |
| `src/Avalonia.Headless.EasyUse.Generators/*` | delete/migrate | Сведение runtime-specific generators в authoring package |
| `tests/DotnetDebug.UiTests.Shared/*` | delete/migrate | Уход от `Compile Include` shared source к отдельному authoring project |
| `tests/EasyUse.Automation.Abstractions.Tests/*` | new | Contract/capability/diagnostics tests для abstraction layer |
| `tests/EasyUse.Automation.Authoring.Tests/*` | new | Generator/diagnostics/topology tests для authoring package |
| `tests/*` | update | Contract/regression/pack smoke/generator/manifest tests |
| `eng/Versions.props` | new | Single source of truth для lockstep versioning |
| `global.json` | update | SDK pinning |
| `Directory.Build.props` | new | Единый build/analyzer policy |
| `Directory.Packages.props` | new | Central package management |
| `.editorconfig` | new/update | Code style/analyzer policy |
| `README.md` | update | Новая архитектура, migration/packaging usage |
| `ControlSupportMatrix.md` | update | Capability model и explicit adapter support matrix |
| `specs/reports/2026-03-06-framework-migration-guide.md` | new | Migration guide для breaking-переходов |
| `specs/reports/2026-03-06-framework-api-migration-map.md` | new | Карта public API migration |
| `specs/reports/2026-03-06-framework-release-notes.md` | new | Release notes industrialization major |

## 17. Таблица соответствий (было -> стало)
| Область | Было | Стало |
| --- | --- | --- |
| Runtime package boundary | runtime + repo bootstrap смешаны | runtime и repo test host разделены |
| Shared automation surface | зависимость shared code от adapter-specific namespaces | единый abstraction-layer |
| Repo test topology | shared source через `Compile Include` в два runtime проекта | отдельный `DotnetDebug.UiTests.Authoring` project-owner |
| Generator delivery | runtime-specific analyzers и прямые adapter-bound generators | отдельный explicit authoring package |
| Blackbox descriptor contract | не определён | generated C# `UiLocatorManifest` + `IUiLocatorManifestProvider` в authoring assembly |
| Headless public API | смешение `Avalonia.*` и `FlaUI.*` namespace story | один canonical namespace family |
| Headless positioning | неявная mixed story | whitebox/in-process runtime |
| Capability support | implicit/knowledge-by-source | hybrid contract model |
| Failure diagnostics | timeout message-only | fixed artifact matrix + failure context |
| Packaging | demo-grade metadata/governance | publishable package discipline |
| CI | локальная проверка и частичные скрипты | build/test/pack/parity mandatory gates |
| Wait/diagnostic primitives | частично дублируются | один canonical source of truth в abstraction-layer |
| Family versioning | не зафиксировано | lockstep releases для всех publishable packages |

## 18. Альтернативы и компромиссы
- Вариант: оставить framework как внутренний demo/reference implementation.
  - Плюсы: минимум работ.
  - Минусы: высокий долг сопровождения и слабая publishability.
  - Почему не выбран: не решает цель industrialization.
- Вариант: оставить shared source compatibility через долговременный `FlaUI.*` surface в headless package.
  - Плюсы: минимум миграции.
  - Минусы: сохраняет ложную public boundary и конфликт package identity.
  - Почему не выбран: промышленный API должен быть adapter-neutral на уровне abstraction-layer, а не через namespace mimicry.
- Вариант: вынести `LaunchFromProject` в отдельный publishable helper package.
  - Плюсы: сохраняет convenience для части consumers.
  - Минусы: снова смешивает framework API с repo/MSBuild-specific workflow.
  - Почему не выбран: `LaunchFromProject` признан internal repo convenience, а не частью industrial public contract.
- Вариант: встраивать generators в runtime packages и доставлять authoring tooling неявно.
  - Плюсы: проще onboarding, меньше явных зависимостей в authoring project.
  - Минусы: смешивает runtime и compile-time responsibility, усложняет dual-runtime проекты и размазывает canonical source генерации.
  - Почему не выбран: industrial contract должен разделять runtime adapter и compile-time authoring layer.
- Вариант: canonical locator manifest как JSON/resource/external build artifact.
  - Плюсы: потенциально удобнее для non-.NET consumers и внешних runners.
  - Минусы: отдельный schema/versioning contract, выше риск drift между assembly и manifest, сложнее pack/publish/diagnostics.
  - Почему не выбран: для текущего .NET-first family лучше generated C# contract в lockstep assembly boundary.
- Вариант: оставить shared authoring в виде `Compile Include` в runtime test projects.
  - Плюсы: минимальная миграция.
  - Минусы: нет одного owner assembly для generated manifest, generator diagnostics дублируются, слабая industrial topology.
  - Почему не выбран: отдельный authoring project даёт cleaner contract и реалистичную reference architecture для consumers.
- Вариант: ограничиться только release engineering без архитектурного split.
  - Плюсы: быстрее.
  - Минусы: не устраняет корневую проблему package/API boundaries.
  - Почему не выбран: лечит симптомы, а не причину.
- Вариант: сделать one-shot полный rewrite framework.
  - Плюсы: теоретически можно сразу получить clean architecture.
  - Минусы: чрезмерный риск, потеря regression stability, слабый rollout.
  - Почему не выбран: противоречит требованию безопасной поэтапной миграции.

## 19. Исполнительные приложения
### 19.1 Canonical API Appendix
Ниже приведён нормативный minimum contract. Реализация может отличаться внутренними деталями, но имена сущностей, их ответственность и семантика должны совпадать.

```csharp
namespace EasyUse.Automation.Abstractions;

public enum UiLocatorKind
{
    AutomationId = 0,
    Name = 1
}

public interface IUiControl
{
    string Name { get; }
    string AutomationId { get; }
}

public interface ITextBoxControl : IUiControl
{
    string Text { get; }
    void EnterText(string value);
}

public interface IButtonControl : IUiControl
{
    void Click();
}

public interface ILabelControl : IUiControl
{
    string Text { get; }
}

public interface IUiControlResolver
{
    TControl Resolve<TControl>(UiControlDefinition definition)
        where TControl : class, IUiControl;
}

public abstract class UiPage
{
    protected UiPage(IUiControlResolver resolver) { /* store resolver */ }

    protected TControl Resolve<TControl>(UiControlDefinition definition)
        where TControl : class, IUiControl
        => throw new NotImplementedException();
}

public sealed record UiControlDefinition(
    string PropertyName,
    UiControlType ControlType,
    string LocatorValue,
    UiLocatorKind LocatorKind = UiLocatorKind.AutomationId,
    bool FallbackToName = true,
    string? CapabilityKey = null);

public sealed record UiPageDefinition(
    string PageTypeFullName,
    string PageName,
    IReadOnlyList<UiControlDefinition> Controls);

public sealed record UiLocatorManifest(
    string ContractVersion,
    string AssemblyName,
    IReadOnlyList<UiPageDefinition> Pages);

public interface IUiLocatorManifestProvider
{
    UiLocatorManifest GetManifest();
}

public sealed record UiRuntimeCapabilities(
    bool SupportsGridCellAccess,
    bool SupportsCalendarRangeSelection,
    bool SupportsTreeNodeExpansionState,
    bool SupportsRawNativeHandles,
    bool SupportsScreenshots);

public enum UiFailureArtifactKind
{
    LogicalTree,
    Screenshot,
    ControlSnapshot,
    ProcessInfo,
    WindowHandle,
    LastObservedValueDump
}

public sealed record UiFailureArtifact(
    UiFailureArtifactKind Kind,
    string LogicalName,
    string RelativePath,
    string ContentType,
    bool IsRequiredByContract,
    string? InlineTextPreview = null);

public sealed record UiFailureContext(
    string OperationName,
    string AdapterId,
    string? PageTypeFullName,
    string? ControlPropertyName,
    string? LocatorValue,
    UiLocatorKind? LocatorKind,
    TimeSpan Timeout,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset FinishedAtUtc,
    string? LastObservedValue,
    UiRuntimeCapabilities Capabilities,
    IReadOnlyList<UiFailureArtifact> Artifacts);

public sealed class UiOperationException : Exception
{
    public UiOperationException(string message, UiFailureContext failureContext, Exception? innerException = null)
        : base(message, innerException)
    {
        FailureContext = failureContext;
    }

    public UiFailureContext FailureContext { get; }
}
```

Дополнительные нормативные правила:
- `UiControlType` сохраняет текущую control matrix и минимум покрывает все контролы, уже используемые в shared regression suite.
- `IUiLocatorManifestProvider` должен генерироваться как public parameterless provider type на одну authoring assembly.
- Blackbox runner не сканирует assembly в поисках ad-hoc generated types; он работает только через прямую зависимость на `IUiLocatorManifestProvider`.
- Universal abstraction contracts в v1 обязательны минимум для control families, уже используемых в shared scenarios: `TextBox`, `Button`, `Label`, `ListBox`, `CheckBox`, `ComboBox`, `RadioButton`, `ToggleButton`, `Slider`, `ProgressBar`, `Calendar`, `DateTimePicker`, `Spinner`, `Tab`, `TabItem`, `Tree`, `Grid`.

### 19.2 Project / Package / Namespace Map
| Project path | Assembly / Package | Root namespace | Packable | Статус / правило |
| --- | --- | --- | --- | --- |
| `src/EasyUse.Automation.Abstractions/EasyUse.Automation.Abstractions.csproj` | `EasyUse.Automation.Abstractions` | `EasyUse.Automation.Abstractions` | yes | canonical public abstraction package |
| `src/EasyUse.Automation.Authoring/EasyUse.Automation.Authoring.csproj` | `EasyUse.Automation.Authoring` | `EasyUse.Automation.Authoring` | yes | analyzers + source generators + manifest/provider generation |
| `src/EasyUse.Session.Contracts/EasyUse.Session.Contracts.csproj` | `EasyUse.Session.Contracts` | `EasyUse.Session.Contracts` | yes | launch/session contracts only |
| `src/EasyUse.TUnit.Core/EasyUse.TUnit.Core.csproj` | `EasyUse.TUnit.Core` | `EasyUse.TUnit.Core` | yes | test lifecycle / asserts / artifact hooks |
| `src/FlaUI.EasyUse/FlaUI.EasyUse.csproj` | `FlaUI.EasyUse` | `FlaUI.EasyUse` | yes | blackbox desktop adapter |
| `src/Avalonia.Headless.EasyUse/Avalonia.Headless.EasyUse.csproj` | `Avalonia.Headless.EasyUse` | `Avalonia.Headless.EasyUse` | yes | whitebox headless adapter |
| `src/EasyUse.TestHost/EasyUse.TestHost.csproj` | `EasyUse.TestHost` | `EasyUse.TestHost` | no | repo-only bootstrap/build helpers |
| `tests/DotnetDebug.UiTests.Authoring/DotnetDebug.UiTests.Authoring.csproj` | `DotnetDebug.UiTests.Authoring` | `DotnetDebug.UiTests.Authoring` | no | shared page objects, shared scenarios, generated manifest/provider owner |
| `tests/DotnetDebug.UiTests.FlaUI.EasyUse/DotnetDebug.UiTests.FlaUI.EasyUse.csproj` | `DotnetDebug.UiTests.FlaUI.EasyUse` | `DotnetDebug.UiTests.FlaUI.EasyUse` | no | FlaUI runtime wrappers + blackbox manifest consumer tests |
| `tests/DotnetDebug.UiTests.Avalonia.Headless/DotnetDebug.UiTests.Avalonia.Headless.csproj` | `DotnetDebug.UiTests.Avalonia.Headless` | `DotnetDebug.UiTests.Avalonia.Headless` | no | headless runtime wrappers + whitebox parity tests |
| `tests/EasyUse.Automation.Abstractions.Tests/EasyUse.Automation.Abstractions.Tests.csproj` | `EasyUse.Automation.Abstractions.Tests` | `EasyUse.Automation.Abstractions.Tests` | no | contract/capability/diagnostics tests |
| `tests/EasyUse.Automation.Authoring.Tests/EasyUse.Automation.Authoring.Tests.csproj` | `EasyUse.Automation.Authoring.Tests` | `EasyUse.Automation.Authoring.Tests` | no | generator output/diagnostics/topology tests |
| `src/FlaUI.EasyUse.Generators/FlaUI.EasyUse.Generators.csproj` | `FlaUI.EasyUse.Generators` | `FlaUI.EasyUse.Generators` | transitional only | удалить к завершению Phase 2 |
| `src/Avalonia.Headless.EasyUse.Generators/Avalonia.Headless.EasyUse.Generators.csproj` | `Avalonia.Headless.EasyUse.Generators` | `Avalonia.Headless.EasyUse.Generators` | transitional only | удалить к завершению Phase 2 |
| `tests/DotnetDebug.UiTests.Shared/*` | n/a | n/a | n/a | удалить после переноса в `DotnetDebug.UiTests.Authoring` |

### 19.3 File-level Migration Map
| Было | Стало | Действие | Комментарий |
| --- | --- | --- | --- |
| `tests/DotnetDebug.UiTests.Shared/Pages/MainWindowPage.cs` | `tests/DotnetDebug.UiTests.Authoring/Pages/MainWindowPage.cs` | move + rewrite | namespace/base type переводятся на abstraction layer |
| `tests/DotnetDebug.UiTests.Shared/Tests/MainWindowScenariosBase.cs` | `tests/DotnetDebug.UiTests.Authoring/Tests/MainWindowScenariosBase.cs` | move + rewrite | shared scenario base перестаёт зависеть от adapter-specific namespaces |
| `tests/DotnetDebug.UiTests.FlaUI.EasyUse/*.csproj` `Compile Include` | `ProjectReference` на `DotnetDebug.UiTests.Authoring` | update | runtime project становится thin wrapper |
| `tests/DotnetDebug.UiTests.Avalonia.Headless/*.csproj` `Compile Include` | `ProjectReference` на `DotnetDebug.UiTests.Authoring` | update | runtime project становится thin wrapper |
| `src/FlaUI.EasyUse/PageObjects/UiControlAttribute.cs` | `src/EasyUse.Automation.Abstractions/PageObjects/UiControlAttribute.cs` | move semantics | runtime package больше не владеет canonical attribute |
| `src/FlaUI.EasyUse/PageObjects/UiPage.cs` | split between `EasyUse.Automation.Abstractions` + adapter resolver implementation | split | базовый page contract уходит в abstraction layer, adapter-specific resolving остаётся в runtime |
| `src/Avalonia.Headless.EasyUse/PageObjects/UiPageAndAttributes.cs` | split between `EasyUse.Automation.Abstractions` + `Avalonia.Headless.EasyUse` | split | удалить legacy/new flavor confusion |
| `src/FlaUI.EasyUse.Generators/UiControlSourceGenerator.cs` | `src/EasyUse.Automation.Authoring/UiControlSourceGenerator.cs` | migrate | generator становится adapter-neutral |
| `src/Avalonia.Headless.EasyUse.Generators/UiControlSourceGenerator.cs` | `src/EasyUse.Automation.Authoring/UiControlSourceGenerator.cs` | merge | убрать dual generator packages |
| `src/*/Waiting/UiWait*.cs` | `src/EasyUse.Automation.Abstractions/Waiting/*` | consolidate | один canonical wait source of truth |
| `DesktopAppSession.LaunchFromProject` в runtime packages | `EasyUse.TestHost` | move | publishable packages теряют repo bootstrap |
| `tests/Verify-UiScenarioDiscoveryParity.ps1` | keep + update | update | parity должен знать про новую authoring topology |

### 19.4 Phase Exit Criteria
#### Phase 1 exit
- `EasyUse.TestHost` существует и является единственным местом для project bootstrap.
- В publishable packages нет `.sln` discovery, локального `dotnet build` и `Assembly.LoadFrom`-based public launch.
- `dotnet build` + `dotnet test` зелёные без функциональной деградации текущих тестов.

#### Phase 2 exit
- `EasyUse.Automation.Abstractions` и `EasyUse.Automation.Authoring` собираются.
- `tests/DotnetDebug.UiTests.Authoring` введён, runtime projects больше не используют `Compile Include` shared source.
- `UiLocatorManifest` / `UiPageDefinition` / `UiControlDefinition` / `IUiLocatorManifestProvider` реализованы и сгенерированы.
- Headless package больше не экспортирует reusable API в `FlaUI.*` namespaces.
- Runtime-specific generator packages удалены или помечены как non-shipping migration-only; publish path идёт только через `EasyUse.Automation.Authoring`.

#### Phase 3 exit
- `UiRuntimeCapabilities`, `UiFailureContext`, `UiFailureArtifact`, `UiOperationException` существуют в abstraction layer.
- Unsupported operations выражаются capability contract, а не implicit `NotSupported` из общих API.
- Failure diagnostics tests зелёные на обоих runtime.

#### Phase 4 exit
- `global.json`, `Directory.Build.props`, `Directory.Packages.props`, `.editorconfig`, `eng/Versions.props` введены и используются.
- Все publishable packages читают version из `eng/Versions.props`.
- Analyzer governance зелёная для `EasyUse.Automation.Authoring`.
- `dotnet pack` green для всех publishable packages.

#### Phase 5 exit
- `FlaUI` smoke/contract suite потребляет generated manifest через `IUiLocatorManifestProvider`.
- topology/discovery/parity tests зелёные после authoring split.
- package smoke, capability contract, generator diagnostics и failure diagnostics включены в mandatory gates.

### 19.5 Worked Example
#### Authoring input
```csharp
using EasyUse.Automation.Abstractions;

namespace DotnetDebug.UiTests.Authoring.Pages;

[UiControl("NumbersInput", UiControlType.TextBox, "NumbersInput")]
[UiControl("CalculateButton", UiControlType.Button, "CalculateButton")]
[UiControl("ResultText", UiControlType.Label, "ResultText")]
public sealed partial class MainWindowPage : UiPage
{
    public MainWindowPage(IUiControlResolver resolver) : base(resolver)
    {
    }
}
```

#### Expected generated page surface
```csharp
namespace DotnetDebug.UiTests.Authoring.Pages;

public sealed partial class MainWindowPage
{
    public ITextBoxControl NumbersInput => Resolve<ITextBoxControl>(MainWindowPageDefinitions.NumbersInput);
    public IButtonControl CalculateButton => Resolve<IButtonControl>(MainWindowPageDefinitions.CalculateButton);
    public ILabelControl ResultText => Resolve<ILabelControl>(MainWindowPageDefinitions.ResultText);
}
```

#### Expected generated manifest/provider
```csharp
namespace DotnetDebug.UiTests.Authoring.Generated;

public sealed class DotnetDebugUiTestsAuthoringManifestProvider : IUiLocatorManifestProvider
{
    public UiLocatorManifest GetManifest() => Manifest;

    public static UiLocatorManifest Manifest { get; } = new(
        ContractVersion: "1",
        AssemblyName: "DotnetDebug.UiTests.Authoring",
        Pages: new[]
        {
            new UiPageDefinition(
                PageTypeFullName: "DotnetDebug.UiTests.Authoring.Pages.MainWindowPage",
                PageName: "MainWindowPage",
                Controls: new[]
                {
                    new UiControlDefinition("NumbersInput", UiControlType.TextBox, "NumbersInput"),
                    new UiControlDefinition("CalculateButton", UiControlType.Button, "CalculateButton"),
                    new UiControlDefinition("ResultText", UiControlType.Label, "ResultText")
                })
        });
}
```

#### Expected FlaUI consumption pattern
```csharp
IUiLocatorManifestProvider provider = new DotnetDebugUiTestsAuthoringManifestProvider();
UiLocatorManifest manifest = provider.GetManifest();

UiPageDefinition page = manifest.Pages.Single(p => p.PageTypeFullName.EndsWith("MainWindowPage"));
UiControlDefinition resultText = page.Controls.Single(c => c.PropertyName == "ResultText");

ILabelControl label = flaUiResolver.Resolve<ILabelControl>(resultText);
string text = label.Text;
```

Нормативные правила для worked example:
- provider type генерируется один на authoring assembly;
- provider имеет public parameterless constructor;
- runtime wrappers не генерируют собственные manifests;
- `FlaUI` smoke tests валидируют именно этот consumption path.

### 19.6 Capability Matrix v1
| Surface / operation | Contract type | FlaUI | Avalonia.Headless | Правило |
| --- | --- | --- | --- | --- |
| Locate by `AutomationId` / `Name` | universal | yes | yes | mandatory |
| Text entry / button click / label read | universal | yes | yes | mandatory |
| CheckBox / Radio / Toggle state | universal | yes | yes | mandatory |
| Combo selection by visible text | universal | yes | yes | mandatory |
| Tab selection | universal | yes | yes | mandatory |
| Slider / Spinner / ProgressBar basic set/read | universal | yes | yes | mandatory |
| Date picker set/read single date | universal | yes | yes | mandatory |
| Tree item selection + selected path read | universal | yes | yes | mandatory |
| Grid row selection by index + selected row label read | universal | yes | yes | mandatory |
| Grid cell access by row/column | capability-specific (`SupportsGridCellAccess`) | yes | no in v1 | not part of universal contract |
| Calendar range / multi-select | capability-specific (`SupportsCalendarRangeSelection`) | partial/yes | no in v1 | optional |
| Tree node expansion state | capability-specific (`SupportsTreeNodeExpansionState`) | yes | no in v1 | optional |
| Native process / handle access | capability-specific (`SupportsRawNativeHandles`) | yes | no | diagnostics only |
| Screenshot capture | capability-specific (`SupportsScreenshots`) | yes | no | diagnostics only |

Правило для EXEC:
- В universal contract попадают только операции, уже используемые в shared scenario baseline.
- Всё, что не universal, должно быть либо capability-gated, либо отсутствовать из generated common surface.

### 19.7 Diagnostics Schema
#### `UiFailureContext`
| Field | Required | Semantics |
| --- | --- | --- |
| `OperationName` | yes | canonical operation id, например `WaitUntilNameEquals` |
| `AdapterId` | yes | `flaui` / `avalonia-headless` |
| `PageTypeFullName` | when known | authoring page type |
| `ControlPropertyName` | when known | property from generated page surface |
| `LocatorValue` | when known | locator from manifest |
| `LocatorKind` | when known | `AutomationId` / `Name` |
| `Timeout` | yes | effective timeout |
| `StartedAtUtc` / `FinishedAtUtc` | yes | timing window |
| `LastObservedValue` | when available | latest seen text/value/state |
| `Capabilities` | yes | snapshot of `UiRuntimeCapabilities` |
| `Artifacts` | yes | never `null`; empty list if nothing collected |

#### `UiFailureArtifact`
| Field | Required | Semantics |
| --- | --- | --- |
| `Kind` | yes | artifact category |
| `LogicalName` | yes | stable semantic name, например `logical-tree` |
| `RelativePath` | yes | path relative to test artifact root |
| `ContentType` | yes | `text/plain`, `application/json`, `image/png` |
| `IsRequiredByContract` | yes | whether absence is a contract breach |
| `InlineTextPreview` | optional | short excerpt for logs |

#### Artifact root and naming
- Artifact root: `artifacts/ui-failures/<test-name>/<utc-timestamp>/`
- Naming pattern: `<sequence>-<operation>-<kind>.<ext>`
- Required artifact set:
  - all runtimes: logical tree dump, last observed values dump;
  - `FlaUI`: screenshot, process info, window handle when available;
  - `Avalonia.Headless`: logical tree snapshot + serialized control state.

### 19.8 Concrete Test Inventory
| Suite | Preferred project | What it must verify |
| --- | --- | --- |
| `ManifestContractTests` | `tests/EasyUse.Automation.Abstractions.Tests` | contract shape and immutability of `UiLocatorManifest` / `UiPageDefinition` / `UiControlDefinition` / `IUiLocatorManifestProvider` |
| `CapabilityContractTests` | `tests/EasyUse.Automation.Abstractions.Tests` | `UiRuntimeCapabilities` semantics and gating rules |
| `FailureDiagnosticsTests` | `tests/EasyUse.Automation.Abstractions.Tests` + runtime-specific tests | mandatory failure context and artifact matrix |
| `AuthoringGeneratorOutputTests` | `tests/EasyUse.Automation.Authoring.Tests` | generated page accessors + generated manifest/provider source |
| `AuthoringGeneratorDiagnosticsTests` | `tests/EasyUse.Automation.Authoring.Tests` | missing partial class, invalid property names, missing base class, analyzer ids |
| `AuthoringTopologyTests` | `tests/EasyUse.Automation.Authoring.Tests` or repo architecture tests | shared source no longer compiled via `Compile Include`; authoring project is single owner |
| `ManifestProviderConsumptionTests` | `tests/DotnetDebug.UiTests.FlaUI.EasyUse` | `FlaUI` consumes manifest through `IUiLocatorManifestProvider` |
| `BlackboxManifestSmokeTests` | `tests/DotnetDebug.UiTests.FlaUI.EasyUse` | `AutomationId`, fallback `Name`, composite control, failure path |
| `WhiteboxParityTests` | `tests/DotnetDebug.UiTests.Avalonia.Headless` | headless whitebox runtime still passes shared scenarios |
| `DiscoveryParityAfterAuthoringSplitTests` | `tests/Verify-UiScenarioDiscoveryParity.ps1` + runtime test projects | test discovery unchanged after authoring project split |
| `PackageSmokeTests` | script/temporary consumer under `tests/` | restore, reference and compile with published packages |

### 19.9 Allowed Transitional State
Следующие transitional states допустимы только внутри migration branch/PR train и не должны попадать в published stable release:
- Временное сосуществование `EasyUse.Automation.Authoring` и старых runtime-specific generator projects допустимо только до завершения Phase 2.
- Временные shim/forwarders для namespace migration допустимы только до того момента, пока репозиторий не мигрирован и тесты не зелёные.
- `tests/DotnetDebug.UiTests.Shared` может существовать физически до завершения переноса, но после появления `DotnetDebug.UiTests.Authoring` runtime projects уже не должны компилировать его через `Compile Include`.
- Временный dual-source generation допустим только для internal migration checks; publishable output должен иметь один canonical generator path.
- Ни один transitional state не может публиковаться как новый stable package version.

### 19.10 Out-of-bounds Edits
Другой агент не должен делать в рамках этой спеки:
- менять пользовательское поведение demo AUT, кроме технически необходимого wiring под тестовый runtime;
- менять `AutomationId`/selectors;
- переписывать тестовый DSL в новый синтаксис без прямой необходимости миграции на abstraction layer;
- вводить новый test framework вместо `TUnit`;
- добавлять web/mobile/runtime beyond `FlaUI` + `Avalonia.Headless`;
- расширять control matrix сверх того, что требуется для текущего regression baseline и capability-gating;
- вводить отдельный compatibility package;
- вводить отдельный `EasyUse.Diagnostics` package;
- публиковать partial-breaking release без migration docs и green gates.

### 19.11 Release Mechanics
- Single source of truth для семейства пакетов: `eng/Versions.props`.
- Нормативные свойства:
  - `EasyUseVersion`
  - optional `EasyUsePrereleaseSuffix`
- Все packable `csproj` читают `PackageVersion` из `eng/Versions.props`.
- Stable release train:
  1. bump `EasyUseVersion` в `eng/Versions.props`;
  2. обновить migration guide / release notes / API migration map;
  3. прогнать `build` / `test` / `pack` / parity / package smoke;
  4. выпустить all publishable packages с одной и той же версией;
  5. создать git tag формата `easyuse-v<version>`.
- Publish order по умолчанию:
  1. `EasyUse.Automation.Abstractions`
  2. `EasyUse.Automation.Authoring`
  3. `EasyUse.Session.Contracts`
  4. `EasyUse.TUnit.Core`
  5. `FlaUI.EasyUse`
  6. `Avalonia.Headless.EasyUse`
- Supported combination policy:
  - только same-version combination считается поддерживаемой;
  - cross-version support не гарантируется и не тестируется.

## 20. Результат прогона линтера
### 20.1 SPEC Linter checklist (A-F)
| Пункт | Статус | Комментарий |
| --- | --- | --- |
| A1 Цель сформулирована | PASS | Раздел 1 |
| A2 AS-IS описан | PASS | Раздел 2 |
| A3 Проблема одна, корневая | PASS | Раздел 3 |
| A4 Non-Goals заданы | PASS | Раздел 5 |
| A5 Ограничения зафиксированы | PASS | Раздел 0 |
| B1 Распределение ответственности | PASS | Разделы 6.1 и 6.5 |
| B2 Детальный дизайн | PASS | Раздел 6.3 |
| B3 Публичный API и миграция | PASS | Разделы 6.3, 10.2, 17 |
| B4 Error/diagnostics model описана | PASS | Раздел 6.3 Phase 3 |
| B5 Release/perf tradeoffs описаны | PASS | Разделы 6.3, 18 |
| C1 Интеграционные точки | PASS | Раздел 8 |
| C2 Изменения состояния/данных | PASS | Раздел 9 |
| C3 Rollout описан | PASS | Раздел 10.1 |
| C4 Rollback описан | PASS | Раздел 10.3 |
| D1 Acceptance criteria измеримы | PASS | Раздел 11 |
| D2 Есть список проверок/команд | PASS | Раздел 11 |
| D3 Тест-план покрывает regression/pack/parity | PASS | Раздел 11 |
| E1 План этапов задан | PASS | Разделы 6.3 и 13 |
| E2 Открытые вопросы не блокируют | PASS | Раздел 14 |
| E3 Масштаб и риски управляемы | PASS | Разделы 0, 12, 13 |
| F1 Соответствие выбранному профилю | PASS | Раздел 15 |

Итоговый статус SPEC Linter: `ГОТОВО`.

### 20.2 SPEC Rubric (0/2/5)
| Критерий | Балл | Обоснование |
| --- | --- | --- |
| 1. Ясность цели и границ | 5 | Одна корневая проблема и чёткие non-goals |
| 2. Понимание текущего состояния | 5 | AS-IS опирается на реальную структуру решения и текущие риски |
| 3. Конкретность целевого дизайна | 5 | TO-BE разбит на модули, правила и фазы |
| 4. Безопасность (миграция, откат) | 5 | rollout/rollback и migration policy описаны явно |
| 5. Тестируемость | 5 | acceptance criteria и validation commands измеримы |
| 6. Готовность к автономной реализации | 5 | этапы, риски и артефакты заданы достаточно подробно |

Итог: **30/30**
Зона: `готово к автономному выполнению`

Слабые места:
- Это large-инициатива; для EXEC потребуется декомпозиция минимум на 4-6 отдельных PR/подспек.

## 21. Approval
Ожидается фраза: **"Спеку подтверждаю"**

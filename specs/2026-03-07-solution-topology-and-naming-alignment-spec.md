# AppAutomation Rename, Topology and Namespace Alignment

## 0. Метаданные
- Тип (профиль): `dotnet-desktop-client` + `refactor-architecture`
- Владелец: Framework Maintainers
- Масштаб: medium
- Целевая ветка: текущая рабочая ветка
- Целевое семейство пакетов и namespace: `AppAutomation`
- Ограничения:
  - Не менять пользовательское поведение `DotnetDebug.Avalonia`.
  - Не менять `AutomationId` и рабочие UI-сценарии.
  - Не добавлять новый функционал в DSL и runtime, кроме технически нужного для выравнивания namespace и project topology.
  - Не вводить долгоживущую dual-namespace модель для `Avalonia.Headless.EasyUse`.
  - Перед завершением сохранить зелёными `dotnet build`, `dotnet test`, `dotnet pack`.
- Связанные ссылки:
  - `specs/2026-03-06-framework-industrialization-spec.md`
  - `README.md`
  - `DotnetDebug.sln`
  - `C:\Projects\My\Agents\instructions\core\quest-governance.md`
  - `C:\Projects\My\Agents\instructions\core\collaboration-baseline.md`
  - `C:\Projects\My\Agents\instructions\core\testing-baseline.md`
  - `C:\Projects\My\Agents\instructions\contexts\testing-dotnet.md`
  - `C:\Projects\My\Agents\instructions\profiles\dotnet-desktop-client.md`
  - `C:\Projects\My\Agents\instructions\profiles\refactor-architecture.md`
  - `C:\Projects\My\Agents\instructions\governance\spec-linter.md`
  - `C:\Projects\My\Agents\instructions\governance\spec-rubric.md`

## 1. Overview / Цель
Зафиксировать `AppAutomation` как каноническое имя framework family и привести решение к одному понятному соглашению по составу проектов, именованию проектов и namespace-family, одновременно убрав физические и логические остатки transitional topology, которые уже противоречат фактической архитектуре после industrialization.

Результат:
- reusable framework публикуется как семейство `AppAutomation.*`;
- решение содержит только живые проекты;
- каждая сборка владеет одной канонической namespace-family;
- папки отражают фактическую ответственность модулей;
- документация и solution topology совпадают с реальным состоянием репозитория.

## 2. Текущее состояние (AS-IS)
Фактическое состояние репозитория после предыдущих фаз:

1. В solution всё ещё включены transitional generator-проекты:
   - `src/FlaUI.EasyUse.Generators`
   - `src/Avalonia.Headless.EasyUse.Generators`
   При этом активные проекты решения на них больше не ссылаются, а канонический generator уже живёт в `src/EasyUse.Automation.Authoring`.

2. На диске остались legacy-каталоги, которые больше не участвуют в build topology:
   - `src/FlaUI.EasyUse.TUnit`
   - `src/Avalonia.Headless.EasyUse.TUnit`
   - `tests/DotnetDebug.UiTests.Shared`

3. `Avalonia.Headless.EasyUse` всё ещё экспортирует reusable типы под чужими namespace-family:
   - `FlaUI.Core.AutomationElements`
   - `FlaUI.Core.Conditions`
   - `FlaUI.Core.Definitions`
   - `FlaUI.EasyUse.Extensions`
   - `FlaUI.EasyUse.Helpers`
   - `FlaUI.EasyUse.PageObjects`
   Это противоречит целевой модели "one package = one canonical namespace family" из industrialization spec.

4. Внутренняя организация папок `src/Avalonia.Headless.EasyUse` смешивает:
   - живой runtime adapter code;
   - fake-FlaUI compatibility model;
   - legacy page object/extensions surface;
   - уже вытащенные в abstractions примитивы вроде wait/page contracts.

5. Документация расходится с реальностью:
   - `README.md` всё ещё описывает `FlaUI.EasyUse.TUnit`;
   - упоминает несуществующий тестовый проект `tests/DotnetDebug.UiTests.FlaUI`;
   - содержит устаревшее требование про preview SDK.

## 3. Проблема
Текущая topology уже функционально работает, но её состав, нейминг и папочная организация вводят в заблуждение: solution содержит мёртвые проекты, runtime-пакет экспортирует чужие namespace, а документация описывает уже несуществующую структуру. Это повышает риск неверных ссылок, регрессий при публикации пакетов и ошибочных инженерных решений при дальнейшей эволюции репозитория.

## 4. Цели дизайна
- Переименовать reusable framework family в `AppAutomation`.
- Убрать из solution и репозитория мёртвые transitional проекты и каталоги.
- Зафиксировать каноническое правило: одна publishable сборка владеет одной namespace-family.
- Выпрямить внутреннюю структуру `Avalonia.Headless.EasyUse`, чтобы live runtime code и compatibility internals были явно разделены.
- Синхронизировать README и project map с реальным состоянием решения.
- Сохранить текущее поведение и зелёный regression набор.

## 5. Non-Goals
- Не переименовывать все проекты ради косметики, если имя уже соответствует ответственности.
- Не переписывать DSL и control contracts заново.
- Не вводить новый runtime, новый test framework или новые control capabilities.
- Не расширять surface `EasyUse.Automation.Abstractions`.
- Не делать дополнительный large-scale package redesign поверх уже принятой industrialization spec.

## 6. Предлагаемое решение (TO-BE)

### 6.1 Naming Policy
Нормативные правила после рефакторинга:

1. Корневое имя publishable framework family: `AppAutomation`.
2. `PackageId`, имя проекта, имя каталога и public namespace-family должны совпадать по владельцу ответственности.
3. Publishable runtime package не экспортирует reusable API под чужим `PackageId`-family.
4. Repo-only проекты используют префикс `DotnetDebug.` и не маскируются под publishable package.
5. В solution не остаются проекты, которые уже не участвуют в dependency graph.
6. Legacy compatibility types допускаются только как `internal`-ориентированный слой в canonical package namespace tree, но не как отдельная public namespace-family.

### 6.2 Целевая схема проектов
В solution остаются только активные проекты:

| Категория | Проекты |
| --- | --- |
| Product apps | `DotnetDebug.Core`, `DotnetDebug`, `DotnetDebug.Avalonia` |
| Reusable automation/runtime | `AppAutomation.Abstractions`, `AppAutomation.Authoring`, `AppAutomation.Session.Contracts`, `AppAutomation.TUnit`, `AppAutomation.FlaUI`, `AppAutomation.Avalonia.Headless` |
| Repo-only automation host | `DotnetDebug.AppAutomation.TestHost` |
| Tests | `DotnetDebug.Tests`, `DotnetDebug.AppAutomation.Authoring`, `DotnetDebug.AppAutomation.Avalonia.Headless.Tests`, `DotnetDebug.AppAutomation.FlaUI.Tests`, `AppAutomation.Abstractions.Tests`, `AppAutomation.Authoring.Tests` |

Из solution и репозитория удаляются как superseded:
- `src/FlaUI.EasyUse.Generators`
- `src/Avalonia.Headless.EasyUse.Generators`
- `src/FlaUI.EasyUse.TUnit`
- `src/Avalonia.Headless.EasyUse.TUnit`
- `tests/DotnetDebug.UiTests.Shared`

### 6.3 Exact Rename Map
#### Projects and folders
| Было | Станет | Тип | Комментарий |
| --- | --- | --- | --- |
| `src/EasyUse.Automation.Abstractions` | `src/AppAutomation.Abstractions` | publishable | canonical abstractions package |
| `src/EasyUse.Automation.Authoring` | `src/AppAutomation.Authoring` | publishable | canonical generator/analyzer package |
| `src/EasyUse.Session.Contracts` | `src/AppAutomation.Session.Contracts` | publishable | launch/session contracts |
| `src/EasyUse.TUnit.Core` | `src/AppAutomation.TUnit` | publishable | TUnit integration surface без лишнего `Core` |
| `src/FlaUI.EasyUse` | `src/AppAutomation.FlaUI` | publishable | FlaUI runtime adapter |
| `src/Avalonia.Headless.EasyUse` | `src/AppAutomation.Avalonia.Headless` | publishable | headless runtime adapter |
| `src/EasyUse.TestHost` | `src/DotnetDebug.AppAutomation.TestHost` | repo-only | явный internal host для этого репозитория |
| `tests/EasyUse.Automation.Abstractions.Tests` | `tests/AppAutomation.Abstractions.Tests` | tests | unit/contract tests for abstractions |
| `tests/EasyUse.Automation.Authoring.Tests` | `tests/AppAutomation.Authoring.Tests` | tests | generator tests |
| `tests/DotnetDebug.UiTests.Authoring` | `tests/DotnetDebug.AppAutomation.Authoring` | repo-only | shared authoring assembly для test assets |
| `tests/DotnetDebug.UiTests.Avalonia.Headless` | `tests/DotnetDebug.AppAutomation.Avalonia.Headless.Tests` | tests | runtime-specific headless tests |
| `tests/DotnetDebug.UiTests.FlaUI.EasyUse` | `tests/DotnetDebug.AppAutomation.FlaUI.Tests` | tests | runtime-specific FlaUI tests |
| `tests/DotnetDebug.UiTests.Shared` | delete | stale | уже superseded authoring project |
| `src/FlaUI.EasyUse.Generators` | delete | stale | superseded by `AppAutomation.Authoring` |
| `src/Avalonia.Headless.EasyUse.Generators` | delete | stale | superseded by `AppAutomation.Authoring` |
| `src/FlaUI.EasyUse.TUnit` | delete | stale | superseded by `AppAutomation.TUnit` |
| `src/Avalonia.Headless.EasyUse.TUnit` | delete | stale | superseded by `AppAutomation.TUnit` |

#### Namespaces
| Было | Станет | Правило |
| --- | --- | --- |
| `EasyUse.Automation.Abstractions` | `AppAutomation.Abstractions` | canonical framework namespace |
| `EasyUse.Automation.Authoring` | `AppAutomation.Authoring` | canonical generator/analyzer namespace |
| `EasyUse.Session.Contracts` | `AppAutomation.Session.Contracts` | session contracts stay explicit |
| `EasyUse.TUnit.Core` | `AppAutomation.TUnit` | shorten and align with package name |
| `EasyUse.TestHost` | `DotnetDebug.AppAutomation.TestHost` | repo-only host namespace |
| `FlaUI.EasyUse` | `AppAutomation.FlaUI` | runtime adapter namespace |
| `Avalonia.Headless.EasyUse` | `AppAutomation.Avalonia.Headless` | runtime adapter namespace |
| `FlaUI.Core.AutomationElements` inside headless package | `AppAutomation.Avalonia.Headless.Internal.AutomationModel` | internal-only implementation namespace |
| `FlaUI.Core.Conditions` inside headless package | `AppAutomation.Avalonia.Headless.Internal.AutomationModel.Conditions` | internal-only implementation namespace |
| `FlaUI.Core.Definitions` inside headless package | `AppAutomation.Avalonia.Headless.Internal.AutomationModel.Definitions` | internal-only implementation namespace |
| `FlaUI.EasyUse.Extensions` inside headless package | remove | abstraction-layer API already owns this responsibility |
| `FlaUI.EasyUse.PageObjects` inside headless package | remove | abstraction-layer API already owns this responsibility |
| `FlaUI.EasyUse.Helpers` inside headless package | `AppAutomation.Avalonia.Headless.Internal.Diagnostics` or remove | only if still used by runtime internals |

### 6.4 Целевая схема namespace ownership
| Проект | Канонический public namespace family | Правило |
| --- | --- | --- |
| `AppAutomation.Abstractions` | `AppAutomation.Abstractions` | один source of truth для authoring/runtime-neutral contracts |
| `AppAutomation.Authoring` | `AppAutomation.Authoring` | один source of truth для generators/analyzers |
| `AppAutomation.Session.Contracts` | `AppAutomation.Session.Contracts` | только launch/session contracts |
| `AppAutomation.TUnit` | `AppAutomation.TUnit` | только test base/assertions/hooks |
| `AppAutomation.FlaUI` | `AppAutomation.FlaUI` | FlaUI runtime adapter |
| `AppAutomation.Avalonia.Headless` | `AppAutomation.Avalonia.Headless` | headless runtime adapter |

Норматив для `AppAutomation.Avalonia.Headless`:
- public reusable API остаётся только в `AppAutomation.Avalonia.Headless.*`;
- fake automation model, если он нужен для внутренней реализации runtime, переносится в `AppAutomation.Avalonia.Headless.Internal.*`;
- `FlaUI.*` namespaces из headless package удаляются полностью.

### 6.5 Folder Organization
Целевая структура `src/AppAutomation.Avalonia.Headless`:

| Папка | Содержимое |
| --- | --- |
| `Automation/` | resolver и runtime-facing automation orchestration |
| `Session/` | runtime/session lifecycle |
| `Internal/AutomationModel/` | внутренние automation element wrappers и condition model |
| `Internal/Diagnostics/` | внутренние технические helper-утилиты, если они нужны только headless runtime |

Удаляются как legacy:
- `Conditions/`
- `Definitions/`
- `Helpers/`
- `PageObjects/`
- `Extensions/`
- `Waiting/`
- `Bridge/` если после переноса она пуста или не нужна

Примечание:
- физическое переименование папок должно следовать только за реальным переносом живого кода;
- пустые transitional каталоги удаляются целиком.

### 6.6 Public API и миграция
Публичные migration points:

1. Все publishable framework packages меняют root-name на `AppAutomation`.
2. `AppAutomation.Avalonia.Headless` перестаёт публиковать типы в `FlaUI.*`.
3. Внутри текущего репозитория все active usages переводятся на `AppAutomation.*` или `DotnetDebug.AppAutomation.*`.
4. Для внешних package consumers это является intentional breaking change и требует явной migration note в README/spec, но отдельный compatibility package не создаётся.

Список ожидаемых внутренних миграций:
- `HeadlessControlResolver` перестаёт зависеть от `FlaUI.Core.AutomationElements` и `FlaUI.Core.Conditions`.
- `DesktopSession` перестаёт возвращать headless window через `FlaUI.Core.AutomationElements.Window`; вместо этого использует canonical internal headless runtime model.
- старые headless `PageObjects`/`Extensions` удаляются как уже superseded abstraction-layer API.

### 6.7 Before / After Dependency View
AS-IS:
- `Avalonia.Headless.EasyUse` = runtime adapter + fake FlaUI compatibility API + legacy page/extensions DSL.
- `EasyUse.*` = transitional root family, уже не отражающая реальную зрелость framework.
- solution = active projects + dead generator projects.
- repo folders = active structure + stale shared/TUnit leftovers.

TO-BE:
- `AppAutomation.*` = единое canonical framework family.
- `AppAutomation.Avalonia.Headless` = only headless runtime adapter + internal automation model.
- solution = only active projects.
- repo folders = only folders backed by live source ownership.

### 6.8 Phased Plan
#### Phase 1. Dead topology cleanup
- Удалить из solution generator-проекты.
- Удалить из репозитория generator-проекты и stale каталоги `*.TUnit`, `DotnetDebug.UiTests.Shared`.
- Переименовать проекты, каталоги, `PackageId`, `AssemblyName` и `RootNamespace` в `AppAutomation.*`.
- Обновить README и project map.

#### Phase 2. Headless namespace normalization
- Перенести fake automation model из `FlaUI.*` namespace в `AppAutomation.Avalonia.Headless.Internal.*`.
- Перепривязать live headless runtime code на canonical/internal namespaces.
- Удалить legacy public surface `PageObjects`, `Extensions`, `Helpers`, `Definitions`, `Conditions`, если он больше не нужен active code.

#### Phase 3. Validation and cleanup
- Добавить/обновить regression tests на headless runtime после namespace migration.
- Проверить `build`, `test`, `pack`.
- Проверить, что в исходниках не осталось public `namespace FlaUI.*` под `src/Avalonia.Headless.EasyUse`.

## 7. Compatibility, Rollout, Rollback
### 7.1 Совместимость
- Поведение demo AUT и UI-сценариев должно остаться без изменений.
- Внутрирепозиторная совместимость сохраняется через прямую миграцию usages в одном changeset.
- Переименование framework family в `AppAutomation` считается intentional rename и допускает широкое обновление `using`/project references в этом же changeset.
- Внешняя package-совместимость для legacy headless `FlaUI.*` namespace не сохраняется; это осознанный breaking change в рамках выпрямления architecture contract.

### 7.2 Rollout
1. Сначала удалить dead projects/folders, не влияющие на runtime.
2. Затем перевести live headless implementation на canonical/internal namespaces.
3. Потом удалить legacy source files и прогнать тесты.
4. В конце обновить README и связанные repo docs.

### 7.3 Rollback
- Если namespace migration ломает runtime-тесты, откат выполняется коммитом, возвращающим headless internal model и связанные usages.
- Если удаление dead projects unexpectedly влияет на build graph, откат ограничивается восстановлением solution entries и каталогов без затрагивания runtime logic.

## 8. Acceptance Criteria
1. `DotnetDebug.sln` использует новые project names под `AppAutomation.*` и больше не содержит `FlaUI.EasyUse.Generators` и `Avalonia.Headless.EasyUse.Generators`.
2. На диске отсутствуют `src/FlaUI.EasyUse.Generators`, `src/Avalonia.Headless.EasyUse.Generators`, `src/FlaUI.EasyUse.TUnit`, `src/Avalonia.Headless.EasyUse.TUnit`, `tests/DotnetDebug.UiTests.Shared`.
3. Исходники publishable framework больше не содержат namespace families `EasyUse.*`, `FlaUI.EasyUse.*` и headless-public `FlaUI.Core.*`.
4. Поиск по `src/AppAutomation.Avalonia.Headless` не находит public namespace-family `FlaUI.Core.*` и `FlaUI.EasyUse.*`.
5. `AppAutomation.Avalonia.Headless` собирается и проходит runtime-тесты после migration.
6. README отражает фактический проектный состав, актуальные package names и stable SDK policy.
7. Полные команды валидации зелёные:
   - `dotnet build DotnetDebug.sln`
   - `dotnet test --solution DotnetDebug.sln --no-build -- --disable-logo --no-progress`
   - `dotnet pack DotnetDebug.sln`

## 9. Проверки и команды
Точечные проверки до полного прогона:

```powershell
dotnet sln DotnetDebug.sln list
dotnet test tests/DotnetDebug.AppAutomation.Avalonia.Headless.Tests/DotnetDebug.AppAutomation.Avalonia.Headless.Tests.csproj
dotnet test tests/DotnetDebug.AppAutomation.FlaUI.Tests/DotnetDebug.AppAutomation.FlaUI.Tests.csproj
rg -n "namespace (EasyUse|FlaUI\\.EasyUse|FlaUI\\.Core)" src tests
rg -n "namespace FlaUI\\." src/AppAutomation.Avalonia.Headless
```

Полный прогон:

```powershell
dotnet build DotnetDebug.sln
dotnet test --solution DotnetDebug.sln --no-build -- --disable-logo --no-progress
dotnet pack DotnetDebug.sln
```

## 10. Открытые вопросы
- Полное переименование `DotnetDebug.Tests` в `DotnetDebug.Core.Tests` сознательно не входит в эту инициативу. Это отдельный косметический refactor без архитектурной необходимости.
- Если в процессе выяснится, что часть headless fake automation model нужна как public contract для уже существующих тестов, миграция будет уточнена до internal/public split без возврата `FlaUI.*` namespace-family.
- `AppAutomation.Session.Contracts` оставляем именно в таком виде, чтобы не потерять явность responsibility; дополнительное уплощение до `AppAutomation.Contracts` в эту инициативу не входит.

## 11. Результат прогона линтера
### 11.1 SPEC Linter Result

| Блок | Пункты | Статус | Комментарий |
| --- | --- | --- | --- |
| A. Полнота спеки | 1-5 | PASS | Цель, AS-IS, проблема, цели и non-goals зафиксированы |
| B. Качество дизайна | 6-10 | PASS | Целевая topology, namespace ownership и migration rules заданы |
| C. Безопасность изменений | 11-13 | PASS | Совместимость, rollout и rollback описаны явно |
| D. Проверяемость | 14-16 | PASS | Acceptance criteria и команды валидации измеримы |
| E. Готовность к автономной реализации | 17-19 | PASS | Есть phased plan и неблокирующие открытые вопросы |
| F. Соответствие профилю | 20 | PASS | Спека соответствует `dotnet-desktop-client` + `refactor-architecture` |

Итог: `ГОТОВО`

### 11.2 SPEC Rubric Result

| Критерий | Балл (0/2/5) | Обоснование |
| --- | --- | --- |
| 1. Ясность цели и границ | 5 | Сфокусировано на topology/naming cleanup без расползания в feature work |
| 2. Понимание текущего состояния | 5 | AS-IS опирается на реальную solution/folder/namespace картину |
| 3. Конкретность целевого дизайна | 5 | Зафиксированы `AppAutomation` rename map, project map, namespace ownership и folder rules |
| 4. Безопасность (миграция, откат) | 5 | Явно описаны rollout/rollback и rename boundary |
| 5. Тестируемость | 5 | Есть точечные и полные команды проверки, плюс measurable acceptance criteria |
| 6. Готовность к автономной реализации | 5 | Объём умеренный, риски локализованы, фазы независимы |

Итоговый балл: `30 / 30`
Зона: `готово к автономному выполнению`

Слабые места:
- rename всей framework family в `AppAutomation` является намеренно широким changeset и потребует аккуратной миграции project references, package ids и namespaces;
- breaking change по legacy headless namespaces осознанный и потребует аккуратной внутрипроектной миграции в одном changeset;
- структура `Avalonia.Headless.EasyUse` может раскрыть скрытые зависимости на fake-FlaUI surface, которые сейчас не видны по solution references.

## 12. Approval
Ожидается фраза: **"Спеку подтверждаю"**

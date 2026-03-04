# Инструкция: Скриншоты и Видео Окон Для Цикла Обратной Связи Агента

## Цель
Этот документ описывает, как агенту автоматически:
1. Найти нужное окно desktop-приложения.
2. Сделать скриншот конкретного окна.
3. Записать видео конкретного окна.
4. Проверить визуальный результат и использовать его в feedback loop.

## Что использовать
1. `nircmdc`:
Быстрый скриншот активного окна.
2. `ffmpeg`:
Скриншот и видео окна по заголовку.

Обе утилиты уже подходят для консольного использования в Windows.

## Быстрый старт
1. Скриншот активного окна (`nircmdc`):

```powershell
nircmdc.exe savescreenshotwin "C:\Temp\active-window.png"
```

2. Скриншот конкретного окна по точному заголовку (`ffmpeg`):

```powershell
ffmpeg -y -f gdigrab -framerate 1 -i "title=DotnetDebug - Visual Studio Code" -frames:v 1 -update 1 "C:\Temp\window.png"
```

3. Видео конкретного окна по точному заголовку (`ffmpeg`):

```powershell
ffmpeg -y -f gdigrab -framerate 15 -i "title=DotnetDebug - Visual Studio Code" -t 10 -c:v libx264 -preset veryfast -crf 23 -pix_fmt yuv420p "C:\Temp\window.mp4"
```

## Как получить точный заголовок окна
Используйте PowerShell:

```powershell
Get-Process |
  Where-Object { $_.MainWindowHandle -ne 0 -and $_.MainWindowTitle } |
  Select-Object ProcessName, Id, MainWindowTitle |
  Format-Table -AutoSize
```

Берите `MainWindowTitle` полностью и подставляйте в `-i "title=..."`.

## Рекомендуемый workflow для агента
1. Запустить приложение или UI-тест и дождаться появления окна.
2. Найти точный заголовок окна (`MainWindowTitle`).
3. Сделать скриншот окна.
4. При необходимости записать короткое видео (5-15 секунд).
5. Открыть изображение и проверить визуально соответствие ожидаемому состоянию.
6. Если есть отклонения, зафиксировать артефакты и повторить цикл после правок.

## Готовый скрипт для feedback loop
Скрипт ищет окно по части заголовка, сохраняет скриншот и 8-секундное видео.

```powershell
$partialTitle = "DotnetDebug"
$outDir = "C:\Temp\ui-feedback"
$timeoutSec = 30

New-Item -ItemType Directory -Path $outDir -Force | Out-Null
$stamp = Get-Date -Format "yyyyMMdd-HHmmss"
$pngPath = Join-Path $outDir "window-$stamp.png"
$mp4Path = Join-Path $outDir "window-$stamp.mp4"

$sw = [System.Diagnostics.Stopwatch]::StartNew()
$proc = $null
while ($sw.Elapsed.TotalSeconds -lt $timeoutSec) {
    $proc = Get-Process |
        Where-Object { $_.MainWindowHandle -ne 0 -and $_.MainWindowTitle -like "*$partialTitle*" } |
        Select-Object -First 1
    if ($proc) { break }
    Start-Sleep -Milliseconds 500
}

if (-not $proc) {
    throw "Окно с заголовком, содержащим '$partialTitle', не найдено за $timeoutSec сек."
}

$title = $proc.MainWindowTitle
Write-Host "Найдено окно: $title"

# Скриншот окна
ffmpeg -y -f gdigrab -framerate 1 -i "title=$title" -frames:v 1 -update 1 $pngPath | Out-Null

# Короткое видео окна
ffmpeg -y -f gdigrab -framerate 15 -i "title=$title" -t 8 -c:v libx264 -preset veryfast -crf 23 -pix_fmt yuv420p $mp4Path | Out-Null

Write-Host "PNG: $pngPath"
Write-Host "MP4: $mp4Path"
```

## Как агенту проверить результат
1. Скриншот:
Агент должен открыть сохраненный PNG и визуально сверить ключевые элементы UI.
Для Codex-агента используйте просмотр изображения по абсолютному пути (tool `view_image`).
2. Видео:
Агент использует MP4 для проверки переходов, анимаций, мерцаний и состояния контролов во времени.
Если агент не умеет напрямую просматривать MP4, извлеките кадры:

```powershell
ffmpeg -y -i "C:\Temp\window.mp4" -vf "fps=1" "C:\Temp\window-frame-%03d.png"
```
3. Практика:
Храните артефакты по timestamp и привязывайте их к конкретному шагу теста/промпта агента.

## Ограничения
1. `gdigrab` не захватывает окно, если оно свернуто.
2. Заголовок окна должен совпадать точно для `title=...`.
3. Для стабильной проверки фиксируйте размер окна и масштаб DPI.
4. В headless-среде без активной desktop-сессии захват может не работать.

## Минимальный набор команд
```powershell
# 1) список окон
Get-Process | ? { $_.MainWindowHandle -ne 0 -and $_.MainWindowTitle } | ft ProcessName,MainWindowTitle -Auto

# 2) скриншот активного окна
nircmdc.exe savescreenshotwin "C:\Temp\active-window.png"

# 3) скриншот конкретного окна
ffmpeg -y -f gdigrab -framerate 1 -i "title=TOOL_WINDOW_TITLE" -frames:v 1 -update 1 "C:\Temp\window.png"

# 4) видео конкретного окна
ffmpeg -y -f gdigrab -framerate 15 -i "title=TOOL_WINDOW_TITLE" -t 10 -c:v libx264 -preset veryfast -crf 23 -pix_fmt yuv420p "C:\Temp\window.mp4"
```

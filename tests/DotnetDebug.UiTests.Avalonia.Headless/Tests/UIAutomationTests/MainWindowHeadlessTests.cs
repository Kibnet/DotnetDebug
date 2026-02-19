using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Interactivity;
using DotnetDebug.Avalonia;
using DotnetDebug.UiTests.Avalonia.Headless.Infrastructure;
using TUnit.Assertions;
using TUnit.Core;

namespace DotnetDebug.UiTests.Avalonia.Headless.Tests.UIAutomationTests;

public sealed class MainWindowFlaUIEasyUseTests
{
    private const string HeadlessUiConstraint = "AvaloniaHeadlessUi";

    [Test]
    [NotInParallel(HeadlessUiConstraint)]
    public async Task Calculate_Gcd_WithDefaultSettings_ShowsResultStepsAndHistory()
    {
        var (initialHistoryItems, state) = await RunScenarioAsync<(int, MainWindowState)>(mainWindow =>
        {
            var startState = ReadList(mainWindow, "HistoryList");
            SetText(mainWindow, "NumbersInput", "48 18 30");
            SelectComboItem(mainWindow, "OperationCombo", "GCD");
            SetChecked(mainWindow, "ShowStepsCheck", true);
            Click(mainWindow, "CalculateButton");

            return (startState.Length, ReadState(mainWindow));
        });

        using (Assert.Multiple())
        {
            await Assert.That(state.ResultText).IsEqualTo("GCD = 6");
            await Assert.That(state.ErrorText).IsEqualTo(string.Empty);
            await Assert.That(state.ModeLabel).Contains("Mode: GCD");
            await Assert.That(state.ModeLabel).Contains("Steps: On");
            await Assert.That(state.StepsCount).IsGreaterThan(0);
            await Assert.That(state.HistoryItems.Length).IsGreaterThanOrEqualTo(initialHistoryItems + 1);
        }
    }

    [Test]
    [NotInParallel(HeadlessUiConstraint)]
    public async Task Calculate_Lcm_UsesNegativeAndAbsoluteOption()
    {
        var state = await RunScenarioAsync(mainWindow =>
        {
            var initialHistory = ReadList(mainWindow, "HistoryList");
            SetText(mainWindow, "NumbersInput", "-4 8 12");
            SelectComboItem(mainWindow, "OperationCombo", "LCM");
            SetChecked(mainWindow, "UseAbsoluteValuesCheck", true);
            SetChecked(mainWindow, "ShowStepsCheck", false);
            Click(mainWindow, "CalculateButton");
            var finalState = ReadState(mainWindow);
            finalState = finalState with
            {
                InitialHistoryItems = initialHistory.Length
            };
            return finalState;
        });

        using (Assert.Multiple())
        {
            await Assert.That(state.ResultText).IsEqualTo("LCM = 24");
            await Assert.That(state.ModeLabel).Contains("Mode: LCM");
            await Assert.That(state.ModeLabel).Contains("Absolute: On");
            await Assert.That(state.ErrorText).IsEqualTo(string.Empty);
            await Assert.That(state.HistoryItems.Length).IsGreaterThanOrEqualTo(state.InitialHistoryItems + 1);
        }
    }

    [Test]
    [NotInParallel(HeadlessUiConstraint)]
    public async Task Calculate_Min_RespectsAbsoluteCheckbox()
    {
        var initialState = await RunScenarioAsync(mainWindow =>
        {
            SelectComboItem(mainWindow, "OperationCombo", "MIN");
            SetChecked(mainWindow, "UseAbsoluteValuesCheck", false);
            SetText(mainWindow, "NumbersInput", "-10 2 5");
            Click(mainWindow, "CalculateButton");
            var firstResult = ReadState(mainWindow);

            SetChecked(mainWindow, "UseAbsoluteValuesCheck", true);
            Click(mainWindow, "CalculateButton");
            var secondResult = ReadState(mainWindow);

            return (firstResult, secondResult);
        });

        await Assert.That(initialState.Item1.ResultText).IsEqualTo("MIN = -10");
        await Assert.That(initialState.Item2.ResultText).IsEqualTo("MIN = 2");
    }

    [Test]
    [NotInParallel(HeadlessUiConstraint)]
    public async Task Calculate_InvalidInput_ShowsError_NoHistory()
    {
        var (initialHistoryItems, state) = await RunScenarioAsync<(int, MainWindowState)>(mainWindow =>
        {
            var initialHistory = ReadList(mainWindow, "HistoryList");
            SelectComboItem(mainWindow, "OperationCombo", "GCD");
            SetText(mainWindow, "NumbersInput", "48 x 30");
            Click(mainWindow, "CalculateButton");
            return (initialHistory.Length, ReadState(mainWindow));
        });

        await Assert.That(state.ErrorText).Contains("Invalid integer: x");
        await Assert.That(state.ResultText).IsEqualTo(string.Empty);
        await Assert.That(state.HistoryItems.Length).IsEqualTo(initialHistoryItems);
    }

    [Test]
    [NotInParallel(HeadlessUiConstraint)]
    public async Task FilterHistory_ByText_ShowsOnlyMatchingItems()
    {
        var result = await RunScenarioAsync(mainWindow =>
        {
            SetText(mainWindow, "NumbersInput", "48 18 30");
            SelectComboItem(mainWindow, "OperationCombo", "GCD");
            Click(mainWindow, "CalculateButton");

            SetText(mainWindow, "NumbersInput", "4 8 12");
            SelectComboItem(mainWindow, "OperationCombo", "LCM");
            SetChecked(mainWindow, "UseAbsoluteValuesCheck", true);
            Click(mainWindow, "CalculateButton");

            SetText(mainWindow, "HistoryFilterInput", "LCM");
            Click(mainWindow, "ApplyFilterButton");
            var filtered = ReadList(mainWindow, "HistoryList");

            SetText(mainWindow, "HistoryFilterInput", string.Empty);
            Click(mainWindow, "ApplyFilterButton");
            var cleared = ReadList(mainWindow, "HistoryList");

            Click(mainWindow, "ClearHistoryButton");
            var afterClear = ReadList(mainWindow, "HistoryList");

            return (FilteredHistory: filtered, ClearedHistory: cleared, ClearedHistoryItems: afterClear);
        });

        await Assert.That(result.FilteredHistory.Length).IsGreaterThan(0);
        await Assert.That(result.FilteredHistory.All(item => item.Contains("LCM", StringComparison.Ordinal))).IsEqualTo(true);
        await Assert.That(result.ClearedHistory.Length).IsGreaterThanOrEqualTo(2);
        await Assert.That(result.ClearedHistoryItems.Length).IsEqualTo(0);
    }

    [Test]
    [NotInParallel(HeadlessUiConstraint)]
    public async Task ControlMix_SliderSpinnerRadioToggle_BuildsSeriesAndShowsProgress()
    {
        var state = await RunScenarioAsync(mainWindow =>
        {
            SelectTab(mainWindow, "ControlMixTabItem");
            SelectComboItem(mainWindow, "MixModeCombo", "Fibonacci");
            SetChecked(mainWindow, "MixShowDetailsCheck", true);
            SetChecked(mainWindow, "MixDirectionAscendingRadio", true);
            SetChecked(mainWindow, "MixDirectionDescendingRadio", false);
            SetChecked(mainWindow, "MixAdvancedToggle", true);
            SetText(mainWindow, "MixCountSpinner", "10");
            SetSlider(mainWindow, "MixSpeedSlider", 4);
            SetText(mainWindow, "MixInput", "1 2");
            Click(mainWindow, "MixRunButton");

            return ReadState(mainWindow);
        });

        await Assert.That(state.SeriesResult).Contains("Series[Fibonacci]");
        await Assert.That(state.SeriesResult).Contains("count=10");
        await Assert.That(state.SeriesItems.FirstOrDefault() ?? string.Empty).Contains("Advanced mode on");
        await Assert.That(state.SeriesItems.Any(item => item.Contains("v1:", StringComparison.Ordinal))).IsEqualTo(true);
        await Assert.That(state.SeriesItems.Any(item => item.Contains("v3:", StringComparison.Ordinal))).IsEqualTo(true);
    }

    [Test]
    [NotInParallel(HeadlessUiConstraint)]
    public async Task DateTime_InvalidRange_ShowsValidation()
    {
        var state = await RunScenarioAsync(mainWindow =>
        {
            SelectTab(mainWindow, "DateTimeTabItem");
            SetDate(mainWindow, "StartDatePicker", DateTime.Today);
            SetDate(mainWindow, "EndDatePicker", DateTime.Today.AddDays(-2));
            Click(mainWindow, "DateDiffButton");
            return ReadState(mainWindow);
        });

        await Assert.That(state.DateErrorText).Contains("End date should be greater than or equal to start date");
            await Assert.That(state.DateDiffItems.Length).IsEqualTo(0);
    }

    [Test]
    [NotInParallel(HeadlessUiConstraint)]
    public async Task DateTime_ComputeDateDifference_ByDatePickers()
    {
        var state = await RunScenarioAsync(mainWindow =>
        {
            SelectTab(mainWindow, "DateTimeTabItem");
            SetDate(mainWindow, "StartDatePicker", DateTime.Today.AddDays(-7));
            SetDate(mainWindow, "EndDatePicker", DateTime.Today);
            Click(mainWindow, "DateDiffButton");
            return ReadState(mainWindow);
        });

        await Assert.That(state.DateResult).Contains("Date difference = 7 days");
        await Assert.That(state.DateDiffItems.Length).IsGreaterThanOrEqualTo(5);
        await Assert.That(state.DateDiffItems.FirstOrDefault() ?? string.Empty).Contains("Start:");
    }

    [Test]
    [NotInParallel(HeadlessUiConstraint)]
    public async Task TabControl_NavigatesAcrossTabs_WithExpectedControls()
    {
        var state = await RunScenarioAsync(mainWindow =>
        {
            SelectTab(mainWindow, "ControlMixTabItem");
            SelectTab(mainWindow, "HierarchyTabItem");
            SelectTab(mainWindow, "DateTimeTabItem");
            SetDate(mainWindow, "StartDatePicker", DateTime.Today.AddDays(-2));
            SetDate(mainWindow, "EndDatePicker", DateTime.Today);
            Click(mainWindow, "DateDiffButton");
            SelectTab(mainWindow, "MathTabItem");
            SelectComboItem(mainWindow, "OperationCombo", "GCD");
            SetText(mainWindow, "NumbersInput", "6 9");
            Click(mainWindow, "CalculateButton");
            return ReadState(mainWindow);
        });

        await Assert.That(state.ResultText).IsEqualTo("GCD = 3");
        await Assert.That(state.ModeLabel).Contains("Mode: GCD");
    }

    [Test]
    [NotInParallel(HeadlessUiConstraint)]
    public async Task Hierarchy_SelectTreeItem_ShowsSelectionInResult()
    {
        var (stateWithSelection, stateAfterClear) = await RunScenarioAsync(mainWindow =>
        {
            SelectTab(mainWindow, "HierarchyTabItem");
            SelectTreeItem(mainWindow, "FibonacciTreeNode");
            var beforeClear = ReadState(mainWindow);
            Click(mainWindow, "HierarchyClearSelectionButton");
            var afterClear = ReadState(mainWindow);
            return (beforeClear, afterClear);
        });

        await Assert.That(stateWithSelection.HierarchyResultLabelText).Contains("Fibonacci");
        await Assert.That(stateWithSelection.HierarchySelectionItems.Length).IsGreaterThanOrEqualTo(2);
        await Assert.That(stateAfterClear.HierarchyResultLabelText).IsEqualTo("No node selected");
        await Assert.That(stateAfterClear.HierarchySelectionItems.Length).IsEqualTo(0);
    }

    private static async Task<T> RunScenarioAsync<T>(Func<MainWindow, T> scenario)
    {
        return await HeadlessSessionHooks.Session.Dispatch(
            () =>
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();
                try
                {
                    return scenario(mainWindow);
                }
                finally
                {
                    mainWindow.Close();
                }
            },
            CancellationToken.None);
    }

    private static MainWindowState ReadState(MainWindow mainWindow)
    {
        return new MainWindowState(
            ResultText: ReadText(mainWindow, "ResultText"),
            ErrorText: ReadText(mainWindow, "ErrorText"),
            ModeLabel: ReadText(mainWindow, "ModeLabel"),
            StepsCount: ReadList(mainWindow, "StepsList").Length,
            Steps: ReadList(mainWindow, "StepsList").ToArray(),
            HistoryItems: ReadList(mainWindow, "HistoryList").ToArray(),
            SeriesResult: ReadText(mainWindow, "SeriesResult"),
            SeriesItems: ReadList(mainWindow, "SeriesList").ToArray(),
            DateResult: ReadText(mainWindow, "DateResult"),
            DateDiffItems: ReadList(mainWindow, "DateDiffList").ToArray(),
            DateErrorText: ReadText(mainWindow, "DateErrorText"),
            HierarchyResultLabelText: ReadText(mainWindow, "HierarchyResultLabel"),
            HierarchySelectionItems: ReadList(mainWindow, "HierarchySelectionList").ToArray()
        );
    }

    private static string ReadText(MainWindow mainWindow, string controlName)
    {
        var control = GetControl<Control>(mainWindow, controlName);
        return control switch
        {
            TextBlock textBlock => textBlock.Text ?? string.Empty,
            Label label => label.Content?.ToString() ?? string.Empty,
            _ => throw new InvalidOperationException($"Control '{controlName}' does not expose text."),
        };
    }

    private static string[] ReadList(MainWindow mainWindow, string controlName)
    {
        var list = GetControl<ListBox>(mainWindow, controlName);
        return list.Items?.OfType<object>()
            .Select(item => item?.ToString() ?? string.Empty)
            .ToArray() ?? Array.Empty<string>();
    }

    private static void SetText(MainWindow mainWindow, string controlName, string value)
    {
        var textBox = GetControl<TextBox>(mainWindow, controlName);
        textBox.Text = value;
    }

    private static void SetChecked(MainWindow mainWindow, string controlName, bool value)
    {
        var control = GetControl<ToggleButton>(mainWindow, controlName);
        control.IsChecked = value;
    }

    private static void Click(MainWindow mainWindow, string controlName)
    {
        var button = GetControl<Button>(mainWindow, controlName);
        button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
    }

    private static void SelectComboItem(MainWindow mainWindow, string controlName, string option)
    {
        var combo = GetControl<ComboBox>(mainWindow, controlName);
        var options = combo.Items?.OfType<object>().ToArray() ?? [];
        for (var index = 0; index < options.Length; index++)
        {
            if (string.Equals(options[index]?.ToString(), option, StringComparison.Ordinal))
            {
                combo.SelectedIndex = index;
                return;
            }
        }

        throw new InvalidOperationException($"ComboBox '{controlName}' does not contain option '{option}'.");
    }

    private static void SelectTab(MainWindow mainWindow, string tabControlName)
    {
        var tabs = GetControl<TabControl>(mainWindow, "MainTabs");
        var tab = GetControl<TabItem>(mainWindow, tabControlName);
        tabs.SelectedItem = tab;
    }

    private static void SetDate(MainWindow mainWindow, string controlName, DateTime date)
    {
        var picker = GetControl<DatePicker>(mainWindow, controlName);
        picker.SelectedDate = date;
    }

    private static void SetSlider(MainWindow mainWindow, string controlName, int value)
    {
        var slider = GetControl<Slider>(mainWindow, controlName);
        slider.Value = value;
    }

    private static void SelectTreeItem(MainWindow mainWindow, string controlName)
    {
        var tree = GetControl<TreeView>(mainWindow, "DemoTree");
        var item = GetControl<TreeViewItem>(mainWindow, controlName);
        item.IsSelected = true;
        tree.SelectedItem = item;
    }

    private static T GetControl<T>(MainWindow mainWindow, string controlName)
        where T : Control
    {
        return mainWindow.FindControl<T>(controlName) ??
               throw new InvalidOperationException($"Control '{controlName}' was not found.");
    }

    private sealed record MainWindowState(
        string ResultText,
        string ErrorText,
        string ModeLabel,
        int StepsCount,
        string[] Steps,
        string[] HistoryItems,
        string SeriesResult,
        string[] SeriesItems,
        string DateResult,
        string[] DateDiffItems,
        string DateErrorText,
        string HierarchyResultLabelText,
        string[] HierarchySelectionItems)
    {
        public int InitialHistoryItems { get; init; }
    }
}

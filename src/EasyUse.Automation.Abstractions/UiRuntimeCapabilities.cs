namespace EasyUse.Automation.Abstractions;

public sealed record UiRuntimeCapabilities(
    string AdapterId,
    bool SupportsGridCellAccess = false,
    bool SupportsCalendarRangeSelection = false,
    bool SupportsTreeNodeExpansionState = false,
    bool SupportsRawNativeHandles = false,
    bool SupportsScreenshots = false);

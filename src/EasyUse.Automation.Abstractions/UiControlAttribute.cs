namespace EasyUse.Automation.Abstractions;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class UiControlAttribute(string propertyName, UiControlType controlType, string locatorValue) : Attribute
{
    public string PropertyName { get; } = propertyName;

    public UiControlType ControlType { get; } = controlType;

    public string LocatorValue { get; } = locatorValue;

    public UiLocatorKind LocatorKind { get; init; } = UiLocatorKind.AutomationId;

    public bool FallbackToName { get; init; } = true;
}

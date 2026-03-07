namespace EasyUse.Automation.Abstractions;

public sealed record UiControlDefinition(
    string PropertyName,
    UiControlType ControlType,
    string LocatorValue,
    UiLocatorKind LocatorKind = UiLocatorKind.AutomationId,
    bool FallbackToName = true);

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

namespace EasyUse.Automation.Abstractions;

public interface IUiControlResolver
{
    UiRuntimeCapabilities Capabilities { get; }

    TControl Resolve<TControl>(UiControlDefinition definition)
        where TControl : class;
}

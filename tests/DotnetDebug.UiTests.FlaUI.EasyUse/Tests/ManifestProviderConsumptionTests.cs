using System.Linq;
using DotnetDebug.UiTests.Authoring.Generated;
using EasyUse.Automation.Abstractions;
using TUnit.Assertions;
using TUnit.Core;

namespace DotnetDebug.UiTests.FlaUI.EasyUse.Tests.UIAutomationTests;

public class ManifestProviderConsumptionTests
{
    [Test]
    public async Task GeneratedManifestProvider_ExposesMainWindowContract()
    {
        IUiLocatorManifestProvider provider = new DotnetDebugUiTestsAuthoringManifestProvider();
        var manifest = provider.GetManifest();

        var page = manifest.Pages.Single(candidate => candidate.PageName == "MainWindowPage");
        var resultText = page.Controls.Single(candidate => candidate.PropertyName == "ResultText");

        using (Assert.Multiple())
        {
            await Assert.That(manifest.ContractVersion).IsEqualTo("1");
            await Assert.That(page.PageTypeFullName).Contains("DotnetDebug.UiTests.Authoring.Pages.MainWindowPage");
            await Assert.That(resultText.ControlType).IsEqualTo(UiControlType.Label);
            await Assert.That(resultText.LocatorValue).IsEqualTo("ResultText");
        }
    }
}

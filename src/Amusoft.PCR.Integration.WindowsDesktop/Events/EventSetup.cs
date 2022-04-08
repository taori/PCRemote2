using System.Diagnostics;
using System.Threading.Tasks;
using Amusoft.PCR.Integration.WindowsDesktop.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Amusoft.PCR.Integration.WindowsDesktop.Events;

public static class EventSetup
{
    public static void Initialize()
    {
    }

    [Conditional("DEBUG")]
    public static async void Debug()
    {
        // todo refactoring. Knowing all related types would be bad design to get a reply for a request
        var request = new GetPromptTextRequest("Prompt", "Please enter a valid value", "Password");
        var response = await ViewModelSpawner.GetResponseAsync<PromptWindow, PromptWindowModel, GetPromptTextRequest, PromptCompleted>(request);
    }
}
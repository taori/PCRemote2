using System;
using System.ComponentModel.DataAnnotations;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Amusoft.PCR.Integration.WindowsDesktop.Events;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Amusoft.PCR.Integration.WindowsDesktop.Windows
{
    /// <summary>
    /// Interaktionslogik für PromptWindow.xaml
    /// </summary>
    public partial class PromptWindow
    {
        public PromptWindow()
        {
            InitializeComponent();
        }
    }

    public class PromptCompleted
    {
        public bool Cancelled { get; init; }
        public string Content { get; init; }
    }

    public partial class PromptWindowModel : ObservableValidator, IRecipient<GetPromptTextRequest>
    {
        public PromptWindowModel()
        {
            WeakReferenceMessenger.Default.RegisterAll(this);
            ValidateAllProperties();
        }

        [ObservableProperty]
        private bool _isOpen;
        
        [ObservableProperty]
        private string _title;
        
        [ObservableProperty]
        [Required]
        [MinLength(3)]
        [AlsoNotifyCanExecuteFor(nameof(ConfirmAsyncCommand))]
        private string _value = string.Empty;

        [ObservableProperty]
        private string _description;

        [ObservableProperty]
        private string _watermarkValue;
        
        private readonly TaskCompletionSource<PromptCompleted> _completion = new();

        private bool CanConfirm() => !GetErrors(nameof(Value)).Any();

        [ICommand(CanExecute = nameof(CanConfirm))]
        public void ConfirmAsync()
        {
	        WeakReferenceMessenger.Default.UnregisterAll(this);
            _completion.TrySetResult(new PromptCompleted()
            {
                Cancelled = false,
                Content = _value
            });

            IsOpen = false;
        }

        [ICommand]
        public void CancelAsync()
        {
	        WeakReferenceMessenger.Default.UnregisterAll(this);
            _completion.TrySetResult(new PromptCompleted()
            {
                Cancelled = true,
                Content = string.Empty
            });

            IsOpen = false;
        }

        public void Receive(GetPromptTextRequest message)
        {
            Title = message.Title;
            Description = message.Description;
            WatermarkValue = message.WatermarkValue;

            message.Reply(_completion.Task);
        }
    }
}

using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Amusoft.PCR.Integration.WindowsDesktop.Events;

public static class ViewModelSpawner
{
    public static Task<TResponse> GetResponseAsync<TWindow, TModel, TRequest, TResponse>(TRequest request)
        where TWindow : Window, new()
        where TModel : IRecipient<TRequest>, new()
        where TRequest : AsyncRequestMessage<TResponse>
        where TResponse : class
    {
        var tcs = new TaskCompletionSource<TResponse>();
        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
        {
            if (Application.Current.MainWindow == null)
                return;

            var window = new TWindow();
            var model = new TModel();
            window.DataContext = model;

            window.Show();
            WeakReferenceMessenger.Default.Send(request);
            
            request.Response.ToObservable()
                .ObserveOnDispatcher()
                .Subscribe(d =>
                {
                    tcs.TrySetResult(d);
                    window.Close();
                });
        }));

        return tcs.Task;
    }
}
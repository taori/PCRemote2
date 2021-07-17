using System;
using System.Threading;
using Xamarin.Essentials;

namespace Amusoft.PCR.Mobile.Droid.Domain.Common
{
	public static class GrpcRequestObserver
	{
		private static int _runningCalls;

		public static event EventHandler<int> CallRunning;
		public static event EventHandler<Exception> CallFailed;
		public static event EventHandler<int> CallFinished;

		public static void NotifyCallRunning()
		{
			Interlocked.Increment(ref _runningCalls);
			MainThread.BeginInvokeOnMainThread(() => CallRunning?.Invoke(null, _runningCalls));
		}

		public static void NotifyCallFailed(Exception exception)
		{
			MainThread.BeginInvokeOnMainThread(() => CallFailed?.Invoke(null, exception));
		}

		public static void NotifyCallFinished()
		{
			Interlocked.Decrement(ref _runningCalls);
			MainThread.BeginInvokeOnMainThread(() => CallFinished?.Invoke(null, _runningCalls));
		}
	}
}
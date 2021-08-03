using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Amusoft.Toolkit.UI
{
	public static class Debouncer
	{
		static readonly ConcurrentDictionary<string, CancellationTokenSource> Tokens = new ConcurrentDictionary<string, CancellationTokenSource>();

		public static void Debounce(string uniqueKey, Action action, TimeSpan delay)
		{
			Debug.WriteLine($"Current Thread: {Thread.CurrentThread.ManagedThreadId}");
			var scheduler = TaskScheduler.FromCurrentSynchronizationContext();

			var token = Tokens.AddOrUpdate(uniqueKey,
				(key) =>  new CancellationTokenSource(),
				(key, existingToken) => 
				{
					//key found - cancel task and recreate
					existingToken.Cancel();
					return new CancellationTokenSource();
				}
			);

			Task.Delay(delay, token.Token).ContinueWith(task =>
			{
				if (!task.IsCanceled)
				{
					Debug.WriteLine($"Current Thread: {Thread.CurrentThread.ManagedThreadId}");
					action();
					Tokens.TryRemove(uniqueKey, out _);
					// cts.Dispose();
				}
			}, token.Token, TaskContinuationOptions.None, scheduler);
		}
	}
}
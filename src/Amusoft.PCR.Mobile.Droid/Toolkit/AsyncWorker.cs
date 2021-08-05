using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using AndroidX.Concurrent.Futures;
using AndroidX.Work;
using Google.Common.Util.Concurrent;
using Java.Lang;
using NLog;
using Exception = System.Exception;
using Logger = NLog.Logger;

namespace Amusoft.PCR.Mobile.Droid.Toolkit
{
	public abstract class AsyncWorker : ListenableWorker, CallbackToFutureAdapter.IResolver
	{
		private static readonly Logger Log = LogManager.GetLogger(nameof(AsyncWorker));

		private readonly CancellationTokenSource _cts;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_cts.Dispose();
			}

			base.Dispose(disposing);
		}

		protected AsyncWorker(Context appContext, WorkerParameters workerParams) : base(appContext, workerParams)
		{
			_cts = new CancellationTokenSource();
		}

		public override IListenableFuture StartWork()
		{
			return CallbackToFutureAdapter.GetFuture(this);
		}

		protected abstract object GetCompletionTag();

		Object CallbackToFutureAdapter.IResolver.AttachCompleter(CallbackToFutureAdapter.Completer p0)
		{
			try
			{
				var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
				Task.Run(async () =>
					{
						Log.Debug("Starting Worker {Name}", GetType().FullName);
						return await DoWorkAsync(_cts.Token);
					}, _cts.Token)
					.ContinueWith(previous =>
					{
						if (previous.IsCompletedSuccessfully)
						{
							Log.Trace(previous.Exception, "Worker {Name} completed successfully", GetType().FullName);
							p0.Set(Result.InvokeSuccess(previous.Result));
						}
						else
						{
							if (previous.IsCanceled)
							{
								Log.Trace(previous.Exception, "Worker {Name} was cancelled", GetType().FullName);
								p0.SetCancelled();
							}

							if (previous.IsFaulted)
							{
								Log.Error(previous.Exception, "Worker {Name} raised an exception", GetType().FullName);
								p0.SetException(new Error(previous.Exception?.ToString() ?? "Unknown cause"));
							}
						}
					}, _cts.Token, TaskContinuationOptions.None, scheduler);
			}
			catch (Exception e)
			{
				Log.Error(e, "Worker {Name} raised an exception", GetType().FullName);
			}

			return GetCompletionTag() as Object;
		}

		public override void OnStopped()
		{
			_cts.Cancel(false);
			base.OnStopped();
		}

		protected abstract Task<Data> DoWorkAsync(CancellationToken cancellationToken);
	}
}
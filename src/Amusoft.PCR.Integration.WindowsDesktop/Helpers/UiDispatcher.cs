using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Amusoft.PCR.Integration.WindowsDesktop.Helpers
{
	public static class UI
	{
		public static SynchronizationContextAwaiter Thread =>
			new(SynchronizationContext.Current);
	}

    public struct SynchronizationContextAwaiter : INotifyCompletion
	{
		private static readonly SendOrPostCallback _postCallback = state => ((Action)state)();

		private readonly SynchronizationContext _context;
		public SynchronizationContextAwaiter(SynchronizationContext context)
		{
			_context = context;
		}

		public SynchronizationContextAwaiter GetAwaiter() { return this; }

		public bool IsCompleted => _context == SynchronizationContext.Current;

		public void OnCompleted(Action continuation) => _context.Post(_postCallback, continuation);

		public void GetResult() { }
	}
}
using System;
using System.Collections.Generic;

namespace Amusoft.PCR.Mobile.Droid.Domain.Common
{
	public static class BackStackHandler
	{
		private static readonly Stack<WeakReference<Action>> Actions = new Stack<WeakReference<Action>>();

		public static void Add(Action action)
		{
			Actions.Push(new WeakReference<Action>(action));
		}

		public static bool PopCall()
		{
			bool alive;
			do
			{
				if (!Actions.TryPop(out var weakReference))
					return false;

				alive = weakReference.TryGetTarget(out var action);
				if (alive)
				{
					action.Invoke();
					return true;
				}
			} while (alive);

			return false;
		}
	}
}
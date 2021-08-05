using System;
using System.Threading.Tasks;
using Amusoft.PCR.Mobile.Droid.Domain.Common;
using Android.Content;
using AndroidX.Work;

namespace Amusoft.PCR.Mobile.Droid.Domain.Server.SystemStateControl
{
	public class SystemStateWorkRequestFactory
	{
		public static async Task<string> EnqueueAsync<TWorker>(Context context, string address, SystemStateKind stateKind, DateTime date)
			where TWorker : DelayedSystemStateWorker
		{
			await SystemStateManager.SetScheduledTimeAsync(address, stateKind, date);
			var workRequest =
				WorkerRequestFactory.CreateOneTime<TWorker>(
				workRequestMutator: builder =>
				{
					builder.AddTag(address);
				},
				inputData: data =>
				{
					data.PutString(DelayedSystemStateWorker.AgentAddressTag, address);
				}, 
				constraintsMutator: constraints =>
				{
					constraints.SetRequiredNetworkType(NetworkType.Connected);
				});

			var uniqueWorkName = stateKind + "+" + address;
			WorkManager.GetInstance(context).EnqueueUniqueWork(uniqueWorkName, ExistingWorkPolicy.Replace, workRequest);
			return uniqueWorkName;
		}
	}
}
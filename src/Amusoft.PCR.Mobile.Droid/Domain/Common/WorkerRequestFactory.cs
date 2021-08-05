using System;
using AndroidX.Work;

namespace Amusoft.PCR.Mobile.Droid.Domain.Common
{
	public static class WorkerRequestFactory
	{
		public static OneTimeWorkRequest CreateOneTime<TWorker>(Action<OneTimeWorkRequest.Builder> workRequestMutator = null, Action<Constraints.Builder> constraintsMutator = null, Action<Data.Builder> inputData = null)
		{
			var requestBuilder = new OneTimeWorkRequest.Builder(typeof(TWorker));

			if (inputData != default)
			{
				var dataBuilder = new Data.Builder();
				inputData.Invoke(dataBuilder);
				requestBuilder.SetInputData(dataBuilder.Build());
			}
			
			workRequestMutator?.Invoke(requestBuilder);

			if (constraintsMutator == null)
			{
				requestBuilder.SetConstraints(Constraints.None);
			}
			else
			{
				var constraintsBuilder = new Constraints.Builder();
				constraintsMutator?.Invoke(constraintsBuilder);
				requestBuilder.SetConstraints(constraintsBuilder.Build());
			}

			return requestBuilder.Build();
		}
	}
}
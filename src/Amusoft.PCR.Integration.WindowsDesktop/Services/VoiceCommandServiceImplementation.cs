using System;
using System.Threading.Tasks;
using Amusoft.PCR.Grpc.Common;
using Amusoft.PCR.Integration.WindowsDesktop.Feature.VoiceCommands;
using Grpc.Core;
using NLog;

namespace Amusoft.PCR.Integration.WindowsDesktop.Services
{
	public class VoiceCommandServiceImplementation : VoiceCommandService.VoiceCommandServiceBase
	{
		private static readonly Logger Log = LogManager.GetLogger(nameof(VoiceCommandServiceImplementation));

		public override Task<DefaultResponse> UpdateVoiceRecognition(UpdateVoiceRecognitionRequest request, ServerCallContext context)
		{
			try
			{
				SpeechManager.Instance.UpdateGrammar(request);
				return Task.FromResult(new DefaultResponse() {Success = true});
			}
			catch (Exception e)
			{
				Log.Error(e, nameof(UpdateVoiceRecognition));

				throw new RpcException(Status.DefaultCancelled, "Failed to update voice recognition");
			}
		}
	}
}
using System.Linq;
using System.Threading.Tasks;

namespace Amusoft.PCR.Domain.VoiceRecognition
{
	public interface IVoiceCommandBinder
	{
		bool CanHandle(ExpandedVoiceCommand command);

		Task ExecuteAsync(ExpandedVoiceCommand command);
	}
}
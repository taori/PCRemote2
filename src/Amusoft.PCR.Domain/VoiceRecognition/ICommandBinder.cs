using System.Linq;
using System.Threading.Tasks;

namespace Amusoft.PCR.Domain.VoiceRecognition
{
	interface ICommandBinder
	{
		bool TryBind(string command, ILookup<string, string> aliasLookup);

		Task ExecuteAsync();
	}
}
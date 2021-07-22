namespace Amusoft.PCR.Grpc.Common
{
	public class CheckState
	{
		public CheckState(string title, bool @checked, string id)
		{
			Title = title;
			Checked = @checked;
			Id = id;
		}

		public string Title { get; set; }

		public bool Checked { get; set; }

		public string Id { get; set; }
	}
}
namespace IndustrialFurnace
{
	public class RequestSaveData
	{
		public long PlayerID { get; set; }


		public RequestSaveData(long playerID)
		{
			PlayerID = playerID;
		}
	}
}

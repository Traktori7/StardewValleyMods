namespace IndustrialFurnace.Data
{
	/// <summary>
	/// The data class for blueprint's item requirements.
	/// </summary>
	public class RequiredItem
	{
		public string? ItemName { get; set; }
		public int ItemAmount { get; set; }
		public int ItemID { get; set; }


		public override string ToString()
		{
			return $"{ItemID} {ItemAmount}";
		}
	}
}

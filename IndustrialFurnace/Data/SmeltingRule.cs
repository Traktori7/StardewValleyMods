namespace IndustrialFurnace.Data
{
	/// <summary>
	/// The data class for a smelting rule.
	/// </summary>
	public class SmeltingRule
	{
		public int InputItemID { get; set; }
		public int InputItemAmount { get; set; }
		public int OutputItemID { get; set; }
		public int OutputItemAmount { get; set; }
		public string[]? RequiredModID { get; set; }
	}
}

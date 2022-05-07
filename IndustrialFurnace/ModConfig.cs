namespace IndustrialFurnace
{
	/// <summary>
	/// The config class.
	/// </summary>
	public class ModConfig
	{
		public int CoalAmount { get; set; } = 5;
		public bool InstantSmelting { get; set; } = false;
		public bool EnableSmokeAnimation { get; set; } = true;
		public bool EnableFireAnimation { get; set; } = true;
	}
}

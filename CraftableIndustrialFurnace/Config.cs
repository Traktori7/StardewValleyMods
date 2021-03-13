using StardewModdingAPI;

namespace CraftableIndustrialFurnace
{
	class Config
	{
		public SButton debugKey { get; set; }

		public Config()
		{
			debugKey = SButton.J;
		}
	}
}

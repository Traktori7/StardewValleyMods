namespace IndustrialFurnace.Data
{
	public class SmokeAnimation
	{
		public bool UseCustomSprite { get; set; } = false;
		public uint SpawnFrequency { get; set; } = 500;
		public int SpawnXOffset { get; set; } = 68;
		public int SpawnYOffset { get; set; } = -64;
		public int SpriteSizeX { get; set; } = 10;
		public int SpriteSizeY { get; set; } = 10;
		public float SmokeScale { get; set; } = 2;
		public float SmokeScaleChange { get; set; } = 0.02f;
	}
}

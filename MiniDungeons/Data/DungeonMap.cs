namespace MiniDungeons.Data
{
	internal class DungeonMap
	{
		public string MapFile { get; set; } = string.Empty;
		public int SpawnWeight { get; set; }
		public int EntryX { get; set; }
		public int EntryY { get; set; }
		public string Challenge { get; set; } = string.Empty;
	}
}

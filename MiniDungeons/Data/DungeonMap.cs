namespace MiniDungeons.Data
{
	internal class DungeonMap
	{
		internal string MapFile { get; set; } = string.Empty;
		internal int SpawnWeight { get; set; }
		public int EntryX { get; set; }
		public int EntryY { get; set; }
		public string Challenge { get; set; } = string.Empty;
	}
}

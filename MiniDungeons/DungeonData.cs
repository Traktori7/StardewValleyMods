using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;


namespace MiniDungeons
{
	internal class DungeonData
	{
		public string DungeonName { get; set; } = string.Empty;
		public string SpawnMapName { get; set; } = string.Empty;
		public float SpawnChance { get; set; } = 0f;
		public int PortalX { get; set; }
		public int PortalY { get; set; }
		public List<DungeonMap> DungeonMaps { get; set; } = new List<DungeonMap>();
	}


	internal class DungeonMap
	{
		internal string MapFile { get; set; } = string.Empty;
		internal int SpawnWeight { get; set; }
		public int EntryX { get; set; }
		public int EntryY { get; set; }
		public string Challenge { get; set; } = string.Empty;
	}
}

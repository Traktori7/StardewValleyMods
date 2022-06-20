using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Monsters;
using xTile;


namespace MiniDungeons.Data
{
	internal class Challenge
	{
		public string ChallengeName { get; set; } = string.Empty;
		public int Timer { get; set; }
		public List<MonsterWave> MonsterWaves { get; set; } = new List<MonsterWave>();
		public int TimerBeforeNextWaveSpawns { get; set; }
		public List<Point> SpawnPoints { get; set; } = new List<Point>();
		public List<SpawnedObject> SpawnedObjects { get; set; } = new List<SpawnedObject>();
	}
}

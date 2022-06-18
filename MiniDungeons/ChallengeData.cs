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

using MiniDungeons.Dungeons;

namespace MiniDungeons
{
	internal class Challenge
	{
		public string ChallengeName { get; set; } = string.Empty;
		public int Timer { get; set; }
	}


	internal class FightChallenge : Challenge
	{
		public List<MonsterWave> MonsterWaves { get; set; } = new List<MonsterWave>();
		public int TimerBeforeNextWaveSpawns { get; set; }
		public List<SpawnPoint> SpawnPoints { get; set; } = new List<SpawnPoint>();
	}


	internal class CollectChallenge : Challenge
	{
		public List<SpawnPoint> SpawnPoints { get; set; } = new List<SpawnPoint>();
		public List<SpawnedObject> SpawnedObjects { get; set; } = new List<SpawnedObject>();
	}


	internal class SpawnPoint
	{
		public int X { get; set; }
		public int Y { get; set; }
	}


	internal class MonsterWave
	{
		public List<MonsterSpawn> Monsters { get; set; } = new List<MonsterSpawn>();
	}


	internal class MonsterSpawn
	{
		public string MonsterName { get; set; } = string.Empty;
		public int SpawnAmount { get; set; }
	}


	internal class SpawnedObject
	{
		public int ObjectID { get; set; }
		public int SpawnedAmount { get; set; }
	}
}

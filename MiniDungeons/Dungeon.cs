using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using MiniDungeons.Data;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Monsters;


namespace MiniDungeons
{
	internal class Dungeon
	{
		private int currentDungeonLocation = -1;
		private int mapType = -1;
		private DungeonState state = DungeonState.NONE;

		private readonly Data.Dungeon data;
		private readonly Dictionary<string, Data.Challenge> challengeData;

		public readonly List<DungeonLocation> dungeonLocations = new List<DungeonLocation>();

		/*public ReadOnlySpan<char> NameWithoutEnding
		{
			get
			{
				return Name.AsSpan(0, Name.IndexOf('_'));
			}
		}*/

		/*public string NameWithoutEnding
		{
			get
			{
				return Name[..Name.IndexOf('_')];
			}
		}*/


		public string Name
		{
			get { return data.DungeonName; }
		}

		public string? ChallengeName
		{
			get
			{
				if (data.DungeonMaps.Count > mapType && mapType != -1)
				{
					return data.DungeonMaps[mapType].Challenge;
				}
				else
				{
					return null;
				}
			}
		}


		public Data.Challenge? Challenge
		{
			get
			{
				if (ChallengeName is not null && challengeData.ContainsKey(ChallengeName))
				{
					return challengeData[ChallengeName];
				}
				else
				{
					return null;
				}
			}
		}


		public DungeonLocation? CurrentDungeonLocation
		{
			get
			{
				if (dungeonLocations.Count > currentDungeonLocation && currentDungeonLocation != -1)
				{
					return dungeonLocations[currentDungeonLocation];
				}
				else
				{
					return null;
				}
			}
		}


		public DungeonMap? CurrentDungeonMap
		{
			get
			{
				if (data.DungeonMaps.Count > mapType && mapType != -1)
				{
					return data.DungeonMaps[mapType];
				}
				else
				{
					return null;
				}
			}
		}


		public float SpawnChance
		{
			get { return data.SpawnChance; }
		}


		public string SpawnMapName
		{
			get { return data.SpawnMapName; }
		}


		public Point EntryPortalPoint
		{
			get { return new Point(data.PortalX, data.PortalY); }
		}


		public Point ExitPortalPoint
		{
			get
			{
				DungeonMap? dungeonMapData = CurrentDungeonMap;

				if (dungeonMapData is not null)
				{
					return new Point(dungeonMapData.EntryX, dungeonMapData.EntryY);
				}
				else
				{
					return new Point(0, 0);
				}
			}
		}


		public Dungeon(Data.Dungeon data, Dictionary<string, Data.Challenge> challengeData)
		{
			this.data = data;
			this.challengeData = challengeData;

			//CreateDungeonLocation(data.DungeonName);
		}


		public DungeonLocation CreateDungeonLocation()
		{
			string name = $"{ModEntry.modIDPrefix}.{Name}_1";
			DungeonLocation dungeonLocation = new DungeonLocation(name);

			PickMapType();

			dungeonLocations.Add(dungeonLocation);

			currentDungeonLocation = 0;

			return dungeonLocation;
		}


		/// <summary>
		/// Picks randomly from the weighted dungeon maps. From: https://stackoverflow.com/a/1761646
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		private void PickMapType()
		{
			int sumOfWeights = 0;

			for (int i = 0; i < data.DungeonMaps.Count; i++)
			{
				sumOfWeights += data.DungeonMaps[i].SpawnWeight;
			}

			int rand = Game1.random.Next(sumOfWeights);

			for (int i = 0; i < data.DungeonMaps.Count; i++)
			{
				if (rand <= data.DungeonMaps[i].SpawnWeight)
				{
					mapType = i;
					return;
					//return data.DungeonMaps[i];
				}
				else
				{
					rand -= data.DungeonMaps[i].SpawnWeight;
				}
			}

			// It should never reach here, but let's log it just in case
			ModEntry.logMonitor.Log($"Something went wrong picking the dungeon map. The sum of the weights is {sumOfWeights}", LogLevel.Error);
			//return null;
		}


		public void RemoveLocations()
		{
			foreach (DungeonLocation location in dungeonLocations)
			{
				if (Game1.locations.Remove(location))
				{
					ModEntry.logMonitor.Log($"Removed location {location.Name} from the locations list", LogLevel.Debug);
				}
			}
		}


		public void DayReset()
		{
			state = DungeonState.NONE;
			mapType = -1;
			currentDungeonLocation = -1;
			dungeonLocations.Clear();
		}


		public virtual void Initialize()
		{
			Data.Challenge? currentChallenge = Challenge;

			if (currentChallenge is null)
			{
				return;
			}

			for (int i = 0; i < currentChallenge.MonsterWaves.Count; i++)
			{
				// TODO: Add wave control and timers
				MonsterWave wave = currentChallenge.MonsterWaves[i];

				for (int j = 0; j < wave.Monsters.Count; j++)
				{
					MonsterSpawn waveSpawn = wave.Monsters[j];

					for (int k = 0; k < waveSpawn.SpawnAmount; k++)
					{
						Point point = PickSpawnPoint(currentChallenge.SpawnPoints);

						SpawnMonster(waveSpawn.MonsterName, point);
					}
				}
			}
			// TODO: SpawnedObject.Count vs SpawnPoints.Count + randomly picking the points
			for (int i = 0; i < currentChallenge.SpawnedObjects.Count; i++)
			{
				SpawnedObject obj = currentChallenge.SpawnedObjects[i];

				Point point = currentChallenge.SpawnPoints[i];

				CurrentDungeonLocation?.objects.Add(new Vector2(point.X, point.Y), new StardewValley.Object(obj.ObjectID, 1));
			}
		}


		public bool TryToSpawnPortal()
		{
			if (CanSpawnDungeonPortal())
			{
				state = DungeonState.DUNGEON_SPAWN_TESTED;

				if (Game1.random.NextDouble() < SpawnChance)
				{
					state = DungeonState.DUNGEON_SPAWNED;

					return true;
				}
			}

			return false;
		}


		private bool CanSpawnDungeonPortal()
		{
			if (state >= DungeonState.DUNGEON_SPAWN_TESTED)
			{
				return false;
			}

			if (!DungeonManager.DungeonSpawningEnabledForDungeon(Name))
			{
				return false;
			}

			return true;
		}


		private Monster? SpawnMonster(string monsterName, Point point)
		{
			Monster? monster = null;

			// TODO: Does this need unhardcodifying? Activator.CreateInstance or something? Or just SMAPI reflection/AccessTools?
			switch (monsterName)
			{
				case "GreenSlime":
					monster = new GreenSlime(new Vector2(point.X, point.Y) * 64f);
					break;
				default:
					ModEntry.logMonitor.Log($"Trying to spawn an unkown monster type {monsterName} at {point}", LogLevel.Error);
					break;
			}

			if (monster is not null)
			{
				CurrentDungeonLocation?.characters.Add(monster);
			}
			
			return monster;
		}


		public virtual void OnMonsterKilled(IEnumerable<NPC> monsters)
		{
			// This tries to test if all of the monsters have been killed, the player should still be in the location
			if (CurrentDungeonLocation?.characters.Any(c => c is Monster) == false)
			{
				state = DungeonState.DUNGEON_CLEARED;

				ModEntry.logMonitor.Log($"Player cleared dungeon {Name}!", LogLevel.Debug);

				if (ModEntry.config.enableHUDNotification)
				{
					Game1.addHUDMessage(new HUDMessage("The dungeon has been cleared", string.Empty));
				}
			}
		}


		public static Point PickSpawnPoint(List<Point> points)
		{
			int rand = Game1.random.Next(points.Count);

			return points[rand];
		}
	}


	public enum DungeonState
	{
		NONE,
		DUNGEON_SPAWN_TESTED,
		DUNGEON_SPAWNED,
		DUNGEON_ENTERED,
		DUNGEON_CLEARED
	}
}

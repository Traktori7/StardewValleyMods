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

using MiniDungeons.Dungeons;


namespace MiniDungeons
{
	internal class DungeonManager
	{
		private readonly IMonitor monitor;

		public Dictionary<string, DungeonData> dungeonData = null!;
		public Dictionary<string, Challenge> challengeData = null!;

		public readonly List<string> testedDungeons = new List<string>();
		public readonly List<Dungeon> activeDungeons = new List<Dungeon>();
		public readonly List<Dungeon> clearedDungeons = new List<Dungeon>();
		public readonly List<Warp> activeWarps = new List<Warp>();

		public int spawnedDungeonsToday = 0;


		//private readonly string warpTarget = "SeedShopDungeon_1";


		private IMonitor Monitor
		{ 
			get
			{
				return monitor;
			}
		}


		public DungeonManager(IMonitor monitor)
		{
			this.monitor = monitor;
		}


		public void PerformDayReset()
		{
			spawnedDungeonsToday = 0;

			testedDungeons.Clear();
			activeDungeons.Clear();
			clearedDungeons.Clear();
		}


		/// <summary>
		/// Remove the added dungeon locations before the game tries to serialize them to the save file
		/// </summary>
		public void RemoveAddedLocations()
		{
			for (int i = 0; i < activeDungeons.Count; i++)
			{
				if (Game1.locations.Remove(activeDungeons[i]))
				{
					Monitor.Log($"Removed location {activeDungeons[i].Name} from the locations list", LogLevel.Debug);
				}
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="e">Arguments from the Warped event</param>
		/// <remarks>New location is quaranteed to be not null</remarks>
		public void PlayerWarped(WarpedEventArgs e)
		{
			// TODO: Consider pattern matching (location is Dungeon dungeon) if the method isn't going to be any more complex
			if (IsLocationMiniDungeon(e.NewLocation))
			{
				Dungeon dungeon = (Dungeon)e.NewLocation;
				dungeon.Initialize();
				Monitor.Log($"Warped to dungeon {dungeon.Name}, the challenge is {dungeon.challenge.ChallengeName}", LogLevel.Debug);
			}

			// TODO: Clear the hardcoded map reference
			if (clearedDungeons.Count > 0)
			{
				for (int i = 0; i < clearedDungeons.Count; i++)
				{
					ClearWarps(clearedDungeons[i]);
				}

				clearedDungeons.Clear();
			}


			if (!CanSpawnDungeon())
			{
				return;
			}

			// TODO: Works only if the map has only 1 type of dungeon spawn enabled
			if (TryGetDungeonData(e.NewLocation.Name, out DungeonData? data))
			{
				if (!CanSpawnDungeon(data.DungeonName))
				{
					return;
				}

				testedDungeons.Add(data.DungeonName);

				if (Game1.random.NextDouble() < data.SpawnChance)
				{
					SpawnDungeonPortal(data, e.NewLocation);
				}
			}
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="e">Arguments from the NpcListChanged event</param>
		/// <remarks>Call this only if the player is currently in a dungeon</remarks>
		public void NpcListChangedInDungeon(NpcListChangedEventArgs e)
		{
			Dungeon? currentDungeon = GetCurrentDungeon();

			if (currentDungeon is null)
			{
				Monitor.Log("The current dungeon is null for some reason", LogLevel.Error);
				return;
			}

			currentDungeon.OnMonsterKilled(e.Removed.Where(x => x is Monster));

			if (currentDungeon.cleared)
			{
				DungeonCleared(currentDungeon);
			}
		}


		private bool CanSpawnDungeon()
		{
			if (spawnedDungeonsToday >= ModEntry.config.maxNumberOfDungeonsPerDay && ModEntry.config.maxNumberOfDungeonsPerDay != -1)
			{
				return false;
			}

			return true;
		}


		private bool CanSpawnDungeon(string dungeonName)
		{
			if (testedDungeons.Contains(dungeonName))
			{
				return false;
			}

			if (!DungeonSpawningEnabledForMap(dungeonName))
			{
				return false;
			}

			return true;
		}


		private bool DungeonSpawningEnabledForMap(string mapName)
		{
			// TODO: Remove hard coded dungeon names
			return mapName switch
			{
				"JojaMartDungeon" => ModEntry.config.enableJoJaPortals,
				"SeedShopDungeon" => ModEntry.config.enablePierrePortals,
				"YobaDungeon" => ModEntry.config.enableYobaPortals,
				_ => false,
			};
		}


		private bool TryGetDungeonData(string? mapName, [NotNullWhen(true)] out DungeonData? data)
		{
			foreach (var kvp in dungeonData)
			{
				if (kvp.Value.SpawnMapName.Equals(mapName))
				{
					data = kvp.Value;
					return true;
				}
			}

			data = null;
			return false;
		}


		public static bool IsLocationMiniDungeon(GameLocation location)
		{
			if (location is null)
			{
				return false;
			}

			if (location is Dungeon)
			{
				return true;
			}

			return false;
		}


		public Dungeon? GetCurrentDungeon()
		{
			for (int i = 0; i < activeDungeons.Count; i++)
			{
				if (activeDungeons[i].Name.Equals(Game1.currentLocation.Name))
				{
					return activeDungeons[i];
				}
			}

			return null;
		}


		private void SpawnDungeonPortal(DungeonData data, GameLocation location)
		{
			Dungeon? dungeon = MakeDungeon(data);

			if (dungeon is null)
			{
				Monitor.Log($"Unknown dungeon name {data.DungeonName}", LogLevel.Error);
				return;
			}

			// TODO: Test value for the warp, since custom locations might need something special to warp to
			// We'll probably have to patch Game1.getLocationFromNameInLocationsList
			// since just inserting the new locations to Game1.locations doesn't seem to be enough

			Warp warp = new Warp(data.PortalX, data.PortalY, dungeon.Name, dungeon.mapType.EntryX, dungeon.mapType.EntryY, false);

			location.warps.Add(warp);
			activeWarps.Add(warp);

			Monitor.Log($"Added warp to {location.Name} at ({data.PortalX} {data.PortalY}) targetting {dungeon.Name}", LogLevel.Debug);

			activeDungeons.Add(dungeon);
			spawnedDungeonsToday++;

			if (ModEntry.config.enableHUDNotification)
			{
				Game1.addHUDMessage(new HUDMessage("A new portal has appeared!", HUDMessage.newQuest_type));
			}

			Game1.locations.Add(dungeon);
		}


		public Dungeon? MakeDungeon(DungeonData data)
		{
			Dungeon? dungeon;
			string mapName;

			DungeonMap? map = PickMapType(data);

			if (map is null)
			{
				return null;
			}

			Challenge challenge = challengeData[map.Challenge];

			switch (data.DungeonName)
			{
				case "SeedShopDungeon":
					mapName = $"{ModEntry.modIDPrefix}.{data.DungeonName}";
					dungeon = new SeedShopDungeon(mapName, map, challenge);
					break;
				default:
					dungeon = null;
					break;
			}

			return dungeon;
		}


		/// <summary>
		/// Picks randomly from the weighted dungeon maps. From: https://stackoverflow.com/a/1761646
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public DungeonMap? PickMapType(DungeonData data)
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
					return data.DungeonMaps[i];
				}
				else
				{
					rand -= data.DungeonMaps[i].SpawnWeight;
				}
			}

			// It should never reach here, but let's log it just in case
			Monitor.Log($"Something went wrong picking the dungeon map. The sum of the weights is {sumOfWeights}", LogLevel.Error);
			return null;
		}


		/// <summary>
		/// Deletes the entry warp for the cleared dungeon
		/// TODO: Make cleaning the warps more robust
		/// </summary>
		private void ClearWarps(Dungeon dungeon)
		{

			DungeonData data = dungeonData[dungeon.Name];

			GameLocation location = Game1.getLocationFromName(data.SpawnMapName);

			if (location is not null)
			{
				location.warps.Remove(activeWarps[0]);
			}
			else
			{
				Monitor.Log($"Failed getting the location from dungeon data {dungeon.Name}", LogLevel.Error);
			}

			activeWarps.Clear();
		}


		private void DungeonCleared(Dungeon currentDungeon)
		{
			Monitor.Log($"Player cleared dungeon {currentDungeon.Name}!", LogLevel.Debug);

			Game1.addHUDMessage(new HUDMessage("The dungeon has been cleared", string.Empty));

			// TODO: Should we remove the dungeon from the active list?
			// Maybe not, since that is used to clear the added locations before saving
			clearedDungeons.Add(currentDungeon);
		}
	}
}

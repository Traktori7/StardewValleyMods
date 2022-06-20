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


namespace MiniDungeons
{
	internal class DungeonManager
	{
		//public Dictionary<string, DungeonData> dungeonData = null!;
		//public Dictionary<string, Challenge> challengeData = null!;

		/*public readonly List<string> testedDungeons = new List<string>();
		public readonly List<Dungeon> activeDungeons = new List<Dungeon>();
		public readonly List<Dungeon> clearedDungeons = new List<Dungeon>();*/
		public readonly List<Warp> activeWarps = new List<Warp>();
		public readonly List<Dungeon> dungeons = new List<Dungeon>();

		public int spawnedDungeonsToday = 0;


		//private readonly string warpTarget = "SeedShopDungeon_1";


		public DungeonManager()
		{

		}


		public void PopulateDungeonList(Dictionary<string, Data.Dungeon> dungeonData, Dictionary<string, Data.Challenge> challengeData)
		{
			// TODO: Add the challenge list to the dungeon instead of just the data?
			List<Challenge> challenges = new List<Challenge>();

			foreach (var val in challengeData.Values)
			{
				Challenge challenge = new Challenge(val);
				challenges.Add(challenge);
			}

			foreach (var kvp in dungeonData)
			{
				Dungeon dungeon = new Dungeon(kvp.Value, challengeData);
				dungeons.Add(dungeon);
			}
		}


		public void PerformDayReset()
		{
			spawnedDungeonsToday = 0;

			/*testedDungeons.Clear();
			activeDungeons.Clear();
			clearedDungeons.Clear();*/
			foreach (var dungeon in dungeons)
			{
				dungeon.DayReset();
			}
		}


		/// <summary>
		/// Remove the added dungeon locations before the game tries to serialize them to the save file
		/// </summary>
		public void RemoveAddedLocations()
		{
			foreach (var dungeon in dungeons)
			{
				dungeon.RemoveLocations();
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
				Dungeon? dungeon = GetDungeon(e.NewLocation as DungeonLocation);

				if (dungeon is not null)
				{
					dungeon.Initialize();

					// TODO: This check might blow up if the challenge name doesn't match the dictionary keys
					string challengeName = dungeon.ChallengeName ?? "unknown challenge";
					ModEntry.logMonitor.Log($"Initialized dungeon {dungeon.Name}, the challenge is {challengeName}", LogLevel.Debug);

					ClearWarps(dungeon);
				}
			}
			else
			{
				TryToSpawnDungeon(e.NewLocation);
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
				ModEntry.logMonitor.Log("The current dungeon is null for some reason", LogLevel.Error);
				return;
			}

			currentDungeon.OnMonsterKilled(e.Removed.Where(x => x is Monster));

			/*if (currentDungeon.cleared)
			{
				DungeonCleared(currentDungeon);
			}*/
		}


		public void TryToSpawnDungeon(GameLocation location)
		{
			if (CanSpawnDungeon())
			{
				// TODO: Works only if the map has only 1 type of dungeon spawn enabled
				if (TryGetDungeon(location.Name, out Dungeon? dungeon))
				{
					if (dungeon.TryToSpawnPortal())
					{
						SpawnDungeonPortal(dungeon, location);
					}
				}
			}
		}


		public Dungeon? GetDungeon(DungeonLocation? location)
		{
			if (location is null)
			{
				return null;
			}

			foreach (var dungeon in dungeons)
			{
				if (dungeon.dungeonLocations.Contains(location))
				{
					return dungeon;
				}
			}

			return null;
		}


		private bool CanSpawnDungeon()
		{
			if (ModEntry.config.maxNumberOfDungeonsPerDay != -1 && spawnedDungeonsToday >= ModEntry.config.maxNumberOfDungeonsPerDay)
			{
				return false;
			}

			return true;
		}


		/*public bool CanSpawnDungeonIn(string locationName)
		{
			if (!CanSpawnDungeon())
			{
				return false;
			}

			foreach (var val in dungeonData.Values)
			{
				if (val.SpawnMapName.Equals(locationName))
				{
					return true;
				}
			}

			return false;
		}*/



		public static bool DungeonSpawningEnabledForMap(string mapName)
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


		private bool TryGetDungeon(string? mapName, [NotNullWhen(true)] out Dungeon? dungeon)
		{
			for (int i = 0; i < dungeons.Count; i++)
			{
				if (dungeons[i].SpawnMapName.Equals(mapName))
				{
					dungeon = dungeons[i];
					return true;
				}
			}

			dungeon = null;
			return false;
		}


		public static bool IsLocationMiniDungeon(GameLocation location)
		{
			if (location is null)
			{
				return false;
			}

			if (location is DungeonLocation)
			{
				return true;
			}

			return false;
		}


		public Dungeon? GetCurrentDungeon()
		{
			foreach (var dungeon in dungeons)
			{
				if (dungeon.CurrentDungeonLocation?.Equals(Game1.currentLocation) == true)
				{
					return dungeon;
				}
			}

			return null;
		}


		/*public Dungeon? MakeDungeon(DungeonData data)
		{
			Dungeon? dungeon;
			string mapName;

			DungeonMapData? map = PickMapType(data);

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
		}*/


		public void SpawnDungeonPortal(Dungeon dungeon, GameLocation location)
		{
			/*Dungeon? dungeon = MakeDungeon(data);

			if (dungeon is null)
			{
				ModEntry.logMonitor.Log($"Unknown dungeon name {data.DungeonName}", LogLevel.Error);
				return;
			}*/

			DungeonLocation dungeonLocation = dungeon.CreateDungeonLocation();

			// TODO: Test value for the warp, since custom locations might need something special to warp to
			// We'll probably have to patch Game1.getLocationFromNameInLocationsList
			// since just inserting the new locations to Game1.locations doesn't seem to be enough

			Point entryPortalPoint = dungeon.EntryPortalPoint;
			Point exitPortalPoint = dungeon.ExitPortalPoint;

			Warp warp = new Warp(entryPortalPoint.X, entryPortalPoint.Y, dungeon.Name, exitPortalPoint.X, exitPortalPoint.Y, false);

			location.warps.Add(warp);
			activeWarps.Add(warp);

			ModEntry.logMonitor.Log($"Added warp to {location.Name} at ({entryPortalPoint.X} {entryPortalPoint.Y}) targetting {dungeon.Name}", LogLevel.Debug);

			//activeDungeons.Add(dungeon);
			spawnedDungeonsToday++;

			if (ModEntry.config.enableHUDNotification)
			{
				Game1.addHUDMessage(new HUDMessage("A new portal has appeared!", HUDMessage.newQuest_type));
			}

			Game1.locations.Add(dungeonLocation);
		}


		/// <summary>
		/// Deletes the entry warp for the cleared dungeon
		/// TODO: Make cleaning the warps more robust
		/// </summary>
		private void ClearWarps(Dungeon dungeon)
		{
			GameLocation location = Game1.getLocationFromName(dungeon.SpawnMapName);

			if (location is not null)
			{
				location.warps.Remove(activeWarps[0]);
			}
			else
			{
				ModEntry.logMonitor.Log($"Failed getting the location from dungeon data {dungeon.Name}", LogLevel.Error);
			}

			activeWarps.Clear();
		}


		/*private void DungeonCleared(Dungeon currentDungeon)
		{
			ModEntry.logMonitor.Log($"Player cleared dungeon {currentDungeon.Name}!", LogLevel.Debug);

			Game1.addHUDMessage(new HUDMessage("The dungeon has been cleared", string.Empty));

			// TODO: Should we remove the dungeon from the active list?
			// Maybe not, since that is used to clear the added locations before saving
			clearedDungeons.Add(currentDungeon);
		}*/
	}
}

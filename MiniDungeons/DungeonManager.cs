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
using xTile.Tiles;


namespace MiniDungeons
{
	internal class DungeonManager
	{
		//public readonly List<Warp> activeWarps = new List<Warp>();
		//public static readonly List<Dungeon> activeDungeons = new List<Dungeon>();
		public static readonly List<Data.Portal> activePortals = new List<Data.Portal>();
		public readonly List<Dungeon> dungeons = new List<Dungeon>();

		public int spawnedDungeonsToday = 0;


		public DungeonManager()
		{

		}


		public void PopulateDungeonList(Dictionary<string, Data.Dungeon> dungeonData, Dictionary<string, Data.Challenge> challengeData)
		{
			// TODO: Add the challenge list to the dungeon instead of just the data?
			List<Challenge> challenges = new List<Challenge>();

			foreach (Data.Challenge val in challengeData.Values)
			{
				Challenge challenge = new Challenge(val);
				challenges.Add(challenge);
			}

			foreach (Data.Dungeon val in dungeonData.Values)
			{
				Dungeon dungeon = new Dungeon(val, challengeData);
				dungeons.Add(dungeon);
			}
		}


		public void PerformDayReset()
		{
			spawnedDungeonsToday = 0;

			foreach (Dungeon dungeon in dungeons)
			{
				dungeon.DayReset();
			}
		}


		/// <summary>
		/// Remove the added dungeon locations before the game tries to serialize them to the save file
		/// </summary>
		public void RemoveAddedLocations()
		{
			foreach (Dungeon dungeon in dungeons)
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

					//ClearWarps(dungeon);
					RemoveWarp(dungeon.Name);
				}
			}
			else
			{
				TryToSpawnDungeon(e.NewLocation);
			}

			SpawnPortalSprites(e.NewLocation);
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

			foreach (Dungeon dungeon in dungeons)
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


		public static bool DungeonSpawningEnabledForDungeon(string dungeonName)
		{
			if (ModEntry.config.enabledDungeons.TryGetValue(dungeonName, out bool enabled))
			{
				return enabled;
			}
			else
			{
				return false;
			}
			// TODO: Remove hard coded dungeon names
			/*return dungeonName switch
			{
				"JojaMartDungeon" => ModEntry.config.enableJoJaPortals,
				"SeedShopDungeon" => ModEntry.config.enablePierrePortals,
				"YobaDungeon" => ModEntry.config.enableYobaPortals,
				_ => false,
			};*/
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
			foreach (Dungeon dungeon in dungeons)
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
			// TODO: Switch away from using warps, since apparently NPCs can use them (confirm).
			// Use some sort of Action with a confirmation box/way to remove the portal?
			DungeonLocation dungeonLocation = dungeon.CreateDungeonLocation();

			// TODO: Test value for the warp, since custom locations might need something special to warp to
			// We'll probably have to patch Game1.getLocationFromNameInLocationsList
			// since just inserting the new locations to Game1.locations doesn't seem to be enough
			// But it seems to work so far...

			Point entryPortalPoint = dungeon.EntryPortalPoint;
			Point exitPortalPoint = dungeon.ExitPortalPoint;

			//Warp warp = new Warp(entryPortalPoint.X, entryPortalPoint.Y, dungeonLocation.Name, exitPortalPoint.X, exitPortalPoint.Y, false);

			//location.warps.Add(warp);
			//activeWarps.Add(warp);

			Tile? tile = GetBackTile(location, entryPortalPoint);

			if (tile is not null)
			{
				string propertyString = $"{ModEntry.actionName} {dungeonLocation.Name} {exitPortalPoint.X} {exitPortalPoint.Y}";
				tile.Properties.Add("TouchAction", new xTile.ObjectModel.PropertyValue(propertyString));
			}
			else
			{
				ModEntry.logMonitor.Log($"Couldn't find tile {entryPortalPoint} in {location.Name}", LogLevel.Error);
			}

			//activeDungeons.Add(dungeon);
			Data.Portal portal = new Data.Portal(location, entryPortalPoint, dungeon);
			activePortals.Add(portal);

			ModEntry.logMonitor.Log($"Added warp to {location.Name} at ({entryPortalPoint.X} {entryPortalPoint.Y}) targetting {dungeonLocation.Name}", LogLevel.Debug);

			//activeDungeons.Add(dungeon);
			spawnedDungeonsToday++;

			if (ModEntry.config.enableHUDNotification)
			{
				Game1.addHUDMessage(new HUDMessage("A new portal has appeared!", HUDMessage.newQuest_type));
			}

			Game1.locations.Add(dungeonLocation);
		}


		internal static void RemoveWarp(string dungeonName)
		{
			foreach (var item in activePortals)
			{
				if (dungeonName.Contains(item.dungeon.Name))
				{
					ClearWarps(item);
					return;
				}
			}
		}


		/// <summary>
		/// Deletes the entry warp for the cleared dungeon
		/// </summary>
		private static void ClearWarps(Data.Portal portal)
		{
			//GameLocation location = Game1.getLocationFromName(dungeon.SpawnMapName);
			GameLocation location = portal.location;

			if (location is not null)
			{
				//location.warps.Remove(activeWarps[0]);
				Tile? tile = GetBackTile(location, portal.point);
				tile?.Properties.Remove("TouchAction");

				location.temporarySprites.Remove(portal.sprite);
			}
			else
			{
				ModEntry.logMonitor.Log($"Failed getting the location from dungeon data {portal.dungeon.Name}", LogLevel.Error);
			}

			//activeWarps.Clear();
			//activeDungeons.Remove(dungeon);
			activePortals.Remove(portal);
		}


		/// <summary>
		/// Gets a tile from the Back layer.
		/// </summary>
		/// <param name="location">The game location.</param>
		/// <param name="tileCoordinates">The tile coordinates of the tile.</param>
		/// <returns>The tile, or null if it wasn't found.</returns>
		private static Tile? GetBackTile(GameLocation location, Point tileCoordinates)
		{
			return location.Map.GetLayer("Back").PickTile(new xTile.Dimensions.Location(tileCoordinates.X * 64, tileCoordinates.Y * 64), Game1.viewport.Size);
		}


		/// <summary>
		/// Tries to spawn the portal sprites in a location.
		/// </summary>
		/// <param name="location">The location where to spawn the portal sprites.</param>
		private static void SpawnPortalSprites(GameLocation location)
		{
			foreach (var item in activePortals)
			{
				if (location.Name.Equals(item.location.Name))
				{
					SpawnPortalSprite(location, item);
				}
			}
		}


		private static void SpawnPortalSprite(GameLocation location, Data.Portal portal)
		{
			Point portalSpawnPoint = portal.point;

			int sizeCorrectionMultiplier = 4;
			float animationIntervalInMilliseconds = 1000f;
			int animationLenght = 4;
			int loops = 1000000000;
			// Y-coordinate needs to be 1 less that the portal position, OR change the portal to be 1x1?
			Vector2 portalPosition = new Vector2(portalSpawnPoint.X, portalSpawnPoint.Y - 1) * 64f;

			TemporaryAnimatedSprite sprite = new TemporaryAnimatedSprite(ModEntry.portalAssetName,
				new Rectangle(0, 0, 16, 32),
				animationIntervalInMilliseconds,
				animationLenght,
				loops,
				portalPosition,
				false,
				false)
			{
				scale = sizeCorrectionMultiplier
			};

			portal.sprite = sprite;
			location.temporarySprites.Add(sprite);
		}
	}
}

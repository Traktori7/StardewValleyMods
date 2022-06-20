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


namespace MiniDungeons
{
	/// <summary>The mod entry point.</summary>
	public class ModEntry : Mod
	{
		internal static IMonitor logMonitor = null!;

		private readonly string collectChallengeDataPath = Path.Combine("assets", "data", "CollectChallengeData.json");
		private readonly string fightChallengeDataPath = Path.Combine("assets", "data", "FightChallengeData.json");
		private readonly string dungeonDataPath = Path.Combine("assets", "data", "DungeonData.json");
		//private readonly string seedShopDungeonMap = "Maps/SeedShopDungeon_1";

		internal static ModConfig config = null!;
		internal static readonly string modIDPrefix = "Traktori.MiniDungeons";

		private DungeonManager dungeonManager = null!;


		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			logMonitor = Monitor;

			config = helper.ReadConfig<ModConfig>();

			ReadData();

			helper.Events.Content.AssetRequested += OnAssetRequested;
			helper.Events.GameLoop.DayStarted += OnDayStarted;
			helper.Events.GameLoop.GameLaunched += OnGameLaunched;
			helper.Events.GameLoop.Saving += OnSaving;
			helper.Events.Player.Warped += OnWarped;
			helper.Events.World.NpcListChanged += OnNpcListChanged;
		}


		private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
		{
			if (e.Name.StartsWith(Path.Combine("Maps", modIDPrefix)))
			{
				e.LoadFromModFile<Map>("assets/maps/SeedShop/SeedShop.tmx", AssetLoadPriority.Low);
			}
		}


		private void OnDayStarted(object? sender, DayStartedEventArgs e)
		{
			dungeonManager.PerformDayReset();
		}


		private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
		{
			var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

			if (configMenu is not null)
			{
				RegisterConfigMenu(configMenu);
			}
		}


		private void OnSaving(object? sender, SavingEventArgs e)
		{
			dungeonManager.RemoveAddedLocations();
		}


		private void OnWarped(object? sender, WarpedEventArgs e)
		{
			if (e.NewLocation is null)
			{
				return;
			}

			dungeonManager.PlayerWarped(e);
		}


		private void OnNpcListChanged(object? sender, NpcListChangedEventArgs e)
		{
			if (e.IsCurrentLocation && DungeonManager.IsLocationMiniDungeon(Game1.currentLocation))
			{
				dungeonManager.NpcListChangedInDungeon(e);
			}
		}


		/// <summary>
		/// Constructs the GenericModConfigMenu menu's options
		/// </summary>
		/// <param name="configMenu"></param>
		private void RegisterConfigMenu(IGenericModConfigMenuApi configMenu)
		{
			configMenu.Register(
				mod: ModManifest,
				reset: () => config = new ModConfig(),
				save: () => Helper.WriteConfig(config)
			);

			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => Helper.Translation.Get("gmcm.enable-notification-label"),
				tooltip: () => Helper.Translation.Get("gmcm.enable-notification-description"),
				getValue: () => config.enableHUDNotification,
				setValue: (bool value) => config.enableHUDNotification = value
			);

			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => Helper.Translation.Get("gmcm.dungeon-limit-label"),
				tooltip: () => Helper.Translation.Get("gmcm.dungeon-limit-description"),
				getValue: () => config.maxNumberOfDungeonsPerDay,
				setValue: (int value) => config.maxNumberOfDungeonsPerDay = value,
				min: -1
			);

			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => Helper.Translation.Get("gmcm.enable-pierre-label"),
				tooltip: () => Helper.Translation.Get("gmcm.enable-pierre-description"),
				getValue: () => config.enablePierrePortals,
				setValue: (bool value) => config.enablePierrePortals = value
			);

			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => Helper.Translation.Get("gmcm.enable-joja-label"),
				tooltip: () => Helper.Translation.Get("gmcm.enable-joja-description"),
				getValue: () => config.enableJoJaPortals,
				setValue: (bool value) => config.enableJoJaPortals = value
			);

			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => Helper.Translation.Get("gmcm.enable-yoba-label"),
				tooltip: () => Helper.Translation.Get("gmcm.enable-yoba-description"),
				getValue: () => config.enableYobaPortals,
				setValue: (bool value) => config.enableYobaPortals = value
			);
		}


		private void ReadData()
		{
			dungeonManager = new DungeonManager();

			Dictionary<string, Data.Challenge> challengeData = ReadChallengeData();
			Dictionary<string, Data.Dungeon> dungeonData = ReadDungeonData();

			dungeonManager.PopulateDungeonList(dungeonData, challengeData);

			//dungeonManager.challengeData = ReadChallengeData();
			//dungeonManager.dungeonData = ReadDungeonData();
		}


		/// <summary>
		/// Reads the challenge data json files
		/// </summary>
		/// <returns>Returns the dictionary of challenge data, or an empty one if the reading failed.</returns>
		private Dictionary<string, Data.Challenge> ReadChallengeData()
		{
			var dict1 = ReadListToDict<Data.Challenge>(fightChallengeDataPath, x => x.ChallengeName);
			var dict2 = ReadListToDict<Data.Challenge>(collectChallengeDataPath, x => x.ChallengeName);

			Dictionary<string, Data.Challenge> challenges = new Dictionary<string, Data.Challenge>();

			foreach (var kvp in dict1)
			{
				challenges[kvp.Key] = kvp.Value;
			}

			foreach (var kvp in dict2)
			{
				challenges[kvp.Key] = kvp.Value;
			}

			return challenges;

			// Combines the 2 dictionaries, From: https://stackoverflow.com/a/53450763
			//return dict1.Concat(dict2).GroupBy(k => k.Key).ToDictionary(k => k.Key, v => v.First().Value);

			/*try
			{
				List<FightChallenge>? data = Helper.Data.ReadJsonFile<List<FightChallenge>>(fightChallengeDataPath);
				List<CollectChallenge>? data2 = Helper.Data.ReadJsonFile<List<CollectChallenge>>(collectChallengeDataPath);

				if (data is not null)
				{
					return data.ToDictionary(x => x.ChallengeName, x => (Challenge)x);

					Dictionary<string, Challenge> dictionary = new Dictionary<string, Challenge>();

					for (int i = 0; i < data.Count; i++)
					{
						dictionary[data[i].ChallengeName] = data[i];
					}

					return dictionary;
				}
				else
				{
					Monitor.Log("Reading the dungeon data failed", LogLevel.Error);
				}
			}
			catch (Exception ex)
			{
				Monitor.Log(ex.ToString(), LogLevel.Error);
			}

			return new Dictionary<string, Challenge>();*/
		}


		/// <summary>
		/// Reads the dungeon data json
		/// </summary>
		/// <returns>Returns the dictionary of dungeon data, or an empty one if the reading failed.</returns>
		private Dictionary<string, Data.Dungeon> ReadDungeonData()
		{
			return ReadListToDict<Data.Dungeon>(dungeonDataPath, x => x.DungeonName);
			/*try
			{
				List<DungeonData>? data = Helper.Data.ReadJsonFile<List<DungeonData>>(dungeonDataPath);

				if (data is not null)
				{
					return data.ToDictionary(x => x.DungeonName);
				}
				else
				{
					Monitor.Log("Reading the dungeon data failed", LogLevel.Error);
				}
			}
			catch (Exception ex)
			{
				Monitor.Log(ex.ToString(), LogLevel.Error);
			}

			return new Dictionary<string, DungeonData>();*/
		}


		private Dictionary<string, TData> ReadListToDict<TData>(string path, Func<TData, string> keySelector) where TData : class, new()
		{
			try
			{
				List<TData>? data = Helper.Data.ReadJsonFile<List<TData>>(path);

				if (data is not null)
				{
					return data.ToDictionary(keySelector);
				}
				else
				{
					Monitor.Log($"Reading {path} data failed", LogLevel.Error);
				}
			}
			catch (Exception ex)
			{
				Monitor.Log(ex.ToString(), LogLevel.Error);
			}

			return new Dictionary<string, TData>();
		}
	}
}
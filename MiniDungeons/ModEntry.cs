using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
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
		internal static ITranslationHelper i18n = null!;

		public static readonly string portalAssetName = PathUtilities.NormalizeAssetName("Traktori.MiniDungeons/PortalSprite");

		private readonly string portalSpritePath = Path.Combine("assets", "PortalSprite.png");
		private readonly string challengeDataPath = Path.Combine("assets", "data", "ChallengeData.json");
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
			i18n = helper.Translation;

			ReadData();

			config = helper.ReadConfig<ModConfig>();

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
			else if (e.Name.IsEquivalentTo(portalAssetName))
			{
				e.LoadFromModFile<Texture2D>(portalSpritePath, AssetLoadPriority.Low);
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
			InitializeConfig();

			configMenu.Register(
				mod: ModManifest,
				reset: () => config = new ModConfig(),
				save: () => Helper.WriteConfig(config)
			);

			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => i18n.Get("gmcm.enable-notification-label"),
				tooltip: () => i18n.Get("gmcm.enable-notification-description"),
				getValue: () => config.enableHUDNotification,
				setValue: (bool value) => config.enableHUDNotification = value
			);

			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => i18n.Get("gmcm.dungeon-limit-label"),
				tooltip: () => i18n.Get("gmcm.dungeon-limit-description"),
				getValue: () => config.maxNumberOfDungeonsPerDay,
				setValue: (int value) => config.maxNumberOfDungeonsPerDay = value,
				min: -1
			);

			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => i18n.Get("gmcm.enable-fighting-challenge-label"),
				tooltip: () => i18n.Get("gmcm.enable-notiffighting-challengeication-description"),
				getValue: () => config.enableFightingchallenges,
				setValue: (bool value) => config.enableFightingchallenges = value
			);

			// TODO: This doesn't seem to work at all
			foreach (Dungeon dungeon in dungeonManager.dungeons)
			{
				configMenu.AddBoolOption(
					mod: ModManifest,
					name: () => i18n.Get("gmcm.enable-dungeon-label", new {dungeonName = dungeon.Name}),
					tooltip: () => i18n.Get("gmcm.enable-dungeon-description", new { dungeonName = dungeon.Name }),
					getValue: () => config.enabledDungeons[dungeon.Name],
					setValue: (bool value) => config.enabledDungeons[dungeon.Name] = value
				);

				configMenu.AddNumberOption(
					mod: ModManifest,
					name: () => i18n.Get("gmcm.dungeon-spawn-chance-label", new { dungeonName = dungeon.Name }),
					tooltip: () => i18n.Get("gmcm.dungeon-spawn-chance-description", new { dungeonName = dungeon.Name, defaultChance = dungeon.SpawnChance }),
					getValue: () => config.dungeonSpawnChances[dungeon.Name],
					setValue: (float value) => config.dungeonSpawnChances[dungeon.Name] = value,
					min: 0f,
					max: 1f,
					interval: 0.01f
				);
			}
		}


		private void InitializeConfig()
		{
			// Makes sure the dictionaries don't contain any old keys, but keeps the old values
			Dictionary<string, bool> temp1 = config.enabledDungeons;
			Dictionary<string, float> temp2 = config.dungeonSpawnChances;

			config.enabledDungeons.Clear();
			config.dungeonSpawnChances.Clear();

			// Initialize with only the values for the currently loaded dungeons
			foreach (Dungeon dungeon in dungeonManager.dungeons)
			{
				if (!config.enabledDungeons.ContainsKey(dungeon.Name))
				{
					config.enabledDungeons[dungeon.Name] = true;
				}
				
				if (!config.dungeonSpawnChances.ContainsKey(dungeon.Name))
				{
					config.dungeonSpawnChances[dungeon.Name] = dungeon.SpawnChance;
				}
			}

			// Overwrite with the old values
			foreach (var item in temp1)
			{
				if (config.enabledDungeons.ContainsKey(item.Key))
				{
					config.enabledDungeons[item.Key] = item.Value;
				}
			}

			foreach (var item in temp2)
			{
				if (config.dungeonSpawnChances.ContainsKey(item.Key))
				{
					config.dungeonSpawnChances[item.Key] = item.Value;
				}
			}
		}


		private void ReadData()
		{
			dungeonManager = new DungeonManager();

			Dictionary<string, Data.Challenge> challengeData = ReadChallengeData();
			Dictionary<string, Data.Dungeon> dungeonData = ReadDungeonData();

			dungeonManager.PopulateDungeonList(dungeonData, challengeData);
		}


		/// <summary>
		/// Reads the challenge data json files
		/// </summary>
		/// <returns>Returns the dictionary of challenge data, or an empty one if the reading failed.</returns>
		private Dictionary<string, Data.Challenge> ReadChallengeData()
		{
			return ReadListToDict<Data.Challenge>(challengeDataPath, x => x.ChallengeName);
		}


		/// <summary>
		/// Reads the dungeon data json
		/// </summary>
		/// <returns>Returns the dictionary of dungeon data, or an empty one if the reading failed.</returns>
		private Dictionary<string, Data.Dungeon> ReadDungeonData()
		{
			return ReadListToDict<Data.Dungeon>(dungeonDataPath, x => x.DungeonName);
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
using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace VersatileGrabber
{
	public class ModEntry : Mod
	{
		public const string versatileGrabberName = "Versatile Grabber";
		public const string saveDataKey = "Save data";

		private const string jsonAssetsUniqueID = "spacechase0.JsonAssets";
		private bool jsonAssetsFound = false;

		private IJsonAssetsApi jsonAssetsApi;

		internal Config config;
		internal ITranslationHelper i18n => Helper.Translation;

		internal static int GrabberID { get; private set; } = -1;
		internal static Texture2D texture;

		internal static IMonitor ModMonitor;
		internal static IModHelper ModHelper;
		internal static GrabberController Controller;


		/******************/
		/* Public methods */
		/******************/

		public override void Entry(IModHelper helper)
		{
			ModMonitor = Monitor;
			ModHelper = helper;
			Controller = new GrabberController();

			//string startingMessage = i18n.Get("template.start", new { mod = helper.ModRegistry.ModID, folder = helper.DirectoryPath });
			//Monitor.Log(startingMessage, LogLevel.Trace);

			config = helper.ReadConfig<Config>();

			helper.Events.Player.InventoryChanged += this.OnInventoryChanged;
			helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
			helper.Events.GameLoop.Saving += this.OnSaving;
			helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
			helper.Events.GameLoop.DayStarted += this.OnDayStarted;
			helper.Events.GameLoop.UpdateTicking += this.OnUpdateTicking;
			helper.Events.Input.ButtonPressed += this.OnButtonPressed;
			helper.Events.World.ObjectListChanged += this.OnObjectListChanged;

			texture = helper.Content.Load<Texture2D>(Path.Combine("assets", "versatile grabber 1.png"));
		}

		









		/*******************/
		/* Private methods */
		/*******************/

		private void LoadApis()
		{
			jsonAssetsApi = Helper.ModRegistry.GetApi<IJsonAssetsApi>(jsonAssetsUniqueID);
			if (jsonAssetsApi != null)
			{
				jsonAssetsFound = true;
			}
		}


		private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
		{
			LoadApis();

			if (!jsonAssetsFound)
			{
				ModMonitor.Log("JsonAssets not found.", LogLevel.Error);
				return;
			}

			jsonAssetsApi.LoadAssets(Path.Combine(ModHelper.DirectoryPath, "assets", "VersatileGrabber"));
			jsonAssetsApi.IdsAssigned += OnIdsAssigned;
		}


		private void OnIdsAssigned(object sender, EventArgs e)
		{
			GrabberID = jsonAssetsApi.GetBigCraftableId(versatileGrabberName);
			ModMonitor.Log($"Versatile Grabber loaded with ID {GrabberID}", LogLevel.Debug);
		}


		private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
		{
			

			// Create one grabber
			/*VersatileGrabber grabber = new VersatileGrabber();
			grabber.ParentSheetIndex = GrabberID;

			Game1.getFarm().dropObject(grabber);*/
		}


		private void OnSaving(object sender, SavingEventArgs e)
		{
			Controller.SaveGrabbers();
		}



		private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
		{
			if (e.Button == SButton.K)
				HarvestItems();
			
		}


		private void OnUpdateTicking(object sender, UpdateTickingEventArgs e)
		{
			
		}


		private void OnDayStarted(object sender, DayStartedEventArgs e)
		{
			ModMonitor.Log("Day started", LogLevel.Debug);
		}


		private void HarvestItems()
		{
			//if (!config.GlobalHarvesting)
				//return;

			List<Vector2> grabbables = new List<Vector2>();

			foreach (var grabber in Controller.versatileGrabbers)
			{
				foreach (var location in Game1.locations)
				{
					grabbables.Clear();

					foreach (var kvp in location.Objects.Pairs)
					{
						// Don't pick up big craftables
						if (kvp.Value.bigCraftable.Value)
							continue;

						// Picks some forageables up for testing
						if (kvp.Value.ParentSheetIndex == 20 || kvp.Value.ParentSheetIndex == 22)
						{
							grabbables.Add(kvp.Key);
						}
					}

					foreach (Vector2 tile in grabbables)
					{
						grabber.Value.AddItemToInventory(location.Objects[tile]);
						location.Objects.Remove(tile);
					}
				}
			}
		}


		private void OnObjectListChanged(object sender, ObjectListChangedEventArgs e)
		{
			ModMonitor.Log("Object list changed", LogLevel.Debug);

			foreach (var keyValuePair in e.Added)
			{
				if (GrabberController.ItemShouldBeVersatileGrabber(keyValuePair.Value))
				{
					// Converts the placed object to a versatile grabber
					VersatileGrabber grabber = new VersatileGrabber(keyValuePair.Value);
					e.Location.objects[keyValuePair.Key] = grabber;

					Controller.AddGrabber(grabber, e.Location, keyValuePair.Key);
				}
			}

			foreach (var kvp in e.Removed)
			{
				if (kvp.Value is VersatileGrabber grabber)
				{
					Controller.RemoveGrabber(grabber, e.Location, kvp.Key);
				}
			}
		}



		private void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
		{
			foreach (var addedItem in e.Added)
			{
				// This check seems to be false for picked up grabbers...
				if (!(addedItem is VersatileGrabber grabber))
					continue;

				// At this point it should be of type Versatile Grabber
				ModMonitor.Log("Versatile grabber found in inventory, converting to SObject", LogLevel.Debug);

				int index = Game1.player.Items.IndexOf(addedItem);
				Game1.player.Items[index] = grabber.ToObject();
			}
			
		}
	}
}

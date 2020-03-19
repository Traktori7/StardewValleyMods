using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Locations;
using StardewValley.Buildings;
using StardewValley.Network;
using System;
using System.IO;

namespace IndustrialFurnace
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod, IAssetEditor, IAssetLoader
    {
        private const string controllerDataSaveKey = "controller-save";
        private const string furnaceBuildingType = "Industrial Furnace";
        private const string saveDataRefreshedMessage = "Save data refreshed";
        private const string requestSaveData = "Request save data";

        private readonly string assetPath = Path.Combine("Buildings", furnaceBuildingType);
        private readonly string blueprintsPath = Path.Combine("Data", "Blueprints");

        private readonly string furnaceOnTexturePath = Path.Combine("assets", "IndustrialFurnaceOn.png");
        private readonly string furnaceOffTexturePath = Path.Combine("assets", "IndustrialFurnaceOff.png");
        private readonly string blueprintDataPath = Path.Combine("assets", "IndustrialFurnaceBlueprint.json");
        private readonly string smeltingRulesDataPath = Path.Combine("assets", "SmeltingRules.json");

        private int furnacesBuilt = 0;      // Used to identify furnaces, placed in maxOccupants field.
        private ModConfig config;
        private ModSaveData modSaveData;
        private BlueprintData blueprintData;
        private SmeltingRulesContainer newSmeltingRules;
        private ITranslationHelper i18n;

        //private IndustrialFurnaceController currentlyOpenedOutput;

        private Texture2D furnaceOn;
        private Texture2D furnaceOff;

        private List<Building> furnaces = new List<Building>();
        private List<IndustrialFurnaceController> furnaceControllers = new List<IndustrialFurnaceController>();
        

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            i18n = helper.Translation;
            
            // Load the two textures
            furnaceOn = helper.Content.Load<Texture2D>(furnaceOnTexturePath);
            furnaceOff = helper.Content.Load<Texture2D>(furnaceOffTexturePath);

            this.config = helper.ReadConfig<ModConfig>();

            // TODO: Use the name specified in the blueprint?
            blueprintData = helper.Data.ReadJsonFile<BlueprintData>(blueprintDataPath);
            newSmeltingRules = helper.Data.ReadJsonFile<SmeltingRulesContainer>(smeltingRulesDataPath);
            CheckSmeltingRules();

            helper.Events.Display.RenderedWorld += this.OnRenderedWorld;
            helper.Events.Display.MenuChanged += this.OnMenuChanged;
            helper.Events.GameLoop.UpdateTicking += this.OnUpdate;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.ReturnedToTitle += this.OnReturnedToTitle;
            helper.Events.GameLoop.Saving += this.OnSaving;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.World.BuildingListChanged += this.OnBuildingListChanged;
            helper.Events.Multiplayer.ModMessageReceived += this.OnModMessageReceived;
        }


        /// <summary>Get whether this instance can load the initial version of the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanLoad<T>(IAssetInfo asset)
        {
            if (asset.AssetNameEquals(assetPath))
            {
                return true;
            }

            return false;
        }


        /// <summary>Load a matched asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public T Load<T>(IAssetInfo asset)
        {
            if (asset.AssetNameEquals(assetPath))
            {
                return this.Helper.Content.Load<T>(furnaceOffTexturePath, ContentSource.ModFolder);
            }

            throw new InvalidOperationException($"Unexpected asset '{asset.AssetName}'.");
        }


        /// <summary>Get whether this instance can edit the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being edit.</param>
        public bool CanEdit<T>(IAssetInfo asset)
        {
            if (asset.AssetNameEquals(blueprintsPath))
            {
                return true;
            }

            return false;
        }


        /// <summary>Edit a matched asset.</summary>
        /// <param name="asset">A helper which encapsulates metadata about an asset and enables changes to it.</param>
        public void Edit<T>(IAssetData asset)
        {
            if (asset.AssetNameEquals(blueprintsPath))
            {
                var editor = asset.AsDictionary<string, string>();
                editor.Data[furnaceBuildingType] = blueprintData.ToBlueprintString(i18n);
                return;
            }

            throw new InvalidOperationException($"Unexpected asset '{asset.AssetName}'.");
        }


        /// <summary>Sends a message for all connected players the updated save data. TODO: Exclude the sender?</summary>
        public void SendUpdateMessage()
        {
            // Refresh the save data for the multiplayer message and send message to all players, including host (currently no harm in doing so)
            InitializeSaveData();
            Helper.Multiplayer.SendMessage<ModSaveData>(modSaveData, saveDataRefreshedMessage, new[] { this.ModManifest.UniqueID });
        }


        /*********
        ** Private methods
        *********/
        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUpdate(object sender, UpdateTickingEventArgs e)
        {
            if (Game1.getFarm() is null) return;

            // Create the smoke effect every 0.5 seconds on active furnaces
            if (e.IsMultipleOf(30))
            {
                GameLocation location = Game1.player.currentLocation;

                if (location != null && location.IsFarm && location.IsOutdoors)
                {
                    for (int i = 0; i < furnaces.Count; i++)
                    {
                        if (!furnaceControllers[GetIndexOfFurnaceControllerWithTag(furnaces[i].maxOccupants.Value)].CurrentlyOn) continue;

                        int x = furnaces[i].tileX.Value;
                        int y = furnaces[i].tileY.Value;

                        // Add smoke sprites
                        location.temporarySprites.Add(new TemporaryAnimatedSprite(Path.Combine("LooseSprites", "Cursors"),
                            new Rectangle(372, 1956, 10, 10),
                            new Vector2(x * 64 + 64 + 4, y * 64 - 64),
                            false, 1f / 500f, Color.Gray)
                        {
                            alpha = 0.75f,
                            motion = new Vector2(0.0f, -0.5f),
                            acceleration = new Vector2(1f / 500f, 0.0f),
                            interval = 99999f,
                            layerDepth = 1f,
                            scale = 2f,
                            scaleChange = 0.02f,
                            rotationChange = (float)(Game1.random.Next(-5, 6) * 3.14159274101257 / 256.0)
                        });

                        // Spark only randomly
                        if (Game1.random.NextDouble() >= 0.2) continue;

                        // Add sparks
                        location.temporarySprites.Add(new TemporaryAnimatedSprite(30,
                            new Vector2(x,y) * 64f + new Vector2(64.0f + (float)Game1.random.NextDouble() * 32.0f - 16.0f, 18f + (float)Game1.random.NextDouble() * 8.0f - 4.0f),
                            Color.White, 4, false, 100f, 10, 64, (float)((y + 1.0) * 64.0 / 10000.0 + 9.99999974737875E-05 + (x + 1.0) * 64.0 / 10000.0), -1, 0)
                        {
                            alphaFade = 0.005f,
                            light = true,
                            lightcolor = Color.Black
                        });

                        // Puff only randomlierly
                        if (Game1.random.NextDouble() >= 0.05) continue;
                        Game1.playSound("fireball");
                    }
                }
            }

            //Monitor.Log("Update ticking event handled", LogLevel.Debug);

            /*for (int i = 0; i < furnaceControllers.Count; i++)
            {
                NetMutex mutex = furnaceControllers[i].output.mutex;

                if (mutex is null) return;

                // Assumes the furnaces can only be built on the farm
                mutex.Update(Game1.getFarm());

                //Monitor.Log("Mutex updated", LogLevel.Debug);

                if (mutex.IsLocked() && Game1.activeClickableMenu is null)
                {
                    mutex.ReleaseLock();

                    Monitor.Log("The lock was released for furnace id: " + furnaceControllers[i].ID, LogLevel.Debug);
                }
            }*/
        }


        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // Integration for Generic Mod Config Menu by spacechase0
            var api = Helper.ModRegistry.GetApi<IGenericModConfigMenuAPI>("spacechase0.GenericModConfigMenu");

            if (api != null)
            {
                api.RegisterModConfig(ModManifest, () => config = new ModConfig(), () => Helper.WriteConfig(config));
                api.RegisterLabel(ModManifest, i18n.Get("gmcm.main-label"), "");
                api.RegisterClampedOption(ModManifest, i18n.Get("gmcm.coal-amount-label"), i18n.Get("gmcm.coal-amount-description"), () => config.CoalAmount, (int val) => config.CoalAmount = val, 1, 100);
                api.RegisterSimpleOption(ModManifest, i18n.Get("gmcm.instant-smelting-label"), i18n.Get("gmcm.instant-smelting-description"), () => config.InstantSmelting, (bool val) => config.InstantSmelting = val);
            }
        }


        private void OnReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            // Reset stuff
            modSaveData = null;
            furnaces.Clear();
            furnaceControllers.Clear();
        }


        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnModMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            if (e.FromModID == ModManifest.UniqueID)
            {
                if (e.Type == saveDataRefreshedMessage)
                {
                    // Receive the save data
                    modSaveData = e.ReadAs<ModSaveData>();
                    // Refresh the furnace data
                    InitializeFurnaceControllers(false);

                    UpdateTextures();
                }
                else if (e.Type == requestSaveData)
                {
                    RequestSaveData request = e.ReadAs<RequestSaveData>();
                    Helper.Multiplayer.SendMessage<ModSaveData>(modSaveData, saveDataRefreshedMessage, new string[] { ModManifest.UniqueID }, new long[] { request.PlayerID });
                }
            }
        }


        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSaving(object sender, SavingEventArgs e)
        {
            if (Game1.player.IsMainPlayer)
            {
                InitializeSaveData();
                this.Helper.Data.WriteSaveData(controllerDataSaveKey, modSaveData);
            }
        }


        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (Game1.player.IsMainPlayer)
            {
                InitializeFurnaceControllers(true);
                //Monitor.Log("OnSaveLoaded IsMainPlayer check passed", LogLevel.Debug);
            }
        }


        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // Ignore if player hasn't loaded in yet, or is stuck in a menu or cutscene
            if (!Context.IsPlayerFree)
                return;

            //this.Monitor.Log($"{Game1.player.Name} pressed {e.Button}.", LogLevel.Debug);
            
            if (e.Button.IsActionButton())
            {
                // Assumes furnaces can be built only on the farm and checks if player is on the farm map
                if (!Game1.currentLocation.IsFarm || !Game1.currentLocation.IsOutdoors)
                    return;

                foreach (Building building in furnaces)
                {
                    // The clicked tile
                    Vector2 tile = e.Cursor.GrabTile;

                    int furnaceTag = building.maxOccupants.Value;
                    IndustrialFurnaceController furnace = furnaceControllers[GetIndexOfFurnaceControllerWithTag(furnaceTag)];

                    // The mouth of the furnace
                    if (tile.X == building.tileX.Value + 1 && tile.Y == building.tileY.Value + 1)
                    {
                        PlaceItemsToTheFurnace(furnace, building);
                        Game1.playSound("coin");
                        
                        SendUpdateMessage();
                    }
                    // The output chest of the furnace
                    else if (tile.X == building.tileX.Value + 3 && tile.Y == building.tileY.Value + 1)
                    {
                        CollectItemsFromTheFurnace(furnace);
                    }
                }
            }
        }


        /// <summary>The event called when the day starts.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            if (Game1.player.IsMainPlayer)
            {
                // Finish smelting items
                foreach (IndustrialFurnaceController furnace in furnaceControllers)
                {
                    if (furnace.CurrentlyOn)
                    {
                        FinishSmelting(furnace);
                    }
                }

                SendUpdateMessage();
            }
            else if (modSaveData is null)
            {
                Helper.Multiplayer.SendMessage<RequestSaveData>(new RequestSaveData(Game1.player.UniqueMultiplayerID), requestSaveData, new string[] { ModManifest.UniqueID });
            }
        }


        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBuildingListChanged(object sender, BuildingListChangedEventArgs e)
        {
            // Add added furnaces to the controller list
            foreach (Building building in e.Added)
            {
                if (IsBuildingIndustrialFurnace(building))
                {
                    furnaces.Add(building);

                    // Add the controller that takes care of the functionality of the furnace
                    IndustrialFurnaceController controller = new IndustrialFurnaceController(furnacesBuilt, false);
                    furnaceControllers.Add(controller);
                    furnacesBuilt++;
                }
            }

            // Remove destroyed furnaces from the controller list
            foreach (Building building in e.Removed)
            {
                if (IsBuildingIndustrialFurnace(building))
                {
                    int index = GetIndexOfFurnaceControllerWithTag(building.maxOccupants.Value);

                    if (index > -1)
                    {
                        furnaceControllers.RemoveAt(index);
                    }

                    furnaces.Remove(building);
                }
            }
        }


        /// <summary>The event called after an active menu is opened or closed.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            // Add the blueprint
            if (e.NewMenu is CarpenterMenu)
            {
                IList<BluePrint> blueprints = this.Helper.Reflection
                    .GetField<List<BluePrint>>(e.NewMenu, "blueprints")
                    .GetValue();

                // Add furnace blueprint, and tag it uniquely based on how many have been built
                blueprints.Add(new BluePrint(furnaceBuildingType)
                {
                    maxOccupants = furnacesBuilt,
                });
            }
        }


        /// <summary></summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRenderedWorld(object sender, RenderedWorldEventArgs e)
        {
            
            foreach (Building building in furnaces)
            {
                // Assumes the furnace is built in the farm to avoid rendering the notification bubble anywhere else
                if (!Game1.player.currentLocation.IsFarm || !Game1.player.currentLocation.IsOutdoors) continue;

                // This gets called before building list gets changed, so check if the furnace has been added yet
                int index = GetIndexOfFurnaceControllerWithTag(building.maxOccupants.Value);
                if (index == -1) continue;

                IndustrialFurnaceController furnaceController = furnaceControllers[index];

                // Copied from Mill.cs draw(SpriteBatch b) with slight edits

                // Check if there is items to render
                if (furnaceController.output.items.Count <= 0 || furnaceController.output.items[0] == null)
                    return;

                // Get the bobbing from current time
                float num = (float)(4.0 * Math.Round(Math.Sin(DateTime.Now.TimeOfDay.TotalMilliseconds / 250.0), 2));

                e.SpriteBatch.Draw(Game1.mouseCursors,
                                   Game1.GlobalToLocal(Game1.viewport, new Vector2(building.tileX.Value * 64 + 180, building.tileY.Value * 64 - 64 + num)),
                                   new Rectangle?(new Rectangle(141, 465, 20, 24)),
                                   Color.White * 0.75f,
                                   0.0f,
                                   Vector2.Zero,
                                   4f,
                                   SpriteEffects.None,
                                   (float)((building.tileY.Value + 1) * 64 / 10000.0 + 9.99999997475243E-07 + building.tileX.Value / 10000.0));

                e.SpriteBatch.Draw(Game1.objectSpriteSheet,
                                   Game1.GlobalToLocal(Game1.viewport, new Vector2(building.tileX.Value * 64 + 185 + 32 + 4, building.tileY.Value * 64 - 32 + 8 + num)),
                                   new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, furnaceController.output.items[0].ParentSheetIndex, 16, 16)),
                                   Color.White * 0.75f,
                                   0.0f,
                                   new Vector2(8f, 8f),
                                   4f,
                                   SpriteEffects.None,
                                   (float)((building.tileY.Value + 1) * 64 / 10000.0 + 9.99999974737875E-06 + building.tileX.Value / 10000.0));
            }
        }


        /// <summary>Place items to the furnace</summary>
        /// <param name="furnace">The furnace controller</param>
        /// <param name="building">The real building</param>
        private void PlaceItemsToTheFurnace(IndustrialFurnaceController furnace, Building building)
        {
            //this.Monitor.Log($"{Game1.player.Name} tried to smelt something.", LogLevel.Debug);

            // Items can be placed only if the furnace is NOT on
            if (furnace.CurrentlyOn)
            {
                DisplayMessage(i18n.Get("message.furnace-running"), 3, "cancel");
                return;
            }

            // Get the current held object
            StardewValley.Object heldItem = Game1.player.ActiveObject;
            if (heldItem == null) return;

            int objectId = heldItem.ParentSheetIndex;
            SmeltingRule rule = newSmeltingRules.GetSmeltingRuleFromInputID(objectId);

            // Check if the object is on the smeltables list
            if (rule != null)
            {
                int amount = heldItem.Stack;

                // Check if the player has enough to smelt
                if (amount >= rule.InputItemAmount)
                {
                    // Remove multiples of the required input amount
                    int smeltAmount = amount / rule.InputItemAmount;
                    Game1.player.removeItemsFromInventory(objectId, smeltAmount * rule.InputItemAmount);
                    furnace.AddItemsToSmelt(objectId, smeltAmount * rule.InputItemAmount);
                }
                else
                {
                    DisplayMessage(i18n.Get("message.need-more-ore", new { oreAmount = rule.InputItemAmount }), 3, "cancel");
                }
            }
            // Check if the player tries to put coal in the furnace and start the smelting
            else if (objectId == StardewValley.Object.coal && !furnace.CurrentlyOn)
            {
                // The input has items to smelt
                if (furnace.input.items.Count > 0)
                {
                    if (heldItem.Stack >= config.CoalAmount)
                    {
                        Game1.player.removeItemsFromInventory(objectId, config.CoalAmount);

                        if (config.InstantSmelting)
                            FinishSmelting(furnace);
                        else
                        {
                            furnace.ChangeCurrentlyOn(true);
                            UpdateTexture(building, true);
                        }

                        Game1.playSound("furnace");
                    }
                    else
                    {
                        DisplayMessage(i18n.Get("message.more-coal", new { coalAmount = config.CoalAmount }), 3, "cancel");
                    }
                }
                else
                {
                    DisplayMessage(i18n.Get("message.place-something-first"), 3, "cancel");
                }
            }
            else
            {
                DisplayMessage(i18n.Get("message.cant-smelt-this"), 3, "cancel");
            }
        }


        private void CollectItemsFromTheFurnace(IndustrialFurnaceController furnace)
        {
            // Clear the output of removed items
            furnace.output.clearNulls();
            //this.Monitor.Log("The furnace output currently has " + furnace.output.items.Count + " items", LogLevel.Debug);

            //this.Monitor.Log("Player " + Game1.player.UniqueMultiplayerID + " tries to open the output", LogLevel.Debug);

            // Show output chest only if it contains something
            if (furnace.output.items.Count == 0) return;

            // Under construction logic for the mutex
            /*furnace.output.mutex.RequestLock((Action)(() =>
            {
                this.Monitor.Log("Player " + Game1.player.UniqueMultiplayerID + " succeeds!", LogLevel.Debug);
                
                // TODO: Create new menu that prevents player from placing items inside the output chest
                Game1.activeClickableMenu = (IClickableMenu)new ItemGrabMenu(furnace.output.items, false, true, new InventoryMenu.highlightThisItem(InventoryMenu.highlightAllItems),
                    null, (string)null,
                    new ItemGrabMenu.behaviorOnItemSelect((item, farmer) => furnace.GrabItemFromChest(item, farmer, this)),
                    false, true, true, true, false, 1, null, -1, null);
            }), (Action) (() =>
            {
                this.Monitor.Log("Player " + Game1.player.UniqueMultiplayerID + " fails", LogLevel.Debug);
            }));*/

            // Display the menu for the output chest
            Game1.activeClickableMenu = (IClickableMenu)new ItemGrabMenu(furnace.output.items, false, true, new InventoryMenu.highlightThisItem(InventoryMenu.highlightAllItems),
                    null, (string)null,
                    new ItemGrabMenu.behaviorOnItemSelect((item, farmer) => furnace.GrabItemFromChest(item, farmer, this)),
                    false, true, true, true, false, 1, null, -1, null);

            //this.Monitor.Log("The output is locked: " + furnace.output.mutex.IsLocked() + " The output is locked by " + Game1.player.UniqueMultiplayerID + ": " + furnace.output.mutex.IsLockHeld(), LogLevel.Debug);
        }


        private void FinishSmelting(IndustrialFurnaceController furnace)
        {
            // TODO: Add checks to prevent loss of items, since it is possible that 'output amount' > 'input amount'

            // Collect the object data to a dictionary first to fix losing items with over 999 stacks
            Dictionary<int, int> smeltablesDictionary = new Dictionary<int, int>();

            foreach (Item item in furnace.input.items)
            {
                int objectId = item.ParentSheetIndex;

                if (smeltablesDictionary.ContainsKey(objectId))
                {
                    smeltablesDictionary[objectId] += item.Stack;
                }
                else
                {
                    smeltablesDictionary.Add(objectId, item.Stack);
                }
            }

            // Now the dictionary consists of ItemID: Amount
            foreach (KeyValuePair<int, int> kvp in smeltablesDictionary)
            {
                SmeltingRule rule = newSmeltingRules.GetSmeltingRuleFromInputID(kvp.Key);

                if (rule is null)
                {
                    this.Monitor.Log("Item with ID " + kvp.Key + " wasn't in the smelting rules despite being in the input chest!", LogLevel.Error);
                    continue;
                }

                // Add the result defined by the smelting rule to the output chest
                furnace.AddItemsToSmeltedChest(rule.OutputItemID, (kvp.Value / rule.InputItemAmount) * rule.OutputItemAmount);
            }

            for (int i = 0; i < furnace.input.items.Count; i++)
            {
                furnace.input.items[i] = null;
            }
            furnace.input.clearNulls();
            furnace.ChangeCurrentlyOn(false);

            // Update the texture of the furnace
            foreach (Building building in furnaces)
            {
                if (building.maxOccupants.Value == furnace.ID)
                    UpdateTexture(building, false);
            }
        }


        /// <summary>Checks if the building is an industrial furnace based on its buildingType</summary>
        private bool IsBuildingIndustrialFurnace(Building building)
        {
            return building.buildingType.Value.Equals(furnaceBuildingType);
        }


        /// <summary>Returns the index of the matching controller in the furnaces list</summary>
        /// <param name="tag">The tag of searched furnace controller</param>
        /// <returns>Either the index or -1 if no tag matches are found</returns>
        private int GetIndexOfFurnaceControllerWithTag(int tag)
        {
            for (int i = 0; i < furnaceControllers.Count; i++)
            {
                // Assumes the furnace has been added to the list once
                if (furnaceControllers[i].ID == tag)
                {
                    return i;
                }
            }

            return -1;
        }


        /// <summary>Switches the building's texture between ON and OFF versions</summary>
        /// <param name="building"></param>
        /// <param name="currentlyOn"></param>
        private void UpdateTexture(Building building, bool currentlyOn)
        {
            if (currentlyOn)
            {
                building.texture = new Lazy<Texture2D>(() => this.furnaceOn);
            }
            else
            {
                building.texture = new Lazy<Texture2D>(() => this.furnaceOff);
            }
        }


        /// <summary>Updates the textures of all furnaces. Used to sync with multiplayer save data changes.</summary>
        private void UpdateTextures()
        {
            for (int i = 0; i < furnaceControllers.Count; i++)
            {
                int id = furnaceControllers[i].ID;

                foreach (Building building in furnaces)
                {
                    if (building.maxOccupants.Value == id)
                    {
                        UpdateTexture(building, furnaceControllers[i].CurrentlyOn);
                    }
                }
            }
        }


        /// <summary>Displays a HUD message of defined type with a possible sound effect</summary>
        /// <param name="s">Displayed message</param>
        /// <param name="type">Message type</param>
        /// <param name="sound">Sound effect</param>
        private void DisplayMessage(string s, int type, string sound = null)
        {
            Game1.addHUDMessage(new HUDMessage(s, type));

            if (sound != null)
            {
                Game1.playSound(sound);
            }
        }


        /// <summary>Remove rules that depend on not installed mods</summary>
        private void CheckSmeltingRules()
        {
            newSmeltingRules.SmeltingRules.RemoveAll(item => item.RequiredModID != null && !Helper.ModRegistry.IsLoaded(item.RequiredModID));
        }


        /// <summary>Update the furnace data from the save data</summary>
        private void InitializeFurnaceControllers(bool readSaveData)
        {
            // Initialize the lists to prevent data leaking from previous games
            furnaces.Clear();
            furnaceControllers.Clear();

            // Load the saved data. If not present, initialize new
            if (readSaveData)
                modSaveData = this.Helper.Data.ReadSaveData<ModSaveData>(controllerDataSaveKey);

            if (modSaveData is null)
            {
                modSaveData = new ModSaveData();
            }
            else
            {
                modSaveData.ParseModSaveDataToControllers(furnaceControllers);
            }

            // Update furnacesBuilt counter to match the highest id of built furnaces (+1)
            int highestId = -1;
            for (int i = 0; i < furnaceControllers.Count; i++)
            {
                if (furnaceControllers[i].ID > highestId) highestId = furnaceControllers[i].ID;
            }
            furnacesBuilt = highestId + 1;

            // Repopulate the list of furnaces, only checks the farm!
            foreach (Building building in ((BuildableGameLocation)Game1.getFarm()).buildings)
            {
                if (IsBuildingIndustrialFurnace(building))
                    furnaces.Add(building);
            }
        }


        /// <summary>Update the save data to match the controllers data</summary>
        private void InitializeSaveData()
        {
            modSaveData.ClearOldData();
            modSaveData.ParseControllersToModSaveData(furnaceControllers);
        }
    }


    /// <summary>
    /// Interface for the GenericModConfigMenu api.
    /// </summary>
    public interface IGenericModConfigMenuAPI
    {
        void RegisterModConfig(IManifest mod, Action revertToDefault, Action saveToFile);
        void RegisterLabel(IManifest mod, string labelName, string labelDesc);
        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<bool> optionGet, Action<bool> optionSet);
        void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet, int min, int max);
    }
}
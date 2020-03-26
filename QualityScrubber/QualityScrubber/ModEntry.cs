using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;


namespace QualityScrubber
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : StardewModdingAPI.Mod
    {
        private const string qualityScrubberType = "Quality Scrubber";

        private ModConfig config;

        private QualityScrubberController controller;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            config = helper.ReadConfig<ModConfig>();

            controller = new QualityScrubberController(Monitor, config.AllowPreserves, config.AllowHoney, config.Duration);

            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        }

        
        public override object GetApi()
        {
            return new QualityScrubberApi(controller);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsPlayerFree)
                return;

            if (e.Button.IsActionButton())
            {
                foreach (var objects in Game1.player.currentLocation.Objects)
                {
                    foreach (var kvp in objects)
                    {
                        if (kvp.Value.Name == qualityScrubberType && kvp.Key == e.Cursor.GrabTile)
                        {
                            // See if the machine accepts the item, suppress the input to prevent the eating menu from opening
                            if (controller.CanProcess(Game1.player.ActiveObject, kvp.Value))
                            {
                                controller.StartProcessing(Game1.player.ActiveObject, kvp.Value, Game1.player);
                                Helper.Input.Suppress(e.Button);
                            }
                        }
                    }
                }
            }
        }
    }
}
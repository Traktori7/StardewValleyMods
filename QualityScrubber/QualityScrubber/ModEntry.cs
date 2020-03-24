using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace QualityScrubber
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
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
            if (!Context.IsWorldReady)
                return;

            if (e.Button.IsActionButton())
            {
                foreach (var esine in Game1.player.currentLocation.Objects)
                {
                    foreach (var kvp in esine)
                    {
                        if (kvp.Value.Name == "Quality Scrubber" && kvp.Key == e.Cursor.GrabTile)
                        {
                            StartProcessing(Game1.player.ActiveObject, kvp.Value);

                            /*if (((QualityScrubber)kvp.Value).performObjectDropInAction(Game1.player.ActiveObject, false, Game1.player))
                            {
                                this.Monitor.Log("Testi onnistui", LogLevel.Debug);

                            }*/
                        }
                    }
                }
            }
        }


        private void StartProcessing(StardewValley.Object inputItem, StardewValley.Object machine)
        {
            this.Monitor.Log("Machine starts to dequalify the item", LogLevel.Debug);
            machine.heldObject.Value = inputItem;
        }
    }

    public class QualityScrubber : StardewValley.Object
    {

        public override bool performObjectDropInAction(Item dropInItem, bool probe, Farmer who)
        {
            return true;
        }
    }
}
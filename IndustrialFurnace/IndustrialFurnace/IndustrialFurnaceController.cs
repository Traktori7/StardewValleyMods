using StardewValley;
using StardewValley.Objects;
using StardewValley.Menus;
using StardewValley.Buildings;
using System;
using Netcode;

namespace IndustrialFurnace
{
    /// <summary>
    /// Furnace data handling class.
    /// </summary>
    public class IndustrialFurnaceController
    {
        private ModEntry mod;

        public readonly int ID;
        public bool CurrentlyOn;

        public Chest input = new Chest();
        public Chest output = new Chest();

        public Building furnace;
        public LightSource lightSource;


        public IndustrialFurnaceController(int tag, bool currentlyOn, ModEntry mod)
        {
            this.mod = mod;
            this.ID = tag;
            this.CurrentlyOn = currentlyOn;
        }


        public void ChangeCurrentlyOn(bool value)
        {
            CurrentlyOn = value;
        }


        public void AddItemsToSmelt(int objectId, int amount)
        {
            StardewValley.Object item = new StardewValley.Object(objectId, amount);
            input.addItem(item);
        }


        public void AddItemsToSmeltedChest(int objectId, int amount)
        {
            StardewValley.Object item = new StardewValley.Object(objectId, amount);
            output.addItem(item);
        }


        public void GrabItemFromChest(Item item, Farmer who)
        {
            if (!who.couldInventoryAcceptThisItem(item))
                return;

            TakeFromOutput(item);

            Game1.activeClickableMenu = (IClickableMenu)new ItemGrabMenu(output.items, false, true,
                new InventoryMenu.highlightThisItem(InventoryMenu.highlightAllItems),
                null, (string)null,
                new ItemGrabMenu.behaviorOnItemSelect((itemParam, farmer) => GrabItemFromChest(itemParam, farmer)),
                false, true, true, true, false, 1, (Item)output, -1, (object)output);
        }


        public void TakeFromOutput(Item item)
        {
            output.items.Remove(item);
            output.clearNulls();

            mod.SendUpdateMessage();
        }
    }
}

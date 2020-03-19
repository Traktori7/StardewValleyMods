using StardewValley;
using StardewValley.Objects;
using StardewValley.Menus;
using System;
using Netcode;

namespace IndustrialFurnace
{
    /// <summary>
    /// Furnace data handling class.
    /// </summary>
    public class IndustrialFurnaceController
    {
        public readonly int ID;
        public bool CurrentlyOn;

        public Chest input = new Chest();
        public Chest output = new Chest();


        public IndustrialFurnaceController(int tag, bool currentlyOn)
        {
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


        public void GrabItemFromChest(Item item, Farmer who, ModEntry mod)
        {
            if (!who.couldInventoryAcceptThisItem(item))
                return;

            output.items.Remove(item);
            output.clearNulls();

            Game1.activeClickableMenu = (IClickableMenu)new ItemGrabMenu(output.items, false, true,
                new InventoryMenu.highlightThisItem(InventoryMenu.highlightAllItems),
                null, (string)null,
                new ItemGrabMenu.behaviorOnItemSelect((itemParam, farmer) => GrabItemFromChest(itemParam, farmer, mod)),
                false, true, true, true, false, 1, (Item)output, -1, (object)output);

            mod.SendUpdateMessage();
        }
    }
}

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
        public readonly int Id;
        public bool CurrentlyOn;

        public Chest input = new Chest();
        public Chest output = new Chest();


        public IndustrialFurnaceController(int tag, bool currentlyOn)
        {
            this.Id = tag;
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

        /*public void GrabItemFromInventory(Item item, Farmer who)
        {
            if (item.Stack == 0)
                item.Stack = 1;
            Item obj = output.addItem(item);
            if (obj == null)
                who.removeItemFromInventory(item);
            else
                obj = who.addItemToInventory(obj);
            this.output.clearNulls();
            int id = Game1.activeClickableMenu.currentlySnappedComponent != null ? Game1.activeClickableMenu.currentlySnappedComponent.myID : -1;
            Game1.activeClickableMenu = (IClickableMenu)new ItemGrabMenu(output.items, false, true, new InventoryMenu.highlightThisItem(InventoryMenu.highlightAllItems),
                new ItemGrabMenu.behaviorOnItemSelect(GrabItemFromInventory), (string)null,
                new ItemGrabMenu.behaviorOnItemSelect(GrabItemFromChest), false, true, true, true, true, 1, (Item)output, -1, (object)output);
            (Game1.activeClickableMenu as ItemGrabMenu).heldItem = obj;
            if (id == -1)
                return;
            Game1.activeClickableMenu.currentlySnappedComponent = Game1.activeClickableMenu.getComponentWithID(id);
            Game1.activeClickableMenu.snapCursorToCurrentSnappedComponent();
        }*/

        public void GrabItemFromChest(Item item, Farmer who, ModEntry mod)
        {
            if (!who.couldInventoryAcceptThisItem(item))
                return;
            output.items.Remove(item);
            output.clearNulls();
            Game1.activeClickableMenu = (IClickableMenu)new ItemGrabMenu(output.items, false, true, new InventoryMenu.highlightThisItem(InventoryMenu.highlightAllItems),
                null, (string)null,
                new ItemGrabMenu.behaviorOnItemSelect((itemParam, farmer) => GrabItemFromChest(itemParam, farmer, mod)), false, true, true, true, false, 1, (Item)output, -1, (object)output);

            mod.SendUpdateMessage();
        }
    }
}

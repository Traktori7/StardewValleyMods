using StardewValley.Objects;

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
    }
}

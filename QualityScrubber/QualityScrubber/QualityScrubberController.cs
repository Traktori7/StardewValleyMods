using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using SObject = StardewValley.Object;


namespace QualityScrubber
{
	public class QualityScrubberController
	{
        private IMonitor Monitor { get; set; }
        public bool AllowPreserves { get; set; }
        public bool AllowHoney { get; set; }
        public int Duration { get; set; }


        public QualityScrubberController(IMonitor monitor, bool allowPreserves, bool allowHoney, int duration)
        {
            this.Monitor = monitor;
            this.AllowPreserves = allowPreserves;
            this.AllowHoney = allowHoney;
            this.Duration = duration;
        }


        public bool CanProcess(Item inputItem, SObject machine)
        {
            if (inputItem is null)
                return false;

            if (machine.heldObject.Value != null)
            {
                //Monitor.Log("The machine is already scrubbing!", LogLevel.Debug);
                return false;
            }

            if (!(inputItem is SObject inputObject))
            {
                //Monitor.Log("You can't scrub this!", LogLevel.Debug);
                return false;
            }

            if (inputObject.Quality == SObject.lowQuality)
            {
                //Monitor.Log("You can't scrub this any more!", LogLevel.Debug);
                return false;
            }

            // Ignore roe/wine/juice/jelly/pickles
            if (!AllowPreserves && inputObject.preserve.Value != null)
            {
                //Monitor.Log("You can't scrub these yet!", LogLevel.Debug);
                return false;
            }

            // Ignore honey...
            if (!AllowHoney && inputObject.ParentSheetIndex == 340)
            {
                //Monitor.Log("You can't scrub honey!", LogLevel.Debug);
                return false;
            }

            return true;
        }


        public SObject GetOutputObject(Item inputObject)
        {
            return new SObject(Vector2.Zero, inputObject.ParentSheetIndex, 1)
            {
                //Name = inputObject.Name,
                // This doesn't seem to do anything...
                //DisplayName = inputObject.DisplayName,
                Quality = SObject.lowQuality
            };
        }


        public void StartProcessing(SObject inputObject, SObject machine, Farmer who)
        {

            /*if (jsonAssetsApiFound)
            {
                int temp = jsonAssetsApi.GetObjectId(inputObject.Name);

                if (inputObject.ParentSheetIndex == temp)
                {
                    Monitor.Log($"The IDs matched for {inputObject.Name}");
                }
            }*/

            // honey example maybe LoadOutputName outputconfigcontroller.cs

            // Try to handle roe/wine/juice/jelly/pickles
            /*if (inputObject is ColoredObject)
            {
                itemToDequalify = new ColoredObject(inputObject.ParentSheetIndex, 1, ((ColoredObject)inputObject).color.Value);
                itemToDequalify.preservedParentSheetIndex.Value = inputObject.preservedParentSheetIndex.Value;
            }
            else
                itemToDequalify = new StardewValley.Object(Vector2.Zero, inputItem.ParentSheetIndex, 1);*/

            SObject outputObject = GetOutputObject(inputObject);
            // Fix the price?

            //this.Monitor.Log("Machine starts to scrub the item", LogLevel.Debug);
            machine.heldObject.Value = outputObject;
            machine.MinutesUntilReady = Duration;

            // Remove the item from inventory, if everything was successful
            if (who.ActiveObject.Stack == 1)
            {
                //who.Items.Remove(who.ActiveObject);
                who.ActiveObject = null;
            }
            else
            {
                who.ActiveObject.Stack -= 1;
            }
        }
    }
}

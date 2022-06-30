using System.Collections.Generic;
using StardewValley;

namespace IndustrialFurnace.Data
{
	/// <summary>
	/// Data class for the save data.
	/// </summary>
	public class ModSaveData
	{
		public List<int> FurnaceControllerId { get; set; }
		public List<bool> FurnaceControllerCurrentlyOn { get; set; }
		public List<Dictionary<int, int>> FurnaceControllerInput { get; set; }
		public List<Dictionary<int, int>> FurnaceControllerOutput { get; set; }


		public ModSaveData()
		{
			FurnaceControllerId = new List<int>();
			FurnaceControllerCurrentlyOn = new List<bool>();
			FurnaceControllerInput = new List<Dictionary<int, int>>();
			FurnaceControllerOutput = new List<Dictionary<int, int>>();
		}


		public void ClearOldData()
		{
			FurnaceControllerId.Clear();
			FurnaceControllerCurrentlyOn.Clear();
			FurnaceControllerInput.Clear();
			FurnaceControllerOutput.Clear();
		}


		/// <summary>Parses the save data from the furnace controller data</summary>
		public void ParseControllersToModSaveData(List<IndustrialFurnaceController> furnaces)
		{
			for (int i = 0; i < furnaces.Count; i++)
			{
				FurnaceControllerId.Add(furnaces[i].ID);
				FurnaceControllerCurrentlyOn.Add(furnaces[i].CurrentlyOn);

				Dictionary<int, int> inputChest = new Dictionary<int, int>();

				for (int j = 0; j < furnaces[i].input.items.Count; j++)
				{
					Item tempItem = furnaces[i].input.items[j];

					if (inputChest.ContainsKey(tempItem.ParentSheetIndex))
						inputChest[tempItem.ParentSheetIndex] += tempItem.Stack;
					else
						inputChest.Add(tempItem.ParentSheetIndex, tempItem.Stack);
				}

				FurnaceControllerInput.Add(inputChest);


				Dictionary<int, int> outputChest = new Dictionary<int, int>();

				for (int j = 0; j < furnaces[i].output.items.Count; j++)
				{
					Item tempItem = furnaces[i].output.items[j];

					if (outputChest.ContainsKey(tempItem.ParentSheetIndex))
						outputChest[tempItem.ParentSheetIndex] += tempItem.Stack;
					else
						outputChest.Add(tempItem.ParentSheetIndex, tempItem.Stack);
				}

				FurnaceControllerOutput.Add(outputChest);
			}
		}


		/// <summary>Parses the furnace controller data from the save data</summary>
		public void ParseModSaveDataToControllers(List<IndustrialFurnaceController> furnaces, ModEntry mod)
		{
			// Assume the lists are equally as long

			for (int i = 0; i < FurnaceControllerId.Count; i++)
			{
				IndustrialFurnaceController controller = new IndustrialFurnaceController(FurnaceControllerId[i], FurnaceControllerCurrentlyOn[i], mod);

				Dictionary<int, int> tempDictionary = FurnaceControllerInput[i];
				foreach (KeyValuePair<int, int> kvp in tempDictionary)
				{
					Object item = new Object(kvp.Key, kvp.Value);
					controller.input.addItem(item);
				}

				tempDictionary = FurnaceControllerOutput[i];
				foreach (KeyValuePair<int, int> kvp in tempDictionary)
				{
					Object item = new Object(kvp.Key, kvp.Value);
					controller.output.addItem(item);
				}

				furnaces.Add(controller);
			}
		}
	}
}

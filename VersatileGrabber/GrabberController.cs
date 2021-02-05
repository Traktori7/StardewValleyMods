using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace VersatileGrabber
{
	class GrabberController
	{
		public readonly Dictionary<Tuple<GameLocation, Vector2>, VersatileGrabber> versatileGrabbers = new Dictionary<Tuple<GameLocation, Vector2>, VersatileGrabber>();

		/**/
		/* Static methods */
		/**/

		/// <summary>
		/// Checks if the item should be converted to a versatile grabber
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static bool ItemShouldBeVersatileGrabber(SObject item)
		{
			// Don't convert if it already is to avoid an infinite loop
			if (item is VersatileGrabber)
				return false;

			// Check if the item is a big craftable
			if (item.bigCraftable.Value)
			{
				if (item.ParentSheetIndex == ModEntry.GrabberID)
				{
					return true;
				}
			}

			return false;
		}


		/**/
		/* Public methods */
		/**/

		public void SaveGrabbers()
		{
			ModSaveData saveData = new ModSaveData();

			foreach (var grabber in versatileGrabbers)
			{
				Tuple<GameLocation, Vector2> tuple = grabber.Key;

				// Create the save data for a grabber
				GrabberSaveData grabberSaveData = new GrabberSaveData()
				{
					LocationName = tuple.Item1.Name,
					TileX = tuple.Item2.X,
					TileY = tuple.Item2.Y
				};
				saveData.VersatileGrabbers.Add(grabberSaveData);

				// Convert the grabber to SObject
				SObject grabberObject = grabber.Value.ToObject();

				// Save the inventory to a chest and give it for the dummy object to hold
				Chest tempChest = new Chest();
				for (int i = 0; i < grabber.Value.items.Count; i++)
				{
					tempChest.items.Add(grabber.Value.items[i]);
				}
				grabberObject.heldObject.Value = tempChest;

				// Replace the grabber with the dummy SObject
				tuple.Item1.Objects[tuple.Item2] = grabberObject;
			}

			ModEntry.ModHelper.Data.WriteSaveData(ModEntry.saveDataKey, saveData);
		}



		public void AddGrabber(VersatileGrabber grabber, GameLocation location, Vector2 tile)
		{
			Tuple<GameLocation, Vector2> position = new Tuple<GameLocation, Vector2>(location, tile);

			if (versatileGrabbers.ContainsKey(position))
			{
				ModEntry.ModMonitor.Log($"The grabber at {tile} has already been registered.", LogLevel.Error);
				return;
			}

			versatileGrabbers.Add(position, grabber);
		}


		public void RemoveGrabber(VersatileGrabber grabber, GameLocation location, Vector2 tile)
		{
			Tuple<GameLocation, Vector2> position = new Tuple<GameLocation, Vector2>(location, tile);

			if (versatileGrabbers.ContainsKey(position))
			{
				versatileGrabbers.Remove(position);
			}
			else
			{
				ModEntry.ModMonitor.Log($"Couldn't remove the grabber at tile {tile}. It's not registered anymore.", LogLevel.Error);
			}
		}
	}
}

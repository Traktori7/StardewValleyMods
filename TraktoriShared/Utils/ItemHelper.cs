using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using MiniDungeons.Data;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Monsters;
using SObject = StardewValley.Object;


namespace TraktoriShared.Utils
{
	internal static class ItemHelper
	{
		/// <summary>
		/// TODO: todo
		/// </summary>
		/// <param name="qualifiedItemID"></param>
		/// <returns></returns>
		internal static int? ParseQualifiedItemID(string qualifiedItemID)
		{
			string[] split = qualifiedItemID.Split('(', ')');

			if (split[0].Equals("O"))
			{
				return int.Parse(split[1]);
			}

			return null;
		}


		/// <summary>
		/// WARNING! This method might be super heavy to run. Consider caching the values in to a dictionary.
		/// </summary>
		/// <param name="objectName">The object's internal name</param>
		/// <returns>Returns the key in Game1.objectInformation, or 0 if the object wasn't found.</returns>
		internal static int GetIDFromObjectName(string objectName)
		{
			// TODO: Cache the result in a string-int dictionary?
			// Special case handling needed atleast for: Weeds, Stone
			// Also probably Rings too, since they're stored in objectInformation, but might construct differently
			ReadOnlySpan<char> objectNameSpan = objectName.AsSpan();

			foreach (KeyValuePair<int, string> kvp in Game1.objectInformation)
			{
				ReadOnlySpan<char> splitName = kvp.Value.AsSpan(0, kvp.Value.IndexOf('/'));

				if (objectNameSpan.Equals(splitName, StringComparison.OrdinalIgnoreCase))
				{
					return kvp.Key;
				}
			}

			return 0;
		}


		/// <summary>
		/// Gets the item matching the given name. Currently about 10 times slower than GetIDFromObjectName.
		/// Probably because of the objectInformation split for the Ring category.
		/// </summary>
		/// <param name="qualifiedItemID">Item name in the format "(ItemType)ItemName" or just "ItemName", which defaults to "(O)ItemName"</param>
		/// <returns>The item, or an error object.</returns>
		internal static Item GetItemFromQualifiedItemID(string qualifiedItemID)
		{
			Item? returnItem = null;
			/*string[] split = qualifiedItemID.Split(new[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);

			string itemType;
			string itemName;

			if (split.Length == 1)
			{
				itemType = "O";
				itemName = split[0];
			}
			else
			{
				itemType = split[0];
				itemName = split[1];
			}*/

			int index = qualifiedItemID.IndexOf(')');

			string itemType;
			string itemName = qualifiedItemID[(index + 1)..];

			if (index > 0)
			{
				itemType = qualifiedItemID[1..index];
			}
			else
			{
				itemType = "O";
			}

			switch (itemType)
			{
				case "O":   // objects
					if (itemName.Contains("Ring") || itemName.Contains("Band"))
					{
						foreach (KeyValuePair<int, string> kvp in Game1.objectInformation)
						{
							//ReadOnlySpan<char> splitName = kvp.Value.AsSpan(0, kvp.Value.IndexOf('/'));
							string[] objectData = kvp.Value.Split('/');

							//ReadOnlySpan<char> type = kvp.Value.AsSpan();

							if (itemName.Equals(objectData[0]))
							{
								// Exclude wedding ring, just in case
								if (kvp.Key != 801 && objectData.Length >= 4 && objectData[3].Equals("Ring"))
								{
									returnItem = new StardewValley.Objects.Ring(kvp.Key);
								}
								else
								{
									returnItem = new SObject(kvp.Key, 1);
								}

								break;
							}
						}
					}
					else
					{
						returnItem = new SObject(GetIDFromObjectName(itemName), 1);
					}
					
					break;
				case "BC":  // big craftables
					foreach (var kvp in Game1.bigCraftablesInformation)
					{
						ReadOnlySpan<char> splitName = kvp.Value.AsSpan(0, kvp.Value.IndexOf('/'));

						if (MemoryExtensions.Equals(splitName, itemName, StringComparison.Ordinal))
						{
							returnItem = new SObject(Vector2.Zero, kvp.Key);
							break;
						}
					}
					break;
				case "B":   // boots
					foreach (var kvp in Game1.content.Load<Dictionary<int, string>>("Data\\Boots"))
					{
						ReadOnlySpan<char> splitName = kvp.Value.AsSpan(0, kvp.Value.IndexOf('/'));

						if (MemoryExtensions.Equals(splitName, itemName, StringComparison.Ordinal))
						{
							returnItem = new StardewValley.Objects.Boots(kvp.Key);
							break;
						}
					}
					break;
				case "H":   // hats
					foreach (var kvp in Game1.content.Load<Dictionary<int, string>>("Data\\hats"))
					{
						ReadOnlySpan<char> splitName = kvp.Value.AsSpan(0, kvp.Value.IndexOf('/'));

						if (MemoryExtensions.Equals(splitName, itemName, StringComparison.Ordinal))
						{
							returnItem = new StardewValley.Objects.Hat(kvp.Key);
							break;
						}
					}
					break;
				case "W":   // weapons
					foreach (var kvp in Game1.content.Load<Dictionary<int, string>>("Data\\weapons"))
					{
						ReadOnlySpan<char> splitName = kvp.Value.AsSpan(0, kvp.Value.IndexOf('/'));

						if (MemoryExtensions.Equals(splitName, itemName, StringComparison.Ordinal))
						{
							if (kvp.Key is 32 or 33 or 34)
							{
								returnItem = new StardewValley.Tools.Slingshot(kvp.Key);
							}
							else
							{
								returnItem = new StardewValley.Tools.MeleeWeapon(kvp.Key);
							}
							
							break;
						}
					}
					break;
				default:
					break;
			}

			return returnItem is not null ? returnItem : new SObject(0,1);
		}


		/// <summary>
		/// Places an item on the ground that can be picked up
		/// </summary>
		/// <param name="location">The location to spawn the item into</param>
		/// <param name="tile">The tile to spawn the item into</param>
		/// <param name="objectName">The internal name of item to spawn. Spawns weeds if the name doesn't match any defined.</param>
		internal static void PlacePickableItem(GameLocation? location, Point tile, string objectName)
		{
			int objectID = GetIDFromObjectName(objectName);
			PlacePickableItem(location, tile, objectID);
		}


		/// <summary>
		/// Places an item on the ground that can be picked up
		/// </summary>
		/// <param name="location">The location to spawn the item into</param>
		/// <param name="tile">The tile to spawn the item into</param>
		/// <param name="objectID">The ID of the item to spawn</param>
		internal static void PlacePickableItem(GameLocation? location, Point tile, int objectID)
		{
			SObject obj = new SObject(objectID, 1);
			PlacePickableItem(location, tile, obj);
		}


		/// <summary>
		/// Places an item on the ground that can be picked up
		/// </summary>
		/// <param name="location">The location to spawn the item into</param>
		/// <param name="tile">The tile to spawn the item into</param>
		/// <param name="obj">The item to spawn</param>
		internal static void PlacePickableItem(GameLocation? location, Point tile, SObject obj)
		{
			// Multiply the tile coordinates by 64 to get the right spawning point
			location?.dropObject(obj, tile.ToVector2() * 64f, Game1.viewport, true);
		}
	}
}

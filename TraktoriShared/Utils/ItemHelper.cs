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


namespace TraktoriShared.Utils
{
	internal static class ItemHelper
	{
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
		/// <param name="objectName">The objects internal name</param>
		/// <returns>The key in Game1.objectInformation</returns>
		internal static int? GetIDFromObjectName(string objectName)
		{
			ReadOnlySpan<char> objectNameSpan = objectName.AsSpan();
			var objectInformation = Game1.objectInformation;

			foreach (var item in objectInformation)
			{
				ReadOnlySpan<char> splitName = item.Value.AsSpan(0, item.Value.IndexOf('/'));

				if (splitName.Equals(objectNameSpan, StringComparison.OrdinalIgnoreCase))
				{
					return item.Key;
				}
			}

			return null;
		}


		/// <summary>
		/// Places an item on the ground that can be picked up
		/// </summary>
		/// <param name="location">The location to spawn the item into</param>
		/// <param name="tile">The tile to spawn the item into</param>
		/// <param name="objectName">The internal name of item to spawn</param>
		internal static void PlacePickableItem(GameLocation? location, Point tile, string objectName)
		{
			int objectID = TraktoriShared.Utils.ItemHelper.GetIDFromObjectName(objectName) ?? 0;
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
			StardewValley.Object obj = new StardewValley.Object(objectID, 1);
			PlacePickableItem(location, tile, obj);
		}


		/// <summary>
		/// Places an item on the ground that can be picked up
		/// </summary>
		/// <param name="location">The location to spawn the item into</param>
		/// <param name="tile">The tile to spawn the item into</param>
		/// <param name="obj">The item to spawn</param>
		internal static void PlacePickableItem(GameLocation? location, Point tile, StardewValley.Object obj)
		{
			// Multiply the tile coordinates by 64 to get the right spawning point
			location?.dropObject(obj, tile.ToVector2() * 64f, Game1.viewport, true);
		}
	}
}

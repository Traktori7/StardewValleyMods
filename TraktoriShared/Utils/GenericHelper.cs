using System;
using System.Collections.Generic;


namespace TraktoriShared.Utils
{
	internal class GenericHelper
	{
		/// <summary>
		/// Tries to find the index key from the provided dictionary for the item data that matches the given name ignoring case.
		/// Works only for dictionaries where the value is data delimited by / and the name is the first entry.
		/// </summary>
		/// <param name="dictionary">The dictionary containing the item data</param>
		/// <param name="itemName">The name of the item to look for</param>
		/// <param name="index">The index key for the item's data, if it was found</param>
		/// <returns>If the mathing item data was found in the dictionary</returns>
		internal static bool TryGetIndexByName(IDictionary<int, string>? dictionary, string itemName, out int index)
		{
			index = 0;

			if (dictionary is null)
			{
				return false;
			}

			ReadOnlySpan<char> objectNameSpan = itemName.AsSpan();

			foreach (KeyValuePair<int, string> kvp in dictionary)
			{
				ReadOnlySpan<char> splitName = kvp.Value.AsSpan(0, kvp.Value.IndexOf('/'));

				if (objectNameSpan.Equals(splitName, StringComparison.OrdinalIgnoreCase))
				{
					index = kvp.Key;
					return true;
				}
			}

			return false;
		}
	}
}

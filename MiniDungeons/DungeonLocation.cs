using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Monsters;


namespace MiniDungeons
{
	internal class DungeonLocation : GameLocation
	{
		public DungeonLocation(string name)
			: base(Path.Combine("Maps", name), name)
		{

		}
	}
}

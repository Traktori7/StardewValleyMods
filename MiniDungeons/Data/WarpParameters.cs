using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Monsters;
using xTile.Dimensions;

using TraktoriShared.Utils;


namespace MiniDungeons.Data
{
	internal class WarpParameters
	{
		public string TargetLocation;
		public Point Point;


		public WarpParameters(string parameters)
		{
			string[] split = parameters.Split(' ');
			TargetLocation = split[1];
			Point = new Point(Convert.ToInt32(split[2]), Convert.ToInt32(split[3]));
		}
	}
}

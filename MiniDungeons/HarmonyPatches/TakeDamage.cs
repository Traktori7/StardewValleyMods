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


namespace MiniDungeons.HarmonyPatches
{
	internal class TakeDamage
	{
		private static IMonitor Monitor = null!;


		internal static void Initialize(IMonitor monitor)
		{
			Monitor = monitor;
		}


		[HarmonyPriority(Priority.Last)]
		public static void TakeDamage_Postfix(Farmer __instance)
		{
			try
			{
				if (__instance.health <= 0 && DungeonManager.IsLocationMiniDungeon(Game1.currentLocation))
				{
					__instance.health = 1;
					DungeonManager.ExitDungeon(true);
				}
			}
			catch (Exception ex)
			{
				Monitor.Log("Harmony patch TakeDamage_Postfix failed", LogLevel.Error);
				Monitor.Log(ex.ToString(), LogLevel.Error);
			}
		}
	}
}

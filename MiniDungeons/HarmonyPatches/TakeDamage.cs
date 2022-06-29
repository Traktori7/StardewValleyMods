﻿using System;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;


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
				if (__instance.health <= 0 && ModEntry.config.enableDeathProtection
					&& DungeonManager.IsLocationMiniDungeon(Game1.currentLocation))
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

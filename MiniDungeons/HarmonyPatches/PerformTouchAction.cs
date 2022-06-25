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
	public class PerformTouchAction
	{
		private static IMonitor Monitor = null!;
		private static ITranslationHelper Translator = null!;

		private static DungeonManager DungeonManager = null!;

		//private static string[]? warpParameters;
		private static Data.WarpParameters? WarpParameters;
		private static readonly string answerYes = "Yes";
		private static readonly string answerNo = "No";
		private static readonly string answerRemove = "Remove";

		internal static void Initialize(IMonitor monitor, ITranslationHelper translationHelper, DungeonManager manager)
		{
			Monitor = monitor;
			Translator = translationHelper;
			DungeonManager = manager;
		}

		public static void PerformTouchAction_Postfix(string fullActionString)
		{
			try
			{
				if (fullActionString is not null)
				{
					string[] actionParams = fullActionString.Split(' ');

					if (actionParams[0].Equals(ModEntry.actionName))
					{
						WarpParameters = new Data.WarpParameters(fullActionString);
						OpenPortalBox();
					}
				}
			}
			catch (Exception ex)
			{
				Monitor.Log("Harmony patch PerformAction_Prefix failed", LogLevel.Error);
				Monitor.Log(ex.ToString(), LogLevel.Error);
			}
		}


		public static void OpenPortalBox()
		{
			List<Response> choices = new List<Response>()
			{
				new Response(answerYes, Translator.Get("dialogue.answer-yes")),
				new Response(answerNo, Translator.Get("dialogue.answer-no")),
				new Response(answerRemove, Translator.Get("dialogue.answer-remove"))
			};

			Game1.currentLocation.createQuestionDialogue($"You see a portal", choices.ToArray(), new GameLocation.afterQuestionBehavior(DoPortalStuff));
		}


		public static void DoPortalStuff(Farmer who, string dialogueID)
		{
			Monitor.Log($"Answered {dialogueID}", LogLevel.Debug);

			if (dialogueID.Equals(answerYes))
			{
				if (WarpParameters is not null)
				{
					Game1.warpFarmer(WarpParameters.TargetLocation, WarpParameters.Point.X, WarpParameters.Point.Y, Game1.player.FacingDirection);
				}
			}
			else if (dialogueID.Equals(answerNo))
			{

			}
			else if (dialogueID.Equals(answerRemove))
			{
				if (WarpParameters is not null)
				{
					DungeonManager.RemoveWarp(WarpParameters.TargetLocation);
				}
			}
		}
	}
}

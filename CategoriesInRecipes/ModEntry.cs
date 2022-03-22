using System;
using StardewModdingAPI;
using HarmonyLib;

namespace CategoriesInRecipes
{
	public class ModEntry : Mod
	{
		/*********
		** Public methods
		*********/
		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			
			RecipePatches.Initialize(Monitor, helper.Translation);

			var harmony = new Harmony(this.ModManifest.UniqueID);

			harmony.Patch(
				original: AccessTools.Method(typeof(StardewValley.CraftingRecipe), nameof(StardewValley.CraftingRecipe.getNameFromIndex)),
				prefix: new HarmonyMethod(typeof(RecipePatches), nameof(RecipePatches.GetNameFromIndex_Prefix))
			);

			harmony.Patch(
				original: AccessTools.Method(typeof(StardewValley.CraftingRecipe), nameof(StardewValley.CraftingRecipe.getSpriteIndexFromRawIndex)),
				prefix: new HarmonyMethod(typeof(RecipePatches), nameof(RecipePatches.GetSpriteIndexFromRawIndex_Prefix))
			);
		}
    }

	public class RecipePatches
    {
		private static IMonitor Monitor;
		private static ITranslationHelper Translator;

		public static void Initialize(IMonitor monitor, ITranslationHelper translation)
        {
			Monitor = monitor;
			Translator = translation;
        }

		public static bool GetNameFromIndex_Prefix(ref int index, ref string __result)
        {
            try
            {
                switch (index)
                {
					case -75:
						index = -3;
						return true;
					case -79:
						__result = Translator.Get("category.fruit");
						return false;
					case -81:
						index = -1;
						return true;
                    default:
						return true;
                }
            }
            catch (Exception e)
            {
				Monitor.Log("Mod failed at patching CraftingRecipe.getNameFromIndex", LogLevel.Error);
				Monitor.Log(e.ToString(), LogLevel.Error);
				return true;
            }
        }

		public static bool GetSpriteIndexFromRawIndex_Prefix(ref int index, ref int __result)
        {
			try
			{
				switch (index)
				{
					case -75:
						index = -3;
						return true;
					case -79:
						// Use Apple's parent sheet index
						__result = 613;
						return false;
					case -81:
						index = -1;
						return true;
					default:
						return true;
				}
			}
			catch (Exception e)
			{
				Monitor.Log("Mod failed at patching CraftingRecipe.getSpriteIndexFromRawIndex", LogLevel.Error);
				Monitor.Log(e.ToString(), LogLevel.Error);
				return true;
			}
		}
    }
}

using StardewModdingAPI;
using StardewModdingAPI.Events;
using IndustrialFurnace;

namespace IndustrialFurnaceAutomate
{
	class ModEntry : Mod
	{
		//private IIndustrialFurnaceAPI? industrialFurnaceAPI;
		//private IAutomateAPI? automate;


		/*********
		** Public methods
		*********/
		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
		}


		/*********
		** Private methods
		*********/
		private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
		{
			//var automationFactory = Helper.ModRegistry.GetApi<IAutomationFactory>("Pathoschild.Automate");
			var automate = Helper.ModRegistry.GetApi<IAutomateAPI>("Pathoschild.Automate");
			var industrialFurnaceAPI = Helper.ModRegistry.GetApi<IIndustrialFurnaceAPI>("Traktori.IndustrialFurnace");

			if (automate is not null && industrialFurnaceAPI is not null)
			{
				automate.AddFactory(new IndustrialFurnaceAutomationFactory(industrialFurnaceAPI));
			}
			else
			{
				if (automate is null)
				{
					Monitor.Log("Could not detect Automate. Are you sure you have installed everything correctly?", LogLevel.Error);
				}
				if (industrialFurnaceAPI is null)
				{
					Monitor.Log("Could not detect Industrial Furnace. Are you sure you have installed everything correctly?", LogLevel.Error);
				}
			}
		}
	}
}

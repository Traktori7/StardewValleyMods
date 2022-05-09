namespace ShowBirthdays
{
	class ModConfig
	{
		public int cycleDuration = 120;
		public string cycleType = "Always";
		public bool showIcon = true;
		internal static string[] cycleTypes = new string[] { "Always", "Hover", "Click" };
	}
}

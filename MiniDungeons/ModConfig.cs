using System.Collections.Generic;


namespace MiniDungeons
{
	internal class ModConfig
	{
		public bool enableHUDNotification = true;
		public int maxNumberOfDungeonsPerDay = 1;
		public bool enableFightingchallenges = true;
		public Dictionary<string, bool> enabledDungeons = new Dictionary<string, bool>();
		public Dictionary<string, float> dungeonSpawnChances = new Dictionary<string, float>();
	}
}

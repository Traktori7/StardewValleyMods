using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

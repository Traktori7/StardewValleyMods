using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using MiniDungeons.Data;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Monsters;


namespace MiniDungeons
{
	internal class Challenge
	{
		private readonly Data.Challenge challengeData;


		public string Name
		{
			get { return challengeData.ChallengeName; }
		}


		public Challenge(Data.Challenge challengeData)
		{
			this.challengeData = challengeData;
		}
	}
}

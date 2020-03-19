using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IndustrialFurnace
{
	public class RequestSaveData
	{
		public long PlayerID { get; set; }


		public RequestSaveData(long playerID)
		{
			this.PlayerID = playerID;
		}
	}
}

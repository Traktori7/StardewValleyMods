using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace VersatileGrabber
{
	public class ModSaveData
	{
		public List<GrabberSaveData> VersatileGrabbers { get; set; }
	}


	public class GrabberSaveData
	{
		public string LocationName { get; set; }
		public float TileX { get; set; }
		public float TileY { get; set; }

	}
}

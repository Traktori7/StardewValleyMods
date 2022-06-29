using StardewValley;
using SObject = StardewValley.Object;


namespace QualityScrubber
{
	public interface IQualityScrubberApi
	{
		public QualityScrubberController Controller { get; }
		public bool CanProcess(Item inputItem, SObject machine);
	}


	public class QualityScrubberApi : IQualityScrubberApi
	{
		public QualityScrubberController Controller { get; }


		public QualityScrubberApi(QualityScrubberController controller)
		{
			this.Controller = controller;
		}


		public bool CanProcess(Item inputItem, SObject machine)
		{
			return Controller.CanProcess(inputItem, machine);
		}
	}
}

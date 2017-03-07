using FloorPlanBuilder.Enums;


namespace FloorPlanBuilder.Classes
{
	internal class Tug
	{
		#region Constructors

		public Tug(Ray insertRay,
				   InsertDirection insertDirection,
				   Ray overlapRay1,
				   Ray overlapRay2,
				   Link sender,
				   Ray senderOverlapRay1,
				   Ray senderOverlapRay2)
		{
			InsertRay = insertRay;
			InsertDirection = insertDirection;
			OverlapRay1 = overlapRay1;
			OverlapRay2 = overlapRay2;
			Sender = sender;
			SenderOverlapRay1 = senderOverlapRay1;
			SenderOverlapRay2 = senderOverlapRay2;
		}

		#endregion

		#region Properties

		public InsertDirection InsertDirection
		{
			get;
		}

		public Ray InsertRay
		{
			get;
		}

		public Ray OverlapRay1
		{
			get;
		}

		public Ray OverlapRay2
		{
			get;
		}

		public Link Sender
		{
			get;
		}

		public Ray SenderOverlapRay1
		{
			get;
		}

		public Ray SenderOverlapRay2
		{
			get;
		}

		#endregion
	}
}
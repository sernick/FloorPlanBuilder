using System.Collections.Generic;


namespace FloorPlanBuilder.Classes
{
	internal class Outline
	{
		#region Constructors

		public Outline(bool isClosed)
		{
			IsClosed = isClosed;
		}

		#endregion

		#region Properties

		public bool IsClosed
		{
			get;
		}

		public List<Ray> Rays
		{
			get;
		} = new List<Ray>();

		public List<Ray> VertexRays
		{
			get;
		} = new List<Ray>();

		#endregion
	}
}
using System.Collections.ObjectModel;


namespace FloorPlanBuilder.Classes
{
	internal class Link
	{
		#region Constructors

		public Link(double crossingRadian,
					double dihedralRadian,
					ReadOnlyCollection<Ray> rays,
					ReadOnlyCollection<Ray> vertexRays,
					ReadOnlyCollection<Tug> tugs,
					bool isClosed)
		{
			CrossingRadian = crossingRadian;
			DihedralRadian = dihedralRadian;
			Rays = rays;
			VertexRays = vertexRays;
			Tugs = tugs;
			IsClosed = isClosed;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Угол нормали к линии пересечения горизонтальной плоскости и плоскости стола, выраженный в радианах.
		/// </summary>
		public double CrossingRadian
		{
			get;
		}

		/// <summary>
		/// Двугранный угол, выраженный в радианах.
		/// </summary>
		public double DihedralRadian
		{
			get;
		}

		public bool IsClosed
		{
			get;
		}

		public ReadOnlyCollection<Ray> Rays
		{
			get;
		}

		public ReadOnlyCollection<Tug> Tugs
		{
			get;
		}

		public ReadOnlyCollection<Ray> VertexRays
		{
			get;
		}

		#endregion
	}
}
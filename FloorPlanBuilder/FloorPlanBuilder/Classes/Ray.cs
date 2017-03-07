using System.Windows;


namespace FloorPlanBuilder.Classes
{
	/// <summary>
	/// Вектор.
	/// </summary>
	internal class Ray
	{
		#region Constructors

		public Ray(string name, double radian, double length)
		{
			Name = name;
			Radian = radian;
			Length = length;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Длина, мм.
		/// </summary>
		public double Length
		{
			get;
		}

		public string Name
		{
			get;
		}

		/// <summary>
		/// Угол, выраженный в радианах.
		/// </summary>
		public double Radian
		{
			get;
		}

		public Point Point
		{
			get;
			internal set;
		}

		#endregion
	}
}
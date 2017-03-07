using System;

using FloorPlanBuilder.Enums;


namespace FloorPlanBuilder.Classes
{
	[Serializable]
	public class Spot : AdapterBase
	{
		#region Constructors

		public Spot(double measuredDistance, double degree, SpotType type)
		{
			MeasuredDistance = measuredDistance;
			Degree = degree;
			Type = type;
		}

		#endregion

		#region Fields

		private string _name;

		#endregion

		#region Properties

		public double Degree
		{
			get;
		}

		public double MeasuredDistance
		{
			get;
		}

		public SpotType Type
		{
			get;
		}

		public string Name
		{
			get { return _name; }
			internal set
			{
				if (_name == value)
				{
					return;
				}
				_name = value;
				RaisePropertyChanged();
			}
		}

		#endregion
	}
}
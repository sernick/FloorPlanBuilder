using System;

using FloorPlanBuilder.Enums;


namespace FloorPlanBuilder.Classes
{
	[Serializable]
	public class Measure : AdapterBase
	{
		#region Fields

		private double _degree;
		private double _distance;

		[NonSerialized]
		private bool _isSelected;

		private MeasureType _type = MeasureType.BeforeVertex;

		#endregion

		#region Properties

		public double Degree
		{
			get { return _degree; }
			set
			{
				if (Equals(_degree, value))
				{
					return;
				}
				_degree = value;
				RaisePropertyChanged();
			}
		}

		public double Distance
		{
			get { return _distance; }
			set
			{
				if (Equals(_distance, value))
				{
					return;
				}
				_distance = value;
				RaisePropertyChanged();
			}
		}

		internal bool IsNew
		{
			get;
			set;
		} = true;

		public bool IsSelected
		{
			get { return _isSelected; }
			set
			{
				if (_isSelected == value)
				{
					return;
				}
				_isSelected = value;
				RaisePropertyChanged();
			}
		}

		public MeasureType Type
		{
			get { return _type; }
			set
			{
				if (_type == value)
				{
					return;
				}
				_type = value;
				RaisePropertyChanged();
			}
		}

		#endregion
	}
}
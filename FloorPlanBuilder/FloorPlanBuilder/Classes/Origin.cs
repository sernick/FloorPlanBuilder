using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;

using FloorPlanBuilder.Config;
using FloorPlanBuilder.Enums;


namespace FloorPlanBuilder.Classes
{
	[Serializable]
	public class Origin : AdapterBase
	{
		#region Constructors

		public Origin()
		{
			Measures = new ObservableCollection<Measure>();
			Measures.CollectionChanged += Measures_CollectionChanged;
		}

		#endregion

		#region Fields

		private double _angleRulerDihedralDegree;
		private ReadOnlyCollection<Spot> _basicSpots = new ReadOnlyCollection<Spot>(new List<Spot>());
		private double _ceilingHeight;
		private InsertDirection _insertDirection;
		private bool _isClosed;
		private string _name;
		private ReadOnlyCollection<Spot> _notCalculatedSpots = new ReadOnlyCollection<Spot>(new List<Spot>());

		[NonSerialized]
		private ICollectionView _notThisOrigins;

		private Spot _overlapSpot1;
		private Spot _overlapSpot2;
		private double _proDigitDihedralDegree;
		private Origin _receiver;
		private Spot _receiverInsertSpot;
		private Spot _receiverOverlapSpot1;
		private Spot _receiverOverlapSpot2;
		private ReadOnlyCollection<Spot> _spots = new ReadOnlyCollection<Spot>(new List<Spot>());
		private double _zamoDistanceDown;
		private double _zamoDistanceUp;

		#endregion

		#region Properties

		public ObservableCollection<Measure> Measures
		{
			get;
		}

		public double AngleRulerDihedralDegree
		{
			get { return _angleRulerDihedralDegree; }
			set
			{
				if (Equals(_angleRulerDihedralDegree, value))
				{
					return;
				}
				_angleRulerDihedralDegree = value;
				RaisePropertyChanged();
			}
		}

		public ReadOnlyCollection<Spot> BasicSpots
		{
			get { return _basicSpots; }
			private set
			{
				if (ReferenceEquals(_basicSpots, value))
				{
					return;
				}
				_basicSpots = value ?? new ReadOnlyCollection<Spot>(new List<Spot>());
				RaisePropertyChanged();
			}
		}

		public double CeilingHeight
		{
			get { return _ceilingHeight; }
			private set
			{
				if (Equals(_ceilingHeight, value))
				{
					return;
				}
				_ceilingHeight = value;
				RaisePropertyChanged();
			}
		}

		public InsertDirection InsertDirection
		{
			get { return _insertDirection; }
			set
			{
				if (_insertDirection == value)
				{
					return;
				}
				_insertDirection = value;
				RaisePropertyChanged();
			}
		}

		public bool IsClosed
		{
			get { return _isClosed; }
			set
			{
				if (_isClosed == value)
				{
					return;
				}
				_isClosed = value;
				RaisePropertyChanged();

				CalculateSpots();
			}
		}

		public string Name
		{
			get { return _name; }
			set
			{
				if (_name == value)
				{
					return;
				}
				_name = value;
				RaisePropertyChanged();

				CorrectNames(Spots);
			}
		}

		public ReadOnlyCollection<Spot> NotCalculatedSpots
		{
			get { return _notCalculatedSpots; }
			private set
			{
				if (ReferenceEquals(_notCalculatedSpots, value))
				{
					return;
				}
				_notCalculatedSpots = value ?? new ReadOnlyCollection<Spot>(new List<Spot>());
				RaisePropertyChanged();
			}
		}

		public ICollectionView NotThisOrigins
		{
			get { return _notThisOrigins; }
			internal set { _notThisOrigins = value; }
		}

		public Spot OverlapSpot1
		{
			get { return _overlapSpot1; }
			set
			{
				if (ReferenceEquals(_overlapSpot1, value))
				{
					return;
				}
				_overlapSpot1 = value;
				RaisePropertyChanged();
			}
		}

		public Spot OverlapSpot2
		{
			get { return _overlapSpot2; }
			set
			{
				if (ReferenceEquals(_overlapSpot2, value))
				{
					return;
				}
				_overlapSpot2 = value;
				RaisePropertyChanged();
			}
		}

		public double ProDigitDihedralDegree
		{
			get { return _proDigitDihedralDegree; }
			set
			{
				if (Equals(_proDigitDihedralDegree, value))
				{
					return;
				}
				_proDigitDihedralDegree = value;
				RaisePropertyChanged();
			}
		}

		public Origin Receiver
		{
			get { return _receiver; }
			set
			{
				if (ReferenceEquals(_receiver, value))
				{
					return;
				}

				if (_receiver != null)
				{
					_receiver.PropertyChanged -= Receiver_PropertyChanged;
				}
				if (value != null)
				{
					value.PropertyChanged += Receiver_PropertyChanged;
				}
				_receiver = value;

				ReceiverInsertSpot = null;
				ReceiverOverlapSpot1 = null;
				ReceiverOverlapSpot2 = null;

				RaisePropertyChanged();
			}
		}

		public Spot ReceiverInsertSpot
		{
			get { return _receiverInsertSpot; }
			set
			{
				if (ReferenceEquals(_receiverInsertSpot, value))
				{
					return;
				}
				_receiverInsertSpot = value;
				RaisePropertyChanged();
			}
		}

		public Spot ReceiverOverlapSpot1
		{
			get { return _receiverOverlapSpot1; }
			set
			{
				if (ReferenceEquals(_receiverOverlapSpot1, value))
				{
					return;
				}
				_receiverOverlapSpot1 = value;
				RaisePropertyChanged();
			}
		}

		public Spot ReceiverOverlapSpot2
		{
			get { return _receiverOverlapSpot2; }
			set
			{
				if (ReferenceEquals(_receiverOverlapSpot2, value))
				{
					return;
				}
				_receiverOverlapSpot2 = value;
				RaisePropertyChanged();
			}
		}

		public ReadOnlyCollection<Spot> Spots
		{
			get { return _spots; }
			private set
			{
				if (ReferenceEquals(_spots, value))
				{
					return;
				}
				_spots = value ?? new ReadOnlyCollection<Spot>(new List<Spot>());
				RaisePropertyChanged();

				OverlapSpot1 = null;
				OverlapSpot2 = null;

				var basicSpots = new List<Spot>();
				var notCalculatedSpots = new List<Spot>();

				foreach (Spot spot in _spots)
				{
					SpotType type = spot.Type;
					if (type != SpotType.Beacon)
					{
						basicSpots.Add(spot);
					}
					if (type != SpotType.CalculatedVertex)
					{
						notCalculatedSpots.Add(spot);
					}
				}

				BasicSpots = basicSpots.AsReadOnly();
				NotCalculatedSpots = notCalculatedSpots.AsReadOnly();
			}
		}

		public double ZamoDistanceDown
		{
			get { return _zamoDistanceDown; }
			set
			{
				if (Equals(_zamoDistanceDown, value))
				{
					return;
				}
				_zamoDistanceDown = value;
				RaisePropertyChanged();

				CeilingHeight = _zamoDistanceDown + ZamoDistanceUp + 2*Singleton.ZamoOffset;
			}
		}

		public double ZamoDistanceUp
		{
			get { return _zamoDistanceUp; }
			set
			{
				if (Equals(_zamoDistanceUp, value))
				{
					return;
				}
				_zamoDistanceUp = value;
				RaisePropertyChanged();

				CeilingHeight = ZamoDistanceDown + _zamoDistanceUp + 2*Singleton.ZamoOffset;
			}
		}

		#endregion

		#region Methods

		private void CalculateSpots()
		{
			Profile profile = Singleton.Instance.Profile;
			if (profile == null)
			{
				return;
			}

			Spots = new ReadOnlyCollection<Spot>(new List<Spot>());

			if (Measures.Count <= 0)
			{
				return;
			}

			var correctMeasures = new List<Measure>();
			foreach (Measure measure in Measures)
			{
				if (measure != null)
				{
					correctMeasures.Add(measure);
				}
			}

			var spots = new List<Spot>();
			var points = new Dictionary<int, Point>();

			for (int i = 0; i < correctMeasures.Count; i++)
			{
				Measure measure = correctMeasures[i];
				MeasureType type = measure.Type;

				if (type == MeasureType.Vertex || type == MeasureType.BeforeVertex || type == MeasureType.ForCalculation)
				{
					double distance = measure.Distance + Singleton.ZamoOffset;
					double radian = measure.Degree*Math.PI/180;

					double x = Math.Cos(radian)*distance;
					double y = Math.Sin(radian)*distance;

					var point = new Point(x, y);
					points.Add(i, point);
				}
			}

			for (int i = 0; i < correctMeasures.Count; i++)
			{
				Measure measure = correctMeasures[i];
				MeasureType type = measure.Type;

				switch (type)
				{
					case MeasureType.Beacon:
					{
						var spot = new Spot(measure.Distance, measure.Degree, SpotType.Beacon);
						spots.Add(spot);
					}
						break;
					case MeasureType.Vertex:
					{
						var spot = new Spot(measure.Distance, measure.Degree, SpotType.Vertex);
						spots.Add(spot);
					}
						break;
					case MeasureType.BeforeVertex:
						int o, q, r;

						List<int> keys = points.Keys.ToList();
						int p = keys.IndexOf(i);

						if (IsClosed)
						{
							if (keys.Count >= 4)
							{
								o = p - 1;
								if (o < 0)
								{
									o = keys.Count - 1;
								}
								q = (p + 1)%keys.Count;
								r = (p + 2)%keys.Count;
							}
							else
							{
								continue;
							}
						}
						else
						{
							if (p >= 1 && p < keys.Count - 2)
							{
								o = p - 1;
								q = p + 1;
								r = p + 2;
							}
							else
							{
								continue;
							}
						}

						Point p1 = points[keys[o]];
						Point p2 = points[keys[p]];
						Point p3 = points[keys[q]];
						Point p4 = points[keys[r]];

						double a1 = p2.Y - p1.Y;
						double b1 = p1.X - p2.X;

						double a2 = p4.Y - p3.Y;
						double b2 = p3.X - p4.X;

						//http://grafika.me/node/237 - определение точки пересечения двух прямых.

						double d = a1*b2 - a2*b1;
						// Если определитель не равен нулю, то система является определенной и имеет единственное решение.
						if (Math.Abs(d) > double.Epsilon)
						{
							double c1 = p2.X*p1.Y - p1.X*p2.Y;
							double c2 = p4.X*p3.Y - p3.X*p4.Y;

							double dx = -c1*b2 - -c2*b1;
							double dy = a1*-c2 - a2*-c1;

							double x, y;
							try
							{
								x = dx/d;
								y = dy/d;
							}
							catch (Exception)
							{
								continue;
							}

							double distance = Math.Sqrt(x*x + y*y);

							double radian = Math.Atan2(y, x);
							double degree = radian*180/Math.PI;
							if (degree < 0)
							{
								degree += 360;
							}

							double measuredDistance = distance - Singleton.ZamoOffset;

							var vertex = new Spot(measuredDistance, degree, SpotType.CalculatedVertex);
							spots.Add(vertex);
						}
						break;
				}
			}

			CorrectNames(spots);

			Spots = spots.AsReadOnly();
		}

		private void CorrectNames(IList<Spot> spots)
		{
			for (int i = 0; i < spots.Count; i++)
			{
				Spot spot = spots[i];
				string name = $"{Name}-{i + 1}";
				spot.Name = name;
			}
		}

		private void Measure_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			string propertyName = e.PropertyName;

			if (propertyName == nameof(Measure.Degree) ||
				propertyName == nameof(Measure.Distance) ||
				propertyName == nameof(Measure.Type))
			{
				CalculateSpots();
			}
		}

		private void Measures_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			var measures = sender as ObservableCollection<Measure>;
			if (measures == null)
			{
				return;
			}

			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					IList newItems = e.NewItems;
					if (newItems != null)
					{
						foreach (object item in newItems)
						{
							var measure = item as Measure;
							if (measure != null)
							{
								if (measure.IsNew)
								{
									int index = measures.IndexOf(measure);
									if (index == 0)
									{
										if (measures.Count == 1)
										{
											measure.Type = MeasureType.Beacon;
										}
									}
									else if (index > 0)
									{
										Measure previous = measures[index - 1];
										switch (previous.Type)
										{
											case MeasureType.Vertex:
												measure.Type = MeasureType.ForCalculation;
												break;

											case MeasureType.BeforeVertex:
												measure.Type = MeasureType.ForCalculation;
												break;

											case MeasureType.ForCalculation:
												measure.Type = MeasureType.BeforeVertex;
												break;

											case MeasureType.Beacon:
												measure.Type = MeasureType.Beacon;
												break;
										}
									}
									measure.IsNew = false;
								}

								measure.PropertyChanged += Measure_PropertyChanged;
							}
						}
					}
					break;

				case NotifyCollectionChangedAction.Remove:
					IList oldItems = e.OldItems;
					if (oldItems != null)
					{
						foreach (object item in oldItems)
						{
							var measure = item as Measure;
							if (measure != null)
							{
								measure.PropertyChanged -= Measure_PropertyChanged;
							}
						}
					}
					break;

				case NotifyCollectionChangedAction.Replace:
					break;

				case NotifyCollectionChangedAction.Move:
					break;

				case NotifyCollectionChangedAction.Reset:
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}

			CalculateSpots();
		}

		[OnDeserialized]
		internal void OnDeserializedMethod(StreamingContext context)
		{
			ObservableCollection<Measure> measures = Measures;
			if (measures != null)
			{
				measures.CollectionChanged += Measures_CollectionChanged;
				foreach (Measure measure in measures)
				{
					if (measure != null)
					{
						measure.PropertyChanged += Measure_PropertyChanged;
					}
				}
			}

			Origin receiver = Receiver;
			if (receiver != null)
			{
				receiver.PropertyChanged += Receiver_PropertyChanged;
			}
		}

		private void Receiver_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(Spots))
			{
				ReceiverInsertSpot = null;
				ReceiverOverlapSpot1 = null;
				ReceiverOverlapSpot2 = null;
			}
		}

		#endregion
	}
}
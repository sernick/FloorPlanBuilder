using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.Serialization;
using System.Windows.Data;


namespace FloorPlanBuilder.Classes
{
	[Serializable]
	public class Storage : AdapterBase
	{
		#region Constructors

		public Storage()
		{
			Origins = new ObservableCollection<Origin>();
			Origins.CollectionChanged += Origins_CollectionChanged;
		}

		#endregion

		#region Fields

		[NonSerialized]
		private Origin _selectedOrigin;

		#endregion

		#region Properties

		public ObservableCollection<Origin> Origins
		{
			get;
		}

		public Origin SelectedOrigin
		{
			get { return _selectedOrigin; }
			set
			{
				if (ReferenceEquals(_selectedOrigin, value))
				{
					return;
				}
				_selectedOrigin = value;
				RaisePropertyChanged();
			}
		}

		#endregion

		#region Methods

		private static void Origins_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			var origins = sender as ObservableCollection<Origin>;
			if (origins == null)
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
							var newOrigin = item as Origin;
							if (newOrigin != null)
							{
								newOrigin.NotThisOrigins = new ListCollectionView(origins) {Filter = o => o != newOrigin};
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
							var oldOrigin = item as Origin;
							if (oldOrigin != null)
							{
								foreach (Origin origin in origins)
								{
									if (origin != null)
									{
										if (ReferenceEquals(origin.Receiver, oldOrigin))
										{
											origin.Receiver = null;
										}
									}
								}
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
		}

		[OnDeserialized]
		internal void OnDeserializedMethod(StreamingContext context)
		{
			Origins.CollectionChanged += Origins_CollectionChanged;
			foreach (Origin origin in Origins)
			{
				origin.NotThisOrigins = new ListCollectionView(Origins) {Filter = o => o != origin};
			}
		}

		#endregion
	}
}
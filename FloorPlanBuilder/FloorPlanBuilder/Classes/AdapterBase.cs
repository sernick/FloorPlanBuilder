using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace FloorPlanBuilder.Classes
{
	[Serializable]
	public abstract class AdapterBase : INotifyPropertyChanged
	{
		#region Methods

		protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

		[field: NonSerialized]
		public event PropertyChangedEventHandler PropertyChanged;
	}
}
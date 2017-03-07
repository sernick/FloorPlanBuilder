using System.Collections;
using System.Windows;

using GongSolutions.Wpf.DragDrop;

using DragDrop = GongSolutions.Wpf.DragDrop.DragDrop;


namespace FloorPlanBuilder.Classes
{
	internal class MeasureDropTarget : IDropTarget
	{
		#region Fields

		private static MeasureDropTarget _instance;

		#endregion

		#region Properties

		public static MeasureDropTarget Instance => _instance ?? (_instance = new MeasureDropTarget());

		#endregion

		#region Methods

		public void DragOver(IDropInfo dropInfo)
		{
			IEnumerable draggedItems = DefaultDropHandler.ExtractData(dropInfo.Data);
			foreach (object item in draggedItems)
			{
				if (!(item is Measure))
				{
					return;
				}
			}
			dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
			dropInfo.Effects = DragDropEffects.Move;
		}

		public void Drop(IDropInfo dropInfo)
		{
			DragDrop.DefaultDropHandler.Drop(dropInfo);
		}

		#endregion
	}
}
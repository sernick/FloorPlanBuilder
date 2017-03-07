using FloorPlanBuilder.Config;


namespace FloorPlanBuilder.Views
{
	public partial class ProfileWindow
	{
		#region Constructors

		public ProfileWindow(Profile profile)
		{
			InitializeComponent();
			DataContext = profile;
		}

		#endregion
	}
}
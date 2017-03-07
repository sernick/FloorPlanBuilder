using System;
using System.IO;
using System.Reflection;

using FloorPlanBuilder.Config;


namespace FloorPlanBuilder.Classes
{
	internal class Singleton
	{
		#region Constructors

		private Singleton()
		{
			const int start = 'A';
			const int end = 'Z';

			const int lettersCount = end - start + 1;

			Alphabet = new char[lettersCount];
			for (int i = start; i <= end; i++)
			{
				Alphabet[i - start] = (char) i;
			}

			string application = Assembly.GetEntryAssembly().GetName().Name;
			string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
									   $"{Developer}",
									   $"{application}",
									   $"{application}.profile");

			Profile = Profile.LoadOrCreate(path);
		}

		#endregion

		#region Fields

		private static volatile Singleton _instance;
		public const double BeaconCircleRadius = 10;
		public const string BeaconPointsLayerName = "Маячковые точки";
		private const string Developer = "Redskin";
		public static readonly string OutlinesFileExtension = ".out";
		public static readonly string OutlinesFileFilter = $"Контуры (*{OutlinesFileExtension})|*{OutlinesFileExtension}";
		public const string OutlinesLayerName = "Контуры";
		private static readonly object SyncRoot = new object();
		public const string VerticesLayerName = "Вершины";
		public const double ZamoOffset = 115;
		public string NewItemPlaceholder = "{NewItemPlaceholder}";

		#endregion

		#region Properties

		public char[] Alphabet
		{
			get;
		}

		public static Singleton Instance
		{
			get
			{
				if (_instance != null)
				{
					return _instance;
				}
				lock (SyncRoot)
				{
					if (_instance == null)
					{
						_instance = new Singleton();
					}
				}
				return _instance;
			}
		}

		public Profile Profile
		{
			get;
			internal set;
		}

		#endregion
	}
}
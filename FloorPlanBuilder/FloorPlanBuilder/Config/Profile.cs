using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Input;

using FloorPlanBuilder.Classes;
using FloorPlanBuilder.Config.Enums;
using FloorPlanBuilder.Views;

using GalaSoft.MvvmLight.CommandWpf;


namespace FloorPlanBuilder.Config
{
	[Serializable]
	public class Profile : AdapterBase
	{
		#region Constructors

		public Profile(string path)
		{
			FileName = path;
		}

		#endregion

		#region Fields

		private BuildingType _buildingType;

		[NonSerialized]
		private string _fileName;

		#endregion

		#region Properties

		public BuildingType BuildingType
		{
			get { return _buildingType; }
			set
			{
				if (_buildingType == value)
				{
					return;
				}
				_buildingType = value;
				RaisePropertyChanged();
			}
		}

		private string FileName
		{
			get { return _fileName; }
			set { _fileName = value; }
		}

		#endregion

		#region Commands

		[NonSerialized]
		private ICommand _applyCommand;

		[NonSerialized]
		private ICommand _closeCommand;

		[NonSerialized]
		private ICommand _saveCommand;

		public ICommand ApplyCommand =>
			_applyCommand ?? (_applyCommand = new RelayCommand<ProfileWindow>(Apply));

		public ICommand CancelCommand =>
			_closeCommand ?? (_closeCommand = new RelayCommand<ProfileWindow>(Cancel));

		public ICommand SaveCommand =>
			_saveCommand ?? (_saveCommand = new RelayCommand<ProfileWindow>(Save));

		#endregion

		#region Methods

		private static void Cancel(ProfileWindow window)
		{
			window?.Close();
		}

		private static Profile Deserialize(string path)
		{
			var stream = new FileStream(path, FileMode.Open);
			try
			{
				var formatter = new BinaryFormatter();
				var profile = formatter.Deserialize(stream) as Profile;
				if (profile != null)
				{
					profile.FileName = path;
				}
				return profile;
			}
			catch
			{
				return null;
			}
			finally
			{
				stream.Close();
			}
		}

		private static Profile Load(string path)
		{
			try
			{
				return Deserialize(path);
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static Profile LoadOrCreate(string fileName)
		{
			Profile profile = null;
			if (File.Exists(fileName))
			{
				profile = Load(fileName);
			}
			return profile ?? new Profile(fileName);
		}

		private void Apply(ProfileWindow window)
		{
			bool success = Save();
			if (success)
			{
				Singleton.Instance.Profile = this;
			}
		}

		public Profile Clone()
		{
			IFormatter formatter = new BinaryFormatter();
			using (Stream stream = new MemoryStream())
			{
				formatter.Serialize(stream, this);
				stream.Seek(0, SeekOrigin.Begin);
				var profile = (Profile) formatter.Deserialize(stream);
				profile.FileName = FileName;
				return profile;
			}
		}

		public bool Save()
		{
			string path = FileName;

			if (Path.IsPathRooted(path))
			{
				string directoryPath = Path.GetDirectoryName(path);
				if (!string.IsNullOrEmpty(directoryPath))
				{
					if (!Directory.Exists(directoryPath))
					{
						try
						{
							Directory.CreateDirectory(directoryPath);
						}
						catch (Exception)
						{
							return false;
						}
					}

					try
					{
						return Serialize(path);
					}
					catch (Exception)
					{
						return false;
					}
				}
			}
			return false;
		}

		private void Save(ProfileWindow window)
		{
			Apply(window);
			Cancel(window);
		}

		private bool Serialize(string path)
		{
			Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
			try
			{
				var formatter = new BinaryFormatter();
				formatter.Serialize(stream, this);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
			finally
			{
				stream.Close();
			}
		}

		#endregion
	}
}
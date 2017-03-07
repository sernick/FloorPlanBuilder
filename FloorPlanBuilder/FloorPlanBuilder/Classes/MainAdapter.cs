using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using AutoCAD;

using FloorPlanBuilder.Config;
using FloorPlanBuilder.Config.Enums;
using FloorPlanBuilder.Enums;
using FloorPlanBuilder.Views;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;

using Microsoft.Win32;

using netDxf;
using netDxf.Entities;
using netDxf.Tables;

using Point = System.Windows.Point;


namespace FloorPlanBuilder.Classes
{
	internal class MainAdapter : ViewModelBase
	{
		#region Constructors

		public MainAdapter()
		{
			Storage = new Storage();
		}

		#endregion

		#region Fields

		private Storage _storage;

		#endregion

		#region Properties

		public Storage Storage
		{
			get { return _storage; }
			set
			{
				if (ReferenceEquals(_storage, value))
				{
					return;
				}
				_storage = value;
				RaisePropertyChanged();
			}
		}

		#endregion

		#region Commands

		private ICommand _buildCommand;
		private ICommand _loadCommand;
		private ICommand _saveCommand;
		private ICommand _showProfileWindowCommand;
		private ICommand _unselectItemCommand;

		public ICommand BuildCommand =>
			_buildCommand ?? (_buildCommand = new RelayCommand<Window>(window => Build(Storage?.Origins, window)));

		public ICommand LoadCommand =>
			_loadCommand ?? (_loadCommand = new RelayCommand<Window>(Load));

		public ICommand SaveCommand =>
			_saveCommand ?? (_saveCommand = new RelayCommand<Window>(Save));

		public ICommand ShowProfileWindowCommand =>
			_showProfileWindowCommand ?? (_showProfileWindowCommand = new RelayCommand<Window>(ShowProfileWindow));

		public ICommand UnselectItemCommand =>
			_unselectItemCommand ?? (_unselectItemCommand = new RelayCommand<DataGrid>(grid => { grid?.UnselectAll(); }));

		#endregion

		#region Methods

		private static void Build(IEnumerable<Origin> origins, Window owner)
		{
			if (origins == null)
			{
				return;
			}

			Profile profile = Singleton.Instance.Profile;
			if (profile == null)
			{
				return;
			}

			var uniqueOrigins = new HashSet<Origin>();
			foreach (Origin origin in origins)
			{
				if (origin != null)
				{
					uniqueOrigins.Add(origin);
				}
			}

			var usedSpotsDictionary = new Dictionary<Origin, List<Spot>>();

			foreach (Origin origin in uniqueOrigins)
			{
				var usedSpots = new List<Spot>();
				foreach (Spot spot in origin.Spots)
				{
					if (spot != null && !usedSpots.Contains(spot))
					{
						usedSpots.Add(spot);
					}
				}
				usedSpotsDictionary.Add(origin, usedSpots);
			}

			var primaryReceivers = new HashSet<Origin>();
			var sendersDictionary = new Dictionary<Origin, List<Origin>>();

			foreach (Origin origin in uniqueOrigins)
			{
				Origin receiver = origin.Receiver;
				Spot spot1 = origin.OverlapSpot1;
				Spot spot2 = origin.OverlapSpot2;
				Spot receiverSpot1 = origin.ReceiverOverlapSpot1;
				Spot receiverSpot2 = origin.ReceiverOverlapSpot2;

				if (receiver != null &&
					spot1 != null &&
					spot2 != null &&
					spot1 != spot2 &&
					receiverSpot1 != null &&
					receiverSpot2 != null &&
					receiverSpot1 != receiverSpot2 &&
					origin.Spots.Contains(spot1) &&
					origin.Spots.Contains(spot2) &&
					receiver.Spots.Contains(receiverSpot1) &&
					receiver.Spots.Contains(receiverSpot2))
				{
					if (usedSpotsDictionary.ContainsKey(origin))
					{
						List<Spot> usedSpots = usedSpotsDictionary[origin];
						if (!usedSpots.Contains(spot1))
						{
							usedSpots.Add(spot1);
						}
						if (!usedSpots.Contains(spot2))
						{
							usedSpots.Add(spot2);
						}
					}

					if (usedSpotsDictionary.ContainsKey(receiver))
					{
						List<Spot> usedSpots = usedSpotsDictionary[receiver];

						Spot receiverInsertSpot = origin.ReceiverInsertSpot;
						if (receiverInsertSpot != null && receiver.Spots.Contains(receiverInsertSpot))
						{
							if (!usedSpots.Contains(receiverInsertSpot))
							{
								usedSpots.Add(receiverInsertSpot);
							}
						}

						if (!usedSpots.Contains(receiverSpot1))
						{
							usedSpots.Add(receiverSpot1);
						}
						if (!usedSpots.Contains(receiverSpot2))
						{
							usedSpots.Add(receiverSpot2);
						}
					}

					if (sendersDictionary.ContainsKey(receiver))
					{
						sendersDictionary[receiver].Add(origin);
					}
					else
					{
						sendersDictionary.Add(receiver, new List<Origin> {origin});
					}
				}
				else
				{
					primaryReceivers.Add(origin);
				}
			}

			var container = new List<List<Outline>>();

			foreach (Origin origin in primaryReceivers)
			{
				Link link = RecursivelyReadDependences(origin, usedSpotsDictionary, sendersDictionary);
				List<Outline> outlines = RecursivelyGetOutlines(link);
				container.Add(outlines);
			}

			BuildingType buildingType = Singleton.Instance.Profile?.BuildingType ?? BuildingType.NetDxf;
			switch (buildingType)
			{
				case BuildingType.NetDxf:
					CreateDocumentWithNetDxf(container, owner);
					break;

				case BuildingType.Autocad:
					CreateDocumentWithAutocad(container);
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public static void CreateDocumentWithAutocad(IReadOnlyCollection<List<Outline>> container)
		{
			if (container.Count <= 0)
			{
				return;
			}

			AcadApplication app;
			try
			{
				app = Marshal.GetActiveObject("AutoCAD.Application") as AcadApplication;
			}
			catch (Exception)
			{
				return;
			}

			if (app == null)
			{
				return;
			}

			var comObjects = new Stack<object>();
			comObjects.Push(app);

			AcadAcCmColor redColor;
			try
			{
				redColor = app.GetInterfaceObject("AutoCAD.AcCmColor.21") as AcadAcCmColor;
			}
			catch (Exception)
			{
				redColor = null;
			}
			if (redColor != null)
			{
				comObjects.Push(redColor);
				redColor.ColorIndex = AcColor.acRed;
			}

			foreach (List<Outline> outlines in container)
			{
				AcadDocument document = app.Documents.Add();
				if (document != null)
				{
					comObjects.Push(document);

					AcadDatabase database = document.Database;
					AcadModelSpace modelSpace = database.ModelSpace;

					AcadLayer outlinesLayer = null;
					AcadLayer beaconPointsLayer = null;

					AcadLayers layers = database.Layers;
					foreach (AcadLayer layer in layers)
					{
						if (layer != null)
						{
							if (string.Equals(layer.Name, Singleton.OutlinesLayerName, StringComparison.OrdinalIgnoreCase))
							{
								outlinesLayer = layer;
							}
							else if (string.Equals(layer.Name, Singleton.BeaconPointsLayerName, StringComparison.OrdinalIgnoreCase))
							{
								beaconPointsLayer = layer;
							}
						}
					}
					if (outlinesLayer == null)
					{
						outlinesLayer = layers.Add(Singleton.OutlinesLayerName);
					}
					if (beaconPointsLayer == null)
					{
						beaconPointsLayer = layers.Add(Singleton.BeaconPointsLayerName);
					}

					foreach (Outline outline in outlines)
					{
						List<Ray> vertexRays = outline.VertexRays;
						if (vertexRays.Count > 0)
						{
							var vertices = new double[vertexRays.Count*3];
							for (int i = 0; i < vertexRays.Count; i++)
							{
								int a = i*3;
								int b = a + 1;

								Point point = vertexRays[i].Point;

								vertices[a] = point.X;
								vertices[b] = point.Y;
							}

							AcadPolyline polyline = modelSpace.AddPolyline(vertices);

							if (polyline != null)
							{
								comObjects.Push(polyline);

								polyline.Layer = outlinesLayer.Name;
								if (outline.IsClosed)
								{
									polyline.Closed = true;
								}
							}
						}

						foreach (Ray ray in outline.Rays)
						{
							if (!vertexRays.Contains(ray))
							{
								Point point = ray.Point;

								var center = new double[3];
								center[0] = point.X;
								center[1] = point.Y;

								AcadCircle circle = modelSpace.AddCircle(center, Singleton.BeaconCircleRadius);
								if (circle != null)
								{
									circle.Layer = beaconPointsLayer.Name;
									if (redColor != null)
									{
										circle.TrueColor = redColor;
									}
								}
							}
						}
					}
					app.ZoomExtents();
				}
			}

			foreach (object comObject in comObjects)
			{
				try
				{
					Marshal.ReleaseComObject(comObject);
				}
				catch (Exception)
				{
					//
				}
			}
		}

		private static void CreateDocumentWithNetDxf(IReadOnlyCollection<List<Outline>> container, Window owner)
		{
			if (container.Count <= 0)
			{
				return;
			}

			foreach (List<Outline> outlines in container)
			{
				var dxf = new DxfDocument();

				var outlinesLayer = new Layer(Singleton.OutlinesLayerName);
				var verticesLayer = new Layer(Singleton.VerticesLayerName);
				var beaconPointsLayer = new Layer(Singleton.BeaconPointsLayerName);

				var leaderStyle = new DimensionStyle("LeaderStyle")
								  {
									  DimScaleOverall = 100
								  };

				dxf.DrawingVariables.LwDisplay = true;

				foreach (Outline outline in outlines)
				{
					List<Ray> vertexRays = outline.VertexRays;
					if (vertexRays.Count > 0)
					{
						var polyline = new LwPolyline {Layer = outlinesLayer};
						foreach (Ray ray in vertexRays)
						{
							Point point = ray.Point;

							var vertex = new LwPolylineVertex(point.X, point.Y);
							polyline.Vertexes.Add(vertex);
						}
						if (outline.IsClosed)
						{
							polyline.IsClosed = true;
						}

						dxf.AddEntity(polyline);
					}

					foreach (Ray ray in outline.Rays)
					{
						Point point = ray.Point;

						Layer layer;
						AciColor color;
						if (vertexRays.Contains(ray))
						{
							layer = verticesLayer;
							color = AciColor.Green;
						}
						else
						{
							layer = beaconPointsLayer;
							color = AciColor.Red;

							var circle = new Circle(new Vector2(point.X, point.Y), Singleton.BeaconCircleRadius)
										 {
											 Layer = layer,
											 Color = color
										 };
							dxf.AddEntity(circle);
						}

						var leader = new Leader(ray.Name, new[] {new Vector2(point.X, point.Y), new Vector2(point.X + 50, point.Y + 50)})
									 {
										 Style = leaderStyle,
										 Layer = layer,
										 Color = color
									 };
						var text = leader.Annotation as MText;
						if (text != null)
						{
							text.Height = 70;
							text.Layer = layer;
							text.Color = color;
						}
						dxf.AddEntity(leader);
					}
				}

				var dialog = new SaveFileDialog
							 {
								 InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
								 FileName = $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.dxf",
								 Filter = "AutoCAD DXF (*.dxf)|*.dxf"
							 };
				bool? result = dialog.ShowDialog(owner);
				if (result == true)
				{
					string fileName = dialog.FileName;
					if (Path.IsPathRooted(fileName))
					{
						try
						{
							dxf.Save(fileName);
						}
						catch (Exception exception)
						{
							MessageBox.Show(owner, exception.Message, "Ошибка", MessageBoxButton.OK);
						}

						if (File.Exists(fileName))
						{
							try
							{
								Process.Start(fileName);
							}
							catch (Exception exception)
							{
								MessageBox.Show(owner, exception.Message, "Ошибка", MessageBoxButton.OK);
							}
						}
					}
				}
			}
		}

		private static Ray GetRayBySpot(Spot spot, IList<Spot> spots, IReadOnlyList<Ray> rays)
		{
			Ray ray;
			if (spot != null)
			{
				int index = spots.IndexOf(spot);
				ray = index >= 0 && index < rays.Count ? rays[index] : null;
			}
			else
			{
				ray = null;
			}
			return ray;
		}

		private static List<Outline> RecursivelyGetOutlines(Link link)
		{
			var outline = new Outline(link.IsClosed);

			foreach (Ray ray in link.Rays)
			{
				if (ray == null)
				{
					continue;
				}

				#region Проецирование точки на горизонтальную плоскость.

				double crossingRadian = link.CrossingRadian;

				double gamma = ray.Radian - crossingRadian;
				double x = ray.Length*Math.Cos(gamma);
				double y = ray.Length*Math.Sin(gamma);

				double yHatch = y*Math.Cos(link.DihedralRadian);
				double gammaHatch = Math.Atan2(yHatch, x);

				double radian = gammaHatch + crossingRadian;
				double length = Math.Sqrt(x*x + yHatch*yHatch);

				Matrix matrix = Matrix.Identity;
				matrix.Rotate(radian*180/Math.PI);

				var point = new Point(length, 0);
				ray.Point = matrix.Transform(point);

				#endregion

				if (!outline.Rays.Contains(ray))
				{
					outline.Rays.Add(ray);
				}
			}

			foreach (Ray ray in link.VertexRays)
			{
				if (ray != null && !outline.VertexRays.Contains(ray) && outline.Rays.Contains(ray))
				{
					outline.VertexRays.Add(ray);
				}
			}

			var outlines = new List<Outline>();
			if (outline.Rays.Count > 0)
			{
				outlines.Add(outline);
			}

			foreach (Tug tug in link.Tugs)
			{
				if (!outline.Rays.Contains(tug.OverlapRay1) ||
					!outline.Rays.Contains(tug.OverlapRay2))
				{
					continue;
				}

				List<Outline> senderOutlines = RecursivelyGetOutlines(tug.Sender);
				if (senderOutlines.Count <= 0)
				{
					continue;
				}

				Outline senderOutline = senderOutlines.FirstOrDefault(p => p.Rays.Contains(tug.SenderOverlapRay1) ||
																		   p.Rays.Contains(tug.SenderOverlapRay2));
				if (senderOutline == null)
				{
					continue;
				}

				if (tug.InsertDirection == InsertDirection.Before)
				{
					Ray firstVertexRay = senderOutline.VertexRays.FirstOrDefault();
					if (firstVertexRay == null ||
						ReferenceEquals(tug.SenderOverlapRay1, firstVertexRay) &&
						ReferenceEquals(tug.OverlapRay1, tug.InsertRay) ||
						ReferenceEquals(tug.SenderOverlapRay2, firstVertexRay) &&
						ReferenceEquals(tug.OverlapRay2, tug.InsertRay))
					{
						continue;
					}
				}
				else if (tug.InsertDirection == InsertDirection.After)
				{
					Ray lastVertexRay = senderOutline.VertexRays.LastOrDefault();
					if (lastVertexRay == null ||
						ReferenceEquals(tug.SenderOverlapRay1, lastVertexRay) &&
						ReferenceEquals(tug.OverlapRay1, tug.InsertRay) ||
						ReferenceEquals(tug.SenderOverlapRay2, lastVertexRay) &&
						ReferenceEquals(tug.OverlapRay2, tug.InsertRay))
					{
						continue;
					}
				}

				Point overlapPoint1 = tug.OverlapRay1.Point;
				Point overlapPoint2 = tug.OverlapRay2.Point;
				Point senderOverlapPoint1 = tug.SenderOverlapRay1.Point;
				Point senderOverlapPoint2 = tug.SenderOverlapRay2.Point;

				Vector vector = overlapPoint2 - overlapPoint1;
				Vector senderVector = senderOverlapPoint2 - senderOverlapPoint1;

				double radian = Math.Atan2(vector.Y, vector.X);
				double senderRadian = Math.Atan2(senderVector.Y, senderVector.X);

				double degree = (radian - senderRadian)*180/Math.PI;

				Matrix matrix = Matrix.Identity;
				matrix.Translate(-senderOverlapPoint1.X, -senderOverlapPoint1.Y);
				matrix.Rotate(degree);
				matrix.Translate(overlapPoint1.X, overlapPoint1.Y);

				foreach (Outline p in senderOutlines)
				{
					if (p != null)
					{
						foreach (Ray ray in p.Rays)
						{
							if (ray != null)
							{
								ray.Point = matrix.Transform(ray.Point);
							}
						}
					}
				}

				int index, vertexIndex;

				Ray insertRay = tug.InsertRay;
				if (insertRay != null)
				{
					index = outline.Rays.IndexOf(insertRay);
					vertexIndex = outline.VertexRays.IndexOf(insertRay);
				}
				else
				{
					index = -1;
					vertexIndex = -1;
				}

				switch (tug.InsertDirection)
				{
					case InsertDirection.Separately:
						outlines.AddRange(senderOutlines);
						break;

					case InsertDirection.Before:
						if (index >= 0 && vertexIndex >= 0)
						{
							outline.Rays.AddRange(senderOutline.Rays);
							outline.VertexRays.InsertRange(vertexIndex, senderOutline.VertexRays);

							foreach (Outline p in senderOutlines)
							{
								if (!ReferenceEquals(p, senderOutline))
								{
									outlines.Add(p);
								}
							}
						}
						break;

					case InsertDirection.After:
						if (index >= 0 && vertexIndex >= 0)
						{
							outline.Rays.AddRange(senderOutline.Rays);
							if (index == outline.Rays.Count - 1)
							{
								outline.VertexRays.AddRange(senderOutline.Rays);
							}
							else
							{
								int insertIndex = vertexIndex + 1;
								outline.VertexRays.InsertRange(insertIndex, senderOutline.VertexRays);
							}

							foreach (Outline p in senderOutlines)
							{
								if (!ReferenceEquals(p, senderOutline))
								{
									outlines.Add(p);
								}
							}
						}
						break;

					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			return outlines;
		}

		private static Link RecursivelyReadDependences(Origin origin,
													   IReadOnlyDictionary<Origin, List<Spot>> usedSpotsDictionary,
													   IReadOnlyDictionary<Origin, List<Origin>> sendersDictionary)
		{
			if (origin == null || !usedSpotsDictionary.ContainsKey(origin))
			{
				return null;
			}

			List<Spot> usedSpots = usedSpotsDictionary[origin];

			var rays = new List<Ray>();
			var vertexRays = new List<Ray>();

			foreach (Spot spot in usedSpots)
			{
				Ray ray;
				if (spot == null)
				{
					ray = null;
				}
				else
				{
					double radian = spot.Degree*Math.PI/180;
					double length = spot.MeasuredDistance + Singleton.ZamoOffset;

					ray = new Ray(spot.Name, radian, length);

					SpotType type = spot.Type;
					if (type == SpotType.Vertex || type == SpotType.CalculatedVertex)
					{
						vertexRays.Add(ray);
					}
				}
				rays.Add(ray);
			}

			var tugs = new List<Tug>();
			if (sendersDictionary.ContainsKey(origin))
			{
				List<Origin> senders = sendersDictionary[origin];
				foreach (Origin sender in senders)
				{
					if (!usedSpotsDictionary.ContainsKey(sender))
					{
						continue;
					}
					List<Spot> senderUsedSpots = usedSpotsDictionary[sender];

					Link senderLink = RecursivelyReadDependences(sender, usedSpotsDictionary, sendersDictionary);

					Ray insertRay = GetRayBySpot(sender.ReceiverInsertSpot, usedSpots, rays);
					InsertDirection insertDirection = insertRay == null ? InsertDirection.Separately : sender.InsertDirection;

					Ray overlapRay1 = GetRayBySpot(sender.ReceiverOverlapSpot1, usedSpots, rays);
					Ray overlapRay2 = GetRayBySpot(sender.ReceiverOverlapSpot2, usedSpots, rays);

					Ray senderOverlapRay1 = GetRayBySpot(sender.OverlapSpot1, senderUsedSpots, senderLink.Rays);
					Ray senderOverlapRay2 = GetRayBySpot(sender.OverlapSpot2, senderUsedSpots, senderLink.Rays);

					var tug = new Tug(insertRay,
									  insertDirection,
									  overlapRay1,
									  overlapRay2,
									  senderLink,
									  senderOverlapRay1,
									  senderOverlapRay2);
					tugs.Add(tug);
				}
			}

			double crossingAngle = (origin.AngleRulerDihedralDegree + 90)*Math.PI/180;
			double dihedralAngle = origin.ProDigitDihedralDegree*Math.PI/180;

			return new Link(crossingAngle,
							dihedralAngle,
							rays.AsReadOnly(),
							vertexRays.AsReadOnly(),
							tugs.AsReadOnly(),
							origin.IsClosed);
		}

		private static void ShowProfileWindow(Window owner)
		{
			Profile clone = Singleton.Instance.Profile.Clone();
			var profileWindow = new ProfileWindow(clone) {Owner = owner};
			profileWindow.ShowDialog();
		}

		private void Load(Window owner)
		{
			var dialog = new OpenFileDialog {Filter = Singleton.OutlinesFileFilter};
			bool? result = dialog.ShowDialog(owner);
			if (result == true)
			{
				string fileName = dialog.FileName;
				if (Path.IsPathRooted(fileName))
				{
					FileStream stream;
					try
					{
						stream = new FileStream(fileName, FileMode.Open);
					}
					catch (Exception exception)
					{
						MessageBox.Show(owner, exception.Message, "Ошибка", MessageBoxButton.OK);
						stream = null;
					}

					if (stream != null)
					{
						var formatter = new BinaryFormatter();
						object obj = null;
						try
						{
							obj = formatter.Deserialize(stream);
						}
						catch (Exception exception)
						{
							MessageBox.Show(owner, exception.Message, "Ошибка", MessageBoxButton.OK);
						}
						finally
						{
							stream.Close();
						}

						var storage = obj as Storage;
						if (storage != null)
						{
							Storage = storage;
						}
					}
				}
			}
		}

		private void Save(Window owner)
		{
			Storage storage = Storage;
			if (storage == null)
			{
				return;
			}

			var dialog = new SaveFileDialog
						 {
							 InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
							 FileName = $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}{Singleton.OutlinesFileExtension}",
							 Filter = Singleton.OutlinesFileFilter
						 };
			bool? result = dialog.ShowDialog(owner);
			if (result == true)
			{
				string fileName = dialog.FileName;
				if (Path.IsPathRooted(fileName))
				{
					Stream stream;
					try
					{
						stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
					}
					catch (Exception exception)
					{
						MessageBox.Show(owner, exception.Message, "Ошибка", MessageBoxButton.OK);
						stream = null;
					}

					if (stream != null)
					{
						var formatter = new BinaryFormatter();
						try
						{
							formatter.Serialize(stream, storage);
						}
						catch (Exception exception)
						{
							MessageBox.Show(owner, exception.Message, "Ошибка", MessageBoxButton.OK);
						}
						finally
						{
							stream.Close();
						}
					}
				}
			}
		}

		#endregion
	}
}
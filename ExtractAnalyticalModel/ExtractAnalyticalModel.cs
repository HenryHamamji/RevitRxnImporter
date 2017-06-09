namespace RevitReactionImporter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;

    using Autodesk.Revit.DB.Structure;

    /// <summary>
    /// Util class for Analytical model extraction from current project model
    /// </summary>
    public class ExtractAnalyticalModel
    {
        public static AnalyticalModel ExtractFromRevitDocument(Document doc, bool splitOnIntersections = false)
        {
            var project = new AnalyticalModel();

            ProjectLocation projectLocation = doc.ActiveProjectLocation;
            //Get Project Site Location
            SiteLocation projectLocationSiteLocation = projectLocation.SiteLocation;
            //Get Project Site Location Latitude
            double projectLocationLatitude = projectLocationSiteLocation.Latitude;
            //Get Project Site Location Longitude
            double projectLocationLongitude = projectLocationSiteLocation.Longitude;
            //Get Project PlaceName (City)
            string projectSiteLocationPlaceName = projectLocationSiteLocation.PlaceName;

            project.ProjectInfo.Latitude = projectLocationLatitude;
            project.ProjectInfo.Longitude = projectLocationLongitude;
            project.ProjectInfo.City = projectSiteLocationPlaceName;

            Dictionary<int, int> barInstanceIdToAnalyticalStickId = new Dictionary<int, int>();
            Dictionary<int, int> analyticalStickIdToBarInstanceId = new Dictionary<int, int>();

            // Collect all the instanced columns
            var columnInstancedCollector = new FilteredElementCollector(doc);
            columnInstancedCollector.OfCategory(BuiltInCategory.OST_StructuralColumns).ToElements();
            columnInstancedCollector.OfClass(typeof(FamilyInstance)).ToList();
            var columnInstancedElementList = columnInstancedCollector.ToList();

            var tmp = columnInstancedElementList.FirstOrDefault();

            // Convert the list of columns of type "Elements" to a new list of columns of type "FamilyInstances"
            var columnInstancedFamilyInstanceList = new List<FamilyInstance>();
            foreach (var columnElement in columnInstancedElementList)
            {
                var columnFamilyInstance = columnElement as FamilyInstance;
                if (columnFamilyInstance == null)
                    continue;
                columnInstancedFamilyInstanceList.Add(columnFamilyInstance);
                barInstanceIdToAnalyticalStickId.Add(columnFamilyInstance.Id.IntegerValue,
                    (columnFamilyInstance.GetAnalyticalModel() as AnalyticalModelStick).Id.IntegerValue);
                analyticalStickIdToBarInstanceId.Add(
                    barInstanceIdToAnalyticalStickId[columnFamilyInstance.Id.IntegerValue],
                    columnFamilyInstance.Id.IntegerValue);
            }

            // Collect all the instanced beams
            var beamInstancedCollector = new FilteredElementCollector(doc);
            beamInstancedCollector.OfCategory(BuiltInCategory.OST_StructuralFraming).ToElements();
            beamInstancedCollector.OfClass(typeof(FamilyInstance)).ToList();
            List<Element> beamInstancedElementList = beamInstancedCollector.ToList();


            // Convert the list of beams of type "Elements" to a new list of columns of type "FamilyInstances"
            var beamInstancedFamilyInstanceList = new List<FamilyInstance>();
            foreach (var beamElement in beamInstancedElementList)
            {
                var beamFamilyInstance = beamElement as FamilyInstance;
                if (beamFamilyInstance == null)
                    continue;

                var analyticalModelStick = beamFamilyInstance.GetAnalyticalModel() as AnalyticalModelStick;
                if (analyticalModelStick == null)
                    continue;

                beamInstancedFamilyInstanceList.Add(beamFamilyInstance);
                barInstanceIdToAnalyticalStickId.Add(beamFamilyInstance.Id.IntegerValue,
                    analyticalModelStick.Id.IntegerValue);
                analyticalStickIdToBarInstanceId.Add(
                    barInstanceIdToAnalyticalStickId[beamFamilyInstance.Id.IntegerValue],
                    beamFamilyInstance.Id.IntegerValue);
            }

            // Collect all the instanced levels
            var levelInstancedCollector = new FilteredElementCollector(doc);
            ICollection<Element> levelsCollection = levelInstancedCollector.OfClass(typeof(Level)).ToElements();

            // Collect all the instanced grids
            var gridInstancedCollector = new FilteredElementCollector(doc);
            ICollection<Element> gridsCollection = gridInstancedCollector.OfClass(typeof(Grid)).ToElements();

            var referencePoint = EstablishReferencePoint(gridsCollection);
            // Convert the list of beams of type "Elements" to a new list of columns of type "FamilyInstances"
            var levelInstancedList = new List<Level>();
            foreach (var levelElement in levelsCollection)
            {
                var levelInstance = levelElement as Level;
                levelInstancedList.Add(levelInstance);
            }
            levelInstancedList.Sort((a, b) => { return a.Elevation.CompareTo(b.Elevation); });
            foreach (var level in levelInstancedList)
            {
                project.ProjectInfo.FloorInfo[level.Name] = level.Elevation * 12.0;
            }

            // GET BEAM INFO
            var beamInstancedCollectorTransaction = new Transaction(doc, "Get Instanced Beams");
            beamInstancedCollectorTransaction.Start();

            int numberOfBeams = beamInstancedFamilyInstanceList.Count();
            for (int i = 0; i < numberOfBeams; i++)
            {
                try
                {
                    var beamList = ExtractStructuralBeam(beamInstancedFamilyInstanceList[i], doc);
                    foreach (var beam in beamList)
                    {
                        project.StructuralMembers.Beams.Add(beam);
                        //System.Windows.Forms.MessageBox.Show(beam.StartReactionTotal);
                        //System.Windows.Forms.MessageBox.Show(beam.EndReactionTotal);

                    }


                }

                catch (Exception e) { }
            }
            //string testReaction = project.StructuralMembers.Beams[0].StartReactionTotal;
            //System.Windows.Forms.MessageBox.Show(project.StructuralMembers.Beams[0].StartReactionTotal);
            //System.Windows.Forms.MessageBox.Show("RAM Reaction import button was clicked");

            beamInstancedCollectorTransaction.Commit();
            return project;
        }

        public static List<Beam> ExtractStructuralBeam(FamilyInstance beamInstance, Document doc)
        {
            var beams = new List<Beam>();

            var beam = new Beam();

            beam.Symbol = beamInstance.LookupParameter("Type").AsValueString().ToUpper();
            var startConn = beamInstance.LookupParameter("Start Connection") != null ? beamInstance.LookupParameter("Start Connection").AsValueString() : "";
            beam.StartConnectionParameter = startConn;
            var endConn = beamInstance.LookupParameter("End Connection") != null ? beamInstance.LookupParameter("End Connection").AsValueString() : "";
            beam.EndConnectionParameter = endConn;
            beam.StructuralUsage = beamInstance.LookupParameter("Structural Usage") != null ? beamInstance.LookupParameter("Structural Usage").AsValueString() : "";
            beam.StructuralMaterial = beamInstance.LookupParameter("Structural Material") != null ? beamInstance.LookupParameter("Structural Material").AsValueString() : "";
            beam.CutLength = beamInstance.LookupParameter("Cut Length") != null ? beamInstance.LookupParameter("Cut Length").AsDouble() * 12 : 0;
            beam.Id = beamInstance.UniqueId;
            beam.ElementId = beamInstance.Id.IntegerValue;
            beam.ElementLevel = GetElementLevel(doc, beamInstance.Id);
            beam.Size = beamInstance.Name;
            beam.StartReactionTotal = beamInstance.LookupParameter("Start Reaction - Total") != null ? beamInstance.LookupParameter("Start Reaction - Total").AsValueString(): "";
            beam.EndReactionTotal = beamInstance.LookupParameter("End Reaction - Total") != null ? beamInstance.LookupParameter("End Reaction - Total").AsValueString() : "";
            Parameter endReactionTotalParameter = beamInstance.LookupParameter("End Reaction - Total");
            endReactionTotalParameter.SetValueString("45");
            // Beam Material Properties
            var beamInstanceMaterial = beamInstance.get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM);
            var beamInstanceMaterialId = doc.GetElement(beamInstanceMaterial.AsElementId()) as Material;

            // Structural Beam Dimensions
            // The parameters here are parameters specific to the symbol and not the instance, so need to get the symbol first
            var beamSymbol = beamInstance.Symbol;
            beam.FamilySymbol = beamSymbol.Name;

            beam.Depth = beamSymbol.LookupParameter("d") != null ? beamSymbol.LookupParameter("d").AsDouble() * 12 : 0;
            beam.CrossSectionalArea = beamSymbol.LookupParameter("A") != null ? beamSymbol.LookupParameter("A").AsDouble() * 144 : 0;
            beam.WeightPerLF = beamSymbol.LookupParameter("W") != null ? beamSymbol.LookupParameter("W").AsDouble() : 0;
            beam.TopFlangeThickness = beamSymbol.LookupParameter("tf") != null ? beamSymbol.LookupParameter("tf").AsDouble() * 12 : 0;
            beam.BottomFlangeThickness = beamSymbol.LookupParameter("tf") != null ? beamSymbol.LookupParameter("tf").AsDouble() * 12 : 0;
            beam.WebThickness = beamSymbol.LookupParameter("tw") != null ? beamSymbol.LookupParameter("tw").AsDouble() * 12 : 0;
            beam.Width = beamSymbol.LookupParameter("bf") != null ? beamSymbol.LookupParameter("bf").AsDouble() * 12 : 0;

            var start = new double[3];
            var locCurve = (LocationCurve)beamInstance.Location;
            start[0] = locCurve.Curve.GetEndPoint(0).X * 12;
            start[1] = locCurve.Curve.GetEndPoint(0).Y * 12;
            start[2] = locCurve.Curve.GetEndPoint(0).Z * 12;

            var end = new double[3];
            end[0] = locCurve.Curve.GetEndPoint(1).X * 12;
            end[1] = locCurve.Curve.GetEndPoint(1).Y * 12;
            end[2] = locCurve.Curve.GetEndPoint(1).Z * 12;

            beam.StartPoint = start;
            beam.EndPoint = end;

            beams.Add(beam);

            return beams;
        }

        // Defines reference point for model geometry mapping from RAM to Revit. Hard-coded as Grid A-1.
        public static double[] EstablishReferencePoint(ICollection<Element> gridsCollection)
        {
            var referencePoint = new double[3];
            var gridOrigin = new double[3];
            var gridDirection = new double[3];
            foreach (var grid in gridsCollection)
            {
                var gridInstance = grid as Autodesk.Revit.DB.Grid;
                if(gridInstance.Name == "A")
                {
                    var gridCurve = gridInstance.Curve as Line;
                    gridOrigin[0] = gridCurve.Origin.X;
                    gridOrigin[1] = gridCurve.Origin.Y;
                    gridOrigin[2] = gridCurve.Origin.Z;
                    gridDirection[0] = gridCurve.Direction.X;
                    gridDirection[1] = gridCurve.Direction.Y;
                    gridDirection[2] = gridCurve.Direction.Z;
                }
                else if(gridInstance.Name == "1")
                {

                }
            }

            return referencePoint;
        }

        public static string GetElementId(Document doc, ElementId id)
        {
            return doc.GetElement(id).Id.ToString();
        }

        public static string GetElementLevel(Document doc, ElementId id)
        {
            string level = null;
            var elem = doc.GetElement(id) as FamilyInstance;

            if (elem.Category.Name == "Structural Columns")
            {
                level = doc.GetElement(elem.LookupParameter("Base Level").AsElementId()).Name;
            }
            else if (elem.Category.Name == "Structural Framing")
            {
                level = doc.GetElement(elem.LookupParameter("Reference Level").AsElementId()).Name;
            }

            return level;
        }

        private static double EPSILON = 0.01;

        private static double VectorDotProduct(double[] a, double[] b)
        {
            return a[0] * b[0] + a[1] * b[1];
        }

        private static double VectorLength(double[] vec)
        {
            return Math.Sqrt(vec[0] * vec[0] + vec[1] * vec[1]);
        }

        private static bool VectorsPerpendicular(double[] a, double[] b)
        {
            var alen = VectorLength(a);
            var blen = VectorLength(b);

            a[0] /= alen;
            a[1] /= alen;

            b[0] /= blen;
            b[1] /= blen;

            return Math.Abs(VectorDotProduct(a, b)) < EPSILON;
        }

        private static double GetDistance(double[] p1, double[] p2)
        {
            return Math.Sqrt(Math.Pow(p2[0] - p1[0], 2) + Math.Pow(p2[1] - p1[1], 2) + Math.Pow(p2[2] - p1[2], 2));
        }

        private static bool PointIsInsideColumnHorizontal(double[] p, FamilyInstance column)
        {
            var start = new double[3];
            var locPoint = (LocationPoint)column.Location;
            start[0] = locPoint.Point.X * 12;
            start[1] = locPoint.Point.Y * 12;

            var baseLevelGlobal = column.Document.GetElement(column.LookupParameter("Base Level").AsElementId()) as Level;
            start[2] = (locPoint.Point.Z + baseLevelGlobal.Elevation) * 12.0;

            // a bit of a heck. To be improved at a later date
            var columnSymbol = column.Symbol;

            // http://thebuildingcoder.typepad.com/blog/2009/06/revit-library-shape-type-catalogue-parameters.html
            var depth = columnSymbol.LookupParameter("d").AsDouble() * 12;
            var width = columnSymbol.LookupParameter("bf").AsDouble() * 12;

            var maxDim = Math.Max(width, depth);

            var cp = start;

            return (p[0] < cp[0] + maxDim) && (p[0] > cp[0] - maxDim) &&
                   (p[1] < cp[1] + maxDim) && (p[1] > cp[1] - maxDim);
        }

        private static bool PointIsInsideColumnVertical(double[] p, FamilyInstance column, Document doc)
        {
            var start = new double[3];
            var locPoint = (LocationPoint)column.Location;
            start[0] = locPoint.Point.X * 12;
            start[1] = locPoint.Point.Y * 12;
            var baseLevelGlobal = column.Document.GetElement(column.LookupParameter("Base Level").AsElementId()) as Level;
            start[2] = (locPoint.Point.Z + baseLevelGlobal.Elevation) * 12.0;

            var end = new double[3];
            end[0] = start[0];
            end[1] = start[1];
            var topLevel = doc.GetElement(column.LookupParameter("Top Level").AsElementId()) as Level;
            var baseLevel = doc.GetElement(column.LookupParameter("Base Level").AsElementId()) as Level;
            var length = (topLevel.Elevation - baseLevel.Elevation) * 12;
            end[2] = start[2] + length;

            var bp = start;
            var tp = end;

            return (p[2] < tp[2] + EPSILON) && (p[2] > bp[2] - EPSILON);
        }

        private static bool PointIsInsideColumn(double[] p, FamilyInstance column, Document doc)
        {
            return PointIsInsideColumnHorizontal(p, column) && PointIsInsideColumnVertical(p, column, doc);
        }

        /// <summary>
        /// Distance calculaions tolerance
        /// </summary>
        private static double tol = 0.0000001;

        /// <summary>
        /// Gets the nearest level by elevation
        /// </summary>
        /// <param name="pointZ">Elevation</param>
        /// <param name="levels">Sorted list of levels</param>
        /// <returns>Nearest level</returns>
        /// <remarks>Levels list should be sorted for the better performance</remarks>>
        private static Level GetNearestLevel(double pointZ, List<Level> levels)
        {
            int levelsCount = levels.Count;
            if (levelsCount == 1 || pointZ < levels[0].Elevation * 12 || Math.Abs(pointZ - levels[0].Elevation * 12) < tol)
            {
                return levels[0];
            }
            for (int i = 0; i < levelsCount - 1; ++i)
            {
                double levelElevation = levels[i].Elevation * 12;
                double nextLevelElevation = levels[i + 1].Elevation * 12;
                double distanceToCurLevel = Math.Abs(pointZ - levelElevation);
                double distanceToNextLevel = Math.Abs(pointZ - nextLevelElevation);
                if (pointZ > levelElevation && pointZ < nextLevelElevation)
                {
                    return distanceToCurLevel < distanceToNextLevel ? levels[i] : levels[i + 1];
                }
                if (distanceToNextLevel < tol)
                {
                    return levels[i + 1];
                }
            }
            return levels[levelsCount - 1];
        }

    }
}

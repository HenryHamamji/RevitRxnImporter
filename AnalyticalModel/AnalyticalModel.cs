using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;

namespace RevitReactionImporter
{
    public class AnalyticalModel
    {
        public ProjectInformation ProjectInfo { get; set; }
        public Members StructuralMembers { get; set; }
        public GridData GridData { get; set; }
        public LevelInfo LevelInfo { get; set; }
        public double[] ReferencePointDataTransfer { get; set; }
        public AnalyticalModel()
        {
            ProjectInfo = new ProjectInformation();
            StructuralMembers = new Members();
            GridData = new GridData();
            LevelInfo = new LevelInfo();
        }


        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            if (ProjectInfo == null)
            {
                ProjectInfo = new ProjectInformation();
            }
            if (StructuralMembers == null)
            {
                StructuralMembers = new Members();
            }
            if (StructuralMembers.Beams == null)
            {
                StructuralMembers.Beams = new List<Beam>();
            }
            if (StructuralMembers.Columns == null)
            {
                StructuralMembers.Columns = new List<Column>();
            }
        }
    }

    public class ProjectInformation
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string City { get; set; }
        public string Units { get; set; }
        public ProjectInformation()
        {
        }
    }

    public class Members
    {
        public List<Column> Columns { get; set; }
        public List<Beam> Beams { get; set; }

        public Members()
        {
            Columns = new List<Column>();
            Beams = new List<Beam>();
        }
    }

    public class Grid
    {
        public string Name { get; set; }
        public Vector Direction { get; set; }
        public double[] Origin { get; set; }
        public GridOrientationClassification GridOrientation {get; set;}
        public GridDirectionalityClassification DirectionalityClassification { get; set; }
        public GridTypeNamingClassification GridTypeNaming { get; set; }

        public Grid()
        {
            Origin = new double[3];
            GridOrientation = GridOrientationClassification.None;
            Direction = new Vector(0,0,0);
            DirectionalityClassification = GridDirectionalityClassification.None;
            GridTypeNaming = GridTypeNamingClassification.None;

        }

        public enum GridTypeNamingClassification
        {
            None,
            Lettered,
            Numbered
        }


        public enum GridDirectionalityClassification
        {
            None,
            Increasing,
            Decreasing
        }

    }

    public enum GridOrientationClassification
    {
        Vertical,
        Horizontal,
        Other,
        None
    }

    public class GridData
    {
        public List<Grid> Grids { get; set; }
        public int VerticalGridCount { get; set; }
        public int HorizontalGridCount { get; set; }
        public int OtherGridCount { get; set; }
        public int TotalGridCount { get; set; }
        public Dictionary<string, double> VerticalGridSpacings { get; set; }
        public Dictionary<string, double> HorizontalGridSpacings { get; set; }


        public GridData()
        {
            Grids = new List<Grid>();
            VerticalGridSpacings = new Dictionary<string, double>();
            HorizontalGridSpacings = new Dictionary<string, double>();
        }


    }



    public class LevelInfo
    {
        public int LevelCount { get; set; }
        public List<LevelFloor> Levels { get; set; }
        public double BaseReferenceElevation { get; set; }
        public Dictionary<string, double> LevelsRevitSpacings { get; set; }

        public LevelInfo()
        {
            Levels = new List<LevelFloor>();
            LevelCount = 0;
            LevelsRevitSpacings = new Dictionary<string, double>();
        }
    }

    public class LevelFloor
    {
        public int ElementId { get; set; }
        public int LevelNumber { get; set; }
        public string Name { get; set; }
        public string MappedRAMLayoutType { get; set; }
        public double Elevation { get; set; }
        public bool MapRAMLayoutTypeToThis { get; set; }
        public int MappingConfidence { get; set; } // Highest = 1.
        public LevelFloor()
        {
            MappedRAMLayoutType = "None";
            MappingConfidence = 0; // Un-mapped.
            MapRAMLayoutTypeToThis = false; // Do not map.

        }
    }

    public class RevitStory
    {
        public double Height { get; set; }

        public RevitStory()
        {
        }
    }

    public enum MemberType
    {
        Beam,
        Column
    }

    public class WideFlange
    {
        public string StructuralMaterial { get; set; }
        public string Id { get; set; }
        public int ElementId { get; set; }
        public string ElementLevel { get; set; }
        public int ElementLevelId { get; set; }
        public string Symbol { get; set; }
        public double Width { get; set; }
        public double Depth { get; set; }
        public double WebThickness { get; set; }
        public string Type { get; set; }
        public string Size { get; set; }
        public double WeightPerLF { get; set; }
        public double CrossSectionalArea { get; set; }
        public double TopFlangeThickness { get; set; }
        public double BottomFlangeThickness { get; set; }

        // The Length property represents the overall length of the member (including the 
        // length of cut backs for beams).
        public double Length { get; set; }

        public string Section { get { return Symbol; } }
        public string FamilySymbol { get; set; }

        public WideFlange()
        {
        }


    }

    public class Column : WideFlange
    {
        //public double Length { get; set; }
        //public string BaseConnection { get; set; }
        //public string TopConnection { get; set; }
        public string BaseConnectionParameter { get; set; }
        public string TopConnectionParameter { get; set; }
        public double[] BasePoint { get; set; }
        public double[] TopPoint { get; set; }
        public double[] FacingOrientation { get; set; }
        public double Rotation { get; set; }
        public string TopLevelName { get; set; }
        public string BaseLevelName { get; set; }

        public Column()
        {
        }

    }

    public class Beam : WideFlange
    {
        public double EndLevelOffset { get; set; }
        public double StartLevelOffset { get; set; }
        public string StartConnectionParameter { get; set; }
        public string EndConnectionParameter { get; set; }
        public string StructuralUsage { get; set; }
        public double[] StartPoint { get; set; }
        public double[] EndPoint { get; set; }
        public string StartReactionTotal { get; set; }
        public string EndReactionTotal { get; set; }
        public string RAMFloorLayoutType { get; set; }
        public bool IsMappedToRAMBeam { get; set; }
        public double ToleranceForSuccessFulMapping { get; set; }
        public BeamOrientationRelativeToGrid OrientationRelativeToGrid { get; set; }

        // The CutLength property is the reduced length of beams after subtracting the 
        // cut backs.
        public double CutLength { get; set; }
        public double[] TopFlangeNormal { get { return new double[] { 0, 0, 1.0 }; } }
        public double ZOffsetValue { get; set; }


        public Beam()
        {
            IsMappedToRAMBeam = false;
        }

        public enum BeamOrientationRelativeToGrid
        {
            None,
            ParallelToHorizontalGrids,
            ParallelToVerticalGrids,
            Other
        }
    }


    public class Vector
    {
        public Vector(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public void Normalize()
        {
            var distance = Math.Sqrt(X * X + Y * Y + Z * Z);
            X /= distance;
            Y /= distance;
            Z /= distance;
        }

        public static Vector Cross(Vector a, Vector b)
        {
            var x = a.Y * b.Z - a.Z * b.Y;
            var y = a.Z * b.X - a.X * b.Z;
            var z = a.X * b.Y - a.Y * b.X;

            return new Vector(x, y, z);
        }

        public static Vector FromTo(double[] a, double[] b)
        {
            var x = b[0] - a[0];
            var y = b[1] - a[1];
            var z = b[2] - a[2];

            return new Vector(x, y, z);
        }
    }

    public class CoordinateSystem
    {
        public CoordinateSystem(double[] o, Vector x, Vector y, Vector z)
        {
            Origin = o;
            XAxis = x;
            YAxis = y;
            ZAxis = z;
        }

        public Vector XAxis { get; set; }
        public Vector YAxis { get; set; }
        public Vector ZAxis { get; set; }
        public double[] Origin { get; set; }
    }
}

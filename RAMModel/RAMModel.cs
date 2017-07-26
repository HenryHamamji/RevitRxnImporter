using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace RevitReactionImporter
{
    public class RAMModel
    {
        public List<RAMBeam> RamBeams {get; set;}
        public double[] OriginRAM { get; set; }
        public int StoryCount { get; set; }
        public List<Story> Stories { get; set; }
        public List<RAMGrid> Grids { get; set; }
        public List<RAMGrid> LetteredGrids { get; set; }
        public List<RAMGrid> NumberedGrids { get; set; }
        public List<RAMGrid> HorizontalGrids { get; set; }
        public List<RAMGrid> VerticalGrids { get; set; }
        public List<RAMGrid> OtherGrids { get; set; }
        public double[] ReferencePointDataTransfer { get; set; }

        public RAMModel()
        {
            RamBeams = new List<RAMBeam>();
            OriginRAM = new double[3];
            Stories = new List<Story>();
            Grids = new List<RAMGrid>();
            HorizontalGrids = new List<RAMGrid>();
            VerticalGrids = new List<RAMGrid>();
            LetteredGrids = new List<RAMGrid>();
            NumberedGrids = new List<RAMGrid>();
            OtherGrids = new List<RAMGrid>();
        }

        public enum GridDirectionalityClassification
        {
            None,
            Increasing,
            Decreasing
        }

        public class Story
        {
            public int Level { get; set; }
            public string StoryLabel { get; set; }
            public string LayoutType { get; set; }
            public double Height { get; set; }
            public double Elevation { get; set; }
            public bool MapRevitLevelToThis { get; set; }

            public Story(int level, string storyLabel, string layoutType, double height, double elevation)
            {
                Level = level;
                StoryLabel = storyLabel;
                LayoutType = layoutType;
                Height = height;
                Elevation = elevation;
                MapRevitLevelToThis = true;
            }
        }

        public class RAMGrid
        {
            public string Name { get; set; }
            public double Location { get; set; }
            public GridTypeNamingClassification GridTypeNaming { get; set; }
            public GridDirectionalityClassification DirectionalityClassification {get; set;}
            public GridOrientationClassification GridOrientation { get; set; }

            public RAMGrid (string name, double location)
            {
                Name = name;
                Location = location;
                GridTypeNaming = GridTypeNamingClassification.None;
                DirectionalityClassification = GridDirectionalityClassification.None;
                GridOrientation = GridOrientationClassification.None;


            }

            public enum GridOrientationClassification
            {
                None,
                Vertical,
                Horizontal,
                Other
            }

            public enum GridTypeNamingClassification
            {
                Lettered,
                Numbered,
                None
            }

        }

        public class RAMBeam
        {
            public string FloorLayoutType { get; set; }
            public string Size { get; set; }
            public bool IsCantilevered { get; set; }
            public double [] StartPoint { get; set; }
            public double[] EndPoint { get; set; }
            public double StartTotalReactionPositive { get; set; }
            public double EndTotalReactionPositive { get; set; }
            public int Id { get; set; }
            public bool IsMappedToRevitBeam { get; set; }

            public RAMBeam(string floorLayoutType, string size, double startPointX, double startPointY, double endPointX, double endPointY, double startReactionTotalPositive, double endReactionTotalPositive )
            {
                StartPoint = new double[3];
                EndPoint = new double[3];
                FloorLayoutType = floorLayoutType;
                Size = size;
                StartPoint[0] = startPointX;
                StartPoint[1] = startPointY;
                EndPoint[0] = endPointX;
                EndPoint[1] = endPointY;
                StartTotalReactionPositive = startReactionTotalPositive;
                EndTotalReactionPositive = endReactionTotalPositive;
                IsCantilevered = false;
                IsMappedToRevitBeam = false;
            }
        }

        // Defines RAM reference point for model geometry mapping from RAM to Revit. Hard-coded as Grid A-1.
        public static double[] EstablishReferencePoint(List<RAMGrid> grids, List<RAMGrid> letteredGrids, List<RAMGrid> numberedGrids)
        {
            var referencePoint = new double[3];
            int letteredGridIndex = -1;
            int numberedGridIndex = -1;
            //var letteredGrid = grids.First(item => item.Name == "A");
            //var numberedGrid = grids.First(item => item.Name == "1");

            var letteredGrid = letteredGrids[0];
            var numberedGrid = numberedGrids[0];

            if (letteredGrid.GridOrientation == RAMGrid.GridOrientationClassification.Horizontal)
            {
                letteredGridIndex = 1;
            }
            else if (letteredGrid.GridOrientation == RAMGrid.GridOrientationClassification.Vertical)
            {
                letteredGridIndex = 0;
            }
            else
            {
                throw new Exception("TODO: Other Lettered Grid Classification");
            }

            if (numberedGrid.GridOrientation == RAMGrid.GridOrientationClassification.Horizontal)
            {
                numberedGridIndex = 1;
            }
            else if (numberedGrid.GridOrientation == RAMGrid.GridOrientationClassification.Vertical)
            {
                numberedGridIndex = 0;
            }
            else
            {
                throw new Exception("TODO: Other Numbered Grid Classification");
            }
            if(numberedGridIndex == letteredGridIndex)
            {
                throw new Exception("Numbered & Lettered Grids are parallel");
            }

            referencePoint[letteredGridIndex] = letteredGrid.Location;
            referencePoint[numberedGridIndex] = numberedGrid.Location;

            //referencePoint[0] = grids.First(item => item.Name == "A").Location;
            //referencePoint[1] = grids.First(item => item.Name == "1").Location;
            referencePoint[2] = 0.0;
            return referencePoint;
        }

        public static void PopulateAdditionalRAMModelInfo(RAMModel ramModel)
        {
            ClassifyGridDirectionalities(ramModel.Grids, ramModel.NumberedGrids, ramModel.LetteredGrids);
            ramModel.ReferencePointDataTransfer = EstablishReferencePoint(ramModel.Grids, ramModel.LetteredGrids, ramModel.NumberedGrids);
        }

        public static void ExecutePythonScript()
        {
            Process process = new Process();

            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                WorkingDirectory = @"C:\dev\RAM Reaction Importer\RAM-Reaction-Importer",
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                RedirectStandardInput = true,
                UseShellExecute = false
            };
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            process.StartInfo = startInfo;
            process.Start();
            process.StandardInput.WriteLine(@"C:\dev\RAM Reaction Importer\RAM-Reaction-Importer");
            process.StandardInput.WriteLine("python getLevels.py");

            // Read the standard output of the app we called.  
            // in order to avoid deadlock we will read output first 
            // and then wait for process terminate: 
            StreamReader myStreamReader = process.StandardOutput;
            string myString = myStreamReader.ReadLine();

            process.Close();

            // write the output we got from python app 
            Console.WriteLine("Value received from script: " + myString);            
        }


        public static RAMModel DeserializeRAMModel()
        {
            var ramModel = new RAMModel();

            // TODO: Beam Data in its own function
            string path = @"C:\dev\RAM Reaction Importer\RAM-Reaction-Importer\beamData.txt";
            string beamDataString = "";
            Char lineDelimiter = ';';
            Char propertyDelimiter = ',';

            using (StreamReader sr = new StreamReader(path))
            {
                // Read the stream to a string.
                beamDataString = sr.ReadToEnd();
            }
            String[] allBeamData = beamDataString.Split(lineDelimiter);
            allBeamData = allBeamData.Take(allBeamData.Length - 1).ToArray();
            var id = 1;
            foreach (var singleBeamData in allBeamData)
            {
                bool isCantilevered = false;
                string[] beamProperties = singleBeamData.Split(propertyDelimiter);

                if(beamProperties[4] == "NA")
                {
                    beamProperties[4] = "0";
                }
                if (beamProperties[5] == "NA")
                {
                    beamProperties[5] = "0";
                }
                if (beamProperties[7] == "NA")
                {
                    beamProperties[7] = "0";
                    isCantilevered = true;
                }
                RAMBeam ramBeam = new RAMBeam(beamProperties[0], beamProperties[1], Convert.ToDouble(beamProperties[2])*12.0, Convert.ToDouble(beamProperties[3])*12.0,
                    Convert.ToDouble(beamProperties[4])*12.0, Convert.ToDouble(beamProperties[5])*12.0, Convert.ToDouble(beamProperties[6]), Convert.ToDouble(beamProperties[7]));
                ramBeam.IsCantilevered = isCantilevered;
                ramBeam.Id = id;
                id += 1;
                ramModel.RamBeams.Add(ramBeam);
                }
            DeserializeRAMStoryData(ramModel);
            DeserializeRAMGridData(ramModel);
            PopulateAdditionalRAMModelInfo(ramModel);
            return ramModel;
        }

        public static void DeserializeRAMStoryData(RAMModel ramModel)
        {
            string path = @"C:\dev\RAM Reaction Importer\RAM-Reaction-Importer\RAMStoryData.txt";
            string storyDataString = "";
            Char lineDelimiter = ';';
            Char propertyDelimiter = ',';
            using (StreamReader sr = new StreamReader(path))
            {
                // Read the stream to a string.
                storyDataString = sr.ReadToEnd();
            }
            String[] allStoryData = storyDataString.Split(lineDelimiter);
            foreach (var singleStoryData in allStoryData)
            {
                string[] storyProperties = singleStoryData.Split(propertyDelimiter);
                Story ramStory = new Story(Convert.ToInt32(storyProperties[0]), storyProperties[1], storyProperties[2], Convert.ToDouble(storyProperties[3]), Convert.ToDouble(storyProperties[4]));
                ramModel.Stories.Add(ramStory);
            }
        }

        public static void DeserializeRAMGridData(RAMModel ramModel)
        {
            string pathX = @"C:\dev\RAM Reaction Importer\RAM-Reaction-Importer\xGridData.txt";
            string pathY = @"C:\dev\RAM Reaction Importer\RAM-Reaction-Importer\yGridData.txt";

            string xGridDataString = "";
            string yGridDataString = "";
            Char lineDelimiter = ';';
            Char propertyDelimiter = ',';
            using (StreamReader sr = new StreamReader(pathX))
            {
                // Read the stream to a string.
                xGridDataString = sr.ReadToEnd();
            }
            String[] allXGridData = xGridDataString.Split(lineDelimiter);
            foreach (var singleXGridData in allXGridData)
            {
                string[] xGridProperties = singleXGridData.Split(propertyDelimiter);
                RAMGrid ramXGrid = new RAMGrid(xGridProperties[0], Convert.ToDouble(xGridProperties[1])*12.0); // Convert feet to inches.
                ClassifyGridNameType(ramXGrid);
                ramXGrid.GridOrientation = RAMGrid.GridOrientationClassification.Vertical;
                ramModel.VerticalGrids.Add(ramXGrid);
                ramModel.Grids.Add(ramXGrid);
            }

            using (StreamReader sr = new StreamReader(pathY))
            {
                // Read the stream to a string.
                yGridDataString = sr.ReadToEnd();
            }
            String[] allYGridData = yGridDataString.Split(lineDelimiter);
            foreach (var singleYGridData in allYGridData)
            {
                string[] yGridProperties = singleYGridData.Split(propertyDelimiter);
                RAMGrid ramYGrid = new RAMGrid(yGridProperties[0], Convert.ToDouble(yGridProperties[1])*12.0); // Convert feet to inches.
                ClassifyGridNameType(ramYGrid);
                ramYGrid.GridOrientation = RAMGrid.GridOrientationClassification.Horizontal;
                ramModel.HorizontalGrids.Add(ramYGrid);
                ramModel.Grids.Add(ramYGrid);
            }
        }

        public static void ClassifyGridDirectionalities(List<RAMGrid> grids, List<RAMGrid> numberedGrids, List<RAMGrid> letteredGrids)
        {
            //var letteredGrids = new List<RAMGrid>();
           //var numberedGrids = new List<RAMGrid>();

            foreach (var grid in grids)
            {
                if(grid.GridTypeNaming == RAMGrid.GridTypeNamingClassification.Lettered)
                {
                    letteredGrids.Add(grid);
                }
                else if (grid.GridTypeNaming == RAMGrid.GridTypeNamingClassification.Numbered)
                {
                    numberedGrids.Add(grid);
                }
                else
                {
                    throw new Exception("Grid does not have proper GridTypeClassification");
                }
            }

            SortGridsByAlphabeticalOrder(letteredGrids);
            SortGridsByNuermicalOrder(numberedGrids);
            ClassifyGridDirectionality(letteredGrids);
            ClassifyGridDirectionality(numberedGrids);

        }

        public static void SortGridsByAlphabeticalOrder(List<RAMGrid> letteredGrids)
        {
            letteredGrids.OrderBy(x => x.Name);
        }

        public static void SortGridsByNuermicalOrder(List<RAMGrid> numberedGrids)
        {
            numberedGrids.OrderBy(x => Convert.ToInt32(x.Name));
        }

        public static void ClassifyGridDirectionality(List<RAMGrid> grids)
        {
            if (grids.Count > 1)
            {
                var locationA = grids[0].Location;
                var locationB = grids[1].Location;
                if (locationA < locationB)
                {
                    foreach (var grid in grids)
                    {
                        grid.DirectionalityClassification = GridDirectionalityClassification.Increasing;
                    }
                }
                else if (locationA > locationB)
                {
                    foreach (var grid in grids)
                    {
                        grid.DirectionalityClassification = GridDirectionalityClassification.Decreasing;
                    }
                }
                else
                {
                    throw new Exception("Comparing overlapping Grids error");
                }
            }
            else
            {
                throw new Exception("Only 1 RAM grid in that direction found error");
            }
        }

        public static void ClassifyGridNameType(RAMGrid ramGrid)
        {
            Char gridNameDelimiter = '.';
            string gridName = ramGrid.Name;
            bool gridNameContainsPeriod = gridName.Contains(gridNameDelimiter);
            if(gridNameContainsPeriod)
            {
                string[] gridNameComponents = gridName.Split(gridNameDelimiter);
                int n;
                bool isNumeric = int.TryParse(gridNameComponents[0], out n);
                if (isNumeric)
                {
                    ramGrid.GridTypeNaming = RAMGrid.GridTypeNamingClassification.Numbered;
                }
                else
                {
                    ramGrid.GridTypeNaming = RAMGrid.GridTypeNamingClassification.Lettered;
                }
            }
            else
            {
                int n;
                bool isNumeric = int.TryParse(gridName, out n);
                if(isNumeric)
                {
                    ramGrid.GridTypeNaming = RAMGrid.GridTypeNamingClassification.Numbered;
                }
                else
                {
                    ramGrid.GridTypeNaming = RAMGrid.GridTypeNamingClassification.Lettered;
                }
            }

        }

        //public static void DeserializeRAMOrigin(RAMModel ramModel)
        //{
        //    string path = @"C:\dev\RAM Reaction Importer\RAM-Reaction-Importer\RAMStoryData.txt";
        //    string storyDataString = "";
        //    Char lineDelimiter = ';';
        //    Char propertyDelimiter = ',';
        //    using (StreamReader sr = new StreamReader(path))
        //    {
        //        // Read the stream to a string.
        //        storyDataString = sr.ReadToEnd();
        //    }
        //    String[] allStoryData = storyDataString.Split(lineDelimiter);
        //    foreach (var singleStoryData in allStoryData)
        //    {
        //        string[] storyProperties = singleStoryData.Split(propertyDelimiter);
        //        Story ramStory = new Story(Convert.ToInt32(storyProperties[0]), storyProperties[1], storyProperties[2], Convert.ToDouble(storyProperties[3]), Convert.ToDouble(storyProperties[4]));
        //        ramModel.Stories.Add(ramStory);
        //    }


        //}


    }


}

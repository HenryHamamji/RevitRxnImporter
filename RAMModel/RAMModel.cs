using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace RevitReactionImporter
{
    public class RAMModel
    {
        public List<RAMBeam> RamBeams {get; set;}
        public double[] OriginRAM { get; set; }
        public int LevelCount { get; set; }
        public List<Story> Stories { get; set; }
        public RAMModel()
        {
            RamBeams = new List<RAMBeam>();
            OriginRAM = new double[3];
            Stories = new List<Story>();
        }

        public class Story
        {
            public int Level { get; set; }
            public string StoryLabel { get; set; }
            public string LayoutType { get; set; }
            public double Height { get; set; }
            public double Elevation { get; set; }

            public Story(int level, string storyLabel, string layoutType, double height, double elevation)
            {
                Level = level;
                StoryLabel = storyLabel;
                LayoutType = layoutType;
                Height = height;
                Elevation = elevation;
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
            }
        }

        public static void ExecutePythonScript()
        {
            Process process = new Process();

            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                WorkingDirectory = @"C:\dev\RAM Reaction Importer\RAM-Reaction-Importer",
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal,
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


        public static void DeserializeRAMModel()
        {
            var RAMModel = new RAMModel();

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
                RAMBeam ramBeam = new RAMBeam(beamProperties[0], beamProperties[1], Convert.ToDouble(beamProperties[2]), Convert.ToDouble(beamProperties[3]),
                    Convert.ToDouble(beamProperties[4]), Convert.ToDouble(beamProperties[5]), Convert.ToDouble(beamProperties[6]), Convert.ToDouble(beamProperties[7]));
                ramBeam.IsCantilevered = isCantilevered;
                RAMModel.RamBeams.Add(ramBeam);
                }
            DeserializeRAMStoryData(RAMModel);

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

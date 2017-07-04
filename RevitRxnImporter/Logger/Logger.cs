using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Net;
using System.Windows.Threading;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;

namespace RevitReactionImporter
{
    public class Logger
    {
        public static ModelCompare.Results Results {get; set;}
        public static string ProjectId { get; set; }
        public Logger(string projectId, ModelCompare.Results results)
        {
            ProjectId = projectId;
            Results = results;
        }

        public static void LocalLog()
        {
            var path = PathUtils.GetLogFile(ProjectId);
            path = PathUtils.GetUserFreandlyLogFile(ProjectId);
            File.WriteAllText(path, String.Empty);
            //var result = "test stirng to append";
            //File.AppendAllText(path, result + Environment.NewLine);
            using (var stream = new FileStream(path, FileMode.Truncate))
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(Properties.Resources.RAM_REACTION_IMPORTER_LOG_HEADER + Environment.NewLine);
                    writer.Write("Revit Level to RAM Floor Layout Type Mapping:" + Environment.NewLine);
                    LogStringToStringMappingData(Results.RevitLevelToRAMFloorLayoutTypeMapping, writer);
                    writer.Write("RAM Mapping Results by Floor:" + Environment.NewLine);
                    LogStringToStringMappingData(Results.RAMMappingResultsByFloor, writer);
                    writer.Write("Revit Mapping Results by Floor:" + Environment.NewLine);
                    LogStringToStringMappingData(Results.RevitMappingResultsByFloor, writer);
                    writer.Write("Total Number of Mapped Beams:" + Results.TotalMappedBeamCount.ToString() + Environment.NewLine);
                    writer.Write(Environment.NewLine + "Revit Beam Tolerances for Mapping Success (inches):" + Environment.NewLine);
                    LogStringToDoubleMappingData(Results.RevitBeamTolerancesForMappingSuccess, writer);

                }
            }
        }

        public static void LogStringToStringMappingData(Dictionary<string, string> dictionary, StreamWriter writer)
        {
            int indentSize = 8;
            var indent = new string(' ', indentSize);

            foreach (var entry in dictionary)
                writer.WriteLine(indent + "{0} : {1}", entry.Key, entry.Value);
                writer.Write(Environment.NewLine);
        }

        public static void LogStringToDoubleMappingData(Dictionary<string, double> dictionary, StreamWriter writer)
        {
            int indentSize = 8;
            var indent = new string(' ', indentSize);

            foreach (var entry in dictionary)
                writer.WriteLine(indent + "{0} : {1}", entry.Key, entry.Value.ToString());
            writer.Write(Environment.NewLine);
        }



    }
}

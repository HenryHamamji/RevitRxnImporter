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
        public static string ProjectId { get; set; }

        public static void LocalLog()
        {
            var file = PathUtils.GetLogFile(ProjectId);
            file = PathUtils.GetUserFreandlyLogFile(ProjectId);

            var result = "test stirng to append";
            File.AppendAllText(file, result + Environment.NewLine);
        }

    }
}

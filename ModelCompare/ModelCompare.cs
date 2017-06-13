using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitReactionImporter
{
    public class ModelCompare
    {
        public void GenerateRevitToRAMLevelMapping(RAMModel.Story levelRAM, Dictionary<string, double> levelRevit)
        {

        }

        public void CompareLevelNames(string levelNameRAM, string levelNameRevit)
        {
            var levelNameRAMLowerCase = levelNameRAM.ToLower();
            var levelNameRevitLowerCase = levelNameRevit.ToLower();
            if(levelNameRAMLowerCase == levelNameRevitLowerCase)
            {

            }

        }
    }
}

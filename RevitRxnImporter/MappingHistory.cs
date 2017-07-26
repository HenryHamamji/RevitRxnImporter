using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitReactionImporter
{
    public class MappingHistory
    {
        public bool IsLevelMappingSetByUser { get; set; }
        public Dictionary<int, string> LevelMappingFromUser { get; set; }

        public MappingHistory(bool isLevelMappingSetByUser, Dictionary<int, string> levelMappingFromUser)
        {
            IsLevelMappingSetByUser = isLevelMappingSetByUser;
            LevelMappingFromUser = levelMappingFromUser;
        }

    }
}

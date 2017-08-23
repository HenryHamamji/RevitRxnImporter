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
        public bool BeamReactionsImported { get; set; }
        public bool BeamStudCountsImported { get; set; }
        public bool BeamCamberValuesImported { get; set; }
        public bool BeamSizesImported { get; set; }

        public MappingHistory(bool isLevelMappingSetByUser, Dictionary<int, string> levelMappingFromUser, bool beamReactionsImported, bool beamStudCountsImported, bool beamCamberValuesImported, bool beamSizesImported)
        {
            IsLevelMappingSetByUser = isLevelMappingSetByUser;
            LevelMappingFromUser = levelMappingFromUser;
            BeamReactionsImported = beamReactionsImported;
            BeamStudCountsImported = beamStudCountsImported;
            BeamCamberValuesImported = beamCamberValuesImported;
            BeamSizesImported = beamSizesImported;
        }

    }

}

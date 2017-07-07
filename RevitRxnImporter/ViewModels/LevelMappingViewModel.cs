using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.ObjectModel;

namespace RevitReactionImporter
{
    public class LevelMappingViewModel
    {

        private RevitReactionImporterApp _rria;
        private string _projectId = "";

        //private readonly JsonSerializerSettings jsonSettings;
        private AnalyticalModel _analyticalModel = null;
        private RAMModel _ramModel = null;

        private LevelMappingView _view = null; // TODO: replace this connection with data binding
        //private LevelMappingView _view { get { return _view; } }

        private Document _document;
        public AnalyticalModel AnalyticalModel { get { return _analyticalModel; } }
        public RAMModel RAMModel { get { return _ramModel; } }
        public ObservableCollection<string> RevitLevelNames { get; private set; }
        public LevelMappingViewModel(LevelMappingView view, Document doc,
            RevitReactionImporterApp rria)
        {
            RevitLevelNames = new ObservableCollection<string>();
            if(_analyticalModel!=null)
            {
                RevitLevelNames = PopulateRevitLevels(_analyticalModel.LevelInfo);

            }
            RevitLevelNames = new ObservableCollection<string>
            {
                "test 1",
                "test 2"
            };
            _rria = rria;
            _view = view;
            _view.ViewModel = this;
            _document = doc;
            _view.RevitLevelNames = RevitLevelNames;

            IList<RibbonItem> ribbonItems = _rria.RibbonPanel.GetItems();
        }

        public ObservableCollection<string> PopulateRevitLevels(LevelInfo revitLevelInfo)
        {
            _view.RevitLevelNames = new ObservableCollection<string>();

            foreach (var revitLevel in revitLevelInfo.Levels)
            {
                _view.RevitLevelNames.Add(revitLevel.Name);
            }
            var revitLevelNames = _view.RevitLevelNames;
            return revitLevelNames;
        }


    }
}

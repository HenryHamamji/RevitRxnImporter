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
        private Document _document;
        public AnalyticalModel AnalyticalModel { get { return _analyticalModel; } }
        public RAMModel RAMModel { get { return _ramModel; } }
        public ObservableCollection<string> CmbContent { get; private set; }

        public LevelMappingViewModel(LevelMappingView view, Document doc,
            RevitReactionImporterApp rria)
        {
            CmbContent = new ObservableCollection<string>
            {
                "test 1",
                "test 2"
            };
            _rria = rria;
            _view = view;
            _view.ViewModel = this;
            _document = doc;

            IList<RibbonItem> ribbonItems = _rria.RibbonPanel.GetItems();
        }


    }
}

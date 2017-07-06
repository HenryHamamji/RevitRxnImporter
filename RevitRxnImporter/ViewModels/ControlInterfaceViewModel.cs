using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Linq;
using System.Windows.Media;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Reflection;
//using Newtonsoft.Json;


namespace RevitReactionImporter
{
    public class ControlInterfaceViewModel
    {
        private RevitReactionImporterApp _rria;
        private string _projectId = "";

        //private readonly JsonSerializerSettings jsonSettings;
        private AnalyticalModel _analyticalModel = null;
        private RAMModel _ramModel = null;
        private LevelMappingViewModel LevelMappingViewModel = null;

        private ControlInterfaceView _view = null; // TODO: replace this connection with data binding
        public LevelMappingView _levelMappingView = null;
        private Document _document;
        public AnalyticalModel AnalyticalModel { get { return _analyticalModel; } }
        public RAMModel RAMModel { get { return _ramModel; } }

        public ControlInterfaceViewModel(ControlInterfaceView view, Document doc,
            RevitReactionImporterApp rria, LevelMappingViewModel levelMappingViewModel, string projectId)
        {
            _rria = rria;

            _view = view;
            _view.ViewModel = this;
            LevelMappingViewModel = levelMappingViewModel;

            _document = doc;
            _projectId = projectId;


            IList<RibbonItem> ribbonItems = _rria.RibbonPanel.GetItems();
            //_levelMappingView = new LevelMappingView();
        }

        public void DocumentClosed()
        {
            // Document already closed, we can't do anything.
        }


        public void ImportBeamReactions()
        {


            //_controlPaneId = new DockablePaneId(Guid.NewGuid());

            //ControlInterfaceView = new ControlInterfaceView();

            //RevitApplication.RegisterDockablePane(_controlPaneId, "RAM to Revit Reaction Importer", ControlInterfaceView);

            RAMModel.ExecutePythonScript();
            RAMModel _ramModel = RAMModel.DeserializeRAMModel();
            _analyticalModel = ExtractAnalyticalModel.ExtractFromRevitDocument(_document);
            ShowLevelMappingPane();
            ModelCompare.Results results = ModelCompare.CompareModels(_ramModel, _analyticalModel);
            System.Windows.Forms.MessageBox.Show("Model Compare Working");
            var logger = new Logger(_projectId, results);
            Logger.LocalLog();



        }


        public void ResetBeamReactions()
        {
        }

        internal void ShowLevelMappingPane()
        {
            _rria.SetupLevelMappingPane();
        }

    }
}

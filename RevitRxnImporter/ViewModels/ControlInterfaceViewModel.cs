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
        public string _projectId = "";

        //private readonly JsonSerializerSettings jsonSettings;
        private AnalyticalModel _analyticalModel = null;
        private RAMModel _ramModel = null;
        private ModelCompare _modelCompare = null;

        private LevelMappingViewModel LevelMappingViewModel = null;
        private ControlInterfaceView _view = null; // TODO: replace this connection with data binding
        public LevelMappingView _levelMappingView = null;
        private Document _document;
        public AnalyticalModel AnalyticalModel { get { return _analyticalModel; } }
        public ModelCompare ModelCompare { get { return _modelCompare; } }
        public RAMModel RAMModel { get { return _ramModel; } }
        //public bool IsLevelMappingSetByUser { get; set; }
        public string RAMModelMetaDataFilePath { get; set; }
        public string RAMModelReactionsFilePath { get; set; }
        public string RAMModelStudsFilePath { get; set; }
        public string RAMModelCamberFilePath { get; set; }
        public string RAMModelSizesFilePath { get; set; }


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

            RAMModel.ExecutePythonScript(RAMModelMetaDataFilePath);
            RAMModel _ramModel = RAMModel.DeserializeRAMModel();
            _analyticalModel = ExtractAnalyticalModel.ExtractFromRevitDocument(_document);
            //if(!LevelMappingViewModel.IsLevelMappingSetByUser)
            //{
                ShowLevelMappingPane(_analyticalModel.LevelInfo, _ramModel.Stories, RAMModelMetaDataFilePath);
            //}

            ModelCompare.Results results = ModelCompare.CompareModels(_ramModel, _analyticalModel);
            System.Windows.Forms.MessageBox.Show("Model Compare Working");
            var logger = new Logger(_projectId, results);
            Logger.LocalLog();



        }


        public void ResetBeamReactions()
        {
        }

        internal void ShowLevelMappingPane(LevelInfo revitLevelInfo, List<RAMModel.Story> ramStories, string filePath)
        {
            LevelMappingViewModel.PopulateRevitLevelsAndRAMFloorLayoutTypesOptions(revitLevelInfo, ramStories);
            LevelMappingViewModel.PopulateLevelMapping(LevelMappingViewModel.LoadMappingHistoryFromDisk(), filePath);
            _rria.SetupLevelMappingPane();
        }

        internal void ConfigureLevelMapping()
        {
            RAMModel.ExecutePythonScript(RAMModelMetaDataFilePath); // list of file paths
            RAMModel _ramModel = RAMModel.DeserializeRAMModel();
            _analyticalModel = ExtractAnalyticalModel.ExtractFromRevitDocument(_document);
            ShowLevelMappingPane(_analyticalModel.LevelInfo, _ramModel.Stories, RAMModelMetaDataFilePath);
        }

        internal void ShowDataFileBrowserWindow(string projectId)
        {
            DataFileBrowser dataFileBrowser = new DataFileBrowser(projectId, _view);
            dataFileBrowser.Show();
        }


        internal void AssignDataFiles(string filePath) // list of filepaths
        {
            RAMModelMetaDataFilePath = filePath;
        }

    }
}

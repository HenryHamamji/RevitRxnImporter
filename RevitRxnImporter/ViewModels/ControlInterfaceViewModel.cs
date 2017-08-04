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
        //public Dictionary<int, string> LevelMappingFromUser { get; private set; }
        public string RAMModelMetaDataFilePath { get; set; }
        public string RAMModelReactionsFilePath { get; set; }
        public string RAMModelStudsFilePath { get; set; }
        public string RAMModelCamberFilePath { get; set; }
        public List<string> RAMFiles { get; set; }

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
            RAMFiles = new List<string>();
            //_levelMappingView = new LevelMappingView();
        }

        public void DocumentClosed()
        {
            // Document already closed, we can't do anything.
        }


        public void ImportBeamReactions()
        {

            // Gather the input files.
            GatherRAMFiles();
            // Check if required files are loaded.
            if (!AreRequiredFilesForBeamReactionsLoaded())
            {
                System.Windows.Forms.MessageBox.Show("RAM Model & RAM Beam Reaction Files Have Not Been Loaded. Please Load these two files.");
                return;
            }

            RAMModel.ExecutePythonScript(RAMFiles);

            RAMModel _ramModel = RAMModel.DeserializeRAMModel();
            _analyticalModel = ExtractAnalyticalModel.ExtractFromRevitDocument(_document);
            //if(!LevelMappingViewModel.IsLevelMappingSetByUser)
            //{
                ShowLevelMappingPane(_analyticalModel.LevelInfo, _ramModel.Stories, RAMFiles);
            //}
            ModelCompare.Results results = ModelCompare.CompareModels(_ramModel, _analyticalModel, LevelMappingViewModel.LevelMappingFromUser);
            System.Windows.Forms.MessageBox.Show("Model Compare Working");
            var logger = new Logger(_projectId, results);
            Logger.LocalLog();
        }

        internal bool AreRequiredFilesForBeamReactionsLoaded()
        {
            bool modelFilePresent = !string.IsNullOrEmpty(RAMModelMetaDataFilePath);
            bool reactionsFilePresent = !string.IsNullOrEmpty(RAMModelReactionsFilePath);
            if(modelFilePresent && reactionsFilePresent)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        internal bool AreRequiredFilesForBeamStudsOrBeamSizesLoaded()
        {
            // beam sizes file path is the same as the studs file path = summary file.
            bool modelFilePresent = !string.IsNullOrEmpty(RAMModelMetaDataFilePath);
            bool studsorBeamSizesFilePresent = !string.IsNullOrEmpty(RAMModelStudsFilePath);
            if (modelFilePresent && studsorBeamSizesFilePresent)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        internal bool AreRequiredFilesForBeamCamberLoaded()
        {
            bool modelFilePresent = !string.IsNullOrEmpty(RAMModelMetaDataFilePath);
            bool camberFilePresent = !string.IsNullOrEmpty(RAMModelCamberFilePath);
            if (modelFilePresent && camberFilePresent)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        internal void GatherRAMFiles()
        {
            LoadRAMMetaDataFileHistoryFromDisk();
            var files = new List<string>();
            if (!string.IsNullOrEmpty(RAMModelMetaDataFilePath))
            {
                files.Add(RAMModelMetaDataFilePath);
            }
            if(!string.IsNullOrEmpty(RAMModelReactionsFilePath))
            {
                files.Add(RAMModelReactionsFilePath);
            }
            if(!string.IsNullOrEmpty(RAMModelStudsFilePath))
            {
                files.Add(RAMModelStudsFilePath);
            }
            if(!string.IsNullOrEmpty(RAMModelCamberFilePath))
            {
                files.Add(RAMModelCamberFilePath);
            }

            RAMFiles = files;
        }

        private void LoadRAMMetaDataFileHistoryFromDisk()
        {
            string fullPath = GetMetaDataFile(_projectId);

            if (!File.Exists(fullPath))
                return;

            var text = File.ReadAllLines(fullPath);

            RAMModelMetaDataFilePath = text[0];
            RAMModelReactionsFilePath = text[1];
            RAMModelStudsFilePath = text[2];
            RAMModelCamberFilePath = text[3];

        }

        internal static string GetMetaDataFile(string projectId)
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dir = System.IO.Path.Combine(folder, @"RevitRxnImporter\metadata");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            return System.IO.Path.Combine(dir, string.Format("Project-{0}_metadata.csv", projectId));
        }



        public void ResetBeamReactions()
        {
        }

        internal void ShowLevelMappingPane(LevelInfo revitLevelInfo, List<RAMModel.Story> ramStories, List<string> filePaths)
        {
            LevelMappingViewModel.PopulateRevitLevelsAndRAMFloorLayoutTypesOptions(revitLevelInfo, ramStories);
            LevelMappingViewModel.PopulateLevelMapping(LevelMappingViewModel.LoadMappingHistoryFromDisk());
            _rria.SetupLevelMappingPane();
        }

        internal void ConfigureLevelMapping()
        {
            GatherRAMFiles();
            RAMModel.ExecutePythonScript(RAMFiles);
            RAMModel _ramModel = RAMModel.DeserializeRAMModel();

            _analyticalModel = ExtractAnalyticalModel.ExtractFromRevitDocument(_document);
            ShowLevelMappingPane(_analyticalModel.LevelInfo, _ramModel.Stories, RAMFiles);
        }

        internal void ShowDataFileBrowserWindow(string projectId)
        {
            DataFileBrowser dataFileBrowser = new DataFileBrowser(projectId, _view);
            dataFileBrowser.Show();
        }


        internal void AssignRAMModelDataFile(string filePath)
        {
            RAMModelMetaDataFilePath = filePath;
        }

        internal void AssignRAMReactionsDataFile(string filePath)
        {
            RAMModelReactionsFilePath = filePath;
        }

        internal void AssignRAMStudsDataFile(string filePath)
        {
            RAMModelStudsFilePath = filePath;
        }

        internal void AssignRAMCamberDataFile(string filePath)
        {
            RAMModelCamberFilePath = filePath;
        }

    }
}

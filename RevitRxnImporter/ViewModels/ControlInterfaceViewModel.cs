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
        public ModelCompare.Results Results { get; set; }

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
            Results = results;
            System.Windows.Forms.MessageBox.Show("Model Compare Working");
            var logger = new Logger(_projectId, results);
            Logger.LocalLog();
        }

        public void ImportStudCounts(string ramStudsFilePath)
        {
            if (!AreRequiredFilesForBeamStudsLoaded())
            {
                System.Windows.Forms.MessageBox.Show("RAM Model, RAM Beam Reaction, & RAM Studs Files Have Not Been Loaded. Please Load these two files.");
                return;
            }
            GatherRAMFiles();
            RAMModel.ExecutePythonScript(RAMFiles);

            RAMModel _ramModel = RAMModel.DeserializeRAMModel();
            var dict = _ramModel.ParseStudFile(ramStudsFilePath);
            _ramModel.MapStudCountsToRAMBeams(dict, _ramModel.RamBeams);
        }

        public void ImportCamberValues(string ramCamberFilePath)
        {
            if (!AreRequiredFilesForBeamCamberLoaded())
            {
                System.Windows.Forms.MessageBox.Show("RAM Model, RAM Beam Reaction, & RAM Camber Files Have Not Been Loaded. Please Load these two files.");
                return;
            }
            GatherRAMFiles();
            RAMModel.ExecutePythonScript(RAMFiles);

            RAMModel _ramModel = RAMModel.DeserializeRAMModel();
            var beamsFromCamberFile = RAMModel.CamberParser.ParseCamberFile(ramCamberFilePath);
            _ramModel.MapCamberToRAMBeams(beamsFromCamberFile, _ramModel.RamBeams);
        }

        public void ImportBeamSizes()
        {
            // Gather the input files.
            GatherRAMFiles();
            // Check if required files are loaded.
            if (!AreRequiredFilesForBeamSizingLoaded())
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

        internal bool AreRequiredFilesForBeamStudsLoaded()
        {
            bool modelFilePresent = !string.IsNullOrEmpty(RAMModelMetaDataFilePath);
            bool reactionsFilePresent = !string.IsNullOrEmpty(RAMModelReactionsFilePath);
            bool studsFilePresent = !string.IsNullOrEmpty(RAMModelStudsFilePath);

            if (modelFilePresent && studsFilePresent && reactionsFilePresent)
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
            bool reactionsFilePresent = !string.IsNullOrEmpty(RAMModelReactionsFilePath);
            bool camberFilePresent = !string.IsNullOrEmpty(RAMModelCamberFilePath);
            if (modelFilePresent && camberFilePresent && reactionsFilePresent)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        internal bool AreRequiredFilesForBeamSizingLoaded()
        {
            bool modelFilePresent = !string.IsNullOrEmpty(RAMModelMetaDataFilePath);
            bool reactionsFilePresent = !string.IsNullOrEmpty(RAMModelReactionsFilePath);
            if (modelFilePresent && reactionsFilePresent)
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



        public void ClearBeamData()
        {
            //DataInputSelectionForClearData dataInputSelectionForClearData = new DataInputSelectionForClearData();
            //dataInputSelectionForClearData.Show();
            ClearAnnotationsMain clearAnnotationsMain = new ClearAnnotationsMain();
            clearAnnotationsMain.Show();
        }

        internal void ShowSelectDataInputWindow()
        {
            var annotationTypeSelectionForVisualization = new AnnotationTypeSelectionForVisualization(_view);
            annotationTypeSelectionForVisualization.Show();
        }

        public void ResetVisualization()
        {
            var viewType = _document.ActiveView.ViewType;
            if (viewType.ToString() != "EngineeringPlan")
            {
                System.Windows.Forms.MessageBox.Show("Please go to the Framing Plan View in order to visualize the results of the RAM data import.");
                return;
            }
            _analyticalModel = ExtractAnalyticalModel.ExtractFromRevitDocument(_document);
            var beamsToReset = new List<Beam>();
            if(Results == null)
            {
                beamsToReset = _analyticalModel.StructuralMembers.Beams;
            }
            else
            {
                beamsToReset = Results.ModelBeamList;
            }

            ResultsVisualizer resultsVisualizer = new ResultsVisualizer(_document);
            resultsVisualizer.ResetVisualsInActiveView(beamsToReset);

        }

        public void VisualizeData(string annotationToVisualize)
        {

            var viewType = _document.ActiveView.ViewType;
            if(viewType.ToString() != "EngineeringPlan")
            {
                System.Windows.Forms.MessageBox.Show("Please go to the Framing Plan View in order to visualize the results of the RAM data import.");
                return;
            }

            ResultsVisualizer resultsVisualizer = new ResultsVisualizer(_document);
            _analyticalModel = ExtractAnalyticalModel.ExtractFromRevitDocument(_document);

            resultsVisualizer.ColorMembers(_analyticalModel, annotationToVisualize, Results.MappedRevitBeams, Results.ModelBeamList);
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

        internal void ShowSelectAnnotationToVisualizeWindow()
        {
            if (Results == null)
            {
                System.Windows.Forms.MessageBox.Show("No RAM data imported. Import RAM data in order to visualize import results.");
                return;
            }
            var annotationTypeSelectionForVisualization = new AnnotationTypeSelectionForVisualization(_view);
            annotationTypeSelectionForVisualization.Show();
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

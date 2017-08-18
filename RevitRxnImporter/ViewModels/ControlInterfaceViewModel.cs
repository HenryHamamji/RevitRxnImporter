﻿using System;
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
using Newtonsoft.Json;


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
        public bool IsLevelMappingSetByUser { get; set; }
        public Dictionary<int, string> LevelMappingFromUser { get; private set; }
        public string RAMModelMetaDataFilePath { get; set; }
        public string RAMModelReactionsFilePath { get; set; }
        public string RAMModelStudsFilePath { get; set; }
        public string RAMModelCamberFilePath { get; set; }
        public List<string> RAMFiles { get; set; }
        public ModelCompare.Results Results { get; set; }
        public bool IsSingleImportPressed { get; set; }
        public bool IsMultipleImportPressed { get; set; }
        public DesignCode UserSetDesignCode { get; set; }
        public LevelMappingView LevelMappingView { get; set; }
        public bool BeamReactionsImported { get; set; }
        public bool BeamStudCountsImported { get; set; }
        public bool BeamCamberValuesImported { get; set; }
        public bool BeamSizesImported { get; set; }


        public ControlInterfaceViewModel(ControlInterfaceView view, Document doc,
            RevitReactionImporterApp rria, string projectId)
        {
            IsLevelMappingSetByUser = false;
            BeamReactionsImported = false;
            BeamStudCountsImported = false;
            BeamCamberValuesImported = false;
            BeamSizesImported = false;

            _rria = rria;
            _view = view;
            _view.ViewModel = this;

            _document = doc;
            _projectId = projectId;
            RAMFiles = new List<string>();

            LevelMappingView = new LevelMappingView(this);
            LevelMappingViewModel = new LevelMappingViewModel(LevelMappingView, _document, _projectId, BeamReactionsImported, BeamStudCountsImported, BeamCamberValuesImported, BeamSizesImported);
            LevelMappingView.ViewModel = LevelMappingViewModel;
     
            var mappingHistory = LoadMappingHistoryFromDisk();


        }

        public void DocumentClosed()
        {
            // Document already closed, we can't do anything.
        }

        private static string GetLevelMappingHistoryFile(string projectId)
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dir = System.IO.Path.Combine(folder, @"RevitRxnImporter\history");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            return System.IO.Path.Combine(dir, string.Format("Project-{0}_history.txt", projectId));
        }

        public MappingHistory LoadMappingHistoryFromDisk()
        {
            string fullPath = GetLevelMappingHistoryFile(_projectId);

            if (!File.Exists(fullPath))
                return new MappingHistory(false, LevelMappingFromUser, false, false, false, false);

            var text = File.ReadAllText(fullPath);

            MappingHistory levelMappingHistory;

            try
            {
                levelMappingHistory = JsonConvert.DeserializeObject<MappingHistory>(text);
            }
            catch
            {
                return new MappingHistory(IsLevelMappingSetByUser, LevelMappingFromUser, BeamReactionsImported, BeamStudCountsImported, BeamCamberValuesImported, BeamSizesImported);
            }

            IsLevelMappingSetByUser = levelMappingHistory.IsLevelMappingSetByUser;
            LevelMappingFromUser = levelMappingHistory.LevelMappingFromUser;
            BeamReactionsImported = levelMappingHistory.BeamReactionsImported;
            BeamStudCountsImported = levelMappingHistory.BeamStudCountsImported;
            BeamCamberValuesImported = levelMappingHistory.BeamCamberValuesImported;
            BeamSizesImported = levelMappingHistory.BeamSizesImported;

            return levelMappingHistory;
        }

        public void SaveLevelMappingHistoryToDisk()
        {
            EnsureLevelMappingHistoryDirectoryExists();

            string fullPath = GetLevelMappingHistoryFile(_projectId);

            var history = new MappingHistory(IsLevelMappingSetByUser, LevelMappingFromUser, BeamReactionsImported, BeamStudCountsImported, BeamCamberValuesImported, BeamSizesImported);
            var histJson = JsonConvert.SerializeObject(history, Formatting.Indented);

            System.IO.File.WriteAllText(fullPath, histJson);
        }

        private void EnsureLevelMappingHistoryDirectoryExists()
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dir = System.IO.Path.Combine(folder, "RevitRxnImporter");

            if (Directory.Exists(dir))
                return;

            Directory.CreateDirectory(dir);
        }

        public int GetLevelIdOfActiveView()
        {
            var activeView = _document.ActiveView;
            var viewType = activeView.ViewType;
            if (viewType.ToString() != "EngineeringPlan")
            {
                System.Windows.Forms.MessageBox.Show("Please go to a Framing Plan View in order to import the relevant RAM data.");
                return -1;
            }
            var level = activeView.GenLevel;
            return Int32.Parse(level.Id.ToString());
        }

        public bool IsImportModeSingle(bool isSingleImportPressed, bool isMultipleImportPressed)
        {
            bool isImportModeSingle;
            if (IsSingleImportPressed && !IsMultipleImportPressed)
            {
                isImportModeSingle = true;
            }
            else if (!IsSingleImportPressed && IsMultipleImportPressed)
            {
                isImportModeSingle = false;
            }
            else
            {
                throw new Exception("Need to debug: No import mode selected or more than one import mode selected.");
            }
            return isImportModeSingle;
        }

        public bool CheckLevelMappingRequirement()
        {
            if (!IsLevelMappingSetByUser)
            {
                System.Windows.Forms.MessageBox.Show("Level mapping has not been set. Please set the Revit Level to RAM Floor Layout Type Mapping.");
                return false;
            }
            return true;
        }

        public bool ImportModeSelected()
        {
            if (!IsMultipleImportPressed && !IsSingleImportPressed)
            {
                System.Windows.Forms.MessageBox.Show("Please select an import mode: Single level or multiple level import.");
                return false;
            }
            return true;
        }

        public void ImportBeamReactions()
        {
            AnnotationType annotationType = AnnotationType.Reaction;
            int singleLevelId = -1;
            // Gather the input files.
            GatherRAMFiles();
            // Check if required files are loaded.
            if (!AreRequiredFilesForBeamReactionsLoaded())
            {
                System.Windows.Forms.MessageBox.Show("RAM Model & RAM Beam Reaction Files Have Not Been Loaded. Please Load these two files.");
                return;
            }

            if (!CheckLevelMappingRequirement())
                return;
            if (!ImportModeSelected())
                return;
            var isImportModeSingle = IsImportModeSingle(IsSingleImportPressed, IsMultipleImportPressed);
            if(isImportModeSingle)
            {
                singleLevelId = GetLevelIdOfActiveView();
                if (singleLevelId < 0)
                    return;
            }
            
            RAMModel.ExecutePythonScript(RAMFiles);
            RAMModel _ramModel = RAMModel.DeserializeRAMModel(UserSetDesignCode);
            _analyticalModel = ExtractAnalyticalModel.ExtractFromRevitDocument(_document);
            if(!LevelMappingViewModel.IsLevelMappingSetByUser)
            {
                ShowLevelMappingPane(_analyticalModel.LevelInfo, _ramModel.Stories, RAMFiles);
            }
            ModelCompare.Results results = ModelCompare.CompareModels(_ramModel, _analyticalModel, LevelMappingViewModel.LevelMappingFromUser, isImportModeSingle, singleLevelId);
            Results = results;
            ResultsAnnotator.AnnotateBeams(_document, results, annotationType);
            //System.Windows.Forms.MessageBox.Show("Mapped Beam Count= " + results.MappedRevitBeams.Count.ToString());
            var logger = new Logger(_projectId, results);
            Logger.LocalLog();
            BeamReactionsImported = true;
            SaveLevelMappingHistoryToDisk();
        }

        public void ImportStudCounts()
        {
            AnnotationType annotationType = AnnotationType.StudCount;
            int singleLevelId = -1;

            // Gather the input files.
            GatherRAMFiles();
            
            if (!AreRequiredFilesForBeamStudsLoaded())
            {
                System.Windows.Forms.MessageBox.Show("RAM Model, RAM Beam Reaction, & RAM Studs Files Have Not Been Loaded. Please Load these two files.");
                return;
            }

            if (!CheckLevelMappingRequirement())
                return;
            if (!ImportModeSelected())
                return;
            var isImportModeSingle = IsImportModeSingle(IsSingleImportPressed, IsMultipleImportPressed);
            if (isImportModeSingle)
            {
                singleLevelId = GetLevelIdOfActiveView();
                if (singleLevelId < 0)
                    return;
            }
            RAMModel.ExecutePythonScript(RAMFiles);
            RAMModel _ramModel = RAMModel.DeserializeRAMModel(UserSetDesignCode);
            _analyticalModel = ExtractAnalyticalModel.ExtractFromRevitDocument(_document);


            if (!LevelMappingViewModel.IsLevelMappingSetByUser)
            {
                ShowLevelMappingPane(_analyticalModel.LevelInfo, _ramModel.Stories, RAMFiles);
            }
            ModelCompare.Results results = ModelCompare.CompareModels(_ramModel, _analyticalModel, LevelMappingViewModel.LevelMappingFromUser, isImportModeSingle, singleLevelId);
            Results = results;
            var dict = _ramModel.ParseStudFile(RAMModelStudsFilePath);
            _ramModel.MapStudCountsToRAMBeams(dict, _ramModel.RamBeams);
            ResultsAnnotator.AnnotateBeams(_document, results, annotationType);
            var logger = new Logger(_projectId, results);
            Logger.LocalLog();
            BeamStudCountsImported = true;
            SaveLevelMappingHistoryToDisk();

        }

        public void ImportCamberValues()
        {
            AnnotationType annotationType = AnnotationType.Camber;
            int singleLevelId = -1;
            // Gather the input files.
            GatherRAMFiles();

            if (!AreRequiredFilesForBeamCamberLoaded())
            {
                System.Windows.Forms.MessageBox.Show("RAM Model, RAM Beam Reaction, & RAM Camber Files Have Not Been Loaded. Please Load these two files.");
                return;
            }

            if (!CheckLevelMappingRequirement())
                return;
            if (!ImportModeSelected())
                return;
            var isImportModeSingle = IsImportModeSingle(IsSingleImportPressed, IsMultipleImportPressed);
            if (isImportModeSingle)
            {
                singleLevelId = GetLevelIdOfActiveView();
                if (singleLevelId < 0)
                    return;
            }
            RAMModel.ExecutePythonScript(RAMFiles);
            RAMModel _ramModel = RAMModel.DeserializeRAMModel(UserSetDesignCode);
            _analyticalModel = ExtractAnalyticalModel.ExtractFromRevitDocument(_document);

            var beamsFromCamberFile = RAMModel.CamberParser.ParseCamberFile(RAMModelCamberFilePath);
            _ramModel.MapCamberToRAMBeams(beamsFromCamberFile, _ramModel.RamBeams);
            if (!LevelMappingViewModel.IsLevelMappingSetByUser)
            {
                ShowLevelMappingPane(_analyticalModel.LevelInfo, _ramModel.Stories, RAMFiles);
            }
            ModelCompare.Results results = ModelCompare.CompareModels(_ramModel, _analyticalModel, LevelMappingViewModel.LevelMappingFromUser, isImportModeSingle, singleLevelId);
            Results = results;
            ResultsAnnotator.AnnotateBeams(_document, results, annotationType);
            var logger = new Logger(_projectId, results);
            Logger.LocalLog();
            BeamCamberValuesImported = true;
            SaveLevelMappingHistoryToDisk();

        }

        public void ImportBeamSizes()
        {
            return;
            AnnotationType annotationType = AnnotationType.Size;
            int singleLevelId = -1;
            // Gather the input files.
            GatherRAMFiles();
            // Check if required files are loaded.
            if (!AreRequiredFilesForBeamSizingLoaded())
            {
                System.Windows.Forms.MessageBox.Show("RAM Model & RAM Beam Reaction Files Have Not Been Loaded. Please Load these two files.");
                return;
            }

            if (!CheckLevelMappingRequirement())
                return;
            if (!ImportModeSelected())
                return;
            var isImportModeSingle = IsImportModeSingle(IsSingleImportPressed, IsMultipleImportPressed);
            if (isImportModeSingle)
            {
                singleLevelId = GetLevelIdOfActiveView();
                if (singleLevelId < 0)
                    return;
            }
            RAMModel.ExecutePythonScript(RAMFiles);
            RAMModel _ramModel = RAMModel.DeserializeRAMModel(UserSetDesignCode);
            _analyticalModel = ExtractAnalyticalModel.ExtractFromRevitDocument(_document);
            if(!LevelMappingViewModel.IsLevelMappingSetByUser)
            {
                ShowLevelMappingPane(_analyticalModel.LevelInfo, _ramModel.Stories, RAMFiles);
            }
            //ModelCompare.Results results = ModelCompare.CompareModels(_ramModel, _analyticalModel, LevelMappingViewModel.LevelMappingFromUser);
            BeamSizesImported = true;
            SaveLevelMappingHistoryToDisk();

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

        //private void LoadRAMMetaDataFileHistoryFromDisk()
        //{
        //    string fullPath = GetMetaDataFile(_projectId);

        //    if (!File.Exists(fullPath))
        //        return;

        //    var text = File.ReadAllLines(fullPath);

        //    RAMModelMetaDataFilePath = text[0];
        //    RAMModelReactionsFilePath = text[1];
        //    RAMModelStudsFilePath = text[2];
        //    RAMModelCamberFilePath = text[3];

        //}

        private void LoadRAMMetaDataFileHistoryFromDisk()
        {
            string fullPath = GetMetaDataFile(_projectId);

            if (!File.Exists(fullPath))
                return;

            var text = File.ReadAllLines(fullPath);
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i].Split(';')[0] == "Model")
                {
                    RAMModelMetaDataFilePath = text[i].Split(';')[1];
                }
                else if (text[i].Split(';')[0] == "Reaction")
                {
                    RAMModelReactionsFilePath = text[i].Split(';')[1];
                }
                else if (text[i].Split(';')[0] == "Stud")
                {
                    RAMModelStudsFilePath = text[i].Split(';')[1];
                }
                else if (text[i].Split(';')[0] == "Camber")
                {
                    RAMModelCamberFilePath = text[i].Split(';')[1];
                }
                else if (text[i].Split(';')[0] == "DesignCode")
                {
                    DesignCode designCode = (DesignCode)Enum.Parse(typeof(DesignCode), text[i].Split(';')[1]);
                    UserSetDesignCode = designCode;
                }
                else throw new Exception("Need to debug: Error loading data file paths.");
            }
        }

        internal static string GetMetaDataFile(string projectId)
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dir = System.IO.Path.Combine(folder, @"RevitRxnImporter\metadata");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            return System.IO.Path.Combine(dir, string.Format("Project-{0}_metadata.csv", projectId));
        }

        public void ClearSelectedBeamAnnotations(ClearAnnotationsMain clearAnnotationsMain)
        {
            _analyticalModel = ExtractAnalyticalModel.ExtractFromRevitDocument(_document);
            var beamsToReset = new List<Beam>();
            beamsToReset = FilterOutBeamsToResetByInputDataType(clearAnnotationsMain);
            beamsToReset = FilterOutBeamsToResetByLevels(beamsToReset, clearAnnotationsMain);
            ResetBeamsParametersBySelectedAnnotations(clearAnnotationsMain, beamsToReset);
        }

        public List<Beam> FilterOutBeamsToResetByInputDataType(ClearAnnotationsMain clearAnnotationsMain)
        {
            var beamList = new List<Beam>();
            if(clearAnnotationsMain.IsUserInputDataTypePressed && clearAnnotationsMain.IsRAMImportDataTypePressed)
            {
                beamList = Results.ModelBeamList;
            }
            else if(clearAnnotationsMain.IsUserInputDataTypePressed && !clearAnnotationsMain.IsRAMImportDataTypePressed)
            {
                beamList = Results.UnMappedBeamList;
            }
            else if (!clearAnnotationsMain.IsUserInputDataTypePressed && clearAnnotationsMain.IsRAMImportDataTypePressed)
            {
                beamList = Results.MappedRevitBeams;
            }
            return beamList;
        }

        public List<Beam> FilterOutBeamsToResetByLevels(List<Beam> beamsToReset, ClearAnnotationsMain clearAnnotationsMain)
        {
            var beamList = new List<Beam>();
            foreach (var beamToReset in beamsToReset)
            {
                string beamToResetLevel = beamToReset.ElementLevel;
                if(clearAnnotationsMain.RevitLevelNamesSelected.Contains(beamToResetLevel))
                {
                    beamList.Add(beamToReset);
                }
            }
            return beamList;
        }

        public void ResetBeamsParametersBySelectedAnnotations(ClearAnnotationsMain clearAnnotationsMain , List<Beam> beamsToReset)
        {
            var clearBeamAnnoationsTransaction = new Transaction(_document, "Clear Beam Annotations");
            clearBeamAnnoationsTransaction.Start();
            foreach (var beamToReset in beamsToReset)
            {
                ElementId beamId = new ElementId(beamToReset.ElementId);
                var beam = _document.GetElement(beamId) as FamilyInstance;

                if (clearAnnotationsMain.IsReactionsPressed)
                {
                    beam.LookupParameter("Start Reaction - Total").SetValueString("");
                    beam.LookupParameter("End Reaction - Total").SetValueString("");
                }

                if (clearAnnotationsMain.IsStudCountsPressed)
                {
                    beam.LookupParameter("Number of studs").SetValueString("");
                }

                if (clearAnnotationsMain.IsCamberValuesPressed)
                {
                    beam.LookupParameter("Camber Size").SetValueString("");
                }
            }
            clearBeamAnnoationsTransaction.Commit();
        }

        public void ShowClearBeamAnnotationsWindow()
        {
            if(Results==null)
            {
                System.Windows.Forms.MessageBox.Show("No RAM data imported. Import RAM data in order to visualize import results.");
                return;
            }

            ClearAnnotationsMain clearAnnotationsMain = new ClearAnnotationsMain(_analyticalModel.LevelInfo, _view);
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
            // Update Model Beam List.
            var modelBeamList = ModelCompare.FilterOutNonRAMBeamsFromRevitBeamList(_analyticalModel.StructuralMembers.Beams);
            modelBeamList = ModelCompare.FilterRevitBeamListByType(modelBeamList);
            Results.ModelBeamList = modelBeamList;
            resultsVisualizer.ColorMembers(_analyticalModel, annotationToVisualize, Results.MappedRevitBeams, Results.ModelBeamList);
        }

        internal void ShowLevelMappingPane(LevelInfo revitLevelInfo, List<RAMModel.Story> ramStories, List<string> filePaths)
        {
            //LevelMappingView LevelMappingView = new LevelMappingView();
            //LevelMappingViewModel = new LevelMappingViewModel(LevelMappingView, _document, _projectId);
            //LevelMappingView.ViewModel = LevelMappingViewModel;
            LevelMappingViewModel.PopulateRevitLevelsAndRAMFloorLayoutTypesOptions(revitLevelInfo, ramStories);
            LevelMappingViewModel.PopulateLevelMapping(LevelMappingViewModel.LoadMappingHistoryFromDisk());
            LevelMappingView.SetupWindowSize();
            LevelMappingView.Show();
            LevelMappingView.Activate();
        }

        internal void ConfigureLevelMapping()
        {
            GatherRAMFiles();
            RAMModel.ExecutePythonScript(RAMFiles);
            RAMModel _ramModel = RAMModel.DeserializeRAMModel(UserSetDesignCode);

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

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;

namespace RevitReactionImporter
{
    public class LevelMappingViewModel
    {

        private RevitReactionImporterApp _rria;
        private string ProjectId { get; set; }

        //private readonly JsonSerializerSettings jsonSettings;
        private AnalyticalModel _analyticalModel = null;
        private RAMModel _ramModel = null;
        //private ControlInterfaceViewModel ControlInterfaceViewModel { get; set;}
        private LevelMappingView _view = null; // TODO: replace this connection with data binding
        //private LevelMappingView _view { get { return _view; } }
        private Document _document;
        //public AnalyticalModel AnalyticalModel { get { return _analyticalModel; } }
        public AnalyticalModel AnalyticalModel { get; set; }
        public RAMModel RAMModel { get { return _ramModel; } }
        public ObservableCollection<string> RevitLevelNames { get; private set; }
        public Dictionary<int, string> LevelMappingFromUser { get; private set; }
        public bool IsLevelMappingSetByUser { get; set; }
        public bool BeamReactionsImported { get; set; }
        public bool BeamStudCountsImported { get; set; }
        public bool BeamCamberValuesImported { get; set; }
        public bool BeamSizesImported { get; set; }

        public LevelMappingViewModel(LevelMappingView view, Document doc, string projectId, bool beamReactionsImported, bool beamStudCountsImported, bool beamCamberValuesImported, bool beamSizesImported)
        {
            RevitLevelNames = new ObservableCollection<string>();
            //if(_analyticalModel!=null)
            //{
            //    RevitLevelNames = PopulateRevitLevels(_analyticalModel.LevelInfo);

            //}

            //_rria = rria;
            _view = view;
            //_view.ViewModel = this;
            _document = doc;
            ProjectId = projectId;
            LevelMappingFromUser = new Dictionary<int, string>();
            IsLevelMappingSetByUser = false;
            BeamReactionsImported = beamReactionsImported;
            BeamStudCountsImported = beamStudCountsImported;
            BeamCamberValuesImported = beamCamberValuesImported;
            BeamSizesImported = beamSizesImported;

            var mappingHistory = LoadMappingHistoryFromDisk();
            IsLevelMappingSetByUser = mappingHistory.IsLevelMappingSetByUser;
            BeamReactionsImported = mappingHistory.BeamReactionsImported;
            BeamStudCountsImported = mappingHistory.BeamStudCountsImported;
            BeamCamberValuesImported = mappingHistory.BeamCamberValuesImported;
            BeamSizesImported = mappingHistory.BeamSizesImported;
            PopulateLevelMapping(mappingHistory);
        }

        public ObservableCollection<string> PopulateRevitLevelsAndRAMFloorLayoutTypesOptions(LevelInfo revitLevelInfo, List<RAMModel.Story> ramStories)
        {
            var revitLevelNamesString = "";
            _view.RevitLevelNames = new ObservableCollection<string>();

            foreach (var revitLevel in revitLevelInfo.Levels)
            {
                _view.RevitLevelNames.Add(revitLevel.Name);
            }
            var revitLevelNames = _view.RevitLevelNames;
            foreach (string levelName in revitLevelNames)
            {
                revitLevelNamesString += levelName + "\r\n";
            }

            _view.RevitLevelTextBlocks.Children.Clear();
            _view.RevitLevelsComboBoxes.Children.Clear();
            foreach (var elem in revitLevelInfo.Levels)
            {
                CreateRevitLevelEntries(elem.Name);
                CreateRAMLayoutTypeEntries(ramStories);
            }
                return revitLevelNames;
        }


        void CreateRevitLevelEntries(string elemName)
        {
            var dataTemplate = new DataTemplate();
            dataTemplate.DataType = typeof(TextBlock);
            var sp = new FrameworkElementFactory(typeof(StackPanel));
            sp.Name = "Revit Level Listings";
            sp.SetValue(StackPanel.OrientationProperty, System.Windows.Controls.Orientation.Vertical);

            var text = new TextBlock();
            text.SetValue(TextBlock.TextProperty, elemName);
            text.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
            text.SetValue(TextBlock.MarginProperty, new Thickness(5, 14, 5, 14));

            dataTemplate.VisualTree = sp;

            _view.RevitLevelTextBlocks.Children.Add(text);

        }

        void CreateRAMLayoutTypeEntries(List<RAMModel.Story> ramStories)
        {
            var combo = new System.Windows.Controls.ComboBox();
            combo.Items.Add("");

            foreach (var ramFloorLayoutType in ramStories)
            {
                combo.Items.Add(ramFloorLayoutType.LayoutType);

            }

            combo.Padding = new Thickness(1, 5, 1, 5);
            combo.BorderThickness = new Thickness(1);
            combo.Width = 200;
            combo.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
            combo.SetValue(TextBlock.MarginProperty, new Thickness(3, 9, 3, 9));
            _view.RevitLevelsComboBoxes.Children.Add(combo);
        }

        public void PopulateLevelMapping(MappingHistory levelMappingHistory)
        {
            // check if set by user.
            if(IsLevelMappingSetByUser)
            {
                //if yes, then load from history.
                LevelMappingFromUser = levelMappingHistory.LevelMappingFromUser;
                SetValueOfRAMFloorLayoutTypeComboBoxesFromUser();
            }
            else
            {
                //TODO:
                //if not set by user then load from algorithm.
                //if algorithm was able to map then load that.
                // if algorithm could not map, then leave the mapping blank.

                //SetValueOfRAMFloorLayoutTypeComboBoxesFromAlgorithm(filePaths);
            }


        }

        public void SetValueOfRAMFloorLayoutTypeComboBoxesFromUser()
        {
            var analyticalModel = ExtractAnalyticalModel.ExtractFromRevitDocument(_document);
            for (int i = 0; i < _view.RevitLevelTextBlocks.Children.Count; i++)
            {
                var revitLevelStackPanelItem = (System.Windows.Controls.TextBlock)_view.RevitLevelTextBlocks.Children[i];
                string revitLevelName = revitLevelStackPanelItem.Text;
                int revitLevelId = GetRevitLevelIdFromName(revitLevelName, analyticalModel.LevelInfo);
                var ramFloorLayoutTypes = _view.RevitLevelsComboBoxes.Children;
                var ramFloorLayoutTypeComboBox = (System.Windows.Controls.ComboBox)ramFloorLayoutTypes[i];
                string selectedValue = LevelMappingFromUser[revitLevelId];
                ramFloorLayoutTypeComboBox.SelectedValue = selectedValue;
            }
        }

        //public void SetValueOfRAMFloorLayoutTypeComboBoxesFromAlgorithm(List<string> filePaths)
        //{
        //    RAMModel.ExecutePythonScript(filePaths);
        //    RAMModel ramModel = RAMModel.DeserializeRAMModel();
        //    var analyticalModel = ExtractAnalyticalModel.ExtractFromRevitDocument(_document);

        //    ModelCompare.Results results = ModelCompare.CompareModels(ramModel, analyticalModel, LevelMappingFromUser);

        //    for (int i = 0; i < _view.RevitLevelTextBlocks.Children.Count; i++)
        //    {
        //        var revitLevelStackPanelItem = (System.Windows.Controls.TextBlock)_view.RevitLevelTextBlocks.Children[i];
        //        string revitLevelName = revitLevelStackPanelItem.Text;
        //        int revitLevelId = GetRevitLevelIdFromName(revitLevelName, analyticalModel.LevelInfo);
        //        var ramFloorLayoutTypes = _view.RevitLevelsComboBoxes.Children;
        //        var ramFloorLayoutTypeComboBox = (System.Windows.Controls.ComboBox)ramFloorLayoutTypes[i];
        //        string selectedValue = results.LevelMapping[revitLevelId];
        //        ramFloorLayoutTypeComboBox.SelectedValue = selectedValue;
        //    }
        //}

        public void SetLevelMappingFromUser()
        {
            var analyticalModel = ExtractAnalyticalModel.ExtractFromRevitDocument(_document);
            for (int i=0; i < _view.RevitLevelTextBlocks.Children.Count; i++)
            {
                var revitLevelStackPanelItem = (System.Windows.Controls.TextBlock) _view.RevitLevelTextBlocks.Children[i];
                string revitLevelName = revitLevelStackPanelItem.Text;
                int revitLevelId = GetRevitLevelIdFromName(revitLevelName, analyticalModel.LevelInfo);
                var ramFloorLayoutTypes = _view.RevitLevelsComboBoxes.Children;
                var ramFloorLayoutTypeComboBox = (System.Windows.Controls.ComboBox)ramFloorLayoutTypes[i];
                string ramFloorLayoutType = ramFloorLayoutTypeComboBox.Text;
                LevelMappingFromUser[revitLevelId] = ramFloorLayoutType;
            }
            IsLevelMappingSetByUser = true;
            SaveLevelMappingHistoryToDisk();
        }

        int GetRevitLevelIdFromName(string revitLevelName, LevelInfo levelInfo)
        {
            return levelInfo.Levels.Where(i => i.Name == revitLevelName).FirstOrDefault().ElementId;
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
            string fullPath = GetLevelMappingHistoryFile(ProjectId);

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

            return levelMappingHistory;
        }

        public void SaveLevelMappingHistoryToDisk()
        {
            EnsureLevelMappingHistoryDirectoryExists();

            string fullPath = GetLevelMappingHistoryFile(ProjectId);

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






    }
}

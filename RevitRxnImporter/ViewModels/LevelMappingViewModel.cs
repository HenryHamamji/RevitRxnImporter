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
        private string _projectId = "";

        //private readonly JsonSerializerSettings jsonSettings;
        private AnalyticalModel _analyticalModel = null;
        private RAMModel _ramModel = null;

        private LevelMappingView _view = null; // TODO: replace this connection with data binding
        //private LevelMappingView _view { get { return _view; } }
        private Document _document;
        //public AnalyticalModel AnalyticalModel { get { return _analyticalModel; } }
        public AnalyticalModel AnalyticalModel { get; set; }
        public RAMModel RAMModel { get { return _ramModel; } }
        public ObservableCollection<string> RevitLevelNames { get; private set; }
        public Dictionary<int, string> LevelMappingFromUser { get; private set; }
        public bool IsLevelMappingSetByUser { get; set; }

        public LevelMappingViewModel(LevelMappingView view, Document doc,
            RevitReactionImporterApp rria)
        {
            RevitLevelNames = new ObservableCollection<string>();
            //if(_analyticalModel!=null)
            //{
            //    RevitLevelNames = PopulateRevitLevels(_analyticalModel.LevelInfo);

            //}
            _rria = rria;
            _view = view;
            _view.ViewModel = this;
            _document = doc;
            LevelMappingFromUser = new Dictionary<int, string>();
            IsLevelMappingSetByUser = false;
            var mappingHistory = LoadMappingHistoryFromDisk();
            IsLevelMappingSetByUser = mappingHistory.IsLevelMappingSetByUser;
            //PopulateLevelMapping(levelMappingHistory);
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
            //_view.RevitLevelsComboBoxes.ItemsSource = revitLevelNames;
            foreach (string levelName in revitLevelNames)
            {
                revitLevelNamesString += levelName + "\r\n";
            }
            //_view.RevitLevelTextBlocks.Text = revitLevelNamesString;

            foreach (var elem in revitLevelInfo.Levels)
            {
                //var item = new ListBoxItem();
                //item.Padding = new Thickness(1, 8, 1, 8);
                //item.BorderThickness = new Thickness(1);

                //var border = new Border();
                //border.CornerRadius = new CornerRadius(2);
                //border.BorderBrush = Brushes.SteelBlue;
                //border.Child = item;
                //item.Style = 
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
            //sp.AppendChild(text);

            dataTemplate.VisualTree = sp;

            //item.ContentTemplate = dataTemplate;
            //_view.RevitLevelTextBlocks.Items.Add(item);
            _view.RevitLevelTextBlocks.Children.Add(text);

        }

        void CreateRAMLayoutTypeEntries(List<RAMModel.Story> ramStories)
        {
            var combo = new System.Windows.Controls.ComboBox();
            combo.Items.Add("");

            foreach (var ramFloorLayoutType in ramStories)
            {
                combo.Items.Add("  " + ramFloorLayoutType.LayoutType);

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
                //if not set by user then load from algorithm.
                //if algorithm was able to map then load that.
                // if algorithm could not map, then leave the mapping blank.

                SetValueOfRAMFloorLayoutTypeComboBoxesFromAlgorithm();
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

        public void SetValueOfRAMFloorLayoutTypeComboBoxesFromAlgorithm()
        {
            RAMModel.ExecutePythonScript();
            RAMModel ramModel = RAMModel.DeserializeRAMModel();
            var analyticalModel = ExtractAnalyticalModel.ExtractFromRevitDocument(_document);

            ModelCompare.Results results = ModelCompare.CompareModels(ramModel, analyticalModel);

            for (int i = 0; i < _view.RevitLevelTextBlocks.Children.Count; i++)
            {
                var revitLevelStackPanelItem = (System.Windows.Controls.TextBlock)_view.RevitLevelTextBlocks.Children[i];
                string revitLevelName = revitLevelStackPanelItem.Text;
                int revitLevelId = GetRevitLevelIdFromName(revitLevelName, analyticalModel.LevelInfo);
                var ramFloorLayoutTypes = _view.RevitLevelsComboBoxes.Children;
                var ramFloorLayoutTypeComboBox = (System.Windows.Controls.ComboBox)ramFloorLayoutTypes[i];
                string selectedValue = results.LevelMapping[revitLevelId];
                ramFloorLayoutTypeComboBox.SelectedValue = "  " + selectedValue;
            }
        }

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

        private static string GetLevelMappingHistoryFile()
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return System.IO.Path.Combine(folder, @"RAMDataImporter\history.txt");
        }

        public MappingHistory LoadMappingHistoryFromDisk()
        {
            string fullPath = GetLevelMappingHistoryFile();

            if (!File.Exists(fullPath))
                return new MappingHistory(IsLevelMappingSetByUser, LevelMappingFromUser);

            var text = File.ReadAllText(fullPath);

            MappingHistory levelMappingHistory;

            try
            {
                levelMappingHistory = JsonConvert.DeserializeObject<MappingHistory>(text);
            }
            catch
            {
                return new MappingHistory(IsLevelMappingSetByUser, LevelMappingFromUser);
            }

            return levelMappingHistory;
        }

        public void SaveLevelMappingHistoryToDisk()
        {
            EnsureLevelMappingHistoryDirectoryExists();

            string fullPath = GetLevelMappingHistoryFile();

            var history = new MappingHistory(IsLevelMappingSetByUser, LevelMappingFromUser);
            var histJson = JsonConvert.SerializeObject(history, Formatting.Indented);

            System.IO.File.WriteAllText(fullPath, histJson);
        }

        private void EnsureLevelMappingHistoryDirectoryExists()
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dir = System.IO.Path.Combine(folder, "RAMDataImporter");

            if (Directory.Exists(dir))
                return;

            Directory.CreateDirectory(dir);
        }






    }
}

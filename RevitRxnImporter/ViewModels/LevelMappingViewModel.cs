using System;
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
            //if(_analyticalModel!=null)
            //{
            //    RevitLevelNames = PopulateRevitLevels(_analyticalModel.LevelInfo);

            //}
            _rria = rria;
            _view = view;
            _view.ViewModel = this;
            _document = doc;

        }

        public ObservableCollection<string> PopulateRevitLevelsAndRAMFloorLayoutTypes(LevelInfo revitLevelInfo, List<RAMModel.Story> ramStories)
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
                var item = new ListBoxItem();
                //item.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 111, 111, 255));
                //item.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 0, 0));
                item.Padding = new Thickness(1, 8, 1, 8);
                item.BorderThickness = new Thickness(1);

                //var border = new Border();
                //border.CornerRadius = new CornerRadius(2);
                //border.BorderBrush = Brushes.SteelBlue;
                //border.Child = item;
                //item.Style = 
                CreateRevitLevelEntries(item, elem.Name);
                CreateRAMLayoutTypeEntries(ramStories);
            }
                return revitLevelNames;
        }


        void CreateRevitLevelEntries(ListBoxItem item, string elemName)
        {
            var dataTemplate = new DataTemplate();
            dataTemplate.DataType = typeof(TextBlock);
            var sp = new FrameworkElementFactory(typeof(StackPanel));
            sp.Name = "Revit Level Listings";
            sp.SetValue(StackPanel.OrientationProperty, System.Windows.Controls.Orientation.Vertical);

            var text = new FrameworkElementFactory(typeof(TextBlock));
            text.SetValue(TextBlock.TextProperty, elemName);
            text.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
            text.SetValue(TextBlock.MarginProperty, new Thickness(5, 5, 5, 5));
            sp.AppendChild(text);

            dataTemplate.VisualTree = sp;

            item.ContentTemplate = dataTemplate;
            _view.RevitLevelTextBlocks.Items.Add(item);
        }

        void CreateRAMLayoutTypeEntries(List<RAMModel.Story> ramStories)
        {
            var combo = new System.Windows.Controls.ComboBox();
            foreach(var ramFloorLayoutType in ramStories)
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











    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace RevitReactionImporter
{
    /// <summary>
    /// Interaction logic for RevitLevels.xaml
    /// </summary>
    public partial class RevitLevels : UserControl
    {
        public ClearAnnotationsMain ClearAnnotationsMain { get; set; }
        public ObservableCollection<string> RevitLevelNames { get; set; }
        public RevitLevels(ClearAnnotationsMain clearAnnotationsMain)
        {
            ClearAnnotationsMain = clearAnnotationsMain;
            RevitLevelNames = new ObservableCollection<string>();
            InitializeComponent();
            RevitLevelListBoxes.ItemsSource = RevitLevelNames;
            PopulateRevitLevels(clearAnnotationsMain);


        }


        public void PopulateRevitLevels(ClearAnnotationsMain clearAnnotationsMain)
        {
            var revitLevelInfo = clearAnnotationsMain.LevelInfo;
            RevitLevelListBoxes = new ListBox();
            foreach (var revitLevel in revitLevelInfo.Levels)
            {
                RevitLevelNames.Add(revitLevel.Name);
            }
            double multiple = 19.0;
            clearAnnotationsMain.Height = (revitLevelInfo.Levels.Count * multiple) + 20.0;
            this.Height = (revitLevelInfo.Levels.Count * multiple) + 5.0;
            RevitLevelListBoxes.Height = revitLevelInfo.Levels.Count * multiple;
        }








    }
}

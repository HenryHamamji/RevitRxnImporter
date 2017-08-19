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
        public ObservableCollection<string> SelectedRevitLevelNames { get; set; }

        public RevitLevels(ClearAnnotationsMain clearAnnotationsMain)
        {
            ClearAnnotationsMain = clearAnnotationsMain;
            RevitLevelNames = new ObservableCollection<string>();
            InitializeComponent();
            RevitLevelListBoxes.ItemsSource = RevitLevelNames;
            PopulateRevitLevels(clearAnnotationsMain);


        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RevitLevelListBoxes.SelectionMode = SelectionMode.Multiple;
            foreach (string item in e.RemovedItems)
            {
               RevitLevelListBoxes.SelectedItems.Remove(item);
            }

            foreach (string item in e.AddedItems)
            {
                RevitLevelListBoxes.SelectedItems.Add(item);
            }
        }

        public void PopulateRevitLevels(ClearAnnotationsMain clearAnnotationsMain)
        {
            var revitLevelInfo = clearAnnotationsMain.LevelInfo;
            foreach (var revitLevel in revitLevelInfo.Levels)
            {
                RevitLevelNames.Add(revitLevel.Name);

            }
            //double userControlMultiple = 14.0;
            //clearAnnotationsMain.Height = (revitLevelInfo.Levels.Count * multiple) + 25.0;
            //this.Height = (revitLevelInfo.Levels.Count * userControlMultiple) + 5.0;
            // RevitLevelListBoxes.Height = revitLevelInfo.Levels.Count * userControlMultiple;
            clearAnnotationsMain.Height = 280;
            this.Height = 220;
            RevitLevelListBoxes.Height = 180;

        }

        public ObservableCollection<string> GetSelectedRevitLevels()
        {
            var selectedRevitLevels = new ObservableCollection<string>();
            for (int i = 0; i < RevitLevelListBoxes.SelectedItems.Count; i++)
            {
                string listitemcontents_str = RevitLevelListBoxes.SelectedItems[i].ToString();
                selectedRevitLevels.Add(listitemcontents_str);
            }

            return selectedRevitLevels;
        }

        private void OnClearSelectionsClick(object sender, RoutedEventArgs e)
        {
            SelectedRevitLevelNames = GetSelectedRevitLevels();
            if (SelectedRevitLevelNames.Count==0)
            {
                System.Windows.Forms.MessageBox.Show("No levels have been selected. Please choose at least one.");
                return;
            }
            ClearAnnotationsMain.RevitLevelNamesSelected = SelectedRevitLevelNames;
            ClearAnnotationsMain.Close();
            ClearAnnotationsMain.ClearSelectedAnnotations();

        }







    }
}

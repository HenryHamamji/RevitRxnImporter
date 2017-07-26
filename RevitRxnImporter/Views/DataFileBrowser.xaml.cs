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
using System.IO;
using Microsoft.Win32;


namespace RevitReactionImporter
{
    /// <summary>
    /// Interaction logic for DataFileBrowser.xaml
    /// </summary>
    public partial class DataFileBrowser : Window
    {
        public string RAMModelMetaDataFilePath { get; set; }
        public string RAMModelReactionsFilePath { get; set; }

        public DataFileBrowser()
        {
            InitializeComponent();
        }

        private void onBrowseFileClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (openFileDialog.ShowDialog() == true)
            {
                string fileName = openFileDialog.FileName;
                txtEditor.Text = fileName;
                RAMModelMetaDataFilePath = fileName;
            }
        }

        private void WriteRAMMetaDetaFilePathsToFile()
        {
            string[] lines = { "First line", "Second line", "Third line" };
            System.IO.File.WriteAllLines(@"C:\Users\Public\TestFolder\WriteLines.txt", lines);
        }
    }
}

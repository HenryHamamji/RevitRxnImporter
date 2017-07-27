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
using Autodesk.Revit.UI;


namespace RevitReactionImporter
{
    /// <summary>
    /// Interaction logic for DataFileBrowser.xaml
    /// </summary>
    public partial class DataFileBrowser : Window
    {
        private string ProjectId { get; set; }
        public string RAMModelMetaDataFilePath { get; set; }
        public string RAMModelReactionsFilePath { get; set; }
        public string RAMModelStudsFilePath { get; set; }
        public string RAMModelCamberFilePath { get; set; }
        public string RAMModelSizesFilePath { get; set; }
        private ExternalEvent assignDataFilesEvent;


        public DataFileBrowser(string projectId, ControlInterfaceView controlInterfaceView)
        {
            ProjectId = projectId;
            InitializeComponent();

            var assignDataFilesHandler = new AssignDataFilesHandler();
            assignDataFilesHandler.DataFileBrowser = this;
            assignDataFilesHandler.ControlInterfaceView = controlInterfaceView;
            assignDataFilesEvent = ExternalEvent.Create(assignDataFilesHandler);

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
                OnOpenFileDialogIsTrue();
                //WriteRAMMetaDetaFilePathsToFile();
            }
        }

        //todo: remove
        private void WriteRAMMetaDetaFilePathsToFile()
        {
            var path = GetMetaDataFile(ProjectId);
            File.WriteAllText(path, String.Empty);
            using (var stream = new FileStream(path, FileMode.Truncate))
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write("Model: " + RAMModelMetaDataFilePath + Environment.NewLine);
                    writer.Write("Reactions: " + RAMModelReactionsFilePath + Environment.NewLine);

                }
            }
        }

        internal static string GetMetaDataFile(string projectId)
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dir = System.IO.Path.Combine(folder, @"RevitRxnImporter\metadata");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            return System.IO.Path.Combine(dir, string.Format("metadata.txt"));
        }

        private void OnOpenFileDialogIsTrue()
        {
            if (assignDataFilesEvent != null)
                assignDataFilesEvent.Raise();
            else
                MessageBox.Show("AssignDataFilesEvent event handler is null");
        }



    }
}

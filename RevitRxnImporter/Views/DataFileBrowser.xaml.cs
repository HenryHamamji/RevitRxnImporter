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
using Excel = Microsoft.Office.Interop.Excel;

namespace RevitReactionImporter
{
    /// <summary>
    /// Interaction logic for DataFileBrowser.xaml
    /// </summary>
    public partial class DataFileBrowser : System.Windows.Window
    {
        private string ProjectId { get; set; }
        public string RAMModelMetaDataFilePath { get; set; }
        public string RAMModelReactionsFilePath { get; set; }
        public string RAMModelStudsFilePath { get; set; }
        public string RAMModelCamberFilePath { get; set; }
        public string TempButtonName { get; set; }
        private ExternalEvent assignDataFilesEvent;


        public DataFileBrowser(string projectId, ControlInterfaceView controlInterfaceView)
        {
            ProjectId = projectId;
            LoadRAMMetaDataFileHistoryFromDisk();
            InitializeComponent();
            txtEditorRAMModel.Text = RAMModelMetaDataFilePath;
            txtEditorRAMReactions.Text = RAMModelReactionsFilePath;
            txtEditorRAMStuds.Text = RAMModelStudsFilePath;
            txtEditorRAMCamber.Text = RAMModelCamberFilePath;
        
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

                var button = sender as System.Windows.Controls.Button;
                TempButtonName = button.Name;
                if (button.Name == "btnRAMModelFile")
                {
                    if(!CheckIfRAMFileIsCorrect(fileName, "Echo"))
                    {
                        System.Windows.Forms.MessageBox.Show("This file is not the RAM Model File. Please provide the correct file.");
                        return;
                    }
                    txtEditorRAMModel.Text = fileName;
                    RAMModelMetaDataFilePath = fileName;
                    //Settings1.Default.RAMModelFilePathSetting = fileName;
                    //Settings1.Default.Save();
                }
                else if(button.Name == "btnRAMReactionsFile")
                {
                    if (!CheckIfRAMFileIsCorrect(fileName, "Reaction"))
                    {
                        System.Windows.Forms.MessageBox.Show("This file is not the RAM Reactions File. Please provide the correct file.");
                        return;
                    }
                    txtEditorRAMReactions.Text = fileName;
                    RAMModelReactionsFilePath = fileName;
                    //Settings1.Default.RAMReactionsFilePathSetting = fileName;
                    //Settings1.Default.Save();
                }
                else if (button.Name == "btnRAMStudsFile")
                {
                    if (!CheckIfRAMFileIsCorrect(fileName, "Summary"))
                    {
                        System.Windows.Forms.MessageBox.Show("This file is not the RAM Studs File. Please provide the correct file.");
                        return;
                    }
                    txtEditorRAMStuds.Text = fileName;
                    RAMModelStudsFilePath = fileName;
                   // Settings1.Default.RAMStudsFilePathSetting = fileName;
                    //Settings1.Default.Save();
                }
                else if (button.Name == "btnRAMCamberFile")
                {
                    if (!CheckIfRAMFileIsCorrect(fileName, "Deflection"))
                    {
                        System.Windows.Forms.MessageBox.Show("This file is not the RAM Camber File. Please provide the correct file.");
                        return;
                    }
                    txtEditorRAMCamber.Text = fileName;
                    RAMModelCamberFilePath = fileName;
                    //Settings1.Default.RAMCamberFilePathSetting = fileName;
                    //Settings1.Default.Save();
                }

                else
                {
                    throw new Exception("Button name is not recognized");
                }
                WriteRAMMetaDetaFilePathsToFile();
                OnOpenFileDialogIsTrue(button.Name);
            }
        }

        internal bool CheckIfRAMFileIsCorrect(string ramModelFilePath, string fileCategoryIdentifier)
        {
            Excel.Application excel = new Excel.Application();
            Excel.Workbook wb = excel.Workbooks.Open(ramModelFilePath);
            Excel.Worksheet excelSheet = wb.ActiveSheet;
            //Read the first cell
            string fileCategory = excelSheet.Cells[1, 1].Value.ToString();
            wb.Close();
            return fileCategory.Contains(fileCategoryIdentifier);
        }

        private void WriteRAMMetaDetaFilePathsToFile()
        {
            var path = GetMetaDataFile(ProjectId);
            File.WriteAllText(path, String.Empty);
            using (var stream = new FileStream(path, FileMode.Truncate))
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(RAMModelMetaDataFilePath + Environment.NewLine);
                    writer.Write(RAMModelReactionsFilePath + Environment.NewLine);
                    writer.Write(RAMModelStudsFilePath + Environment.NewLine);
                    writer.Write(RAMModelCamberFilePath);

                }
            }
        }

        internal static string GetMetaDataFile(string projectId)
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dir = System.IO.Path.Combine(folder, @"RevitRxnImporter\metadata");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            return System.IO.Path.Combine(dir, string.Format("Project-{0}_metadata.csv", projectId));

            //return System.IO.Path.Combine(dir, string.Format("metadata.txt"));
        }

        public void LoadRAMMetaDataFileHistoryFromDisk()
        {
            string fullPath = GetMetaDataFile(ProjectId);

            if (!File.Exists(fullPath))
                return;

            var text = File.ReadAllLines(fullPath);

            //MappingHistory levelMappingHistory;
            RAMModelMetaDataFilePath = text[0];
            RAMModelReactionsFilePath = text[1];
            RAMModelStudsFilePath = text[2];
            RAMModelCamberFilePath = text[3];

            //return levelMappingHistory;
        }



        private void OnOpenFileDialogIsTrue(string buttonName)
        {
            if (assignDataFilesEvent != null)
                assignDataFilesEvent.Raise();
            else
                MessageBox.Show("AssignDataFilesEvent event handler is null");
        }



    }
}

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
            InitializeComponent();
            txtEditorRAMModel.Text = Settings1.Default.RAMModelFilePathSetting;
            txtEditorRAMReactions.Text = Settings1.Default.RAMReactionsFilePathSetting;
            txtEditorRAMStuds.Text = Settings1.Default.RAMStudsFilePathSetting;
            txtEditorRAMCamber.Text = Settings1.Default.RAMCamberFilePathSetting;
        
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
                    if(!CheckIfRAMModelFileIsCorrect(fileName))
                    {
                        System.Windows.Forms.MessageBox.Show("This file is not a RAM Model File. Please provide the correct file.");
                        return;
                    }
                    txtEditorRAMModel.Text = fileName;
                    RAMModelMetaDataFilePath = fileName;
                    Settings1.Default.RAMModelFilePathSetting = fileName;
                    Settings1.Default.Save();
                }
                else if(button.Name == "btnRAMReactionsFile")
                {
                    txtEditorRAMReactions.Text = fileName;
                    RAMModelReactionsFilePath = fileName;
                    Settings1.Default.RAMReactionsFilePathSetting = fileName;
                    Settings1.Default.Save();
                }
                else if (button.Name == "btnRAMStudsFile")
                {
                    txtEditorRAMStuds.Text = fileName;
                    RAMModelStudsFilePath = fileName;
                    Settings1.Default.RAMStudsFilePathSetting = fileName;
                    Settings1.Default.Save();
                }
                else if (button.Name == "btnRAMCamberFile")
                {
                    txtEditorRAMCamber.Text = fileName;
                    RAMModelCamberFilePath = fileName;
                    Settings1.Default.RAMCamberFilePathSetting = fileName;
                    Settings1.Default.Save();
                }
                else
                {
                    throw new Exception("Button name is not recognized");
                }

                OnOpenFileDialogIsTrue(button.Name);
            }
        }

        internal bool CheckIfRAMModelFileIsCorrect(string ramModelFilePath)
        {
            Excel.Application excel = new Excel.Application();
            Excel.Workbook wb = excel.Workbooks.Open(ramModelFilePath);
            Excel.Worksheet excelSheet = wb.ActiveSheet;
            //Read the first cell
            string fileCategory = excelSheet.Cells[1, 1].Value.ToString();
            wb.Close();
            return fileCategory.Contains("Echo");
        }

        //todo: remove
        //private void WriteRAMMetaDetaFilePathsToFile()
        //{
        //    var path = GetMetaDataFile(ProjectId);
        //    File.WriteAllText(path, String.Empty);
        //    using (var stream = new FileStream(path, FileMode.Truncate))
        //    {
        //        using (var writer = new StreamWriter(stream))
        //        {
        //            writer.Write("Model: " + RAMModelMetaDataFilePath + Environment.NewLine);
        //            writer.Write("Reactions: " + RAMModelReactionsFilePath + Environment.NewLine);

        //        }
        //    }
        //}

        internal static string GetMetaDataFile(string projectId)
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dir = System.IO.Path.Combine(folder, @"RevitRxnImporter\metadata");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            return System.IO.Path.Combine(dir, string.Format("metadata.txt"));
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

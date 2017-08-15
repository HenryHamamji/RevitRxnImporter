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
        public DesignCode UserSetDesignCode {get; set;}


    public DataFileBrowser(string projectId, ControlInterfaceView controlInterfaceView)
        {
            UserSetDesignCode = DesignCode.LRFD;
            ProjectId = projectId;
            LoadRAMMetaDataFileHistoryFromDisk();
            InitializeComponent();
            txtEditorRAMModel.Text = RAMModelMetaDataFilePath;
            txtEditorRAMReactions.Text = RAMModelReactionsFilePath;
            txtEditorRAMStuds.Text = RAMModelStudsFilePath;
            txtEditorRAMCamber.Text = RAMModelCamberFilePath;
            btnDesignCode.Content = UserSetDesignCode;

            var assignDataFilesHandler = new AssignDataFilesHandler();
            assignDataFilesHandler.DataFileBrowser = this;
            assignDataFilesHandler.ControlInterfaceView = controlInterfaceView;
            assignDataFilesEvent = ExternalEvent.Create(assignDataFilesHandler);

        }
        private void onChangeDesignCodeClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            string designCode = button.Content.ToString();
            if(designCode == "LRFD")
            {
                button.Content = "ASD";
                UserSetDesignCode = DesignCode.ASD;
                WriteRAMMetaDetaFilePathsToFile();
            }
            else if (designCode == "ASD")
            {
                button.Content = "LRFD";
                UserSetDesignCode = DesignCode.LRFD;
                WriteRAMMetaDetaFilePathsToFile();
            }
            else
            {
                throw new Exception("Need to debug: Invalid Design Code Set By User");
            }
        }

        private void ClearTextOnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var textBox = sender as System.Windows.Controls.TextBox;
            if (Keyboard.IsKeyDown(Key.Back) || Keyboard.IsKeyDown(Key.Delete))
            {
                textBox.Text = "";
            }
            if (textBox.Name == "txtEditorRAMModel")
            {
                RAMModelCamberFilePath = textBox.Text;
            }
            else if (textBox.Name == "txtEditorRAMReactions")
            {
                RAMModelCamberFilePath = textBox.Text;
            }
            else if (textBox.Name == "txtEditorRAMStuds")
            {
                RAMModelCamberFilePath = textBox.Text;
            }
            else if (textBox.Name == "txtEditorRAMCamber")
            {
                RAMModelCamberFilePath = textBox.Text;
            }
            else throw new Exception("Need to debug: Could not find TextBox Name to Clear.");
            WriteRAMMetaDetaFilePathsToFile();
        }

        private void onBrowseFileClick(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            TempButtonName = button.Name;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if(button.Name == "btnRAMStudsFile" || button.Name == "btnRAMCamberFile")
            {
                openFileDialog.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";

            }
            else
            {
                openFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
            }
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (openFileDialog.ShowDialog() == true)
            {
                string fileName = openFileDialog.FileName;


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
                    if (!CheckIfRAMFileIsCorrect(fileName, "Beam Summary"))
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
                    writer.Write("Model;" + RAMModelMetaDataFilePath + Environment.NewLine);
                    writer.Write("Reaction;" + RAMModelReactionsFilePath + Environment.NewLine);
                    writer.Write("Stud;" + RAMModelStudsFilePath + Environment.NewLine);
                    writer.Write("Camber;" + RAMModelCamberFilePath + Environment.NewLine);
                    writer.Write("DesignCode;" + UserSetDesignCode.ToString());

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

        private void LoadRAMMetaDataFileHistoryFromDisk()
        {
            string fullPath = GetMetaDataFile(ProjectId);

            if (!File.Exists(fullPath))
                return;

            var text = File.ReadAllLines(fullPath);
            for(int i=0; i < text.Length; i++)
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
                   DesignCode designCode = (DesignCode) Enum.Parse(typeof(DesignCode), text[i].Split(';')[1]);
                    UserSetDesignCode = designCode;
                }
                else throw new Exception("Need to debug: Error loading data file paths.");
            }
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

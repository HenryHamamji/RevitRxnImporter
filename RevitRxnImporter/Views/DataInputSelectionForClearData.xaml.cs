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
using Autodesk.Revit.UI;

namespace RevitReactionImporter
{

    public partial class DataInputSelectionForClearData : UserControl
    {
        public bool IsRAMImportDataTypePressed { get; set; }
        public bool IsUserInputDataTypePressed { get; set; }
        public ClearAnnotationsMain ClearAnnotationsMain { get; set; }

        public DataInputSelectionForClearData(ClearAnnotationsMain clearAnnotationsMain)
        {
            IsRAMImportDataTypePressed = false;
            IsUserInputDataTypePressed = false;
            ClearAnnotationsMain = clearAnnotationsMain;
            InitializeComponent();
        }

        private void OnRAMImportDataTypeClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (!IsRAMImportDataTypePressed)
            {
                IsRAMImportDataTypePressed = true;
                ClearAnnotationsMain.IsRAMImportDataTypePressed = true;
                button.Background = Brushes.LightSteelBlue;
                Keyboard.ClearFocus();
            }
            else
            {
                IsRAMImportDataTypePressed = false;
                ClearAnnotationsMain.IsRAMImportDataTypePressed = false;
                button.ClearValue(Control.BackgroundProperty);
                Keyboard.ClearFocus();
            }
        }

        private void OnUserInputDataTypeClick(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            if (!IsUserInputDataTypePressed)
            {
                IsUserInputDataTypePressed = true;
                ClearAnnotationsMain.IsUserInputDataTypePressed = true;
                button.Background = Brushes.LightSteelBlue;
                Keyboard.ClearFocus();
            }
            else
            {
                IsUserInputDataTypePressed = false;
                ClearAnnotationsMain.IsUserInputDataTypePressed = false;
                button.ClearValue(Control.BackgroundProperty);
                Keyboard.ClearFocus();
            }
        }

        private void OnSelectDataTypesToClearClick(object sender, RoutedEventArgs e)
        {
            if (!ClearAnnotationsMain.IsUserInputDataTypePressed && !ClearAnnotationsMain.IsRAMImportDataTypePressed)
            {
                System.Windows.Forms.MessageBox.Show("No input data type is selected. Please choose at least one.");
                return;
            }
            ClearAnnotationsMain.ContentHolder.Content = new BeamAnnotationToClear(ClearAnnotationsMain);
        }

    }
}

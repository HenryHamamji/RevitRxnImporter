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
using Autodesk.Revit.UI;
namespace RevitReactionImporter
{
    public partial class ClearAnnotationsMain : Window
    {
        public LevelInfo LevelInfo { get; set;}
        public bool IsRAMImportDataTypePressed { get; set; }
        public bool IsUserInputDataTypePressed { get; set; }
        public bool IsReactionsPressed { get; set; }
        public bool IsStudCountsPressed { get; set; }
        public bool IsCamberValuesPressed { get; set; }
        public ObservableCollection<string> RevitLevelNamesSelected { get; set; }
        private ControlInterfaceView ControlInterfaceView { get; set; }
        private ExternalEvent clearDataEvent;

        public ClearAnnotationsMain(LevelInfo levelInfo, ControlInterfaceView view)
        {
            ControlInterfaceView = view;
            LevelInfo = levelInfo;
            InitializeComponent();
            this.ContentHolder.Content = new DataInputSelectionForClearData(this);

            var clearDataHandler = new ClearDataHandler();
            clearDataHandler.View = view;
            clearDataHandler.ClearAnnotationsMain = this;
            clearDataEvent = ExternalEvent.Create(clearDataHandler);
        }

        public void ClearSelectedAnnotations()
        {
            if (clearDataEvent != null)
                clearDataEvent.Raise();
            else
                MessageBox.Show("clearDataEvent event handler is null");
        }
    }
}

using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;

using Autodesk.Revit.UI;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace RevitReactionImporter
{
    public partial class ControlInterfaceView : Page, IDockablePaneProvider
    {

        internal ControlInterfaceViewModel ViewModel { get; set; }

        internal DockablePaneProviderData _paneProviderData = null;

        private ExternalEvent analyzeEvent;
        private ExternalEvent reportsEvent;

        internal ObservableCollection<string> ConnectionTypes = new ObservableCollection<string>();

        public string SelectedConnectionType { get; set; }

        public ControlInterfaceView()
        {
            InitializeComponent();

            ViewModel = null;

            //var analyzeHander = new AnalyzeHandler();
            //analyzeHander.View = this;
            //analyzeEvent = ExternalEvent.Create(analyzeHander);

            //var estimateHander = new EstimateHandler();
            //estimateHander.View = this;
            //estimateEvent = ExternalEvent.Create(estimateHander);
        }

        public void SetupDockablePane(DockablePaneProviderData data)
        {
            data.FrameworkElement = this;

            data.InitialState.SetFloatingRectangle(new Rectangle(200, 150, 580, 730));
            data.InitialState.DockPosition = DockPosition.Floating;

            data.FrameworkElement.MaxHeight = 300;
            data.FrameworkElement.MaxWidth = 250;

            _paneProviderData = data;
        }

        private void OnImportBeamReactionsClick(object sender, RoutedEventArgs e)
        {
            if (analyzeEvent != null)
                analyzeEvent.Raise();
            else
                MessageBox.Show("AnalyzeEvent event handler is null");
        }

        private void OnResetBeamReactionsClick(object sender, RoutedEventArgs e)
        {
            if (reportsEvent != null)
                reportsEvent.Raise();
            else
                MessageBox.Show("ReportsEvent event handler is null");
        }

    }

    public enum ButtonType
    {
        Analyze,
        Estimate
    }
}

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

        private ExternalEvent importRAMReactionsEvent;
        private ExternalEvent clearReactionsEvent;
        private ExternalEvent configureEvent;
        internal ObservableCollection<string> ConnectionTypes = new ObservableCollection<string>();

        public string SelectedConnectionType { get; set; }

        public ControlInterfaceView()
        {
            InitializeComponent();

            ViewModel = null;

            var importRAMReactionsHandler = new ImportRAMReactionsHandler();
            importRAMReactionsHandler.View = this;
            importRAMReactionsEvent = ExternalEvent.Create(importRAMReactionsHandler);

            var clearReactionsHandler = new ClearReactionsHandler();
            clearReactionsHandler.View = this;
            clearReactionsEvent = ExternalEvent.Create(clearReactionsHandler);

            var configureHandler = new ConfigureHandler();
            configureHandler.ControlInterfaceView = this;
            configureEvent = ExternalEvent.Create(configureHandler);
        }

        public void SetupDockablePane(DockablePaneProviderData data)
        {
            data.FrameworkElement = this;

            data.InitialState.SetFloatingRectangle(new Rectangle(200, 150, 500, 350));
            data.InitialState.DockPosition = DockPosition.Floating;

            data.FrameworkElement.MaxHeight = 200;
            data.FrameworkElement.MaxWidth = 300;

            _paneProviderData = data;
        }

        private void OnImportBeamReactionsClick(object sender, RoutedEventArgs e)
        {
            if (importRAMReactionsEvent != null)
                importRAMReactionsEvent.Raise();
            else
                MessageBox.Show("ImportRAMReactionsEvent event handler is null");
        }

        private void OnResetBeamReactionsClick(object sender, RoutedEventArgs e)
        {
            if (clearReactionsEvent != null)
                clearReactionsEvent.Raise();
            else
                MessageBox.Show("ClearReactionsEvent event handler is null");
        }

        private void OnConfigureClick(object sender, RoutedEventArgs e)
        {
            if (configureEvent != null)
                configureEvent.Raise();
            else
                MessageBox.Show("ConfigureEvent event handler is null");
        }
    }

    public enum ButtonType
    {
        Analyze,
        Estimate
    }

}

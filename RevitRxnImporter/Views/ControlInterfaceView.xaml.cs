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
        private ExternalEvent importRAMStudsEvent;
        private ExternalEvent importRAMCamberEvent;
        private ExternalEvent importRAMBeamSizesEvent;

        private ExternalEvent configureEvent;
        private ExternalEvent showDataFilesBrowserEvent;
        private ExternalEvent showAnnotationSelectionToVisualizeBrowserEvent;
        private ExternalEvent resetVisualizationEvent;
        private ExternalEvent showClearBeamAnnotationsWindowEvent;

        internal ObservableCollection<string> ConnectionTypes = new ObservableCollection<string>();

        public string SelectedConnectionType { get; set; }

        public ControlInterfaceView()
        {
            InitializeComponent();

            ViewModel = null;

            var importRAMReactionsHandler = new ImportRAMReactionsHandler();
            importRAMReactionsHandler.View = this;
            importRAMReactionsEvent = ExternalEvent.Create(importRAMReactionsHandler);

            var importRAMStudsHandler = new ImportRAMStudsHandler();
            importRAMStudsHandler.View = this;
            importRAMStudsEvent = ExternalEvent.Create(importRAMStudsHandler);

            var importRAMCamberHandler = new ImportRAMCamberHandler();
            importRAMCamberHandler.View = this;
            importRAMCamberEvent = ExternalEvent.Create(importRAMCamberHandler);

            var importRAMBeamSizingHandler = new ImportRAMBeamSizingHandler();
            importRAMBeamSizingHandler.View = this;
            importRAMBeamSizesEvent = ExternalEvent.Create(importRAMBeamSizingHandler);

            var configureHandler = new ConfigureHandler();
            configureHandler.ControlInterfaceView = this;
            configureEvent = ExternalEvent.Create(configureHandler);

            var showDataFilesBrowserHandler = new ShowDataFilesBrowserHandler();
            showDataFilesBrowserHandler.ControlInterfaceView = this;
            showDataFilesBrowserEvent = ExternalEvent.Create(showDataFilesBrowserHandler);

            var showAnnotationSelectionToVisualizeBrowser = new ShowAnnotationSelectionToVisualizeBrowser();
            showAnnotationSelectionToVisualizeBrowser.ControlInterfaceView = this;
            showAnnotationSelectionToVisualizeBrowserEvent = ExternalEvent.Create(showAnnotationSelectionToVisualizeBrowser);

            var resetVisualizationHandler = new ResetVisualizationHandler();
            resetVisualizationHandler.View = this;
            resetVisualizationEvent = ExternalEvent.Create(resetVisualizationHandler);

            var showClearBeamAnnotationsWindowHandler = new ShowClearBeamAnnotationsWindowHandler();
            showClearBeamAnnotationsWindowHandler.ControlInterfaceView = this;
            showClearBeamAnnotationsWindowEvent = ExternalEvent.Create(showClearBeamAnnotationsWindowHandler);
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

        private void OnImportBeamStudsClick(object sender, RoutedEventArgs e)
        {
            if (importRAMStudsEvent != null)
                importRAMStudsEvent.Raise();
            else
                MessageBox.Show("ImportRAMStudssEvent event handler is null");
        }

        private void OnImportBeamCamberClick(object sender, RoutedEventArgs e)
        {
            if (importRAMCamberEvent != null)
                importRAMCamberEvent.Raise();
            else
                MessageBox.Show("ImportRAMCamberEvent event handler is null");
        }


        private void OnConfigureClick(object sender, RoutedEventArgs e)
        {
            if (configureEvent != null)
                configureEvent.Raise();
            else
                MessageBox.Show("ConfigureEvent event handler is null");
        }

        private void OnShowDataFilesBrowserClick(object sender, RoutedEventArgs e)
        {
            if (showDataFilesBrowserEvent != null)
                showDataFilesBrowserEvent.Raise();
            else
                MessageBox.Show("ShowDataFilesBrowserEvent event handler is null");
        }

        private void OnImportBeamSizesClick(object sender, RoutedEventArgs e)
        {
            if (importRAMBeamSizesEvent != null)
                importRAMBeamSizesEvent.Raise();
            else
                MessageBox.Show("ImportRAMBeamSizesEvent event handler is null");
        }

        private void OnVisualizeClick(object sender, RoutedEventArgs e)
        {
            if (showAnnotationSelectionToVisualizeBrowserEvent != null)
                showAnnotationSelectionToVisualizeBrowserEvent.Raise();
            else
                MessageBox.Show("ShowAnnotationSelectionToVisualizeBrowserEvent event handler is null");
        }

        private void OnResetVisualizationClick(object sender, RoutedEventArgs e)
        {
            if (resetVisualizationEvent != null)
                resetVisualizationEvent.Raise();
            else
                MessageBox.Show("resetVisualizationEvent event handler is null");
        }

        private void OnShowClearBeamAnnotationsWindowClick(object sender, RoutedEventArgs e)
        {
            if (showClearBeamAnnotationsWindowEvent != null)
                showClearBeamAnnotationsWindowEvent.Raise();
            else
                MessageBox.Show("showClearBeamAnnotationsWindowEvent event handler is null");
        }

        private void OnClearDataClick(object sender, RoutedEventArgs e)
        {
            ViewModel.ShowClearBeamAnnotationsWindow();
        }


    }

}

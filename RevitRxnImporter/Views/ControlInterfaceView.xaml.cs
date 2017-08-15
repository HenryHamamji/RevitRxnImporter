using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Media;
using Autodesk.Revit.UI;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace RevitReactionImporter
{
    public partial class ControlInterfaceView : Page, IDockablePaneProvider
    {

        public bool IsSingleImportPressed { get; set; }
        public bool IsMultipleImportPressed { get; set; }

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
            IsMultipleImportPressed = false;
            IsSingleImportPressed = false;
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

            data.InitialState.SetFloatingRectangle(new Rectangle(200, 150, 620, 510));
            data.InitialState.DockPosition = DockPosition.Floating;
            data.FrameworkElement.ClipToBounds = true;
            data.FrameworkElement.MaxHeight = 360;
            data.FrameworkElement.MaxWidth = 420;
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

        private void OnSingleImportClick(object sender, RoutedEventArgs e)
        {
            SingleImportButtonIsPressed(sender, e);
        }

        private void OnMultipleImportClick(object sender, RoutedEventArgs e)
        {
            MultipleImportButtonIsPressed(sender, e);
        }

        private void SingleImportButtonIsPressed(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            if (!IsSingleImportPressed)
            {
                IsSingleImportPressed = true;
                ViewModel.IsSingleImportPressed = true;
                IsMultipleImportPressed = false;
                ViewModel.IsMultipleImportPressed = false;
                button.Background = Brushes.LightSteelBlue;
                Keyboard.ClearFocus();
                MultipleImport.ClearValue(Control.BackgroundProperty);
                Keyboard.ClearFocus();
            }
            else
            {

            }
        }

        private void MultipleImportButtonIsPressed(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            if (!IsMultipleImportPressed)
            {
                IsMultipleImportPressed = true;
                ViewModel.IsMultipleImportPressed = true;
                IsSingleImportPressed = false;
                ViewModel.IsSingleImportPressed = false;
                button.Background = Brushes.LightSteelBlue;
                Keyboard.ClearFocus();
                SingleImport.ClearValue(Control.BackgroundProperty);
                Keyboard.ClearFocus();
            }
            else
            {

            }
        }

    }

}

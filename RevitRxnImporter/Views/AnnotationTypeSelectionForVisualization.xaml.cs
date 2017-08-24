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
    public partial class AnnotationTypeSelectionForVisualization : System.Windows.Window
    {
        public string AnnotationToVisualize { get; set; }
        private ExternalEvent annotationTypeForVisualizationSelected;
        private ExternalEvent visualizationTaskEnded;
        public bool IsReactionsPressed { get; set; }
        public bool IsStudCountsPressed { get; set; }
        public bool IsCamberValuesPressed { get; set; }
        public bool IsBeamSizesPressed { get; set; }
        public ControlInterfaceView ControlInterfaceView { get; set; }


        public AnnotationTypeSelectionForVisualization(ControlInterfaceView controlInterfaceView, ResultsVisualizer.ParameterUpdater parameterUpdater)
        {
            InitializeComponent();

            var selectAnnotationToVisualizeHandler = new SelectAnnotationToVisualizeHandler();
            selectAnnotationToVisualizeHandler.AnnotationTypeSelectionForVisualization = this;
            selectAnnotationToVisualizeHandler.ControlInterfaceView = controlInterfaceView;
            annotationTypeForVisualizationSelected = ExternalEvent.Create(selectAnnotationToVisualizeHandler);

            var visualizationTaskEndedHandler = new VisualizationTaskEndedHandler();
            visualizationTaskEndedHandler._parameterUpdater = parameterUpdater;
            visualizationTaskEnded = ExternalEvent.Create(visualizationTaskEndedHandler);
            IsReactionsPressed = false;
            IsStudCountsPressed = false;
            IsCamberValuesPressed = false;
            IsBeamSizesPressed = false;
            ControlInterfaceView = controlInterfaceView;
        }

        public void OnAnnotationToVisualizeClick(object sender, RoutedEventArgs e)
        {
            ControlInterfaceView.ViewModel.LoadVisualizationHistoryFromDisk();
            var button = sender as System.Windows.Controls.Button;
            UpdatePressed(button);
            AnnotationToVisualize = button.Name;
            if (annotationTypeForVisualizationSelected != null)
            {
                annotationTypeForVisualizationSelected.Raise();
            }
            else
                MessageBox.Show("AnnotationTypeForVisualizationSelectedEvent event handler is null");
        }

        
        public void AnnotationToVisualizeWindowClosed(object sender, RoutedEventArgs e)
        {
            return;
        }

        private void OnVisualizationTaskEnded(object sender, EventArgs e)
        {
            if (visualizationTaskEnded != null)
            {
                visualizationTaskEnded.Raise();
            }
            else
                MessageBox.Show("VisualizationTaskEndedEvent event handler is null");
        }

        private void UpdatePressed(Button button)
        {
            if (button.Name == "VisualizeRAMReactions")
            {
                IsReactionsPressed = true;
                IsStudCountsPressed = false;
                IsCamberValuesPressed = false;
                IsBeamSizesPressed = false;
                VisualizeRAMSizes.ClearValue(Control.BackgroundProperty);
                VisualizeRAMStuds.ClearValue(Control.BackgroundProperty);
                VisualizeRAMCamber.ClearValue(Control.BackgroundProperty);
                button.Background = Brushes.LightSteelBlue;
                Keyboard.ClearFocus();
            }
            if (button.Name == "VisualizeRAMSizes")
            {
                IsReactionsPressed = false;
                IsStudCountsPressed = false;
                IsCamberValuesPressed = false;
                IsBeamSizesPressed = true;
                VisualizeRAMReactions.ClearValue(Control.BackgroundProperty);
                VisualizeRAMStuds.ClearValue(Control.BackgroundProperty);
                VisualizeRAMCamber.ClearValue(Control.BackgroundProperty);
                button.Background = Brushes.LightSteelBlue;
                Keyboard.ClearFocus();
            }
            if (button.Name == "VisualizeRAMStuds")
            {
                IsReactionsPressed = false;
                IsStudCountsPressed = true;
                IsCamberValuesPressed = false;
                IsBeamSizesPressed = false;
                VisualizeRAMReactions.ClearValue(Control.BackgroundProperty);
                VisualizeRAMSizes.ClearValue(Control.BackgroundProperty);
                VisualizeRAMCamber.ClearValue(Control.BackgroundProperty);
                button.Background = Brushes.LightSteelBlue;
                Keyboard.ClearFocus();
            }
            if (button.Name == "VisualizeRAMCamber")
            {
                IsReactionsPressed = false;
                IsStudCountsPressed = false;
                IsCamberValuesPressed = true;
                IsBeamSizesPressed = false;
                VisualizeRAMReactions.ClearValue(Control.BackgroundProperty);
                VisualizeRAMSizes.ClearValue(Control.BackgroundProperty);
                VisualizeRAMStuds.ClearValue(Control.BackgroundProperty);
                button.Background = Brushes.LightSteelBlue;
                Keyboard.ClearFocus();
            }

        }
    }
}

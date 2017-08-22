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

        public AnnotationTypeSelectionForVisualization(ControlInterfaceView controlInterfaceView, ResultsVisualizer.ParameterUpdater parameterUpdater)
        {
            InitializeComponent();

            var selectAnnotationToVisualizeHandler = new SelectAnnotationToVisualizeHandler();
            selectAnnotationToVisualizeHandler.AnnotationTypeSelectionForVisualization = this;
            selectAnnotationToVisualizeHandler.ControlInterfaceView = controlInterfaceView;
            annotationTypeForVisualizationSelected = ExternalEvent.Create(selectAnnotationToVisualizeHandler);

            var visualizationTaskEndedHandler = new VisualizationTaskEndedHandler();
            //visualizationTaskEndedHandler.AnnotationTypeSelectionForVisualization = this;
            visualizationTaskEndedHandler._parameterUpdater = parameterUpdater;
            visualizationTaskEnded = ExternalEvent.Create(visualizationTaskEndedHandler);
        }

        public void OnAnnotationToVisualizeClick(object sender, RoutedEventArgs e)
        {
            OnVisualizationTaskEnded(sender, e);

            var button = sender as System.Windows.Controls.Button;
            AnnotationToVisualize = button.Name;
            if (annotationTypeForVisualizationSelected != null)
            {
                //Close();
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
    }
}

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

        public AnnotationTypeSelectionForVisualization(ControlInterfaceView controlInterfaceView)
        {
            InitializeComponent();

            var selectAnnotationToVisualizeHandler = new SelectAnnotationToVisualizeHandler();
            selectAnnotationToVisualizeHandler.AnnotationTypeSelectionForVisualization = this;
            selectAnnotationToVisualizeHandler.ControlInterfaceView = controlInterfaceView;
            annotationTypeForVisualizationSelected = ExternalEvent.Create(selectAnnotationToVisualizeHandler);
        }

        public void OnAnnotationToVisualizeClick(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            AnnotationToVisualize = button.Name;
            if (annotationTypeForVisualizationSelected != null)
            {
                Close();
                annotationTypeForVisualizationSelected.Raise();
            }
            else
                MessageBox.Show("AnnotationTypeForVisualizationSelectedEvent event handler is null");
        }




    }
}

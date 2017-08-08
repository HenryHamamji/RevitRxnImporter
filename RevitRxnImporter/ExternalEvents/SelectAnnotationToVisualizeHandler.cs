using Autodesk.Revit.UI;

namespace RevitReactionImporter
{
    class SelectAnnotationToVisualizeHandler : BaseIdleHandler
    {
        public ControlInterfaceView ControlInterfaceView { get; set; }
        public AnnotationTypeSelectionForVisualization AnnotationTypeSelectionForVisualization { get; set; }

        public override void Run()
        {
            if (ControlInterfaceView.ViewModel == null)
                return;
            ControlInterfaceView.ViewModel.VisualizeData(AnnotationTypeSelectionForVisualization.AnnotationToVisualize);
            

        }

        public override string GetName()
        {
            return "AnnotationTypeSelectionForVisualization";
        }
    }
}




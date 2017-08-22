using Autodesk.Revit.UI;

namespace RevitReactionImporter
{
    class VisualizationTaskEndedHandler : BaseIdleHandler
    {
        //public AnnotationTypeSelectionForVisualization AnnotationTypeSelectionForVisualization { get; set; }
        public ResultsVisualizer.ParameterUpdater _parameterUpdater { get; set; }

        public override void Run()
        {
            //if (AnnotationTypeSelectionForVisualization == null)
            //    return;
            if (_parameterUpdater == null)
                return;

            _parameterUpdater.StopTracking();
        }

        public override string GetName()
        {
            return "VisualizationTaskEndedHandler";
        }
    }
}




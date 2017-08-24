using Autodesk.Revit.UI;

namespace RevitReactionImporter
{
    class VisualizationTaskEndedHandler : BaseIdleHandler
    {
        public ResultsVisualizer.ParameterUpdater _parameterUpdater { get; set; }

        public override void Run()
        {
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




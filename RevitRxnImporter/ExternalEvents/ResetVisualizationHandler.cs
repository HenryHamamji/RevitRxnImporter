using Autodesk.Revit.UI;

namespace RevitReactionImporter
{
    class ResetVisualizationHandler : BaseIdleHandler
    {
        public ControlInterfaceView View { get; set; }

        public override void Run()
        {
            if (View.ViewModel == null)
                return;

            View.ViewModel.ResetVisualization();
        }

        public override string GetName()
        {
            return "ResetVisualizationHandler";
        }
    }
}

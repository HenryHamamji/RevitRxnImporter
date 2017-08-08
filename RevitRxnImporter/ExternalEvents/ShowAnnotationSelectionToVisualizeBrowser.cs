using Autodesk.Revit.UI;

namespace RevitReactionImporter
{
    class ShowAnnotationSelectionToVisualizeBrowser : BaseIdleHandler
    {
        public ControlInterfaceView ControlInterfaceView { get; set; }
        public override void Run()
        {
            if (ControlInterfaceView.ViewModel == null)
                return;

            ControlInterfaceView.ViewModel.ShowSelectAnnotationToVisualizeWindow();
        }

        public override string GetName()
        {
            return "ShowAnnotationSelectionToVisualizeBrowser";
        }
    }
}




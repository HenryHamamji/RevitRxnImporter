using Autodesk.Revit.UI;

namespace RevitReactionImporter
{
    class ClearDataHandler : BaseIdleHandler
    {
        public ControlInterfaceView View { get; set; }
        public ClearAnnotationsMain ClearAnnotationsMain { get; set; }

        public override void Run()
        {
            if (View.ViewModel == null)
                return;
            if (ClearAnnotationsMain == null)
                return;

            View.ViewModel.ClearSelectedBeamAnnotations(ClearAnnotationsMain);
        }

        public override string GetName()
        {
            return "ClearDataHandler";
        }
    }
}

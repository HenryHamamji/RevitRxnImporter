using Autodesk.Revit.UI;

namespace RevitReactionImporter
{
    class ImportRAMBeamSizingHandler : BaseIdleHandler
    {
        public ControlInterfaceView View { get; set; }

        public override void Run()
        {
            if (View.ViewModel == null)
                return;

            View.ViewModel.ImportBeamSizes();
        }

        public override string GetName()
        {
            return "ImportRAMBeamSizingHandler";
        }
    }
}




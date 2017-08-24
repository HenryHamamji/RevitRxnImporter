using Autodesk.Revit.UI;

namespace RevitReactionImporter
{
    class ImportRAMCamberHandler : BaseIdleHandler
    {
        public ControlInterfaceView View { get; set; }

        public override void Run()
        {
            if (View.ViewModel == null)
                return;

            View.ViewModel.ImportCamberValues();
        }

        public override string GetName()
        {
            return "ImportRAMCamberHandler";
        }
    }
}




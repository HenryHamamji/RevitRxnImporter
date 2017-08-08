using Autodesk.Revit.UI;

namespace RevitReactionImporter
{
    class ClearDataHandler : BaseIdleHandler
    {
        public ControlInterfaceView View { get; set; }

        public override void Run()
        {
            if (View.ViewModel == null)
                return;

            View.ViewModel.ClearBeamData();
        }

        public override string GetName()
        {
            return "ClearDataHandler";
        }
    }
}

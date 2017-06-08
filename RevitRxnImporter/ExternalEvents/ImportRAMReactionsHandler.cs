using Autodesk.Revit.UI;

namespace RevitReactionImporter
{
    class ImportRAMReactionsHandler : BaseIdleHandler
    {
        public ControlInterfaceView View { get; set; }

        public override void Run()
        {
            if (View.ViewModel == null)
                return;

            View.ViewModel.ImportBeamReactions();
        }

        public override string GetName()
        {
            return "ImportRAMReactionsHandler";
        }
    }
}




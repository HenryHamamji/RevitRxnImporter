using Autodesk.Revit.UI;

namespace RevitReactionImporter
{
    class ClearReactionsHandler : BaseIdleHandler
    {
        public ControlInterfaceView View { get; set; }

        public override void Run()
        {
            if (View.ViewModel == null)
                return;

            View.ViewModel.ResetBeamReactions();
        }

        public override string GetName()
        {
            return "ClearReactionsHandler";
        }
    }
}

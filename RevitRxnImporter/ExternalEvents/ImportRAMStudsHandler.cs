using Autodesk.Revit.UI;

namespace RevitReactionImporter
{
    class ImportRAMStudsHandler : BaseIdleHandler
    {
        public ControlInterfaceView View { get; set; }

        public override void Run()
        {
            if (View.ViewModel == null)
                return;

            View.ViewModel.ImportStudCounts(View.ViewModel.RAMModelStudsFilePath);
        }

        public override string GetName()
        {
            return "ImportRAMStudsHandler";
        }
    }
}




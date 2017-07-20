using Autodesk.Revit.UI;

namespace RevitReactionImporter
{
    class SetLevelMappingFromUserHandler : BaseIdleHandler
    {
        public LevelMappingView View { get; set; }

        public override void Run()
        {
            if (View.ViewModel == null)
                return;

            View.ViewModel.SetLevelMappingFromUser();
        }

        public override string GetName()
        {
            return "SetLevelMappingFromUserHandler";
        }
    }
}




using Autodesk.Revit.UI;

namespace RevitReactionImporter
{
    class ConfigureHandler : BaseIdleHandler
    {
        public LevelMappingView LevelMappingView { get; set; }
        public ControlInterfaceView ControlInterfaceView { get; set;}
        public override void Run()
        {
            if (ControlInterfaceView.ViewModel == null)
                return;

            ControlInterfaceView.ViewModel.ConfigureOptions();
        }

        public override string GetName()
        {
            return "ConfigureHandler";
        }
    }
}




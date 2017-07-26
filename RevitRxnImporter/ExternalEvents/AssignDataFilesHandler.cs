using Autodesk.Revit.UI;

namespace RevitReactionImporter
{
    class AssignDataFilesHandler : BaseIdleHandler
    {
        public LevelMappingView LevelMappingView { get; set; }
        public ControlInterfaceView ControlInterfaceView { get; set; }
        public override void Run()
        {
            if (ControlInterfaceView.ViewModel == null)
                return;

            ControlInterfaceView.ViewModel.ShowDataFileBrowserWindow();
        }

        public override string GetName()
        {
            return "AssignDataFilesHandler";
        }
    }
}




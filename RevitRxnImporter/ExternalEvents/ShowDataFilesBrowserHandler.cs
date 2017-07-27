using Autodesk.Revit.UI;

namespace RevitReactionImporter
{
    class ShowDataFilesBrowserHandler : BaseIdleHandler
    {
        public LevelMappingView LevelMappingView { get; set; }
        public ControlInterfaceView ControlInterfaceView { get; set; }
        public override void Run()
        {
            if (ControlInterfaceView.ViewModel == null)
                return;

            ControlInterfaceView.ViewModel.ShowDataFileBrowserWindow(ControlInterfaceView.ViewModel._projectId);
        }

        public override string GetName()
        {
            return "AssignDataFilesHandler";
        }
    }
}




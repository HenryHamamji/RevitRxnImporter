using Autodesk.Revit.UI;

namespace RevitReactionImporter
{
    class ShowClearBeamAnnotationsWindowHandler : BaseIdleHandler
    {
        public ControlInterfaceView ControlInterfaceView { get; set; }
        public override void Run()
        {
            if (ControlInterfaceView.ViewModel == null)
                return;

            ControlInterfaceView.ViewModel.ShowClearBeamAnnotationsWindow();
        }

        public override string GetName()
        {
            return "ShowClearBeamAnnotationsWindowHandler";
        }
    }
}




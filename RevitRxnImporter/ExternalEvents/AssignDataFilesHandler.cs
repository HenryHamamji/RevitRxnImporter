using Autodesk.Revit.UI;

namespace RevitReactionImporter
{
    class AssignDataFilesHandler : BaseIdleHandler
    {
        public ControlInterfaceView ControlInterfaceView { get; set; }
        public DataFileBrowser DataFileBrowser { get; set; }

        public override void Run()
        {
            if (ControlInterfaceView.ViewModel == null)
                return;
            if(DataFileBrowser.TempButtonName == "btnRAMModelFile")
            {
                ControlInterfaceView.ViewModel.AssignRAMModelDataFile(DataFileBrowser.RAMModelMetaDataFilePath);
            }
            else if (DataFileBrowser.TempButtonName == "btnRAMReactionsFile")
            {
                ControlInterfaceView.ViewModel.AssignRAMReactionsDataFile(DataFileBrowser.RAMModelReactionsFilePath);

            }
            else if (DataFileBrowser.TempButtonName == "btnRAMStudsFile")
            {
                ControlInterfaceView.ViewModel.AssignRAMStudsDataFile(DataFileBrowser.RAMModelStudsFilePath);

            }
            else if (DataFileBrowser.TempButtonName == "btnRAMCamberFile")
            {
                ControlInterfaceView.ViewModel.AssignRAMCamberDataFile(DataFileBrowser.RAMModelCamberFilePath);

            }
            DataFileBrowser.TempButtonName = "";

        }

        public override string GetName()
        {
            return "AssignDataFilesHandler";
        }
    }
}




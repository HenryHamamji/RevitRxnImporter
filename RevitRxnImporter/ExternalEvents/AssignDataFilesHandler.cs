﻿using Autodesk.Revit.UI;

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

            ControlInterfaceView.ViewModel.AssignDataFiles(DataFileBrowser.RAMModelMetaDataFilePath); // list of filePaths
        }

        public override string GetName()
        {
            return "AssignDataFilesHandler";
        }
    }
}




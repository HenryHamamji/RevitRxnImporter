using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Linq;
using System.Windows.Media;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

//using Newtonsoft.Json;


namespace RevitReactionImporter
{
    public class ControlInterfaceViewModel
    {
        private RevitReactionImporterApp _ida;

        //private readonly JsonSerializerSettings jsonSettings;


        private ControlInterfaceView _view = null; // TODO: replace this connection with data binding
        private Document _document;

        public ControlInterfaceViewModel(ControlInterfaceView view, Document doc,
            RevitReactionImporterApp ida)
        {
            _ida = ida;

            _view = view;
            _view.ViewModel = this;

            _document = doc;


            IList<RibbonItem> ribbonItems = _ida.RibbonPanel.GetItems();
        }

        public void DocumentClosed()
        {
            // Document already closed, we can't do anything.
        }


        public void ImportBeamReactions()
        {
        }

        public void ResetBeamReactions()
        {
        }

    }
}

using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Windows;
using System.Windows.Threading;
using MessageBox = System.Windows.MessageBox;
using View = Autodesk.Revit.DB.View;

namespace RevitReactionImporter
{
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class ControlInterfaceDockingPane : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message,
            ElementSet elements)
        {
            try
            {
                var paneId = RevitReactionImporterApp.GetControlPaneDockableId();
                var controlPane = commandData.Application.GetDockablePane(paneId);
                controlPane.Show();
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return Result.Failed;
            }
        }
    }

    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class NoActionCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            return Result.Succeeded;
        }
    }

    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class RevitReactionImporterApp : IExternalApplication
    {
        private const string APP_TAB_NAME = "S.A. MIRO STRUCTURAL";
        internal ToolTip ToolTip { get; set; }
        public static UIControlledApplication RevitApplication { get; set; }
        public ControlInterfaceView ControlInterfaceView { get; set; }
        public LevelMappingView LevelMappingPaneView { get; set; }
        private LevelMappingViewModel LevelMappingViewModel { get; set; }
        public bool IsLevelMappingPaneVisible { get; set; }
        public bool LevelMappingPaneVisibilityChanged { get; set; }
        public ControlInterfaceViewModel ControlInterfaceViewModel { get; set; }
        private static DockablePaneId _controlPaneId;
        public static DockablePaneId _levelMappingId;
        private DockablePane _levelMapping = null;
        public Document Document { get; private set; }
        private UIApplication _uiApplication;
        public string _projectId = "";
        private string _assemblyPath = @"C:\dev\RevitRxnImporter\RevitRxnImporter\bin\Debug\RevitReactionImporterApp.dll"; // "" System.Reflection.Assembly.GetExecutingAssembly().Location;
        public Autodesk.Revit.UI.RibbonPanel RibbonPanel { get; private set; }

        public Result OnStartup(UIControlledApplication uiApp)
        {
            RevitApplication = uiApp;
            try
            {
                uiApp.CreateRibbonTab(APP_TAB_NAME);
                RibbonPanel = uiApp.CreateRibbonPanel(APP_TAB_NAME, "RAM Data Importer");
                AddPushButton(RibbonPanel);
                uiApp.ControlledApplication.DocumentCreated += DocumentCreatedHandler;
                uiApp.ControlledApplication.DocumentOpened += DocumentOpenedHandler;
                uiApp.ControlledApplication.DocumentChanged += DocumentChangedHandler;
                uiApp.ControlledApplication.DocumentClosed += DocumentClosedHandler;
                _controlPaneId = new DockablePaneId(Guid.NewGuid());
                ControlInterfaceView = new ControlInterfaceView();
                RevitApplication.RegisterDockablePane(_controlPaneId, "RAM to Revit Data Importer", ControlInterfaceView);
                uiApp.Idling += UiIdlingHandler;
                NullProperties();
                _assemblyPath = @"C:\dev\RevitRxnImporter\RevitRxnImporter\bin\Debug\RevitReactionImporterApp.dll"; //System.Reflection.Assembly.GetExecutingAssembly().Location;
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return Result.Failed;
            }
        }

        public Result OnShutdown(UIControlledApplication uiApp)
        {
            try
            {
                uiApp.Idling -= UiIdlingHandler;
                //TODO: unregister documents
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return Result.Failed;
            }
        }

        public void DocumentOpenedHandler(object sender, DocumentOpenedEventArgs args)
        {
            DocumentHandler(args.Document);
        }

        public static DockablePaneId GetControlPaneDockableId()
        {
            return _controlPaneId;
        }

        private void DocumentCreatedHandler(object sender, DocumentCreatedEventArgs args)
        {
            DocumentHandler(args.Document);
        }

        private void DocumentChangedHandler(object sender, DocumentChangedEventArgs e)
        {
            if (e.Operation != UndoOperation.TransactionCommitted)
            {
                return;
            }
        }


        private void DocumentHandler(Document doc)
        {
            Document = doc;
            _projectId = Document.Title;
            ControlInterfaceViewModel = new ControlInterfaceViewModel(ControlInterfaceView, Document, this, _projectId);
            new DockablePane(_controlPaneId).Hide();
            ToolTip = new ToolTip();
            foreach (RibbonTab tab in ComponentManager.Ribbon.Tabs)
            {
                if (tab.Id == "Modify")
                {
                    tab.PropertyChanged += SelectEvent;
                    break;
                }
            }
        }

        private void SelectEvent(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Title" && _uiApplication != null)
            {
                // This check in needed when an item is selected in a project,
                // and the user switches to another project.
                if (_uiApplication.ActiveUIDocument == null)
                    return;
            }
        }

        private void AddPushButton(Autodesk.Revit.UI.RibbonPanel panel)
        {
            PushButton appButton = panel.AddItem(new PushButtonData("RAM Data Importer", "Import RAM Data", _assemblyPath, "RevitReactionImporter.ControlInterfaceDockingPane")) as PushButton;
            appButton.ToolTip = "RAM Data Importer";
            System.Windows.Media.Imaging.BitmapImage imagePushButton = new System.Windows.Media.Imaging.BitmapImage();
            imagePushButton.BeginInit();
            imagePushButton.UriSource = new Uri(@"C:\dev\RevitRxnImporter\RevitRxnImporter\Resources\icon.jpeg");
            imagePushButton.DecodePixelHeight = 50;
            appButton.Image = imagePushButton;
            appButton.LargeImage = imagePushButton;
            imagePushButton.EndInit();
            System.Windows.Media.Imaging.BitmapImage imageToolTip = new System.Windows.Media.Imaging.BitmapImage();
            imageToolTip.BeginInit();
            imageToolTip.UriSource = new Uri(@"C:\dev\RevitRxnImporter\RevitRxnImporter\Resources\icon.jpeg");
            imageToolTip.DecodePixelHeight = 200;
            appButton.ToolTipImage = imageToolTip;
            imageToolTip.EndInit();
        }


        PushButtonData CreatePushButtonData(string id, string text, Type cmd, System.Drawing.Bitmap bitmap = null)
        {
            PushButtonData btn = new PushButtonData(id, text, System.Reflection.Assembly.GetExecutingAssembly().Location, cmd.FullName);
            return btn;
        }

        //private void TooltipUpdate(List<ElementId> elementIds)
        //{
        //    var p = Cursor.Position;

        //    if (elementIds.Count == 1)
        //    {
        //        var id = elementIds.First();
        //        string s = ResultsAggregator.GetElementStatusMessage(id);

        //        if (s != null)
        //        {
        //            ToolTip.Location = p + new System.Drawing.Size(ToolTip.Offset);
        //            ToolTip.SetText(s);
        //            ToolTip.AutoSize = true;
        //            ToolTip.MaximumSize = new System.Drawing.Size(500, 100);

        //            ToolTip.Show();
        //        }
        //    }
        //    else
        //    {
        //        ToolTip.Hide();
        //    }
        //}

        private void NullProperties()
        {
            ControlInterfaceViewModel = null;
            LevelMappingViewModel = null;
        }


        public void DocumentClosedHandler(object sender, DocumentClosedEventArgs args)
        {
            ControlInterfaceViewModel.DocumentClosed();
            Document = null;
            _uiApplication = null;
            _projectId = "";
            NullProperties();
        }

        public void UiIdlingHandler(object sender, Autodesk.Revit.UI.Events.IdlingEventArgs args)
        {
            if (Document == null)
                return;

            _uiApplication = sender as UIApplication;
            var uiDocument = _uiApplication.ActiveUIDocument;

            if (ControlInterfaceViewModel == null)
                return;
        }

        // Gets the active UI View from the UIDoc.
        public static UIView GetActiveUiView(UIDocument uidoc)
        {
            Document doc = uidoc.Document;
            View view = doc.ActiveView;
            IList<UIView> uiviews = uidoc.GetOpenUIViews();
            UIView uiview = null;
            foreach (UIView uv in uiviews)
            {
                if (uv.ViewId.Equals(view.Id))
                {
                    uiview = uv;
                    break;
                }
            }
            return uiview;
        }

        public View3D GetView3d(Document doc)
        {
            return new FilteredElementCollector(doc).OfClass(typeof(View3D)).Cast<View3D>().FirstOrDefault<View3D>(
                v => v.Name.Equals("{3D}"));
        }
    }  

}

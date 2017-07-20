using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Autodesk.Revit.UI;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;

namespace RevitReactionImporter
{
    /// <summary>
    /// Interaction logic for LevelMapping.xaml
    /// </summary>
    //public partial class LevelMapping : UserControl
    //{
    //    public LevelMapping()
    //    {
    //        InitializeComponent();
    //    }
    //}
    public partial class LevelMappingView : Page, IDockablePaneProvider
    {
        public ObservableCollection<string> RevitLevels { get; set; }
        internal LevelMappingViewModel ViewModel { get; set; }
        internal DockablePaneProviderData _paneProviderData = null;
        public DockablePaneId LevelMappingPaneId { get; set; }

        private ExternalEvent setLevelMappingFromUserEvent;
        //private ExternalEvent clearReactionsEvent;

        public ObservableCollection<string> RevitLevelNames { get; set; }

        public LevelMappingView(DockablePaneId paneId)
        {
            InitializeComponent();
            LevelMappingPaneId = paneId;
            if(ViewModel!=null)
            {
                RevitLevelNames = ViewModel.RevitLevelNames;

            }
            DataContext = this;


            var setLevelMappingFromUserHandler = new SetLevelMappingFromUserHandler();
            setLevelMappingFromUserHandler.View = this;
            setLevelMappingFromUserEvent = ExternalEvent.Create(setLevelMappingFromUserHandler);
        }

        public void SetupDockablePane(DockablePaneProviderData data)
        {
            data.FrameworkElement = this;

            data.InitialState.SetFloatingRectangle(new Rectangle(200, 150, 500, 350));
            data.InitialState.DockPosition = DockPosition.Floating;

            //data.FrameworkElement.MaxHeight = 200;
            //data.FrameworkElement.MaxWidth = 300;

            _paneProviderData = data;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void OnClickSetLevelMappingFromUser(object sender, RoutedEventArgs e)
        {
            if (setLevelMappingFromUserEvent != null)
                setLevelMappingFromUserEvent.Raise();
            else
                MessageBox.Show("SetLevelMappingFromUser event handler is null");
        }
    }
}

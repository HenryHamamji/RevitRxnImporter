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
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using Autodesk.Revit.UI;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;

namespace RevitReactionImporter
{
    /// <summary>
    /// Interaction logic for LevelMapping.xaml
    /// </summary>
    public partial class LevelMappingView : Window
    {
        public ObservableCollection<string> RevitLevels { get; set; }
        internal LevelMappingViewModel ViewModel { get; set; }
        private ExternalEvent setLevelMappingFromUserEvent;
        public ObservableCollection<string> RevitLevelNames { get; set; }
        public ControlInterfaceViewModel ControlInterfaceViewModel { get; set; }

        public LevelMappingView(ControlInterfaceViewModel controlInterfaceViewModel)
        {
            InitializeComponent();
            if(ViewModel!=null)
            {
                RevitLevelNames = ViewModel.RevitLevelNames;
            }
            ControlInterfaceViewModel = controlInterfaceViewModel;
            var setLevelMappingFromUserHandler = new SetLevelMappingFromUserHandler();
            setLevelMappingFromUserHandler.View = this;
            setLevelMappingFromUserEvent = ExternalEvent.Create(setLevelMappingFromUserHandler);
        }

        public void SetupWindowSize()
        {
            int height = 200;
            int width = 580;
            if(RevitLevelNames!=null)
            {
                height = RevitLevelNames.Count * 40;
            }
            Height = height;
            Width = width;
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }

    }
}

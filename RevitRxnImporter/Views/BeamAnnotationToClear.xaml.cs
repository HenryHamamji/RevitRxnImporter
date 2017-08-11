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

namespace RevitReactionImporter
{
    public partial class BeamAnnotationToClear : UserControl
    {
        public bool IsReactionsPressed { get; set; }
        public bool IsStudCountsPressed { get; set; }
        public bool IsCamberValuesPressed { get; set; }
        public ClearAnnotationsMain ClearAnnotationsMain { get; set; }

        public BeamAnnotationToClear(ClearAnnotationsMain clearAnnotationsMain)
        {
            IsReactionsPressed = false;
            IsStudCountsPressed = false;
            IsCamberValuesPressed = false;
            ClearAnnotationsMain = clearAnnotationsMain;
            InitializeComponent();
        }

        private void OnReactionsToClearClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (!IsReactionsPressed)
            {
                IsReactionsPressed = true;
                ClearAnnotationsMain.IsReactionsPressed = true;
                button.Background = Brushes.LightSteelBlue;
                Keyboard.ClearFocus();
            }
            else
            {
                IsReactionsPressed = false;
                ClearAnnotationsMain.IsReactionsPressed = false;
                button.ClearValue(Control.BackgroundProperty);
                Keyboard.ClearFocus();
            }
        }

        private void OnStudCountsToClearClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (!IsStudCountsPressed)
            {
                IsStudCountsPressed = true;
                ClearAnnotationsMain.IsStudCountsPressed = true;        
                button.Background = Brushes.LightSteelBlue;
                Keyboard.ClearFocus();
            }
            else
            {
                IsStudCountsPressed = false;
                ClearAnnotationsMain.IsStudCountsPressed = false;
                button.ClearValue(Control.BackgroundProperty);
                Keyboard.ClearFocus();
            }
        }

        private void OnCamberValuesToClearClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (!IsCamberValuesPressed)
            {
                IsCamberValuesPressed = true;
                ClearAnnotationsMain.IsCamberValuesPressed = true;
                button.Background = Brushes.LightSteelBlue;
                Keyboard.ClearFocus();
            }
            else
            {
                IsCamberValuesPressed = false;
                ClearAnnotationsMain.IsCamberValuesPressed = false;
                button.ClearValue(Control.BackgroundProperty);
                Keyboard.ClearFocus();
            }
        }

        private void OnSelectLevelsToClearClick(object sender, RoutedEventArgs e)
        {
            if (!ClearAnnotationsMain.IsReactionsPressed && !ClearAnnotationsMain.IsCamberValuesPressed && !ClearAnnotationsMain.IsStudCountsPressed)
            {
                System.Windows.Forms.MessageBox.Show("No beam annotation to clear has been selected. Please choose at least one.");
                return;
            }
            ClearAnnotationsMain.ContentHolder.Content = new RevitLevels(ClearAnnotationsMain);

        }


    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Abr.IPScanner.Views
{
    /// <summary>
    /// Interaktionslogik für OverviewView.xaml
    /// </summary>
    public partial class OverviewView : UserControl
    {
        public OverviewView()
        {
            InitializeComponent();
        }

        private void Border_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Application.Current.MainWindow.DragMove();
            }
        }
    }
}

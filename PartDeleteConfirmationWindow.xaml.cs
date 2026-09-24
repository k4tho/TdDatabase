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
using System.Windows.Shapes;

namespace PartsInfo
{
    /// <summary>
    /// Interaction logic for PartDeleteConfirmationWindow.xaml
    /// </summary>
    public partial class PartDeleteConfirmationWindow : Window
    {
        PartInfoWindow partInfoWindow;
        public PartDeleteConfirmationWindow(PartInfoWindow partInfoWindow)
        {
            InitializeComponent();
            this.partInfoWindow = partInfoWindow;
        }

        

        private void No_Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Yes_Button_Click(object sender, RoutedEventArgs e)
        {
            partInfoWindow.DeletePart();
            Close();
        }
    }
}

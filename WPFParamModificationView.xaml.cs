using ClassGetMS.Services;
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

namespace ClassGetMSReferences.Views
{
    /// <summary>
    /// Logique d'interaction pour WPFParamModification.xaml
    /// </summary>
    public partial class WPFParamModificationView : Window
    {
        private IDataService dataService;

        public WPFParamModificationView()
        {
            InitializeComponent();
        }

        public WPFParamModificationView(IDataService dataService)
        {
            this.dataService = dataService;
        }

        private void CbxBase_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}

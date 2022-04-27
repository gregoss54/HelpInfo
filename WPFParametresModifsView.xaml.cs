using ClassGetMS;
using ClassGetMSReferences.ViewModel;
using System.ComponentModel;
using System.Windows;

namespace ClassGetMSReferences.Views
{
    /// <summary>
    /// Logique d'interaction pour WPFParametresModifs.xaml
    /// </summary>
    public partial class WPFParametresModifsView : Window
    {
        public WPFParametresModifsView()
        {
            InitializeComponent();
        }

        ///// <summary>
        ///// Pour garder une page au premier plan
        ///// </summary>
        //System.Windows.Forms.Form OwnerForm;

        //public StrucParam ParamGlobaux { get; internal set; }

        //public WPFParametresModifs(System.Windows.Forms.Form Owner)
        //     : this()
        //{
        //    this.OwnerForm = Owner;
        //}
        //void WPFParametresModifs_Closing(object sender, CancelEventArgs e)
        //{
        //    if (WPFParametresViewModel.val == true)
        //    {
        //        string msg = "Etes vous certain de vouloir quitter?";
        //        MessageBoxResult result =
        //          System.Windows.MessageBox.Show(
        //            msg,
        //            " ",
        //            MessageBoxButton.YesNo,
        //            MessageBoxImage.Warning);
        //        if (result == MessageBoxResult.No)
        //        {
        //            // Si l'utilisateur ne veux pas fermer, cancel la fermeture
        //            e.Cancel = true;
        //        }
        //    }
        //}
    }
}
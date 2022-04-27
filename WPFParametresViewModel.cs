using ClassGetMS.Models;
using ClassGetMS.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MvvmDialogs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using ClassGetMSReferences.Views;
using ClassGetMS;
using IDialogService = MvvmDialogs.IDialogService;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Forms;
using ListViewItem = System.Windows.Controls.ListViewItem;
using MessageBox = System.Windows.MessageBox;

// Désactivation directive du préprocesseur: permet de désactivier le warning CD4014 :
// Dans la mesure où cet appel n'est pas attendu, l'exécution de la méthode actuelle continue avant la fin de l'appel. 
// Envisagez d'appliquer l'opérateur 'await' au résultat de l'appel.
#pragma warning disable 4014


namespace ClassGetMSReferences.ViewModel

/// <summary>
/// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// Auteur : Masson Gregory
/// Crée le 02/03/22
/// 
/// Description : 
/// Permet la modification ou la création des paramétres 
/// les parametres sont :
///     -Officiels: valables por toutes les organisations en fonction de la convention collective, de la loi
///     -liés à l'établissement, un établissement ou une organisation possède des paramêtres 
///     -liés à l'agent: après négociation, un agent peut âvoir des paramétres qui ne sont pas ceux de ces collègues mais également,  certain paramétres sont propres à l'agent (jour de CA, ...)
///     il est possible d'ajouter, de supprimer (si non utlisé) et de modifier (mais pas le code) d'un parametre
///     les parametres utilisés ne peuvent être supprimés que logiquement mais en général un  paramétres possède une nouvelle date d'entrée en vigueur
///     La date de mise en vigueur précise la si le parametre est actuellement utilisé ou si il le sera dans l'avenir
/// Algo
/// Au démarrage, en fonction de l'action à réaliser (Parametre officiel, etablissement, agent)
/// afficher le contenu des zones de saisies
/// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// </summary>
///  
{
    public class WPFParametresViewModel : ViewModelBase, IModalDialogViewModel
    {
        #region Déclaration des variables de classe      

        // Structure de données transmise entre les écrans permettant de retrouver les caractèristiques de l'application
        public static ClassGetMS.StrucParam ParamGlobaux = new ClassGetMS.StrucParam();


        // instanciation d'un DialogService
        readonly DateTime dateInit = new DateTime(0001, 1, 1);
        private IDialogService _dialogService;
        private SqlConnection _oConnection; // Parametre de connexion à la database qui sera utilisé dans tout le programme
        private IDataService _dataService;
        private DataSet Ds;
        private string StSQL;              // Requete
        private ParamOfficiel _paramOffi;
        public ParamOfficiel paramOffi;

        //instanciation obligatoire d'un boolean à l'appel du DialogService
        public bool? DialogResult
        {
            get
            {
                return true;
            }
        }



        //création d'une variable de type ObservableObject (qui se remplira au fur et à mesure de la saisie des informations)
        private ParamOfficiel _paramOff;
        public ParamOfficiel paramOff
        {
            get
            {
                return _paramOff;
            }
            set
            {
                if (value != _paramOff)
                {
                    _paramOff = value;
                    RaisePropertyChanged("paramètresofficiels");
                }
            }
        }
        /// <summary>
        /// pour afficher les elements dans la ListView, il va nous falloir une Liste d'objets observables qui récupére les objets observables ParamOfficiels
        /// => Création de la liste de type List (pour Binder)
        /// </summary>
        private List<ParamOfficiel> _ListViewParamOfficiels;
        public List<ParamOfficiel> ListViewParamOfficiels
        {
            get
            {
                return _ListViewParamOfficiels;
            }
            set
            {
                if (value != _ListViewParamOfficiels)
                {
                    _ListViewParamOfficiels = value;
                    RaisePropertyChanged("listViewParamOfficiels");
                }
            }
        }

        public static bool val { get; internal set; }

        public SqlConnection oConnection { get; private set; }

        #endregion

        #region Déclaration des RelayCommand 

        // création de la commande loaded
        public RelayCommand LoadedCommand { get; private set; }
        // création d'une commande qui controlera le bouton "ajouter"
        public RelayCommand AjouterCommand { get; set; }
        // création d'une commande qui controlera le bouton "modifier"
        public RelayCommand chkBoxListeCheck { get; set; }
        // création d'une commande qui controlera le bouton "supprimer"
        public RelayCommand<ParamOfficiel> SupprimerCommand { get; set; }
        // création d'une commande qui ferme la fenetre WPF
        public RelayCommand<Window> QuitterCommand { get; private set; }
        //création d'une commande qui gére la selection d'un element a modifier
        public RelayCommand<ParamOfficiel> ListeDblClickVersModif { get; set; }
        // création d'une commande qui controlera le bouton "modifier"
        public RelayCommand<ParamOfficiel> ModifierCommand { get; set; }
        // création de la commande gérant le bouton d'aide
        public RelayCommand AideCommand { get; set; }


        // 2éme page

        public Window WPFParametresModifsView { get; private set; }
        // création d'une commande qui ferme la fenetre WPF
        public RelayCommand<Window> QuitterModifsCommand { get; set; }

        #endregion

        private void Suppression(ParamOfficiel paramOff)
        {
            using (SqlConnection oConnection = new SqlConnection(_dataService.ParamGlobaux.ConnectionString))
            {
                try
                {
                    if (paramOff.Supprimer == true)
                    {
                        bool exist = false;

                        foreach (ParamOfficiel paramOffi in ListViewParamOfficiels)
                        {
                            if (paramOff.Nom != paramOff.Nom)
                            {
                                exist = false;
                            }
                            // Dés qu'il est trouvé dans la table, on sort de la boucle
                            else
                            {
                                exist = true;
                                break;
                            }
                        }


                        MessageBoxResult res = MessageBox.Show("Êtes-vous sûr de vouloir supprimer cet élément ?",
                            "Décision à prendre", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                        // Si l'on confirme la suppression
                        if (res == MessageBoxResult.Yes)
                        {
                            // Si le param n'existe pas dans les tables ParamOfficiels de la base de données, on supprime totalement l'acte de la base de données
                            if (exist == false)
                            {
                                string StSQL = "DELETE FROM ParamOfficiels WHERE Nom = @Nom && DateEntreeVigueur = @DateEntreeVigueur";
                                using (SqlCommand command = new SqlCommand(StSQL, oConnection))
                                {
                                    command.Parameters.AddWithValue("@Nom", paramOff.Nom);

                                    oConnection.Open();
                                    command.ExecuteNonQuery();
                                }
                            }

                            // Si le param existe dans les tables ParamOfficiels de la base de données, on fera une suppression logique
                            else if (exist == true)
                            {
                                string StSQL = "UPDATE ParamOfficiels SET Supprimer = 1 WHERE Nom = @Nom";
                                using (SqlCommand command = new SqlCommand(StSQL, oConnection))
                                {
                                    command.Parameters.AddWithValue("@Nom", paramOff.Nom);
                                    oConnection.Open();
                                    command.ExecuteNonQuery();
                                }
                            }

                            ActualiseListView();
                            MessageBox.Show("L'élément a bien été supprimé.");
                        }
                        else
                            ActualiseListView();
                    }

                    // Si on veut décocher la case "Supprimer"
                    else
                    {
                        MessageBoxResult res = MessageBox.Show("Êtes-vous sûr de vouloir restaurer cet élément ?",
                            "Décision à prendre", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                        // Si l'on confirme la restauration
                        if (res == MessageBoxResult.Yes)
                        {
                            string StSQL = "UPDATE ParamOfficiels SET Supprimer = 0 WHERE Nom = @Nom";
                            using (SqlCommand command = new SqlCommand(StSQL, oConnection))
                            {
                                command.Parameters.AddWithValue("@Nom", paramOff.Nom);
                                oConnection.Open();
                                command.ExecuteNonQuery();
                            }
                            ActualiseListView();
                            Xceed.Wpf.Toolkit.MessageBox.Show("L'élément a bien été restauré.");
                        }
                        else
                            ActualiseListView();
                    }
                }

                catch (Exception e)
                {
                    Debug.WriteLine("WPFParametresViewModel.Suppression : " + e.Message);
                }
            }
        }



        #region Constructeur
        public WPFParametresViewModel(MvvmDialogs.IDialogService dialogService, IDataService dataService)
        {
            //déclaration du Dialog dans le constructeur
            _dialogService = dialogService;
            _dataService = dataService;


            LoadedCommand = new RelayCommand(() => Initialize());
            AideCommand = new RelayCommand(() => ClassUILibrary.Design.AfficheAide("AideParametres.pdf", oConnection));

            #endregion

            #region Suppression BD

            //void Suppression(ParamOfficiel paramOfficiel)
            //{
            //    if (paramOfficiel.Supprimer == true)
            //    {
            //        bool exist = false;
            //        ListViewParamOfficiels = _dataService.GetParametreOfficiel();
            //        foreach (ParamOfficiel item in ListViewParamOfficiels)
            //        {
            //            if (paramOfficiel.Nom != item.Nom)
            //            {
            //                exist = false;
            //            }
            //            else
            //            {
            //                exist = true;
            //                break;
            //            }
            //        }
            //        using (SqlConnection connection = new SqlConnection(_dataService.ParamGlobaux.ConnectionString))
            //        {
            //            try
            //            {
            //                MessageBoxResult result;
            //                result = MessageBox.Show("Voulez vous vraiment supprimer ce paramètre?", "Suppression", MessageBoxButton.YesNo);
            //                if (result == MessageBoxResult.Yes)
            //                {
            //                    if (exist == false)
            //                    {

            //                        StSQL = "Delete From ParamOfficiels Where Paramofficiels.Nom ='" + paramOfficiel.Nom + "' & ParamOfficiels.DateEntreeVigueur ='" + paramOfficiel.DateEntréeVigueur + "';";

            //                        using (SqlCommand command = new SqlCommand(StSQL, oConnection))
            //                        {
            //                            oConnection.Open();
            //                            command.ExecuteNonQuery();
            //                        }
            //                        MessageBox.Show("Le paramètre est supprimé");
            //                    }
            //                    else
            //                    {
            //                        MessageBox.Show("Suppressiion annulée");
            //                    }
            //                    ChargementListView();
            //                }
            //            }
            //            catch (Exception e)
            //            {
            //                Debug.WriteLine("DataService.Suppression: " + e.Message);
            //            }
            //            finally
            //            {
            //                if (oConnection.State == ConnectionState.Open)
            //                    oConnection.Close();
            //            }
            //        }
            //    }
            //}



            #endregion

            #region Boutons

            #region Bt Modifier

            //------------------
            //  Evenement DoubleClic/ Bouton Modifier avec selection préalable
            //------------------
            
            ListeDblClickVersModif = new RelayCommand<ParamOfficiel>((ParamOfficiel) =>
            {

                if (ParamOfficiel != null)
                    openDetails("modifier", ParamOfficiel);
                else
                    MessageBox.Show("Veuillez sélectionner un paramètre à modifier.");

            });


            ModifierCommand = new RelayCommand<ParamOfficiel>((item) =>
                {

                    if (item != null)
                    {
                        paramOff = item;
                        checkCode = false;

                        //on envoit vers la fenetre
                        _dialogService.ShowDialog<WPFParametresModifsView>(this, this);
                    }
                    else
                    {
                        MessageBox.Show("aucune ligne selectionnée");
                    }
                });


            #endregion
            #region BT Ajouter


            // Ouverture de la page d'ajout d'un paramètre
            AjouterCommand = new RelayCommand(() => openDetails("ajouter"));

            #endregion
            #region BT Supprimer

            // Ouverture de la page de suppression de l'acte
            SupprimerCommand = new RelayCommand<ParamOfficiel>((paramOfficiel) =>
            {

                if (paramOfficiel != null)
                    Suppression(paramOfficiel);
                else
                    MessageBox.Show("Echec lors de la suppression. Veuillez réessayer.");
            });


            // Ouverture de la page de suppression 
            SupprimerCommand = new RelayCommand<ParamOfficiel>((item) =>
            {
                if (item != null)
                {
                    // connexion à la base
                    using (ProgetEntities pg = new ProgetEntities())
                    {
                        ParamOfficiel paramOff = pg.ParamOfficiels.Where(a => a.Nom == item.Nom).SingleOrDefault();
                        List<ParamOfficiel> pu = pg.ParamOfficiels.Where(a => a.Nom == paramOff.Nom).ToList();

                        if (pu.Count == 0)
                        {
                            pg.ParamOfficiels.Remove(paramOff);
                            pg.SaveChanges();
                        }
                        else
                        {
                            paramOff.Supprimer = !item.Supprimer;
                            pg.SaveChanges();
                        }
                        ActualiseListView();
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("aucune ligne selectionnée");
                }

            });





            #endregion
            #region BT Quitter
            //------------------
            //  Bouton Quitter
            //------------------


            QuitterCommand = new RelayCommand<Window>(FermetureFenetre);
            // QuitterModifsCommand = new RelayCommand<Window>(FermetureFenetreAM);

        }

      
        //Declaration d'un booleen gerant si l'on peut sortir sans message d'avertissement
        //A chaque modification d'un attribut, on le passe a faux
        private bool _AllowExit;
        public bool AllowExit
        {
            get
            {
                return _AllowExit;
            }
            set
            {
                if (value != _AllowExit)
                {
                    _AllowExit = value;
                    RaisePropertyChanged("AllowExit");
                }


            }
        }

        #endregion

        #endregion

        #region Gestion de l'affichage et de l'actualisation de la ListView (affichage logique) 

        public void ChargementListView()
        {
            if (chkListe == true)
            {
                chkListe = false;
            }
            List<ParamOfficiel> paramOfficiels = new List<ParamOfficiel>();

            ListViewParamOfficiels = new List<ParamOfficiel>();

            using (ProgetEntities context = new ProgetEntities(Model.GenerateEFConnectionString(oConnection.ConnectionString)))
                try
                {
                    paramOfficiels = context.ParamOfficiels.ToList();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }


            // parcourons la listView pour en extraire les elements
            foreach (ParamOfficiel item in ListViewParamOfficiels)
            {
                //on passe les données a la LV
                ListViewParamOfficiels.Add(new ParamOfficiel(item.Nom, item.Type, item.Valeur, item.Commentaire, item.Base, item.DateEntréeVigueur, item.Obligatoire, item.Supprimer));
            }
        }


        // Methode appellée apres chaque ajout de ligne ou de suppression de 
        // ligne dans la  ListView pour actualisation de l'affichage
        // </summary>
        public void ActualiseListView()
        {
            List<ParamOfficiel> paramOfficiels = new List<ParamOfficiel>();

            ListViewParamOfficiels = new List<ParamOfficiel>();

            using (ProgetEntities context = new ProgetEntities(Model.GenerateEFConnectionString(oConnection.ConnectionString)))
                try
                {
                    paramOfficiels = context.ParamOfficiels.ToList();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
        }


        #endregion

        #region Fermeture des fenetres
        // méthodes gérant la fermeture des fenetres
        private void FermetureFenetre(Window fenetre)
        {
            paramOff = new ParamOfficiel();
            chkListe = false;
            ChargementListView();
            fenetre.Close();


        }

        //private void FermetureFenetreAM(Window fenetre)
        //{
        //    // Si on est en mode modification
        //    if (!paramOffiCheck)
        //    {
        //        MessageBoxResult res = MessageBox.Show("Êtes-vous sûr de vouloir annuler les modifications ?",
        //                        "Décision à prendre", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
        //        if (res == MessageBoxResult.Yes)
        //        {
        //            paramOffi = new ParamOfficiel();
        //            fenetre.Close();
        //        }
        //    }
        //    else
        //        fenetre.Close();
        //}

        private bool _Close;
        public bool Close
        {
            get
            {
                return _Close;
            }
            set
            {
                if (value != _Close)
                {
                    _Close = value;
                    RaisePropertyChanged(nameof(Close));
                }
            }
        }
        #endregion

        #region Binding titre
        //Binding du titre suivant fenetre voulue

        private string _title = "Paramètres Officiels";


        public string title
        {
            get
            {
                return _title;
            }
            set
            {
                if (value != _title)
                {
                    _title = value;
                    RaisePropertyChanged("title");
                }
            }
        }
        #endregion

        #region Propriétés



        // Variable booléenne qui indiquera si l'utilisateur a un droit en contrôle totale sur la page ou non
        private bool _boolDroitCT;
        public bool boolDroitCT
        {
            get
            {
                return _boolDroitCT;
            }
            set
            {
                if (value != _boolDroitCT)
                {
                    _boolDroitCT = value;
                    RaisePropertyChanged("boolDroitCT");
                }
            }
        }

        // Variable booléenne qui indiquera si l'utilisateur a un droit en modification sur la page ou non
        private bool _boolDroitModif;
        public bool boolDroitModif
        {
            get
            {
                return _boolDroitModif;
            }
            set
            {
                if (value != _boolDroitModif)
                {
                    _boolDroitModif = value;
                    RaisePropertyChanged("boolDroitModif");
                }
            }
        }

        /// <summary>
        /// booléen permettant de faire varier la fenetre WPF entre la fonction "Ajouter" et la fonction "Modifier"
        /// </summary>
        private bool _checkCode;
        public bool checkCode
        {
            get
            {
                return _checkCode;
            }
            set
            {
                if (value != _checkCode)
                {
                    _checkCode = value;
                    RaisePropertyChanged("checkCode");
                }
            }
        }


        // Variable booléenne qui va indiquer si la checkbox "Afficher tous les éléments" est cochée ou non
        private bool _chkListe;
        public bool chkListe
        {
            get
            {
                return _chkListe;
            }
            set
            {
                if (value != _chkListe)
                {
                    _chkListe = value;
                    RaisePropertyChanged("chkListe");
                }
            }
        }
        // Booléen qui va indiquer si un acte a été entré en paramètre de la fonction opendetails ou non
        private bool _paramOffiCheck;
        public bool paramOffiCheck
        {
            get
            {
                return _paramOffiCheck;
            }
            set
            {
                if (value != _paramOffiCheck)
                {
                    _paramOffiCheck = value;
                    RaisePropertyChanged("paramOffiCheck");
                }
            }
        }
        #endregion

        #region Checkbox du booleen Base de la LV
        // Booléen qui va définir son état "Base" dans la LV

        private bool? _Base_Param;
        public bool? Base_Param
        {
            get { return (_Base_Param ?? false) ? _Base_Param : false; }
            set
            {
                _Base_Param = value;

            }

        }
        #endregion

        #region Checkbox du booleen obligatoire de la LV
        // Booléen qui va définir son état "Obligatoire" dans la LV

        private bool? _Obligatoire_Param;
        public bool? Obligatoire_Param
        {
            get { return (_Obligatoire_Param ?? false) ? _Obligatoire_Param : false; }
            set
            {
                _Obligatoire_Param = value;

            }

        }
        #endregion

        #region Checkbox du booleen Supprimer de la LV
        // Booléen qui va définir son état "Supprimer" dans la LV

        private bool? _Supprimer_Param;
        public bool? Supprimer_Param
        {
            get { return (_Supprimer_Param ?? false) ? _Supprimer_Param : false; }
            set
            {
                _Supprimer_Param = value;

            }

        }

        #endregion

        #region Gestion des droits/ Accès à la base de donnée/ Initialize
        public void Initialize()
        {
            Close = false;
            chkListe = false;
            oConnection = new SqlConnection(_dataService.ParamGlobaux.ConnectionString);
            ListViewParamOfficiels = _dataService.GetParametreOfficiel();

            // Récupération des droits de l'utilisateur pour cette page
            string StDroit;
            {
                StDroit = ClassGetMS.Droits.ValeurDroit(_dataService.ParamGlobaux.Matricule, _dataService.ParamGlobaux.IDEtablissement, _dataService.ParamGlobaux.ConnectionString, "Gestion des paramètres Officiels");
                if (StDroit == "CT" || _dataService.ParamGlobaux.Matricule == _dataService.ParamGlobaux.StNomProget)
                {
                    boolDroitCT = true;
                    boolDroitModif = true;
                }
                else if (StDroit == "M")
                {
                    boolDroitCT = false;
                    boolDroitModif = true;
                }
                else
                {
                    boolDroitCT = false;
                    boolDroitModif = false;
                }
            }
        }

        /// <summary>
        /// La méthode appelée lors du click sur Ajouter
        /// </summary>
        /// <param name="mode"></param>
        void openDetails(string mode)
        {
            paramOff = new ParamOfficiel();
            paramOff.Nom = "";
            paramOff.DateEntréeVigueur = DateTime.Now;
            Debug.WriteLine("méthode ajout");
            paramOffiCheck = true;


            if (mode == "ajouter")
                _dialogService.ShowDialog<WPFParametresModifsView>(this, this);
            else
                MessageBox.Show("Erreur lors de l'ouverture. Veuillez réessayer.");
        }

        /// <summary>
        /// La méthode appelée lors de l'evenement Modifier
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="paramOfficiel"></param>
        void openDetails(string mode, ParamOfficiel paramOfficiel)
        {
            paramOff = paramOfficiel;

            paramOffiCheck = false;

            if (mode == "modifier" && paramOff != null)
            {
                _dialogService.ShowDialog<WPFParametresModifsView>(this, this);
                ActualiseListView();
            }
            else
                MessageBox.Show("Erreur lors de l'ouverture. Veuillez réessayer.");


        }
    }
}



#endregion

#region Déclaration des variables de la table "ParamOfficiels"
//public class ParamOfficiel : List<ParamOfficiel>
//{
//    private string _Nom;
//    public string Nom
//    {//
//        get
//        {
//            return _Nom;
//        }
//        set
//        {
//            if (value != _Nom)
//            {
//                _Nom = value; RaisePropertyChanged("Nom");

//            }
//        }

//            private string _Type;
//    public string Type
//    {
//        get
//        {
//            return _Type;
//        }
//        set
//        {
//            //if (value != _Type)
//            //{
//            //    _Type = value; RaisePropertyChanged("Type");

//            //}
//        }
//    }
//    private string _Valeur;
//    public string Valeur
//    {
//        get
//        {
//            return _Valeur;
//        }
//        set
//        {
//            if (value != _Valeur)
//            {
//                // _Valeur = value; RaisePropertyChanged("Valeur");

//            }
//        }
//    }
//    private string _Commentaire;
//    public string Commentaire
//    {
//        get
//        {
//            return _Commentaire;
//        }
//        set
//        {
//            //if (value != _Commentaire)
//            //{
//            //    _Commentaire = value; RaisePropertyChanged("Commentaire");

//            //}
//        }
//    }
//    private bool _Base;
//    public bool Base
//    {
//        get
//        {
//            return _Base;
//        }
//        set
//        {
//            //if (value != _Base)
//            //{
//            //    _Base = value; RaisePropertyChanged("Base");

//            //}
//        }
//    }
//    private DateTime _DateEntreeVigueur;
//    public DateTime DateEntreeVigueur
//    {
//        get
//        {
//            return _DateEntreeVigueur;
//        }
//        set
//        {
//            //if (value != _DateEntreeVigueur)
//            //{
//            //    _DateEntreeVigueur = value; 
//            //RaisePropertyChanged("DateEntreeVigueur");

//            //}
//        }
//    }
//    private bool _Obligatoire;
//    public bool obligatoire
//    {
//        get
//        {
//            return _Obligatoire;
//        }
//        set
//        {
//            //if (value != _Obligatoire)
//            //{
//            //    _Obligatoire = value; RaisePropertyChanged("Obligatoire");

//            //}
//        }
//    }
//    private bool? _Supprimer;
//    public bool? Supprimer
//    {
//        get
//        {
//            return _Supprimer;
//        }
//        set
//        {
//            if (value != _Supprimer)
//            {
//                _Supprimer = value;
//                //RaisePropertyChanged("Supprimer");

//            }
//        }
//    }
//}
    


            #endregion

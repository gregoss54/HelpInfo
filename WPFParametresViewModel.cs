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
using System.Windows.Ink;
using System.Windows.Input;


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
        // Structure de données transmise entre les écrans permettant de retrouver les caractèristiques de l'application
        public static ClassGetMS.StrucParam ParamGlobaux = new ClassGetMS.StrucParam();

        // instanciation d'un DialogService
        readonly DateTime dateInit = new DateTime(0001, 1, 1);
        private IDialogService _dialogService;
        private SqlConnection _oConnection; // Parametre de connexion à la database qui sera utilisé dans tout le programme
        private IDataService _dataService;
        public ProgetEntities ProgetContext;
        public string EFConnectionString;


        #region Propriétés
        //instanciation obligatoire d'un boolean à l'appel du DialogService
        public bool? DialogResult
        {
            get
            {
                return true;
            }
        }

        private string _Nom;
        public string Nom
        {

            get
            {
                return _Nom;
            }
            set
            {
                if (value != _Nom)
                {
                    _Nom = value;
                    RaisePropertyChanged("Nom");
                    this.AllowExit = false;
                }
            }
        }

        private string _Valeur;
        public string Valeur
        {
            get
            {
                return _Valeur;
            }
            set
            {
                if (value != _Valeur)
                {
                    _Valeur = value;
                    RaisePropertyChanged("Valeur");
                    this.AllowExit = false;
                }
            }
        }

        private string _Type;
        public string Type
        {
            get
            {
                return _Type;
            }
            set
            {
                if (value != _Type)
                {
                    _Type = value;
                    RaisePropertyChanged("Type");
                    this.AllowExit = false;
                }
            }
        }

        private string _Commentaire;
        public string Commentaire
        {
            get
            {
                return _Commentaire;
            }
            set
            {
                if (value != _Commentaire)
                {
                    _Commentaire = value;
                    RaisePropertyChanged("Commentaire");
                    this.AllowExit = false;
                }
            }
        }
        private bool _Base;
        public bool Base
        {
            get
            {
                return _Base;
            }
            set
            {
                if (value != _Base)
                {
                    _Base = value;
                    RaisePropertyChanged("Base");
                    this.AllowExit = false;
                }
            }
        }
        private DateTime _DateEntréeVigueur;
        public DateTime DateEntréeVigueur
        {
            get
            {
                return _DateEntréeVigueur;
            }
            set
            {
                if (value != _DateEntréeVigueur)
                {
                    _DateEntréeVigueur = value;
                    RaisePropertyChanged("DateEntréeVigueur");
                    this.AllowExit = false;
                }
            }
        }
        private bool _Obligatoire;
        public bool Obligatoire
        {
            get
            {
                return _Obligatoire;
            }
            set
            {
                if (value != _Obligatoire)
                {
                    _Obligatoire = value;
                    RaisePropertyChanged("Obligatoire");
                    this.AllowExit = false;
                }
            }
        }
        private bool _Supprimer;
        public bool Supprimer
        {
            get
            {
                return _Supprimer;
            }
            set
            {
                if (value != _Supprimer)
                {
                    _Supprimer = value;
                    RaisePropertyChanged("Supprimer");
                    this.AllowExit = false;
                }
            }
        }

        #endregion

        #region Variables

        /// <summary>
        /// Variable qui contient la liste des types sélectionnables
        /// </summary>
        private List<string> _ListeTypes;

        public List<string> ListeTypes
        {
            get { return _ListeTypes; }
            set
            {
                _ListeTypes = value;
                RaisePropertyChanged(nameof(ListeTypes));
            }
        }
        private string _SelectedType;

        public string SelectedType
        {
            get { return _SelectedType; }
            set
            {

                _SelectedType = value;
                RaisePropertyChanged(nameof(SelectedType));
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
                    RaisePropertyChanged("paramOff");
                }
            }
        }
        /// <summary>
        /// pour afficher les elements dans la ListView, il va nous falloir une Liste d'objets observables qui récupére les objets observables ParamOfficiels
        /// => Création de la liste de type List (pour Binder)
        /// </summary>
        private List<ParamOfficiel> _ListeViewParamOfficiels;
        public List<ParamOfficiel> ListeViewParamOfficiels
        {
            get
            {
                return _ListeViewParamOfficiels;
            }
            set
            {
                if (value != _ListeViewParamOfficiels)
                {
                    _ListeViewParamOfficiels = value;
                    RaisePropertyChanged("ListeViewParamOfficiels");
                }
            }
        }

        private ParamOfficiel _SelectedParamOfficiel;
        public ParamOfficiel SelectedParamOfficiel
        {
            get
            {
                return _SelectedParamOfficiel;
            }
            set
            {
                if (value != _SelectedParamOfficiel)
                {
                    _SelectedParamOfficiel = value;
                    RaisePropertyChanged("SelectedParamOfficiel");
                }
            }
        }
        private ParamOfficiel _NewParamOfficiel;
        public ParamOfficiel NewParamOfficiel
        {
            get
            {
                return _NewParamOfficiel;
            }
            set
            {
                if (value != _NewParamOfficiel)
                {
                    _NewParamOfficiel = value;
                    RaisePropertyChanged("NewParamOfficiel");
                }
            }
        }


        #endregion

        #region Déclaration des RelayCommand 

        // création de la commande loaded
        public RelayCommand LoadedCommand { get; private set; }
        // création d'une commande qui controlera le bouton "ajouter"
        public RelayCommand AjouterCommand { get; set; }
        // création d'une commande qui controlera le bouton "modifier"
        public RelayCommand<ParamOfficiel> ModifierCommand { get; set; }
        public RelayCommand chkBoxListeCheck { get; set; }
        // création d'une commande qui controlera le bouton "supprimer"
        public RelayCommand<ParamOfficiel> SupprimerCommand { get; set; }
        // création d'une commande qui ferme la fenetre WPF
        public RelayCommand QuitterCommand { get; private set; }
        // création de la commande gérant le bouton d'aide
        public RelayCommand AideCommand { get; set; }
        //Commande gérant la checkbox"Aficher tout"
        public RelayCommand CbxAfficherTout { get; set; }
        public static bool val { get; internal set; }
        public SqlConnection oConnection { get; private set; }

        #endregion

        #region Constructeur
        public WPFParametresViewModel(IDialogService dialogService, IDataService dataService)
        {

            //déclaration du Dialog dans le constructeur
            _dialogService = dialogService;
            _dataService = dataService;
            CbxChecked = false;

            LoadedCommand = new RelayCommand(() => Initialize());
            AideCommand = new RelayCommand(() => ClassUILibrary.Design.AfficheAide("AideParametres.pdf", oConnection));
            QuitterCommand = new RelayCommand(() => Close = true);
            SupprimerCommand = new RelayCommand<ParamOfficiel>(DeleteParamOfficiel);
            ModifierCommand = new RelayCommand<ParamOfficiel>(ModifParametre);
            AjouterCommand = new RelayCommand(AddParametre);

            #endregion

        #region Méthodes

            void AddParametre()
            {
                WPFParamModificationViewModel fen = new WPFParamModificationViewModel(_dataService, _dialogService);
                ParamOfficiel param = new ParamOfficiel();
                fen.paramOff = param;
                fen.checkCode = true;
                fen.SelectedParamOfficiel = SelectedParamOfficiel;

                _dialogService.ShowDialog<WPFParamModificationView>(this, fen);


                ActualiseListView();

            }
            void ModifParametre(ParamOfficiel SelectedParamOfficiel)
            {

                WPFParamModificationViewModel fen = new WPFParamModificationViewModel(_dataService, _dialogService);
                fen.SelectedParamOfficiel = SelectedParamOfficiel;
                fen.checkCode = false;
                _dialogService.ShowDialog<WPFParamModificationView>(this, fen);


                ActualiseListView();
            }
            SupprimerCommand = new RelayCommand<ParamOfficiel>((DeletedParamOfficiel) =>
            {
                DeletedParamOfficiel = SelectedParamOfficiel;
                if (DeletedParamOfficiel != null)
                {
                    Debug.WriteLine("Effacement  " + DeletedParamOfficiel);
                    _dataService.DeleteParamOfficiel(DeletedParamOfficiel);
                    ActualiseListView();
                }
                else
                {
                    System.Windows.MessageBox.Show("Aucun param selectionné", "Suppression");

                }
            });

            void DeleteParamOfficiel(ParamOfficiel DeletedParOfficiel)
            {
                string requete = "DELETE FROM ParamOfficiels WHERE ParamOfficiels.Nom = '" + DeletedParOfficiel.Nom + "' AND ParamOfficiels.DateEntréeVigueur = '" + DeletedParOfficiel.DateEntréeVigueur + "' ;";
            }

            #endregion

        #region Gestion des droits/ Accès à la base de donnée/ Initialize
            void Initialize()
            {
                //oConnection = ClassLibraryProget.DataBase.OpenSqlServer(_dataService.ParamGlobaux.ConnectionString);
                Close = false;
                chkListe = false;
                oConnection = new SqlConnection(_dataService.ParamGlobaux.ConnectionString);
                ListeViewParamOfficiels = _dataService.GetParametreOfficiel();


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
            #endregion

        #region CheckBox "Afficher tous les elements"
            CbxAfficherTout = new RelayCommand(() =>
            {

                if (CbxChecked == true)
                {
                    ActualiseListView();
                }
                else
                {
                    ChargementListView();
                }
            });
        }
        #endregion

        #region Gestion de l'affichage et de l'actualisation de la ListView (affichage logique) 

        public void ChargementListView()
        {
            if (CbxChecked == true)
            {
                CbxChecked = false;
            }
            List<ParamOfficiel> paramOfficiels = new List<ParamOfficiel>();

            ListeViewParamOfficiels = new List<ParamOfficiel>();

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
            foreach (ParamOfficiel item in ListeViewParamOfficiels)
            {
                //on passe les données a la LV
                ListeViewParamOfficiels.Add(new ParamOfficiel(item.Nom, item.Type, item.Valeur, item.Commentaire, item.Base, item.DateEntréeVigueur, item.Obligatoire, item.Supprimer));
            }
        }


        // Methode appellée apres chaque ajout de ligne ou de suppression de 
        // ligne dans la  ListView pour actualisation de l'affichage
        // </summary>
        public void ActualiseListView()
        {
            oConnection = new SqlConnection(_dataService.ParamGlobaux.ConnectionString);
            ListeViewParamOfficiels = _dataService.GetParametreOfficiel();

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

        /// <summary>
        /// Declaration d'un booleen gerant si l'on peut sortir sans message d'avertissement
        /// A chaque modification d'un attribut, on le passe a faux
        /// </summary>

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

        /// <summary>
        /// booléen permettant la fermeture des fenetres
        /// </summary>
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

        /// <summary>
        /// booléen ramenant la valeur de l'état de Check de la CheckBox "Afficher tous les elements"
        /// </summary>
        private bool _CbxChecked;
        public bool CbxChecked
        {
            get
            {
                return _CbxChecked;
            }
            set
            {
                if (value != _CbxChecked)
                {
                    _CbxChecked = value;
                    RaisePropertyChanged("CbxChecked");
                }
            }
        }

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
    }
}
        #endregion



 

        


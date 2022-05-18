using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight.CommandWpf;
using ClassGetMS.Services;
using MvvmDialogs;
using System.Data.SqlClient;
using ClassGetMS;
using ClassGetMS.Models;
using System.Windows.Controls;
using System.Globalization;
using System.Text.RegularExpressions;



namespace ClassGetMSReferences.ViewModel
{
    public class WPFParamModificationViewModel : ViewModelBase, IModalDialogViewModel
    {
        #region Propriétés

        private IDialogService _dialogService;
        private IDataService _dataService;
        public StrucParam ParamGlobaux;
        public ParamOfficiel SelectedParamOfficiel;
        public List<ParamOfficiel> ListeViewParamOfficiels { get; set; }
        public bool checkCode;

       
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
                   
                }
            }
        }

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
       
        private string _Titre;
        public string Titre
        {
            get
            {
                return _Titre;
            }
            set
            {
                if (value != _Titre)
                {
                    _Titre = value;
                    RaisePropertyChanged("Titre");

                }
            }
        }

        #endregion

        #region Initialize
        private void Initialize()
        {

            Close = false;
            ParamGlobaux = _dataService.ParamGlobaux;
            oConnection = new SqlConnection(_dataService.ParamGlobaux.ConnectionString);
            ListeTypes = new List<String> { "B", "C", "D", "N" };

            //Recuperation de l'existant pour ajout/modif
            if (SelectedParamOfficiel != null)
            {
                Nom = SelectedParamOfficiel.Nom;
                Type = SelectedParamOfficiel.Type;
                Valeur = SelectedParamOfficiel.Valeur;
                Commentaire = SelectedParamOfficiel.Commentaire;
                Base = SelectedParamOfficiel.Base;
                DateEntréeVigueur = SelectedParamOfficiel.DateEntréeVigueur;
                Obligatoire = SelectedParamOfficiel.Obligatoire;
                Supprimer = SelectedParamOfficiel.Supprimer;
            }
            else
            //Instanciation Nouveau paramètre
            {
                Nom = "";
                Type = "C";
                Valeur = "";
                Commentaire = "";
                Base = false;
                DateEntréeVigueur = DateTime.Today;
                Obligatoire = false;
                Supprimer = false;
            }

        }
        #endregion

        #region Constructeurs

        public WPFParamModificationViewModel(IDataService dataService, IDialogService dialogService)
        {
            _dataService = dataService;
            _dialogService = dialogService;
       

            QuitterCommand = new RelayCommand(() => Close = true);
            LoadedCommand = new RelayCommand(() => Initialize());
            CbxBase = new RelayCommand(() => ComboBase());
            CbxObligatoire = new RelayCommand(() => ComboObligatoire());
            CbxSupprimer = new RelayCommand(() => ComboSupprimer());
            ValiderAjoutModifCommand = new RelayCommand(() =>SaveParamOfficiel());
       
        }

        #endregion

        #region Declaration RelayCommand
        //Commande du bouton quitter
        public RelayCommand QuitterCommand { get; set; }
        //Commande de chargement
        public RelayCommand LoadedCommand { get; private set; }
        //Commande du bouton Valider
        public RelayCommand ValiderAjoutModifCommand { get; set; }
        public SqlConnection oConnection { get; private set; }
        //Commande gérant la checkbox"supprimer"
        public RelayCommand CbxSupprimer { get; set; }
        //Commande gérant la checkbox"Obligatiore" 
        public RelayCommand CbxObligatoire { get; set; }
        //Commande gérant la checkbox"Base"
        public RelayCommand CbxBase { get; set; }

        #endregion

        #region Methodes

        //Booléen renvoyant l'état "supprimer" dans la LV
        private bool ComboSupprimer()
        {
            return Supprimer;
        }
        //Booléen renvoyant l'état "obligatoire" dans la LV
        private bool ComboObligatoire()
        {
            return Obligatoire;
        }
        //Booléen renvoyant l'état "base" dans la LV
        private bool ComboBase()
        {
            return Base;
        }

        // Ajout et modif d'un paramètre (utilisation des methodes de DataService "update" et "store")
        public void SaveParamOfficiel()
        {
            this.NewParamOfficiel = new ParamOfficiel();
            NewParamOfficiel.Nom = Nom;
            NewParamOfficiel.Type = Type;
            NewParamOfficiel.Valeur = Valeur;
            NewParamOfficiel.Commentaire = Commentaire;
            NewParamOfficiel.DateEntréeVigueur = DateEntréeVigueur;
            NewParamOfficiel.Obligatoire = Obligatoire;
            NewParamOfficiel.Base = Base;
            NewParamOfficiel.Supprimer = Supprimer;

            bool exist = true;
            if (SelectedParamOfficiel != null)
                exist =(NewParamOfficiel.Nom != SelectedParamOfficiel.Nom || NewParamOfficiel.DateEntréeVigueur != SelectedParamOfficiel.DateEntréeVigueur ? true : false) ;

            if (!checkCode  && !exist)
                _dataService.UpdateParametreOfficiel(NewParamOfficiel);      
            else
                _dataService.StoreParamOfficiel(NewParamOfficiel);
            Close = true;
                       
        }


        #endregion

        #region Variables

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
                RaisePropertyChanged("ListeTypes");
            }
        }
        /// <summary>
        /// Création d'une variable de type ObservableObject (qui se remplira au fur et à mesure de la saisie des informations)
        /// </summary>
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
        
        //A chaque modification d'un attribut, on le passe a faux
        private bool _AllowExit;
        public static bool val { get; internal set; }
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
    }
        #endregion

        #region Controle de saisie de l'ajout/Modif

    /// <summary>
    /// méthode gérant le contrôle de saisie du Nom
    /// </summary>
    public class NomParamOfficiel : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null)
            {
                return new ValidationResult(false, "Veuillez saisir une valeur");
            }
            else
            {
                WPFParamModificationViewModel.val = true;
                if (!Regex.IsMatch(value.ToString(), @"^[a-zA-Z '-éèêïôàâù]+$"))
                {
                    return new ValidationResult(false, "Caractères spéciaux interdits");
                }
            }
            return ValidationResult.ValidResult;
        }
    }

    /// <summary>
    /// méthode gérant le contrôle de saisie du Type
    /// </summary>
    public class TypeParamOfficiel : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null)
            {
                return new ValidationResult(false, "Veuillez saisir une valeur");
            }
            else
            {
                WPFParamModificationViewModel.val = true;
                if (!Regex.IsMatch(value.ToString(), @"^[a-zA-Z '-éèêïôàâù]+$"))
                {
                    return new ValidationResult(false, "Caractères spéciaux interdits");
                }
            }
            return ValidationResult.ValidResult;
        }
    }

    /// <summary>
    /// méthode gérant le contrôle de saisie du Valeur
    /// </summary>
    public class ValeurParamOfficiel : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null)
            {
                return new ValidationResult(false, "Veuillez saisir une valeur");
            }
            else
            {
                WPFParamModificationViewModel.val = true;
                if (!Regex.IsMatch(value.ToString(), @"^[a-zA-Z '-éèêïôàâù]+$"))
                {
                    return new ValidationResult(false, "Caractères spéciaux interdits");
                }
            }
            return ValidationResult.ValidResult;
        }
    }

    /// <summary>
    /// méthode gérant le contrôle de saisie du Commentaire
    /// </summary>
    public class CommentaireParamOfficiel : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null)
            {
                return new ValidationResult(false, "Veuillez saisir une valeur");
            }
            else
            {
                WPFParamModificationViewModel.val = true;
                if (!Regex.IsMatch(value.ToString(), @"^[a-zA-Z '-éèêïôàâù]+$"))
                {
                    return new ValidationResult(false, "Caractères spéciaux interdits");
                }
            }
            return ValidationResult.ValidResult;
        }
    }
    public class DateParamOfficiel : ValidationRule
    {


        public DateTime EarlyDate { get; set; }


        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            DateTime date = new DateTime();
            if (!DateTime.TryParse(((DateTime)value).ToString(), out date))
            {
                return new ValidationResult(false, "Format de date invalide");
            }
            else
            {
                if (date.CompareTo(this.EarlyDate) < 0)
                {
                    return new ValidationResult(false, "la date ne peut pas précédée la date de réference");
                }
                return new ValidationResult(true, null);
            }
        }
    }
}
    #endregion





       

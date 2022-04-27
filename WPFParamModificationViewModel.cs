using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassGetMSReferences.Views;
using System.Windows;
using GalaSoft.MvvmLight.CommandWpf;
using ClassGetMS.Services;
using MvvmDialogs;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Data;
using ClassGetMS;
using System.Data.Common;
using ClassGetMS.Models;
using System.Data.Entity;
using System.Data.Services;
using System.Windows.Controls;
using System.Globalization;
using System.Text.RegularExpressions;



namespace ClassGetMSReferences.ViewModel
{
    public class WPFParamModificationViewModel : ViewModelBase, IModalDialogViewModel
    {
        private IDialogService _dialogService;
        private IDataService _dataService;
        public StrucParam ParamGlobaux;
        public ParamOfficiel SelectedParamOfficiel;
        public List<ParamOfficiel> ListeViewParamOfficiels { get; set; }
        public bool checkCode;

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
        //Declaration d'un booleen gerant si l'on peut sortir sans message d'avertissement
        //A chaque modification d'un attribut , on le passe a faux


       
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

        #region Constructeurs

        private WPFParamModificationViewModel()
        {
            paramOff = new ParamOfficiel();
        }

        public WPFParamModificationViewModel(IDataService dataService, IDialogService dialogService)
        {
            this._dataService = dataService;
            this._dialogService = dialogService;
           
            QuitterCommand = new RelayCommand(() => Close = true);
            LoadedCommand = new RelayCommand(() => Initialize());
            CbxBase = new RelayCommand(() => ComboBase());
            CbxObligatoire = new RelayCommand(() => ComboObligatoire());
            CbxSupprimer = new RelayCommand(() => ComboSupprimer());
            ValiderAjoutModifCommand = new RelayCommand(() =>SaveParamOfficiel());

        }

        #endregion


        #region Declaration RelayCommand

        public RelayCommand QuitterCommand { get; set; }
        public RelayCommand LoadedCommand { get; private set; }
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

        private bool ComboSupprimer()
        {
            return Supprimer;
        }

        private bool ComboObligatoire()
        {
            return Obligatoire;
        }

        private bool ComboBase()
        {
            return Base;
        }

        // Ajout et modif d'un paramètre
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

            bool exist =(NewParamOfficiel.Nom != SelectedParamOfficiel.Nom || NewParamOfficiel.DateEntréeVigueur != SelectedParamOfficiel.DateEntréeVigueur ? true : false) ;

            if (checkCode  && exist)
                _dataService.StoreParamOfficiel(NewParamOfficiel);
            else
                _dataService.UpdateParametreOfficiel(NewParamOfficiel);
            Close = true;
                       
        }

        public void ActualiseListView()
        {
            oConnection = new SqlConnection(_dataService.ParamGlobaux.ConnectionString);
            ListeViewParamOfficiels = _dataService.GetParametreOfficiel();

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
            
            Nom = SelectedParamOfficiel.Nom;
            Type = SelectedParamOfficiel.Type;
            Valeur = SelectedParamOfficiel.Valeur;
            Commentaire = SelectedParamOfficiel.Commentaire;
            Base = SelectedParamOfficiel.Base;
            DateEntréeVigueur = SelectedParamOfficiel.DateEntréeVigueur;
            Obligatoire = SelectedParamOfficiel.Obligatoire;
            Supprimer = SelectedParamOfficiel.Supprimer;


            ////Ajout d'un paramètre partant de rien 
            //WPFParamModificationViewModel NouvelleLigne = new WPFParamModificationViewModel();
          
            //Nom = NouvelleLigne.Nom;
            //Type = NouvelleLigne.Type;
            //Valeur = NouvelleLigne.Valeur;
            //Commentaire = NouvelleLigne.Commentaire;
            //Base = NouvelleLigne.Base;
            //DateEntréeVigueur = NouvelleLigne.DateEntréeVigueur;
            //Obligatoire = NouvelleLigne.Obligatoire;
            //Supprimer = NouvelleLigne.Supprimer;
            

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

        private ParamOfficiel _paramOff;
        //création d'une variable de type ObservableObject (qui se remplira au fur et à mesure de la saisie des informations)
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




        #endregion

        #region Constructeur
        /// <summary>
        /// Classe dont la fonction est de définir les objets qui devront être observés.
        /// Permet ainsi de faire le lien avec la page WPF par l'intermédiaire du Binding.
        /// Necessite l'implémentation de ObservableObject
        /// </summary>
        //public class ParamOfficielObservable : ObservableObject
        //{

        //    /// <summary>
        //    /// constructeur de la classe
        //    /// </summary>
        //    /// <param name="Nom"></param>
        //    /// <param name="type"></param>
        //    /// <param name="valeur"></param>
        //    /// <param name="commentaire"></param>
        //    /// <param name="base"></param>
        //    /// <param name="obligatoire"></param>
        //    /// <param name="supprimer"></param>
        //    ///  
        //    public ParamOfficielObservable(string Nom, string Type, string Valeur, string Commentaire, bool Base, DateTime DateEntréeVigueur, bool Obligatoire, bool Supprimer)
        //    {
        //        this.Nom = Nom;
        //        this.Type = Type;
        //        this.Valeur = Valeur;
        //        this.Commentaire = Commentaire;
        //        this.Base = Base;
        //        this.DateEntréeVigueur = DateEntréeVigueur;
        //        this.Obligatoire = Obligatoire;
        //        this.Supprimer = Supprimer;
        //        this.AllowExit = true;
        //    }
        //    public ParamOfficielObservable (ParamOfficiel paramOff)
        //    {
        //        this.Nom = paramOff.Nom;
        //        this.Type = paramOff.Type;
        //        this.Valeur = paramOff.Valeur;
        //        this.Commentaire = paramOff.Commentaire;
        //        this.Base = paramOff.Base;
        //        this.DateEntréeVigueur = paramOff.DateEntréeVigueur;
        //        this.Obligatoire = paramOff.Obligatoire;
        //        this.Supprimer = paramOff.Supprimer;
        //        this.AllowExit = true;
        //    }
        //    /// <summary>
        //    /// méthode de conversion d'un ObjetObservable en Param
        //    /// <param name="ObjetObservable"></param>
        //    /// <returns></returns>

        //    public static ParamOfficiel ConversionEnParamOfficiel(ParamOfficielObservable ObjetObservable)
        //    {
        //        //créons un nouveau paramètre
        //        ParamOfficiel paramOff = new ParamOfficiel();

        //        //definissons ce paramètre
        //        paramOff.Nom = ObjetObservable.Nom;
        //        paramOff.Type = ObjetObservable.Type;
        //        paramOff.Valeur = ObjetObservable.Valeur;
        //        paramOff.Commentaire = ObjetObservable.Commentaire;
        //        paramOff.Base = false;
        //        paramOff.DateEntréeVigueur = ObjetObservable.DateEntréeVigueur;
        //        paramOff.Obligatoire = false;
        //        paramOff.Supprimer = false;

        //        return paramOff;
        //    }
        //    //declaration d'un constructeur vide
        //    public ParamOfficielObservable()
        //    {
        //        Nom = "";
        //        Type = "";
        //        Valeur = "";
        //        Commentaire = "";
        //        Base = false;
        //        DateEntréeVigueur = DateTime.Now;
        //        Obligatoire = false;
        //        Supprimer = false;
        //        AllowExit = true;

        //    }




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
                WPFLieuxViewModel.val = true;
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
                WPFParametresViewModel.val = true;
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
                WPFParametresViewModel.val = true;
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
                WPFParametresViewModel.val = true;
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
                return new ValidationResult(false, "Invalid date format");
            }
            else
            {
                if (date.CompareTo(this.EarlyDate) < 0)// set the logical to validate
                {
                    return new ValidationResult(false, "the date can not be before than admit date");
                }
                return new ValidationResult(true, null);
            }
        }
    }
}
    #endregion





       

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DocDirect.Commands;
using DocDirect.ViewModel.Interface;
using DocDirect.Navigator.Navigation;

namespace DocDirect.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Constant
        public static readonly string _filesListViewModelAlias = "FilesListPageVM";
        public static readonly string _aboutViewModelAlias = "AboutViewPageVM";
        public static readonly string _clientInformationModelAlias = "ClientInformationPageVM";

        private readonly IViewModelsResolver _resolver;
        #endregion Constant End

        #region Filds
        private ICommand _goToFilesListPage;
        private ICommand _goAboutViewPage;
        private ICommand _goClientInforamation;

        private INotifyPropertyChanged _filesListChangedViewModel;
        private INotifyPropertyChanged _aboutViewChangedViewModel;
        private INotifyPropertyChanged _clientInforamationChangedViewModel;
        #endregion

        #region Properties   
        public ICommand GoToFilesList
        {
            get { return _goToFilesListPage; }
            set 
            {
                _goToFilesListPage = value;
                OnPropertyChanged("GoToFilesList");
            }
        }
        public ICommand GoAboutView
        {
            get { return _goAboutViewPage; }
            set
            {
                _goAboutViewPage = value;
                OnPropertyChanged("GoAboutView");
            }
        }
        public ICommand GoClientInforamation
        {
            get { return _goClientInforamation; }
            set
            {
                _goClientInforamation = value;
                OnPropertyChanged("GoClientInforamation");
            }
        }

        public INotifyPropertyChanged FilesList
        {
            get { return _filesListChangedViewModel; }
        }
        public INotifyPropertyChanged AboutView
        {
            get { return _aboutViewChangedViewModel; }
        }
        public INotifyPropertyChanged ClientInforamation
        {
            get { return _clientInforamationChangedViewModel; }
        }
        #endregion

        #region Constructors 
        public MainViewModel(IViewModelsResolver resolver)
        {
            _resolver = resolver;

            InitializeNotifyPropertyChanged();
            InitializeCommands();

            GoToFilesList.Execute(null);
        }
        private void InitializeNotifyPropertyChanged()
        {
            _filesListChangedViewModel = _resolver.GetViewModelInstance(_filesListViewModelAlias);
            _aboutViewChangedViewModel = _resolver.GetViewModelInstance(_aboutViewModelAlias);
            _clientInforamationChangedViewModel = _resolver.GetViewModelInstance(_clientInformationModelAlias);
        }
        private void InitializeCommands()
        {
            GoToFilesList = new RelayCommand(param => this.GoToFilesListPageCommandExecute());
            GoAboutView = new RelayCommand(param => this.GoToAboutViewPageCommandExecute());
            GoClientInforamation = new RelayCommand(param => this.GoToClientInformationPageCommandExecute());
        }
        #endregion 

        private void GoToFilesListPageCommandExecute()
        {
            Navigation.Navigate(Navigation._filesListAlias, FilesList);
        }

        private void GoToAboutViewPageCommandExecute()
        {
            Navigation.Navigate(Navigation._aboutViewAlias, AboutView);
        }

        private void GoToClientInformationPageCommandExecute()
        {
            Navigation.Navigate(Navigation._clientInforamationViewAlias, ClientInforamation);
        }
    }
}

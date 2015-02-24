using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DocDirect.ViewModel.Interface;
using DocDirect.Commands;
using DocDirect.Navigator.Navigation;

namespace DocDirect.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Constant
        public static readonly string m_HomeViewModelAlias = "HomePageVM";
        public static readonly string m_CollectionViewModelAlias = "CollectionPageVM";

        private readonly IViewModelsResolver m_resolver;
        #endregion

        #region Filds
        private ICommand _goToHomePage;
        private ICommand _goToCollectionPage;

        private INotifyPropertyChanged _homeChangeViewModel;
        private INotifyPropertyChanged _collectionChangeViewModel;
        #endregion

        #region Properties
        public ICommand GoToHomePage
        {
            get { return _goToHomePage; }
            set
            {
                _goToHomePage = value;
                OnPropertyChanged("GoToHomePage");
            }
        }
        public ICommand GoToCollectionPage
        {
            get { return _goToCollectionPage; }
            set
            {
                _goToCollectionPage = value;
                OnPropertyChanged("GoToCollectionPage");
            }
        }
        
        public INotifyPropertyChanged HomeViewModel
        {
            get { return _homeChangeViewModel; }
        }
        public INotifyPropertyChanged CollectionViewModel
        {
            get { return _collectionChangeViewModel; }
        }
        #endregion

        #region Constructors 
        public MainViewModel(IViewModelsResolver resolver)
        {
            m_resolver = resolver;

            _homeChangeViewModel = m_resolver.GetViewModelInstance(m_HomeViewModelAlias);
            _collectionChangeViewModel = m_resolver.GetViewModelInstance(m_CollectionViewModelAlias);

            InitializeCommands();

            GoToHomePage.Execute(null);
        }
        private void InitializeCommands()
        {
            GoToHomePage = new RelayCommand(param => this.GoToHomePageCommandExecute());
            GoToCollectionPage = new RelayCommand(param => this.GoToCollectionPageCommandExecute());
        }
        #endregion 

        private void GoToHomePageCommandExecute()
        {
            Navigation.Navigate(Navigation.m_HomePageAlias, HomeViewModel);
        }

        private void GoToCollectionPageCommandExecute()
        {
            Navigation.Navigate(Navigation.m_CollectionPageAlias, CollectionViewModel);
        }

    }
}

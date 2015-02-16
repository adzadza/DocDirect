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
        private ICommand m_goToHomePage;
        private ICommand m_goToCollectionPage;
        private ICommand m_CloseWindowCommand;

        private INotifyPropertyChanged m_homeChangeViewModel;
        private INotifyPropertyChanged m_collectionChangeViewModel;
        #endregion

        #region Properties
        public ICommand GoToHomePage
        {
            get { return m_goToHomePage; }
            set
            {
                m_goToHomePage = value;
                OnPropertyChanged("GoToHomePage");
            }
        }
        public ICommand GoToCollectionPage
        {
            get { return m_goToCollectionPage; }
            set
            {
                m_goToCollectionPage = value;
                OnPropertyChanged("GoToCollectionPage");
            }
        }
        public ICommand CloseWindowCommand
        {
            get { return m_CloseWindowCommand; }
            set
            {
                m_CloseWindowCommand = value;
                OnPropertyChanged("CloseWindowCommand");
            }
        }

        public INotifyPropertyChanged HomeViewModel
        {
            get { return m_homeChangeViewModel; }
        }
        public INotifyPropertyChanged CollectionViewModel
        {
            get { return m_collectionChangeViewModel; }
        }
        #endregion

        #region Constructors 
        public MainViewModel(IViewModelsResolver resolver)
        {
            m_resolver = resolver;

            m_homeChangeViewModel = m_resolver.GetViewModelInstance(m_HomeViewModelAlias);
            m_collectionChangeViewModel = m_resolver.GetViewModelInstance(m_CollectionViewModelAlias);

            InitializeCommands();
        }
        private void InitializeCommands()
        {
            GoToHomePage = new RelayCommand<INotifyPropertyChanged>(GoToHomePageCommandExecute);
            GoToCollectionPage = new RelayCommand<INotifyPropertyChanged>(GoToCollectionPageCommandExecute);
            //CloseWindowCommand = new RelayCommand<>(CloseWindowCommand);
        }
        #endregion 

        private void GoToHomePageCommandExecute(INotifyPropertyChanged viewModel)
        {
            Navigation.Navigate(Navigation.m_HomePageAlias, HomeViewModel);
        }

        private void GoToCollectionPageCommandExecute(INotifyPropertyChanged viewModel)
        {
            Navigation.Navigate(Navigation.m_CollectionPageAlias, CollectionViewModel);
        }

        private void CloseWindowCommand(object target, ExecutedRoutedEventArgs e)
        {
            //SystemCommands.CloseWindow(this);
        }
    }
}

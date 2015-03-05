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

        private readonly IViewModelsResolver _resolver;
        #endregion Constant End

        #region Filds
        private ICommand _goToFilesListPage;
        private INotifyPropertyChanged _filesListChangedViewModel;
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

        INotifyPropertyChanged FilesList
        {
            get { return _filesListChangedViewModel; }
        }
        #endregion

        #region Constructors 
        public MainViewModel(IViewModelsResolver resolver)
        {
            _resolver = resolver;

            _filesListChangedViewModel = _resolver.GetViewModelInstance(_filesListViewModelAlias);

            InitializeCommands();

            GoToFilesList.Execute(null);
        }
        private void InitializeCommands()
        {
            GoToFilesList = new RelayCommand(param => this.GoToFilesListPageCommandExecute());
        }
        #endregion 

        private void GoToFilesListPageCommandExecute()
        {
            Navigation.Navigate(Navigation._filesListAlias, FilesList);
        }

    }
}

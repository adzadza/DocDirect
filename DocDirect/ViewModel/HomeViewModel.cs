using System.ComponentModel;
using System.Runtime.CompilerServices;
using DocDirect.ViewModel.Interface;
using DocDirect.Commands;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using System;
using Windows.Storage;

namespace DocDirect.ViewModel
{
    class HomeViewModel : ViewModelBase
    {
        #region Fields
        private NotifyTaskCompletion<ObservableCollection<FileViewModel>> _filesList { get; set; }
        #endregion

        #region Constructor
        public HomeViewModel()
        {
            //InitializetionCommand = AsyncCommand.Create(token => Service.GetFilesAsync());
            Initialization = InitializeAsync();
            Debugger.Break();
        }
        private async Task InitializeAsync()
        {
            _filesList = new NotifyTaskCompletion<ObservableCollection<FileViewModel>>(Service.GetFilesAsync());

        }
        #endregion

        #region Property
        //public NotifyTaskCompletion<ObservableCollection<FileViewModel>> UrlByteCount { get; private set; }
        //public IAsyncCommand InitializetionCommand { get; private set; }
        public Task Initialization { get; private set; }
        public NotifyTaskCompletion<ObservableCollection<FileViewModel>> FilesList
        { 
            get { return _filesList; }
            set {
                _filesList = value;
                OnPropertyChanged("FilesList");
            }
        }
        #endregion
    }

    public static class Service
    {
        public static async Task<ObservableCollection<FileViewModel>> 
            GetFilesAsync()
        {
            Debugger.Break();
            
            ObservableCollection<FileViewModel> filesList = new ObservableCollection<FileViewModel>();

            StorageFolder files = await StorageFolder.GetFolderFromPathAsync(@"D:\Doc");

            foreach (StorageFile file in await files.GetFilesAsync())
            {
                var PropertiesFile = await file.GetBasicPropertiesAsync();

                FileModel p = new FileModel(
                    file.DisplayName,
                    file.Path,
                    PropertiesFile.Size);

                filesList.Add(new FileViewModel(p));
            }
            return filesList;
        }
    
    }
}

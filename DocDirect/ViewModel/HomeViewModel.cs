using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using System.Linq.Expressions;
using System.ComponentModel;
using DocDirect.ViewModel.Interface;
using DocDirect.Commands;
using System.Threading;

namespace DocDirect.ViewModel
{
    class HomeViewModel : ViewModelBase
    {
        #region Fields
        private ObservableCollection<FileViewModel> _filesList { get; set; }
        #endregion

        #region Constructor
        public HomeViewModel()
        {
            InitializetionCommand = new AsyncCommand(async () =>
            {
                FilesList = await Service.GetFilesAsync();
            });

            Debugger.Break();
        }
        #endregion

        #region Property
        public IAsyncCommand InitializetionCommand { get; private set; }
        public ObservableCollection<FileViewModel> FilesList { 
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
        public static async Task<ObservableCollection<FileViewModel>> GetFilesAsync(CancellationToken token = new CancellationToken())
        {
            Debugger.Break();

            await Task.Delay(TimeSpan.FromSeconds(3), token).ConfigureAwait(false);

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

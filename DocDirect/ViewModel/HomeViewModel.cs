using DocDirect.Commands;
using System.Collections.ObjectModel;
using System.IO;
using System;
using System.Windows.Input;
using System.Diagnostics;

namespace DocDirect.ViewModel
{
    class HomeViewModel : ViewModelBase
    {
        #region Fields
        private ObservableCollection<FileViewModel> _filesList;
        #endregion

        #region Constructor
        public HomeViewModel()
        {
            DirectoryInfo directory = new DirectoryInfo(@"D:\Doc");

            _filesList = new ObservableCollection<FileViewModel>();
            try
            {
                foreach (var file in directory.EnumerateFiles("*", SearchOption.AllDirectories))
                {
                    var modelFile = new FileModel(
                        file.Name,
                        file.FullName,
                        file.Length);

                    _filesList.Add(new FileViewModel(modelFile));
                }
            }
            catch (Exception ex)
            {
                Debugger.Log(1, "Error", ex.Message);
            }
        }
        #endregion

        #region Property
        public ObservableCollection<FileViewModel> FilesList
        { 
            get { return _filesList; }
            set {
                _filesList = value;
                OnPropertyChanged("FilesList");
            }
        }
        #endregion

        #region Commands
        //RelayCommand _selectedItem;
        #endregion
    }
}

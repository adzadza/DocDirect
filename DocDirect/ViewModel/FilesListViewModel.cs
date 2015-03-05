using DocDirect.Commands;
using System.Collections.ObjectModel;
using System.IO;
using System;
using System.Windows.Input;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace DocDirect.ViewModel
{
    class FilesListViewModel : ViewModelBase
    {
        #region Fields
        private ObservableCollection<FileViewModel> _filesList;
        #endregion

        #region Constructor
        public FilesListViewModel()
        {
            _filesList = new ObservableCollection<FileViewModel>();
            _filesList = GetFiles();
        }

        private void InitialisCommands()
        {
            //_thumbnailViewCommand = new RelayCommand(ViewThumbnailListFiles);
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
        
        public ICommand ThumbnailViewCommand
        {
            get { return _thumbnailViewCommand; }
            set
            {
                _thumbnailViewCommand = value;
                OnPropertyChanged("ThumbnailViewCommand");
            }
        }
        #endregion

        #region Commands
        private ICommand _thumbnailViewCommand;

        #endregion

        #region Method
        private string GetFileType(string fileType)
        {
            if (isImage(fileType)) return "Image";
            else if (isDocument(fileType)) return "Docement";

            if (isVideo(fileType)) return "Video";
            else if (isMusic(fileType)) return "Music";

            return "Other";
        }

        private bool isImage(string fileType)
        {
            string pattern = "(.jpg)|(.png)";
            return Regex.IsMatch(fileType, pattern, RegexOptions.IgnoreCase);
        }
        private bool isDocument(string fileType)
        {
            string pattern = "(.doc)|(.docx)";
            return Regex.IsMatch(fileType, pattern, RegexOptions.IgnoreCase);
        }
        private bool isVideo(string fileType)
        {
            string pattern = "(.avi)";
            return Regex.IsMatch(fileType, pattern, RegexOptions.IgnoreCase);
        }
        private bool isMusic(string fileType)
        {
            string pattern = "(.mp3)|(.flac)";
            return Regex.IsMatch(fileType, pattern, RegexOptions.IgnoreCase);
        }

        private ObservableCollection<FileViewModel> GetFiles()
        {
            DirectoryInfo directory = new DirectoryInfo(@"D:\Doc");

            ObservableCollection<FileViewModel> filesList = new ObservableCollection<FileViewModel>();
            try
            {
                foreach (var file in directory.EnumerateFiles("*", SearchOption.AllDirectories))
                {
                    var modelFile = new FileModel(
                        file.Name,
                        file.FullName,
                        file.Length,
                        GetFileType(file.Extension)
                    );

                    filesList.Add(new FileViewModel(modelFile));
                }
            }
            catch (Exception ex)
            {
                Debugger.Log(1, "Error", ex.Message);
            }

            return filesList;
        }
        
        #endregion
    }
}

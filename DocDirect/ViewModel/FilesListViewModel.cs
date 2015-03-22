using DocDirect.Commands;
using System.Collections.ObjectModel;
using System.IO;
using System;
using System.Windows.Input;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using Microsoft.Practices.Composite.Presentation.Commands;
using System.Windows;
using System.Collections.Specialized;
using Windows.Storage;
using Windows.Networking.Sockets;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Storage.Streams;
using DocDirect.Commands.AsyncCommand;
using System.Threading;

namespace DocDirect.ViewModel
{
    class FilesListViewModel : ViewModelBase
    {
        #region Fields
        private ObservableCollection<FileViewModel> _filesList = new ObservableCollection<FileViewModel>();
        private string _sizeSelectedFile;
        private ulong  _countItem = 0;

        private String _port = "6505";
        private String _workFolderName = @"C:\Doc";
        private StreamSocketListener _tcpListener;
        private StorageFolder _workFolder;
        private FileViewModel _selectedFile;
        private uint _sizeBufer = 1024;
        #endregion

        #region Constructor
        public FilesListViewModel()
        {
            InitialisCommands();

            _filesList = GetFiles();
        }

        private void InitialisCommands()
        {
            this.SelectedFileCommand = new RelayCommand(obj => this.SetCommandSelected(obj));

            this.OpenFileCommand = new RelayCommand(param => this.OpenFile(param));
            this.RemoveFileCommand = new RelayCommand(param => this.RemoveFile(param));
            this.CopyFileCommand = new RelayCommand(param => this.CopyFile(param));
            this.PastFileCommand = new RelayCommand(param => this.PastFile());

            //this.SendFileCommand = new RelayCommand(param => this.SendFileToClient(param));

            this.SendFileCommandAsync = AsyncCommand.Create(token => this.SendFileToClient(CurrentSelectedFile, token));
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
        public string SelectedFileSize
        {
            get { return _sizeSelectedFile; }
            set
            {
                if(_sizeSelectedFile!=value)
                {
                    _sizeSelectedFile = value;
                    OnPropertyChanged("CurrentSelectedFile");
                }
            }
        }
        public FileViewModel CurrentSelectedFile
        {
            get { return _selectedFile; }
            set {
                _selectedFile = value;
                OnPropertyChanged("CurrentSelectedFile");
            }
        }
        public ulong CountItem
        {
            get { return _countItem; }
            set { 
                _countItem = value;
                OnPropertyChanged("CountItem");
            }
        }        
        #endregion

        #region Commands
        public ICommand SelectedFileCommand { get; set;}

        public ICommand OpenFileCommand { get; private set; }
        public ICommand RemoveFileCommand { get; private set; }
        public ICommand CopyFileCommand { get; private set; }
        public ICommand PastFileCommand { get; private set; }
        public ICommand SendFileCommand { get; private set; }

        public IAsyncCommand SendFileCommandAsync { get; private set; }
        #endregion

        #region Method
        private string GetFileType(string fileType)
        {
            if (isImage(fileType)) return "Image";
            else if (isDocument(fileType)) return "Document";

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
            string pattern = "(.doc)|(.docx)|(.pdf)|(.xls)|(.xlsx)|(.txt)";
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
            DirectoryInfo directory = new DirectoryInfo(_workFolderName);
            ObservableCollection<FileViewModel> filesList = new ObservableCollection<FileViewModel>();

            if (directory.Exists)
            {
                try
                {
                    foreach (var file in directory.EnumerateFiles("*", SearchOption.AllDirectories))
                    {
                        var modelFile = new FileModel(
                            file.Name,
                            file.FullName,
                            file.Length,
                            GetFileType(file.Extension),
                            file.LastAccessTime,
                            file.Extension
                        );
                        _countItem++;
                        filesList.Add(new FileViewModel(modelFile));
                    }
                }
                catch (Exception) {}
            }
            else
            {
                Directory.CreateDirectory(_workFolderName);
            }

            return filesList;
        }
        
        // Add one file to collection
        private FileViewModel SetFileToCollection(string path)
        {
            FileInfo file = new FileInfo(path);
            var modelFile = new FileModel(
                        file.Name,
                        file.FullName,
                        file.Length,
                        GetFileType(file.Extension),
                        file.LastAccessTime,
                        file.Extension
                    );
            return new FileViewModel(modelFile);
        }

        private string ConverterSize(long size)
        {
            if (size < 1024) return size.ToString() + " KBit";
            else
                if (size > 1024 && size < 1048576) return (size / 1024).ToString() + " KB";
                else
                    if (size > 1048576 && size < 1073741824) return (size / 1048576).ToString() + " MB";
                    else
                        return (size / 1073741824).ToString() + " GB";
        }       
        private string GetNameFile(string path)
        {
            string[] item = path.Split('\\');
            return item[item.Length - 1];
        }

        private void DeleteItemObservableCollection(string nameFile)
        {
            foreach (var item in _filesList)
            {
                if (item.Name == nameFile)
                {
                    _filesList.Remove(item);

                    --CountItem;
                    break;
                }                
            }
        }
        #endregion

        #region Handler Command
        private void OpenFile(object param)
        {
            if (param is FileViewModel) 
            { 
                FileViewModel file = param as FileViewModel;
            
                if (System.IO.File.Exists(file.Path))            
                    Process.Start(file.Path);
                else
                {
                    DialogBoxInfo dlg = new DialogBoxInfo("This file does not exist!", "Inforamation");
                    dlg.ShowDialog();
                }
            }
        }
        private void RemoveFile(object param)
        {
            if (param is FileViewModel)
            {
                FileViewModel file = param as FileViewModel;

                if (System.IO.File.Exists(file.Path))
                {
                    DialogBoxInfo dlg = new DialogBoxInfo("Really want to delete the file?", "Question");
                    dlg.ShowDialog();

                    if (dlg.DialogResult == true)
                    {
                        try
                        {
                            System.IO.File.Delete(file.Path);
                            DeleteItemObservableCollection(file.Name);
                        }
                        catch (IOException)
                        {
                            DialogBoxInfo dlgError = new DialogBoxInfo("This file can not be deleted!", "Error");
                            dlg.ShowDialog();
                        }
                        catch (Exception) { }
                    }
                }
            }
        }
        private void CopyFile(object param)
        {
            FileViewModel file = param as FileViewModel;

            if (file != null) 
            { 
                StringCollection FileCollection = new StringCollection();
                FileCollection.Add(file.Path);
                Clipboard.SetFileDropList(FileCollection);
            }
        }
        private void PastFile()
        {
            StringCollection fileCollection = new StringCollection();
            fileCollection = Clipboard.GetFileDropList();

            if(fileCollection.Count > 0)
            {
                string local;
                foreach(var path in fileCollection)
                {
                    local = _workFolderName+ "\\" +GetNameFile(path);
                    try { 
                        File.Copy(path, local);
                        FilesList.Add(SetFileToCollection(local));
                    }
                    catch(IOException)
                    {
                        if (!local.Equals(path))
                        {
                            DialogBoxInfo dlg = new DialogBoxInfo("This file exists, replace?", "Information");
                            dlg.ShowDialog();

                            if (dlg.DialogResult == true)
                            {
                                File.Delete(local);

                                File.Copy(path, local);
                                FilesList.Add(SetFileToCollection(local));
                            }
                        }
                    }
                    catch (Exception) { }
                }
               
            }
        }
        private void SetCommandSelected(object param)
        {
            if (param != null) 
            { 
                FileViewModel file = param as FileViewModel;

                //сохраним размер, для того что бы вывести его на экран
                SelectedFileSize = "  1 item selected " + ConverterSize(file.Size);

                // и сохраним модель файла для того что бы передать в async command'у
                CurrentSelectedFile = file;
            }
        }

        private async Task SendFileToClient(object param, CancellationToken token = new CancellationToken())
        {
            if (param is FileViewModel)
            {
                FileViewModel file = param as FileViewModel;

                if (System.IO.File.Exists(file.Path))
                {
                    StorageFile fileStorage = await StorageFile.GetFileFromPathAsync(file.Path);

                    await SendFileAsync(fileStorage);
                }
            }
        }
        #endregion End Handler Command

        #region FileTransfer
        private async Task Start()
        {
            _tcpListener = new StreamSocketListener();
            _tcpListener.ConnectionReceived += OnClientConnectionReceived;
            await _tcpListener.BindServiceNameAsync(_port);
            await _tcpListener.BindEndpointAsync(new HostName("192.168.10.66"), "adzadza");

            _workFolder = await StorageFolder.GetFolderFromPathAsync(_workFolderName);
        }

        private async void OnClientConnectionReceived(StreamSocketListener listener, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            StorageFile file;
            using (StreamSocket socket = args.Socket)
            using (var rw = new DataReader(socket.InputStream))
            {
                //1. Read the filename lenght
                await rw.LoadAsync(sizeof(Int32));
                var fileNameLenght = (uint)rw.ReadInt32();
                // 2. Read the filename
                await rw.LoadAsync(fileNameLenght);
                var originalFileName = rw.ReadString(fileNameLenght);
                // 3. Read the file length
                await rw.LoadAsync(sizeof(UInt64));
                var fileLenght = rw.ReadUInt64();

                // 4. Read file
                using (var memStream = await DownloadFile(rw, fileLenght))
                {
                    file = await _workFolder.CreateFileAsync(originalFileName, CreationCollisionOption.ReplaceExisting);
                    using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        await RandomAccessStream.CopyAndCloseAsync(memStream.GetInputStreamAt(0), fileStream.GetOutputStreamAt(0));
                    }
                    rw.DetachStream();
                }
            }
        }

        public async Task SendFileAsync(StorageFile selectedFile)
        {
            await Start();

            byte[] output = new byte[_sizeBufer];
            var prop = await selectedFile.GetBasicPropertiesAsync();

            using (StreamSocket socket = new StreamSocket())
            using (var dw = new DataWriter(socket.OutputStream))
            {
                await socket.ConnectAsync(new HostName("192.167.10.33"), _port);

                // 1. Send the filename length
                dw.WriteInt32(selectedFile.Name.Length); // filename length is fixed
                // 2. Send the filename
                dw.WriteString(selectedFile.Name);
                // 3. Send the file length
                dw.WriteUInt64(prop.Size);
                // 4. Send the file
                var fileStream = await selectedFile.OpenStreamForReadAsync();
                while (fileStream.Position < (long)prop.Size)
                {
                    var rlen = await fileStream.ReadAsync(output, 0, output.Length);
                    dw.WriteBytes(output);
                }

                await dw.FlushAsync();
                await dw.StoreAsync();

                await socket.OutputStream.FlushAsync();
            }
        }
        private async Task<InMemoryRandomAccessStream> DownloadFile(DataReader rw, ulong fileLength)
        {
            var memStream = new InMemoryRandomAccessStream();

            // Download the file
            while (memStream.Position < fileLength)
            {
                //Verbose(string.Format("Receiving file...{0}/{1} bytes", memStream.Position, fileLength));
                var lenToRead = Math.Min(1024, fileLength - memStream.Position);
                await rw.LoadAsync((uint)lenToRead);
                var tempBuff = rw.ReadBuffer((uint)lenToRead);
                await memStream.WriteAsync(tempBuff);
            }

            return memStream;
        }
        #endregion
    }
}

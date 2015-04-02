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
using Windows.Networking;
using Windows.Storage.Streams;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using DocDirect.Commands.AsyncCommand;
using DocDirect.Commands;
using DocDirect.Crypto;
using System.ComponentModel;

namespace DocDirect.ViewModel
{
    class FilesListViewModel : ViewModelBase
    {
        #region Fields
        private ObservableCollection<FileViewModel> _filesList = new ObservableCollection<FileViewModel>();        
        private CryptoUtils      _cryptoUtils;
        private string           _sizeSelectedFile;
        private ulong            _countItem = 0;
        private int              _currentProgress;
        private bool             _progressVisibility=false; 
 
        private String           _workFolderName = @"C:\Doc";
        private StorageFolder    _workFolder;
        private FileViewModel    _selectedFile;
        private uint             _sizeBufer = 1024;
        private String           _port = "22112";
        private HostName         _LocalHostName  = new HostName("192.168.1.46");   // DEBUG
        private HostName         _RemoveHostName = new HostName("192.168.1.38");   // DEBUG
        #endregion

        #region Constructor
        public FilesListViewModel()
        {
            InitialisCommands();

            _filesList = GetFiles();
            _cryptoUtils = new CryptoUtils();

            ProgressVisibility = false;
        }

        private void InitialisCommands()
        {
            this.SelectedFileCommand = new RelayCommand(obj => this.SetCommandSelected(obj));

            this.OpenFileCommand = new RelayCommand(param => this.OpenFile(param));
            this.RemoveFileCommand = new RelayCommand(param => this.RemoveFile(param));
            this.CopyFileCommand = new RelayCommand(param => this.CopyFile(param));
            this.PastFileCommand = new RelayCommand(param => this.PastFile());
            
            this.SendFileCommandAsync = AsyncCommand.Create(token => this.SendFileToClient(CurrentSelectedFile, token));
            this.InitialisStreamSocketAsync = AsyncCommand.Create(tocken => this.InitialisStreamSocket(tocken));

            InitialisStreamSocketAsync.Execute(null);
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
                    OnPropertyChanged("SelectedFileSize");
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
        public int CurrentProgressDownload
        {
            get { return this._currentProgress; }
            private set
            {
                if (this._currentProgress != value)
                {
                    this._currentProgress = value;
                    this.OnPropertyChanged("CurrentProgressDownload");
                }
            }
        }
        public bool ProgressVisibility 
        {
            get { return _progressVisibility; }
            set
            {
                _progressVisibility = value;
                OnPropertyChanged("ProgressVisibility");
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
        public IAsyncCommand InitialisStreamSocketAsync { get; private set; }

        public ICommand DownloadFileProgressCommand { get; private set; }
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

        private void ReportProgress(int value)
        {
            this.CurrentProgressDownload = value;
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
                            dlgError.ShowDialog();
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

        private async Task InitialisStreamSocket(CancellationToken token = new CancellationToken())
        {
            try
            {
                StreamSocketListener Listener = new StreamSocketListener();
                Listener.ConnectionReceived += OnClientConnectionReceived;

                CoreApplication.Properties.Add("Listener", Listener);

                await Listener.BindEndpointAsync(_LocalHostName, _port);

                _workFolder = await StorageFolder.GetFolderFromPathAsync(_workFolderName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                CoreApplication.Properties.Remove("Listener");
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
                    
                    CoreApplication.Properties.Remove("FileSend");                    
                    CoreApplication.Properties.Add("FileSend", fileStorage);

                    await ExchangeKeySend();
                }
            }
        }
        #endregion End Handler Command

        #region FileTransfer
        private async void OnClientConnectionReceived(StreamSocketListener listener, 
                                                      StreamSocketListenerConnectionReceivedEventArgs args)
        {
            using (StreamSocket socket = args.Socket)
            using (var dr = new DataReader(socket.InputStream))
            {
                await dr.LoadAsync(sizeof(Byte));
                byte step = dr.ReadByte();

                switch(step)
                {
                    case (Byte)Step.SendKey:
                        await ExchangeKeyDownload(dr);
                        break;
                    case (Byte)Step.ReturnKey:
                        await ReturnKeyDownload(dr);
                        break;
                    case (Byte)Step.DownloadFileWhithHash:
                        await DownloadFileAsync(dr);
                        break;
                    case (Byte)Step.SuccessfulTransmission:
                        SuccessfulTransmission();
                        break;
                    case (Byte)Step.TransmissionError:
                        TransmissionError();
                        break;
                    default:
                        break;
                }
                dr.DetachStream();
            }
        }

        public async Task SendFileAsync(IProgress<int> progress)
        {            
            Debug.WriteLine("---> SendFileAsync <----\n");
            object outValue;
            if (CoreApplication.Properties.TryGetValue("FileSend", out outValue))
            {
                StorageFile file = outValue as StorageFile;
                byte[] hashFile = _cryptoUtils.GenerateHashFile(file);

                byte[] output = new byte[_sizeBufer];
                var prop = await file.GetBasicPropertiesAsync();

                using (StreamSocket socket = new StreamSocket())
                using (var dw = new DataWriter(socket.OutputStream))
                {
                    await socket.ConnectAsync(_RemoveHostName, _port);
                    // Write byte current step
                    dw.WriteByte((byte)Step.DownloadFileWhithHash);
                    // Send lenght hash
                    dw.WriteUInt32((UInt32)hashFile.Length);
                    // Send hash
                    dw.WriteBytes(hashFile);
                    // Send the filename length
                    dw.WriteInt32(file.Name.Length); // filename length is fixed
                    // Send the filename
                    dw.WriteString(file.Name);
                    // Send the file length
                    dw.WriteUInt64(prop.Size);
                    // Send the file
                    var fileStream = await file.OpenStreamForReadAsync();
                    ProgressVisibility = true;
                    while (fileStream.Position < (long)prop.Size)
                    {
                        // сalculate the percentage of downloaded data
                        progress.Report(Convert.ToInt32((fileStream.Position * 100) / (long)prop.Size));

                        var rlen = await fileStream.ReadAsync(output, 0, output.Length);
                        dw.WriteBytes(output);
                    }
                    ProgressVisibility = false;

                    await dw.FlushAsync();
                    await dw.StoreAsync();

                    await socket.OutputStream.FlushAsync();
                }
            }
            else
            {
                DialogBoxInfo dlg = new DialogBoxInfo("File not found!", "Error");
                dlg.ShowDialog();
            }
        }
        private async Task DownloadFileAsync(DataReader rw)
        {
            Debug.WriteLine("--->> DownloadFileAsync");
           
            //1. Get lenght hash
            await rw.LoadAsync(sizeof(UInt32));
            UInt32 lenght = rw.ReadUInt32();
            
            byte[] hashFile = new byte[lenght]; 
            //2. Download hash
            await rw.LoadAsync(lenght);
            rw.ReadBytes(hashFile);

            StorageFile file;
            //3. Read the filename lenght
            await rw.LoadAsync(sizeof(Int32));
            var fileNameLenght = (uint)rw.ReadInt32();
            // 4. Read the filename
            await rw.LoadAsync(fileNameLenght);
            var originalFileName = rw.ReadString(fileNameLenght);
            // 5. Read the file length
            await rw.LoadAsync(sizeof(UInt64));
            var fileLenght = rw.ReadUInt64();
            
            var progressIndicator = new Progress<int>(ReportProgress);
            ProgressVisibility = true;
            
            // 6. Read file
            using (var memStream = await DownloadFile(rw, fileLenght, progressIndicator))
            {
                file = await _workFolder.CreateFileAsync(originalFileName, CreationCollisionOption.ReplaceExisting);
                using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    await RandomAccessStream.CopyAndCloseAsync(memStream.GetInputStreamAt(0), fileStream.GetOutputStreamAt(0));
                }
            }
            
            ProgressVisibility = false;
            // compare the hash value
            if(_cryptoUtils.CompareHash(file, hashFile))
            {
                //DialogBoxInfo dlg = new DialogBoxInfo("File is received, the hash value identical!", "Info");
                //dlg.ShowDialog();        
                // 
                await SendSuccessfulTransmission();
            }
        }
        private async Task<InMemoryRandomAccessStream> DownloadFile(DataReader rw, ulong fileLength, IProgress<int> progress)
        {
            var memStream = new InMemoryRandomAccessStream();
            // Download the file
            while (memStream.Position < fileLength)
            {
                // сalculate the percentage of downloaded data
                progress.Report( Convert.ToInt32((memStream.Position * 100) / fileLength));

                var lenToRead = Math.Min(1024, fileLength - memStream.Position);
                await rw.LoadAsync((uint)lenToRead);
                var tempBuff = rw.ReadBuffer((uint)lenToRead);
                await memStream.WriteAsync(tempBuff);
            }

            return memStream;
        }
        private async Task ExchangeKeySend()
        {
            Debug.WriteLine("--->> ExchangeKeySend");

            using (StreamSocket socket = new StreamSocket())
            using (var dw = new DataWriter(socket.OutputStream))
            {
                await socket.ConnectAsync(_RemoveHostName, _port);

                // Write byte current step
                dw.WriteByte((byte)Step.SendKey);

                dw.WriteUInt64(_cryptoUtils.distributedKey);
                // Write open key
                dw.WriteUInt64(_cryptoUtils.PublicKey.E);
                dw.WriteUInt64(_cryptoUtils.PublicKey.N);

                await dw.FlushAsync();
                await dw.StoreAsync();

                await socket.OutputStream.FlushAsync();
            }
        }
        private async Task ExchangeKeyDownload(DataReader dr) 
        {
            Debug.WriteLine("--->> ExchangeKeyDownload");

            await dr.LoadAsync(sizeof(UInt64));
            _cryptoUtils.distributedKey = dr.ReadUInt64();

            await dr.LoadAsync(sizeof(UInt64));
            _cryptoUtils.PublicKey.E = dr.ReadUInt64();

            await dr.LoadAsync(sizeof(UInt64));
            _cryptoUtils.PublicKey.N = dr.ReadUInt64();

            dr.DetachStream();

            _cryptoUtils.GenerateSharedKey();
            //_cryptoUtils.GenerateHash();

            await ReturnKeySend();
        }
        private async Task ReturnKeySend()
        {
            Debug.WriteLine("--->> ReturnKeySend");

            using (StreamSocket socket = new StreamSocket())
            using (var dw = new DataWriter(socket.OutputStream))
            {
                await socket.ConnectAsync(_RemoveHostName, _port);

                // Write byte current step
                dw.WriteByte((byte)Step.ReturnKey);

                dw.WriteUInt64(_cryptoUtils.distributedKey);

                await dw.FlushAsync();
                await dw.StoreAsync();

                await socket.OutputStream.FlushAsync();
            }
        }
        private async Task ReturnKeyDownload(DataReader dr)
        {
            Debug.WriteLine("--->> ReturnKeyDownload");

            await dr.LoadAsync(sizeof(UInt64));
            _cryptoUtils.distributedKey = dr.ReadUInt64();
           
            dr.DetachStream();

            await SendFileAsync();
        }
        
        private async Task SendSuccessfulTransmission()
        {
            Debug.WriteLine("--->> SendSuccessfulTransmission");

            using (StreamSocket socket = new StreamSocket())
            using (var dw = new DataWriter(socket.OutputStream))
            {
                await socket.ConnectAsync(_RemoveHostName, _port);

                // Write byte current step
                dw.WriteByte((byte)Step.SuccessfulTransmission);

                await dw.FlushAsync();
                await dw.StoreAsync();

                await socket.OutputStream.FlushAsync();
            }
        }
        private void SuccessfulTransmission()
        {
            Debug.WriteLine("--->> SuccessfulTransmission");
                        
            //DialogBoxInfo dlg = new DialogBoxInfo("File has been successfully sent!", "Info");
            //dlg.ShowDialog();
        }
        private void TransmissionError()
        {
            Debug.WriteLine("--->> TransmissionError");

            DialogBoxInfo dlg = new DialogBoxInfo("Send a file failed!", "Info");
            dlg.ShowDialog();
        }
        #endregion FileTransfer
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using Windows.ApplicationModel.Core;
using Windows.Storage;

namespace DocDirect.Crypto
{
    enum Step : byte { ExchangeKey = 1, SendFileWhithHash, DownloadFileWhithHash, SuccessfulTransmission, TransmissionError };

    public class CryptoUtils
    {
        private DiffiHelman  _diffi;
        private RSA          _rsa;
        private Key          _publicKey;
        private byte[]       _hashFile;
       
        public CryptoUtils()
        {
            _diffi = new DiffiHelman();
            _rsa   = new RSA();
            _publicKey = new Key();
        }

        #region Property
        // отправляется пользователю для генерации общего ключа
        public UInt64 distributedKey
        {
            get { return _diffi.distributedKey; }
            set { _diffi.distributedKey = value; }
        }
        public UInt64 sharedKey
        {
            get { return _diffi.sharedKey; }
            set { _diffi.sharedKey = value; }
        }
        public UInt64 P
        {
            get { return _diffi.P; }
        }
        public UInt64 randKey
        {
            get { return _diffi.randKey; }
        }       
        // открытый ключ
        public Key PublicKey
        {
            get { return _publicKey; }
            set { _publicKey = value; }
        }
        public byte[] GetHashFile
        {
            get { return _hashFile; }
            set { _hashFile = value; }
        }
        #endregion

        #region Method
        public void GenerateHash()
        {
            StorageFile _file;
            object outValue;
            if (CoreApplication.Properties.TryGetValue("FileSend", out outValue))
            {
                _file = (StorageFile)outValue;

                _hashFile = GetMD5Hash(_file);
            }
        }
        public void GenerateGeneralKey()
        {
            // вычисляем общий ключ
            sharedKey = DiffiHelman.PowMod(distributedKey, randKey, P);
        }

        public void CompareHash(StorageFile file)
        {
            byte[] tmpNewHashFile = GetMD5Hash(file);

            bool bEqual = true;
            if (tmpNewHashFile.Length == _hashFile.Length)
            {
                for(int i=0; i<_hashFile.Length-1; i++)
                {
                    if (tmpNewHashFile[i] != _hashFile[i])
                    {
                        bEqual = false;
                        break;
                    }
                }
            }
            if(bEqual)
            {
                DialogBoxInfo dlg = new DialogBoxInfo("File is received, the hash value identical!", "Info");
                dlg.ShowDialog();
            }
        }
        private byte[] GetMD5Hash(StorageFile file)
        {
            RIPEMD160 myRIPEMD160 = RIPEMD160Managed.Create();

            FileStream fileStream = new FileStream(file.Path, FileMode.Open);
            // Be sure it's positioned to the beginning of the stream.
            fileStream.Position = 0;
            // Compute the hash of the fileStream.
            byte[] hash = myRIPEMD160.ComputeHash(fileStream);

            fileStream.Close();
            return hash;
        }
        #endregion
    }

    class DiffiHelman
    {
        private UInt64 _p = 11, _a = 7;  // От 0 до 18 446 744 073 709 551 615
        private UInt64 _randKey = 3;     // X
        private UInt64 _distributedKey;  // Y
        private UInt64 _sharedKey;       // z

        #region Constructor
        public DiffiHelman()
        {
            //Random rand = new Random();
            //_x = Convert.ToUInt64(rand.Next()%11);
            _distributedKey = PowMod(A, randKey, P);
        }
        #endregion
        
        #region Property
        public UInt64 P
        {
            get { return _p; }
        }
        public UInt64 A
        {
            get { return _a; }
        }
        public UInt64 randKey
        {
            get { return _randKey; }
            private set { _randKey = value; }
        }
        public UInt64 distributedKey
        {
            get { return _distributedKey; }
            set { _distributedKey = value; }
        }
        public UInt64 sharedKey
        {
            get { return _sharedKey; }
            set { _sharedKey = value; }
        }
        #endregion

        #region Method
        // Быстрое возведение в степень d по модулю числа n
        public static UInt64 PowMod(UInt64 x, UInt64 d, UInt64 n)
        {
            UInt64 y = 1;
            while (d > 0)
            {
                if ((d % 2) != 0)
                {
                    y = (y * x) % n;
                    d = d - 1;
                }
                d /= 2;
                x = (x * x) % n;
            }
            return y;
        }
        #endregion
    }

    class RSA
    {
        private UInt64 p, q, e, d, n;        
        #region Constructor
        public RSA()
        {
            p = 3557;
            q = 2579;
            n = p * q;
            e = 3;

            // Получаем дешифрующию экспоненту
            d = Inverse(e, (p - 1) * (q - 1));
        }
        #endregion

        #region Property
        public Key PublicKey
        {
            get { return new Key(e, n); }
        }
        public Key PrivateKey
        {
            get { return new Key(d, n); }
        }        
        #endregion

        #region Method
        // Нахождение мультипликативно обратного
        public UInt64 Inverse(UInt64 a, UInt64 b)
        {
            Ternary q = Evclid_Ex(a, b);
            if (q.d == 1)
            {
                if (q.x < 0) q.x = (q.x + b) % b;
                return q.x;
            }
            return 0;
        }
        // Расширенный алгоритм Евклида 
        public Ternary Evclid_Ex(UInt64 a, UInt64 b)
        {
            UInt64 q, r, x1 = 0, x2 = 1, y1 = 1, y2 = 0, x, y;
            if (b == 0)
                return new Ternary(a, 1, 0);

            while (b > 0)
            {
                q = a / b; r = a - q * b;
                x = x2 - q * x1;
                y = y2 - q * y1;
                a = b; b = r;
                x2 = x1; x1 = x; y2 = y1; y1 = y;
            }
            return new Ternary(x2, y2, a);
        }
        #endregion
        public class Ternary
        {
            public UInt64 x=0, y=0, d=0;
            public Ternary(UInt64 X, UInt64 Y, UInt64 D)
            {
                x = X; y = Y; d = D;
            }
        }        
    }

    public class Key
    {
        private UInt64 _e, _n;

        public Key()
        {
            _e = 0;
            _n = 0;
        }
        public Key(UInt64 e, UInt64 n)
        {
            _e = e;
            _n = n;
        }

        public UInt64 E
        {
            get { return _e; }
            set { _e = value; }
        }
        public UInt64 N
        {
            get { return _n; }
            set { _n = value; }
        }
    }
}


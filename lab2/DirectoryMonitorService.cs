
using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.ServiceProcess;
using System.Text;

namespace DirectoryMonitorService
{
    public partial class DirectoryMonitorService : ServiceBase
    {
        FileSystemWatcher fileSystemWatcher;
        private string compressFile;
        private string sourceFile;
        private string targetPath;
        private string fileText;

        public DirectoryMonitorService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            fileSystemWatcher = new FileSystemWatcher("A:\\SourceDirectory")
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = true
            };
            fileSystemWatcher.Created += DirectoryChanged;
            fileSystemWatcher.Deleted += DirectoryChanged;
            fileSystemWatcher.Renamed += FileRenamed;
            fileSystemWatcher.Changed += DirectoryChanged;
        }
        private void FileRenamed(object sender, RenamedEventArgs renamedEvent)
        {
            sourceFile = renamedEvent.FullPath;
            compressFile = renamedEvent.FullPath.Substring(0, renamedEvent.FullPath.IndexOf('.')) + ".rar";
            EncryptFileText(sourceFile);
            Compress(sourceFile, compressFile);
            File.Delete(sourceFile);

            targetPath = CreateArchieve() + compressFile.Substring(compressFile.IndexOf('y') + 2);
            File.Copy(compressFile, targetPath);

            Decompress(targetPath, targetPath.Substring(0, targetPath.IndexOf(".rar")) + ".txt");
            DecryptFileText(targetPath.Substring(0, targetPath.IndexOf(".rar")) + ".txt");
            File.Delete(targetPath);
        }
        public string CreateArchieve()
        {
            DateTime date = DateTime.Now;
            string filepath = "A:\\TargetDirectory\\" +  "archive" + date.ToString("D");
            if(!Directory.Exists(filepath))
            {
                Directory.CreateDirectory(filepath);
            }
            return filepath + "\\";
        }
        public void EncryptFileText(string filePath)
        {
            byte[] encryptedData;
            using (StreamReader reader = new StreamReader(filePath))
            {
                fileText = reader.ReadToEnd();
            }
            using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
            {
                encryptedData = RSAEncrypt(fileText, RSA.ExportParameters(false), false);
            }
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine(encryptedData);
            }
        }
        public void DecryptFileText(string filePath)
        {
            byte[] decryptedData;
            using (StreamWriter sr = new StreamWriter(filePath))
            {
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    decryptedData = RSADecrypt(fileText, RSA.ExportParameters(false), false);
                }
                sr.WriteLine(fileText);
            }
        }
        private void DirectoryChanged(object sender, FileSystemEventArgs renamedEvent)
        {           
            var message = $"{renamedEvent.ChangeType} - {renamedEvent.FullPath} - {DateTime.Now}{Environment.NewLine}";
            File.AppendAllText(@"A:\TargetDirectory\log.txt", message);
        }
        public static void Compress(string sourceFile, string compressedFile)
        {
            using (FileStream sourceStream = new FileStream(sourceFile, FileMode.Open))
            {
                using (FileStream targetStream = File.Create(compressedFile))
                {
                    using (GZipStream compressionStream = new GZipStream(targetStream, CompressionMode.Compress))
                    {
                        sourceStream.CopyTo(compressionStream);
                    }
                }
            }
        }
        public static void Decompress(string compressedFile, string targetFile)
        {
            using (FileStream sourceStream = new FileStream(compressedFile, FileMode.OpenOrCreate))
            {
                using (FileStream targetStream = File.Create(targetFile))
                {
                    using (GZipStream decompressionStream = new GZipStream(sourceStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(targetStream);
                    }
                }
            }
        }
        public static byte[] RSAEncrypt(string info, RSAParameters RSAKeyInfo, bool DoOAEPPadding)
        {
            UnicodeEncoding ByteConverter = new UnicodeEncoding();
            byte[] dataToEncrypt = ByteConverter.GetBytes(info);
            try
            {
                byte[] encryptedData;
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    RSA.ImportParameters(RSAKeyInfo);
                    encryptedData = RSA.Encrypt(dataToEncrypt, DoOAEPPadding);
                }
                return encryptedData;
            }
            catch (CryptographicException)
            {
                return null;
            }
        }
        public static byte[] RSADecrypt(string info, RSAParameters RSAKeyInfo, bool DoOAEPPadding)
        {
            UnicodeEncoding ByteConverter = new UnicodeEncoding();
            byte[] DataToDecrypt = ByteConverter.GetBytes(info);
            try
            {
                byte[] decryptedData;
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    RSA.ImportParameters(RSAKeyInfo);
                    decryptedData = RSA.Decrypt(DataToDecrypt, DoOAEPPadding);
                }
                return decryptedData;
            }
            catch (CryptographicException)
            {
                return null;
            }
        }
        protected override void OnStop()
        {
        }
    }
}

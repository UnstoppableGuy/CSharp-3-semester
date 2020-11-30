using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace FWS
{
    public class Logger
    {
        FileSystemWatcher watcher;
        readonly Options options;
        readonly object obj = new object();
        public Logger(Options options)
        {
            this.options = options;

            if (!Directory.Exists(this.options.SourcePath))
            {
                Directory.CreateDirectory(this.options.SourcePath);
            }
            if (!Directory.Exists(this.options.TargetPath))
            {
                Directory.CreateDirectory(this.options.TargetPath);
            }
            watcher = new FileSystemWatcher(this.options.SourcePath);
            watcher.Deleted += OnDeleted;
            watcher.Created += OnCreated;
            watcher.Changed += OnChanged;
            watcher.Renamed += OnRenamed;
        }
        private void OnCreated(object sender, FileSystemEventArgs eventArgs)
        {
            string filePath = eventArgs.FullPath;
            string fileEvent = "created";
            Events(filePath, fileEvent);
            rawfiles.Add(filePath);
        }
        private void OnRenamed(object sender, RenamedEventArgs eventArgs)
        {
            string filePath = eventArgs.OldFullPath;
            string fileEvent = "renamed to " + eventArgs.FullPath;
            Events(filePath, fileEvent);
        }
        private void OnChanged(object sender, FileSystemEventArgs eventArgs)
        {
            string filePath = eventArgs.FullPath;
            string fileEvent = "changed";
            Events(filePath, fileEvent);
        }
        private void OnDeleted(object sender, FileSystemEventArgs eventArgs)
        {
            string filePath = eventArgs.FullPath;
            string fileEvent = "deleted";
            Events(filePath, fileEvent);
        }
        void Events(string filePath, string fileEvent)
        {
            eventswithfiles.Append($"{DateTime.Now:dd/MM/yyyy HH:mm:ss} file {filePath} was {fileEvent}\n");
        }
        readonly StringBuilder eventswithfiles = new StringBuilder();
        readonly List<string> rawfiles = new List<string>();
        public void Start()
        {
            WriteToFile($"Service was started at {DateTime.Now:dd/MM/yyyy HH:mm:ss}\n");
            if (!Directory.Exists(options.SourcePath))
            {
                Directory.CreateDirectory(options.SourcePath);

                watcher = new FileSystemWatcher(options.SourcePath);
                watcher.Deleted += OnDeleted;
                watcher.Created += OnCreated;
                watcher.Changed += OnChanged;
                watcher.Renamed += OnRenamed;
                watcher.EnableRaisingEvents = true;
            }
            if (!Directory.Exists(options.TargetPath))
            {
                Directory.CreateDirectory(options.TargetPath);
            }
            if (eventswithfiles.Length > 0)
            {
                WriteToFile(eventswithfiles.ToString());

                eventswithfiles.Clear();
            }
            if (rawfiles.Count == 0) return;
            lock (obj)
            {
                watcher.EnableRaisingEvents = false;
                for (int i = 0; i < rawfiles.Count; i++)
                {
                    try
                    {
                        FileInfo fileInfo = new FileInfo(rawfiles[i]);
                        string newFileName = $"Sales_{fileInfo.CreationTime:dd_MM_yyyy_HH_mm_ss}";
                        newFileName += fileInfo.Extension;
                        string newFilePath = Path.Combine(options.SourcePath, newFileName);
                        string newTargetPath = Path.Combine(options.TargetPath, newFileName);
                        if (options.ArchiveOptions.NeedToArchive)
                        {
                            string temp = newFileName;
                            newFileName += ".gz";
                            newFilePath = Path.Combine(options.SourcePath, newFileName);
                            newTargetPath = Path.Combine(options.TargetPath, newFileName);
                            int counter = 1;
                            while (File.Exists(newFilePath) || File.Exists(newTargetPath))
                            {
                                newFileName = "(" + counter.ToString() + ")" + temp + ".gz";
                                newFilePath = Path.Combine(options.SourcePath, newFileName);
                                newTargetPath = Path.Combine(options.TargetPath, newFileName);
                                counter++;
                            }
                            Compress(rawfiles[i], newFilePath);
                        }
                        else
                        {
                            string temp = newFileName;
                            newFilePath = Path.Combine(options.SourcePath, newFileName);
                            newTargetPath = Path.Combine(options.TargetPath, newFileName);
                            int counter = 1;
                            while (File.Exists(newFilePath) || File.Exists(newTargetPath))
                            {
                                newFileName = "(" + counter.ToString() + ")" + temp;
                                newFilePath = Path.Combine(options.SourcePath, newFileName);
                                newTargetPath = Path.Combine(options.TargetPath, newFileName);
                                counter++;
                            }
                            File.Copy(rawfiles[i], newFilePath);
                        }
                        if (options.NeedToEncrypt)
                        {
                            File.Encrypt(newFilePath);
                        }
                        File.Move(newFilePath, newTargetPath);
                        if (options.NeedToEncrypt)
                        {
                            File.Decrypt(newTargetPath);
                        }
                        string decompressedFilePath = Path.Combine(options.TargetPath, "archive");
                        decompressedFilePath = Path.Combine(decompressedFilePath, fileInfo.CreationTime.Year.ToString());
                        decompressedFilePath = Path.Combine(decompressedFilePath, fileInfo.CreationTime.Month.ToString());
                        decompressedFilePath = Path.Combine(decompressedFilePath, fileInfo.CreationTime.Day.ToString());
                        if (!Directory.Exists(decompressedFilePath))
                        {
                            Directory.CreateDirectory(decompressedFilePath);
                        }
                        if (options.ArchiveOptions.NeedToArchive)
                        {
                            decompressedFilePath = Path.Combine(decompressedFilePath, newFileName.Remove(newFileName.Length - 3, 3));
                            Decompress(newTargetPath, decompressedFilePath);
                        }
                        else
                        {
                            decompressedFilePath = Path.Combine(decompressedFilePath, newFileName);
                            File.Copy(newTargetPath, decompressedFilePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        using (StreamWriter sw = new StreamWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exceptions.txt"), true))
                        {
                            sw.WriteLine($"{DateTime.Now:dd/MM/yyyy HH:mm:ss} Exception: {ex.Message}");
                        }
                    }
                }
                watcher.EnableRaisingEvents = true;
                rawfiles.Clear();
            }
        }
        public void Stop()
        {
            watcher.EnableRaisingEvents = false;
            eventswithfiles.Clear();
            WriteToFile($"Service was stopped at {DateTime.Now:dd/MM/yyyy HH:mm:ss}\n");
        }
        public void WriteToFile(string message)
        {
            if (!Directory.Exists(options.SourcePath))
            {
                Directory.CreateDirectory(options.SourcePath);
                watcher = new FileSystemWatcher(options.SourcePath);
                watcher.Deleted += OnDeleted;
                watcher.Created += OnCreated;
                watcher.Changed += OnChanged;
                watcher.Renamed += OnRenamed;
                watcher.EnableRaisingEvents = true;
            }
            if (!Directory.Exists(options.TargetPath))
            {
                Directory.CreateDirectory(options.TargetPath);
            }
            lock (obj)
            {
                using (StreamWriter sw = new StreamWriter(options.LogFilePath, true))
                {
                    sw.Write(message);
                }
            }
        }
        void Compress(string sourceFile, string compressedFile)
        {
            using (FileStream sourceStream = new FileStream(sourceFile, FileMode.Open))
            {
                using (FileStream targetStream = new FileStream(compressedFile, FileMode.OpenOrCreate))
                {
                    using (GZipStream compressionStream = new GZipStream(targetStream, options.ArchiveOptions.Level))
                    {
                        sourceStream.CopyTo(compressionStream);
                    }
                }
            }
        }
        void Decompress(string compressedFile, string targetFile)
        {
            using (FileStream sourceStream = new FileStream(compressedFile, FileMode.Open))
            {
                using (FileStream targetStream = new FileStream(targetFile, FileMode.OpenOrCreate))
                {
                    using (GZipStream decompressionStream = new GZipStream(sourceStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(targetStream);
                    }
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using Aspose.Zip;

namespace AutoCallGraphExtraction
{
    class Program
    {
        const string IDAPath = @"C:\Program Files\IDA Pro 7.5 SP3\";
        const string ScriptPath = @"C:\Project\CreateGDL.py";
        const string MalwareSamplesPath = @"D:\APTMalware-master\samples";
        const string OutputPath = @"D:\APTMalware-master\Output";
        const string searchPattern = "*.zip";
        const string password = "infected";
        static List<string> runtimeFilesExtensions = new List<string>() { "", "id0", "id1", "id2", "nam", "til" };
        static List<Process> processes = new List<Process>();
        static List<KeyValuePair<int, string>> pairList = new List<KeyValuePair<int, string>>();

        static void Main(string[] args)
        {
            List<string> errorLog = new List<string>();

            var subDirectories = GetSubDirectories(MalwareSamplesPath);

            foreach (var dir in subDirectories)
            {
                Console.WriteLine(string.Format("Working on {0}...", dir.FullName));
                var outputDirPath = Path.Combine(OutputPath, dir.Name);
                Directory.CreateDirectory(outputDirPath);

                var fileList = Directory.GetFiles(dir.FullName, searchPattern);
                var total = fileList.Count();
                var count = 0;
                foreach (var filePath in fileList)
                {
                    count++;
                    Console.WriteLine(string.Format("{0}/{1}", count, total));
                    try
                    {
                        var fileName = Path.GetFileNameWithoutExtension(filePath);

                        using (FileStream zipFile = File.Open(filePath, FileMode.Open, FileAccess.Read))
                        {
                            var decryptedFile = new Archive(zipFile, new ArchiveLoadOptions() { DecryptionPassword = password });
                            decryptedFile.ExtractToDirectory(outputDirPath);
                        }
                        CallRunProcessAsync(Path.Combine(outputDirPath, fileName));
                    }
                    catch
                    {
                        errorLog.Add(filePath);
                        continue;
                    }

                    System.Threading.Thread.Sleep(10000);
                }
            }

            System.Threading.Thread.Sleep(60000);
            if (errorLog.Count > 0)
            {
                File.WriteAllLines(Path.Combine(OutputPath, "ErrorLog.txt"), errorLog);
            }
        }
        static async void CallRunProcessAsync(string filePath)
        {
            Task<Process> task = RunProcessAsync(filePath);
            var process = await task;
            if (task.IsCompleted)
            {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var outputDirPath = Path.GetDirectoryName(filePath);

                task.Result.WaitForExit(5000);
                task.Result.Dispose();

                DeleteRuntimeFiles(fileName, outputDirPath);
            }
        }
        static async Task<Process> RunProcessAsync(string filePath)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = "cmd.exe";
            processStartInfo.WorkingDirectory = IDAPath;
            processStartInfo.Arguments = string.Format(@"/C idat64 -a -A -S{0} {1}", ScriptPath, filePath);

            processStartInfo.UseShellExecute = true;
            processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            Process process = new Process();
            process.StartInfo = processStartInfo;
            await Task.Run(() =>
            {
                process.Start();
                processes.Add(process);
                pairList.Add(new KeyValuePair<int, string>(process.Id, filePath));
            });
            return process;
        }
        /// <summary>
        /// IDA Text Inteface does not accept space in file path
        /// This method renames the directories
        /// </summary>
        static DirectoryInfo[] GetSubDirectories(string mainDirectoryPath)
        {
            var mainDirectory = new DirectoryInfo(MalwareSamplesPath);
            var subDirectories = mainDirectory.GetDirectories();

            foreach (var dir in subDirectories)
            {
                if (dir.Name.Contains(" "))
                    Directory.Move(dir.FullName, dir.FullName.Replace(" ", string.Empty));
            }

            subDirectories = mainDirectory.GetDirectories().ToList().FindAll(i => !i.Name.Contains(" ")).ToArray();
            return subDirectories;
        }
        static void DeleteRuntimeFiles(string fileName, string outputDirPath)
        {
            try
            {
                foreach (var fileExtension in runtimeFilesExtensions)
                {
                    var targetFileName = string.Format("{0}.{1}", fileName, fileExtension);
                    var targetPath = Path.Combine(outputDirPath, targetFileName);
                    if (File.Exists(targetPath))
                    {
                        File.Delete(targetPath);
                    }
                }
            }
            catch
            {

            }
        }
    }
}

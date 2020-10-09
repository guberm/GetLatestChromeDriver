using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using Microsoft.Win32;

namespace GetLatestChromeDriver {
    internal class Program {
        private static string path;

        private static void Main(string[] args) {
            GetLatestChromeDriver();
        }

        public static void KillProcess(int pid) {
            KillProcessAndChildren(pid);
        }

        public static void KillProcess(string processName) {
            var procs = Process.GetProcessesByName(processName);
            foreach (var proc in procs)
                KillProcessAndChildren(proc.Id);
        }

        private static void KillProcessAndChildren(int parentProcessId) {
            if (parentProcessId == 0) return;

            var searcher = new ManagementObjectSearcher
                ("Select * From Win32_Process Where ParentProcessID=" + parentProcessId);
            var moc = searcher.Get();
            foreach (var o in moc) {
                var mo = (ManagementObject) o;
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
                Thread.Sleep(500);
            }

            try {
                var proc = Process.GetProcessById(parentProcessId);
                proc.Kill();
                Thread.Sleep(500);
            }
            catch (ArgumentException) {
                // Process already exited.
            }
        }

        private static void KillLockedProcess(string lockedFilePath) {
            ZipFile.ExtractToDirectory($"{path}\\Handle.zip", $"{path}\\");

            var tool = new Process();
            tool.StartInfo.FileName = "handle.exe";
            tool.StartInfo.Arguments = lockedFilePath + " /accepteula";
            tool.StartInfo.UseShellExecute = false;
            tool.StartInfo.RedirectStandardOutput = true;
            tool.Start();
            tool.WaitForExit();
            var outputTool = tool.StandardOutput.ReadToEnd();

            var matchPattern = @"(?<=\s+pid:\s+)\b(\d+)\b(?=\s+)";
            foreach (Match match in Regex.Matches(outputTool, matchPattern)) {
                var proc = Process.GetProcessById(int.Parse(match.Value)).Id;
                KillProcess(proc);
            }
        }

        public static void GetLatestChromeDriver() {
            var chromeVer =
                (string) Registry.GetValue(@"HKEY_CURRENT_USER\Software\Google\Chrome\BLBeacon", "version", null);

            chromeVer = chromeVer.Split('.')[0];

            path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            if (File.Exists($"{path}\\chromedriver.exe"))
                try {
                    File.Delete($"{path}\\chromedriver.exe");
                }
                catch {
                    KillLockedProcess($"{path}\\chromedriver.exe");
                    File.Delete($"{path}\\chromedriver.exe");
                }

            var ver = GetHtmlDocument("https://chromedriver.storage.googleapis.com/");

            var versions = new List<string>();

            var xml = new XmlDocument();
            xml.LoadXml(ver);
            foreach (XmlNode node in xml.DocumentElement.ChildNodes)
                switch (node.Name) {
                    case "Contents": {
                        foreach (var xmlElement in node.Cast<XmlElement>()
                            .Where(xmlElement => xmlElement.Name == "Key"))
                            try {
                                if (xmlElement.InnerText.Contains($"LATEST_RELEASE_{chromeVer}"))
                                    versions.Add(xmlElement.InnerText);
                            }
                            catch {
                                // ignored
                            }

                        break;
                    }
                }

            ver = GetHtmlDocument($"https://chromedriver.storage.googleapis.com/{versions.Last()}");

            using (var webClient = new WebClient()) {
                webClient.DownloadFile($"https://chromedriver.storage.googleapis.com/{ver}/chromedriver_win32.zip",
                    $"{path}\\chromedriver.zip");
            }

            var count = 0;
            while (!File.Exists($"{path}\\chromedriver.zip") && count < 10) Thread.Sleep(1000);

            if (File.Exists($"{path}\\chromedriver.zip"))
                ZipFile.ExtractToDirectory($"{path}\\chromedriver.zip", $"{path}\\");

            if (File.Exists($"{path}\\chromedriver.zip")) File.Delete($"{path}\\chromedriver.zip");
        }

        private static string GetHtmlDocument(string url) {
            var request = (HttpWebRequest) WebRequest.Create(url);
            request.MaximumAutomaticRedirections = 300;
            request.MaximumResponseHeadersLength = 300;
            request.AllowWriteStreamBuffering = true;
            request.Credentials = CredentialCache.DefaultCredentials;
            var response = (HttpWebResponse) request.GetResponse();
            var receiveStream = response.GetResponseStream();
            var readStream = new StreamReader(receiveStream, Encoding.UTF8);
            var resp = readStream.ReadToEnd();
            response.Close();
            readStream.Close();

            return resp;
        }
    }
}
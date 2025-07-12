using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace GetLatestChromeDriver
{
    internal class Program
    {
        private static string? _applicationPath;
        private static readonly HttpClient HttpClient = new();

        private static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("ChromeDriver Updater - Starting...");
                await GetLatestChromeDriverAsync();
                Console.WriteLine("ChromeDriver updated successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        }

        public static void KillProcess(int pid)
        {
            KillProcessAndChildren(pid);
        }

        public static void KillProcess(string processName)
        {
            var procs = Process.GetProcessesByName(processName);
            foreach (var proc in procs)
                KillProcessAndChildren(proc.Id);
        }

        private static void KillProcessAndChildren(int parentProcessId)
        {
            if (parentProcessId == 0) return;

            // This functionality is Windows-specific
            if (!OperatingSystem.IsWindows()) return;

            try
            {
                var searcher = new ManagementObjectSearcher
                    ("Select * From Win32_Process Where ParentProcessID=" + parentProcessId);
                var moc = searcher.Get();
                foreach (var o in moc)
                {
                    var mo = (ManagementObject)o;
                    KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
                    Thread.Sleep(500);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not enumerate child processes: {ex.Message}");
            }

            try
            {
                var proc = Process.GetProcessById(parentProcessId);
                proc.Kill();
                Thread.Sleep(500);
            }
            catch (ArgumentException)
            {
                // Process already exited.
            }
        }

        private static void KillLockedProcess(string lockedFilePath)
        {
            if (_applicationPath == null) return;

            try
            {
                ZipFile.ExtractToDirectory($"{_applicationPath}\\Handle.zip", $"{_applicationPath}\\");

                var tool = new Process();
                tool.StartInfo.FileName = "handle.exe";
                tool.StartInfo.Arguments = lockedFilePath + " /accepteula";
                tool.StartInfo.UseShellExecute = false;
                tool.StartInfo.RedirectStandardOutput = true;
                tool.Start();
                tool.WaitForExit();
                var outputTool = tool.StandardOutput.ReadToEnd();

                var matchPattern = @"(?<=\s+pid:\s+)\b(\d+)\b(?=\s+)";
                foreach (Match match in Regex.Matches(outputTool, matchPattern))
                {
                    var proc = Process.GetProcessById(int.Parse(match.Value)).Id;
                    KillProcess(proc);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not kill locked process: {ex.Message}");
            }
        }

        public static async Task GetLatestChromeDriverAsync()
        {
            // Get Chrome version
            var chromeVersion = GetChromeVersion();
            if (string.IsNullOrEmpty(chromeVersion))
            {
                throw new InvalidOperationException("Could not detect Chrome version. Please ensure Chrome is installed.");
            }

            Console.WriteLine($"Detected Chrome version: {chromeVersion}");

            // Get major version number
            var majorVersion = chromeVersion.Split('.')[0];

            _applicationPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
            if (_applicationPath == null)
            {
                throw new InvalidOperationException("Could not determine application path.");
            }

            var chromeDriverPath = Path.Combine(_applicationPath, "chromedriver.exe");

            // Remove existing chromedriver if it exists
            if (File.Exists(chromeDriverPath))
            {
                try
                {
                    File.Delete(chromeDriverPath);
                }
                catch
                {
                    KillLockedProcess(chromeDriverPath);
                    File.Delete(chromeDriverPath);
                }
            }

            // Get the compatible ChromeDriver version using Chrome for Testing API
            var driverVersion = await GetChromeDriverVersionAsync(majorVersion);
            if (string.IsNullOrEmpty(driverVersion))
            {
                throw new InvalidOperationException($"Could not find compatible ChromeDriver for Chrome version {majorVersion}");
            }

            Console.WriteLine($"Downloading ChromeDriver version: {driverVersion}");

            // Download ChromeDriver
            await DownloadChromeDriverAsync(driverVersion, _applicationPath);

            Console.WriteLine("ChromeDriver extraction completed.");
        }

        private static string? GetChromeVersion()
        {
            try
            {
                // This tool is Windows-specific, so we can safely use Registry
                if (OperatingSystem.IsWindows())
                {
                    // Try registry first (most reliable on Windows)
                    var chromeVersion = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Google\Chrome\BLBeacon", "version", null) as string;
                    if (!string.IsNullOrEmpty(chromeVersion))
                    {
                        return chromeVersion;
                    }

                    // Fallback: try HKEY_LOCAL_MACHINE
                    chromeVersion = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Google\Chrome\BLBeacon", "version", null) as string;
                    if (!string.IsNullOrEmpty(chromeVersion))
                    {
                        return chromeVersion;
                    }
                }

                // Fallback: try to get version from chrome.exe
                var chromePaths = new[]
                {
                    @"C:\Program Files\Google\Chrome\Application\chrome.exe",
                    @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Google\Chrome\Application\chrome.exe")
                };

                foreach (var chromePath in chromePaths)
                {
                    if (File.Exists(chromePath))
                    {
                        var versionInfo = FileVersionInfo.GetVersionInfo(chromePath);
                        if (!string.IsNullOrEmpty(versionInfo.FileVersion))
                        {
                            return versionInfo.FileVersion;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not detect Chrome version: {ex.Message}");
            }

            return null;
        }

        private static async Task<string?> GetChromeDriverVersionAsync(string chromeMajorVersion)
        {
            try
            {
                // Use Chrome for Testing API to get the latest ChromeDriver version for the Chrome version
                var url = "https://googlechromelabs.github.io/chrome-for-testing/known-good-versions-with-downloads.json";
                var response = await HttpClient.GetStringAsync(url);
                
                using var doc = JsonDocument.Parse(response);
                var versions = doc.RootElement.GetProperty("versions");

                // Find the latest version that matches the major Chrome version
                string? latestVersion = null;
                foreach (var version in versions.EnumerateArray())
                {
                    var versionString = version.GetProperty("version").GetString();
                    if (versionString != null && versionString.StartsWith(chromeMajorVersion + "."))
                    {
                        // Check if this version has ChromeDriver downloads available
                        if (version.TryGetProperty("downloads", out var downloads) &&
                            downloads.TryGetProperty("chromedriver", out var chromeDriverDownloads))
                        {
                            latestVersion = versionString;
                        }
                    }
                }

                return latestVersion;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not get ChromeDriver version from API: {ex.Message}");
                return null;
            }
        }

        private static async Task DownloadChromeDriverAsync(string version, string outputPath)
        {
            try
            {
                // Get download URL from Chrome for Testing API
                var url = "https://googlechromelabs.github.io/chrome-for-testing/known-good-versions-with-downloads.json";
                var response = await HttpClient.GetStringAsync(url);
                
                using var doc = JsonDocument.Parse(response);
                var versions = doc.RootElement.GetProperty("versions");

                string? downloadUrl = null;
                foreach (var versionInfo in versions.EnumerateArray())
                {
                    var versionString = versionInfo.GetProperty("version").GetString();
                    if (versionString == version)
                    {
                        if (versionInfo.TryGetProperty("downloads", out var downloads) &&
                            downloads.TryGetProperty("chromedriver", out var chromeDriverDownloads))
                        {
                            foreach (var download in chromeDriverDownloads.EnumerateArray())
                            {
                                var platform = download.GetProperty("platform").GetString();
                                if (platform == "win32")
                                {
                                    downloadUrl = download.GetProperty("url").GetString();
                                    break;
                                }
                            }
                        }
                        break;
                    }
                }

                if (string.IsNullOrEmpty(downloadUrl))
                {
                    throw new InvalidOperationException($"Could not find download URL for ChromeDriver version {version}");
                }

                var zipPath = Path.Combine(outputPath, "chromedriver.zip");
                
                Console.WriteLine($"Downloading from: {downloadUrl}");
                var zipBytes = await HttpClient.GetByteArrayAsync(downloadUrl);
                await File.WriteAllBytesAsync(zipPath, zipBytes);

                // Wait a bit to ensure file is completely written
                await Task.Delay(1000);

                if (File.Exists(zipPath))
                {
                    ZipFile.ExtractToDirectory(zipPath, outputPath, overwriteFiles: true);
                    File.Delete(zipPath);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to download ChromeDriver: {ex.Message}", ex);
            }
        }
    }
}
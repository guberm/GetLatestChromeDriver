using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using Microsoft.Win32;

namespace GetLatestChromeDriver
{
    class Program
    {
        static void Main(string[] args)
        {
            GetLatestChromeDriver();
        }

        public static void GetLatestChromeDriver()
        {
            string chromeVer =
                (string) Registry.GetValue(@"HKEY_CURRENT_USER\Software\Google\Chrome\BLBeacon", "version", null);

            chromeVer = chromeVer.Split('.')[0];

            string path = Assembly.GetExecutingAssembly().Location;
            string ppp = path.Split('\\').Last();
            path = path.Replace(ppp, "");

            if (File.Exists($"{path}chromedriver.exe"))
            {
                File.Delete($"{path}chromedriver.exe");
            }

            string ver = GetHtmlDocument("https://chromedriver.storage.googleapis.com/");

            List<string> versions = new List<string>();

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(ver);
            foreach (XmlNode node in xml.DocumentElement.ChildNodes)
            {
                switch (node.Name)
                {
                    case "Contents":

                    {
                        foreach (XmlElement xmlElement in node.Cast<XmlElement>()
                            .Where(xmlElement => xmlElement.Name == "Key"))
                        {
                            try
                            {
                                if (xmlElement.InnerText.Contains($"LATEST_RELEASE_{chromeVer}"))
                                {
                                    versions.Add(xmlElement.InnerText);
                                }
                            }
                            catch
                            {
                                // ignored
                            }
                        }

                        break;
                    }
                }
            }

            ver = GetHtmlDocument($"https://chromedriver.storage.googleapis.com/{versions.Last()}");

            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadFile($"https://chromedriver.storage.googleapis.com/{ver}/chromedriver_win32.zip",
                    $"{path}chromedriver.zip");
            }

            int count = 0;
            while (!File.Exists($"{path}chromedriver.zip") && count < 10)
            {
                Thread.Sleep(1000);
            }

            if (File.Exists($"{path}chromedriver.zip"))
            {
                System.IO.Compression.ZipFile.ExtractToDirectory($"{path}chromedriver.zip", $"{path}");
            }

            if (File.Exists($"{path}chromedriver.zip"))
            {
                File.Delete($"{path}chromedriver.zip");
            }
        }

        private static string GetHtmlDocument(string url)
        {
            HttpWebRequest request = (HttpWebRequest) HttpWebRequest.Create(url);
            request.MaximumAutomaticRedirections = 300;
            request.MaximumResponseHeadersLength = 300;
            request.AllowWriteStreamBuffering = true;
            request.Credentials = CredentialCache.DefaultCredentials;
            HttpWebResponse response = (HttpWebResponse) request.GetResponse();
            Stream receiveStream = response.GetResponseStream();
            StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
            string resp = readStream.ReadToEnd();
            response.Close();
            readStream.Close();

            return resp;
        }
    }
}

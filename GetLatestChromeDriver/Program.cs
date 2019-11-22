using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;

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
            string path = Assembly.GetExecutingAssembly().Location;
            string ppp = path.Split('\\').Last();
            path = path.Replace(ppp, "");

            if (File.Exists($"{path}chromedriver.exe"))
            {
                File.Delete($"{path}chromedriver.exe");
            }

            string ver = GetHtmlDocument("https://chromedriver.storage.googleapis.com/LATEST_RELEASE");

            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadFile($"https://chromedriver.storage.googleapis.com/{ver}/chromedriver_win32.zip", $"{path}chromedriver.zip");
            }

            int count = 0;
            while (!File.Exists($"{path}chromedriver.zip") && count < 10)
            {
                Thread.Sleep(1000);
            }

            if (File.Exists($"{path}chromedriver.zip"))
            {
                ZipFile.ExtractToDirectory($"{path}chromedriver.zip", $"{path}");
            }

            if (File.Exists($"{path}chromedriver.zip"))
            {
                File.Delete($"{path}chromedriver.zip");
            }
        }

        private static string GetHtmlDocument(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.MaximumAutomaticRedirections = 300;
            request.MaximumResponseHeadersLength = 300;
            request.AllowWriteStreamBuffering = true;
            request.Credentials = CredentialCache.DefaultCredentials;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream receiveStream = response.GetResponseStream();
            StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
            string resp = readStream.ReadToEnd();
            response.Close();
            readStream.Close();

            return resp;
        }
    }
}

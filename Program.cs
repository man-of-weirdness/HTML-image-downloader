using System;
using System.Xml;
using System.IO;
using HtmlAgilityPack;
using System.Net;
using System.Collections.Generic;
using System.Text;
using System.Security;

namespace ConsoleApp7
{
    class Program
    {
        static void Main(string[] args)
        {
            #region startchecks
            Console.WriteLine("make sure you are running this with Administrator privileges for best results.");
            Console.WriteLine("local file (Y) or network resource? (N)");
            var doc = new HtmlDocument();
            if (UserResponse())
            {
                LocalStart(ref doc);
            }
            else
            {
                NetStart(ref doc);
            }
            #endregion
            #region setupread
            var AllTheText = doc.ParsedText;
            #endregion
            Console.WriteLine(AllTheText);
            #region parsing
            List<(string, int)> sources = new List<(string, int)>();
            #region FindingImages
            while (AllTheText.Contains("<img") && AllTheText.Contains("src =\"") && AllTheText.Contains('.'))
            {
                int start = AllTheText.Substring(AllTheText.IndexOf("<img")).IndexOf("src=\"") + 5;
                string source = AllTheText.Substring(start, AllTheText.Substring(start).IndexOf("\""));
                sources.Add((source, start));
                AllTheText = AllTheText.Remove(AllTheText.IndexOf("<img"), source.Length);
                Console.WriteLine($"{start}: {source}");
            }
            #endregion
            string SavePath;
            using (WebClient wc = new WebClient())
            {
                
#region SavePathSetting
                
            SavePathStart:
                if (args.Length > 0)
                { SavePath = args[0]; }
                else
                {
                    Console.WriteLine("Specify a path to save downloaded images to:");
                    SavePath = Console.ReadLine();
                }
                try
                {
                    SavePath = Path.GetFullPath(SavePath);
                }
                catch (Exception)
                {
                    Console.ForegroundColor = (ConsoleColor)12;
                    Console.WriteLine("Error");
                    Console.ForegroundColor = (ConsoleColor)7;
                    goto SavePathStart;
                }
                #endregion
                foreach ((string, int) tuppy in sources)
                {
                    string imagepath = SavePath + System.IO.Path.GetFileName(tuppy.Item1);
                    wc.DownloadFile(tuppy.Item1, imagepath);
                    AllTheText.Replace(tuppy.Item1, imagepath);
                }
                
            }
            doc.LoadHtml(AllTheText);
        FilenameDialog:
            Console.WriteLine("Final document filename? (with extension)");
            try
            {
                Console.Write(SavePath);
                var finaldoc = File.Create(SavePath + Console.ReadLine(), 4096, FileOptions.None);
                finaldoc.Write(Encoding.Default.GetBytes(AllTheText));
                doc.Load(finaldoc);
                finaldoc.Dispose();
                Console.WriteLine();
                
            }
            catch (SecurityException)
            {
                Console.ForegroundColor = (ConsoleColor)12;
                Console.WriteLine("You need to open this program with administrator privileges for this destination.");
                Console.ForegroundColor = (ConsoleColor)7;
                goto FilenameDialog;
            }
            catch (Exception)
            {
                Console.ForegroundColor = (ConsoleColor)12;
                Console.WriteLine("Error");
                Console.ForegroundColor = (ConsoleColor)7;
                goto FilenameDialog;
            }
            Console.WriteLine("Finished.");
           // MAKE IT SAVE!!!!!!!!!!!!!!!
            #endregion
            Console.ReadKey();
        }

        #region OtherMethods
        static bool UserResponse()
        {
        URstart:
            char UR = Console.ReadKey().KeyChar;
            Console.WriteLine("");
            if(UR == 'y' || UR == 'Y') { return true;  }
            else if(UR == 'n' || UR == 'N') { return false;  }
            else
            {
                Console.ForegroundColor = (ConsoleColor)12;
                Console.WriteLine("Error");
                Console.ForegroundColor = (ConsoleColor)7;
                goto URstart;
            }
        }
        static void LocalStart(ref HtmlDocument doc)
        {
            LocalStart:
            string path = Console.ReadLine();
            if (!path.EndsWith(".html") || !File.Exists(path))
            {
                Console.ForegroundColor = (ConsoleColor)12;
                Console.WriteLine("Error in path name");
                Console.ForegroundColor = (ConsoleColor)7;
                goto LocalStart;
            }
            doc.Load(path);
        }

        static void NetStart(ref HtmlDocument doc)
        {
        NetStart:
            string path = Console.ReadLine();
            if(!CheckWebsite(path))
            {
                Console.ForegroundColor = (ConsoleColor)12;
                Console.WriteLine("Error in path name");
                Console.ForegroundColor = (ConsoleColor)7;
                goto NetStart;
            }
            using (WebClient wc = new WebClient())
            {
                doc.LoadHtml(wc.DownloadString(path));
            }
        }

        static bool CheckWebsite(string URL)
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(URL);
                req.Method = "GET";
                var resp = req.GetResponse();
                if(resp.ResponseUri != req.RequestUri)
                {
                    Console.WriteLine($"you got a response but it was from {resp.ResponseUri}");
                    Console.WriteLine("are you sure this is what you want? (Y/N)");
                    if(!UserResponse())
                    {
                        throw new Exception("this isnt it chief");
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion
    }
}

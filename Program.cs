using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityChangesetsRipper
{
    class Program
    {
        static List<string> changesetsFound;

        static string unityArchiveURL = "https://unity3d.com/get-unity/download/archive";

        //static string unityPatchesURL = "https://web.archive.org/web/20200531192759/https://unity3d.com/unity/qa/patch-releases?page={0}";
        static string unityPatchesURL = "https://unity3d.com/unity/qa/patch-releases/{0}.{1}.{2}p{3}";

        static void Main(string[] args)
        {
            changesetsFound = new List<string>(1000);

            string unityArchiveRAW = DownloadURLAsText(unityArchiveURL);

            ExtractChangesetsUnityHubURIs(unityArchiveRAW);
            ExtractChangesetsSetupLink(unityArchiveRAW, "5.");

            /*for (int i = 1; i < 40; i++)
            {
                unityArchiveRAW = DownloadURLAsText(string.Format(unityPatchesURL, i));
                ExtractChangesetsSetupLink(unityArchiveRAW, "5.");
            }*/

            for (int major = 5; major < 2018; major++)
            {
                if (major == 6)
                    major = 2017;

                for (int minor = 0; minor < 7; minor++)
                {
                    if (major >= 2017 && minor > 2)
                        break;

                    if (major >= 2017 && minor == 0)
                        minor++;

                    for (int fix = 0; fix < 10; fix++)
                    {
                        for (int p = 1; p < 10; p++)
                        {
                            unityArchiveRAW = DownloadURLAsText(string.Format(unityPatchesURL, major, minor, fix, p));
                            ExtractChangesetsSetupLink(unityArchiveRAW, major + ".");
                        }
                    }
                }
            }



            ExportChangesetList();
        }


        static void ExportChangesetList()
        {
            changesetsFound = changesetsFound.Distinct().ToList();
            changesetsFound = changesetsFound.OrderBy(x => x.Substring(13).Trim()).ToList();

            StringBuilder sb = new StringBuilder(1000 * 30);

            changesetsFound.ForEach(item => sb.Append(item + "\r\n"));

            System.IO.File.WriteAllText("unity_changesets_master.txt", sb.ToString());
        }


        static void ExtractChangesetsUnityHubURIs(string rawStr)
        {
            int i = 0;
            while (i < rawStr.Length)
            {
                int idx = rawStr.IndexOf("unityhub://", i);
                if (idx > 0)
                {
                    int j = idx;
                    while (rawStr[j] != '\"')
                    {
                        j++;
                    }

                    changesetsFound.Add(SplitChangesetUnityHubURI(rawStr.Substring(idx, j - idx)));

                    i = j;
                }
                else
                {
                    break;
                }
            }
        }


        static void ExtractChangesetsSetupLink(string rawStr, string verPattern)
        {
            string pattern = "/Windows64EditorInstaller/UnitySetup64-" + verPattern;

            int i = 0;
            while (i < rawStr.Length)
            {
                int idx = rawStr.IndexOf(pattern, i);
                if (idx > 0)
                {
                    // Go back 12 chars
                    idx -= 12;

                    int j = idx;
                    while (rawStr[j] != '\"')
                    {
                        j++;
                    }

                    changesetsFound.Add(SplitChangesetDonwloadWin64Link(rawStr.Substring(idx, j - idx)));

                    i = j;
                }
                else
                {
                    break;
                }
            }
        }


        static string SplitChangesetUnityHubURI(string aStr)
        {
            string s = aStr.Replace("unityhub://", "");
            var parts = s.Split('/');
            return parts[1] + " " + parts[0];
        }


        static string SplitChangesetDonwloadWin64Link(string aStr)
        {
            // Sample
            //var s = "e80cc3114ac1/Windows64EditorInstaller/UnitySetup64-5.6.7f1.exe";

            return aStr.Replace("/Windows64EditorInstaller/UnitySetup64-", " ").Replace(".exe", "");
        }


        static string DownloadURLAsText(string aAddress)
        {
            string rawTxt = null;

            try
            {
                Console.WriteLine("checking at: " + aAddress);
                using (System.Net.WebClient webClient = new System.Net.WebClient())
                {
                    rawTxt = webClient.DownloadString(aAddress);
                }
            }
            catch(System.Net.WebException e)
            {
                rawTxt = "";
            }

            return rawTxt;
        }
    }
}

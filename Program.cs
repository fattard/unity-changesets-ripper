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

        static void Main(string[] args)
        {
            changesetsFound = new List<string>(1000);

            string unityArchiveRAW = DownloadURLAsText(unityArchiveURL);

            ExtractChangesetsUnityHubURIs(unityArchiveRAW);
            ExtractChangesetsSetupLink(unityArchiveRAW, "5.");

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

            using (System.Net.WebClient webClient = new System.Net.WebClient())
            {
                rawTxt = webClient.DownloadString(aAddress);
            }

            return rawTxt;
        }
    }
}

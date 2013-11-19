using System.Net;
using System.Text;
using System;
using System.Collections.Generic;
using System.Threading;

class LocateMe
{

    public List<string> Location { get { return Locate(); } }

    private List<string> Locate()
    {
        while (true)
        {
            try
            {
                using (WebClient Client = new WebClient())
                {
                    string WebPage = Client.DownloadString("https://duckduckgo.com/?q=whats+my+ip");
                    string[] html = WebPage
                        .Split(new char[] { '>', '<', '"', '=', ',', ' ', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);

                    for (int i = 0; i < html.Length; i++)
                    {
                        if (html[i].Contains("http://open.mapquest.com/?q"))
                        {
                            i++;
                            List<string> rar = new List<string>();

                            while (!html[i].Contains("/a"))
                            {
                                rar.Add(html[i++]);
                            }

                            return rar;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Thread.Sleep(60000);
            }
            return null;
        }
        
    }
}


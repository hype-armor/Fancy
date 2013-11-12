using System.Net;
using System.Text;
using System;

class LocateMe
{
		
	public string Location { get { return Locate(); } }

	private string Locate()
	{
		//State();
		using (WebClient Client = new WebClient())
		{
			string[] html = Client.DownloadString("https://duckduckgo.com/?q=whats+my+ip")
			    .Split(new char[] { '>', '<', '/', '"', '=' }, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < html.Length; i++)
			{

				if (html[i].Contains("Your IP address is"))
				{
					string[] rar = html[i + 6].Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    return ZIP(rar[0], rar[1].Substring(0, 2)); //html[i + 6]; // returns "City, State, Country"
				}
			}
		}
		return null;
	}

	private string ZIP(string City, string State)
	{
		//http://zipcode.org/city/OK/TULSA

		//WebClient Client = new WebClient();
		using (WebClient Client = new WebClient())
		{
            string[] html = Client.DownloadString("http://zipcode.org/city/" + State + "/" + City)
			    .Split(new char[] { '>', '<', '/', '"', '=' }, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < html.Length; i++)
			{

				if (html[i].Contains("Area Code"))
				{
					for (i = 800; i < html.Length; i++)
					{

						if (html[i].Contains("a href"))
						{
							return html[i+1]; // returns a zip for the area.
						}
					}
				}
			}
		}
		return null;
	}
}


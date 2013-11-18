using ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml;

class Weather
{
	public string ZIP { get; set; }
	public string Temperature = "0";
	public string Condition = "Loading...";
    public BitmapImage Icon;
    public string Hazard { get; set; }
    public string iconURL = "";
    private readonly string APIKEY = "c0a06bca9f1999b7";

    public void Wunderground()
    {
        XmlDocument WUnderWeather = new XmlDocument();
        WUnderWeather.Load("http://api.wunderground.com/api/" + APIKEY + "/conditions/q/" + ZIP + ".xml");
        XmlNamespaceManager NameSpaceMgr = new XmlNamespaceManager(WUnderWeather.NameTable);
        XmlNode WeatherNode = WUnderWeather.SelectNodes("/response/current_observation/weather", NameSpaceMgr)[0];
        XmlNode TempNode = WUnderWeather.SelectNodes("/response/current_observation/temp_f", NameSpaceMgr)[0];
        XmlNode IconNode = WUnderWeather.SelectNodes("/response/current_observation/icon_url", NameSpaceMgr)[0];

        Condition = WeatherNode.InnerText;
        Temperature = TempNode.InnerText.ToDecimal().ToString("###");
        //GetIcon(IconNode.InnerText.Replace("/i/c/k/", "/i/c/i/"));
        iconURL = IconNode.InnerText.Replace("/i/c/k/", "/i/c/i/");

        WUnderWeather.Load("http://api.wunderground.com/api/" + APIKEY + "/alerts/q/" + ZIP + ".xml");
        if (!string.IsNullOrEmpty(WUnderWeather.SelectNodes("/response/alerts", NameSpaceMgr)[0].InnerText))
        {
            XmlNode HazardsNode = WUnderWeather.SelectNodes("/response/alerts/alert/description", NameSpaceMgr)[0];
            XmlNode HazardStartNode = WUnderWeather.SelectNodes("/response/alerts/alert/date", NameSpaceMgr)[0];
            XmlNode HazardExpiresNode = WUnderWeather.SelectNodes("/response/alerts/alert/expires", NameSpaceMgr)[0];

            string NewLine = Environment.NewLine;
            Hazard = HazardsNode.InnerText + NewLine + "From: " + HazardStartNode.InnerText + NewLine + "Until: " + HazardExpiresNode.InnerText;
        }
        else
        {
            Hazard = "";
        }
    }

	private void GetIcon(string url)
	{
	    using (WebClient wc = new WebClient())
	    {
		    byte[] WebPage = wc.DownloadData(url);
		    Icon = toBitmap(byteArrayToImage(WebPage));
	    }
	}

    private Bitmap byteArrayToImage(byte[] byteArrayIn)
	{
		MemoryStream ms = new MemoryStream(byteArrayIn);
		//Image returnImage = Image.FromStream(ms);
		
		return new Bitmap(ms);// returnImage;
	}

    private BitmapImage toBitmap(Bitmap bmp)
	{
		//System.Drawing.Bitmap bmp;

		MemoryStream ms = new MemoryStream();
		bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
		ms.Position = 0;
		BitmapImage bi = new BitmapImage();
		bi.BeginInit();
		bi.StreamSource = ms;
		bi.EndInit();

		return bi;
	}
}


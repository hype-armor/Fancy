using ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Media.Imaging;
using System.Xml;

interface IWeather
{
    Uri Uri { get; }
    string Zip { get; }
    decimal Temperature { get; }
    string Condition { get; }
    string TemperatureXpath { get; set; }
    string ConditionXpath { get; set; }

    XmlDocument XmlDoc { get; }

    void UpdateData();
    System.Timers.Timer Timer { set; }
    void TimerElapsed(object sender, ElapsedEventArgs e);
}

public class WeatherFactory
{
    public Weather Create(string service)
    {
        Weather weather = null;

        if (service == "nws")
        {
            weather = new Nws();
        }
        else if (service == "nws2")
        {
            weather = new Nws2();
        }

        return weather;
    }
}

public class Nws : Weather
{
    public Nws()
    {
        Zip = "74037";
        Uri = new Uri("https://forecast.weather.gov/MapClick.php?lat=36.0022&lon=-95.9746&unit=0&lg=english&FcstType=dwml");
        TemperatureXpath = "/dwml/data[2]/parameters/temperature[1]/value";
        ConditionXpath = "/dwml/data[2]/parameters/weather/weather-conditions[1]/@weather-summary";
        webClient = new WebClient();
        webClient.Headers.Add("user-agent", "nws@gregsemail.us");
    }
}

public class Nws2 : Nws
{
    public Nws2()
    {
        Header = new KeyValuePair<string, string>("user-agent", "nws@gregsemail.us");
    }
}


public class Weather : IWeather
{
    private string _zip;
    public string Zip { get => _zip; set => _zip = value; }

    private decimal _temperature = 0;
    public decimal Temperature => _temperature;

    private string _condition;
    public string Condition => _condition;

    private Uri _uri;
    public Uri Uri { get => _uri; set => _uri = value; }

    private XmlDocument _xmlDocument = new XmlDocument();
    public XmlDocument XmlDoc => _xmlDocument;

    private string _temperatureXpath;
    public string TemperatureXpath { get => _temperatureXpath; set => _temperatureXpath = value; }
    private string _conditionXpath;
    public string ConditionXpath { get => _conditionXpath; set => _conditionXpath = value; }

    private WebClient _webClient;
    public WebClient webClient { get => _webClient; set => _webClient = value; }

    private System.Timers.Timer _timer;
    public System.Timers.Timer Timer { set => _timer = value; }

    private KeyValuePair<string, string> _header;
    public KeyValuePair<string, string> Header
    {
        get { return _header; }
        set { _header = value; }
    }


    private bool isUpdating = false;
    
    public void UpdateData()
    {
        isUpdating = true;
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        try
        {
            _webClient.Headers.Add(Header.Key, Header.Value);
            string xml = _webClient.DownloadString(_uri.AbsoluteUri);
            _xmlDocument.LoadXml(xml);
            XmlNamespaceManager NameSpaceMgr = new XmlNamespaceManager(_xmlDocument.NameTable);
            XmlNode TemperatureNode = _xmlDocument.SelectSingleNode(TemperatureXpath, NameSpaceMgr);
            XmlNode WeatherNode = _xmlDocument.SelectSingleNode(ConditionXpath, NameSpaceMgr);
            
            _condition = WeatherNode.Value;
            _temperature = TemperatureNode.InnerText.ToDecimal();
        }
        catch (System.Net.WebException we)
        {
            _condition += "?";
            
        }
        catch (System.NotSupportedException nse)
        {
            _condition += "?";
        }
        isUpdating = false;
    }

    public void Start()
    {
        if (_timer == null)
        {
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += new ElapsedEventHandler(TimerElapsed);
            _timer.Enabled = true;
        }
    }

    public void TimerElapsed(object sender, ElapsedEventArgs e)
    {
        _timer.Interval = 1000;
        if (isUpdating == false)
        {
            UpdateData();
        }
    }
}

class WeatherA
{
	public string ZIP { get; set; }
	public string Temperature = "0";
	public string Condition = "Loading...";

    public void Wunderground()
    {
        try
        {
            XmlDocument WUnderWeather = new XmlDocument();
            WUnderWeather.Load("http://api.wunderground.com/api/c0a06bca9f1999b7/conditions/q/" + ZIP + ".xml");
            XmlNamespaceManager NameSpaceMgr = new XmlNamespaceManager(WUnderWeather.NameTable);
            XmlNode WeatherNode = WUnderWeather.SelectNodes("/response/current_observation/weather", NameSpaceMgr)[0];
            XmlNode temperature = WUnderWeather.SelectNodes("/response/current_observation/temperature_string", NameSpaceMgr)[0];
            //temperature_string

            Condition = WeatherNode.InnerText;
            Temperature = temperature.InnerText;
        }
        catch (System.Net.WebException)
        {
            Condition = "??";
            Temperature = "??";
        }
    }

	public string GetHazards()
	{
		while (true)
		{
			try
			{
				string SavedLocation = "http://alerts.weather.gov/cap/wwaatmget.php?x=" + GetZone() + "&y=0";
				XmlDocument XMLWeather = new XmlDocument();
				XMLWeather.Load(SavedLocation);
				XmlNamespaceManager NameSpaceMgr = new XmlNamespaceManager(XMLWeather.NameTable);
				XmlNode DocumentNode = XMLWeather.SelectNodes("/", NameSpaceMgr)[0]["feed"]["entry"]["title"];

				XMLWeather = null;
				GC.Collect();
				return DocumentNode.InnerText;
			}
			catch
			{
				// Try again.
				Thread.Sleep(5000);
				continue;
			} 
		}
	}

	private string GetZone()
	{
		while (true)
		{
			try
			{
				List<decimal> Coords = null;
				WebClient wc = new WebClient();
				byte[] buffer = wc.DownloadData("http://forecast.weather.gov/MapClick.php?textField1=" + Coords[0] + "&textField2=" + Coords[1]);
				string[] html = Encoding.UTF8.GetString(buffer, 0, buffer.Length).Split('>');

				for (int i = 0; i < html.Length; i++)
				{
					if (html[i].Contains("Zone Area Forecast for"))// Tulsa County, OK
					{
						//<a href=\"MapClick.php?zoneid=OKZ060\"
						string[] zone = html[i - 1].Split(new char[] { '"', '\\', '=' }, StringSplitOptions.RemoveEmptyEntries);
						return zone[zone.Length - 1];
					}
				}
			}
			catch
			{
				// Try again.
				Thread.Sleep(5000);
				continue;
			}
		}
	}

	public BitmapImage GetIcon(string condition)
	{
	    using (WebClient wc = new WebClient())
	    {
		    byte[] WebPage = wc.DownloadData("https://ssl.gstatic.com/onebox/weather/64/" + condition + ".png");
		    return toBitmap(byteArrayToImage(WebPage));
	    }
	}

	public Bitmap byteArrayToImage(byte[] byteArrayIn)
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


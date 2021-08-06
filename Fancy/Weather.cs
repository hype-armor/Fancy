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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

interface IWeather
{
    Uri Uri { get; }
    string Zip { get; }
    void UpdateData();
    System.Timers.Timer Timer { set; }
    void TimerElapsed(object sender, ElapsedEventArgs e);
    Forecast forecast { get; }

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
        WebClient = new WebClient();
        WebClient.Headers.Add("user-agent", "nws@gregsemail.us");
    }
}

public class Nws2 : Nws
{
    public Nws2()
    {
        Uri = new Uri("https://api.weather.gov/gridpoints/TSA/58,74/forecast");
        Header = new KeyValuePair<string, string>("user-agent", "nws@gregsemail.us");
    }
}


public class Nws3 : Nws2
{
    public int test = 0;
    public event EventHandler<ChangedEventArgs> ChangeHappened;
    protected virtual void Changed(ChangedEventArgs e)
    {
        ChangeHappened?.Invoke(this, e);

    }

    public class ChangedEventArgs : EventArgs
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public Nws3()
    {
        ChangeHappened += Nws3_ChangeHappened;
    }

    private void Nws3_ChangeHappened(object sender, ChangedEventArgs e)
    {
        throw new NotImplementedException();
    }
}

public class Forecast
{
    public int number = 0;
    public string name = "Today";
    public string startTime;// = "2021-08-05T17:00:00-05:00";
    public string endTime;// = "2021-08-05T18:00:00-05:00";
    public bool isDaytime;// = true;
    public double temperature = 0;
    public string temperatureUnit = "F";
    public string temperatureTrend;// = null;
    public string windSpeed;// = "5 mph";
    public string windDirection;// = "S";
    public BitmapImage weatherImage
    {
        get
        { 
            return (new WeatherImage()).GetIcon(icon); ;
        }
    }
    private Uri icon;// = new Uri("https://api.weather.gov/icons/land/day/tsra_sct,60?size=medium");
    public string shortForecast = "";
    public string detailedForecast;// = "Showers and thunderstorms likely. Partly sunny, with a high near 86. South wind around 5 mph. Chance of precipitation is 60%. New rainfall amounts between a tenth and quarter of an inch possible.";

    public Forecast(JToken json)
    {
        number = Int32.Parse(json["number"].ToString());
        name = json["name"].ToString();
        startTime = json["startTime"].ToString();
        endTime = json["endTime"].ToString();
        isDaytime = bool.Parse(json["isDaytime"].ToString());
        temperature = double.Parse(json["temperature"].ToString());
        temperatureUnit = json["temperatureUnit"].ToString();
        temperatureTrend = json["temperatureTrend"].ToString();
        windSpeed = json["windSpeed"].ToString();
        windDirection = json["windDirection"].ToString();
        icon = new Uri(json["icon"].ToString());
        shortForecast = json["shortForecast"].ToString();
        detailedForecast = json["detailedForecast"].ToString();
    }
}


public class Weather : IWeather
{
    private string _zip;
    public string Zip { get => _zip; set => _zip = value; }

    private Uri _uri;
    public Uri Uri { get => _uri; set => _uri = value; }

    private WebClient _webClient;
    public WebClient WebClient { get => _webClient; set => _webClient = value; }

    private System.Timers.Timer _timer;
    public System.Timers.Timer Timer { set => _timer = value; }

    private KeyValuePair<string, string> _header;
    public KeyValuePair<string, string> Header
    {
        get { return _header; }
        set { _header = value; }
    }

    private Forecast _forecast;
    public Forecast forecast { get => _forecast; }

    private bool isUpdating = false;
    
    public void UpdateData()
    {
        isUpdating = true;
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        try
        {
            _webClient.Headers.Add(Header.Key, Header.Value);
            string result = _webClient.DownloadString(_uri);
            JObject o = JObject.Parse(result);
            JToken p = o["properties"]["periods"][0];
            _forecast = new Forecast(p);
        }
        catch (System.Net.WebException we)
        {
            //_forecast.shortForecast += "?";
            
        }
        catch (System.NotSupportedException nse)
        {
            //_forecast.shortForecast += "?";
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

public class WeatherImage
{
	public BitmapImage GetIcon(Uri uri)
	{
	    using (WebClient wc = new WebClient())
	    {
            wc.Headers.Add("user-agent", "nws@gregsemail.us");
            byte[] WebPage = wc.DownloadData(uri);
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


using System;
using System.Net;

class Time
{
	public string Now { get { return GetTimeFromDDG(); } }

	private string GetTimeFromDDG()
	{
		//https://duckduckgo.com/?q=what+time+is+it
		WebClient wc = new WebClient();
		try
		{
			char[] separators = new char[] { '<', '>', '\\', '/', '|' };

			string timeisPage = wc.DownloadString("https://duckduckgo.com/html/?q=what%20time%20is%20it");
			timeisPage = timeisPage.Remove(0, timeisPage.IndexOf("\n\n\t\n\n\n\n            ") +19);
			string[] timeisSplit = timeisPage.Split(separators, StringSplitOptions.RemoveEmptyEntries);
			string Time = timeisSplit[0].Remove(timeisSplit[0].Length - 5);
			DateTime result;
			if (DateTime.TryParse(Time, out result))
			{
				return result.ToString("t");
			}
			throw new Exception();
		}
		catch
		{
			return DateTime.Now.ToString("t");
		}
		finally
		{
			wc.Dispose();
			GC.Collect();
		}
	}
}
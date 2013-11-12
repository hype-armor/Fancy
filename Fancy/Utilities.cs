using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;
using System.Windows.Controls;

namespace ExtensionMethods
{
	public static class MyExtensions
	{
		public static bool IsDate(this TextBox value)
		{
			DateTime Datet;
			if (DateTime.TryParse(value.Text, out Datet))
			{
				return true;
			}
			else
			{
				return false;
			}

		}

		public static string ToDate(this object value)
		{
			DateTime Datet;
			if (value != "")
			{
				if (!DateTime.TryParse(value.ToString(), out Datet))
				{
					value = "Invalid Date! " + value;
					return null;
				}
				else
				{
					return Datet.ToString();
				}
			}
			else
			{
				return null;
			}
		}

		public static bool IsDecimal(this object value)
		{
			decimal result;
			if (Decimal.TryParse(value.ToString(), out result))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public static decimal ToDecimal(this object value)
		{
			decimal result;
			if (Decimal.TryParse(value.ToString(), out result))
			{
				return result;
			}
			else
			{
				return decimal.MinValue;
			}
		}

		public static bool IsInt(this object value)
		{
			int Number;
			if (int.TryParse(value.ToString(), out Number))
			{
				return true;
			}
			else
			{
				return false;
			}

		}

		public static int ToInt(this object value)
		{
			int Number;
			if (int.TryParse(value.ToString(), out Number))
			{
				return Number;
			}
			else
			{
				return Number;
			}
		}

	}
}
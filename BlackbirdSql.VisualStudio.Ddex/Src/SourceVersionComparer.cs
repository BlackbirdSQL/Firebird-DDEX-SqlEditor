using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.VisualStudio.Data.Framework;

using BlackbirdSql.Common;



namespace BlackbirdSql.VisualStudio.Ddex;

internal class SourceVersionComparer : DataSourceVersionComparer, IComparer<string>
{
	public SourceVersionComparer()
	{
		base.Comparer = this;
	}

	public int Compare(string x, string y)
	{
		if (x == null && y == null)
		{
			return 0;
		}

		if (x == null)
		{
			return -1;
		}

		if (y == null)
		{
			return 1;
		}

		string[] array = x.Split('.');
		string[] array2 = y.Split('.');
		int num = Math.Min(array.Length, array2.Length);

		for (int i = 0; i < num; i++)
		{
			int num2 = int.Parse(array[i], CultureInfo.InvariantCulture);
			int value = int.Parse(array2[i], CultureInfo.InvariantCulture);
			int num3 = num2.CompareTo(value);
			if (num3 != 0)
			{
				return num3;
			}
		}

		return 0;
	}
}

﻿// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using System;
using System.Collections.Generic;
using System.Globalization;

using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services.SupportEntities;



namespace BlackbirdSql.VisualStudio.Ddex.Ctl;


// =========================================================================================================
//										VxbSourceVersionComparer Class
//
/// <summary>
/// Implementation of <see cref="IVsDataSourceVersionComparer"/> and <see cref="IComparer{T}"/> interfaces
/// </summary>
// =========================================================================================================
public class VxbSourceVersionComparer : DataSourceVersionComparer, IComparer<string>
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - VxbSourceVersionComparer
	// ---------------------------------------------------------------------------------


	public VxbSourceVersionComparer()
	{
		// Evs.Trace(typeof(VxbSourceVersionComparer), ".ctor");

		base.Comparer = this;
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Method Implementations - VxbSourceVersionComparer
	// =========================================================================================================


	/// <summary>
	/// IComparer implementation. Compares two version strings.
	/// </summary>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <returns>-1: x < y; 0 x == y and 1: x > y</returns>
	public int Compare(string x, string y)
	{
		Evs.Trace(GetType(), nameof(Compare));

		try
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
		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
			throw;
		}

		return 0;

	}


	#endregion Method Implementations

}

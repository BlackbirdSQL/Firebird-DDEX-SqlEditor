// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


using System;
using System.Collections.Generic;

using BlackbirdSql.Core;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Core.CommandProviders;

using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services.SupportEntities;



namespace BlackbirdSql.VisualStudio.Ddex;


// =========================================================================================================
//									TConnectionEquivalencyComparer Class
//
/// <summary>
/// Implementation of <see cref="IVsDataConnectionEquivalencyComparer"/> interface
/// </summary>
// =========================================================================================================
public class TConnectionEquivalencyComparer : DataConnectionEquivalencyComparer
{

	// ---------------------------------------------------------------------------------
	#region Constructors
	// ---------------------------------------------------------------------------------


	public TConnectionEquivalencyComparer() : base()
	{
		// Diag.Trace();
	}


	#endregion Constructors





	// =========================================================================================================
	#region Method Implementations - TConnectionEquivalencyComparer
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Checks whether or not connection property objects are equivalent
	/// </summary>
	/// <param name="connectionProperties1"></param>
	/// <param name="connectionProperties2"></param>
	/// <returns>true if equivalent else false</returns>
	/// <remarks>
	/// We consider connections equivalent if they will produce the same results. The connection properties
	/// that determine this equivalency are defined in <see cref="CoreProperties.EquivalencyKeys"/>.
	/// </remarks>
	// ---------------------------------------------------------------------------------
	protected override bool AreEquivalent(IVsDataConnectionProperties connectionProperties1, IVsDataConnectionProperties connectionProperties2)
	{
		// The only interception we can make when a new query lists tables or views is when the
		// Microsoft.VisualStudio.Data.Package.DataConnectionManager checks if the connection it requires
		// is equivalent to this connection. We don't have access to the DataStore so to avoid a complete rewrite
		// we're going to hack it by invalidating the connection.
		// 
		
		if (CommandProperties.CommandObjectType != CommandProperties.DataObjectType.None)
		{
			// Diag.Trace("RESETTNG CONNECTION - COMMANDTYPE CURRENT:LAST: " + CommandProperties.CommandObjectType);
			return false;
		}


		int equivalencyValueCount = 0;
		int equivalencyKeyCount = ModelPropertySet.EquivalencyDescriberCount;
		object value1, value2;
		Describer describer;

		try
		{
			// Keep it simple. Loop thorugh each connection string and compare
			// If it's not in the other or null use default

			foreach (KeyValuePair<string, object> param in connectionProperties1)
			{
				// If all equivalency keys have been checked, break
				if (equivalencyValueCount == equivalencyKeyCount)
					break;

				// Get the correct key for the parameter in connection 1
				if ((describer = ModelPropertySet.RecursiveGetSynonymDescriber(param.Key)) == null)
				{
					ArgumentException ex = new($"Could not locate Describer for connection parameter '{param.Key}'.");
					Diag.Dug(ex);
					throw ex;
				}

				// Exclude non-applicable connection values.
				// Typically we may require a password and if it's already in, for example, the SE we have rights to it.
				// There would be no point ignoring that password just because some spurious value differs. For example 'Connection Lifetime'.

				if (!describer.IsEquivalency)
					continue;

				equivalencyValueCount++;

				// For both connections we set the value to default if it's null or doesn't exist
				if (param.Value != null)
					value1 = param.Value;
				else
					value1 = describer.DefaultValue;

				// We can't do a straight lookup on the second string because it may be a synonym so we have to loop
				// through the parameters, find the real key, and use that

				value2 = FindKeyValueInConnection(describer, connectionProperties2);

				value2 ??= describer.DefaultValue;

				if (!AreEquivalent(describer.DerivedParameter, value1, value2))
				{
					// Diag.Trace("Connection parameter '" + key + "' mismatch: '" + (value1 != null ? value1.ToString() : "null") + "' : '" + (value2 != null ? value2.ToString() : "null"));
					return false;
				}
				// Diag.Trace("Connection parameter '" + key + "' equivalent: '" + (value1 != null ? value1.ToString() : "null") + "' : '" + (value2 != null ? value2.ToString() : "null"));
			}

			if (equivalencyValueCount < equivalencyKeyCount)
			{

				foreach (KeyValuePair<string, object> param in connectionProperties2)
				{
					// If all equivalency keys have been checked, break
					if (equivalencyValueCount == equivalencyKeyCount)
						break;

					// Get the correct key for the parameter in connection 2
					if ((describer = ModelPropertySet.RecursiveGetSynonymDescriber(param.Key)) == null)
					{
						ArgumentException ex = new($"Could not locate Describer for connection parameter '{param.Key}'.");
						Diag.Dug(ex);
						throw ex;
					}



					// Exclude non-applicable connection values.
					// Typically we may require a password and if it's already in, for example, the SE we have rights to it.
					// There would be no point ignoring that password just because some spurious value differs. For example 'Connection Lifetime'. 

					if (!describer.IsEquivalency)
						continue;

					equivalencyValueCount++;

					// For both connections we set the value to default if it's null or doesn't exist
					if (param.Value != null)
						value2 = param.Value;
					else
						value2 = describer.DefaultValue;

					// We can't do a straight lookup on the first connection because it may be a synonym so we have to loop
					// through the parameters, find the real key, and use that
					value1 = FindKeyValueInConnection(describer, connectionProperties1);

					value1 ??= describer.DefaultValue;

					if (!AreEquivalent(describer.DerivedParameter, value2, value1))
					{
						// Diag.Trace("Connection2 parameter '" + key + "' mismatch: '" + (value2 != null ? value2.ToString() : "null") + "' : '" + (value1 != null ? value1.ToString() : "null"));
						return false;
					}
					// Diag.Trace("Connection2 parameter '" + key + "' equivalent: '" + (value2 != null ? value2.ToString() : "null") + "' : '" + (value1 != null ? value1.ToString() : "null"));
				}
			}

		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return false;
		}

		// Diag.Trace("Connections are equivalent");

		return true;
	}


	#endregion Method Implementations





	// =========================================================================================================
	#region Methods - TConnectionEquivalencyComparer
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Performs an equivalency comparison of to values of the connection property 'key'
	/// </summary>
	/// <param name="key"></param>
	/// <param name="value1"></param>
	/// <param name="value2"></param>
	/// <returns>true if equivalent else false</returns>
	// ---------------------------------------------------------------------------------
	protected bool AreEquivalent(string key, object value1, object value2)
	{
		// Diag.Trace();
		string text1 = value1 as string;
		string text2 = value2 as string;

		/*
		if (key == "Data Source")
		{
			if (!string.Equals(text1, text2, StringComparison.OrdinalIgnoreCase))
			{
				if (text1 != null)
					text1 = StandardizeDataSource(text1);

				if (text2 != null)
					text2 = StandardizeDataSource(text2);

			}
		}
		*/

		if (!string.Equals(text1, text2, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Finds the value of the connection property 'key' in a connection properties list
	/// given that the property key used in the list may be a synonym.
	/// </summary>
	/// <param name="key"></param>
	/// <param name="connectionProperties"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentException"></exception>
	// ---------------------------------------------------------------------------------
	private object FindKeyValueInConnection(Describer describer, IVsDataConnectionProperties connectionProperties)
	{
		// First try for matching keys
		object value;

		if (describer.Parameter != null)
		{
			if (connectionProperties.TryGetValue(describer.Parameter, out value))
				return value;
		}

		if (describer.Parameter == null || describer.Name != describer.Parameter)
		{
			if (connectionProperties.TryGetValue(describer.Name, out value))
				return value;
		}


		Describer connectionDescriber;


		foreach (KeyValuePair<string, object> parameter in connectionProperties)
		{
			if ((connectionDescriber = ModelPropertySet.RecursiveGetSynonymDescriber(parameter.Key)) == null)
			{
				ArgumentException ex = new($"Could not locate Describer for connection parameter '{parameter.Key}'.");
				Diag.Dug(ex);
				throw ex;
			}


			if (connectionDescriber.Name == describer.Name)
				return parameter.Value;
		}

		return null;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Standardizes the DataSource (Server hostname) [Deprecated]
	/// </summary>
	/// <param name="dataSource"></param>
	/// <returns>The standardized hostname</returns>
	// ---------------------------------------------------------------------------------
	protected static string StandardizeDataSource(string dataSource)
	{
		dataSource = dataSource.ToUpperInvariant();
		string[] array = new string[2] { ".", "localhost" };
		foreach (string text in array)
		{
			if (dataSource.Equals(text, StringComparison.Ordinal))
			{
				dataSource = Environment.MachineName.ToUpperInvariant(); // + dataSource[text.Length..];
				break;
			}
		}

		return dataSource;
	}


	#endregion Methods

}


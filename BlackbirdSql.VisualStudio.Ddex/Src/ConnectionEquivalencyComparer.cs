using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services.SupportEntities;

using BlackbirdSql.Common;
using BlackbirdSql.VisualStudio.Ddex.Extensions;

namespace BlackbirdSql.VisualStudio.Ddex;


// To be checked
internal class ConnectionEquivalencyComparer : DataConnectionEquivalencyComparer
{

	public ConnectionEquivalencyComparer()
	{
		// Diag.Trace();
	}

	protected override bool AreEquivalent(IVsDataConnectionProperties connectionProperties1, IVsDataConnectionProperties connectionProperties2)
	{

		// Reset the connection if we're doing a localized server explorer node query
		// It's the only way to get the built in query provider to reread the table list
		if (DataToolsCommands.ObjectType != DataToolsCommands.DataObjectType.None)
			return false;

		object value1, value2;

		try
		{
			// Keep it simple. Loop thorugh each connection string and compare
			// If it's not in the other or null use default

			foreach (KeyValuePair<string, object> param in connectionProperties1)
			{
				// Get the correct key for the parameter in connection 1
				if (!ConnectionString.Synonyms.TryGetValue(param.Key, out string key))
				{
					ArgumentException ex = new ArgumentException("Connection parameter '" + param.Key + "' in connection 1 is invalid");
					Diag.Dug(ex);
					throw (ex);

				}

				// We don't do password - not sure if underlying engine may have encrypted it
				if (string.Equals(key, "Password", StringComparison.OrdinalIgnoreCase))
					continue;

				// For both connections we set the value to default if it's null or doesn't exist
				if (param.Value != null)
					value1 = param.Value;
				else
					value1 = ConnectionString.DefaultValues[key];

				// We can't do a straight lookup on the second string because it may be a synonym so we have to loop
				// through the parameters, find the real key, and use that

				value2 = FindKeyValueInConnection(key, connectionProperties2);
				value2 ??= ConnectionString.DefaultValues[key];

				if (!AreEquivalent(key, value1, value2))
				{
					ArgumentException ex = new ArgumentException("Parameter '" + param.Key + "' in connection 1 has no matching value in connection 2");
					Diag.Dug(ex);
					return false;
				}
			}

			foreach (KeyValuePair<string, object> param in connectionProperties2)
			{
				// Get the correct key for the parameter in connection 2
				if (!ConnectionString.Synonyms.TryGetValue(param.Key, out string key))
				{
					ArgumentException ex = new ArgumentException("Connection parameter '" + param.Key + "' in connection 2 is invalid");
					Diag.Dug(ex);
					throw (ex);

				}

				if (string.Equals(key, "Password", StringComparison.OrdinalIgnoreCase))
					continue;


				// For both connections we set the value to default if it's null or doesn't exist
				if (param.Value != null)
					value2 = param.Value;
				else
					value2 = ConnectionString.DefaultValues[key];

				// We can't do a straight lookup on the first connection because it may be a synonym so we have to loop
				// through the parameters, find the real key, and use that
				value1 = FindKeyValueInConnection(key, connectionProperties1);
				value1 ??= ConnectionString.DefaultValues[key];

				if (!AreEquivalent(key, value2, value1))
				{
					ArgumentException ex = new ArgumentException("Parameter '" + param.Key + "' in connection 2 has no matching value in connection 1");
					Diag.Dug(ex);

					return false;
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


	protected bool AreEquivalent(string key, object value1, object value2)
	{
		string text1 = value1 as string;
		string text2 = value2 as string;

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

		if (!string.Equals(text1, text2, StringComparison.Ordinal))
		{
			return false;
		}

		return true;
	}



	private object FindKeyValueInConnection(string key, IVsDataConnectionProperties connectionProperties)
	{
		foreach (KeyValuePair<string, object> parameter in connectionProperties)
		{
			if (!ConnectionString.Synonyms.TryGetValue(parameter.Key, out string parameterKey))
				continue;

			if (key == parameterKey)
				return parameter.Value;
		}

		return null;
	}




	private static string StandardizeDataSource(string dataSource)
	{

		dataSource = dataSource.ToUpperInvariant();
		string[] array = new string[2] { ".", "(LOCAL)" };
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
}


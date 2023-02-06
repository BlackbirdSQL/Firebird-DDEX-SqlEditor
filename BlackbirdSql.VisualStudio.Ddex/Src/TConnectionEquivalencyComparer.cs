using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services.SupportEntities;

using BlackbirdSql.Common;
using BlackbirdSql.VisualStudio.Ddex.Schema;
using BlackbirdSql.Common.Extensions.Commands;

namespace BlackbirdSql.VisualStudio.Ddex;


// To be checked
internal class TConnectionEquivalencyComparer : DataConnectionEquivalencyComparer
{

	public TConnectionEquivalencyComparer() : base()
	{
		Diag.Trace();
	}

	protected override bool AreEquivalent(IVsDataConnectionProperties connectionProperties1, IVsDataConnectionProperties connectionProperties2)
	{
		// Reset the connection if we're doing a localized server explorer node query
		// It's the only way to get the built in query provider to reread the table list
		if (DataToolsCommands.ObjectType != DataToolsCommands.DataObjectType.None)
			return false;

		int equivalencyValueCount = 0;
		int equivalencyKeyCount = DslConnectionString.EquivalencyKeys.Count;
		object value1, value2;

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
				if (!DslConnectionString.Synonyms.TryGetValue(param.Key, out string key))
				{
					throw new ArgumentException("Connection parameter '" + param.Key + "' in connection 1 is invalid");
				}

				// Exclude non-applicable connection values.
				// Typically we may require a password and if it's already in, for example, the SE we have rights to it.
				// There would be no point ignoring that password just because some spurious value differs. For example 'Connection Lifetime'.

				if (!DslConnectionString.EquivalencyKeys.Contains(key))
					continue;

				equivalencyValueCount++;

				// For both connections we set the value to default if it's null or doesn't exist
				if (param.Value != null)
					value1 = param.Value;
				else
					value1 = DslConnectionString.DefaultValues[key];

				// We can't do a straight lookup on the second string because it may be a synonym so we have to loop
				// through the parameters, find the real key, and use that

				try
				{
					value2 = FindKeyValueInConnection(key, connectionProperties2);
				}
				catch (ArgumentException)
				{
					throw new ArgumentException("Parameter '" + param.Key + "' in connection 1 has no matching value in connection 2");
				}

				value2 ??= DslConnectionString.DefaultValues[key];

				if (!AreEquivalent(key, value1, value2))
				{
					Diag.Trace("Connection parameter '" + key + "' mismatch: '" + (value1 != null ? value1.ToString() : "null") + "' : '" + (value2 != null ? value2.ToString() : "null"));
					return false;
				}
			}

			if (equivalencyValueCount < equivalencyKeyCount)
			{

				foreach (KeyValuePair<string, object> param in connectionProperties2)
				{
					// If all equivalency keys have been checked, break
					if (equivalencyValueCount == equivalencyKeyCount)
						break;

					// Get the correct key for the parameter in connection 2
					if (!DslConnectionString.Synonyms.TryGetValue(param.Key, out string key))
					{
						throw new ArgumentException("Connection parameter '" + param.Key + "' in connection 2 is invalid");
					}

					// Exclude non-applicable connection values.
					// Typically we may require a password and if it's already in, for example, the SE we have rights to it.
					// There would be no point ignoring that password just because some spurious value differs. For example 'Connection Lifetime'. 

					if (!DslConnectionString.EquivalencyKeys.Contains(key))
						continue;

					equivalencyValueCount++;

					// For both connections we set the value to default if it's null or doesn't exist
					if (param.Value != null)
						value2 = param.Value;
					else
						value2 = DslConnectionString.DefaultValues[key];

					// We can't do a straight lookup on the first connection because it may be a synonym so we have to loop
					// through the parameters, find the real key, and use that
					try
					{
						value1 = FindKeyValueInConnection(key, connectionProperties1);
					}
					catch (ArgumentException)
					{
						throw new ArgumentException("Parameter '" + param.Key + "' in connection 2 has no matching value in connection 1");
					}

					value1 ??= DslConnectionString.DefaultValues[key];

					if (!AreEquivalent(key, value2, value1))
					{
						Diag.Trace("Connection2 parameter '" + key + "' mismatch: '" + (value2 != null ? value2.ToString() : "null") + "' : '" + (value1 != null ? value1.ToString() : "null"));
						return false;
					}
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



	private object FindKeyValueInConnection(string key, IVsDataConnectionProperties connectionProperties)
	{
		// First try for matching keys
		if (connectionProperties.TryGetValue(key, out object value))
		{
			return value;
		}

		foreach (KeyValuePair<string, object> parameter in connectionProperties)
		{
			if (!DslConnectionString.Synonyms.TryGetValue(parameter.Key, out string parameterKey))
				continue;

			if (key == parameterKey)
				return parameter.Value;
		}

		throw new ArgumentException();
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


/*
 *  Visual Studio DDEX Provider for BlackbirdSql DslClient
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.blackbirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2005 Carlos Guzman Alvarez
 *  All Rights Reserved.
 *   
 *  Contributors:
 *    Jiri Cincura (jiri@cincura.net)
 */

using Microsoft.VisualStudio.Data;
using Microsoft.VisualStudio.Data.AdoDotNet;



namespace BlackbirdSql.VisualStudio.DataTools;

internal class ObjectIdentifierConverter : AdoDotNetObjectIdentifierConverter
{
	#region · Fields ·

#pragma warning disable IDE0044 // Add readonly modifier
	private DataConnection connection;
#pragma warning restore IDE0044 // Add readonly modifier

	#endregion

	#region · Constructors ·

	public ObjectIdentifierConverter(DataConnection connection) : base(connection)
	{
		this.connection = connection;
	}

	#endregion

	#region · Protected Methods ·

	protected override string FormatPart(string typeName, object identifierPart, bool withQuotes)
	{
		string openQuote = (string)this.connection.SourceInformation[DataSourceInformation.IdentifierOpenQuote];
		string closeQuote = (string)this.connection.SourceInformation[DataSourceInformation.IdentifierCloseQuote];
		string identifier = (identifierPart is string @string) ? @string : null;

		if (withQuotes && identifier != null && !this.IsQuoted(identifier))
		{
			if (!identifier.StartsWith(openQuote))
			{
				identifier = openQuote + identifier;
			}

			if (!identifier.EndsWith(closeQuote))
			{
				identifier += openQuote;
			}
		}

		// return ((identifier != null) ? identifier : String.Empty);
		return identifier;
	}

	#endregion

	#region · Private Methods ·

	private bool IsQuoted(string value)
	{
		string openQuote = (string)this.connection.SourceInformation[DataSourceInformation.IdentifierOpenQuote];
		string closeQuote = (string)this.connection.SourceInformation[DataSourceInformation.IdentifierOpenQuote];

		return (value.StartsWith(openQuote) && value.EndsWith(closeQuote));
	}

	#endregion
}

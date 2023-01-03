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
 *  Copyright (c) 2023 GA Christos
 *  All Rights Reserved.
 *   
 *  Contributors:
 *    GA Christos
 */

using System;
using Microsoft.VisualStudio.Data.Framework.AdoDotNet;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;

using BlackbirdSql.Common;



namespace BlackbirdSql.VisualStudio.Ddex;

internal class ObjectIdentifierConverter : AdoDotNetObjectIdentifierConverter
{
	#region · Fields ·


	#endregion

	#region · Constructors ·

	public ObjectIdentifierConverter(IVsDataConnection connection) : base(connection)
	{
		Diag.Dug();
	}

	#endregion

	#region · Protected Methods ·

	protected override string FormatPart(string typeName, object identifierPart, DataObjectIdentifierFormat format)
	{
		if (identifierPart is null or DBNull)
		{
			Diag.Dug("Identifier is null");
			return null;
		}

		Diag.Dug();

		IVsDataSourceInformation sourceInformation;
		string openQuote;
		string closeQuote;

		string identifierPartString = (identifierPart is string @string) ? @string : null;

		if (format.HasFlag(DataObjectIdentifierFormat.WithQuotes) && RequiresQuoting(identifierPartString)
			&& RequiresQuoting(identifierPartString))
		{
			sourceInformation = (IVsDataSourceInformation)Site.GetService(typeof(IVsDataSourceInformation));
			openQuote = (string)sourceInformation[SourceInformation.IdentifierOpenQuote];
			closeQuote = (string)sourceInformation[SourceInformation.IdentifierCloseQuote];

			identifierPartString = openQuote + identifierPartString + closeQuote;
		}

		Diag.Dug("Converted identifier: " + identifierPartString);
		// return ((identifier != null) ? identifier : String.Empty);
		return identifierPartString;
	}

	#endregion

	#region · Private Methods ·

	#endregion
}

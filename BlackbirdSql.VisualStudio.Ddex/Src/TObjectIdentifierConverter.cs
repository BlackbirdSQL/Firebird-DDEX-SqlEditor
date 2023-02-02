/*
 *  Visual Studio DDEX Provider for FirebirdClient (BlackbirdSql)
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

internal class TObjectIdentifierConverter : AdoDotNetObjectIdentifierConverter
{
	#region � Fields �


	#endregion

	#region � Constructors �

	public TObjectIdentifierConverter() : base()
	{
		Diag.Trace();
	}

	public TObjectIdentifierConverter(IVsDataConnection connection) : base(connection)
	{
		Diag.Trace();
	}

	#endregion

	#region � Protected Methods �

	protected override string FormatPart(string typeName, object identifierPart, DataObjectIdentifierFormat format)
	{
		Diag.Trace();
		if (identifierPart is null or DBNull)
		{
			Diag.Dug(true, "Identifier is null");
			return null;
		}

		IVsDataSourceInformation sourceInformation;
		string openQuote;
		string closeQuote;

		string identifierPartString = (identifierPart is string @string) ? @string : null;

		if (format.HasFlag(DataObjectIdentifierFormat.WithQuotes) && RequiresQuoting(identifierPartString)
			&& RequiresQuoting(identifierPartString))
		{
			sourceInformation = (IVsDataSourceInformation)Site.GetService(typeof(IVsDataSourceInformation));
			openQuote = (string)sourceInformation[TSourceInformation.IdentifierOpenQuote];
			closeQuote = (string)sourceInformation[TSourceInformation.IdentifierCloseQuote];

			identifierPartString = openQuote + identifierPartString + closeQuote;
		}

		// Diag.Trace("Converted identifier: " + identifierPartString);

		return identifierPartString;
	}

	#endregion

	#region � Private Methods �

	#endregion
}
//
// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)
//


using System;
using Microsoft.VisualStudio.Data.Framework.AdoDotNet;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;

using BlackbirdSql.Common;



namespace BlackbirdSql.VisualStudio.Ddex;


// =========================================================================================================
//									TObjectIdentifierConverter Class
//
/// <summary>
/// Implementation of <see cref="IVsDataObjectIdentifierConverter"/> interface
/// </summary>
// =========================================================================================================
internal class TObjectIdentifierConverter : AdoDotNetObjectIdentifierConverter
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - TObjectIdentifierConverter
	// ---------------------------------------------------------------------------------


	public TObjectIdentifierConverter() : base()
	{
		// Diag.Trace();
	}


	public TObjectIdentifierConverter(IVsDataConnection connection) : base(connection)
	{
		// Diag.Trace();
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Method Implementations - TObjectIdentifierConverter
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Override to customize the formatting that is added to the identifier part.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected override string FormatPart(string typeName, object identifierPart, DataObjectIdentifierFormat format)
	{
		Diag.Trace();
		if (identifierPart is null or DBNull)
		{
			Diag.Dug(true, typeName + ": Identifier is null");
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


	#endregion Method Implementations

}

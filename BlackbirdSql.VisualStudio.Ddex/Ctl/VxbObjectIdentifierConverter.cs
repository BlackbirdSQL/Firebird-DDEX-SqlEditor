// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Data.Framework.AdoDotNet;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;



namespace BlackbirdSql.VisualStudio.Ddex.Ctl;


// =========================================================================================================
//									VxbObjectIdentifierConverter Class
//
/// <summary>
/// Implementation of <see cref="IVsDataObjectIdentifierConverter"/> interface
/// </summary>
// =========================================================================================================
public class VxbObjectIdentifierConverter : AdoDotNetObjectIdentifierConverter
{

	// ---------------------------------------------------------------------------------
	#region Constructors / Destructors - VxbObjectIdentifierConverter
	// ---------------------------------------------------------------------------------


	public VxbObjectIdentifierConverter() : base()
	{
		// Evs.Trace(typeof(VxbObjectIdentifierConverter), ".ctor");

	}


	public VxbObjectIdentifierConverter(IVsDataConnection connection) : base(connection)
	{
		// Evs.Trace(typeof(VxbObjectIdentifierConverter), ".ctor(IVsDataConnection)");

	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Method Implementations - VxbObjectIdentifierConverter
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Override to customize the formatting that is added to the identifier part.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected override string FormatPart(string typeName, object identifierPart, DataObjectIdentifierFormat format)
	{
		// Evs.Trace();

		if (identifierPart is null or DBNull)
		{
			// Evs.Trace(typeName + ": Identifier is null");
			return null;
		}

		IVsDataSourceInformation sourceInformation;
		string openQuote;
		string closeQuote;

		string identifierPartString = (identifierPart is string @string) ? @string : null;

		if (format.HasFlag(DataObjectIdentifierFormat.WithQuotes) && RequiresQuoting(identifierPartString))
		{
			sourceInformation = (IVsDataSourceInformation)Site.GetService(typeof(IVsDataSourceInformation));
			openQuote = (string)sourceInformation[VxbSourceInformation.IdentifierOpenQuote];
			closeQuote = (string)sourceInformation[VxbSourceInformation.IdentifierCloseQuote];

			identifierPartString = openQuote + identifierPartString + closeQuote;
		}

		// Evs.Trace("typeName: " + typeName + " DataObjectIdentifierFormat: " + format + " Converted identifier: " + identifierPartString);

		return identifierPartString;
	}



	//
	// Summary:
	//     Divides the specified string version of an identifier into a set of formatted
	//     identifier parts by using the identifier separator character.
	//
	// Parameters:
	//   typeName:
	//     The name of an object type.
	//
	//   identifier:
	//     The identifier to split into parts.
	//
	// Returns:
	//     An array of string values representing each an identifier extracted from the
	//     input string.
	protected override string[] SplitIntoParts(string typeName, string identifier)
	{
		// Evs.Trace("typeName: " + typeName + " identifier: " + identifier);

		return Regex.Split(identifier, "\\.");

		// return base.SplitIntoParts(typeName, identifier);
	}

	//
	// Summary:
	//     Removes formatting of identifier parts, such as trimming leading and trailing
	//     spaces and removing quotation marks.
	//
	// Parameters:
	//   typeName:
	//     The name of a data object type.
	//
	//   identifierPart:
	//     A formatted identifier part.
	//
	// Returns:
	//     The new identifier part, without its formatting.
	protected override object UnformatPart(string typeName, string identifierPart)
	{
		// Evs.Trace("typeName: " + typeName + " identifier: " + identifierPart);
		return base.UnformatPart(typeName, identifierPart);
	}



	//
	// Summary:
	//     Indicates whether the specified identifier part requires quotation marks.
	//
	// Parameters:
	//   identifierPart:
	//     The name of the identifier part
	//
	// Returns:
	//     A Boolean value indicating whether the identifier part requires quotation marks.
	protected override bool RequiresQuoting(string identifierPart)
	{
		return true;
	}



	//
	// Summary:
	//     Concatenates identifier parts into a string and inserts the separator character
	//     in between.
	//
	// Parameters:
	//   typeName:
	//     The type of the database object.
	//
	//   identifierParts:
	//     An array of formatted identifier parts.
	//
	//   format:
	//     A value of the Microsoft.VisualStudio.Data.Services.DataObjectIdentifierFormat
	//     enumeration. This value is used to check whether the string is used for display
	//     purposes only. If so, the format of the string is changed.
	//
	// Returns:
	//     The concatenated string containing all identifiers in the given order.
	protected override string BuildString(string typeName, string[] identifierParts, DataObjectIdentifierFormat format)
	{
		string str = base.BuildString(typeName, identifierParts, format);

		// Evs.Trace("typeName: " + typeName + " DataObjectIdentifierFormat: " + format + " Result: " + str);
		return str;
	}

	//
	// Summary:
	//     Handles the Microsoft.VisualStudio.Data.Framework.DataSiteableObject`1.SiteChanged
	//     event.
	//
	// Parameters:
	//   e:
	//     An System.EventArgs object containing the event data.
	protected override void OnSiteChanged(EventArgs e)
	{
		Evs.Trace(GetType(), nameof(OnSiteChanged));

		base.OnSiteChanged(e);
	}


	#endregion Method Implementations

}

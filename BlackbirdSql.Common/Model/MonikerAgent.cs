
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;

using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Enums;
using Microsoft.VisualStudio.Data.Services;


namespace BlackbirdSql.Common.Model;

// =========================================================================================================
//
//											MonikerAgent Class
//
/// <summary>
/// This class is inherited from Core.MonikerAgent and centrally controls document monikers for the
/// extension.
/// A BlackbirdSql moniker is a url (fbsql://) based on the Server, Database and identifier keys of a
/// document.
/// </summary>
/// <remarks>
/// Monikers must be uniquely identifiable by the server->databasePath document moniker prefix and
/// also by the identifier keys of a document.
/// </remarks>
// =========================================================================================================
public class MonikerAgent : Core.Model.MonikerAgent
{

	[Browsable(false)]
	public string MiscDocumentMoniker => BuildMiscDocumentMoniker();




	public MonikerAgent(IVsDataExplorerNode node, bool isUnique = false, bool alternate = false)
		: base(node, isUnique, alternate)
	{
	}


	public MonikerAgent(string server, string database, EnModelObjectType objectType,
			IList<string> identifierList, bool isUnique = false, bool alternate = false)
		: base(server, database, objectType, identifierList, isUnique, alternate)
	{
	}




	public string BuildMiscDocumentMoniker()
	{
		string text = DocumentMoniker;
		string extension = Path.GetExtension(text);
		string arg = text[..text.LastIndexOf(extension, StringComparison.OrdinalIgnoreCase)];
		string result = text;

		if (IsUnique)
		{
			for (int i = 1; i < 1000; i++)
			{
				if (!RdtManager.Instance.IsFileInRdt(result))
				{
					break;
				}
				result = ((i > 100) ? string.Format(CultureInfo.InvariantCulture, "{0}_{1}{2}", arg, DateTime.Now.Ticks, extension) : string.Format(CultureInfo.InvariantCulture, "{0}_{1}{2}", arg, i, extension));
			}
		}
		else
		{
			result = string.Format(CultureInfo.InvariantCulture, "{0}{1}", arg, extension);
		}

		// Tracer.Trace(GetType(), "BuildMiscDocumentMoniker()", "Misc DocumentMoniker: {0}", result);

		return result;
	}



	public static string BuildMiscDocumentMoniker(IVsDataExplorerNode node,
	ref IList<string> identifierArray, bool isUnique, bool alternate)
	{
		MonikerAgent moniker = new(node, isUnique, alternate);
		identifierArray = moniker.Identifier;

		Tracer.Trace(typeof(MonikerAgent), "BuildMiscDocumentMoniker(IVsDataExplorerNode)", "ObjectName: {0}", moniker.ObjectName);

		string result = moniker.MiscDocumentMoniker;

		// Tracer.Trace(typeof(MonikerAgent), "BuildMiscDocumentMoniker(IVsDataExplorerNode)", "DocumentMoniker: {0}", result);

		return result;
	}



	public static string BuildMiscDocumentMoniker(string server, string database, EnModelObjectType elementType,
		ref IList<string> identifierArray, bool isUnique, bool alternate)
	{
		MonikerAgent moniker = new(server, database, elementType, identifierArray, isUnique, alternate);
		identifierArray = moniker.Identifier;

		string result = moniker.MiscDocumentMoniker;

		// Tracer.Trace(typeof(MonikerAgent), "BuildMiscDocumentMoniker(server, database, identifierArray)", "DocumentMoniker: {0}", result);

		return result;
	}

}

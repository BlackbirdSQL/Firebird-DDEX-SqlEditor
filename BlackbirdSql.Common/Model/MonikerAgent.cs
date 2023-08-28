
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Core.Interfaces;

using Microsoft.VisualStudio.Data.Services;




namespace BlackbirdSql.Common.Model;


public class MonikerAgent : Core.Model.MonikerAgent
{

	public string MiscDocumentMoniker => BuildMiscDocumentMoniker();


	public MonikerAgent(bool isUnique = true, bool lowercaseDatabase = false) : base(isUnique, lowercaseDatabase)
	{
	}


	public MonikerAgent(string fbSqlUrl, bool isUnique = true, bool lowercaseDatabase = false)
		: base(fbSqlUrl, isUnique, lowercaseDatabase)
	{
	}

	public MonikerAgent(IBPropertyAgent ci, bool isUnique = true, bool lowercaseDatabase = false)
		: base(ci, isUnique, lowercaseDatabase)
	{
	}


	public MonikerAgent(IVsDataExplorerNode node, bool isUnique = true, bool lowercaseDatabase = false)
		: base(node, isUnique, lowercaseDatabase)
	{
	}

	public MonikerAgent(string server, string database, string user, EnModelObjectType objectType,
		IList<string> identifierList, bool isUnique = true, bool lowercaseDatabase = false)
		: base(server, database, user, objectType, identifierList, isUnique, lowercaseDatabase)
	{
	}


	public MonikerAgent(string server, string database, string user, EnModelObjectType objectType,
		object[] identifier, bool isUnique = true, bool lowercaseDatabase = false)
		: base(server, database, user, objectType, identifier, isUnique, lowercaseDatabase)
	{
	}


	public string BuildMiscDocumentMoniker()
	{
		string text = DocumentMoniker;
		string extension = Path.GetExtension(text);
		string arg = text[..text.LastIndexOf(extension, StringComparison.OrdinalIgnoreCase)];
		string text2 = text;

		if (_IsUnique)
		{
			for (int i = 1; i < 1000; i++)
			{
				if (!RdtManager.Instance.IsFileInRdt(text2))
				{
					break;
				}
				text2 = ((i > 100) ? string.Format(CultureInfo.InvariantCulture, "{0}_{1}{2}", arg, DateTime.Now.Ticks, extension) : string.Format(CultureInfo.InvariantCulture, "{0}_{1}{2}", arg, i, extension));
			}
		}
		else
		{
			text2 = string.Format(CultureInfo.InvariantCulture, "{0}{1}", arg, extension);
		}
		return text2;
	}



	public static string BuildMiscDocumentMoniker(IVsDataExplorerNode node,
		ref IList<string> identifierArray, bool isUnique)
	{
		MonikerAgent moniker = new(node, isUnique);
		identifierArray = moniker.Identifier;

		return moniker.MiscDocumentMoniker;
	}



	public static string BuildMiscDocumentMoniker(string server, string database, string user, EnModelObjectType elementType,
		ref IList<string> identifierArray, bool isUnique)
	{
		MonikerAgent moniker = new(server, database, user, elementType, identifierArray, isUnique);
		identifierArray = moniker.Identifier;

		return moniker.MiscDocumentMoniker;
	}
}

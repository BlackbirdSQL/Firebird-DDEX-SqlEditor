
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;

using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Ctl.Interfaces;

using Microsoft.VisualStudio.Data.Services;


namespace BlackbirdSql.Common.Model;

public class MonikerAgent : Core.Model.MonikerAgent
{

	public string MiscDocumentMoniker => BuildMiscDocumentMoniker();



	public MonikerAgent(bool isUnique = false, bool alternate = false)
		: base(isUnique, alternate)
	{
	}


	public MonikerAgent(IDbConnection connection) : base(connection)
	{
	}


	public MonikerAgent(IBPropertyAgent ci) : base(ci)
	{
	}


	public MonikerAgent(IVsDataExplorerNode node, bool isUnique = false, bool alternate = false)
		: base(node, isUnique, alternate)
	{
	}

	public MonikerAgent(string server, string database, EnModelObjectType objectType,
			IList<string> identifierList, bool isUnique = false, bool alternate = false)
		: base(server, database, objectType, identifierList, isUnique, alternate)
	{
	}

	public MonikerAgent(string displayMember, string dataset, string server, int port, EnDbServerType serverType, string database,
			string user, string password, string role, string charset, int dialect, EnModelObjectType objectType, bool isUnique)
		: base(displayMember, dataset, server, port, serverType, database, user, password, role, charset, dialect, objectType, null, isUnique)
	{
	}

	public MonikerAgent(string displayMember, string dataset, string server, int port, EnDbServerType serverType,
			string database, string user, string password, string role, string charset, int dialect,
			EnModelObjectType objectType, object[] identifier, bool isUnique)
		: base(displayMember, dataset, server, port, serverType, database, user, password, role, charset, dialect, objectType, identifier, isUnique)
	{
	}




	public string BuildMiscDocumentMoniker()
	{
		string text = DocumentMoniker;
		string extension = Path.GetExtension(text);
		string arg = text[..text.LastIndexOf(extension, StringComparison.OrdinalIgnoreCase)];
		string result = text;

		if (_IsUnique)
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

		Tracer.Trace(GetType(), "BuildMiscDocumentMoniker()", "Misc DocumentMoniker: {0}", result);

		return result;
	}



	public static string BuildMiscDocumentMoniker(IVsDataExplorerNode node,
		ref IList<string> identifierArray, bool isUnique, bool alternate)
	{
		MonikerAgent moniker = new(node, isUnique, alternate);
		identifierArray = moniker.Identifier;

		string result = moniker.MiscDocumentMoniker;

		Tracer.Trace(typeof(MonikerAgent), "BuildMiscDocumentMoniker(IVsDataExplorerNode)", "DocumentMoniker: {0}", result);

		return result;
	}



	public static string BuildMiscDocumentMoniker(string server, string database, EnModelObjectType elementType,
		ref IList<string> identifierArray, bool isUnique, bool alternate)
	{
		MonikerAgent moniker = new(server, database, elementType, identifierArray, isUnique, alternate);
		identifierArray = moniker.Identifier;

		string result = moniker.MiscDocumentMoniker;

		Tracer.Trace(typeof(MonikerAgent), "BuildMiscDocumentMoniker(server, database, identifierArray)", "DocumentMoniker: {0}", result);

		return result;
	}
}

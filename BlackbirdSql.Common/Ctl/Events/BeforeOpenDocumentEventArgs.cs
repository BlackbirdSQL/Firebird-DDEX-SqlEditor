// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// Microsoft.VisualStudio.Data.Tools.Package, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Package.DesignerServices.BeforeOpenDocumentEventArgs
using System;
using System.Collections.Generic;
using System.Data.Common;
using BlackbirdSql.Core.Model.Enums;

namespace BlackbirdSql.Common.Ctl.Events;

public class BeforeOpenDocumentEventArgs : EventArgs
{
	public string Moniker { get; private set; }

	public DbConnectionStringBuilder ConnectionString { get; private set; }

	public DatabaseLocation Location { get; private set; }

	public IList<string> IdentifierList { get; private set; }

	public EnModelObjectType ElementType { get; private set; }
	public EnModelTargetType TargetType { get; private set; }

	public BeforeOpenDocumentEventArgs(string mkDocument, DbConnectionStringBuilder scsb,
		IList<string> identifierList, EnModelObjectType elementType, EnModelTargetType targetType)
	{
		/*
		if (string.IsNullOrWhiteSpace(mkDocument))
		{
			throw ExceptionFactory.CreateArgumentException("mkDocument");
		}
		if (scsb == null)
		{
			throw ExceptionFactory.CreateArgumentException("scsb");
		}
		*/
		Moniker = mkDocument;
		ConnectionString = scsb;
		IdentifierList = identifierList;
		ElementType = elementType;
		Location = default;
		TargetType = targetType;
	}

	public BeforeOpenDocumentEventArgs(string mkDocument, DatabaseLocation dbl,
		IList<string> identifierList, EnModelObjectType elementType, EnModelTargetType targetType)
	{
		Moniker = mkDocument;
		ConnectionString = null;
		IdentifierList = identifierList;
		ElementType = elementType;
		Location = dbl;
		TargetType = targetType;
	}

}

// Microsoft.VisualStudio.Data.Tools.Package, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Package.DesignerServices.BeforeOpenDocumentEventArgs

using System;
using System.Collections.Generic;
using System.Data.Common;
using BlackbirdSql.Sys.Enums;



namespace BlackbirdSql.Shared.Events;


public class BeforeOpenDocumentEventArgs(string mkDocument, DbConnectionStringBuilder scsb,
	IList<string> identifierList, EnModelObjectType elementType, EnModelTargetType targetType) : EventArgs
{
	public string MkDocument { get; private set; } = mkDocument;

	public DbConnectionStringBuilder ConnectionString { get; private set; } = scsb;

	public IList<string> IdentifierList { get; private set; } = identifierList;

	public EnModelObjectType ElementType { get; private set; } = elementType;
	public EnModelTargetType TargetType { get; private set; } = targetType;



}

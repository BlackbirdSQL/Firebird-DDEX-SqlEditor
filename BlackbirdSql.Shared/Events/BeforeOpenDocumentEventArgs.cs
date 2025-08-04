// Microsoft.VisualStudio.Data.Tools.Package, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Package.DesignerServices.BeforeOpenDocumentEventArgs

using System;
using System.Collections.Generic;
using System.Data.Common;
using BlackbirdSql.Sys.Enums;



namespace BlackbirdSql.Shared.Events;


internal class BeforeOpenDocumentEventArgs(string mkDocument, DbConnectionStringBuilder scsb,
	IList<string> identifierList, EnModelObjectType elementType, EnModelTargetType targetType) : EventArgs
{
	internal string MkDocument { get; private set; } = mkDocument;

	internal DbConnectionStringBuilder ConnectionString { get; private set; } = scsb;

	internal IList<string> IdentifierList { get; private set; } = identifierList;

	internal EnModelObjectType ElementType { get; private set; } = elementType;
	internal EnModelTargetType TargetType { get; private set; } = targetType;



}

// Microsoft.SqlServer.ConnectionDlg.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.ConnectionDlg.Core.Extensibility.IDependencyManager

using System.Collections.Generic;
using BlackbirdSql.Core.Model;

namespace BlackbirdSql.Core.Interfaces;


public interface IBDependencyManager
{
	IEnumerable<ExportableDescriptor<T>> GetServiceDescriptors<T>(IBServerDefinition serviceMetadata = null) where T : IBExportable;
}

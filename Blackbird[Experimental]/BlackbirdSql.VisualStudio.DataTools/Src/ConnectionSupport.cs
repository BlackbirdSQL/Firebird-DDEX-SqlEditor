/*
 *  Visual Studio DDEX Provider for FirebirdClient
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2005 Carlos Guzman Alvarez
 *  All Rights Reserved.
 *   
 *  Contributors:
 *    Jiri Cincura (jiri@cincura.net)
 */

using System;
using BlackbirdSql.Common;
using Microsoft.VisualStudio.Data;
using Microsoft.VisualStudio.Data.AdoDotNet;

namespace BlackbirdSql.VisualStudio.DataTools;

internal class ConnectionSupport : AdoDotNetConnectionSupport
{
	#region · Constructors ·

	public ConnectionSupport() : base(SystemData.Invariant)
	{
		Diag.Trace();
	}

	#endregion

	#region · Protected Methods ·


	protected override DataSourceInformation CreateDataSourceInformation()
	{
		Diag.Trace();

		return new SourceInformation(base.Site as DataConnection);
	}

	protected override DataObjectIdentifierConverter CreateObjectIdentifierConverter()
	{
		return new ObjectIdentifierConverter(base.Site as DataConnection);
	}

	protected override object GetServiceImpl(Type serviceType)
	{
		Diag.Trace("Service: " + serviceType.FullName);

		if (serviceType == typeof(DataViewSupport))
		{
			return new ViewSupport();
		}
		else if (serviceType == typeof(DataObjectSupport))
		{
			return new ObjectSupport();
		}
		else if (serviceType == typeof(DataObjectIdentifierResolver))
		{
			return new ObjectIdentifierResolver(base.Site as DataConnection);
		}

		return base.GetServiceImpl(serviceType);
	}


	#endregion
}

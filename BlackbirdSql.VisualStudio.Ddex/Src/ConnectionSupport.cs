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
 *  Copyright (c) 2023 GA Christos
 *  All Rights Reserved.
 *   
 *  Contributors:
 *    GA Christos
 */

using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Data.Framework.AdoDotNet;
using Microsoft.VisualStudio.Data.Services.SupportEntities;

using BlackbirdSql.Common;



namespace BlackbirdSql.VisualStudio.Ddex;

internal class ConnectionSupport : AdoDotNetConnectionSupport
{
	#region · Constructors ·

	public ConnectionSupport() : base()
	{
		Diag.Trace();
	}

	#endregion

	#region · Protected Methods ·

	protected override object CreateService(IServiceContainer container, Type serviceType)
	{
		Diag.Trace(String.Format("CreateService({0})", serviceType.FullName));


		if (serviceType == typeof(IVsDataSourceInformation))
		{
			return new SourceInformation(Site);
		}
		else if (serviceType == typeof(IVsDataViewSupport))
		{
			return new ViewSupport("BlackbirdSql.VisualStudio.Ddex.ViewSupport", typeof(ConnectionSupport).Assembly);
			// return new ViewSupport();
		}
		else if (serviceType == typeof(IVsDataObjectSupport))
		{
			return new ObjectSupport(Site);
		}
		else if (serviceType == typeof(IVsDataObjectIdentifierConverter))
		{
			return new ObjectIdentifierConverter(Site);
		}
		else if (serviceType == typeof(IVsDataObjectMemberComparer))
		{
			return new ObjectMemberComparer(Site);
		}
		else if (serviceType == typeof(IVsDataObjectIdentifierResolver))
		{
			return new ObjectIdentifierResolver(Site);
		}
		else if (serviceType == typeof(IVsDataMappedObjectConverter))
		{
			return new MappedObjectConverter(Site);
		}

		return base.CreateService(container, serviceType);
	}

	#endregion
}

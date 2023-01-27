/*
 *  Visual Studio DDEX Provider for FirebirdClient (BlackbirdSql)
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.blackbirdsql.org/index.php?op=doc&id=idpl
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
using System.Data.Common.BlackbirdSql;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services.SupportEntities;

using BlackbirdSql.Common;
using FirebirdSql.Data.FirebirdClient;

namespace BlackbirdSql.VisualStudio.Ddex;

[Guid(Configuration.PackageData.ObjectFactoryServiceGuid)]


public interface IProviderObjectFactory
{
}

public sealed class ProviderObjectFactory : DataProviderObjectFactory, IProviderObjectFactory
{
	#region · Constructors ·

	public ProviderObjectFactory()
	{
	}

	#endregion

	#region · Methods ·

	public override object CreateObject(Type objType)
	{
		// Diag.Trace("CreateObject: " + objType.FullName);


		if (objType == typeof(IVsDataConnectionSupport))
		{
			return new ConnectionSupport();
		}
		else if (objType == typeof(IVsDataConnectionUIControl))
		{
			return new ConnectionUIControl();
		}
		else if (objType == typeof(IVsDataConnectionProperties) || objType == typeof(IVsDataConnectionUIProperties))
		{
			return new ConnectionProperties();
		}
		else if (objType == typeof(IVsDataSourceInformation))
		{
			return new SourceInformation();
		}
		else if (objType == typeof(IVsDataViewSupport))
		{
			return new DataViewSupport("BlackbirdSql.VisualStudio.Ddex.ViewSupport", typeof(ProviderObjectFactory).Assembly);
			// return new ViewSupport();
		}
		else if (objType == typeof(IVsDataConnectionEquivalencyComparer))
		{
			return new ConnectionEquivalencyComparer();
		}

		return null;
	}

	#endregion
}

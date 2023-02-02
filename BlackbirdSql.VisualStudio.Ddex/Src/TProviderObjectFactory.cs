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
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Data.Framework;
using Microsoft.VisualStudio.Data.Services;
using Microsoft.VisualStudio.Data.Services.SupportEntities;

using BlackbirdSql.Common;



namespace BlackbirdSql.VisualStudio.Ddex;

[Guid(Configuration.PackageData.ObjectFactoryServiceGuid)]


public interface IProviderObjectFactory
{
}

public sealed class TProviderObjectFactory : DataProviderObjectFactory, IProviderObjectFactory
{
	#region · Constructors ·

	public TProviderObjectFactory() : base()
	{
		Diag.Trace();
	}

	#endregion

	#region · Methods ·

	public override object CreateObject(Type objType)
	{
		/* Uncomment this and change PackageSupportedObjects._UseFactoryOnly to true to debug implementations
		 * Don't forget to do the same for TConnectionSupport if you do.
		 * 
		if (objType == typeof(IVsDataConnectionSupport))
		{
			Diag.Trace();
			return new TConnectionSupport();
		}
		else if (objType == typeof(IVsDataConnectionUIControl))
		{
			Diag.Trace();
			return new TConnectionUIControl();
		}
		else if (objType == typeof(IVsDataConnectionPromptDialog))
		{
			Diag.Trace();
			return new TConnectionPromptDialog();
		}
		else if (objType == typeof(IVsDataConnectionProperties))
		{
			Diag.Trace();
			return new TConnectionProperties();
		}
		else if (objType == typeof(IVsDataConnectionUIProperties))
		{
			Diag.Trace();
			return new TConnectionUIProperties();
		}
		else if (objType == typeof(IVsDataObjectIdentifierResolver))
		{
			Diag.Trace();
			return new TObjectIdentifierResolver((IVsDataConnection)Site);
		}
		else if (objType == typeof(IVsDataObjectSupport))
		{
			Diag.Trace();
			return new TObjectSupport((IVsDataConnection)Site);
		}
		else if (objType == typeof(IVsDataSourceInformation))
		{
			Diag.Trace();
			return new TSourceInformation();
		}
		else if (objType == typeof(IVsDataViewSupport))
		{
			Diag.Trace();
			return new DataViewSupport("BlackbirdSql.VisualStudio.Ddex.TViewSupport", typeof(ProviderObjectFactory).Assembly);
			// return new TViewSupport();
		}
		else if (objType == typeof(IVsDataConnectionEquivalencyComparer))
		{
			Diag.Trace();
			return new TConnectionEquivalencyComparer();
		}
		*/

		Diag.Dug(true, objType.FullName + " is not supported");
		return null;
	}

	#endregion
}

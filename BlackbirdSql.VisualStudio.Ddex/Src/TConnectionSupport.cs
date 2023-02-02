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
using Microsoft.VisualStudio.Data.Core;
using Microsoft.VisualStudio.Data.Services;

namespace BlackbirdSql.VisualStudio.Ddex;

internal class TConnectionSupport : AdoDotNetConnectionSupport
{


	#region · Constructors ·

	public TConnectionSupport() : base()
	{
		Diag.Trace();
	}

	#endregion

	#region · Protected Methods ·

	protected override object CreateService(IServiceContainer container, Type serviceType)
	{
		/* Uncomment this and change PackageSupportedObjects._UseFactoryOnly to true to debug implementations
		 * Don't forget to do the same for the ProviderObjectFactory if you do.
		 * 
		if (serviceType == typeof(IVsDataSourceInformation))
		{
			Diag.Trace();
			return new TSourceInformation(Site);
		}
		else if (serviceType == typeof(IVsDataObjectSelector))
		{
			return new TObjectSelector(Site);
		}
		else if (serviceType == typeof(IVsDataObjectMemberComparer))
		{
			Diag.Trace();
			return new TObjectMemberComparer(Site);
		}
		else if (serviceType == typeof(IVsDataObjectIdentifierConverter))
		{
			Diag.Trace();
			return new TObjectIdentifierConverter(Site);
		}
		else if (serviceType == typeof(IVsDataMappedObjectConverter))
		{
			Diag.Trace();
			return new TMappedObjectConverter(Site);
		}
		*/

		Diag.Dug(true, serviceType.FullName + " is not directly supported");

		return base.CreateService(container, serviceType);
	}


	public override bool Open(bool doPromptCheck)
	{

		try
		{
			Diag.Trace("Prompt: " + doPromptCheck + " IsOpen: " + State + " ConnectionString: " + ConnectionString);
			if (State == DataConnectionState.Open)
				return true;

			IVsDataConnectionUIProperties vsDataConnectionUIProperties =
				((IVsDataSiteableObject<IVsDataProvider>)this).Site.CreateObject<IVsDataConnectionUIProperties>(base.Site.Source);

			vsDataConnectionUIProperties.Parse(ConnectionString);

			if (doPromptCheck && !vsDataConnectionUIProperties.IsComplete)
			{
				return false;
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}

		return base.Open(doPromptCheck);
	}

	#endregion
}

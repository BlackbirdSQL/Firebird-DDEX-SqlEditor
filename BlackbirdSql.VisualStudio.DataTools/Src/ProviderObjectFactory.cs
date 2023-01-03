/*
 *  Visual Studio DDEX Provider for BlackbirdSql DslClient
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
 *  Copyright (c) 2005 Carlos Guzman Alvarez
 *  All Rights Reserved.
 *   
 *  Contributors:
 *    Jiri Cincura (jiri@cincura.net)
 */

using System;
using System.Data.Common.BlackbirdSql;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Data;
using Microsoft.VisualStudio.Data.AdoDotNet;

using BlackbirdSql.Common;



namespace BlackbirdSql.VisualStudio.DataTools;

[Guid(Configuration.PackageData.ObjectFactoryServiceGuid)]

public interface IProviderObjectFactory
{
}


internal class ProviderObjectFactory : AdoDotNetProviderObjectFactory, IProviderObjectFactory
{
	#region · Constructors ·

	public ProviderObjectFactory() : base()
	{
		Diag.Dug();


		// Inline attempt to register factory on machine
		if (DbProviderFactoriesEx.DbProviderFactoriesRegisterFactory(typeof(Data.DslClient.DslProviderFactory),
			Data.Properties.Resources.Provider_ShortDisplayName, Data.Properties.Resources.Provider_DisplayName))
		{
			Diag.Dug("DbProviderFactory registration completed during ProviderObjectFactory instantiation");

			AppDomain.CurrentDomain.AssemblyResolve += (_, args) =>
			{
				var assembly = typeof(Data.DslClient.DslProviderFactory).Assembly;

				if (args.Name == assembly.FullName)
					Diag.Dug(true, "Dsl Provider Factory failed to load: " + assembly.FullName);

				return args.Name == assembly.FullName ? assembly : null;
			};

		}
		else
		{
			Diag.Dug("DbProviderFactory registration not completed during ProviderObjectFactory instantiation. Factory already registered");
		}

	}

	#endregion

	#region · Methods ·

	public override object CreateObject(Type objType)
	{
		Diag.Dug("Object type: " + objType.FullName);

		if (objType == typeof(DataConnectionSupport))
		{
			return new ConnectionSupport();
		}
		else if (objType == typeof(DataConnectionUIControl) || objType == typeof(DataConnectionPromptDialog))
		{
			return new ConnectionUIControl();
		}
		else if (objType == typeof(DataConnectionProperties))
		{
			return new ConnectionProperties();
		}

		return base.CreateObject(objType);
	}

	#endregion
}

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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Microsoft.VisualStudio.Data.Framework.AdoDotNet;

using BlackbirdSql.Common;
using FirebirdSql.Data.Common;



namespace BlackbirdSql.VisualStudio.Ddex;

internal class TConnectionUIProperties : AdoDotNetConnectionProperties
{


	#region · Properties ·

	/// <summary>
	/// Checks the connection string and activates PromptDialog if incomplete
	/// </summary>
	public override bool IsComplete
	{
		get
		{
			Diag.Trace("ProtectedMandatoryProperties required");
			// This has to be ProtectedMandatoryProperties for password PromptDialog to be activated
			foreach (string property in Schema.DslConnectionString.ProtectedMandatoryProperties)
			{
				if (!TryGetValue(property, out object value) || string.IsNullOrEmpty((string)value))
				{
					return false;
				}
			}

			return true;
		}
	}

	#endregion



	#region · Constructors ·

	public TConnectionUIProperties() : base()
	{
		Diag.Trace();
	}

	#endregion


}

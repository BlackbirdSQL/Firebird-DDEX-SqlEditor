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


using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.VisualStudio.Data.AdoDotNet;

using BlackbirdSql.Common;
using BlackbirdSql.Data.Common;



namespace BlackbirdSql.VisualStudio.DataTools;

internal class ConnectionProperties : AdoDotNetConnectionProperties
{

	#region · Properties ·

	public override bool IsComplete
    {
        get 
        {
            foreach (string property in GetBasicProperties())
            {
                if (this[property] is not string || (this[property] as string).Length == 0)
                {
                    return false;
                }
            }

            return true;
        }
    }

	#endregion



	#region · Constructors ·

	public ConnectionProperties() : this(null) { }

	public ConnectionProperties(string connectionString) : base(Configuration.PackageData.Invariant, connectionString)
	{
		Diag.Dug(false);

		TypeDescriptor.AddProvider
			(
				new AssociatedMetadataTypeTypeDescriptionProvider(typeof(ConnectionStringBuilder)),
				typeof(ConnectionStringBuilder)
			);
	}

	#endregion



	#region · Methods ·


	public override string[] GetBasicProperties()
    {
        return new string[] { "Data Source", "Initial Catalog", "User ID", "Password" };
    }


	#endregion


}

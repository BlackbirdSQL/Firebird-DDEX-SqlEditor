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
using System.Data;
using Microsoft.VisualStudio.Data.Framework.AdoDotNet;
using Microsoft.VisualStudio.Data.Services;

using BlackbirdSql.Common;



namespace BlackbirdSql.VisualStudio.Ddex;


public class MappedObjectConverter : AdoDotNetMappedObjectConverter
{

	public MappedObjectConverter() : base()
	{
		Diag.Trace();
	}
	public MappedObjectConverter(IVsDataConnection connection) : base(connection)
	{
		Diag.Trace();
	}

	protected override DbType GetDbTypeFromNativeType(string nativeType)
	{
		Diag.Trace();
		DataRow[] rows = this.DataTypes.Select(String.Format("TypeName = '{0}'", nativeType));

		if (rows != null && rows.Length > 0)
		{
			return (DbType)Convert.ToInt32(rows[0]["DbType"]);
		}

		return base.GetDbTypeFromNativeType(nativeType);
	}

	protected override int GetProviderTypeFromNativeType(string nativeType)
	{
		Diag.Trace();
		DataRow[] rows = this.DataTypes.Select(String.Format("TypeName = '{0}'", nativeType));

		if (rows != null && rows.Length > 0)
		{
			return Convert.ToInt32(rows[0]["ProviderDbType"]);
		}

		return base.GetProviderTypeFromNativeType(nativeType);
	}

	protected override Type GetFrameworkTypeFromNativeType(string nativeType)
	{
		Diag.Trace();
		DataRow[] rows = this.DataTypes.Select(String.Format("TypeName = '{0}'", nativeType));

		if (rows != null && rows.Length > 0)
		{
			return Type.GetType(rows[0]["DataType"].ToString());
		}

		return base.GetFrameworkTypeFromNativeType(nativeType);
	}
}

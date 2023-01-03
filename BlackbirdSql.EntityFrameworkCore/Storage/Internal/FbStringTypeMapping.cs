/*
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/BlackbirdSQL/NETProvider/raw/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$Authors = Jiri Cincura (jiri@cincura.net)

using System.Data;
using System.Data.Common;
using BlackbirdSql.Data.DslClient;
using Microsoft.EntityFrameworkCore.Storage;

namespace BlackbirdSql.Data.Entity.Core.Storage.Internal;

public class FbStringTypeMapping : StringTypeMapping
{
	readonly DslDbType _fbDbType;

	public FbStringTypeMapping(string storeType, DbType dbType, DslDbType fbDbType, int? size = null, bool unicode = true)
		: base(storeType, dbType, unicode: unicode, size: size)
	{
		_fbDbType = fbDbType;
	}

	protected FbStringTypeMapping(RelationalTypeMappingParameters parameters, DslDbType fbDbType)
		: base(parameters)
	{
		_fbDbType = fbDbType;
	}

	protected override void ConfigureParameter(DbParameter parameter)
	{
		((DslParameter)parameter).FbDbType = _fbDbType;
	}

	protected override string GenerateNonNullSqlLiteral(object value)
	{
		var svalue = value.ToString();
		return IsUnicode
			? $"_UTF8'{EscapeSqlLiteral(svalue)}'"
			: $"'{EscapeSqlLiteral(svalue)}'";
	}

	protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
		=> new FbStringTypeMapping(parameters, _fbDbType);
}

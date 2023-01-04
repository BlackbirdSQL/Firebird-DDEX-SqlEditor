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

//$Authors = Siegfried Pammer (siegfried.pammer@gmail.com), Jiri Cincura (jiri@cincura.net)

using System;
using System.Data.Common;
using BlackbirdSql.Data.DslClient;
using Microsoft.EntityFrameworkCore.Storage;

namespace BlackbirdSql.EntityFrameworkCore.Storage.Internal;

public class FbTimeSpanTypeMapping : TimeSpanTypeMapping
{
	readonly DslDbType _fbDbType;

	public FbTimeSpanTypeMapping(string storeType, DslDbType fbDbType)
		: base(storeType)
	{
		_fbDbType = fbDbType;
	}

	protected FbTimeSpanTypeMapping(RelationalTypeMappingParameters parameters, DslDbType fbDbType)
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
		switch (_fbDbType)
		{
			case DslDbType.Time:
				return $"CAST('{value:hh\\:mm\\:ss\\.ffff}' AS TIME)";
			default:
				throw new ArgumentOutOfRangeException(nameof(_fbDbType), $"{nameof(_fbDbType)}={_fbDbType}");
		}
	}

	protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
		=> new FbTimeSpanTypeMapping(parameters, _fbDbType);
}

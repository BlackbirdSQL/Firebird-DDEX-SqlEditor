/*
 *	This is a replication of the FirebirdClient Class which is not accessible
 *	
 *    The contents of this file are subject to the Initial
 *    Developer's Public License Version 1.0 (the "License");
 *    you may not use this file except in compliance with the
 *    License. You may obtain a copy of the License at
 *    https://github.com/FirebirdSQL/NETProvider/raw/master/license.txt.
 *
 *    Software distributed under the License is distributed on
 *    an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either
 *    express or implied. See the License for the specific
 *    language governing rights and limitations under the License.
 *
 *    All Rights Reserved.
 */

//$OriginalAuthors = Carlos Guzman Alvarez, Jiri Cincura (jiri@cincura.net)

using System;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Types;

using BlackbirdSql.Core;



namespace BlackbirdSql.VisualStudio.Ddex.Model;

internal static class DslTypeHelper
{

	public static int GetSqlTypeFromBlrType(int type)
	{
		switch (type)
		{
			case IscCodes.blr_varying:
			case IscCodes.blr_varying2:
				return IscCodes.SQL_VARYING;

			case IscCodes.blr_text:
			case IscCodes.blr_text2:
			case IscCodes.blr_cstring:
			case IscCodes.blr_cstring2:
				return IscCodes.SQL_TEXT;

			case IscCodes.blr_short:
				return IscCodes.SQL_SHORT;

			case IscCodes.blr_long:
				return IscCodes.SQL_LONG;

			case IscCodes.blr_quad:
				return IscCodes.SQL_QUAD;

			case IscCodes.blr_int64:
			case IscCodes.blr_blob_id:
				return IscCodes.SQL_INT64;

			case IscCodes.blr_double:
				return IscCodes.SQL_DOUBLE;

			case IscCodes.blr_d_float:
				return IscCodes.SQL_D_FLOAT;

			case IscCodes.blr_float:
				return IscCodes.SQL_FLOAT;

			case IscCodes.blr_sql_date:
				return IscCodes.SQL_TYPE_DATE;

			case IscCodes.blr_sql_time:
				return IscCodes.SQL_TYPE_TIME;

			case IscCodes.blr_timestamp:
				return IscCodes.SQL_TIMESTAMP;

			case IscCodes.blr_blob:
				return IscCodes.SQL_BLOB;

			case IscCodes.blr_bool:
				return IscCodes.SQL_BOOLEAN;

			case IscCodes.blr_ex_timestamp_tz:
				return IscCodes.SQL_TIMESTAMP_TZ_EX;

			case IscCodes.blr_timestamp_tz:
				return IscCodes.SQL_TIMESTAMP_TZ;

			case IscCodes.blr_sql_time_tz:
				return IscCodes.SQL_TIME_TZ;

			case IscCodes.blr_ex_time_tz:
				return IscCodes.SQL_TIME_TZ_EX;

			case IscCodes.blr_dec64:
				return IscCodes.SQL_DEC16;

			case IscCodes.blr_dec128:
				return IscCodes.SQL_DEC34;

			case IscCodes.blr_int128:
				return IscCodes.SQL_INT128;

			default:
				throw InvalidDataType(type);
		}
	}

	public static string GetDataTypeName(DslDbDataType type)
	{
		switch (type)
		{
			case DslDbDataType.Array:
				return "ARRAY";

			case DslDbDataType.Binary:
				return "BLOB";

			case DslDbDataType.Text:
				return "BLOB SUB_TYPE 1";

			case DslDbDataType.Char:
			case DslDbDataType.Guid:
				return "CHAR";

			case DslDbDataType.VarChar:
				return "VARCHAR";

			case DslDbDataType.SmallInt:
				return "SMALLINT";

			case DslDbDataType.Integer:
				return "INTEGER";

			case DslDbDataType.Float:
				return "FLOAT";

			case DslDbDataType.Double:
				return "DOUBLE PRECISION";

			case DslDbDataType.BigInt:
				return "BIGINT";

			case DslDbDataType.Numeric:
				return "NUMERIC";

			case DslDbDataType.Decimal:
				return "DECIMAL";

			case DslDbDataType.Date:
				return "DATE";

			case DslDbDataType.Time:
				return "TIME";

			case DslDbDataType.TimeStamp:
				return "TIMESTAMP";

			case DslDbDataType.Boolean:
				return "BOOLEAN";

			case DslDbDataType.TimeStampTZ:
			case DslDbDataType.TimeStampTZEx:
				return "TIMESTAMP WITH TIME ZONE";

			case DslDbDataType.TimeTZ:
			case DslDbDataType.TimeTZEx:
				return "TIME WITH TIME ZONE";

			case DslDbDataType.Dec16:
			case DslDbDataType.Dec34:
				return "DECFLOAT";

			case DslDbDataType.Int128:
				return "INT128";

			default:
				throw InvalidDataType((int)type);
		}
	}

	public static Type GetTypeFromDbDataType(DslDbDataType type)
	{
		switch (type)
		{
			case DslDbDataType.Array:
				return typeof(System.Array);

			case DslDbDataType.Binary:
				return typeof(System.Byte[]);

			case DslDbDataType.Text:
			case DslDbDataType.Char:
			case DslDbDataType.VarChar:
				return typeof(System.String);

			case DslDbDataType.Guid:
				return typeof(System.Guid);

			case DslDbDataType.SmallInt:
				return typeof(System.Int16);

			case DslDbDataType.Integer:
				return typeof(System.Int32);

			case DslDbDataType.BigInt:
				return typeof(System.Int64);

			case DslDbDataType.Float:
				return typeof(System.Single);

			case DslDbDataType.Double:
				return typeof(System.Double);

			case DslDbDataType.Numeric:
			case DslDbDataType.Decimal:
				return typeof(System.Decimal);

			case DslDbDataType.Date:
			case DslDbDataType.TimeStamp:
				return typeof(System.DateTime);

			case DslDbDataType.Time:
				return typeof(System.TimeSpan);

			case DslDbDataType.Boolean:
				return typeof(System.Boolean);

			case DslDbDataType.TimeStampTZ:
			case DslDbDataType.TimeStampTZEx:
				return typeof(FbZonedDateTime);

			case DslDbDataType.TimeTZ:
			case DslDbDataType.TimeTZEx:
				return typeof(FbZonedTime);

			case DslDbDataType.Dec16:
			case DslDbDataType.Dec34:
				return typeof(FbDecFloat);

			case DslDbDataType.Int128:
				return typeof(System.Numerics.BigInteger);

			default:
				throw InvalidDataType((int)type);
		}
	}

	public static FbDbType GetFbDataTypeFromType(Type type)
	{
		if (type.IsEnum)
		{
			return GetFbDataTypeFromType(Enum.GetUnderlyingType(type));
		}

		if (type == typeof(System.DBNull))
		{
			return FbDbType.VarChar;
		}

		if (type == typeof(System.String))
		{
			return FbDbType.VarChar;
		}
		else if (type == typeof(System.Char))
		{
			return FbDbType.Char;
		}
		else if (type == typeof(System.Boolean))
		{
			return FbDbType.Boolean;
		}
		else if (type == typeof(System.Byte) || type == typeof(System.SByte) || type == typeof(System.Int16) || type == typeof(System.UInt16))
		{
			return FbDbType.SmallInt;
		}
		else if (type == typeof(System.Int32) || type == typeof(System.UInt32))
		{
			return FbDbType.Integer;
		}
		else if (type == typeof(System.Int64) || type == typeof(System.UInt64))
		{
			return FbDbType.BigInt;
		}
		else if (type == typeof(System.Single))
		{
			return FbDbType.Float;
		}
		else if (type == typeof(System.Double))
		{
			return FbDbType.Double;
		}
		else if (type == typeof(System.Decimal))
		{
			return FbDbType.Decimal;
		}
		else if (type == typeof(System.DateTime))
		{
			return FbDbType.TimeStamp;
		}
		else if (type == typeof(System.TimeSpan))
		{
			return FbDbType.Time;
		}
		else if (type == typeof(System.Guid))
		{
			return FbDbType.Guid;
		}
		else if (type == typeof(FbZonedDateTime))
		{
			return FbDbType.TimeStampTZ;
		}
		else if (type == typeof(FbZonedTime))
		{
			return FbDbType.TimeTZ;
		}
		else if (type == typeof(FbDecFloat))
		{
			return FbDbType.Dec34;
		}
		else if (type == typeof(System.Numerics.BigInteger))
		{
			return FbDbType.Int128;
		}
		else if (type == typeof(System.Byte[]))
		{
			return FbDbType.Binary;
		}
#if NET6_0_OR_GREATER
		else if (type == typeof(System.DateOnly))
		{
			return FbDbType.Date;
		}
#endif
#if NET6_0_OR_GREATER
		else if (type == typeof(System.TimeOnly))
		{
			return FbDbType.Time;
		}
#endif
		else
		{
			ArgumentException ex = new($"Unknown type: {type}.");
			Diag.Dug(ex);
			throw ex;
		}
	}



	public static DslDbDataType GetDbDataTypeFromBlrType(int type, int subType, int scale)
	{
		return GetDbDataTypeFromSqlType(GetSqlTypeFromBlrType(type), subType, scale);
	}

	public static DslDbDataType GetDbDataTypeFromSqlType(int type, int subType, int scale, int? length = null, Charset charset = null)
	{
		// Special case for Guid handling
		if ((type == IscCodes.SQL_TEXT || type == IscCodes.SQL_VARYING) && length == 16 && (charset?.IsOctetsCharset ?? false))
		{
			return DslDbDataType.Guid;
		}

		switch (type)
		{
			case IscCodes.SQL_TEXT:
				return DslDbDataType.Char;

			case IscCodes.SQL_VARYING:
				return DslDbDataType.VarChar;

			case IscCodes.SQL_SHORT:
				if (subType == 2)
				{
					return DslDbDataType.Decimal;
				}
				else if (subType == 1)
				{
					return DslDbDataType.Numeric;
				}
				else if (scale < 0)
				{
					return DslDbDataType.Decimal;
				}
				else
				{
					return DslDbDataType.SmallInt;
				}

			case IscCodes.SQL_LONG:
				if (subType == 2)
				{
					return DslDbDataType.Decimal;
				}
				else if (subType == 1)
				{
					return DslDbDataType.Numeric;
				}
				else if (scale < 0)
				{
					return DslDbDataType.Decimal;
				}
				else
				{
					return DslDbDataType.Integer;
				}

			case IscCodes.SQL_QUAD:
			case IscCodes.SQL_INT64:
				if (subType == 2)
				{
					return DslDbDataType.Decimal;
				}
				else if (subType == 1)
				{
					return DslDbDataType.Numeric;
				}
				else if (scale < 0)
				{
					return DslDbDataType.Decimal;
				}
				else
				{
					return DslDbDataType.BigInt;
				}

			case IscCodes.SQL_FLOAT:
				return DslDbDataType.Float;

			case IscCodes.SQL_DOUBLE:
			case IscCodes.SQL_D_FLOAT:
				if (subType == 2)
				{
					return DslDbDataType.Decimal;
				}
				else if (subType == 1)
				{
					return DslDbDataType.Numeric;
				}
				else if (scale < 0)
				{
					return DslDbDataType.Decimal;
				}
				else
				{
					return DslDbDataType.Double;
				}

			case IscCodes.SQL_BLOB:
				if (subType == 1)
				{
					return DslDbDataType.Text;
				}
				else
				{
					return DslDbDataType.Binary;
				}

			case IscCodes.SQL_TIMESTAMP:
				return DslDbDataType.TimeStamp;

			case IscCodes.SQL_TYPE_TIME:
				return DslDbDataType.Time;

			case IscCodes.SQL_TYPE_DATE:
				return DslDbDataType.Date;

			case IscCodes.SQL_ARRAY:
				return DslDbDataType.Array;

			case IscCodes.SQL_NULL:
				return DslDbDataType.Null;

			case IscCodes.SQL_BOOLEAN:
				return DslDbDataType.Boolean;

			case IscCodes.SQL_TIMESTAMP_TZ:
				return DslDbDataType.TimeStampTZ;

			case IscCodes.SQL_TIMESTAMP_TZ_EX:
				return DslDbDataType.TimeStampTZEx;

			case IscCodes.SQL_TIME_TZ:
				return DslDbDataType.TimeTZ;

			case IscCodes.SQL_TIME_TZ_EX:
				return DslDbDataType.TimeTZEx;

			case IscCodes.SQL_DEC16:
				return DslDbDataType.Dec16;

			case IscCodes.SQL_DEC34:
				return DslDbDataType.Dec34;

			case IscCodes.SQL_INT128:
				if (subType == 2)
				{
					return DslDbDataType.Decimal;
				}
				else if (subType == 1)
				{
					return DslDbDataType.Numeric;
				}
				else if (scale < 0)
				{
					return DslDbDataType.Decimal;
				}
				else
				{
					return DslDbDataType.Int128;
				}

			default:
				throw InvalidDataType(type);
		}
	}





	public static Exception InvalidDataType(int type)
	{
		return new ArgumentException($"Invalid data type: {type}.");
	}


}

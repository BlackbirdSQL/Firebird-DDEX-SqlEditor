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
using BlackbirdSql.Sys;
using BlackbirdSql.Sys.Enums;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Types;


namespace BlackbirdSql.Data.Model;

internal static class DbTypeHelper
{

	internal static int GetSqlTypeFromBlrType(int type)
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

	internal static string GetDataTypeName(EnDbDataType type)
	{
		switch (type)
		{
			case EnDbDataType.Array:
				return "ARRAY";

			case EnDbDataType.Binary:
				return "BLOB";

			case EnDbDataType.Text:
				return "BLOB SUB_TYPE 1";

			case EnDbDataType.Char:
			case EnDbDataType.Guid:
				return "CHAR";

			case EnDbDataType.VarChar:
				return "VARCHAR";

			case EnDbDataType.SmallInt:
				return "SMALLINT";

			case EnDbDataType.Integer:
				return "INTEGER";

			case EnDbDataType.Float:
				return "FLOAT";

			case EnDbDataType.Double:
				return "DOUBLE PRECISION";

			case EnDbDataType.BigInt:
				return "BIGINT";

			case EnDbDataType.Numeric:
				return "NUMERIC";

			case EnDbDataType.Decimal:
				return "DECIMAL";

			case EnDbDataType.Date:
				return "DATE";

			case EnDbDataType.Time:
				return "TIME";

			case EnDbDataType.TimeStamp:
				return "TIMESTAMP";

			case EnDbDataType.Boolean:
				return "BOOLEAN";

			case EnDbDataType.TimeStampTZ:
			case EnDbDataType.TimeStampTZEx:
				return "TIMESTAMP WITH TIME ZONE";

			case EnDbDataType.TimeTZ:
			case EnDbDataType.TimeTZEx:
				return "TIME WITH TIME ZONE";

			case EnDbDataType.Dec16:
			case EnDbDataType.Dec34:
				return "DECFLOAT";

			case EnDbDataType.Int128:
				return "INT128";

			default:
				throw InvalidDataType((int)type);
		}
	}

	internal static Type GetTypeFromDbDataType(EnDbDataType type)
	{
		switch (type)
		{
			case EnDbDataType.Array:
				return typeof(System.Array);

			case EnDbDataType.Binary:
				return typeof(System.Byte[]);

			case EnDbDataType.Text:
			case EnDbDataType.Char:
			case EnDbDataType.VarChar:
				return typeof(System.String);

			case EnDbDataType.Guid:
				return typeof(System.Guid);

			case EnDbDataType.SmallInt:
				return typeof(System.Int16);

			case EnDbDataType.Integer:
				return typeof(System.Int32);

			case EnDbDataType.BigInt:
				return typeof(System.Int64);

			case EnDbDataType.Float:
				return typeof(System.Single);

			case EnDbDataType.Double:
				return typeof(System.Double);

			case EnDbDataType.Numeric:
			case EnDbDataType.Decimal:
				return typeof(System.Decimal);

			case EnDbDataType.Date:
			case EnDbDataType.TimeStamp:
				return typeof(System.DateTime);

			case EnDbDataType.Time:
				return typeof(System.TimeSpan);

			case EnDbDataType.Boolean:
				return typeof(System.Boolean);

			case EnDbDataType.TimeStampTZ:
			case EnDbDataType.TimeStampTZEx:
				return typeof(FbZonedDateTime);

			case EnDbDataType.TimeTZ:
			case EnDbDataType.TimeTZEx:
				return typeof(FbZonedTime);

			case EnDbDataType.Dec16:
			case EnDbDataType.Dec34:
				return typeof(FbDecFloat);

			case EnDbDataType.Int128:
				return typeof(System.Numerics.BigInteger);

			default:
				throw InvalidDataType((int)type);
		}
	}

	internal static FbDbType GetFbDataTypeFromType(Type type)
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
			Diag.Ex(ex);
			throw ex;
		}
	}



	internal static EnDbDataType GetDbDataTypeFromBlrType(int type, int subType, int scale)
	{
		return GetDbDataTypeFromSqlType(GetSqlTypeFromBlrType(type), subType, scale);
	}

	internal static EnDbDataType GetDbDataTypeFromSqlType(int type, int subType, int scale, int? length = null, Charset charset = null)
	{
		// Special case for Guid handling
		if ((type == IscCodes.SQL_TEXT || type == IscCodes.SQL_VARYING) && length == 16 && (charset?.IsOctetsCharset ?? false))
		{
			return EnDbDataType.Guid;
		}

		switch (type)
		{
			case IscCodes.SQL_TEXT:
				return EnDbDataType.Char;

			case IscCodes.SQL_VARYING:
				return EnDbDataType.VarChar;

			case IscCodes.SQL_SHORT:
				if (subType == 2)
				{
					return EnDbDataType.Decimal;
				}
				else if (subType == 1)
				{
					return EnDbDataType.Numeric;
				}
				else if (scale < 0)
				{
					return EnDbDataType.Decimal;
				}
				else
				{
					return EnDbDataType.SmallInt;
				}

			case IscCodes.SQL_LONG:
				if (subType == 2)
				{
					return EnDbDataType.Decimal;
				}
				else if (subType == 1)
				{
					return EnDbDataType.Numeric;
				}
				else if (scale < 0)
				{
					return EnDbDataType.Decimal;
				}
				else
				{
					return EnDbDataType.Integer;
				}

			case IscCodes.SQL_QUAD:
			case IscCodes.SQL_INT64:
				if (subType == 2)
				{
					return EnDbDataType.Decimal;
				}
				else if (subType == 1)
				{
					return EnDbDataType.Numeric;
				}
				else if (scale < 0)
				{
					return EnDbDataType.Decimal;
				}
				else
				{
					return EnDbDataType.BigInt;
				}

			case IscCodes.SQL_FLOAT:
				return EnDbDataType.Float;

			case IscCodes.SQL_DOUBLE:
			case IscCodes.SQL_D_FLOAT:
				if (subType == 2)
				{
					return EnDbDataType.Decimal;
				}
				else if (subType == 1)
				{
					return EnDbDataType.Numeric;
				}
				else if (scale < 0)
				{
					return EnDbDataType.Decimal;
				}
				else
				{
					return EnDbDataType.Double;
				}

			case IscCodes.SQL_BLOB:
				if (subType == 1)
				{
					return EnDbDataType.Text;
				}
				else
				{
					return EnDbDataType.Binary;
				}

			case IscCodes.SQL_TIMESTAMP:
				return EnDbDataType.TimeStamp;

			case IscCodes.SQL_TYPE_TIME:
				return EnDbDataType.Time;

			case IscCodes.SQL_TYPE_DATE:
				return EnDbDataType.Date;

			case IscCodes.SQL_ARRAY:
				return EnDbDataType.Array;

			case IscCodes.SQL_NULL:
				return EnDbDataType.Null;

			case IscCodes.SQL_BOOLEAN:
				return EnDbDataType.Boolean;

			case IscCodes.SQL_TIMESTAMP_TZ:
				return EnDbDataType.TimeStampTZ;

			case IscCodes.SQL_TIMESTAMP_TZ_EX:
				return EnDbDataType.TimeStampTZEx;

			case IscCodes.SQL_TIME_TZ:
				return EnDbDataType.TimeTZ;

			case IscCodes.SQL_TIME_TZ_EX:
				return EnDbDataType.TimeTZEx;

			case IscCodes.SQL_DEC16:
				return EnDbDataType.Dec16;

			case IscCodes.SQL_DEC34:
				return EnDbDataType.Dec34;

			case IscCodes.SQL_INT128:
				if (subType == 2)
				{
					return EnDbDataType.Decimal;
				}
				else if (subType == 1)
				{
					return EnDbDataType.Numeric;
				}
				else if (scale < 0)
				{
					return EnDbDataType.Decimal;
				}
				else
				{
					return EnDbDataType.Int128;
				}

			default:
				throw InvalidDataType(type);
		}
	}


	internal static string ConvertDataTypeToSql(string type, int length, int precision, int scale)
	{
		type = type.ToUpper();

		string sql = type;

		switch (type)
		{
			case "BINARY":
			case "CHAR":
			case "CHARACTER":
			case "VARBINARY":
			case "BINARY VARYING":
			case "VARCHAR":
			case "CHAR VARYING":
			case "CHARACTER VARYING":
			case "BLOB SUB_TYPE 1":
			case "BLOB SUB_TYPE 2":
			case "BLOB SUB_TYPE 3":
				sql += $"({length})";
				break;
			case "DECFLOAT":
				sql += $"({precision})";
				break;
			case "DECIMAL":
			case "NUMERIC":
				sql += $"({precision}, {scale})";
				break;
			case "FLOAT":
				if (precision > 0)
					sql += $"({precision})";
				break;
			default:
				break;
		}

		return sql;
	}

	internal static string ConvertDataTypeToSql(object type, object length, object precision, object scale)
	{
		return ConvertDataTypeToSql((string)type, (int)length, (int)precision, (int)scale);
	}



		internal static Exception InvalidDataType(int type)
	{
		return new ArgumentException($"Invalid data type: {type}.");
	}


}

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
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Types;


namespace BlackbirdSql.Data.Model;

internal static class DbTypeHelper
{

	public static int GetSqlTypeFromBlrType(int type)
	{
		switch (type)
		{
			case DbIscCodes.blr_varying:
			case DbIscCodes.blr_varying2:
				return DbIscCodes.SQL_VARYING;

			case DbIscCodes.blr_text:
			case DbIscCodes.blr_text2:
			case DbIscCodes.blr_cstring:
			case DbIscCodes.blr_cstring2:
				return DbIscCodes.SQL_TEXT;

			case DbIscCodes.blr_short:
				return DbIscCodes.SQL_SHORT;

			case DbIscCodes.blr_long:
				return DbIscCodes.SQL_LONG;

			case DbIscCodes.blr_quad:
				return DbIscCodes.SQL_QUAD;

			case DbIscCodes.blr_int64:
			case DbIscCodes.blr_blob_id:
				return DbIscCodes.SQL_INT64;

			case DbIscCodes.blr_double:
				return DbIscCodes.SQL_DOUBLE;

			case DbIscCodes.blr_d_float:
				return DbIscCodes.SQL_D_FLOAT;

			case DbIscCodes.blr_float:
				return DbIscCodes.SQL_FLOAT;

			case DbIscCodes.blr_sql_date:
				return DbIscCodes.SQL_TYPE_DATE;

			case DbIscCodes.blr_sql_time:
				return DbIscCodes.SQL_TYPE_TIME;

			case DbIscCodes.blr_timestamp:
				return DbIscCodes.SQL_TIMESTAMP;

			case DbIscCodes.blr_blob:
				return DbIscCodes.SQL_BLOB;

			case DbIscCodes.blr_bool:
				return DbIscCodes.SQL_BOOLEAN;

			case DbIscCodes.blr_ex_timestamp_tz:
				return DbIscCodes.SQL_TIMESTAMP_TZ_EX;

			case DbIscCodes.blr_timestamp_tz:
				return DbIscCodes.SQL_TIMESTAMP_TZ;

			case DbIscCodes.blr_sql_time_tz:
				return DbIscCodes.SQL_TIME_TZ;

			case DbIscCodes.blr_ex_time_tz:
				return DbIscCodes.SQL_TIME_TZ_EX;

			case DbIscCodes.blr_dec64:
				return DbIscCodes.SQL_DEC16;

			case DbIscCodes.blr_dec128:
				return DbIscCodes.SQL_DEC34;

			case DbIscCodes.blr_int128:
				return DbIscCodes.SQL_INT128;

			default:
				throw InvalidDataType(type);
		}
	}

	public static string GetDataTypeName(EnDbDataType type)
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

	public static Type GetTypeFromDbDataType(EnDbDataType type)
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



	public static EnDbDataType GetDbDataTypeFromBlrType(int type, int subType, int scale)
	{
		return GetDbDataTypeFromSqlType(GetSqlTypeFromBlrType(type), subType, scale);
	}

	public static EnDbDataType GetDbDataTypeFromSqlType(int type, int subType, int scale, int? length = null, Charset charset = null)
	{
		// Special case for Guid handling
		if ((type == DbIscCodes.SQL_TEXT || type == DbIscCodes.SQL_VARYING) && length == 16 && (charset?.IsOctetsCharset ?? false))
		{
			return EnDbDataType.Guid;
		}

		switch (type)
		{
			case DbIscCodes.SQL_TEXT:
				return EnDbDataType.Char;

			case DbIscCodes.SQL_VARYING:
				return EnDbDataType.VarChar;

			case DbIscCodes.SQL_SHORT:
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

			case DbIscCodes.SQL_LONG:
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

			case DbIscCodes.SQL_QUAD:
			case DbIscCodes.SQL_INT64:
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

			case DbIscCodes.SQL_FLOAT:
				return EnDbDataType.Float;

			case DbIscCodes.SQL_DOUBLE:
			case DbIscCodes.SQL_D_FLOAT:
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

			case DbIscCodes.SQL_BLOB:
				if (subType == 1)
				{
					return EnDbDataType.Text;
				}
				else
				{
					return EnDbDataType.Binary;
				}

			case DbIscCodes.SQL_TIMESTAMP:
				return EnDbDataType.TimeStamp;

			case DbIscCodes.SQL_TYPE_TIME:
				return EnDbDataType.Time;

			case DbIscCodes.SQL_TYPE_DATE:
				return EnDbDataType.Date;

			case DbIscCodes.SQL_ARRAY:
				return EnDbDataType.Array;

			case DbIscCodes.SQL_NULL:
				return EnDbDataType.Null;

			case DbIscCodes.SQL_BOOLEAN:
				return EnDbDataType.Boolean;

			case DbIscCodes.SQL_TIMESTAMP_TZ:
				return EnDbDataType.TimeStampTZ;

			case DbIscCodes.SQL_TIMESTAMP_TZ_EX:
				return EnDbDataType.TimeStampTZEx;

			case DbIscCodes.SQL_TIME_TZ:
				return EnDbDataType.TimeTZ;

			case DbIscCodes.SQL_TIME_TZ_EX:
				return EnDbDataType.TimeTZEx;

			case DbIscCodes.SQL_DEC16:
				return EnDbDataType.Dec16;

			case DbIscCodes.SQL_DEC34:
				return EnDbDataType.Dec34;

			case DbIscCodes.SQL_INT128:
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


	public static string ConvertDataTypeToSql(string type, int length, int precision, int scale)
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

	public static string ConvertDataTypeToSql(object type, object length, object precision, object scale)
	{
		return ConvertDataTypeToSql((string)type, (int)length, (int)precision, (int)scale);
	}



		public static Exception InvalidDataType(int type)
	{
		return new ArgumentException($"Invalid data type: {type}.");
	}


}

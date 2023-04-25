#include "pch.h"
#include "Gram.h"


namespace BlackbirdDsl {




List<SysStr^>^ Gram::BackTrackingTypes::get()
{
	if (_BackTrackingTypes != nullptr)
		return _BackTrackingTypes;



	_BackTrackingTypes = gcnew List<SysStr^>(gcnew array<SysStr^>
	{
		Expressions::EXPRESSION, Expressions::SUBQUERY,
		Expressions::BRACKET_EXPRESSION, Expressions::TABLE_EXPRESSION,
		Expressions::RECORD, Expressions::IN_LIST,
		Expressions::MATCH_ARGUMENTS, Expressions::TABLE,
		Expressions::TEMPORARY_TABLE, Expressions::COLUMN_TYPE,
		Expressions::COLDEF, Expressions::PRIMARY_KEY,
		Expressions::CONSTRAINT, Expressions::COLUMN_LIST,
		Expressions::CHECK, Expressions::COLLATE, Expressions::LIKE,
		Expressions::INDEX, Expressions::INDEX_TYPE,
		Expressions::INDEX_SIZE, Expressions::INDEX_PARSER,
		Expressions::FOREIGN_KEY, Expressions::REFERENCE,
		Expressions::PARTITION, Expressions::PARTITION_HASH,
		Expressions::PARTITION_COUNT, Expressions::PARTITION_KEY,
		Expressions::PARTITION_KEY_ALGORITHM, Expressions::PARTITION_RANGE,
		Expressions::PARTITION_LIST, Expressions::PARTITION_DEF,
		Expressions::PARTITION_VALUES, Expressions::SUBPARTITION_DEF,
		Expressions::PARTITION_DATA_DIR, Expressions::PARTITION_INDEX_DIR,
		Expressions::PARTITION_COMMENT, Expressions::PARTITION_MAX_ROWS,
		Expressions::PARTITION_MIN_ROWS, Expressions::SUBPARTITION_COMMENT,
		Expressions::SUBPARTITION_DATA_DIR, Expressions::SUBPARTITION_INDEX_DIR,
		Expressions::SUBPARTITION_KEY, Expressions::SUBPARTITION_KEY_ALGORITHM,
		Expressions::SUBPARTITION_MAX_ROWS, Expressions::SUBPARTITION_MIN_ROWS,
		Expressions::SUBPARTITION, Expressions::SUBPARTITION_HASH,
		Expressions::SUBPARTITION_COUNT, Expressions::CHARSET,
		Expressions::ENGINE, Expressions::QUERY,
		Expressions::INDEX_ALGORITHM, Expressions::INDEX_LOCK,
		Expressions::SUBQUERY_FACTORING, Expressions::CUSTOM_FUNCTION,
		Expressions::SIMPLE_FUNCTION, Expressions::TRIGGER,
		Expressions::NEXT, Expressions::VALUE
	});

	return _BackTrackingTypes;

}



IDictionary<SysStr^, Gram::Token^>^ Gram::Tokens::get()
{
	if (_Tokens != nullptr)
		return _Tokens;

	int i = 0;

	_Tokens = gcnew Dictionary<SysStr^, Token^>(532, System::StringComparer::OrdinalIgnoreCase);

	_Tokens->Add((SysStr^)Values::TOK_ABS, gcnew Token(Keys::TOK_ABS, FlagsTokenCategory::FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_ABSOLUTE, gcnew Token(Keys::TOK_ABSOLUTE, FlagsTokenCategory::FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_ACCENT, gcnew Token(Keys::TOK_ACCENT, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_ACOS, gcnew Token(Keys::TOK_ACOS, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_ACOSH, gcnew Token(Keys::TOK_ACOSH, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_ACTION, gcnew Token(Keys::TOK_ACTION, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_ACTIVE, gcnew Token(Keys::TOK_ACTIVE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_ADD, gcnew Token(Keys::TOK_ADD, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_ADMIN, gcnew Token(Keys::TOK_ADMIN, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_AFTER, gcnew Token(Keys::TOK_AFTER, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_ALL, gcnew Token(Keys::TOK_ALL, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_ALTER, gcnew Token(Keys::TOK_ALTER, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_ALWAYS, gcnew Token(Keys::TOK_ALWAYS, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_AND, gcnew Token(Keys::TOK_AND, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_ANY, gcnew Token(Keys::TOK_ANY, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_AS, gcnew Token(Keys::TOK_AS, FlagsTokenCategory::RESERVED, AS_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_ASC, gcnew Token(Keys::TOK_ASC, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_ASCENDING, gcnew Token(Keys::TOK_ASCENDING, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_ASCII_CHAR, gcnew Token(Keys::TOK_ASCII_CHAR, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_ASCII_VAL, gcnew Token(Keys::TOK_ASCII_VAL, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_ASIN, gcnew Token(Keys::TOK_ASIN, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_ASINH, gcnew Token(Keys::TOK_ASINH, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_AT, gcnew Token(Keys::TOK_AT, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_ATAN, gcnew Token(Keys::TOK_ATAN, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_ATAN2, gcnew Token(Keys::TOK_ATAN2, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_ATANH, gcnew Token(Keys::TOK_ATANH, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_AUTO, gcnew Token(Keys::TOK_AUTO, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_AUTONOMOUS, gcnew Token(Keys::TOK_AUTONOMOUS, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_AVG, gcnew Token(Keys::TOK_AVG, FlagsTokenCategory::AGGR_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_BACKUP, gcnew Token(Keys::TOK_BACKUP, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_BASE64_DECODE, gcnew Token(Keys::TOK_BASE64_DECODE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_BASE64_ENCODE, gcnew Token(Keys::TOK_BASE64_ENCODE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_BEFORE, gcnew Token(Keys::TOK_BEFORE, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_BEGIN, gcnew Token(Keys::TOK_BEGIN, FlagsTokenCategory::BLOCK, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_BETWEEN, gcnew Token(Keys::TOK_BETWEEN, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_BIGINT, gcnew Token(Keys::TOK_BIGINT, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_BIN_AND, gcnew Token(Keys::TOK_BIN_AND, FlagsTokenCategory::AGGR_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_BIN_COMPLEMENT_GTR, gcnew Token(Keys::TOK_BIN_COMPLEMENT_GTR, FlagsTokenCategory::OPERATOR, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_BIN_COMPLIMENT_LSS, gcnew Token(Keys::TOK_BIN_COMPLIMENT_LSS, FlagsTokenCategory::OPERATOR, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_BIN_NOT, gcnew Token(Keys::TOK_BIN_NOT, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_BIN_OPR_COMPLEMENT, gcnew Token(Keys::TOK_BIN_OPR_COMPLEMENT, FlagsTokenCategory::OPERATOR, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_BIN_OPR_XOR, gcnew Token(Keys::TOK_BIN_OPR_XOR, FlagsTokenCategory::OPERATOR, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_BIN_OR, gcnew Token(Keys::TOK_BIN_OR, FlagsTokenCategory::AGGR_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_BIN_SHL, gcnew Token(Keys::TOK_BIN_SHL, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_BIN_SHR, gcnew Token(Keys::TOK_BIN_SHR, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_BIN_XOR, gcnew Token(Keys::TOK_BIN_XOR, FlagsTokenCategory::AGGR_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_BIN_XOR_GTR, gcnew Token(Keys::TOK_BIN_XOR_GTR, FlagsTokenCategory::OPERATOR, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_BIN_XOR_LSS, gcnew Token(Keys::TOK_BIN_XOR_LSS, FlagsTokenCategory::OPERATOR, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_BINARY, gcnew Token(Keys::TOK_BINARY, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_BIND, gcnew Token(Keys::TOK_BIND, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_BIND_PARAM, gcnew Token(Keys::TOK_BIND_PARAM, FlagsTokenCategory::OPERATOR, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_BIT_LENGTH, gcnew Token(Keys::TOK_BIT_LENGTH, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_BLOB, gcnew Token(Keys::TOK_BLOB, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_BLOB_APPEND, gcnew Token(Keys::TOK_BLOB_APPEND, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_BLOCK, gcnew Token(Keys::TOK_BLOCK, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_BODY, gcnew Token(Keys::TOK_BODY, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_BOOLEAN, gcnew Token(Keys::TOK_BOOLEAN, FlagsTokenCategory::COLNAME_KEYWORD, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_BOTH, gcnew Token(Keys::TOK_BOTH, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_BREAK, gcnew Token(Keys::TOK_BREAK, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_BY, gcnew Token(Keys::TOK_BY, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_CALLER, gcnew Token(Keys::TOK_CALLER, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_CASCADE, gcnew Token(Keys::TOK_CASCADE, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_CASE, gcnew Token(Keys::TOK_CASE, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_CAST, gcnew Token(Keys::TOK_CAST, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_CEIL, gcnew Token(Keys::TOK_CEIL, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_CEILING, gcnew Token(Keys::TOK_CEILING, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_CHAR, gcnew Token(Keys::TOK_CHAR, FlagsTokenCategory::PARAM_FUNCTION, AS_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_CHAR_LENGTH, gcnew Token(Keys::TOK_CHAR_LENGTH, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_CHAR_TO_UUID, gcnew Token(Keys::TOK_CHAR_TO_UUID, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_CHARACTER, gcnew Token(Keys::TOK_CHARACTER, FlagsTokenCategory::PARAM_FUNCTION, AS_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_CHARACTER_LENGTH, gcnew Token(Keys::TOK_CHARACTER_LENGTH, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_CHECK, gcnew Token(Keys::TOK_CHECK, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_CLEAR, gcnew Token(Keys::TOK_CLEAR, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_CLOSE, gcnew Token(Keys::TOK_CLOSE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_COALESCE, gcnew Token(Keys::TOK_COALESCE, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_COLLATE, gcnew Token(Keys::TOK_COLLATE, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_COLLATION, gcnew Token(Keys::TOK_COLLATION, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_COLUMN, gcnew Token(Keys::TOK_COLUMN, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_COMMA, gcnew Token(Keys::TOK_COMMA, FlagsTokenCategory::SEPARATOR, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_COMMENT, gcnew Token(Keys::TOK_COMMENT, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_COMMIT, gcnew Token(Keys::TOK_COMMIT, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_COMMITTED, gcnew Token(Keys::TOK_COMMITTED, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_COMMON, gcnew Token(Keys::TOK_COMMON, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_COMPARE_DECFLOAT, gcnew Token(Keys::TOK_COMPARE_DECFLOAT, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_COMPUTED, gcnew Token(Keys::TOK_COMPUTED, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_CONCATENATE, gcnew Token(Keys::TOK_CONCATENATE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_CONDITIONAL, gcnew Token(Keys::TOK_CONDITIONAL, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_CONNECT, gcnew Token(Keys::TOK_CONNECT, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_CONNECTIONS, gcnew Token(Keys::TOK_CONNECTIONS, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_CONSISTENCY, gcnew Token(Keys::TOK_CONSISTENCY, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_CONSTRAINT, gcnew Token(Keys::TOK_CONSTRAINT, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_CONTAINING, gcnew Token(Keys::TOK_CONTAINING, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_CONTINUE, gcnew Token(Keys::TOK_CONTINUE, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_CORR, gcnew Token(Keys::TOK_CORR, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_COS, gcnew Token(Keys::TOK_COS, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_COSH, gcnew Token(Keys::TOK_COSH, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_COT, gcnew Token(Keys::TOK_COT, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_COUNT, gcnew Token(Keys::TOK_COUNT, FlagsTokenCategory::AGGR_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_COUNTER, gcnew Token(Keys::TOK_COUNTER, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_COVAR_POP, gcnew Token(Keys::TOK_COVAR_POP, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_COVAR_SAMP, gcnew Token(Keys::TOK_COVAR_SAMP, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_CREATE, gcnew Token(Keys::TOK_CREATE, FlagsTokenCategory::RESERVED, AS_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_CROSS, gcnew Token(Keys::TOK_CROSS, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_CRYPT_HASH, gcnew Token(Keys::TOK_CRYPT_HASH, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_CSTRING, gcnew Token(Keys::TOK_CSTRING, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_CTR_BIG_ENDIAN, gcnew Token(Keys::TOK_CTR_BIG_ENDIAN, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_CTR_LENGTH, gcnew Token(Keys::TOK_CTR_LENGTH, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_CTR_LITTLE_ENDIAN, gcnew Token(Keys::TOK_CTR_LITTLE_ENDIAN, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_CUME_DIST, gcnew Token(Keys::TOK_CUME_DIST, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_CURRENT, gcnew Token(Keys::TOK_CURRENT, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_CURRENT_CONNECTION, gcnew Token(Keys::TOK_CURRENT_CONNECTION, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_CURRENT_DATE, gcnew Token(Keys::TOK_CURRENT_DATE, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_CURRENT_ROLE, gcnew Token(Keys::TOK_CURRENT_ROLE, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_CURRENT_TIME, gcnew Token(Keys::TOK_CURRENT_TIME, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_CURRENT_TIMESTAMP, gcnew Token(Keys::TOK_CURRENT_TIMESTAMP, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_CURRENT_TRANSACTION, gcnew Token(Keys::TOK_CURRENT_TRANSACTION, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_CURRENT_USER, gcnew Token(Keys::TOK_CURRENT_USER, FlagsTokenCategory::FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_CURSOR, gcnew Token(Keys::TOK_CURSOR, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_DATA, gcnew Token(Keys::TOK_DATA, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_DATABASE, gcnew Token(Keys::TOK_DATABASE, FlagsTokenCategory::FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_DATE, gcnew Token(Keys::TOK_DATE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_DATEADD, gcnew Token(Keys::TOK_DATEADD, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_DATEDIFF, gcnew Token(Keys::TOK_DATEDIFF, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_DAY, gcnew Token(Keys::TOK_DAY, FlagsTokenCategory::PARAM_FUNCTION, AS_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_DB_KEY, gcnew Token(Keys::TOK_DB_KEY, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_DDL, gcnew Token(Keys::TOK_DDL, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_DEBUG, gcnew Token(Keys::TOK_DEBUG, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_DEC, gcnew Token(Keys::TOK_DEC, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_DECFLOAT, gcnew Token(Keys::TOK_DECFLOAT, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_DECIMAL, gcnew Token(Keys::TOK_DECIMAL, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_DECIMAL_NUMBER, gcnew Token(Keys::TOK_DECIMAL_NUMBER, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_DECLARE, gcnew Token(Keys::TOK_DECLARE, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_DECODE, gcnew Token(Keys::TOK_DECODE, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_DECRYPT, gcnew Token(Keys::TOK_DECRYPT, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_DEFAULT, gcnew Token(Keys::TOK_DEFAULT, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_DEFINER, gcnew Token(Keys::TOK_DEFINER, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_DELETE, gcnew Token(Keys::TOK_DELETE, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_DELETING, gcnew Token(Keys::TOK_DELETING, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_DENSE_RANK, gcnew Token(Keys::TOK_DENSE_RANK, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_DESC, gcnew Token(Keys::TOK_DESC, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_DESCENDING, gcnew Token(Keys::TOK_DESCENDING, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_DESCRIPTOR, gcnew Token(Keys::TOK_DESCRIPTOR, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_DETERMINISTIC, gcnew Token(Keys::TOK_DETERMINISTIC, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_DIFFERENCE, gcnew Token(Keys::TOK_DIFFERENCE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_DISABLE, gcnew Token(Keys::TOK_DISABLE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_DISCONNECT, gcnew Token(Keys::TOK_DISCONNECT, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_DISTINCT, gcnew Token(Keys::TOK_DISTINCT, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_DO, gcnew Token(Keys::TOK_DO, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_DOMAIN, gcnew Token(Keys::TOK_DOMAIN, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_DOUBLE, gcnew Token(Keys::TOK_DOUBLE, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_DROP, gcnew Token(Keys::TOK_DROP, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_ELSE, gcnew Token(Keys::TOK_ELSE, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_ENABLE, gcnew Token(Keys::TOK_ENABLE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_ENCRYPT, gcnew Token(Keys::TOK_ENCRYPT, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_END, gcnew Token(Keys::TOK_END, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_ENGINE, gcnew Token(Keys::TOK_ENGINE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_ENTRY_POINT, gcnew Token(Keys::TOK_ENTRY_POINT, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_ESCAPE, gcnew Token(Keys::TOK_ESCAPE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_EXCEPTION, gcnew Token(Keys::TOK_EXCEPTION, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_EXCESS, gcnew Token(Keys::TOK_EXCESS, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_EXCLUDE, gcnew Token(Keys::TOK_EXCLUDE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_EXECUTE, gcnew Token(Keys::TOK_EXECUTE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_EXISTS, gcnew Token(Keys::TOK_EXISTS, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_EXIT, gcnew Token(Keys::TOK_EXIT, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_EXP, gcnew Token(Keys::TOK_EXP, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_EXTENDED, gcnew Token(Keys::TOK_EXTENDED, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_EXTERNAL, gcnew Token(Keys::TOK_EXTERNAL, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_EXTRACT, gcnew Token(Keys::TOK_EXTRACT, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_FALSE, gcnew Token(Keys::TOK_FALSE, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_FETCH, gcnew Token(Keys::TOK_FETCH, FlagsTokenCategory::RESERVED, AS_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_FILE, gcnew Token(Keys::TOK_FILE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_FILTER, gcnew Token(Keys::TOK_FILTER, FlagsTokenCategory::UNRESERVED,AS_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_FIRST, gcnew Token(Keys::TOK_FIRST, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_FIRST_DAY, gcnew Token(Keys::TOK_FIRST_DAY, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_FIRST_VALUE, gcnew Token(Keys::TOK_FIRST_VALUE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_FIRSTNAME, gcnew Token(Keys::TOK_FIRSTNAME, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_FLOAT, gcnew Token(Keys::TOK_FLOAT, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_FLOAT_NUMBER, gcnew Token(Keys::TOK_FLOAT_NUMBER, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_FLOOR, gcnew Token(Keys::TOK_FLOOR, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_FOLLOWING, gcnew Token(Keys::TOK_FOLLOWING, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_FOR, gcnew Token(Keys::TOK_FOR, FlagsTokenCategory::RESERVED, AS_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_FOREIGN, gcnew Token(Keys::TOK_FOREIGN, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_FREE_IT, gcnew Token(Keys::TOK_FREE_IT, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_FROM, gcnew Token(Keys::TOK_FROM, FlagsTokenCategory::RESERVED, AS_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_FULL, gcnew Token(Keys::TOK_FULL, FlagsTokenCategory::FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_FUNCTION, gcnew Token(Keys::TOK_FUNCTION, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_GDSCODE, gcnew Token(Keys::TOK_GDSCODE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_GEN_ID, gcnew Token(Keys::TOK_GEN_ID, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_GEN_UUID, gcnew Token(Keys::TOK_GEN_UUID, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_GENERATED, gcnew Token(Keys::TOK_GENERATED, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_GENERATOR, gcnew Token(Keys::TOK_GENERATOR, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_GEQ, gcnew Token(Keys::TOK_GEQ, FlagsTokenCategory::OPERATOR, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_GLOBAL, gcnew Token(Keys::TOK_GLOBAL, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_GRANT, gcnew Token(Keys::TOK_GRANT, FlagsTokenCategory::RESERVED, AS_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_GRANTED, gcnew Token(Keys::TOK_GRANTED, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_GROUP, gcnew Token(Keys::TOK_GROUP, FlagsTokenCategory::RESERVED, AS_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_HASH, gcnew Token(Keys::TOK_HASH, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_HAVING, gcnew Token(Keys::TOK_HAVING, FlagsTokenCategory::RESERVED, AS_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_HEX_DECODE, gcnew Token(Keys::TOK_HEX_DECODE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_HEX_ENCODE, gcnew Token(Keys::TOK_HEX_ENCODE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_HOUR, gcnew Token(Keys::TOK_HOUR, FlagsTokenCategory::PARAM_FUNCTION, AS_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_IDENTITY, gcnew Token(Keys::TOK_IDENTITY, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_IDLE, gcnew Token(Keys::TOK_IDLE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_IF, gcnew Token(Keys::TOK_IF, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_IGNORE, gcnew Token(Keys::TOK_IGNORE, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_IIF, gcnew Token(Keys::TOK_IIF, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_IN, gcnew Token(Keys::TOK_IN, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_INACTIVE, gcnew Token(Keys::TOK_INACTIVE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_INCLUDE, gcnew Token(Keys::TOK_INCLUDE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_INCREMENT, gcnew Token(Keys::TOK_INCREMENT, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_INDEX, gcnew Token(Keys::TOK_INDEX, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_INNER, gcnew Token(Keys::TOK_INNER, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_INPUT_TYPE, gcnew Token(Keys::TOK_INPUT_TYPE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_INSENSITIVE, gcnew Token(Keys::TOK_INSENSITIVE, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_INSERT, gcnew Token(Keys::TOK_INSERT, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_INSERTING, gcnew Token(Keys::TOK_INSERTING, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_INT, gcnew Token(Keys::TOK_INT, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_INT128, gcnew Token(Keys::TOK_INT128, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_INTEGER, gcnew Token(Keys::TOK_INTEGER, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_INTO, gcnew Token(Keys::TOK_INTO, FlagsTokenCategory::RESERVED, AS_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_INTRODUCER, gcnew Token(Keys::TOK_INTRODUCER, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_INVOKER, gcnew Token(Keys::TOK_INVOKER, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_IS, gcnew Token(Keys::TOK_IS, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_ISOLATION, gcnew Token(Keys::TOK_ISOLATION, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_IV, gcnew Token(Keys::TOK_IV, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_JOIN, gcnew Token(Keys::TOK_JOIN, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_KEY, gcnew Token(Keys::TOK_KEY, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_LAG, gcnew Token(Keys::TOK_LAG, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_LAST, gcnew Token(Keys::TOK_LAST, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_LAST_DAY, gcnew Token(Keys::TOK_LAST_DAY, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_LAST_VALUE, gcnew Token(Keys::TOK_LAST_VALUE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_LASTNAME, gcnew Token(Keys::TOK_LASTNAME, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_LATERAL, gcnew Token(Keys::TOK_LATERAL, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_LEAD, gcnew Token(Keys::TOK_LEAD, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_LEADING, gcnew Token(Keys::TOK_LEADING, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_LEAVE, gcnew Token(Keys::TOK_LEAVE, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_LEFT, gcnew Token(Keys::TOK_LEFT, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_LEGACY, gcnew Token(Keys::TOK_LEGACY, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_LENGTH, gcnew Token(Keys::TOK_LENGTH, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_LEQ, gcnew Token(Keys::TOK_LEQ, FlagsTokenCategory::OPERATOR, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_LEVEL, gcnew Token(Keys::TOK_LEVEL, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_LIFETIME, gcnew Token(Keys::TOK_LIFETIME, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_LIKE, gcnew Token(Keys::TOK_LIKE, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_LIMBO, gcnew Token(Keys::TOK_LIMBO, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_LIMIT64_INT, gcnew Token(Keys::TOK_LIMIT64_INT, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_LIMIT64_NUMBER, gcnew Token(Keys::TOK_LIMIT64_NUMBER, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_LINGER, gcnew Token(Keys::TOK_LINGER, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_LIST, gcnew Token(Keys::TOK_LIST, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_LN, gcnew Token(Keys::TOK_LN, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_LOCAL, gcnew Token(Keys::TOK_LOCAL, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_LOCALTIME, gcnew Token(Keys::TOK_LOCALTIME, FlagsTokenCategory::FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_LOCALTIMESTAMP, gcnew Token(Keys::TOK_LOCALTIMESTAMP, FlagsTokenCategory::FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_LOCK, gcnew Token(Keys::TOK_LOCK, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_LOCKED, gcnew Token(Keys::TOK_LOCKED, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_LOG, gcnew Token(Keys::TOK_LOG, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_LOG10, gcnew Token(Keys::TOK_LOG10, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_LONG, gcnew Token(Keys::TOK_LONG, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_LOWER, gcnew Token(Keys::TOK_LOWER, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_LPAD, gcnew Token(Keys::TOK_LPAD, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_LPARAM, gcnew Token(Keys::TOK_LPARAM, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_MAKE_DBKEY, gcnew Token(Keys::TOK_MAKE_DBKEY, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_MANUAL, gcnew Token(Keys::TOK_MANUAL, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_MAPPING, gcnew Token(Keys::TOK_MAPPING, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_MATCHED, gcnew Token(Keys::TOK_MATCHED, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_MATCHING, gcnew Token(Keys::TOK_MATCHING, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_MAXIMUM, gcnew Token(Keys::TOK_MAXIMUM, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_MAXVALUE, gcnew Token(Keys::TOK_MAXVALUE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_MERGE, gcnew Token(Keys::TOK_MERGE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_MESSAGE, gcnew Token(Keys::TOK_MESSAGE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_MIDDLENAME, gcnew Token(Keys::TOK_MIDDLENAME, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_MILLISECOND, gcnew Token(Keys::TOK_MILLISECOND, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_MINIMUM, gcnew Token(Keys::TOK_MINIMUM, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_MINUTE, gcnew Token(Keys::TOK_MINUTE, FlagsTokenCategory::PARAM_FUNCTION, AS_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_MINVALUE, gcnew Token(Keys::TOK_MINVALUE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_MOD, gcnew Token(Keys::TOK_MOD, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_MODE, gcnew Token(Keys::TOK_MODE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_MODULE_NAME, gcnew Token(Keys::TOK_MODULE_NAME, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_MONTH, gcnew Token(Keys::TOK_MONTH, FlagsTokenCategory::PARAM_FUNCTION, AS_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_NAME, gcnew Token(Keys::TOK_NAME, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_NAMES, gcnew Token(Keys::TOK_NAMES, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_NATIONAL, gcnew Token(Keys::TOK_NATIONAL, FlagsTokenCategory::COLNAME_KEYWORD, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_NATIVE, gcnew Token(Keys::TOK_NATIVE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_NATURAL, gcnew Token(Keys::TOK_NATURAL, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_NCHAR, gcnew Token(Keys::TOK_NCHAR, FlagsTokenCategory::COLNAME_KEYWORD, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_NEQ, gcnew Token(Keys::TOK_NEQ, FlagsTokenCategory::OPERATOR, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_NEXT, gcnew Token(Keys::TOK_NEXT, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_NO, gcnew Token(Keys::TOK_NO, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_NORMALIZE_DECFLOAT, gcnew Token(Keys::TOK_NORMALIZE_DECFLOAT, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_NOT, gcnew Token(Keys::TOK_NOT, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_NOT_GTR, gcnew Token(Keys::TOK_NOT_GTR, FlagsTokenCategory::OPERATOR, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_NOT_GTR_LSS, gcnew Token(Keys::TOK_NOT_GTR_LSS, FlagsTokenCategory::OPERATOR, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_NOT_LSS, gcnew Token(Keys::TOK_NOT_LSS, FlagsTokenCategory::OPERATOR, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_NTH_VALUE, gcnew Token(Keys::TOK_NTH_VALUE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_NTILE, gcnew Token(Keys::TOK_NTILE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_NULL, gcnew Token(Keys::TOK_NULL, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_NULLIF, gcnew Token(Keys::TOK_NULLIF, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_NULLS, gcnew Token(Keys::TOK_NULLS, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_NUM126, gcnew Token(Keys::TOK_NUM126, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_NUMBER, gcnew Token(Keys::TOK_NUMBER, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_NUMBER32BIT, gcnew Token(Keys::TOK_NUMBER32BIT, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_NUMBER64BIT, gcnew Token(Keys::TOK_NUMBER64BIT, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_NUMERIC, gcnew Token(Keys::TOK_NUMERIC, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_OCTET_LENGTH, gcnew Token(Keys::TOK_OCTET_LENGTH, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_OF, gcnew Token(Keys::TOK_OF, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_OFFSET, gcnew Token(Keys::TOK_OFFSET, FlagsTokenCategory::RESERVED, AS_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_OLDEST, gcnew Token(Keys::TOK_OLDEST, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_ON, gcnew Token(Keys::TOK_ON, FlagsTokenCategory::RESERVED, AS_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_ONLY, gcnew Token(Keys::TOK_ONLY, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_OPEN, gcnew Token(Keys::TOK_OPEN, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_OPR_EQ, gcnew Token(Keys::TOK_OPR_EQ, FlagsTokenCategory::OPERATOR, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_OPR_GTR, gcnew Token(Keys::TOK_OPR_GTR, FlagsTokenCategory::OPERATOR, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_OPR_LSS, gcnew Token(Keys::TOK_OPR_LSS, FlagsTokenCategory::OPERATOR, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_OPTION, gcnew Token(Keys::TOK_OPTION, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_OR, gcnew Token(Keys::TOK_OR, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_ORDER, gcnew Token(Keys::TOK_ORDER, FlagsTokenCategory::RESERVED, AS_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_OS_NAME, gcnew Token(Keys::TOK_OS_NAME, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_OTHERS, gcnew Token(Keys::TOK_OTHERS, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_OUTER, gcnew Token(Keys::TOK_OUTER, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_OUTPUT_TYPE, gcnew Token(Keys::TOK_OUTPUT_TYPE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_OVER, gcnew Token(Keys::TOK_OVER, FlagsTokenCategory::UNRESERVED,AS_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_OVERFLOW, gcnew Token(Keys::TOK_OVERFLOW, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_OVERLAY, gcnew Token(Keys::TOK_OVERLAY, FlagsTokenCategory::COLNAME_KEYWORD, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_OVERRIDING, gcnew Token(Keys::TOK_OVERRIDING, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_PACKAGE, gcnew Token(Keys::TOK_PACKAGE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_PAD, gcnew Token(Keys::TOK_PAD, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_PAGE, gcnew Token(Keys::TOK_PAGE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_PAGE_SIZE, gcnew Token(Keys::TOK_PAGE_SIZE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_PAGES, gcnew Token(Keys::TOK_PAGES, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_PARAMETER, gcnew Token(Keys::TOK_PARAMETER, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_PAREN_CLOSE, gcnew Token(Keys::TOK_PAREN_CLOSE, FlagsTokenCategory::BLOCK, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_PAREN_OPEN, gcnew Token(Keys::TOK_PAREN_OPEN, FlagsTokenCategory::BLOCK, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_PARTITION, gcnew Token(Keys::TOK_PARTITION, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_PASSWORD, gcnew Token(Keys::TOK_PASSWORD, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_PERCENT_RANK, gcnew Token(Keys::TOK_PERCENT_RANK, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_PI, gcnew Token(Keys::TOK_PI, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_PKCS_1_5, gcnew Token(Keys::TOK_PKCS_1_5, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_PLACING, gcnew Token(Keys::TOK_PLACING, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_PLAN, gcnew Token(Keys::TOK_PLAN, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_PLUGIN, gcnew Token(Keys::TOK_PLUGIN, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_POOL, gcnew Token(Keys::TOK_POOL, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_POSITION, gcnew Token(Keys::TOK_POSITION, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_POST_EVENT, gcnew Token(Keys::TOK_POST_EVENT, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_POWER, gcnew Token(Keys::TOK_POWER, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_PRECEDING, gcnew Token(Keys::TOK_PRECEDING, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_PRECISION, gcnew Token(Keys::TOK_PRECISION, FlagsTokenCategory::RESERVED, AS_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_PRESERVE, gcnew Token(Keys::TOK_PRESERVE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_PRIMARY, gcnew Token(Keys::TOK_PRIMARY, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_PRIOR, gcnew Token(Keys::TOK_PRIOR, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_PRIVILEGE, gcnew Token(Keys::TOK_PRIVILEGE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_PRIVILEGES, gcnew Token(Keys::TOK_PRIVILEGES, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_PROCEDURE, gcnew Token(Keys::TOK_PROCEDURE, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_PROTECTED, gcnew Token(Keys::TOK_PROTECTED, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_PUBLICATION, gcnew Token(Keys::TOK_PUBLICATION, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_QUANTIZE, gcnew Token(Keys::TOK_QUANTIZE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_RAND, gcnew Token(Keys::TOK_RAND, FlagsTokenCategory::FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RANGE, gcnew Token(Keys::TOK_RANGE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RANK, gcnew Token(Keys::TOK_RANK, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RDB_ERROR, gcnew Token(Keys::TOK_RDB_ERROR, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RDB_GET_CONTEXT, gcnew Token(Keys::TOK_RDB_GET_CONTEXT, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RDB_GET_TRANSACTION_CN, gcnew Token(Keys::TOK_RDB_GET_TRANSACTION_CN, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RDB_RECORD_VERSION, gcnew Token(Keys::TOK_RDB_RECORD_VERSION, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RDB_ROLE_IN_USE, gcnew Token(Keys::TOK_RDB_ROLE_IN_USE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RDB_SET_CONTEXT, gcnew Token(Keys::TOK_RDB_SET_CONTEXT, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RDB_SYSTEM_PRIVILEGE, gcnew Token(Keys::TOK_RDB_SYSTEM_PRIVILEGE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_READ, gcnew Token(Keys::TOK_READ, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_REAL, gcnew Token(Keys::TOK_REAL, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RECREATE, gcnew Token(Keys::TOK_RECREATE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RECURSIVE, gcnew Token(Keys::TOK_RECURSIVE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_REFERENCES, gcnew Token(Keys::TOK_REFERENCES, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_REGR_AVGX, gcnew Token(Keys::TOK_REGR_AVGX, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_REGR_AVGY, gcnew Token(Keys::TOK_REGR_AVGY, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_REGR_COUNT, gcnew Token(Keys::TOK_REGR_COUNT, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_REGR_INTERCEPT, gcnew Token(Keys::TOK_REGR_INTERCEPT, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_REGR_R2, gcnew Token(Keys::TOK_REGR_R2, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_REGR_SLOPE, gcnew Token(Keys::TOK_REGR_SLOPE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_REGR_SXX, gcnew Token(Keys::TOK_REGR_SXX, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_REGR_SXY, gcnew Token(Keys::TOK_REGR_SXY, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_REGR_SYY, gcnew Token(Keys::TOK_REGR_SYY, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RELATIVE, gcnew Token(Keys::TOK_RELATIVE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RELEASE, gcnew Token(Keys::TOK_RELEASE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_REPLACE, gcnew Token(Keys::TOK_REPLACE, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_REQUESTS, gcnew Token(Keys::TOK_REQUESTS, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RESERVING, gcnew Token(Keys::TOK_RESERVING, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RESERVING, gcnew Token(Keys::TOK_RESERVING, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RESET, gcnew Token(Keys::TOK_RESET, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RESETTING, gcnew Token(Keys::TOK_RESETTING, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RESTART, gcnew Token(Keys::TOK_RESTART, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_RESTRICT, gcnew Token(Keys::TOK_RESTRICT, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RETAIN, gcnew Token(Keys::TOK_RETAIN, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_RETURN, gcnew Token(Keys::TOK_RETURN, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_RETURNING, gcnew Token(Keys::TOK_RETURNING, FlagsTokenCategory::RESERVED, AS_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RETURNING_VALUES, gcnew Token(Keys::TOK_RETURNING_VALUES, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RETURNS, gcnew Token(Keys::TOK_RETURNS, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_REVERSE, gcnew Token(Keys::TOK_REVERSE, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_REVOKE, gcnew Token(Keys::TOK_REVOKE, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_RIGHT, gcnew Token(Keys::TOK_RIGHT, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_ROLE, gcnew Token(Keys::TOK_ROLE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_ROLLBACK, gcnew Token(Keys::TOK_ROLLBACK, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_ROUND, gcnew Token(Keys::TOK_ROUND, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_ROW, gcnew Token(Keys::TOK_ROW, FlagsTokenCategory::COLNAME_KEYWORD, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_ROW_COUNT, gcnew Token(Keys::TOK_ROW_COUNT, FlagsTokenCategory::FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_ROW_NUMBER, gcnew Token(Keys::TOK_ROW_NUMBER, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_ROWS, gcnew Token(Keys::TOK_ROWS, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_RPAD, gcnew Token(Keys::TOK_RPAD, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RSA_DECRYPT, gcnew Token(Keys::TOK_RSA_DECRYPT, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RSA_ENCRYPT, gcnew Token(Keys::TOK_RSA_ENCRYPT, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RSA_PRIVATE, gcnew Token(Keys::TOK_RSA_PRIVATE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RSA_PUBLIC, gcnew Token(Keys::TOK_RSA_PUBLIC, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RSA_SIGN_HASH, gcnew Token(Keys::TOK_RSA_SIGN_HASH, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RSA_VERIFY_HASH, gcnew Token(Keys::TOK_RSA_VERIFY_HASH, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SALT_LENGTH, gcnew Token(Keys::TOK_SALT_LENGTH, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SAVEPOINT, gcnew Token(Keys::TOK_SAVEPOINT, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SCALAR_ARRAY, gcnew Token(Keys::TOK_SCALAR_ARRAY, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SCALEDINT, gcnew Token(Keys::TOK_SCALEDINT, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_SCHEMA, gcnew Token(Keys::TOK_SCHEMA, FlagsTokenCategory::FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SCROLL, gcnew Token(Keys::TOK_SCROLL, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_SECOND, gcnew Token(Keys::TOK_SECOND, FlagsTokenCategory::PARAM_FUNCTION, AS_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SECURITY, gcnew Token(Keys::TOK_SECURITY, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SEGMENT, gcnew Token(Keys::TOK_SEGMENT, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_SELECT, gcnew Token(Keys::TOK_SELECT, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_SENSITIVE, gcnew Token(Keys::TOK_SENSITIVE, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SEQUENCE, gcnew Token(Keys::TOK_SEQUENCE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SERVERWIDE, gcnew Token(Keys::TOK_SERVERWIDE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SESSION, gcnew Token(Keys::TOK_SESSION, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_SET, gcnew Token(Keys::TOK_SET, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SHADOW, gcnew Token(Keys::TOK_SHADOW, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SHARED, gcnew Token(Keys::TOK_SHARED, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_SIGN, gcnew Token(Keys::TOK_SIGN, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SIGNATURE, gcnew Token(Keys::TOK_SIGNATURE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_SIMILAR, gcnew Token(Keys::TOK_SIMILAR, FlagsTokenCategory::FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SIN, gcnew Token(Keys::TOK_SIN, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SINGULAR, gcnew Token(Keys::TOK_SINGULAR, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SINH, gcnew Token(Keys::TOK_SINH, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SIZE, gcnew Token(Keys::TOK_SIZE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SKIP, gcnew Token(Keys::TOK_SKIP, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_SMALLINT, gcnew Token(Keys::TOK_SMALLINT, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SNAPSHOT, gcnew Token(Keys::TOK_SNAPSHOT, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_SOME, gcnew Token(Keys::TOK_SOME, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SORT, gcnew Token(Keys::TOK_SORT, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SOURCE, gcnew Token(Keys::TOK_SOURCE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_SPACE, gcnew Token(Keys::TOK_SPACE, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_SQL, gcnew Token(Keys::TOK_SQL, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SQLCODE, gcnew Token(Keys::TOK_SQLCODE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_SQLSTATE, gcnew Token(Keys::TOK_SQLSTATE, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_SQRT, gcnew Token(Keys::TOK_SQRT, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_STABILITY, gcnew Token(Keys::TOK_STABILITY, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_START, gcnew Token(Keys::TOK_START, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_STARTING, gcnew Token(Keys::TOK_STARTING, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_STARTS, gcnew Token(Keys::TOK_STARTS, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_STATEMENT, gcnew Token(Keys::TOK_STATEMENT, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_STATISTICS, gcnew Token(Keys::TOK_STATISTICS, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_STDDEV_POP, gcnew Token(Keys::TOK_STDDEV_POP, FlagsTokenCategory::AGGR_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_STDDEV_SAMP, gcnew Token(Keys::TOK_STDDEV_SAMP, FlagsTokenCategory::AGGR_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_STRING, gcnew Token(Keys::TOK_STRING, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SUB_TYPE, gcnew Token(Keys::TOK_SUB_TYPE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_SUBSTRING, gcnew Token(Keys::TOK_SUBSTRING, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_SUM, gcnew Token(Keys::TOK_SUM, FlagsTokenCategory::AGGR_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SUSPEND, gcnew Token(Keys::TOK_SUSPEND, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SYMBOL, gcnew Token(Keys::TOK_SYMBOL, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SYSTEM, gcnew Token(Keys::TOK_SYSTEM, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_TABLE, gcnew Token(Keys::TOK_TABLE, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_TAGS, gcnew Token(Keys::TOK_TAGS, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_TAN, gcnew Token(Keys::TOK_TAN, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_TANH, gcnew Token(Keys::TOK_TANH, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_TARGET, gcnew Token(Keys::TOK_TARGET, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_TEMPORARY, gcnew Token(Keys::TOK_TEMPORARY, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_THEN, gcnew Token(Keys::TOK_THEN, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_TIES, gcnew Token(Keys::TOK_TIES, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_TIME, gcnew Token(Keys::TOK_TIME, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_TIMEOUT, gcnew Token(Keys::TOK_TIMEOUT, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_TIMESTAMP, gcnew Token(Keys::TOK_TIMESTAMP, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_TIMEZONE_HOUR, gcnew Token(Keys::TOK_TIMEZONE_HOUR, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_TIMEZONE_MINUTE, gcnew Token(Keys::TOK_TIMEZONE_MINUTE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_TIMEZONE_NAME, gcnew Token(Keys::TOK_TIMEZONE_NAME, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_TO, gcnew Token(Keys::TOK_TO, FlagsTokenCategory::RESERVED, AS_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_TOTALORDER, gcnew Token(Keys::TOK_TOTALORDER, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_TRAILING, gcnew Token(Keys::TOK_TRAILING, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_TRANSACTION, gcnew Token(Keys::TOK_TRANSACTION, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_TRAPS, gcnew Token(Keys::TOK_TRAPS, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_TRIGGER, gcnew Token(Keys::TOK_TRIGGER, FlagsTokenCategory::RESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_TRIM, gcnew Token(Keys::TOK_TRIM, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_TRUE, gcnew Token(Keys::TOK_TRUE, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_TRUNC, gcnew Token(Keys::TOK_TRUNC, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_TRUSTED, gcnew Token(Keys::TOK_TRUSTED, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_TWO_PHASE, gcnew Token(Keys::TOK_TWO_PHASE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_TYPE, gcnew Token(Keys::TOK_TYPE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_UMINUS, gcnew Token(Keys::TOK_UMINUS, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_UNBOUNDED, gcnew Token(Keys::TOK_UNBOUNDED, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_UNCOMMITTED, gcnew Token(Keys::TOK_UNCOMMITTED, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_UNDO, gcnew Token(Keys::TOK_UNDO, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_UNICODE_CHAR, gcnew Token(Keys::TOK_UNICODE_CHAR, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_UNICODE_VAL, gcnew Token(Keys::TOK_UNICODE_VAL, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_UNION, gcnew Token(Keys::TOK_UNION, FlagsTokenCategory::RESERVED, AS_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_UNIQUE, gcnew Token(Keys::TOK_UNIQUE, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_UNKNOWN, gcnew Token(Keys::TOK_UNKNOWN, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_UPDATE, gcnew Token(Keys::TOK_UPDATE, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_UPDATING, gcnew Token(Keys::TOK_UPDATING, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_UPLUS, gcnew Token(Keys::TOK_UPLUS, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_UPPER, gcnew Token(Keys::TOK_UPPER, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_USAGE, gcnew Token(Keys::TOK_USAGE, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_USER, gcnew Token(Keys::TOK_USER, FlagsTokenCategory::FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_USING, gcnew Token(Keys::TOK_USING, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_UUID_TO_CHAR, gcnew Token(Keys::TOK_UUID_TO_CHAR, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_VALUE, gcnew Token(Keys::TOK_VALUE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_VALUES, gcnew Token(Keys::TOK_VALUES, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_VAR_POP, gcnew Token(Keys::TOK_VAR_POP, FlagsTokenCategory::AGGR_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_VAR_SAMP, gcnew Token(Keys::TOK_VAR_SAMP, FlagsTokenCategory::AGGR_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_VARBINARY, gcnew Token(Keys::TOK_VARBINARY, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_VARCHAR, gcnew Token(Keys::TOK_VARCHAR, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_VARIABLE, gcnew Token(Keys::TOK_VARIABLE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_VARYING, gcnew Token(Keys::TOK_VARYING, FlagsTokenCategory::RESERVED, AS_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_VERSION, gcnew Token(Keys::TOK_VERSION, FlagsTokenCategory::FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_VIEW, gcnew Token(Keys::TOK_VIEW, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_VOID, gcnew Token(Keys::TOK_VOID, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_WAIT, gcnew Token(Keys::TOK_WAIT, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_WEEK, gcnew Token(Keys::TOK_WEEK, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_WEEKDAY, gcnew Token(Keys::TOK_WEEKDAY, FlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_WHEN, gcnew Token(Keys::TOK_WHEN, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_WHERE, gcnew Token(Keys::TOK_WHERE, FlagsTokenCategory::RESERVED, AS_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_WHILE, gcnew Token(Keys::TOK_WHILE, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_WINDOW, gcnew Token(Keys::TOK_WINDOW, FlagsTokenCategory::RESERVED, AS_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_WITH, gcnew Token(Keys::TOK_WITH, FlagsTokenCategory::RESERVED, AS_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_WITHOUT, gcnew Token(Keys::TOK_WITHOUT, FlagsTokenCategory::UNRESERVED,AS_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_WORK, gcnew Token(Keys::TOK_WORK, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_WRITE, gcnew Token(Keys::TOK_WRITE, FlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_YEAR, gcnew Token(Keys::TOK_YEAR, FlagsTokenCategory::PARAM_FUNCTION, AS_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_YEARDAY, gcnew Token(Keys::TOK_YEARDAY, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_YYERRCODE, gcnew Token(Keys::TOK_YYERRCODE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_ZONE, gcnew Token(Keys::TOK_ZONE, FlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));


	return _Tokens;
}


}
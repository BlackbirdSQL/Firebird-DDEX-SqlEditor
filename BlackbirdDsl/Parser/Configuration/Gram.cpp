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

	_Tokens->Add((SysStr^)Values::TOK_ABS, gcnew Token(Keys::TOK_ABS, EnFlagsTokenCategory::FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_ABSOLUTE, gcnew Token(Keys::TOK_ABSOLUTE, EnFlagsTokenCategory::FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_ACCENT, gcnew Token(Keys::TOK_ACCENT, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_ACOS, gcnew Token(Keys::TOK_ACOS, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_ACOSH, gcnew Token(Keys::TOK_ACOSH, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_ACTION, gcnew Token(Keys::TOK_ACTION, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_ACTIVE, gcnew Token(Keys::TOK_ACTIVE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_ADD, gcnew Token(Keys::TOK_ADD, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_ADMIN, gcnew Token(Keys::TOK_ADMIN, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_AFTER, gcnew Token(Keys::TOK_AFTER, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_ALL, gcnew Token(Keys::TOK_ALL, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_ALTER, gcnew Token(Keys::TOK_ALTER, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_ALWAYS, gcnew Token(Keys::TOK_ALWAYS, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_AND, gcnew Token(Keys::TOK_AND, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_ANY, gcnew Token(Keys::TOK_ANY, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_AS, gcnew Token(Keys::TOK_AS, EnFlagsTokenCategory::RESERVED, AS_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_ASC, gcnew Token(Keys::TOK_ASC, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_ASCENDING, gcnew Token(Keys::TOK_ASCENDING, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_ASCII_CHAR, gcnew Token(Keys::TOK_ASCII_CHAR, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_ASCII_VAL, gcnew Token(Keys::TOK_ASCII_VAL, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_ASIN, gcnew Token(Keys::TOK_ASIN, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_ASINH, gcnew Token(Keys::TOK_ASINH, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_AT, gcnew Token(Keys::TOK_AT, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_ATAN, gcnew Token(Keys::TOK_ATAN, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_ATAN2, gcnew Token(Keys::TOK_ATAN2, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_ATANH, gcnew Token(Keys::TOK_ATANH, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_AUTO, gcnew Token(Keys::TOK_AUTO, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_AUTONOMOUS, gcnew Token(Keys::TOK_AUTONOMOUS, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_AVG, gcnew Token(Keys::TOK_AVG, EnFlagsTokenCategory::AGGR_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_BACKUP, gcnew Token(Keys::TOK_BACKUP, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_BASE64_DECODE, gcnew Token(Keys::TOK_BASE64_DECODE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_BASE64_ENCODE, gcnew Token(Keys::TOK_BASE64_ENCODE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_BEFORE, gcnew Token(Keys::TOK_BEFORE, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_BEGIN, gcnew Token(Keys::TOK_BEGIN, EnFlagsTokenCategory::BLOCK, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_BETWEEN, gcnew Token(Keys::TOK_BETWEEN, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_BIGINT, gcnew Token(Keys::TOK_BIGINT, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_BIN_AND, gcnew Token(Keys::TOK_BIN_AND, EnFlagsTokenCategory::AGGR_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_BIN_COMPLEMENT_GTR, gcnew Token(Keys::TOK_BIN_COMPLEMENT_GTR, EnFlagsTokenCategory::OPERATOR, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_BIN_COMPLIMENT_LSS, gcnew Token(Keys::TOK_BIN_COMPLIMENT_LSS, EnFlagsTokenCategory::OPERATOR, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_BIN_NOT, gcnew Token(Keys::TOK_BIN_NOT, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_BIN_OPR_COMPLEMENT, gcnew Token(Keys::TOK_BIN_OPR_COMPLEMENT, EnFlagsTokenCategory::OPERATOR, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_BIN_OPR_XOR, gcnew Token(Keys::TOK_BIN_OPR_XOR, EnFlagsTokenCategory::OPERATOR, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_BIN_OR, gcnew Token(Keys::TOK_BIN_OR, EnFlagsTokenCategory::AGGR_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_BIN_SHL, gcnew Token(Keys::TOK_BIN_SHL, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_BIN_SHR, gcnew Token(Keys::TOK_BIN_SHR, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_BIN_XOR, gcnew Token(Keys::TOK_BIN_XOR, EnFlagsTokenCategory::AGGR_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_BIN_XOR_GTR, gcnew Token(Keys::TOK_BIN_XOR_GTR, EnFlagsTokenCategory::OPERATOR, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_BIN_XOR_LSS, gcnew Token(Keys::TOK_BIN_XOR_LSS, EnFlagsTokenCategory::OPERATOR, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_BINARY, gcnew Token(Keys::TOK_BINARY, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_BIND, gcnew Token(Keys::TOK_BIND, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_BIND_PARAM, gcnew Token(Keys::TOK_BIND_PARAM, EnFlagsTokenCategory::OPERATOR, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_BIT_LENGTH, gcnew Token(Keys::TOK_BIT_LENGTH, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_BLOB, gcnew Token(Keys::TOK_BLOB, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_BLOB_APPEND, gcnew Token(Keys::TOK_BLOB_APPEND, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_BLOCK, gcnew Token(Keys::TOK_BLOCK, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_BODY, gcnew Token(Keys::TOK_BODY, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_BOOLEAN, gcnew Token(Keys::TOK_BOOLEAN, EnFlagsTokenCategory::COLNAME_KEYWORD, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_BOTH, gcnew Token(Keys::TOK_BOTH, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_BREAK, gcnew Token(Keys::TOK_BREAK, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_BY, gcnew Token(Keys::TOK_BY, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_CALLER, gcnew Token(Keys::TOK_CALLER, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_CASCADE, gcnew Token(Keys::TOK_CASCADE, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_CASE, gcnew Token(Keys::TOK_CASE, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_CAST, gcnew Token(Keys::TOK_CAST, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_CEIL, gcnew Token(Keys::TOK_CEIL, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_CEILING, gcnew Token(Keys::TOK_CEILING, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_CHAR, gcnew Token(Keys::TOK_CHAR, EnFlagsTokenCategory::PARAM_FUNCTION, AS_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_CHAR_LENGTH, gcnew Token(Keys::TOK_CHAR_LENGTH, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_CHAR_TO_UUID, gcnew Token(Keys::TOK_CHAR_TO_UUID, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_CHARACTER, gcnew Token(Keys::TOK_CHARACTER, EnFlagsTokenCategory::PARAM_FUNCTION, AS_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_CHARACTER_LENGTH, gcnew Token(Keys::TOK_CHARACTER_LENGTH, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_CHECK, gcnew Token(Keys::TOK_CHECK, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_CLEAR, gcnew Token(Keys::TOK_CLEAR, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_CLOSE, gcnew Token(Keys::TOK_CLOSE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_COALESCE, gcnew Token(Keys::TOK_COALESCE, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_COLLATE, gcnew Token(Keys::TOK_COLLATE, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_COLLATION, gcnew Token(Keys::TOK_COLLATION, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_COLUMN, gcnew Token(Keys::TOK_COLUMN, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_COMMA, gcnew Token(Keys::TOK_COMMA, EnFlagsTokenCategory::SEPARATOR, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_COMMENT, gcnew Token(Keys::TOK_COMMENT, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_COMMIT, gcnew Token(Keys::TOK_COMMIT, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_COMMITTED, gcnew Token(Keys::TOK_COMMITTED, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_COMMON, gcnew Token(Keys::TOK_COMMON, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_COMPARE_DECFLOAT, gcnew Token(Keys::TOK_COMPARE_DECFLOAT, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_COMPUTED, gcnew Token(Keys::TOK_COMPUTED, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_CONCATENATE, gcnew Token(Keys::TOK_CONCATENATE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_CONDITIONAL, gcnew Token(Keys::TOK_CONDITIONAL, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_CONNECT, gcnew Token(Keys::TOK_CONNECT, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_CONNECTIONS, gcnew Token(Keys::TOK_CONNECTIONS, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_CONSISTENCY, gcnew Token(Keys::TOK_CONSISTENCY, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_CONSTRAINT, gcnew Token(Keys::TOK_CONSTRAINT, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_CONTAINING, gcnew Token(Keys::TOK_CONTAINING, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_CONTINUE, gcnew Token(Keys::TOK_CONTINUE, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_CORR, gcnew Token(Keys::TOK_CORR, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_COS, gcnew Token(Keys::TOK_COS, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_COSH, gcnew Token(Keys::TOK_COSH, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_COT, gcnew Token(Keys::TOK_COT, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_COUNT, gcnew Token(Keys::TOK_COUNT, EnFlagsTokenCategory::AGGR_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_COUNTER, gcnew Token(Keys::TOK_COUNTER, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_COVAR_POP, gcnew Token(Keys::TOK_COVAR_POP, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_COVAR_SAMP, gcnew Token(Keys::TOK_COVAR_SAMP, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_CREATE, gcnew Token(Keys::TOK_CREATE, EnFlagsTokenCategory::RESERVED, AS_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_CROSS, gcnew Token(Keys::TOK_CROSS, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_CRYPT_HASH, gcnew Token(Keys::TOK_CRYPT_HASH, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_CSTRING, gcnew Token(Keys::TOK_CSTRING, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_CTR_BIG_ENDIAN, gcnew Token(Keys::TOK_CTR_BIG_ENDIAN, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_CTR_LENGTH, gcnew Token(Keys::TOK_CTR_LENGTH, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_CTR_LITTLE_ENDIAN, gcnew Token(Keys::TOK_CTR_LITTLE_ENDIAN, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_CUME_DIST, gcnew Token(Keys::TOK_CUME_DIST, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_CURRENT, gcnew Token(Keys::TOK_CURRENT, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_CURRENT_CONNECTION, gcnew Token(Keys::TOK_CURRENT_CONNECTION, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_CURRENT_DATE, gcnew Token(Keys::TOK_CURRENT_DATE, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_CURRENT_ROLE, gcnew Token(Keys::TOK_CURRENT_ROLE, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_CURRENT_TIME, gcnew Token(Keys::TOK_CURRENT_TIME, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_CURRENT_TIMESTAMP, gcnew Token(Keys::TOK_CURRENT_TIMESTAMP, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_CURRENT_TRANSACTION, gcnew Token(Keys::TOK_CURRENT_TRANSACTION, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_CURRENT_USER, gcnew Token(Keys::TOK_CURRENT_USER, EnFlagsTokenCategory::FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_CURSOR, gcnew Token(Keys::TOK_CURSOR, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_DATA, gcnew Token(Keys::TOK_DATA, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_DATABASE, gcnew Token(Keys::TOK_DATABASE, EnFlagsTokenCategory::FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_DATE, gcnew Token(Keys::TOK_DATE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_DATEADD, gcnew Token(Keys::TOK_DATEADD, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_DATEDIFF, gcnew Token(Keys::TOK_DATEDIFF, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_DAY, gcnew Token(Keys::TOK_DAY, EnFlagsTokenCategory::PARAM_FUNCTION, AS_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_DB_KEY, gcnew Token(Keys::TOK_DB_KEY, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_DDL, gcnew Token(Keys::TOK_DDL, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_DEBUG, gcnew Token(Keys::TOK_DEBUG, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_DEC, gcnew Token(Keys::TOK_DEC, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_DECFLOAT, gcnew Token(Keys::TOK_DECFLOAT, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_DECIMAL, gcnew Token(Keys::TOK_DECIMAL, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_DECIMAL_NUMBER, gcnew Token(Keys::TOK_DECIMAL_NUMBER, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_DECLARE, gcnew Token(Keys::TOK_DECLARE, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_DECODE, gcnew Token(Keys::TOK_DECODE, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_DECRYPT, gcnew Token(Keys::TOK_DECRYPT, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_DEFAULT, gcnew Token(Keys::TOK_DEFAULT, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_DEFINER, gcnew Token(Keys::TOK_DEFINER, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_DELETE, gcnew Token(Keys::TOK_DELETE, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_DELETING, gcnew Token(Keys::TOK_DELETING, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_DENSE_RANK, gcnew Token(Keys::TOK_DENSE_RANK, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_DESC, gcnew Token(Keys::TOK_DESC, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_DESCENDING, gcnew Token(Keys::TOK_DESCENDING, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_DESCRIPTOR, gcnew Token(Keys::TOK_DESCRIPTOR, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_DETERMINISTIC, gcnew Token(Keys::TOK_DETERMINISTIC, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_DIFFERENCE, gcnew Token(Keys::TOK_DIFFERENCE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_DISABLE, gcnew Token(Keys::TOK_DISABLE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_DISCONNECT, gcnew Token(Keys::TOK_DISCONNECT, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_DISTINCT, gcnew Token(Keys::TOK_DISTINCT, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_DO, gcnew Token(Keys::TOK_DO, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_DOMAIN, gcnew Token(Keys::TOK_DOMAIN, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_DOUBLE, gcnew Token(Keys::TOK_DOUBLE, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_DROP, gcnew Token(Keys::TOK_DROP, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_ELSE, gcnew Token(Keys::TOK_ELSE, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_ENABLE, gcnew Token(Keys::TOK_ENABLE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_ENCRYPT, gcnew Token(Keys::TOK_ENCRYPT, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_END, gcnew Token(Keys::TOK_END, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_ENGINE, gcnew Token(Keys::TOK_ENGINE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_ENTRY_POINT, gcnew Token(Keys::TOK_ENTRY_POINT, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_ESCAPE, gcnew Token(Keys::TOK_ESCAPE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_EXCEPTION, gcnew Token(Keys::TOK_EXCEPTION, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_EXCESS, gcnew Token(Keys::TOK_EXCESS, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_EXCLUDE, gcnew Token(Keys::TOK_EXCLUDE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_EXECUTE, gcnew Token(Keys::TOK_EXECUTE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_EXISTS, gcnew Token(Keys::TOK_EXISTS, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_EXIT, gcnew Token(Keys::TOK_EXIT, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_EXP, gcnew Token(Keys::TOK_EXP, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_EXTENDED, gcnew Token(Keys::TOK_EXTENDED, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_EXTERNAL, gcnew Token(Keys::TOK_EXTERNAL, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_EXTRACT, gcnew Token(Keys::TOK_EXTRACT, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_FALSE, gcnew Token(Keys::TOK_FALSE, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_FETCH, gcnew Token(Keys::TOK_FETCH, EnFlagsTokenCategory::RESERVED, AS_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_FILE, gcnew Token(Keys::TOK_FILE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_FILTER, gcnew Token(Keys::TOK_FILTER, EnFlagsTokenCategory::UNRESERVED,AS_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_FIRST, gcnew Token(Keys::TOK_FIRST, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_FIRST_DAY, gcnew Token(Keys::TOK_FIRST_DAY, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_FIRST_VALUE, gcnew Token(Keys::TOK_FIRST_VALUE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_FIRSTNAME, gcnew Token(Keys::TOK_FIRSTNAME, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_FLOAT, gcnew Token(Keys::TOK_FLOAT, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_FLOAT_NUMBER, gcnew Token(Keys::TOK_FLOAT_NUMBER, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_FLOOR, gcnew Token(Keys::TOK_FLOOR, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_FOLLOWING, gcnew Token(Keys::TOK_FOLLOWING, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_FOR, gcnew Token(Keys::TOK_FOR, EnFlagsTokenCategory::RESERVED, AS_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_FOREIGN, gcnew Token(Keys::TOK_FOREIGN, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_FREE_IT, gcnew Token(Keys::TOK_FREE_IT, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_FROM, gcnew Token(Keys::TOK_FROM, EnFlagsTokenCategory::RESERVED, AS_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_FULL, gcnew Token(Keys::TOK_FULL, EnFlagsTokenCategory::FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_FUNCTION, gcnew Token(Keys::TOK_FUNCTION, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_GDSCODE, gcnew Token(Keys::TOK_GDSCODE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_GEN_ID, gcnew Token(Keys::TOK_GEN_ID, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_GEN_UUID, gcnew Token(Keys::TOK_GEN_UUID, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_GENERATED, gcnew Token(Keys::TOK_GENERATED, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_GENERATOR, gcnew Token(Keys::TOK_GENERATOR, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_GEQ, gcnew Token(Keys::TOK_GEQ, EnFlagsTokenCategory::OPERATOR, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_GLOBAL, gcnew Token(Keys::TOK_GLOBAL, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_GRANT, gcnew Token(Keys::TOK_GRANT, EnFlagsTokenCategory::RESERVED, AS_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_GRANTED, gcnew Token(Keys::TOK_GRANTED, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_GROUP, gcnew Token(Keys::TOK_GROUP, EnFlagsTokenCategory::RESERVED, AS_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_HASH, gcnew Token(Keys::TOK_HASH, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_HAVING, gcnew Token(Keys::TOK_HAVING, EnFlagsTokenCategory::RESERVED, AS_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_HEX_DECODE, gcnew Token(Keys::TOK_HEX_DECODE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_HEX_ENCODE, gcnew Token(Keys::TOK_HEX_ENCODE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_HOUR, gcnew Token(Keys::TOK_HOUR, EnFlagsTokenCategory::PARAM_FUNCTION, AS_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_IDENTITY, gcnew Token(Keys::TOK_IDENTITY, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_IDLE, gcnew Token(Keys::TOK_IDLE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_IF, gcnew Token(Keys::TOK_IF, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_IGNORE, gcnew Token(Keys::TOK_IGNORE, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_IIF, gcnew Token(Keys::TOK_IIF, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_IN, gcnew Token(Keys::TOK_IN, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_INACTIVE, gcnew Token(Keys::TOK_INACTIVE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_INCLUDE, gcnew Token(Keys::TOK_INCLUDE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_INCREMENT, gcnew Token(Keys::TOK_INCREMENT, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_INDEX, gcnew Token(Keys::TOK_INDEX, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_INNER, gcnew Token(Keys::TOK_INNER, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_INPUT_TYPE, gcnew Token(Keys::TOK_INPUT_TYPE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_INSENSITIVE, gcnew Token(Keys::TOK_INSENSITIVE, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_INSERT, gcnew Token(Keys::TOK_INSERT, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_INSERTING, gcnew Token(Keys::TOK_INSERTING, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_INT, gcnew Token(Keys::TOK_INT, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_INT128, gcnew Token(Keys::TOK_INT128, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_INTEGER, gcnew Token(Keys::TOK_INTEGER, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_INTO, gcnew Token(Keys::TOK_INTO, EnFlagsTokenCategory::RESERVED, AS_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_INTRODUCER, gcnew Token(Keys::TOK_INTRODUCER, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_INVOKER, gcnew Token(Keys::TOK_INVOKER, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_IS, gcnew Token(Keys::TOK_IS, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_ISOLATION, gcnew Token(Keys::TOK_ISOLATION, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_IV, gcnew Token(Keys::TOK_IV, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_JOIN, gcnew Token(Keys::TOK_JOIN, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_KEY, gcnew Token(Keys::TOK_KEY, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_LAG, gcnew Token(Keys::TOK_LAG, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_LAST, gcnew Token(Keys::TOK_LAST, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_LAST_DAY, gcnew Token(Keys::TOK_LAST_DAY, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_LAST_VALUE, gcnew Token(Keys::TOK_LAST_VALUE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_LASTNAME, gcnew Token(Keys::TOK_LASTNAME, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_LATERAL, gcnew Token(Keys::TOK_LATERAL, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_LEAD, gcnew Token(Keys::TOK_LEAD, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_LEADING, gcnew Token(Keys::TOK_LEADING, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_LEAVE, gcnew Token(Keys::TOK_LEAVE, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_LEFT, gcnew Token(Keys::TOK_LEFT, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_LEGACY, gcnew Token(Keys::TOK_LEGACY, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_LENGTH, gcnew Token(Keys::TOK_LENGTH, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_LEQ, gcnew Token(Keys::TOK_LEQ, EnFlagsTokenCategory::OPERATOR, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_LEVEL, gcnew Token(Keys::TOK_LEVEL, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_LIFETIME, gcnew Token(Keys::TOK_LIFETIME, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_LIKE, gcnew Token(Keys::TOK_LIKE, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_LIMBO, gcnew Token(Keys::TOK_LIMBO, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_LIMIT64_INT, gcnew Token(Keys::TOK_LIMIT64_INT, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_LIMIT64_NUMBER, gcnew Token(Keys::TOK_LIMIT64_NUMBER, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_LINGER, gcnew Token(Keys::TOK_LINGER, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_LIST, gcnew Token(Keys::TOK_LIST, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_LN, gcnew Token(Keys::TOK_LN, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_LOCAL, gcnew Token(Keys::TOK_LOCAL, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_LOCALTIME, gcnew Token(Keys::TOK_LOCALTIME, EnFlagsTokenCategory::FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_LOCALTIMESTAMP, gcnew Token(Keys::TOK_LOCALTIMESTAMP, EnFlagsTokenCategory::FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_LOCK, gcnew Token(Keys::TOK_LOCK, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_LOCKED, gcnew Token(Keys::TOK_LOCKED, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_LOG, gcnew Token(Keys::TOK_LOG, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_LOG10, gcnew Token(Keys::TOK_LOG10, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_LONG, gcnew Token(Keys::TOK_LONG, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_LOWER, gcnew Token(Keys::TOK_LOWER, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_LPAD, gcnew Token(Keys::TOK_LPAD, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_LPARAM, gcnew Token(Keys::TOK_LPARAM, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_MAKE_DBKEY, gcnew Token(Keys::TOK_MAKE_DBKEY, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_MANUAL, gcnew Token(Keys::TOK_MANUAL, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_MAPPING, gcnew Token(Keys::TOK_MAPPING, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_MATCHED, gcnew Token(Keys::TOK_MATCHED, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_MATCHING, gcnew Token(Keys::TOK_MATCHING, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_MAXIMUM, gcnew Token(Keys::TOK_MAXIMUM, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_MAXVALUE, gcnew Token(Keys::TOK_MAXVALUE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_MERGE, gcnew Token(Keys::TOK_MERGE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_MESSAGE, gcnew Token(Keys::TOK_MESSAGE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_MIDDLENAME, gcnew Token(Keys::TOK_MIDDLENAME, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_MILLISECOND, gcnew Token(Keys::TOK_MILLISECOND, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_MINIMUM, gcnew Token(Keys::TOK_MINIMUM, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_MINUTE, gcnew Token(Keys::TOK_MINUTE, EnFlagsTokenCategory::PARAM_FUNCTION, AS_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_MINVALUE, gcnew Token(Keys::TOK_MINVALUE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_MOD, gcnew Token(Keys::TOK_MOD, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_MODE, gcnew Token(Keys::TOK_MODE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_MODULE_NAME, gcnew Token(Keys::TOK_MODULE_NAME, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_MONTH, gcnew Token(Keys::TOK_MONTH, EnFlagsTokenCategory::PARAM_FUNCTION, AS_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_NAME, gcnew Token(Keys::TOK_NAME, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_NAMES, gcnew Token(Keys::TOK_NAMES, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_NATIONAL, gcnew Token(Keys::TOK_NATIONAL, EnFlagsTokenCategory::COLNAME_KEYWORD, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_NATIVE, gcnew Token(Keys::TOK_NATIVE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_NATURAL, gcnew Token(Keys::TOK_NATURAL, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_NCHAR, gcnew Token(Keys::TOK_NCHAR, EnFlagsTokenCategory::COLNAME_KEYWORD, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_NEQ, gcnew Token(Keys::TOK_NEQ, EnFlagsTokenCategory::OPERATOR, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_NEXT, gcnew Token(Keys::TOK_NEXT, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_NO, gcnew Token(Keys::TOK_NO, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_NORMALIZE_DECFLOAT, gcnew Token(Keys::TOK_NORMALIZE_DECFLOAT, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_NOT, gcnew Token(Keys::TOK_NOT, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_NOT_GTR, gcnew Token(Keys::TOK_NOT_GTR, EnFlagsTokenCategory::OPERATOR, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_NOT_GTR_LSS, gcnew Token(Keys::TOK_NOT_GTR_LSS, EnFlagsTokenCategory::OPERATOR, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_NOT_LSS, gcnew Token(Keys::TOK_NOT_LSS, EnFlagsTokenCategory::OPERATOR, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_NTH_VALUE, gcnew Token(Keys::TOK_NTH_VALUE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_NTILE, gcnew Token(Keys::TOK_NTILE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_NULL, gcnew Token(Keys::TOK_NULL, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_NULLIF, gcnew Token(Keys::TOK_NULLIF, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_NULLS, gcnew Token(Keys::TOK_NULLS, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_NUM126, gcnew Token(Keys::TOK_NUM126, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_NUMBER, gcnew Token(Keys::TOK_NUMBER, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_NUMBER32BIT, gcnew Token(Keys::TOK_NUMBER32BIT, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_NUMBER64BIT, gcnew Token(Keys::TOK_NUMBER64BIT, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_NUMERIC, gcnew Token(Keys::TOK_NUMERIC, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_OCTET_LENGTH, gcnew Token(Keys::TOK_OCTET_LENGTH, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_OF, gcnew Token(Keys::TOK_OF, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_OFFSET, gcnew Token(Keys::TOK_OFFSET, EnFlagsTokenCategory::RESERVED, AS_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_OLDEST, gcnew Token(Keys::TOK_OLDEST, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_ON, gcnew Token(Keys::TOK_ON, EnFlagsTokenCategory::RESERVED, AS_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_ONLY, gcnew Token(Keys::TOK_ONLY, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_OPEN, gcnew Token(Keys::TOK_OPEN, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_OPR_EQ, gcnew Token(Keys::TOK_OPR_EQ, EnFlagsTokenCategory::OPERATOR, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_OPR_GTR, gcnew Token(Keys::TOK_OPR_GTR, EnFlagsTokenCategory::OPERATOR, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_OPR_LSS, gcnew Token(Keys::TOK_OPR_LSS, EnFlagsTokenCategory::OPERATOR, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_OPTION, gcnew Token(Keys::TOK_OPTION, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_OR, gcnew Token(Keys::TOK_OR, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_ORDER, gcnew Token(Keys::TOK_ORDER, EnFlagsTokenCategory::RESERVED, AS_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_OS_NAME, gcnew Token(Keys::TOK_OS_NAME, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_OTHERS, gcnew Token(Keys::TOK_OTHERS, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_OUTER, gcnew Token(Keys::TOK_OUTER, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_OUTPUT_TYPE, gcnew Token(Keys::TOK_OUTPUT_TYPE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_OVER, gcnew Token(Keys::TOK_OVER, EnFlagsTokenCategory::UNRESERVED,AS_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_OVERFLOW, gcnew Token(Keys::TOK_OVERFLOW, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_OVERLAY, gcnew Token(Keys::TOK_OVERLAY, EnFlagsTokenCategory::COLNAME_KEYWORD, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_OVERRIDING, gcnew Token(Keys::TOK_OVERRIDING, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_PACKAGE, gcnew Token(Keys::TOK_PACKAGE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_PAD, gcnew Token(Keys::TOK_PAD, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_PAGE, gcnew Token(Keys::TOK_PAGE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_PAGE_SIZE, gcnew Token(Keys::TOK_PAGE_SIZE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_PAGES, gcnew Token(Keys::TOK_PAGES, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_PARAMETER, gcnew Token(Keys::TOK_PARAMETER, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_PAREN_CLOSE, gcnew Token(Keys::TOK_PAREN_CLOSE, EnFlagsTokenCategory::BLOCK, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_PAREN_OPEN, gcnew Token(Keys::TOK_PAREN_OPEN, EnFlagsTokenCategory::BLOCK, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_PARTITION, gcnew Token(Keys::TOK_PARTITION, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_PASSWORD, gcnew Token(Keys::TOK_PASSWORD, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_PERCENT_RANK, gcnew Token(Keys::TOK_PERCENT_RANK, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_PI, gcnew Token(Keys::TOK_PI, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_PKCS_1_5, gcnew Token(Keys::TOK_PKCS_1_5, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_PLACING, gcnew Token(Keys::TOK_PLACING, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_PLAN, gcnew Token(Keys::TOK_PLAN, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_PLUGIN, gcnew Token(Keys::TOK_PLUGIN, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_POOL, gcnew Token(Keys::TOK_POOL, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_POSITION, gcnew Token(Keys::TOK_POSITION, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_POST_EVENT, gcnew Token(Keys::TOK_POST_EVENT, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_POWER, gcnew Token(Keys::TOK_POWER, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_PRECEDING, gcnew Token(Keys::TOK_PRECEDING, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_PRECISION, gcnew Token(Keys::TOK_PRECISION, EnFlagsTokenCategory::RESERVED, AS_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_PRESERVE, gcnew Token(Keys::TOK_PRESERVE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_PRIMARY, gcnew Token(Keys::TOK_PRIMARY, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_PRIOR, gcnew Token(Keys::TOK_PRIOR, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_PRIVILEGE, gcnew Token(Keys::TOK_PRIVILEGE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_PRIVILEGES, gcnew Token(Keys::TOK_PRIVILEGES, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_PROCEDURE, gcnew Token(Keys::TOK_PROCEDURE, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_PROTECTED, gcnew Token(Keys::TOK_PROTECTED, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_PUBLICATION, gcnew Token(Keys::TOK_PUBLICATION, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_QUANTIZE, gcnew Token(Keys::TOK_QUANTIZE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_RAND, gcnew Token(Keys::TOK_RAND, EnFlagsTokenCategory::FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RANGE, gcnew Token(Keys::TOK_RANGE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RANK, gcnew Token(Keys::TOK_RANK, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RDB_ERROR, gcnew Token(Keys::TOK_RDB_ERROR, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RDB_GET_CONTEXT, gcnew Token(Keys::TOK_RDB_GET_CONTEXT, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RDB_GET_TRANSACTION_CN, gcnew Token(Keys::TOK_RDB_GET_TRANSACTION_CN, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RDB_RECORD_VERSION, gcnew Token(Keys::TOK_RDB_RECORD_VERSION, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RDB_ROLE_IN_USE, gcnew Token(Keys::TOK_RDB_ROLE_IN_USE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RDB_SET_CONTEXT, gcnew Token(Keys::TOK_RDB_SET_CONTEXT, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RDB_SYSTEM_PRIVILEGE, gcnew Token(Keys::TOK_RDB_SYSTEM_PRIVILEGE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_READ, gcnew Token(Keys::TOK_READ, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_REAL, gcnew Token(Keys::TOK_REAL, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RECREATE, gcnew Token(Keys::TOK_RECREATE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RECURSIVE, gcnew Token(Keys::TOK_RECURSIVE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_REFERENCES, gcnew Token(Keys::TOK_REFERENCES, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_REGR_AVGX, gcnew Token(Keys::TOK_REGR_AVGX, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_REGR_AVGY, gcnew Token(Keys::TOK_REGR_AVGY, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_REGR_COUNT, gcnew Token(Keys::TOK_REGR_COUNT, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_REGR_INTERCEPT, gcnew Token(Keys::TOK_REGR_INTERCEPT, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_REGR_R2, gcnew Token(Keys::TOK_REGR_R2, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_REGR_SLOPE, gcnew Token(Keys::TOK_REGR_SLOPE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_REGR_SXX, gcnew Token(Keys::TOK_REGR_SXX, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_REGR_SXY, gcnew Token(Keys::TOK_REGR_SXY, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_REGR_SYY, gcnew Token(Keys::TOK_REGR_SYY, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RELATIVE, gcnew Token(Keys::TOK_RELATIVE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RELEASE, gcnew Token(Keys::TOK_RELEASE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_REPLACE, gcnew Token(Keys::TOK_REPLACE, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_REQUESTS, gcnew Token(Keys::TOK_REQUESTS, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RESERVING, gcnew Token(Keys::TOK_RESERVING, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RESERVING, gcnew Token(Keys::TOK_RESERVING, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RESET, gcnew Token(Keys::TOK_RESET, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RESETTING, gcnew Token(Keys::TOK_RESETTING, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RESTART, gcnew Token(Keys::TOK_RESTART, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_RESTRICT, gcnew Token(Keys::TOK_RESTRICT, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RETAIN, gcnew Token(Keys::TOK_RETAIN, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_RETURN, gcnew Token(Keys::TOK_RETURN, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_RETURNING, gcnew Token(Keys::TOK_RETURNING, EnFlagsTokenCategory::RESERVED, AS_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RETURNING_VALUES, gcnew Token(Keys::TOK_RETURNING_VALUES, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RETURNS, gcnew Token(Keys::TOK_RETURNS, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_REVERSE, gcnew Token(Keys::TOK_REVERSE, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_REVOKE, gcnew Token(Keys::TOK_REVOKE, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_RIGHT, gcnew Token(Keys::TOK_RIGHT, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_ROLE, gcnew Token(Keys::TOK_ROLE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_ROLLBACK, gcnew Token(Keys::TOK_ROLLBACK, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_ROUND, gcnew Token(Keys::TOK_ROUND, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_ROW, gcnew Token(Keys::TOK_ROW, EnFlagsTokenCategory::COLNAME_KEYWORD, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_ROW_COUNT, gcnew Token(Keys::TOK_ROW_COUNT, EnFlagsTokenCategory::FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_ROW_NUMBER, gcnew Token(Keys::TOK_ROW_NUMBER, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_ROWS, gcnew Token(Keys::TOK_ROWS, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_RPAD, gcnew Token(Keys::TOK_RPAD, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RSA_DECRYPT, gcnew Token(Keys::TOK_RSA_DECRYPT, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RSA_ENCRYPT, gcnew Token(Keys::TOK_RSA_ENCRYPT, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RSA_PRIVATE, gcnew Token(Keys::TOK_RSA_PRIVATE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RSA_PUBLIC, gcnew Token(Keys::TOK_RSA_PUBLIC, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RSA_SIGN_HASH, gcnew Token(Keys::TOK_RSA_SIGN_HASH, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_RSA_VERIFY_HASH, gcnew Token(Keys::TOK_RSA_VERIFY_HASH, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SALT_LENGTH, gcnew Token(Keys::TOK_SALT_LENGTH, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SAVEPOINT, gcnew Token(Keys::TOK_SAVEPOINT, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SCALAR_ARRAY, gcnew Token(Keys::TOK_SCALAR_ARRAY, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SCALEDINT, gcnew Token(Keys::TOK_SCALEDINT, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_SCHEMA, gcnew Token(Keys::TOK_SCHEMA, EnFlagsTokenCategory::FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SCROLL, gcnew Token(Keys::TOK_SCROLL, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_SECOND, gcnew Token(Keys::TOK_SECOND, EnFlagsTokenCategory::PARAM_FUNCTION, AS_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SECURITY, gcnew Token(Keys::TOK_SECURITY, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SEGMENT, gcnew Token(Keys::TOK_SEGMENT, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_SELECT, gcnew Token(Keys::TOK_SELECT, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_SENSITIVE, gcnew Token(Keys::TOK_SENSITIVE, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SEQUENCE, gcnew Token(Keys::TOK_SEQUENCE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SERVERWIDE, gcnew Token(Keys::TOK_SERVERWIDE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SESSION, gcnew Token(Keys::TOK_SESSION, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_SET, gcnew Token(Keys::TOK_SET, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SHADOW, gcnew Token(Keys::TOK_SHADOW, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SHARED, gcnew Token(Keys::TOK_SHARED, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_SIGN, gcnew Token(Keys::TOK_SIGN, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SIGNATURE, gcnew Token(Keys::TOK_SIGNATURE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_SIMILAR, gcnew Token(Keys::TOK_SIMILAR, EnFlagsTokenCategory::FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SIN, gcnew Token(Keys::TOK_SIN, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SINGULAR, gcnew Token(Keys::TOK_SINGULAR, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SINH, gcnew Token(Keys::TOK_SINH, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SIZE, gcnew Token(Keys::TOK_SIZE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SKIP, gcnew Token(Keys::TOK_SKIP, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_SMALLINT, gcnew Token(Keys::TOK_SMALLINT, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SNAPSHOT, gcnew Token(Keys::TOK_SNAPSHOT, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_SOME, gcnew Token(Keys::TOK_SOME, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SORT, gcnew Token(Keys::TOK_SORT, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SOURCE, gcnew Token(Keys::TOK_SOURCE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_SPACE, gcnew Token(Keys::TOK_SPACE, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_SQL, gcnew Token(Keys::TOK_SQL, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SQLCODE, gcnew Token(Keys::TOK_SQLCODE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_SQLSTATE, gcnew Token(Keys::TOK_SQLSTATE, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_SQRT, gcnew Token(Keys::TOK_SQRT, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_STABILITY, gcnew Token(Keys::TOK_STABILITY, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_START, gcnew Token(Keys::TOK_START, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_STARTING, gcnew Token(Keys::TOK_STARTING, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_STARTS, gcnew Token(Keys::TOK_STARTS, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_STATEMENT, gcnew Token(Keys::TOK_STATEMENT, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_STATISTICS, gcnew Token(Keys::TOK_STATISTICS, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_STDDEV_POP, gcnew Token(Keys::TOK_STDDEV_POP, EnFlagsTokenCategory::AGGR_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_STDDEV_SAMP, gcnew Token(Keys::TOK_STDDEV_SAMP, EnFlagsTokenCategory::AGGR_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_STRING, gcnew Token(Keys::TOK_STRING, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SUB_TYPE, gcnew Token(Keys::TOK_SUB_TYPE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_SUBSTRING, gcnew Token(Keys::TOK_SUBSTRING, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_SUM, gcnew Token(Keys::TOK_SUM, EnFlagsTokenCategory::AGGR_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SUSPEND, gcnew Token(Keys::TOK_SUSPEND, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SYMBOL, gcnew Token(Keys::TOK_SYMBOL, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_SYSTEM, gcnew Token(Keys::TOK_SYSTEM, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_TABLE, gcnew Token(Keys::TOK_TABLE, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_TAGS, gcnew Token(Keys::TOK_TAGS, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_TAN, gcnew Token(Keys::TOK_TAN, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_TANH, gcnew Token(Keys::TOK_TANH, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_TARGET, gcnew Token(Keys::TOK_TARGET, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_TEMPORARY, gcnew Token(Keys::TOK_TEMPORARY, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_THEN, gcnew Token(Keys::TOK_THEN, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_TIES, gcnew Token(Keys::TOK_TIES, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_TIME, gcnew Token(Keys::TOK_TIME, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_TIMEOUT, gcnew Token(Keys::TOK_TIMEOUT, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_TIMESTAMP, gcnew Token(Keys::TOK_TIMESTAMP, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_TIMEZONE_HOUR, gcnew Token(Keys::TOK_TIMEZONE_HOUR, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_TIMEZONE_MINUTE, gcnew Token(Keys::TOK_TIMEZONE_MINUTE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_TIMEZONE_NAME, gcnew Token(Keys::TOK_TIMEZONE_NAME, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_TO, gcnew Token(Keys::TOK_TO, EnFlagsTokenCategory::RESERVED, AS_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_TOTALORDER, gcnew Token(Keys::TOK_TOTALORDER, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_TRAILING, gcnew Token(Keys::TOK_TRAILING, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_TRANSACTION, gcnew Token(Keys::TOK_TRANSACTION, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_TRAPS, gcnew Token(Keys::TOK_TRAPS, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_TRIGGER, gcnew Token(Keys::TOK_TRIGGER, EnFlagsTokenCategory::RESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_TRIM, gcnew Token(Keys::TOK_TRIM, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_TRUE, gcnew Token(Keys::TOK_TRUE, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_TRUNC, gcnew Token(Keys::TOK_TRUNC, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_TRUSTED, gcnew Token(Keys::TOK_TRUSTED, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_TWO_PHASE, gcnew Token(Keys::TOK_TWO_PHASE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_TYPE, gcnew Token(Keys::TOK_TYPE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_UMINUS, gcnew Token(Keys::TOK_UMINUS, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_UNBOUNDED, gcnew Token(Keys::TOK_UNBOUNDED, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_UNCOMMITTED, gcnew Token(Keys::TOK_UNCOMMITTED, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_UNDO, gcnew Token(Keys::TOK_UNDO, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_UNICODE_CHAR, gcnew Token(Keys::TOK_UNICODE_CHAR, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_UNICODE_VAL, gcnew Token(Keys::TOK_UNICODE_VAL, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_UNION, gcnew Token(Keys::TOK_UNION, EnFlagsTokenCategory::RESERVED, AS_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_UNIQUE, gcnew Token(Keys::TOK_UNIQUE, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_UNKNOWN, gcnew Token(Keys::TOK_UNKNOWN, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_UPDATE, gcnew Token(Keys::TOK_UPDATE, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_UPDATING, gcnew Token(Keys::TOK_UPDATING, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_UPLUS, gcnew Token(Keys::TOK_UPLUS, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_UPPER, gcnew Token(Keys::TOK_UPPER, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_USAGE, gcnew Token(Keys::TOK_USAGE, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_USER, gcnew Token(Keys::TOK_USER, EnFlagsTokenCategory::FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_USING, gcnew Token(Keys::TOK_USING, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_UUID_TO_CHAR, gcnew Token(Keys::TOK_UUID_TO_CHAR, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_VALUE, gcnew Token(Keys::TOK_VALUE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_VALUES, gcnew Token(Keys::TOK_VALUES, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_VAR_POP, gcnew Token(Keys::TOK_VAR_POP, EnFlagsTokenCategory::AGGR_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_VAR_SAMP, gcnew Token(Keys::TOK_VAR_SAMP, EnFlagsTokenCategory::AGGR_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_VARBINARY, gcnew Token(Keys::TOK_VARBINARY, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_VARCHAR, gcnew Token(Keys::TOK_VARCHAR, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_VARIABLE, gcnew Token(Keys::TOK_VARIABLE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_VARYING, gcnew Token(Keys::TOK_VARYING, EnFlagsTokenCategory::RESERVED, AS_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_VERSION, gcnew Token(Keys::TOK_VERSION, EnFlagsTokenCategory::FUNCTION, BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_VIEW, gcnew Token(Keys::TOK_VIEW, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_VOID, gcnew Token(Keys::TOK_VOID, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_WAIT, gcnew Token(Keys::TOK_WAIT, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_WEEK, gcnew Token(Keys::TOK_WEEK, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_WEEKDAY, gcnew Token(Keys::TOK_WEEKDAY, EnFlagsTokenCategory::PARAM_FUNCTION, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_WHEN, gcnew Token(Keys::TOK_WHEN, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_WHERE, gcnew Token(Keys::TOK_WHERE, EnFlagsTokenCategory::RESERVED, AS_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_WHILE, gcnew Token(Keys::TOK_WHILE, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_WINDOW, gcnew Token(Keys::TOK_WINDOW, EnFlagsTokenCategory::RESERVED, AS_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_WITH, gcnew Token(Keys::TOK_WITH, EnFlagsTokenCategory::RESERVED, AS_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_WITHOUT, gcnew Token(Keys::TOK_WITHOUT, EnFlagsTokenCategory::UNRESERVED,AS_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_WORK, gcnew Token(Keys::TOK_WORK, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_WRITE, gcnew Token(Keys::TOK_WRITE, EnFlagsTokenCategory::RESERVED, BARE_LABEL, i++));
	_Tokens->Add((SysStr^)Values::TOK_YEAR, gcnew Token(Keys::TOK_YEAR, EnFlagsTokenCategory::PARAM_FUNCTION, AS_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_YEARDAY, gcnew Token(Keys::TOK_YEARDAY, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_YYERRCODE, gcnew Token(Keys::TOK_YYERRCODE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));
	// _Tokens->Add((SysStr^)Values::TOK_ZONE, gcnew Token(Keys::TOK_ZONE, EnFlagsTokenCategory::UNRESERVED,BARE_LABEL, i++));


	return _Tokens;
}


}
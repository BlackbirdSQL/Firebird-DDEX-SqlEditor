#include "pch.h"
#include "Parser.h"

// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


#include "Scanner.h"
#include "UnionParser.h"
#include "SqlParser.h"
#include "OffsetCalculator.h"



namespace BlackbirdDsl {




/**
 * It parses the given SQL statement and generates a detailled
 * output array for every part of the statement. The method can
 * also generate [position] fields within the output, which hold
 * the character position for every statement part. The calculation
 * of the positions needs some time, if you don't need positions in
 * your application, set the parameter to false.
 *
 * @param SysStr  $sql           The SQL statement.
 * @param boolean $calcPositions True, if the output should contain [position], false otherwise.
 *
 * @return array An associative array with all meta information about the SQL statement.
 */
StringCell^ Parser::Execute(SysStr^ sql)
{
	/*
	* this function splits up a SQL statement into easy to "parse"
	* tokens for the SQL processor
	*/
	StringCell^ tokens = Scanner::Split(sql);

	if (TokenizeOnly)
		return tokens;


	StringCell^ output = Parse(tokens);


	// calc the positions of some important tokens
	if (OffsetCapture)
	{
		OffsetCalculator::SetOffsetsWithinSql(sql, output);
	}

	return output;
}



StringCell^ Parser::Parse(StringCell^ root)
{
	// this is the highest level lexical analysis. This is the part of the
	// code which finds UNION and UNION ALL query parts

	StringCell^ output = ParserInstance(UnionParser)->Parse(root);

	// If there was no UNION or UNION ALL in the query, then the query is
	// stored at $queries[0].
	if (!IsNullPtr(output) && !output->IsNullOrEmpty && !IsUnion(output))
	{
		output = ParserInstance(SqlParser)->Parse(output[0]);
	}

	return output;
}

}
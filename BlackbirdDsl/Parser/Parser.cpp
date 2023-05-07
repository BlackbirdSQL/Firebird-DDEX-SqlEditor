#include "pch.h"
#include "Parser.h"

// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)


#include "Scanner.h"
#include "UnionParser.h"
#include "SqlParser.h"
#include "OffsetCalculator.h"



namespace BlackbirdDsl {




// ----------------------------------------------------------------------------------
/// <summary>
/// Parses the given SQL statement and generates a nested StringCell output array of
/// the statement and optional offsets. 
/// </summary>
/// <returns>StringCell object</returns>
// ----------------------------------------------------------------------------------
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
#include "pch.h"
#include "SqlParser.h"

#include "ParserManager.h"


namespace BlackbirdDsl {


/// <summary>
/// This function breaks up the SQL statement into logical sections.
/// Some sections are delegated to specialized processors.
/// </summary>
StringCell^ SqlParser::Parse(StringCell^ root)
{
	return root;
	int skip_next = 0;

	SysStr^ prev_category = "";
	SysStr^ token_category = "";
	SysStr^ trim;
	SysStr^ upper;

	StringCell^ retnode = gcnew StringCell();
	StringCell^ token;

	// root may come as a numeric indexed array starting with an index greater than 0 (or as a boolean)
	int tokenCount = root->Count;

	for (int tokenNumber = 0; tokenNumber < tokenCount; ++tokenNumber)
	{
		if (root->IsUnpopulated[tokenNumber])
			continue;

		token = root[tokenNumber];
		trim = token->Trimmed(); // this removes also \n and \t!

		// if it starts with an "(", it should follow a SELECT
		if (trim->StartsWith("(") && token_category == "")
		{
			token_category = "BRACKET";
			prev_category = token_category;
		}

		// If it isn"t obvious, when skip_next is set, then we ignore the next real token, that is we ignore whitespace.
		if (skip_next > 0)
		{
			if (trim == "")
			{
				if (token_category != "")
				{ // is this correct??
					retnode[token_category] = token;
				}
				continue;
			}
			// to skip the token we replace it with whitespace
			trim = "";
			*token = "";
			skip_next--;

			if (skip_next > 0)
				continue;
		}

		upper = trim->ToUpper();


		// Tokens that get their own sections. These keywords have subclauses. 
		if (upper == "SELECT" || upper == "ORDER" || upper == "VALUES" || upper == "GROUP"
			|| upper == "HAVING" || upper == "WHERE" || upper == "CALL" || upper == "PROCEDURE"
			|| upper == "FUNCTION" || upper == "SERVER" || upper == "LOGFILE"
			|| upper == "DEFINER" || upper == "RETURNS" || upper == "TABLESPACE"
			|| upper == "TRIGGER" || upper == "DO" || upper == "FLUSH" || upper == "KILL"
			|| upper == "RESET" || upper == "STOP"  || upper == "PURGE" || upper == "EXECUTE"
			|| upper == "PREPARE")
		{
			token_category = upper;
		}
		else if (upper == "DEALLOCATE")
		{
			if (trim == "DEALLOCATE")
				skip_next = 1;
			token_category = upper;
		}
		else if (upper == "DUPLICATE")
		{
			if (token_category != "VALUES")
				token_category = upper;
		}
		else if (upper == "SET")
		{
			if (token_category != "TABLE")
				token_category = upper;
		}
		else if (upper == "LIMIT" || upper == "PLUGIN")
		{
			// no separate section
			if (token_category != "SHOW")
				token_category = upper;
		}
		else if (upper == "FROM")
		{
			// this FROM is different from FROM in other DML (not join related)
			if (token_category == "PREPARE")
				continue;
			// no separate section
			if (token_category != "SHOW")
				token_category = upper;
		}
		else if (upper == "EXPLAIN" || upper == "DESCRIBE" || upper == "SHOW")
		{
			token_category = upper;
		}
		else if (upper == "DESC")
		{
			// short version of DESCRIBE
			if (token_category == "")
				token_category = upper;
			// else direction of ORDER-BY
		}
		else if (upper == "RENAME")
		{
			token_category = upper;
		}
		else if (upper == "DATABASE" || upper == "SCHEMA")
		{
			if (prev_category != "DROP" && prev_category != "SHOW")
				token_category = upper;
		}
		else if (upper == "EVENT")
		{
			// issue 71
			if (prev_category == "DROP" || prev_category == "ALTER" || prev_category == "CREATE")
			{
				token_category = upper;
			}
		}
		else if (upper == "DATA")
		{
			// prevent wrong handling of DATA as keyword
			if (prev_category == "LOAD")
				token_category = upper;
		}
		else if (upper == "INTO")
		{
			// prevent wrong handling of CACHE within LOAD INDEX INTO CACHE...
			if (prev_category == "LOAD")
			{
				retnode[prev_category] = token;
				continue;
			}
			token_category = prev_category = upper;
		}
		else if (upper == "USER")
		{
			// prevent wrong processing as keyword
			if (prev_category == "CREATE" || prev_category == "RENAME" || prev_category == "DROP")
			{
				token_category = upper;
			}
		}
		else if (upper == "VIEW")
		{
			// prevent wrong processing as keyword
			if (prev_category == "CREATE" || prev_category == "ALTER" || prev_category == "DROP")
			{
				token_category = upper;
			}


			// These root get their own section, but have no subclauses. These root identify the statement but have no specific subclauses of their own.

		}
		else if (upper == "DELETE" || upper == "ALTER" || upper == "INSERT" || upper == "OPTIMIZE"
			|| upper == "GRANT" || upper == "REVOKE" || upper == "HANDLER" || upper == "LOAD"
			|| upper == "ROLLBACK" || upper == "SAVEPOINT" || upper == "UNLOCK" || upper == "INSTALL"
			|| upper == "UNINSTALL" || upper == "ANALZYE" || upper == "BACKUP" || upper == "CHECKSUM"
			|| upper == "REPAIR" || upper == "RESTORE" || upper == "HELP")
		{
			token_category = upper;
			// set the category in || upper == these get subclauses in a future version of MySQL
			retnode[upper, 0] = token;
			continue;
		}
		else if (upper == "TRUNCATE")
		{

			if (prev_category == "")
			{
				// set the category in || upper == these get subclauses in a future version of MySQL
				token_category = upper;
				retnode[upper, 0] = token;
				continue;
			}
			// part of the CREATE TABLE statement or a function
			retnode[prev_category] = token;
			continue;
		}
		else if (upper == "REPLACE")
		{
			if (prev_category == "")
			{
				// set the category in || upper == these get subclauses in a future version of MySQL
				token_category = upper;
				continue;
			}
			// part of the CREATE TABLE statement or a function
			retnode[prev_category] = token;
			continue;

		}
		else if (upper == "IGNORE")
		{
			if (prev_category == "TABLE")
			{
				// part of the CREATE TABLE statement
				retnode[prev_category] = token;
				continue;
			}
			if (token_category == "FROM")
			{
				// part of the FROM statement (index hint)
				retnode[token_category] = token;
				continue;
			}
			retnode["OPTIONS"] = (StringCell^)upper;
			continue;

		}
		else if (upper == "CHECK")
		{
			if (prev_category == "TABLE")
			{
				retnode[prev_category] = token;;
				continue;
			}
			token_category = upper;
			retnode[upper, 0] = token;
			continue;
		}
		else if (upper == "CREATE")
		{
			if (prev_category != "SHOW")
				token_category = upper;

		}
		else if (upper == "INDEX")
		{
			if (prev_category == "CREATE" || prev_category == "DROP")
			{
				retnode[prev_category] = token;;
				token_category = upper;
			}

		}
		else if (upper == "TABLE")
		{
			if (prev_category == "CREATE")
			{
				retnode[prev_category] = token;;
				token_category = upper;
			}
			if (prev_category == "TRUNCATE")
			{
				retnode[prev_category] = token;;
				token_category = upper;
			}

		}
		else if (upper == "TEMPORARY")
		{
			if (prev_category == "CREATE")
			{
				retnode[prev_category] = token;;
				token_category = prev_category;
				continue;
			}

		}
		else if (upper == "IF")
		{
			if (prev_category == "TABLE")
			{
				token_category = "CREATE";
				retnode[token_category]->Merge(retnode[prev_category]);
				retnode[prev_category] = gcnew StringCell();
				retnode[token_category] = token;
				prev_category = token_category;
				continue;
			}

		}
		else if (upper == "NOT")
		{
			if (prev_category == "CREATE")
			{
				token_category = prev_category;
				retnode[prev_category] = token;;
				continue;
			}

		}
		else if (upper == "EXISTS")
		{
			if (prev_category == "CREATE")
			{
				retnode[prev_category] = token;;
				prev_category = token_category = "TABLE";
				continue;
			}

		}
		else if (upper == "CACHE")
		{
			if (prev_category == "" || prev_category == "RESET" || prev_category == "FLUSH"
				|| prev_category == "LOAD")
			{
				token_category = upper;
				continue;
			}

		}
		else if (upper == "LOCK")
		{
			if (token_category == "")
			{
				token_category = upper;
				retnode[upper, 0] = token;
				continue;
			}
			else if (token_category != "INDEX")
			{
				trim = "LOCK IN SHARE MODE";
				skip_next = 3;
				retnode["OPTIONS"] = trim;
				continue;
			}
		}
		else if (upper == "USING")
		{
			if (token_category == "EXECUTE")
			{
				token_category = upper;
				continue;
			}
			if (token_category == "FROM" && retnode->IsUnpopulated["DELETE"])
			{
				token_category = upper;
				continue;
			}

		}
		else if (upper == "DROP")
		{
			if (token_category != "ALTER")
			{
				token_category = upper;
				continue;
			}

		}
		else if (upper == "FOR")
		{
			if (prev_category != "SHOW" && token_category == "FROM")
			{
				skip_next = 1;
				retnode["OPTIONS"] = "FOR UPDATE"; // TODO: this could be generate problems within the position calculator
				continue;
			}

		}
		else if (upper == "UPDATE")
		{
			if (token_category == "")
			{
				token_category = upper;
				continue;
			}
			if (token_category == "DUPLICATE")
			{
				continue;
			}

		}
		else if (upper == "START")
		{
			trim = "BEGIN";
			retnode[upper, 0] = upper; // TODO: this could be generate problems within the position calculator
			skip_next = 1;

		}
		else if (upper == "TO")
		{
			if (token_category != "RENAME")
				continue;

		}
		else if (upper == "BY")
		{
			if (prev_category != "TABLE")
				continue;

		}
		else if (upper == "ALL" || upper == "SHARE" || upper == "MODE" || upper == ";")
		{
			continue;
		}
		else if (upper == "KEY")
		{
			if (token_category == "DUPLICATE")
			{
				continue;
			}
		}
		else if (upper == "LOW_PRIORITY" || upper == "DELAYED" || upper == "QUICK"
			|| upper == "HIGH_PRIORITY")
		{
			retnode["OPTIONS"] = (StringCell^)trim;
			continue;

		}
		else if (upper == "USE")
		{
			if (token_category == "FROM")
			{
				// index hint within FROM clause
				retnode[token_category] = token;
				continue;
			}
			// set the category in || upper == these get subclauses in a future version of MySQL
			token_category = upper;
			retnode[upper, 0] = token;

		}
		else if (upper == "FORCE")
		{
			if (token_category == "FROM")
			{
				// index hint within FROM clause
				retnode[token_category] = token;
				continue;
			}
			retnode->Add("OPTIONS", trim);
			continue;

		}
		else if (upper == "WITH")
		{
			if (token_category == "GROUP")
			{
				skip_next = 1;
				retnode->Add("OPTIONS", "WITH ROLLUP"); // TODO: this could be generate problems within the position calculator
				continue;
			}
			if (token_category == "")
			{
				token_category = upper;
			}

		}
		else if (upper == "AS")
		{
		}
		else if (upper == "" || upper == ",")
		{
		}


		// remove obsolete category after union (empty category because of
		// empty token before select)
		if (token_category != "" && (prev_category == token_category))
		{
			retnode[token_category] = token;
		}

		prev_category = token_category;

	}

	if (retnode->Count == 0)
		return NullCell;

	return ParserManager::Parse(retnode, _Options);
	
}

}
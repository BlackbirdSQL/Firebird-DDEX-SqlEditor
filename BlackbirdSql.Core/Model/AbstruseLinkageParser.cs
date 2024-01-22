// $License = https://github.com/BlackbirdSQL/NETProvider-DDEX/blob/master/Docs/license.txt
// $Authors = GA Christos (greg@blackbirdsql.org)

using System;
using BlackbirdDsl;
using C5;


namespace BlackbirdSql.Core.Model;

// =========================================================================================================
//										AbstruseLinkageParser Class
//
/// <summary>
/// Handles Trigger / Generator linkage parsing specific tasks utilizing BlackbirdDsl.<see cref="Parser"/>.
/// </summary>
// =========================================================================================================
public abstract class AbstruseLinkageParser : IDisposable
{


	// -----------------------------------------------------------------------------------------------------
	#region Fields - AbstruseLinkageParser
	// -----------------------------------------------------------------------------------------------------



	// A static class lock
	protected static object _LockClass = new object();
	// A protected 'this' object lock
	protected object _LockObject = new();

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Instance of the BlackbirdDsl.<see cref="Parser"/>.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected Parser _DslParser = null;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - AbstruseLinkageParser
	// =========================================================================================================


	/// <summary>
	/// Getter to retrieve or create an instance of the BlackbirdDsl.<see cref="Parser"/>.
	/// </summary>
	public Parser DslParser => _DslParser ??= new Parser(DslOptions.TOKENIZE_ONLY);


	#endregion Property accessors





	// =========================================================================================================
	#region Constructors / Destructors - AbstruseLinkageParser
	// =========================================================================================================


	/// <summary>
	/// Protected default .ctor for creating an instance for a db connection.
	/// </summary>
	/// <param name="connection"></param>
	protected AbstruseLinkageParser()
	{
		// Tracer.Trace(typeof(AbstruseLinkageParser), $"StaticId:[{"0000"}] AbstruseLinkageParser(FbConnection)");
	}



	/// <summary>
	/// Disposes of a parser.
	/// </summary>
	/// <param name="disposing">
	/// True if this is a permanent disposal and a transient
	/// parser should not be stored else false.
	/// </param>
	/// <returns>True of the parser was found and disposed else false.</returns>
	public abstract void Dispose();


	/// <summary>
	/// Disposes of a parser.
	/// </summary>
	/// <param name="disposing">
	/// True if this is a permanent disposal and a transient
	/// parser should not be stored else false.
	/// </param>
	/// <returns>True of the parser was found and disposed else false.</returns>

	protected abstract bool Dispose(bool disposing);


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Methods - AbstruseLinkageParser
	// =========================================================================================================


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Parses a trigger DSL statement.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected (string, int, int) ParseTriggerDSL(string sql)
	{
		return ParseTriggerDSL(sql, null, null, null);
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Parses a trigger DSL statement given a trigger name.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected (string, int, int) ParseTriggerDSL(string sql, string trigger)
	{
		return ParseTriggerDSL(sql, trigger, null, null);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Parses a trigger DSL statement givern a trigger and table name.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected (string, int, int) ParseTriggerDSL(string sql, string trigger, string table)
	{
		return ParseTriggerDSL(sql, trigger, table, null);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Parses a trigger DSL statement givern a trigger, table name and column name.
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected (string, int, int) ParseTriggerDSL(string sql, string trigger, string table, string column)
	{
		int increment = -1;
		int seed = -1;
		string generator = null;


		int sequence = -1;
		int stage = 0;
		int i;
		string token, sequenceToken;

		string[][] _Sequences =
		[
			[ "CREATE", "TRIGGER", "_TRIGGER_", "FOR", "_TABLE_",
				"AS", "BEGIN", "_COLUMN_", "=", "GEN_ID", "_GENPARAM_" ],
			[ "CREATE", "TRIGGER", "_TRIGGER_", "FOR", "_TABLE_",
				"AS", "BEGIN", "_COLUMN_", "=", "NEXT", "VALUE", "FOR", "_GENERATOR_" ],
			// new string[] { "SET", "GENERATOR", "_GENERATOR_", "TO", "_SEED_" },
			// new string[] { "ALTER", "SEQUENCE", "_GENERATOR_", "RESTART", "WITH", "_SEED_" }
		];

		bool[] _Completed = [false, false /*, false, false */];

		bool[][] _Sequencing =
		[
			[ false, true, true, true, true,
				false, false, false, true, true, true ],
			[ false, true, true, true, true,
				false, false, false, true, true, true, true, true ],
			// new bool[] { false, true, true, true, true },
			// new bool[] { false, true, true, true, true, true }
		];




		StringCell tokens = DslParser.Execute(sql.ToUpper());


		foreach (StringCell tokenCell in tokens.Enumerator)
		{
			token = tokenCell.ToString().Trim();

			if (String.IsNullOrEmpty(token))
				continue;

			if (sequence == -1)
			{
				stage = 0;
				for (i = 0; i < _Sequences.Length; i++)
				{
					if (_Completed[i])
						continue;

					if (token == _Sequences[i][stage])
					{
						sequence = i;
						stage++;
						break;
					}
					// Special case
					if (i < 2 && token == _Sequences[i][5])
					{
						sequence = i;
						stage = 6;
						break;
					}
				}
				continue;
			}

			sequenceToken = _Sequences[sequence][stage];

			switch (sequenceToken)
			{
				case "GEN_ID":
					// Single special case. We're not going to do nested sequences just for one.
					if (token != sequenceToken)
					{
						sequence++;
						sequenceToken = _Sequences[sequence][stage];
					}
					break;
				case "_TRIGGER_":
					if (trigger != null)
						sequenceToken = trigger;
					else
						sequenceToken = token;
					break;
				case "_TABLE_":
					if (table != null)
						sequenceToken = table;
					else
						sequenceToken = token;
					break;
				case "_COLUMN_":
					if (column != null)
						sequenceToken = "NEW." + column;
					else if (token.StartsWith("NEW."))
						sequenceToken = token;
					break;
				case "_GENPARAM_":
					if (token.StartsWith("(") && token.EndsWith(")"))
					{
						(generator, increment) = GetGenIdParams(token);
						if (generator != null)
							sequenceToken = token;
					}
					break;
				case "_GENERATOR_":
					if (generator == null)
					{
						generator = token;
						increment = 1;
					}
					sequenceToken = generator;
					break;
				case "_SEED_":
					seed = Convert.ToInt32(token);
					sequenceToken = token;
					break;
			}

			if (token != sequenceToken)
			{
				if (_Sequencing[sequence][stage])
					sequence = -1;
				continue;
			}

			stage++;

			if (stage == _Sequences[sequence].Length)
			{
				if (sequence < 2)
					_Completed[0] = _Completed[1] = true;
				else
					_Completed[2] = _Completed[3] = true;
				sequence = -1;
			}

			for (i = 0; i < _Completed.Length; i++)
			{
				if (!_Completed[i])
					break;
			}

			if (i == _Completed.Length)
				break;
		}

		if (seed == -1 && generator != null)
			seed = 0;

		return (generator, increment, seed);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Splits the legacy GEN_ID function arguments into generator name and increment. 
	/// </summary>
	// ---------------------------------------------------------------------------------
	protected (string, int) GetGenIdParams(string param)
	{
		int increment = -1;
		string generator = null;

		char[] chrs = ['(', ')', ' '];

		string[] parameters = param.Trim(chrs).Split(',');

		if (parameters.Length > 0)
			generator = parameters[0].Trim();

		if (parameters.Length > 1)
			increment = Convert.ToInt32(parameters[1].Trim());
		else if (generator != null)
			increment = 1;

		return (generator, increment);
	}


	#endregion Methods

}

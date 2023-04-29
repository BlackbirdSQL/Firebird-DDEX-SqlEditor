using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Threading;

using FirebirdSql.Data.FirebirdClient;

using C5;
using BlackbirdDsl;
using BlackbirdSql.VisualStudio.Ddex.Schema;
using Nerdbank.Streams;
using System.Windows;
using System.Reflection.Emit;
using System.Diagnostics.Metrics;
using Microsoft.Build.Framework.XamlTypes;

namespace BlackbirdSql.Common.Extensions;

/// <summary>
/// Handles <see cref="DslParser"/> parsing specific tasks.
/// </summary>
internal abstract class AbstruseLinkageParser
{
	protected Parser _DslParser = null;

	protected FbConnection _Connection = null;

	protected System.Diagnostics.Stopwatch _Stopwatch;



	public System.Diagnostics.Stopwatch Stopwatch
	{
		get
		{
			_Stopwatch ??= new();

			return _Stopwatch;
		}
	}


	public Parser DslParser
	{
		get
		{
			_DslParser ??= new Parser();
			return _DslParser;
		}

	}



	protected AbstruseLinkageParser(FbConnection connection)
	{
		_Connection = connection;
	}


	public (string, int, int) ParseGeneratorInfo(string sql)
	{
		return ParseGeneratorInfo(sql, null, null, null);
	}


	public (string, int, int) ParseGeneratorInfo(string sql, string trigger)
	{
		return ParseGeneratorInfo(sql, trigger, null, null);
	}


	public (string, int, int) ParseGeneratorInfo(string sql, string trigger, string table)
	{
		return ParseGeneratorInfo(sql, trigger, table, null);
	}

	public (string, int, int) ParseGeneratorInfo(string sql, string trigger, string table, string column)
	{
		int increment = -1;
		int seed = -1;
		string generator = null;


		int sequence = -1;
		int stage = 0;
		int i;
		string token, sequenceToken;

		string[][] _Sequences =
		{
			new string[] { "CREATE", "TRIGGER", "_TRIGGER_", "FOR", "_TABLE_",
				"AS", "BEGIN", "_COLUMN_", "=", "GEN_ID", "_GENPARAM_" },
			new string[] { "CREATE", "TRIGGER", "_TRIGGER_", "FOR", "_TABLE_",
				"AS", "BEGIN", "_COLUMN_", "=", "NEXT", "VALUE", "FOR", "_GENERATOR_" },
			// new string[] { "SET", "GENERATOR", "_GENERATOR_", "TO", "_SEED_" },
			// new string[] { "ALTER", "SEQUENCE", "_GENERATOR_", "RESTART", "WITH", "_SEED_" }
		};

		bool[] _Completed = { false, false /*, false, false */ };

		bool[][] _Sequencing =
		{
			new bool[] { false, true, true, true, true,
				false, false, false, true, true, true },
			new bool[] { false, true, true, true, true,
				false, false, false, true, true, true, true, true },
			// new bool[] { false, true, true, true, true },
			// new bool[] { false, true, true, true, true, true }
		};




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

	protected (string, int) GetGenIdParams(string param)
	{
		int increment = -1;
		string generator = null;

		char[] chrs = { '(', ')', ' ' };

		string[] parameters = param.Trim(chrs).Split(',');

		if (parameters.Length > 0)
			generator = parameters[0].Trim();

		if (parameters.Length > 1)
			increment = Convert.ToInt32(parameters[1].Trim());
		else if (generator != null)
			increment = 1;

		return (generator, increment);
	}

}

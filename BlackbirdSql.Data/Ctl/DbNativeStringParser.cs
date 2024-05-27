
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using FirebirdSql.Data.Isql;



namespace BlackbirdSql.Data.Ctl;


public class DbNativeStringParser
{
	public DbNativeStringParser(string targetString)
	{
		_Tokens = [" "];
		_Source = targetString;
		_SourceLength = targetString.Length;
	}



	private readonly string _Source;
	private readonly int _SourceLength;
	private string[] _Tokens;



	public string[] Tokens
	{
		get
		{
			return _Tokens;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException();
			}

			foreach (string value2 in value)
			{
				if (value == null)
				{
					throw new ArgumentNullException();
				}

				if (string.IsNullOrEmpty(value2))
				{
					throw new ArgumentException();
				}
			}

			_Tokens = value;
		}
	}



	public IEnumerable<FbStatement> Parse()
	{
		int num = 0;
		int index2 = 0;
		StringBuilder rawResult = new StringBuilder();
		while (index2 < _SourceLength)
		{
			if (GetChar(index2) == '\'')
			{
				rawResult.Append(GetChar(index2));
				index2++;
				rawResult.Append(ProcessLiteral(ref index2));
				rawResult.Append(GetChar(index2));
				index2++;
				continue;
			}

			if (GetChar(index2) == '-' && GetNextChar(index2) == '-')
			{
				index2++;
				ProcessSinglelineComment(ref index2);
				index2++;
				continue;
			}

			if (GetChar(index2) == '/' && GetNextChar(index2) == '*')
			{
				index2++;
				ProcessMultilineComment(ref index2);
				index2++;
				continue;
			}

			string[] tokens = Tokens;
			int num2 = 0;
			while (true)
			{
				if (num2 < tokens.Length)
				{
					string text = tokens[num2];
					if (string.Compare(_Source, index2, text, 0, text.Length, StringComparison.Ordinal) == 0)
					{
						index2 += text.Length;
						FbStatement statement = Reflect.CreateInstance<FbStatement>([_Source.Substring(num, index2 - num - text.Length),
							rawResult.ToString()]);
						yield return statement;
						num = index2;
						rawResult.Clear();
						break;
					}

					num2++;
					continue;
				}

				if (rawResult.Length != 0 || !char.IsWhiteSpace(GetChar(index2)))
				{
					rawResult.Append(GetChar(index2));
				}

				index2++;
				break;
			}
		}

		if (index2 >= _SourceLength)
		{
			string text2 = _Source[num..];

			if (!(text2.Trim() == string.Empty))
			{
				FbStatement statement = Reflect.CreateInstance<FbStatement>([text2, rawResult.ToString()]);

				yield return statement;

				_ = _SourceLength;
				rawResult.Clear();
			}
		}
		else
		{
			FbStatement statement = Reflect.CreateInstance<FbStatement>([_Source[num..index2], rawResult.ToString()]);

			yield return statement;

			rawResult.Clear();
		}
	}

	private string ProcessLiteral(ref int index)
	{
		StringBuilder stringBuilder = new StringBuilder();
		while (index < _SourceLength)
		{
			if (GetChar(index) == '\'')
			{
				if (GetNextChar(index) != '\'')
				{
					break;
				}

				stringBuilder.Append(GetChar(index));
				index++;
			}

			stringBuilder.Append(GetChar(index));
			index++;
		}

		return stringBuilder.ToString();
	}

	private void ProcessMultilineComment(ref int index)
	{
		while (index < _SourceLength)
		{
			if (GetChar(index) == '*' && GetNextChar(index) == '/')
			{
				index++;
				break;
			}

			index++;
		}
	}

	private void ProcessSinglelineComment(ref int index)
	{
		while (index < _SourceLength && GetChar(index) != '\n')
		{
			if (GetChar(index) == '\r')
			{
				if (GetNextChar(index) == '\n')
				{
					index++;
				}

				break;
			}

			index++;
		}
	}

	private char GetChar(int index)
	{
		return _Source[index];
	}

	private char? GetNextChar(int index)
	{
		if (index + 1 >= _SourceLength)
		{
			return null;
		}

		return _Source[index + 1];
	}
}

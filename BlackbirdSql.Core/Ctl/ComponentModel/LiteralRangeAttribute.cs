using System;


namespace BlackbirdSql.Core.Ctl.ComponentModel;

/// <summary>
/// /// Range attribute that provides literal suffixes and values in the edit box.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class LiteralRangeAttribute : Attribute
{
	private readonly int _MinLen = -1;
	private readonly int _MaxLen = -1;
	private readonly int _Min = int.MinValue;
	private readonly int _Max = int.MinValue;
	private readonly string _Uom = "";



	public int MinLen => _MinLen;
	public int MaxLen => _MaxLen;

	public int Min => _Min;
	public int Max => _Max;
	public string Uom => _Uom;

	public LiteralRangeAttribute(int min, int max, string uom)
	{
		_Min = min;
		_Max = max;
		_Uom = uom;
	}

	public LiteralRangeAttribute(string uom, int minlen, int maxlen)
	{
		_Uom = uom;
		_MaxLen = maxlen;
		_MinLen = minlen;
	}

}

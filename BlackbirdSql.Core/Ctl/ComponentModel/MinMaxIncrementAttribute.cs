using System;


namespace BlackbirdSql.Core.Ctl.ComponentModel;

/// <summary>
/// /// Can be extended to other numeric types by adding constructors
/// for them and tagging the type.
/// Use in conjunction with RangeConverter in place of RangeAttribute if rounding
/// is required. Increment determines rounding.
/// </summary>
/// <remarks>
/// Rounding has not been implemented yet. Just add a constructor for double
/// and determine rounding from the decimal places on Increment if you need it.
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class MinMaxIncrementAttribute : Attribute
{
	private readonly int _Min;
	private readonly int _Max;
	private readonly int _Increment;


	public int Min => _Min;
	public int Max => _Max;
	public int Increment => _Increment;


	public MinMaxIncrementAttribute(int min, int max, int increment = 1)
	{
		_Min = min;
		_Max = max;
		_Increment = increment;
	}
}

// IndexRange, Version=1.0.3.0, Culture=neutral, PublicKeyToken=35e6a3c4212514c6
// System.Index
using System.Runtime.CompilerServices;
using SysResources = BlackbirdSql.Sys.Properties.Resources;


namespace System;

public readonly struct Index : IEquatable<Index>
{
	private readonly int _value;

	public static Index Start => new Index(0);

	public static Index End => new Index(-1);

	public int Value
	{
		get
		{
			if (_value < 0)
			{
				return ~_value;
			}
			return _value;
		}
	}

	public bool IsFromEnd => _value < 0;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Index(int value, bool fromEnd = false)
	{
		if (value < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(value), SysResources.ExceptionNonNegativeRequired);
		}
		if (fromEnd)
		{
			_value = ~value;
		}
		else
		{
			_value = value;
		}
	}

	private Index(int value)
	{
		_value = value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Index FromStart(int value)
	{
		if (value < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(value), SysResources.ExceptionNonNegativeRequired);
		}
		return new Index(value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Index FromEnd(int value)
	{
		if (value < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(value), SysResources.ExceptionNonNegativeRequired);
		}
		return new Index(~value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetOffset(int length)
	{
		int offset = _value;
		if (IsFromEnd)
		{
			offset += length + 1;
		}
		return offset;
	}

#nullable enable
	public override bool Equals(object? value)
	{
		if (value is Index index)
		{
			return _value == index._value;
		}
		return false;
	}
#nullable disable


	public bool Equals(Index other)
	{
		return _value == other._value;
	}

	public override int GetHashCode()
	{
		return _value;
	}

	public static implicit operator Index(int value)
	{
		return FromStart(value);
	}

	public override string ToString()
	{
		if (IsFromEnd)
		{
			return "^" + (uint)Value;
		}
		return ((uint)Value).ToString();
	}
}

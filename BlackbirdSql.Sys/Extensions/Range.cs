// IndexRange, Version=1.0.3.0, Culture=neutral, PublicKeyToken=35e6a3c4212514c6
// System.Range

using System.Runtime.CompilerServices;



namespace System;


public readonly struct Range : IEquatable<Range>
{

	public Range(Index start, Index end)
	{
		Start = start;
		End = end;
	}




	public Index Start { get; }

	public Index End { get; }

	public static Range All => Index.Start..Index.End;



#nullable enable
	public override bool Equals(object? value)
	{
		if (value is Range r && r.Start.Equals(Start))
		{
			return r.End.Equals(End);
		}
		return false;
	}
#nullable disable

	public bool Equals(Range other)
	{
		if (other.Start.Equals(Start))
		{
			return other.End.Equals(End);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Start.GetHashCode() * 31 + End.GetHashCode();
	}

	public override string ToString()
	{
		return Start.ToString() + ".." + End;
	}

	public static Range StartAt(Index start)
	{
		return start..Index.End;
	}

	public static Range EndAt(Index end)
	{
		return Index.Start..end;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	// [CLSCompliant(false)]
	public (int Offset, int Length) GetOffsetAndLength(int length)
	{
		int start = Start.GetOffset(length);
		int end = End.GetOffset(length);
		if ((uint)end > (uint)length || (uint)start > (uint)end)
		{
			throw new ArgumentOutOfRangeException("length");
		}
		return (start, end - start);
	}
}

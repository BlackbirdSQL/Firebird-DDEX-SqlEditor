using System;


namespace BlackbirdSql.Core.Ctl.ComponentModel;



/// <summary>
/// /// Provides parameters for use by other descriptors.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class ParametersAttribute(params object[] args) : Attribute()
{

	private readonly object[] _Args = args;


	public object this[int index]
		=> _Args.Length > index ? _Args[index] : null;

	public int Length => _Args != null ? _Args.Length : 0;
}

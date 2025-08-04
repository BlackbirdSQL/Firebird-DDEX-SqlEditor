

using System.Diagnostics.Tracing;
using BlackbirdSql.Sys.Ctl.Diagnostics;



namespace BlackbirdSql.Core;

[EventSource(Name = $"BlackbirdSql.{C_Name}.Etw")]


public class Evs : AbstractEvs<Evs>
{
	public Evs() : base(C_Name)
	{
	}

	private const string C_Name = "Core";
}
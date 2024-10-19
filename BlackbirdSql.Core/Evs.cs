

using System.Diagnostics.Tracing;
using BlackbirdSql.Sys.Ctl.Diagnostics;



namespace BlackbirdSql.Core;

[EventSource(Name = C_Name)]


public class Evs : AbstractEvs<Evs>
{
	public Evs() : base()
	{
	}

	private const string C_Name = "BlackbirdSql.Core.Etw";
}
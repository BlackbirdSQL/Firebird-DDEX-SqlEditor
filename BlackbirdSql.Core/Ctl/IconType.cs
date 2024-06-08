using BlackbirdSql.Sys.Interfaces;

namespace BlackbirdSql.Core.Ctl;


public class IconType(string name, string prefix) : IBIconType
{
	private readonly string _Name = name;
	private readonly string _Prefix = prefix;



	protected static int _Seed = -1;
	private readonly int _Id = ++_Seed;


	public int Id => _Id;
	public string Name => _Name;

	public override string ToString() => $"{_Prefix}{Name}x";
}

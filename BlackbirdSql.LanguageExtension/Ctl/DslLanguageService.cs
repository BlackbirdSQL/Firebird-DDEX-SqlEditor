



using BlackbirdSql.Core.Model;
using BlackbirdSql.LanguageExtension.Properties;

namespace BlackbirdSql.LanguageExtension
{
	public class DslLanguageService : AbstractLanguageService
	{
		public override string Name => "Blackbird SQL Tools";


		public override string GetFormatFilterList()
		{
			return string.Format(Resources.FormatFilterList, MonikerAgent.C_SqlExtension) + $"\n*{MonikerAgent.C_SqlExtension}\n";
		}

	}
}

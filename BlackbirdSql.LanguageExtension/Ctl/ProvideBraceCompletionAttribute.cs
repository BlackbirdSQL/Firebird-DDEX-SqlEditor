
using System;
using System.Globalization;
using Microsoft.VisualStudio.Shell;

namespace BlackbirdSql.LanguageExtension
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	public class ProvideBraceCompletionAttribute : RegistrationAttribute
	{
		private readonly string languageName;

		public ProvideBraceCompletionAttribute(string languageName)
		{
			this.languageName = languageName;
		}

		public override void Register(RegistrationContext context)
		{
			string name = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}\\{2}", "Languages", "Language Services", languageName);
			using Key key = context.CreateKey(name);
			key.SetValue("ShowBraceCompletion", 1);
		}

		public override void Unregister(RegistrationContext context)
		{
			string name = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}\\{2}", "Languages", "Language Services", languageName);
			context.RemoveKey(name);
		}
	}
}

//
// Plagiarized from Community.VisualStudio.Toolkit extension
//

using System;
using System.Reflection;



namespace BlackbirdSql.Core.Ctl.Interfaces;


public interface IBModelPropertyWrapper
{
	PropertyInfo PropInfo { get; }
	string PropertyName { get; }
	string Automator { get; }
	bool IsAutomator { get; }
	bool InvertAutomation { get; }
	int AutomationEnableValue { get; }


	int DisplayOrder { get; }

	object DefaultValue { get; }

	Func<object, object> WrappedPropertyGetMethod { get; }

}

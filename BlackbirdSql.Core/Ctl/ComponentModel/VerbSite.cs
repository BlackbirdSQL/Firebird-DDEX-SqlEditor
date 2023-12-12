using System;
using System.ComponentModel.Design;
using System.ComponentModel;
using System.Reflection;
using BlackbirdSql.Core.Ctl.Events;

using Microsoft.VisualStudio.Utilities;


namespace BlackbirdSql.Core.Ctl.ComponentModel;

// =========================================================================================================
//										VerbSite Class
//
/// <summary>
/// Settings models Site for handling DialogPage verbs.
/// </summary>
// =========================================================================================================
public class VerbSite(object component) : IMenuCommandService, ISite
{

	// our target object
	protected object _Component = component;


	public event AutomationVerbEventHandler AutomationVerbExecutedEvent;


	#region IMenuCommandService Members
	// IMenuCommandService provides DesignerVerbs, seen as commands in the PropertyGrid control

	public void AddCommand(MenuCommand command)
	{
		throw new NotImplementedException();
	}

	public void AddVerb(DesignerVerb verb)
	{
		throw new NotImplementedException();
	}

	public MenuCommand FindCommand(CommandID commandID)
	{
		throw new NotImplementedException();
	}

	public bool GlobalInvoke(CommandID commandID)
	{
		throw new NotImplementedException();
	}

	public void RemoveCommand(MenuCommand command)
	{
		throw new NotImplementedException();
	}

	public void RemoveVerb(DesignerVerb verb)
	{
		throw new NotImplementedException();
	}

	public void ShowContextMenu(CommandID menuID, int x, int y)
	{
		throw new NotImplementedException();
	}

	// ** Item of interest ** Return the DesignerVerbs collection
	public DesignerVerbCollection Verbs
	{
		get
		{
			string cmdText;
			CommandID cmdId;
			CommandIdAttribute cmdIdAttr;
			DesignerVerbCollection verbs = [];

			// Use reflection to enumerate all the public methods on the object
			MethodInfo[] mia = _Component.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);

			foreach (MethodInfo mi in mia)
			{
				object[] cmdIdAttrs = mi.GetCustomAttributes(typeof(CommandIdAttribute), true);
				if (cmdIdAttrs == null || cmdIdAttrs.Length == 0)
					continue;

				object[] displayAttrs = mi.GetCustomAttributes(typeof(GlobalizedVerbTextAttribute), true);

				if (displayAttrs != null && displayAttrs.Length > 0)
					cmdText = ((GlobalizedVerbTextAttribute)displayAttrs[0]).DisplayName;
				else
					cmdText = mi.Name;

				// Add a DesignerVerb with our VerbEventHandler
				// The method name will appear in the command pane
				cmdIdAttr = (CommandIdAttribute)cmdIdAttrs[0];
				cmdId = new CommandID(new Guid(cmdIdAttr.CommandSetGuid), cmdIdAttr.CommandId);
				verbs.Add(new DesignerVerb(cmdText, new EventHandler(OnDesignerVerbInvoked), cmdId));
			}

			return verbs;
		}
	}

	// ** Item of interest ** Handle invokaction of the DesignerVerbs
	private void OnDesignerVerbInvoked(object sender, EventArgs e)
	{
		AutomationVerbExecutedEvent?.Invoke(sender, e);
	}

	#endregion

	#region ISite Members
	// ISite required to represent this object directly to the PropertyGrid

	public IComponent Component
	{
		get { throw new NotImplementedException(); }
	}

	// ** Item of interest ** Implement the Container property
	public IContainer Container
	{
		// Returning a null Container works fine in this context
		get { return null; }
	}

	// ** Item of interest ** Implement the DesignMode property
	public bool DesignMode
	{
		// While this *is* called, it doesn't seem to matter whether we return true or false
		get { return true; }
	}

	public string Name
	{
		get { return null; }
		set { throw new NotImplementedException(); }
	}

	#endregion

	#region IServiceProvider Members
	// IServiceProvider is the mechanism used by the PropertyGrid to discover our IMenuCommandService support

	// ** Item of interest ** Respond to requests for IMenuCommandService
	public object GetService(Type serviceType)
	{
		if (serviceType == typeof(IMenuCommandService))
			return this;
		return null;
	}

	#endregion
}

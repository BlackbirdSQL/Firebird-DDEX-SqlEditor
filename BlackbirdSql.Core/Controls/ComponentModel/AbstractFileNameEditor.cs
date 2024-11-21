// System.Windows.Forms.Design, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// System.Windows.Forms.Design.FileNameEditor
using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using BlackbirdSql.Core.Ctl.ComponentModel;
using BlackbirdSql.Core.Properties;

namespace BlackbirdSql.Core.Controls.ComponentModel;

/// <summary>
/// File name lookup dialog editor using ParametersAttribute and the local
/// Properties.AttributeResources.resx for parameterizing the dialog. 
/// Where arg[0]: Title resource, arg[1] = Filter resource and optionally
/// arg[2]: CheckFileExists [default: false]. 
/// </summary>
public abstract class AbstractFileNameEditor : UITypeEditor
{
	private ITypeDescriptorContext _Context = null;
	private OpenFileDialog _OpenFileDialog;

	public System.Resources.ResourceManager LocalResMgr => AttributeResources.ResourceManager;
	public abstract System.Resources.ResourceManager ResMgr { get; }


	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		if (provider != null && provider.GetService(typeof(IWindowsFormsEditorService)) is IWindowsFormsEditorService)
		{
			if (_OpenFileDialog == null)
			{
				_OpenFileDialog = new OpenFileDialog();
				_Context = context;
				InitializeDialog(_OpenFileDialog);
			}

			string initialFullPath = null;
			string fileName = value as string;

			if (fileName != null)
			{
				initialFullPath = Path.GetFullPath(fileName);
				string initialPath = Path.GetDirectoryName(initialFullPath);
				string initialFile = Path.GetFileName(initialFullPath);

				_OpenFileDialog.InitialDirectory = initialPath;
				_OpenFileDialog.FileName = initialFile;
			}

			if (_OpenFileDialog.ShowDialog() == DialogResult.OK)
			{

				string result = Path.GetFullPath(_OpenFileDialog.FileName);

				if (fileName != null && result.Equals(initialFullPath, StringComparison.OrdinalIgnoreCase))
					result = fileName;

				return result;
			}
		}
		return value;
	}

	public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
	{
		return UITypeEditorEditStyle.Modal;
	}

	protected virtual void InitializeDialog(OpenFileDialog openFileDialog)
	{
		if (openFileDialog == null)
		{
			throw new ArgumentNullException("openFileDialog");
		}

		bool localTitle = true;
		bool localFilter = true;
		bool checkExists = false;
		string title = "GlobalizedDialogGenericOpenFile";
		string filter = "GlobalizedDialogFilterGeneric";

		try
		{
			if (_Context.PropertyDescriptor.Attributes[typeof(ParametersAttribute)] is ParametersAttribute paramsAttr)
			{
				if (paramsAttr.Length > 0)
				{
					localTitle = false;
					title = (string)paramsAttr[0];
				}
				if (paramsAttr.Length > 1)
				{
					localFilter = false;
					filter = (string)paramsAttr[1];
				}
				checkExists = paramsAttr.Length > 2 && (bool)paramsAttr[2];
			}

			openFileDialog.Title = localTitle ? LocalResMgr.GetString(title) : ResMgr.GetString(title);
			openFileDialog.Filter = localFilter ? LocalResMgr.GetString(filter) : ResMgr.GetString(filter);
			openFileDialog.CheckFileExists = checkExists;
		}
		catch (Exception ex)
		{
			Diag.Ex(ex);
			throw ex;
		}
	}
}

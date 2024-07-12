// Microsoft.VisualStudio.Data.Providers.Common, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Providers.Common.Host

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;



namespace BlackbirdSql.Core.Ctl;


/// <summary>
/// Editor related Host services.
/// </summary>
public class Hostess(IServiceProvider dataViewHierarchyServiceProvider)
	: AbstractHostess(dataViewHierarchyServiceProvider)
{
	public static Type GetManagedTypeFromCLSID(Guid classId)
	{
		Type type = Type.GetTypeFromCLSID(classId);

		if (type != null && type.IsCOMObject)
			type = null;

		return type;
	}


	public static Type GetTypeFromAssembly(Assembly assembly, string typeName, bool throwOnError = false)
	{
		return assembly.GetType(typeName, throwOnError);
	}


	public static Assembly LoadAssemblyFrom(string fileName)
		=> Assembly.LoadFrom(fileName);





	public static object CreateManagedInstance(Guid classId)
	{
		Type managedTypeFromCLSID = GetManagedTypeFromCLSID(classId)
			?? throw new TypeLoadException(classId.ToString("B"));
		return Activator.CreateInstance(managedTypeFromCLSID);
	}



	public static object CreateInstance(Type type, params object[] args)
	{
		try
		{
			if (type.IsInterface)
				throw new TypeAccessException($"Cannot create interface instance: {type.Name}.");

			return Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, args, null);
		}
		catch (TargetInvocationException ex)
		{
			Diag.Dug(ex);
			throw ex.InnerException;
		}
	}
}

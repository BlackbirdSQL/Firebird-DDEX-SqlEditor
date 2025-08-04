// Microsoft.VisualStudio.Data.Providers.Common, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Providers.Common.Host

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;



namespace BlackbirdSql.Core.Ctl;


/// <summary>
/// Editor related Host services.
/// </summary>
internal class Hostess(IServiceProvider dataViewHierarchyServiceProvider)
	: AbstractHostess(dataViewHierarchyServiceProvider)
{
	internal static Type GetManagedTypeFromCLSID(Guid classId)
	{
		Type type = Type.GetTypeFromCLSID(classId);

		if (type != null && type.IsCOMObject)
			type = null;

		return type;
	}


	internal static Type GetTypeFromAssembly(Assembly assembly, string typeName, bool throwOnError = false)
	{
		return assembly.GetType(typeName, throwOnError);
	}


	internal static Assembly LoadAssemblyFrom(string fileName)
		=> Assembly.LoadFrom(fileName);





	internal static object CreateManagedInstance(Guid classId)
	{
		Type managedTypeFromCLSID = GetManagedTypeFromCLSID(classId)
			?? throw new TypeLoadException(classId.ToString("B"));
		return Activator.CreateInstance(managedTypeFromCLSID);
	}



	internal static object CreateInstance(Type type, params object[] args)
	{
		try
		{
			if (type.IsInterface)
				throw new TypeAccessException($"Cannot create interface instance: {type.Name}.");

			return Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, args, null);
		}
		catch (TargetInvocationException ex)
		{
			Diag.Ex(ex);
			throw ex.InnerException;
		}
	}
}

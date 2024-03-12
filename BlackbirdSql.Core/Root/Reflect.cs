using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


namespace BlackbirdSql.Core;

// =========================================================================================================
//												Reflect Class
//
/// <summary>
/// The central location for exposing class members with restricted access modifiers. No code should ever
/// access or update restricted members outside of this class so that we can keep tabs on where in the
/// BlackbirdSql extension suite code is bypassing access modifiers.
/// No restricted access whatsoever should take place outside of this class.
/// </summary>
// =========================================================================================================
public abstract class Reflect
{


	public static T CreateInstance<T>(params object[] args)
	{
		var type = typeof(T);
		var instance = type.Assembly.CreateInstance(
			type.FullName, false,
			BindingFlags.Instance | BindingFlags.NonPublic,
			null, args, null, null);
		return (T)instance;
	}





	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates and adds an event handler delegate to a class object event handler given
	/// the containing delegate method class object, the name of the delegated method,
	/// the method access modifier binding flags, the event containing class object, the
	/// event name and the event access modifier binding flags.
	/// </summary>
	/// <returns>
	/// The created method delegate which can be stored and used in future calls to
	/// <see cref="RemoveEventHandler"/>.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public static Delegate AddEventHandler(object handlerMethodContainerInstance, string handlerMethodName,
		BindingFlags handlerMethodBindingFlags, object eventContainerClassInstance, string eventName,
		BindingFlags eventBindingFlags)
	{
		MethodInfo handlerMethodInfo = handlerMethodContainerInstance.GetType().GetMethod(handlerMethodName,
			handlerMethodBindingFlags);

		if (handlerMethodInfo == null)
		{
			COMException ex = new($"Could not find method info for '{handlerMethodName}()' in container class '{handlerMethodContainerInstance.GetType()}'. Aborting.");
			Diag.Dug(ex);
			return null;
		}

		Type typeEventContainerClassInstance = eventContainerClassInstance.GetType();

		EventInfo eventInfo = typeEventContainerClassInstance.GetEvent(eventName, eventBindingFlags);

		if (eventInfo == null)
		{
			COMException ex = new($"Could not get EventInfo for adding a delegate to event '{eventName}' in container class '{typeEventContainerClassInstance}'.");
			Diag.Dug(ex);
			return null;
		}

		Type eventHandlerType = eventInfo.EventHandlerType;
		Delegate eventHandlerDelegate = null;


		try
		{
			eventHandlerDelegate = Delegate.CreateDelegate(eventHandlerType, handlerMethodContainerInstance, handlerMethodInfo);
			eventInfo.AddEventHandler(eventContainerClassInstance, eventHandlerDelegate);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}

		return eventHandlerDelegate;

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the value of an attribute internal field given the PropertyDescriptor,
	/// the Attribute type and the internal name of the attribute value field.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static object GetAttributeValue(PropertyDescriptor descriptor, Type attributeType, string attributeValueFieldName)
	{
		return GetAttributeValue(descriptor, attributeType, attributeValueFieldName,
			BindingFlags.NonPublic | BindingFlags.Instance);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets the value of an attribute internal field given the PropertyDescriptor,
	/// the Attribute type, the internal name of the attribute value field and the
	/// accessor binding flags of the attribute field.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static object GetAttributeValue(PropertyDescriptor descriptor, Type attributeType,
		string attributeValueFieldName, BindingFlags bindingFlags)
	{
		Attribute attribute = descriptor.Attributes[attributeType];

		if (attribute == null)
			return null;

		return GetFieldValue(attribute, attributeValueFieldName, bindingFlags);

	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Get the class object field given the containing class object, the field name
	/// and access modifier binding flags.
	/// </summary>
	/// <returns>
	/// Returns the field object else logs a diagnostics exception and returns null on error
	/// </returns>
	// ---------------------------------------------------------------------------------
	public static object GetField(object containerClassInstance, string fieldName, BindingFlags bindingFlags)
	{
		Type typeClassInstance = containerClassInstance.GetType();

		FieldInfo fieldInfo = GetFieldInfo(containerClassInstance, fieldName, bindingFlags);

		if (fieldInfo == null)
			return null;

		object fieldObject = fieldInfo.GetValue(containerClassInstance);

		if (fieldObject == null)
		{
			COMException ex = new($"Field '{fieldName}' in container class '{typeClassInstance}' is uninitialized and returned null.");
			Diag.Dug(ex);
			return null;
		}

		return fieldObject;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Get a class object field's FieldInfo given the containing class object,
	/// the field name and access modifier binding flags.
	/// </summary>
	/// <returns>
	/// Returns the field's FieldInfo object else logs a diagnostics exception and
	/// returns null on error
	/// </returns>
	// ---------------------------------------------------------------------------------
	public static FieldInfo GetFieldInfo(object containerClassInstance, string fieldName, BindingFlags bindingFlags)
	{
		Type typeContainerClassInstance = containerClassInstance.GetType();

		FieldInfo fieldInfo = typeContainerClassInstance.GetField(fieldName, bindingFlags);

		if (fieldInfo == null)
		{
			COMException ex = new($"Could not get FieldInfo for field '{fieldName}' in container class '{typeContainerClassInstance}'.");
			Diag.Dug(ex);
			return null;
		}

		return fieldInfo;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Get a static class field's FieldInfo given the containing class type,
	/// the field name and access modifier binding flags.
	/// </summary>
	/// <returns>
	/// Returns the field's FieldInfo object else logs a diagnostics exception and
	/// returns null on error
	/// </returns>
	// ---------------------------------------------------------------------------------
	public static FieldInfo GetFieldInfo(Type typeContainerClass, string fieldName, BindingFlags bindingFlags)
	{
		FieldInfo fieldInfo = typeContainerClass.GetField(fieldName, bindingFlags);

		if (fieldInfo == null)
		{
			COMException ex = new($"Could not get FieldInfo for static field '{fieldName}' in container class '{typeContainerClass}'.");
			Diag.Dug(ex);
			return null;
		}

		return fieldInfo;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Get a class object base's field FieldInfo given the containing class object,
	/// the field name and access modifier binding flags.
	/// </summary>
	/// <returns>
	/// Returns the field's FieldInfo object else logs a diagnostics exception and
	/// returns null on error
	/// </returns>
	// ---------------------------------------------------------------------------------
	public static FieldInfo GetFieldInfoBase(object containerClassInstance, string fieldName, BindingFlags bindingFlags)
	{
		Type typeContainerClassInstance = containerClassInstance.GetType().BaseType;

		FieldInfo fieldInfo = typeContainerClassInstance.GetField(fieldName, bindingFlags);

		if (fieldInfo == null)
		{
			COMException ex = new($"Could not get FieldInfo for field '{fieldName}' in container class '{typeContainerClassInstance}'.");
			Diag.Dug(ex);
			return null;
		}

		return fieldInfo;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets a class object field's value given the containing class object and the
	/// FieldInfo object.
	/// </summary>
	/// <returns>
	/// Returns the field's value 's else logs a diagnostics exception and returns null
	/// on error.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public static object GetFieldInfoValue(object containerClassInstance, FieldInfo fieldInfo)
	{
		object value;

		try
		{
			value = fieldInfo.GetValue(containerClassInstance);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex, $"Could not get Field Value for field '{fieldInfo.Name}' in container class '{containerClassInstance.GetType()}'.");
			return null;
		}

		return value;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Get a class object field's value given the containing class object, the field
	/// name and access modifier binding flags.
	/// </summary>
	/// <returns>
	/// Returns the field's value else logs a diagnostics exception and returns null
	/// on error
	/// </returns>
	// ---------------------------------------------------------------------------------
	public static object GetFieldValue(object containerClassInstance, string fieldName,
		BindingFlags bindingFlags)
	{
		FieldInfo fieldInfo = GetFieldInfo(containerClassInstance, fieldName, bindingFlags);

		if (fieldInfo == null)
			return null;

		return GetFieldInfoValue(containerClassInstance, fieldInfo);
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Get a class object base's field value given the containing class object, the field
	/// name and access modifier binding flags.
	/// </summary>
	/// <returns>
	/// Returns the field's value else logs a diagnostics exception and returns null
	/// on error
	/// </returns>
	// ---------------------------------------------------------------------------------
	public static object GetFieldValueBase(object containerClassInstance, string fieldName,
		BindingFlags bindingFlags)
	{
		FieldInfo fieldInfo = GetFieldInfoBase(containerClassInstance, fieldName, bindingFlags);

		if (fieldInfo == null)
			return null;

		return GetFieldInfoValue(containerClassInstance, fieldInfo);
	}

	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Get a class object property's PropertyInfo given the containing class object,
	/// the property name and access modifier binding flags.
	/// </summary>
	/// <returns>
	/// Returns the property's PropertyInfo object else logs a diagnostics exception and
	/// returns null on error
	/// </returns>
	// ---------------------------------------------------------------------------------
	public static PropertyInfo GetPropertyInfo(object containerClassInstance,
		string propertyName, BindingFlags bindingFlags)
	{
		Type typeContainerClassInstance = containerClassInstance.GetType();

		return GetPropertyInfo(typeContainerClassInstance, propertyName, bindingFlags);
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Get a class object property's PropertyInfo given the containing class Type,
	/// the property name and access modifier binding flags.
	/// </summary>
	/// <returns>
	/// Returns the property's PropertyInfo object else logs a diagnostics exception and
	/// returns null on error
	/// </returns>
	// ---------------------------------------------------------------------------------
	public static PropertyInfo GetPropertyInfo(Type typeContainerClass,
		string propertyName, BindingFlags bindingFlags)
	{
		PropertyInfo propertyInfo = typeContainerClass.GetProperty(propertyName, bindingFlags);

		if (propertyInfo == null)
		{
			COMException ex = new($"Could not get PropertyInfo for property '{propertyName}' in container class '{typeContainerClass}'.");
			Diag.Dug(ex);
			return null;
		}

		return propertyInfo;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Gets a class object property's value given the containing class object and the
	/// PropertyInfo object.
	/// </summary>
	/// <returns>
	/// Returns the property's value else logs a diagnostics exception and returns null
	/// on error.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public static object GetPropertyInfoValue(object containerClassInstance, PropertyInfo propertyInfo)
	{
		object value;

		try
		{
			value = propertyInfo.GetValue(containerClassInstance);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex, $"Could not get Property Value for property '{propertyInfo.Name}' in container class '{containerClassInstance.GetType()}'.");
			return null;
		}

		return value;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Get a class object property's value given the containing class object, the
	/// property name and access modifier binding flags.
	/// </summary>
	/// <returns>
	/// Returns the property's value else logs a diagnostics exception and returns null
	/// on error
	/// </returns>
	// ---------------------------------------------------------------------------------
	public static object GetPropertyValue(object containerClassInstance, string propertyName,
		BindingFlags bindingFlags)
	{
		PropertyInfo propertyInfo = GetPropertyInfo(containerClassInstance, propertyName, bindingFlags);

		if (propertyInfo == null)
			return null;

		return GetPropertyInfoValue(containerClassInstance, propertyInfo);
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Invokes a restricted ambiguous method or method containing parameter modifiers
	/// synchronously.
	/// </summary>
	/// <param name="containerClassInstance">
	/// The object instance containing the method.
	/// </param>
	/// <param name="method">The method name.</param>
	/// <param name="bindingFlags">The access modifier binding flags.</param>
	/// <param name="args">The parameters array else defaults to null.</param>
	/// <param name="argTypes">
	/// The argument types else null if the types must must be derived from the
	/// arguments/parameters. If argTypes is null all parameters must be non-null
	/// otherwise the parameter type cannot be established.
	/// </param>
	/// <param name="argModifiers">
	/// The argument modifiers else defaults to null if no modifiers exist.
	/// </param>
	/// <returns>
	/// True if method completed successfully else logs a diagnostics exception and
	/// returns false on error.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public static bool InvokeAmbiguousMethod(object containerClassInstance, string method,
		BindingFlags bindingFlags, object[] args = null, Type[] argTypes = null,
		ParameterModifier[] argModifiers = null)
	{
		// Tracer.Trace(GetType(), "InvokeMethod()", "Method: {0}", method);

		try
		{
			args ??= [];

			if (argTypes == null)
			{
				argTypes = new Type[args.Length];

				for (int i = 0; i < argTypes.Length; i++)
					argTypes[i] = args[i].GetType();
			}

			argModifiers ??= [];

			Type typeClassInstance = containerClassInstance.GetType();
			MethodInfo methodInfo = typeClassInstance.GetMethod(method, bindingFlags, null, argTypes, argModifiers);


			if (methodInfo == null)
			{
				COMException ex = new($"Could not find ambiguous method info for '{method}()' in container class '{containerClassInstance.GetType()}'. Aborting.");
				Diag.Dug(ex);
				return false;
			}

			methodInfo.Invoke(containerClassInstance, args);

		}
		catch (Exception ex)
		{
			Diag.Dug(ex, $"Could not invoke ambiguous method for '{method}()' in container class '{containerClassInstance.GetType()}'.");
			return false;
		}

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Invokes a restricted method synchronously. For ambiguous methods or methods
	/// containing parameter modifiers use <see cref="InvokeAmbiguousMethod"/>.
	/// </summary>
	/// <returns>
	/// True if method completed successfully else logs a diagnostics exception and
	/// returns false on error.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public static bool InvokeMethod(object containerClassInstance, string method,
		BindingFlags bindingFlags, object[] args = null)
	{
		// Tracer.Trace(GetType(), "InvokeMethod()", "Method: {0}", method);

		try 
		{
			args ??= [];

			Type typeClassInstance = containerClassInstance.GetType();
			MethodInfo methodInfo = typeClassInstance.GetMethod(method, bindingFlags);


			if (methodInfo == null)
			{
				COMException ex = new($"Could not find method info for '{method}()' in container class '{containerClassInstance.GetType()}'. Aborting.");
				Diag.Dug(ex);
				return false;
			}

			methodInfo.Invoke(containerClassInstance, args);

		}
		catch (Exception ex)
		{
			Diag.Dug(ex, $"Could not invoke method for '{method}()' in container class '{containerClassInstance.GetType()}'.");
			return false;
		}

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Removes an event handler delegate from a class object event handler given the
	/// containing class object, the event name, the delegate and access modifier
	/// binding flags.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static void RemoveEventHandler(object containerClassInstance, string eventName,
		Delegate eventHandler, BindingFlags bindingFlags)
	{
		Type typeContainerClassInstance = containerClassInstance.GetType();

		EventInfo eventInfo = typeContainerClassInstance.GetEvent(eventName, bindingFlags);

		if (eventInfo == null)
		{
			COMException ex = new($"Could not get EventInfo for removing a delegate from event '{eventName}' in container class '{typeContainerClassInstance}'.");
			Diag.Dug(ex);
			return;
		}


		try
		{
			eventInfo.RemoveEventHandler(containerClassInstance, eventHandler);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
		}

	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Sets a class object field's value given the containing class object, the
	/// FieldInfo object and the value.
	/// </summary>
	/// <returns>
	/// Returns true if successful else Logs a diagnostics exception on error and
	/// returns false.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public static bool SetFieldInfoValue(object containerClassInstance, FieldInfo fieldInfo, object value)
	{
		try
		{
			fieldInfo.SetValue(containerClassInstance, value);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex, $"Could not set Field Value for field '{fieldInfo.Name}' in container class '{containerClassInstance.GetType()}'.");
			return false;
		}

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Sets a class field's value given the containing class instance, the field name,
	/// access modifier binding flags and the value.
	/// </summary>
	/// <returns>
	/// Returns true if successful else Logs a diagnostics exception on error and
	/// returns false.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public static bool SetFieldValue(object containerClassInstance, string fieldName,
		BindingFlags bindingFlags, object value)
	{
		FieldInfo fieldInfo = GetFieldInfo(containerClassInstance, fieldName, bindingFlags);

		if (fieldInfo == null)
			return false;

		try
		{
			fieldInfo.SetValue(containerClassInstance, value);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex, $"Could not set Field Value for static field '{fieldInfo.Name}' in container class '{containerClassInstance.GetType()}'.");
			return false;
		}

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Sets a static class field's value given the containing class type, the
	/// field name, access modifier binding flags and the value.
	/// </summary>
	/// <returns>
	/// Returns true if successful else Logs a diagnostics exception on error and
	/// returns false.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public static bool SetFieldValue(Type typeContainerClass, string fieldName,
		BindingFlags bindingFlags, object value)
	{
		FieldInfo fieldInfo = GetFieldInfo(typeContainerClass, fieldName, bindingFlags);

		if (fieldInfo == null)
			return false;

		try
		{
			fieldInfo.SetValue(null, value);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex, $"Could not set Field Value for static field '{fieldInfo.Name}' in container class '{typeContainerClass}'.");
			return false;
		}

		return true;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Sets a class base field's value given the containing class instance, the field name,
	/// access modifier binding flags and the value.
	/// </summary>
	/// <returns>
	/// Returns true if successful else Logs a diagnostics exception on error and
	/// returns false.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public static bool SetFieldValueBase(object containerClassInstance, string fieldName,
		BindingFlags bindingFlags, object value)
	{
		FieldInfo fieldInfo = GetFieldInfoBase(containerClassInstance, fieldName, bindingFlags);

		if (fieldInfo == null)
			return false;

		try
		{
			fieldInfo.SetValue(containerClassInstance, value);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex, $"Could not set Field Value for static field '{fieldInfo.Name}' in container class '{containerClassInstance.GetType()}'.");
			return false;
		}

		return true;
	}

}

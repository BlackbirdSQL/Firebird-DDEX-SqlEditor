using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;



namespace BlackbirdSql;

[SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "<Pending>")]


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
		// Tracer.Trace(typeof(Reflect), "CreateInstance<T>()", "Instance Type: {0}.", typeof(T).FullName);

		var type = typeof(T);
		var instance = type.Assembly.CreateInstance(type.FullName, false,
			BindingFlags.Instance | BindingFlags.NonPublic, null, args, null, null);

		return (T)instance;
	}

	public static object CreateInstance(string containerClassName, params object[] args)
	{
		Type typeContainerClass;

		try
		{
			typeContainerClass = Type.GetType(containerClassName);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return null;
		}

		if (typeContainerClass == null)
		{
			TypeLoadException ex = new($"Could not get type: {containerClassName}. Aborting.");
			Diag.Dug(ex);
			return null;
		}

		object instance;

		try
		{ 
			instance = typeContainerClass.Assembly.CreateInstance(typeContainerClass.FullName, false,
				BindingFlags.Instance | BindingFlags.NonPublic, null, args, null, null);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			return null;
		}

		if (instance == null)
		{
			NotSupportedException ex = new($"Could not create instance of type: {typeContainerClass.AssemblyQualifiedName}. Aborting.");
			Diag.Dug(ex);
			return null;
		}


		return instance;
	}





	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Creates and adds an event handler delegate to a class object event handler given
	/// the containing delegate method class object, the name of the delegated method,
	/// the method access modifier binding flags, the event containing class object, the
	/// event name and the event access modifier binding flags.
	/// The handler must be a private or protected instance method.
	/// </summary>
	/// <returns>
	/// The created method delegate which can be stored and used in future calls to
	/// <see cref="RemoveEventHandler"/>.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public static Delegate AddEventHandler(object handlerMethodContainerInstance, string handlerMethodName,
		object eventContainerClassInstance, string eventName, BindingFlags eventBindingFlags = BindingFlags.Default)
	{
		// Tracer.Trace(typeof(Reflect), "AddEventHandler()", "Handler name: {0}, event name: {1}.", handlerMethodName, eventName);

		if (eventBindingFlags == BindingFlags.Default)
			eventBindingFlags = BindingFlags.Instance | BindingFlags.Public;

		MethodInfo handlerMethodInfo = handlerMethodContainerInstance.GetType().GetMethod(handlerMethodName,
			BindingFlags.Instance | BindingFlags.NonPublic);

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
	/// Gets the value of an attribute public field given the PropertyDescriptor,
	/// the Attribute type, the public name of the attribute value field and the
	/// accessor binding flags of the attribute field.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static object GetAttributeValue(PropertyDescriptor descriptor, Type attributeType,
		string attributeValueFieldName, BindingFlags bindingFlags = BindingFlags.Default)
	{
		// Tracer.Trace(typeof(Reflect), "GetAttributeValue()", "Attribute type: {0}, attribute field: {1}.", attributeType.FullName, attributeValueFieldName);

		if (bindingFlags == BindingFlags.Default)
			bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;

		Attribute attribute = descriptor.Attributes[attributeType];

		if (attribute == null)
			return null;

		return GetFieldValueImpl(attribute, attributeValueFieldName, bindingFlags);

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
	public static object GetField(object containerClassInstance, string fieldName, BindingFlags bindingFlags = BindingFlags.Default)
	{
		// Tracer.Trace(typeof(Reflect), "GetField()", "Container class: {0}, field: {1}.", containerClassInstance.GetType().FullName, fieldName);

		if (bindingFlags == BindingFlags.Default)
			bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;

		Type typeClassInstance = containerClassInstance.GetType();

		FieldInfo fieldInfo = GetFieldInfoImpl(containerClassInstance, fieldName, bindingFlags);

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
	public static FieldInfo GetFieldInfo(object containerClassInstance, string fieldName, BindingFlags bindingFlags = BindingFlags.Default)
	{
		// Tracer.Trace(typeof(Reflect), "GetFieldInfo(object)", "Container class: {0}, field: {1}.", containerClassInstance.GetType().FullName, fieldName);

		return GetFieldInfoImpl(containerClassInstance, fieldName, bindingFlags);

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
	private static FieldInfo GetFieldInfo(Type typeContainerClass, string fieldName, BindingFlags bindingFlags = BindingFlags.Default)
	{
		if (bindingFlags == BindingFlags.Default)
			bindingFlags = BindingFlags.Static | BindingFlags.NonPublic;


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
	/// Get a class object field's FieldInfo given the containing class object,
	/// the field name and access modifier binding flags.
	/// </summary>
	/// <returns>
	/// Returns the field's FieldInfo object else logs a diagnostics exception and
	/// returns null on error
	/// </returns>
	// ---------------------------------------------------------------------------------
	private static FieldInfo GetFieldInfoImpl(object containerClassInstance, string fieldName, BindingFlags bindingFlags = BindingFlags.Default)
	{

		if (bindingFlags == BindingFlags.Default)
			bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;

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
	/// Get a class object base's field FieldInfo given the containing class object,
	/// the field name and access modifier binding flags.
	/// </summary>
	/// <returns>
	/// Returns the field's FieldInfo object else logs a diagnostics exception and
	/// returns null on error
	/// </returns>
	// ---------------------------------------------------------------------------------
	private static FieldInfo GetFieldInfoBase(object containerClassInstance, string fieldName, BindingFlags bindingFlags = BindingFlags.Default)
	{
		if (bindingFlags == BindingFlags.Default)
			bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;

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
		// Tracer.Trace(typeof(Reflect), "GetFieldInfoValue()", "Container class: {0}, fieldinfo name: {1}.", containerClassInstance.GetType().FullName, fieldInfo.Name);

		return GetFieldInfoValueImpl(containerClassInstance, fieldInfo);
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
	private static object GetFieldInfoValueImpl(object containerClassInstance, FieldInfo fieldInfo)
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
		BindingFlags bindingFlags = BindingFlags.Default)
	{
		// Tracer.Trace(typeof(Reflect), "GetFieldValue()", "Container class: {0}, field: {1}.", containerClassInstance.GetType().FullName, fieldName);

		return GetFieldValueImpl(containerClassInstance, fieldName, bindingFlags);
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
	private static object GetFieldValueImpl(object containerClassInstance, string fieldName,
		BindingFlags bindingFlags = BindingFlags.Default)
	{
		if (bindingFlags == BindingFlags.Default)
			bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;


		FieldInfo fieldInfo = GetFieldInfoImpl(containerClassInstance, fieldName, bindingFlags);

		if (fieldInfo == null)
			return null;

		return GetFieldInfoValueImpl(containerClassInstance, fieldInfo);
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
		BindingFlags bindingFlags = BindingFlags.Default)
	{
		// Tracer.Trace(typeof(Reflect), "GetFieldValueBase()", "Container class: {0}, field: {1}.", containerClassInstance.GetType().FullName, fieldName);

		if (bindingFlags == BindingFlags.Default)
			bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;

		FieldInfo fieldInfo = GetFieldInfoBase(containerClassInstance, fieldName, bindingFlags);

		if (fieldInfo == null)
			return null;

		return GetFieldInfoValueImpl(containerClassInstance, fieldInfo);
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
	private static PropertyInfo GetPropertyInfo(object containerClassInstance,
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
	private static PropertyInfo GetPropertyInfo(Type typeContainerClass,
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
	private static object GetPropertyInfoValue(object containerClassInstance, PropertyInfo propertyInfo)
	{
		// Tracer.Trace(typeof(Reflect), "GetPropertyInfoValue()");

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
		BindingFlags bindingFlags = BindingFlags.Default)
	{
		// Tracer.Trace(typeof(Reflect), "GetPropertyValue(object)", "Container class: {0}, property: {1}.", containerClassInstance.GetType().FullName, propertyName);

		if (bindingFlags == BindingFlags.Default)
			bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;

		PropertyInfo propertyInfo = GetPropertyInfo(containerClassInstance, propertyName, bindingFlags);

		if (propertyInfo == null)
			return null;

		return GetPropertyInfoValue(containerClassInstance, propertyInfo);
	}


	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Get a class static property's value given the containing class fqn, the
	/// property name and access modifier binding flags.
	/// </summary>
	/// <returns>
	/// Returns the property's value else logs a diagnostics exception and returns null
	/// on error
	/// </returns>
	// ---------------------------------------------------------------------------------
	public static object GetPropertyValue(string containerClassName, string propertyName,
		BindingFlags bindingFlags = BindingFlags.Default)
	{
		// Tracer.Trace(typeof(Reflect), "GetPropertyValue(Type)", "Container class: {0}, property: {1}.", containerClassName, propertyName);

		if (bindingFlags == BindingFlags.Default)
			bindingFlags = BindingFlags.Static | BindingFlags.NonPublic;

		Type typeContainerClass = Type.GetType(containerClassName);
		PropertyInfo propertyInfo = typeContainerClass.GetProperty(propertyName, bindingFlags);

		if (propertyInfo == null)
			return null;

		return propertyInfo.GetValue(null);
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
	///  An object containing the return value of the invoked method, or null on error
	///  or void.
	/// </returns>
	// ---------------------------------------------------------------------------------
	private static object InvokeAmbiguousMethod(object containerClassInstance, string method,
		BindingFlags bindingFlags, object[] args = null, Type[] argTypes = null,
		ParameterModifier[] argModifiers = null)
	{
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
				return null;
			}

			return methodInfo.Invoke(containerClassInstance, args);

		}
		catch (Exception ex)
		{
			Diag.Dug(ex, $"Could not invoke ambiguous method for '{method}()' in container class '{containerClassInstance.GetType()}'.");
		}

		return null;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Invokes a restricted ambiguous method or method containing parameter modifiers
	/// for a BaseType given the base type depth.
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
	///  An object containing the return value of the invoked method, or null on error
	///  or void.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public static object InvokeAmbiguousMethodBaseType(object containerClassInstance, string method,
		int depth, BindingFlags bindingFlags, object[] args = null, Type[] argTypes = null,
		ParameterModifier[] argModifiers = null)
	{
		// Tracer.Trace(typeof(Reflect), "InvokeAmbiguousMethodBaseType()", "Container class: {0}, method: {1}.", containerClassInstance.GetType().FullName, method);

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

			for (int i = 0; i < depth; i++)
				typeClassInstance = typeClassInstance.BaseType;

			MethodInfo methodInfo = typeClassInstance.GetMethod(method, bindingFlags, null, argTypes, argModifiers);


			if (methodInfo == null)
			{
				COMException ex = new($"Could not find ambiguous method info for '{method}()' in container class '{containerClassInstance.GetType()}'. Aborting.");
				Diag.Dug(ex);
				return null;
			}

			return methodInfo.Invoke(containerClassInstance, args);

		}
		catch (Exception ex)
		{
			Diag.Dug(ex, $"Could not invoke ambiguous method for '{method}()' in container class '{containerClassInstance.GetType()}'.");
		}

		return null;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Invokes a restricted generic method synchronously. For ambiguous methods or
	/// methods containing parameter modifiers use <see cref="InvokeAmbiguousMethod"/>.
	/// </summary>
	/// <returns>
	///  An object containing the return value of the invoked method, or null on error
	///  or void.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public static object InvokeGenericMethod<T>(object containerClassInstance, string method,
		BindingFlags bindingFlags = BindingFlags.Default, object[] args = null)
	{
		// Tracer.Trace(typeof(Reflect), "InvokeGenericMethod<T>()", "Container class: {0}, method: {1}, return type: {2}.",
		//	containerClassInstance.GetType().FullName, method, typeof(T).FullName);

		if (bindingFlags == BindingFlags.Default)
			bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
		Type typeClassInstance = containerClassInstance.GetType();

		args ??= [];



		MethodInfo methodInfo;
		try
		{
			methodInfo = typeClassInstance.GetMethod(method, bindingFlags);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex, $"GetMethod failure for '{method}<{typeof(T)}>()' in container class '{containerClassInstance.GetType()}'.");
			return null;
		}

		if (methodInfo == null)
		{
			COMException ex = new($"Could not find method info for '{method}<{typeof(T)}>()' in container class '{containerClassInstance.GetType()}'. Aborting.");
			Diag.Dug(ex);
			return null;
		}


		MethodInfo genericMethodInfo;
		try
		{ 
			genericMethodInfo = methodInfo.MakeGenericMethod(typeof(T));

		}
		catch (Exception ex)
		{
			Diag.Dug(ex, $"MakeGenericMethod failure for '{method}<{typeof(T)}>()' in container class '{containerClassInstance.GetType()}'.");
			return null;
		}


		if (genericMethodInfo == null)
		{
			COMException ex = new($"Could not find generic method info for '{method}<{typeof(T)}>()' in container class '{containerClassInstance.GetType()}'. Aborting.");
			Diag.Dug(ex);
			return null;
		}


		try
		{ 
			return genericMethodInfo.Invoke(containerClassInstance, args);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex, $"Could not invoke method for '{method}()' in container class '{containerClassInstance.GetType()}'.");
		}

		return null;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Invokes a restricted method synchronously. For ambiguous methods or methods
	/// containing parameter modifiers use <see cref="InvokeAmbiguousMethod"/>.
	/// </summary>
	/// <returns>
	///  An object containing the return value of the invoked method, or null on error
	///  or void.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public static object InvokeMethod(object containerClassInstance, string method,
		BindingFlags bindingFlags = BindingFlags.Default, object[] args = null)
	{
		// Tracer.Trace(typeof(Reflect), "InvokeMethod(object)", "Container class: {0}, method: {1}.", containerClassInstance.GetType().FullName, method);

		if (bindingFlags == BindingFlags.Default)
			bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;

		try
		{
			args ??= [];

			Type typeClassInstance = containerClassInstance.GetType();
			MethodInfo methodInfo = typeClassInstance.GetMethod(method, bindingFlags);


			if (methodInfo == null)
			{
				COMException ex = new($"Could not find method info for '{method}()' in container class '{containerClassInstance.GetType()}'. Aborting.");
				Diag.Dug(ex);
				return null;
			}

			return methodInfo.Invoke(containerClassInstance, args);

		}
		catch (Exception ex)
		{
			Diag.Dug(ex, $"Could not invoke method for '{method}()' in container class '{containerClassInstance.GetType()}'.");
		}

		return null;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Invokes a restricted static method synchronously. For ambiguous methods or
	/// methods containing parameter modifiers use <see cref="InvokeAmbiguousMethod"/>.
	/// </summary>
	/// <returns>
	/// True if method completed successfully else logs a diagnostics exception and
	/// returns false on error.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public static object InvokeMethod(Type typeContainerClass, string method,
		BindingFlags bindingFlags = BindingFlags.Default, object[] args = null)
	{
		// Tracer.Trace(typeof(Reflect), "InvokeMethod(Type)", "Container class: {0}, method: {1}.", typeContainerClass.FullName, method);

		if (bindingFlags == BindingFlags.Default)
			bindingFlags = BindingFlags.Static | BindingFlags.NonPublic;

		try
		{
			args ??= [];

			MethodInfo methodInfo = typeContainerClass.GetMethod(method, bindingFlags);


			if (methodInfo == null)
			{
				COMException ex = new($"Could not find method info for '{method}()' in container class '{typeContainerClass}'. Aborting.");
				Diag.Dug(ex);
				return null;
			}

			return methodInfo.Invoke(typeContainerClass, args);

		}
		catch (Exception ex)
		{
			Diag.Dug(ex, $"Could not invoke method for '{method}()' in container class '{typeContainerClass}'.");
		}

		return null;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Invokes a restricted BaseType method synchronously given the depth of the
	/// BaseType.
	/// For ambiguous methods or methods containing parameter modifiers use
	/// <see cref="InvokeAmbiguousMethod"/>.
	/// </summary>
	/// <returns>
	///  An object containing the return value of the invoked method, or null on error
	///  or void.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public static object InvokeMethodBaseType(object containerClassInstance, string method,
		int depth, BindingFlags bindingFlags = BindingFlags.Default, object[] args = null)
	{
		// Tracer.Trace(typeof(Reflect), "InvokeMethodBaseType()", "Container class: {0}, method: {1}.", containerClassInstance.GetType().FullName, method);

		if (bindingFlags == BindingFlags.Default)
			bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;


		try
		{
			args ??= [];

			Type typeClassInstance = containerClassInstance.GetType();

			for (int i = 0; i < depth; i++)
				typeClassInstance = typeClassInstance.BaseType;

			MethodInfo methodInfo = typeClassInstance.GetMethod(method, bindingFlags);


			if (methodInfo == null)
			{
				COMException ex = new($"Could not find method info for '{method}()' in container class '{containerClassInstance.GetType()}'. Aborting.");
				Diag.Dug(ex);
				return null;
			}

			return methodInfo.Invoke(containerClassInstance, args);

		}
		catch (Exception ex)
		{
			Diag.Dug(ex, $"Could not invoke method for '{method}()' in container class '{containerClassInstance.GetType()}'.");
		}

		return null;
	}



	// ---------------------------------------------------------------------------------
	/// <summary>
	/// Removes an event handler delegate from a class object event handler given the
	/// containing class object, the event name, the delegate and access modifier
	/// binding flags.
	/// </summary>
	// ---------------------------------------------------------------------------------
	public static void RemoveEventHandler(object containerClassInstance, string eventName,
		Delegate eventHandler, BindingFlags eventBindingFlags = BindingFlags.Default)
	{
		// Tracer.Trace(typeof(Reflect), "RemoveEventHandler()", "Container class: {0}, event: {1}.", containerClassInstance.GetType().FullName, eventName);

		if (eventBindingFlags == BindingFlags.Default)
			eventBindingFlags = BindingFlags.Instance | BindingFlags.Public;

		Type typeContainerClassInstance = containerClassInstance.GetType();

		EventInfo eventInfo = typeContainerClassInstance.GetEvent(eventName, eventBindingFlags);

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
		// Tracer.Trace(typeof(Reflect), "SetFieldInfoValue()", "Container class: {0}, field: {1}.", containerClassInstance.GetType().FullName, fieldInfo.Name);

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
		object value, BindingFlags bindingFlags = BindingFlags.Default)
	{
		// Tracer.Trace(typeof(Reflect), "SetFieldValue(object)", "Container class: {0}, field: {1}.", containerClassInstance.GetType().FullName, fieldName);

		if (bindingFlags == BindingFlags.Default)
			bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;

		FieldInfo fieldInfo = GetFieldInfoImpl(containerClassInstance, fieldName, bindingFlags);

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
		object value, BindingFlags bindingFlags = BindingFlags.Default)
	{
		// Tracer.Trace(typeof(Reflect), "SetFieldValue(Type)", "Container class: {0}, field: {1}.", typeContainerClass.FullName, fieldName);

		if (bindingFlags == BindingFlags.Default)
			bindingFlags = BindingFlags.Static | BindingFlags.NonPublic;

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
		object value, BindingFlags bindingFlags = BindingFlags.Default)
	{
		// Tracer.Trace(typeof(Reflect), "SetFieldValueBase()", "Container class: {0}, field: {1}.", containerClassInstance.GetType().FullName, fieldName);

		if (bindingFlags == BindingFlags.Default)
			bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;

		FieldInfo fieldInfo = GetFieldInfoBase(containerClassInstance, fieldName, bindingFlags);

		if (fieldInfo == null)
			return false;

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
	/// Sets a class property's value given the containing class instance, the property
	/// name, access modifier binding flags and the value.
	/// </summary>
	/// <returns>
	/// Returns true if successful else Logs a diagnostics exception on error and
	/// returns false.
	/// </returns>
	// ---------------------------------------------------------------------------------
	public static bool SetPropertyValue(object containerClassInstance, string propertyName,
		object value, BindingFlags bindingFlags = BindingFlags.Default)
	{
		// Tracer.Trace(typeof(Reflect), "SetPropertyValue()", "Container class: {0}, property: {1}.", containerClassInstance.GetType().FullName, propertyName);

		if (bindingFlags == BindingFlags.Default)
			bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;

		PropertyInfo propInfo = GetPropertyInfo(containerClassInstance, propertyName, bindingFlags);

		if (propInfo == null)
			return false;

		try
		{
			propInfo.SetValue(containerClassInstance, value);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex, $"Could not set Property Value for property '{propInfo.Name}' in container class '{containerClassInstance.GetType()}'.");
			return false;
		}

		return true;
	}

}

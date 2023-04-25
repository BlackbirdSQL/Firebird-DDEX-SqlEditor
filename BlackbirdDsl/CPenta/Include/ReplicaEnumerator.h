#pragma once
#include "pch.h"
#include "IReplicant.h"



using namespace System::Collections::Generic;


namespace C5 {




// Forward declaration
// template<typename T> public ref class AbstruseReplicant;



template<typename Tekey, typename T> public ref struct ReplicaEnumerator :
	public IEnumerator<KeyValuePair<Tekey, T>>, public IEnumerator<T>
{

public:

	int _CurrentIndex;
	// AbstruseReplicant<T>^ _Instance;
	IReplicant<T>^ _Instance;
	IDictionary<SysStr^, T>^ _DictInstance;
	IList<T>^ _ListInstance;

	bool _UseReplicaKey = false;

public:
//	ReplicaEnumerator(IReplicant<T>^ item, bool useReplicaKey)
	ReplicaEnumerator(Object^ item, bool useReplicaKey)
	{
		_Instance = (IReplicant<T>^)item;
		_DictInstance = (IDictionary<SysStr^, T>^)item;
		_ListInstance = (IList<T>^)item;
		_CurrentIndex = -1;
		_UseReplicaKey = useReplicaKey;
	};

	~ReplicaEnumerator()
	{
	};

	virtual bool MoveNext() // = IEnumerator<KeyValuePair<Tekey, T>>::MoveNext
	{
		// This shouldn't matter.. it's one and the same
		int count = _Instance->IsDictionary ? _DictInstance->Count : _ListInstance->Count;

		if (_CurrentIndex < count - 1)
		{
			_CurrentIndex++;
			return true;
		}

		return false;
	};


	property KeyValuePair<Tekey, T> Current
	{
		virtual KeyValuePair<Tekey, T> get() = IEnumerator<KeyValuePair<Tekey, T>>::Current::get
		{
			// This shouldn't matter.. it's one and the same
			int count = _Instance->IsDictionary ? _DictInstance->Count : _ListInstance->Count;

			if (_CurrentIndex < 0 || _CurrentIndex >= count)
				return KeyValuePair<Tekey, T>();

			SysStr^ segment = _Instance->Key[_CurrentIndex];
			T value;

			if (_Instance->IsDictionary)
				value = _DictInstance[segment];
			else
				value = _ListInstance[_CurrentIndex];

			Tekey key;

			if (_UseReplicaKey)
				key = ReplicaKey(_Instance->IsDictionary, _CurrentIndex, segment);
			else
				key = Tekey(segment);

			return KeyValuePair<Tekey, T>(key, value);
		}
	};

	property T ListCurrent
	{
		virtual T get() = IEnumerator<T>::Current::get
		{
			return _ListInstance[_CurrentIndex];
		}
	};


	// This is required as IEnumerator<T> also implements IEnumerator
	property SysObj^ ArrayCurrent
	{
		virtual SysObj^ get() = System::Collections::IEnumerator::Current::get
		{
			return _ListInstance[_CurrentIndex];
		}
	};


	virtual void Reset() // = IEnumerator<KeyValuePair<SysStr^, T>>::Reset
	{
		_CurrentIndex = -1;
	};


};





}

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class DIContainer
{
    private readonly Dictionary<Type, Binding> _bindings = new();
    private readonly HashSet<Type> _nonLazyTypes = new();
    private readonly Dictionary<Type, ConstructorInfo> _constructorCache = new();
    
    public enum LifeCycle
    {
        Singleton,
        Transient
    }
    
    private class Binding
    {
        public Func<object> Factory { get; set; }
        public LifeCycle LifeCycle { get; set; }
        public bool IsLazy { get; set; }
        public object Instance { get; set; }  
    }
    
    public void Bind<T>(T instance, bool nonLazy = false, LifeCycle lifeCycle = LifeCycle.Singleton)
    {
        _bindings[typeof(T)] = new Binding
        {
            Factory = () => instance,
            LifeCycle = lifeCycle,
            IsLazy = !nonLazy 
        };

        if (nonLazy) InitializeImmediately(typeof(T));
    }
    
    public void Bind<T>(Func<T> factory, bool nonLazy = false, LifeCycle lifeCycle = LifeCycle.Singleton)
    {
        _bindings[typeof(T)] = new Binding
        {
            Factory = () => factory(),
            LifeCycle = lifeCycle,
            IsLazy = !nonLazy
        };

        if (nonLazy) InitializeImmediately(typeof(T));
    }
    
    public void Bind<T>(GameObject prefab, bool nonLazy = false, LifeCycle lifeCycle = LifeCycle.Singleton) where T : MonoBehaviour
    {
        _bindings[typeof(T)] = new Binding
        {
            Factory = () =>
            {
                var instance = UnityEngine.Object.Instantiate(prefab).GetComponent<T>();
                if (instance == null) throw new Exception($"Prefab does not contain component of type {typeof(T)}");
                return instance;
            },
            LifeCycle = lifeCycle,
            IsLazy = !nonLazy
        };

        if (nonLazy) InitializeImmediately(typeof(T));
    }
    
    public void Bind<T>(bool nonLazy = false, LifeCycle lifeCycle = LifeCycle.Singleton) where T : class
    {
        _bindings[typeof(T)] = new Binding
        {
            Factory = () => CreateInstance(typeof(T)),
            LifeCycle = lifeCycle,
            IsLazy = !nonLazy
        };

        if (nonLazy) InitializeImmediately(typeof(T));
    }
    
    public T Resolve<T>()
    {
        return (T)Resolve(typeof(T));
    }

    private object Resolve(Type type)
    {
        if (_bindings.TryGetValue(type, out var binding))
        {
            if (binding.IsLazy && binding.Instance == null)
            {
                binding.Instance = binding.Factory();
            }
            else if (binding.LifeCycle == LifeCycle.Singleton && binding.Instance == null)
            {
                binding.Instance = binding.Factory(); 
            }

            return binding.Instance ?? binding.Factory();
        }

        throw new Exception($"Type {type} not found in DI container.");
    }
    
    private object CreateInstance(Type type)
    {
        if (!_constructorCache.TryGetValue(type, out var constructor))
        {
            constructor = type.GetConstructors()[0];  
            _constructorCache[type] = constructor;
        }

        var parameters = constructor.GetParameters();
        var parameterInstances = new object[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            parameterInstances[i] = Resolve(parameters[i].ParameterType);
        }

        return constructor.Invoke(parameterInstances);
    }
    
    private void InitializeImmediately(Type type)
    {
        if (!_nonLazyTypes.Contains(type))
        {
            Resolve(type);
            _nonLazyTypes.Add(type);
        }
    }
}


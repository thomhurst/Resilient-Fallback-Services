﻿namespace TomLonghurst.Services.Resilient.Fallback.Models;

internal class Implementation
{
    public Type Type { get; }
    public object Instance { get; }
    public Func<IServiceProvider,object> Factory { get; }

    protected Implementation(Type type)
    {
        Type = type;
    }
    
    protected Implementation(object instance)
    {
        Instance = instance;
    }

    protected Implementation(Func<IServiceProvider,object> factory)
    {
        Factory = factory;
    }
}

internal class Implementation<T> : Implementation
{
    public Type Type { get; } = typeof(T);

    public Implementation(T instance) : base(instance)
    {
    }

    public Implementation() : base(typeof(T))
    {
    }

    public Implementation(Func<IServiceProvider,object> factory) : base(factory)
    {
    }
}
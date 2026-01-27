using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUpdate <T> where T : IUpdate<T>
{
    void Update();
}


public interface IFixUpdate<T> where T : IFixUpdate<T>
{
    
}


public class UpdateManager<T>  where T : IUpdate<T>,IDisposable
{
    public void Dispose()
    {
        
    }
}

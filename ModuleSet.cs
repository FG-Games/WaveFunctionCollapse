using System;
using UnityEngine;

namespace WaveFunctionCollapse
{
    public abstract class ModuleSet<T> : ScriptableObject
        where T : Module<T>
    {
        public T[] Modules { get => _modules; }
        [SerializeField] private T[] _modules;        
    }
}
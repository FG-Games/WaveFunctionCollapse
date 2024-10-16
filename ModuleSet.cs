using System;
using UnityEngine;

namespace WaveFunctionCollapse
{
    public abstract class ModuleSet<T> : ScriptableObject
        where T : Module<T>
    {
        public T[] Modules { get => _modules; }
        public T Module(SuperPosition<T> superPosition) => Modules[superPosition.ModuleIndex];
        public SuperPosition<T> SuperPosition (int i) => new SuperPosition<T>(Modules[i].Orientations, Modules[i].Constraints, i);
        [SerializeField] private T[] _modules;
        

        
    }
}
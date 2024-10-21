using System;
using UnityEngine;
using Unity.Collections;

namespace WaveFunctionCollapse
{
    public abstract class ModuleSet<T> : ScriptableObject
        where T : Module<T>
    {
        public T[] Modules { get => _modules; }        
        [SerializeField] private T[] _modules;

        public T Module(SuperPosition superPosition) => Modules[superPosition.ModuleIndex];
        public SuperPosition SuperPosition (int i) => new SuperPosition(Modules[i].Orientations, i);
    }
}
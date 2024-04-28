using System;
using System.Collections.Generic;
using UnityEngine;

namespace TribesAndTributes.WFC
{
    [Serializable]
    public abstract class Cell<T, A> : MonoBehaviour
        where T : Module<T>
    {
        public A Address;
        public abstract Module<T> Module { get; }
        public abstract byte ModuleOrientation { get; } 
        public abstract CellField<T, A> CellField { get; }


        // --- Cell Field --- //

        public Cell<T, A>[] GetAdjacentCells() => CellField.GetAdjacentCells(Address);


        // --- WFC Events --- //

        public virtual CellSuperPosition<T, A> CreateCSP(WaveFunctionCollapse<T, A> wfc)
        {
            return new CellSuperPosition<T, A>(this, wfc);
        }

        public abstract void OnCollapse(SuperPosition<T> collapsedPosition);
        public abstract void OnDecohere();
    }

    [Serializable]
    public abstract class CellField<T, A>
        where T : Module<T>
    {
        public int Seed;
        public abstract Cell<T, A> GetCell(A address);
        public abstract Cell<T, A>[] GetAdjacentCells(A address);
        public abstract ICSPfield<T, A> CreateCellSuperPositions(WaveFunctionCollapse<T, A> wfc);
    }
}
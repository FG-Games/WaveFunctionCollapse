using System;
using UnityEngine;

namespace WaveFunctionCollapse
{
    [Serializable]
    public abstract class Cell<T, A> : MonoBehaviour
        where T : Module<T>
    {
        public A Address;
        public abstract Module<T> Module { get; }
        public abstract int ModuleOrientation { get; } 
        public abstract CellField<T, A> CellField { get; }


        // --- Cell Field --- //

        public Cell<T, A>[] GetAdjacentCells() => CellField.GetAdjacentCells(Address);


        // --- WFC Events --- //

        public abstract CellSuperPosition<T, A> CreateCSP(CellConstraint superPositions);
        public abstract void OnCollapse(Vector2Int collapsedPosition);
        public abstract void OnDecohere();
    }

    [Serializable]
    public abstract class CellField<T, A>
        where T : Module<T>
    {
        public int Seed;
        public ModuleSet<T> ModuleSet;
        public abstract Cell<T, A> GetCell(A address);
        public abstract Cell<T, A>[] GetAdjacentCells(A address);
        public abstract ICSPfield<T, A> CreateCellSuperPositions(CellFieldCollapse<T, A> wfc);
    }
}
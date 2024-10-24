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
        public abstract ICellField<T, A> CellField { get; }


        // --- Cell Field --- //

        public IAdjacentCell<Cell<T, A>> GetAdjacentCells() => CellField.GetAdjacentCells(Address);


        // --- WFC Events --- //

        public abstract CellSuperPosition<T, A> CreateCSP(CellFieldCollapse<T, A> wfc);
        public abstract void OnCollapse(SuperPosition<T> collapsedPosition);
        public abstract void OnDecohere();
    }


    public interface ICellField<T, A>
        where T : Module<T>
    {
        int Seed { get;}
        Cell<T, A> GetCell(A address);
        IAdjacentCell<Cell<T, A>> GetAdjacentCells(A address);
        ICSPfield<T, A> CreateCellSuperPositions(CellFieldCollapse<T, A> wfc);
    }
}
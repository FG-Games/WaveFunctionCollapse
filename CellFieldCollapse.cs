using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;

namespace WaveFunctionCollapse
{
    public class CellFieldCollapse<T, A>
        where T : Module<T>
    {
        public bool AllCellsCollapsed { get => _entropyHeap.Count == 0; }
        private ICSPfield<T, A> _cspField;
        private Heap<CellSuperPosition<T, A>> _entropyHeap;
        private System.Random _random;

        public CellFieldCollapse(CellField<T, A> cellField)
        {
            _cspField = cellField.CreateCellSuperPositions(this);
            _entropyHeap = new Heap<CellSuperPosition<T, A>>(_cspField.Count);
            _random = new System.Random(cellField.Seed);
        }

        public void UpdateSeed(int seed) => _random = new System.Random(seed);

        public CellSuperPosition<T, A> GetCellSuperPosition(A address) => _cspField.GetCellSuperPosition(address);

        public void Add2EntropyHeap(CellSuperPosition<T, A> csp)
        {
            if(csp.Collapsed)
                Debug.LogError("Can't register collapsed cell as SuperPosition");
            else if(csp.HeapIndex == -1)
                Debug.LogWarning("CellSuperPosition has heap index of -1");

            if(_entropyHeap.Contains(csp))
                _entropyHeap.UpdateItem(csp);
            else
                _entropyHeap.Add(csp);
        }

        public void CollapseNext()
        {
            if(AllCellsCollapsed)
                return;

            CellSuperPosition<T, A> csp = _entropyHeap.RemoveFirst();

            if (csp.Collapsed)
                CollapseNext();
            else
                csp.CollapseRandom(_random);
        }

        public void CollapseAt(A address, int index)
        {
            _cspField.GetCellSuperPosition(address).CollapseToModule(index, _random);
        }
    }
}
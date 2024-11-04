using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

namespace WaveFunctionCollapse
{
    public abstract class ModuleSet<T> : ScriptableObject
        where T : Module<T>
    {
        public T[] Modules { get => _modules; }
        [SerializeField] private T[] _modules;

        public CellConstraintSet[] CellConstraintSets
        {
            get
            {
                CellConstraintSet[] cellConstraintSets = new CellConstraintSet[_modules.Length];

                for (int i = 0; i < _modules.Length; i++)
                    cellConstraintSets[i] = _modules[i].Constraints;

                return cellConstraintSets;
            }
        }

        public List<SuperPosition> SuperPositions // MAKE THIS A CONSTRAINT
        {
            get
            {
                List<SuperPosition> superPositions = new List<SuperPosition>();
                SuperOrientation superOrientation = _modules[0].AllOrientations;

                for (int i = 0; i < _modules.Length; i++)
                    superPositions.Add(new SuperPosition(superOrientation, i));

                return superPositions;
            }
        }
    }
}
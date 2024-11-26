using System;
using System.Collections.Generic;
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

        public CellConstraint SuperPositions
        {
            get
            {
                SuperPosition[] superPositions = new SuperPosition[_modules.Length];
                SuperOrientation superOrientation = _modules[0].AllOrientations;

                for (int i = 0; i < superPositions.Length; i++)
                    superPositions[i] = new SuperPosition(superOrientation, i);

                return new CellConstraint(superPositions);
            }
        }
    }
}
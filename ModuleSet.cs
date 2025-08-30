/// <summary>
/// A ModuleSet is a collection of Modules. It provides access to the complete set of Modules,
/// the corresponding set of all CellConstraintSets, and a MaxEntropyPosition.
/// 
/// The MaxEntropyPosition represents the state where all Modules and their orientations
/// are considered possible, i.e., the superposition with maximum uncertainty or entropy.
/// It is necessary to create initial CellSuperPositions in a CSPfield.
/// </summary>

using UnityEngine;

namespace WaveFunctionCollapse
{
    public abstract class ModuleSet<T> : ScriptableObject
        where T : Module<T>
    {
        public T[] Modules { get => _modules; }
        [SerializeField] private T[] _modules;

        public CellConstraint MaxEntropyPosition
        {
            get
            {
                SuperPosition[] superPositions = new SuperPosition[_modules.Length];
                SuperOrientation superOrientation = _modules[0].AllOrientations;

                for (int i = 0; i < superPositions.Length; i++)
                    superPositions[i] = new SuperPosition(i, superOrientation);

                return new CellConstraint(superPositions);
            }
        }
        
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
    }
}
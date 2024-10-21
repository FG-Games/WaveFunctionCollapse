using System;
using System.Linq;
using UnityEngine;
using Unity.Collections;

namespace WaveFunctionCollapse
{
    [Serializable]
    public struct CellConstraint
    {
        public int Count => _superPositions.Length;
        public int PossiblePositions
        {
            get
            {
                int possiblePositions = 0;

                for (int i = 0; i < Count; i ++)
                    if(_possiblePositions[i])
                        possiblePositions ++;
                    
                return possiblePositions;
            }
        }

        public int Entropy
        {
            get
            {
                SetEntropy();
                return _entopy;
            }
        }

        [SerializeField] private int _entopy;
        [SerializeField] private SuperPosition[] _superPositions;
        [SerializeField] private bool[] _possiblePositions;
        private SuperPosition _booleanSuperposition;

        public CellConstraint(int setCount)
        {
            _entopy = 0;
            _superPositions = new SuperPosition[setCount];
            _possiblePositions = new bool[setCount];
            _booleanSuperposition = new SuperPosition();
        }

        public void EnterSuperPosition(SuperPosition superPosition)
        {
            _superPositions[superPosition.ModuleIndex] = superPosition;
            _possiblePositions[superPosition.ModuleIndex] = true;
        }

        public void SetEntropy()
        {
            _entopy = 0;

            for (int i = 0; i < Count; i ++)
                if(_possiblePositions[i])
                    _entopy += this[i].Orientations.Count;
        }

        public SuperPosition this[int moduleIndex]
        {
            get
            {
                if (moduleIndex < 0 || moduleIndex >= Count)
                    throw new IndexOutOfRangeException($"Index {moduleIndex} is out of range for the count {Count}.");

                if(_possiblePositions[moduleIndex])
                    return _superPositions[moduleIndex];
                else
                    throw new IndexOutOfRangeException($"Index {moduleIndex} is has no possible SuperPosition");
            }
            set
            {
                if (moduleIndex < 0 || moduleIndex >= Count)
                    throw new IndexOutOfRangeException($"Index {moduleIndex} is out of range for the count {Count}.");

                _superPositions[moduleIndex] = value;
            }
        }

        public SuperPosition GetPossiblePosition(int index)
        {
            if (index < 0 || index >= PossiblePositions)
                throw new IndexOutOfRangeException($"Index {index} is out of range for the amount of possible positions {PossiblePositions}.");

            int counter = -1;

            for (int i = 0; i < Count; i ++)
            {
                if(_possiblePositions[i])
                {
                    counter ++;

                    if(index == counter)
                        return _superPositions[i];
                }
            }

            throw new IndexOutOfRangeException($"Index {index} is out of range for the amount of possible positions {PossiblePositions}.");
        }

        public void Dispose()
        {
            Debug.Log("NEVER Disposed SuperPositions");

            /*if (_superPositions.IsCreated)
                _superPositions.Dispose();
            if (_possiblePositions.IsCreated)
                _possiblePositions.Dispose();*/
        }


        // --- Constraints --- //

        public void Union (CellConstraint other)
        {            
            for (int i = 0; i < _superPositions.Length; i ++)
            {
                // Check possible SuperPositions and add them
                if(_possiblePositions[i] || other._possiblePositions[i])
                {
                    if(_superPositions[i].Union(other._superPositions[i]))
                    {
                        _superPositions[i] = _booleanSuperposition;
                        EnterSuperPosition(_superPositions[i]);
                    }
                }
            }
        }

        public void Intersection (CellConstraint other)
        {
            for (int i = 0; i < _superPositions.Length; i ++)
            {
                // Check common possible SuperPositions and intersect them 
                if(_possiblePositions[i] && other._possiblePositions[i])
                {
                    if(!_superPositions[i].Intersection(other._superPositions[i]))
                        _possiblePositions[i] = false;
                }
                else
                {
                    _possiblePositions[i] = false;
                }
            }

            if(PossiblePositions == 0)
                Debug.LogError("No collapse possible");
        }

        private CellConstraint add(CellConstraint addedConstraint) 
        {
            addedConstraint.Union(this);
            return addedConstraint;
        }

        private CellConstraint rotate(int rotation) 
        {
            CellConstraint rotated = this;

            for (int i = 0; i < Count; i ++)
                rotated._superPositions[i].Rotate(rotation);

            return rotated;
        }


        // --- Operators --- //

        public static CellConstraint operator + (CellConstraint a, CellConstraint b) => a.add(b);

        public static CellConstraint operator * (CellConstraint a, int i) => a.rotate(i);

        /*public bool TestForMismatch(string prefix)
        {
            bool result = _count != _possiblePositions.Count(p => p);
            Debug.Assert(!result, prefix + $"Mismatch between _count ({_count}) and actual possible positions({_possiblePositions.Count(p => p)})!");
            return result;
        }*/
    }


    [Serializable]
    public struct CellConstraintSet // A set of constraints: one for each adjacent cell // HEX ONLY
    {
        public const int Length = 6;

        [SerializeField] private CellConstraint _constraint0, _constraint1, _constraint2, _constraint3, _constraint4, _constraint5;

        public CellConstraintSet(CellConstraint[] cellConstraints)
        {
            _constraint0 = cellConstraints[0];
            _constraint1 = cellConstraints[1];
            _constraint2 = cellConstraints[2];
            _constraint3 = cellConstraints[3];
            _constraint4 = cellConstraints[4];
            _constraint5 = cellConstraints[5];
        }

        public CellConstraint this[int index]
        {
            get
            {
                return index switch
                {
                    0 => _constraint0, 
                    1 => _constraint1,
                    2 => _constraint2,
                    3 => _constraint3,
                    4 => _constraint4,
                    5 => _constraint5,
                    _ => throw new IndexOutOfRangeException("Index out of range")
                };
            }
            set
            {
                switch (index)
                {
                    case 0: _constraint0 = value; break;
                    case 1: _constraint1 = value; break;
                    case 2: _constraint2 = value; break;
                    case 3: _constraint3 = value; break;
                    case 4: _constraint4 = value; break;
                    case 5: _constraint5 = value; break;
                    default: throw new IndexOutOfRangeException("Index out of range");
                }
            }
        }

        private CellConstraintSet merge(CellConstraintSet addition)
        {
            CellConstraintSet addedSet = this;

            for(int i = 0; i < Length; i++)
                addedSet[i] += addition[i];

            return addedSet;
        }

        private CellConstraintSet rotate(int rotation)
        {
            CellConstraintSet rotatedSet = this;

            for (int i = 0; i < Length; i ++)
                rotatedSet[i] = this[addRotations(rotation, i)] * rotation;

            return rotatedSet;
        }

        private byte addRotations(int rotationA, int rotationB) => (byte)((rotationA + rotationB) % Length);

        public static CellConstraintSet operator + (CellConstraintSet obj1, CellConstraintSet obj2) => obj1.merge(obj2);

        public static CellConstraintSet operator * (CellConstraintSet obj1, int i) => obj1.rotate(i);
    }

    public static class CreateUnmanaged<T>
        where T : Module<T>
    {
        // What the hell is this? 
        // The constructor of SuperPositions can't be generic, as Module<T> will be
        // a managed type and won't be used in a NativeArray. 

        public static CellConstraint CellConstraint(SuperModule<T>[] superModules, int setCount)
        {
            CellConstraint superPositions = new CellConstraint(setCount);

            for (int i = 0; i < superModules.Length; i ++)
                superPositions.EnterSuperPosition(superModules[i].GetSuperPosition());

            superPositions.SetEntropy();
            return superPositions;
        }

        public static CellConstraint CellConstraint(ModuleSet<T> moduleSet)
        {
            CellConstraint superPositions = new CellConstraint(moduleSet.Modules.Length);

            for (int i = 0; i < moduleSet.Modules.Length; i ++)
                superPositions.EnterSuperPosition(moduleSet.SuperPosition(i));

            superPositions.SetEntropy();
            return superPositions;
        }

        public static CellConstraintSet CellConstraintSet(SuperModuleArray<T>[] superModuleArraySet)
        {
            CellConstraint[] constraints = new CellConstraint[superModuleArraySet.Length];

            for (int i = 0; i < superModuleArraySet.Length; i ++)
                constraints[i] = superModuleArraySet[i].GetSuperPositions();

            return new CellConstraintSet(constraints);
        }
    }
}
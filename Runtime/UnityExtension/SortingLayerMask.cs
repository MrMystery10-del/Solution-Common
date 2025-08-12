using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using UnityEngine;

namespace Solution.Common.UnityExtension {

    /**
    * <summary>
    * Represents a bitmask for Unity's sorting layers.
    * Allows checking which sorting layers are included via ID, name or index.
    * Useful for filtering and organizing rendering logic based on sorting layers.
    * </summary>
    */
    [DebuggerDisplay("Mask = {mask}, Layers = {ToString()}")]
    [Serializable]
    public struct SortingLayerMask : IEquatable<SortingLayerMask> {
    
        #region Fields
        
        [SerializeField] private int mask;
        
        #endregion
        
        #region Static Properties

        /**
        * <summary>
        * An empty sorting layer mask.
        * </summary>
        */
        public static readonly SortingLayerMask None = new(0);
        
        /**
        * <summary>
        * An sorting layer mask including all layers.
        * </summary>
        */
        public static SortingLayerMask All {
            get {
                int all = 0;
                
                for (int index = 0; index < SortingLayer.layers.Length; index++) {
                    all |= 1 << index;
                }
                return new SortingLayerMask(all);
            }
        }
        
        #endregion
        
        #region Public Properties
        
        /**
        * <summary>
        * The raw integer bitmask representing selected sorting layers.
        * </summary>
        */
        public readonly int Mask => mask;
        
        /**
        * <summary>
        * Checks if the mask contains no sorting layers.
        * </summary>
        */
        public readonly bool IsEmpty {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => mask == 0;
        }
        
        /**
        * <summary>
        * Checks whether the mask contains exactly one sorting layer.
        * </summary>
        */
        public readonly bool IsSingleLayer {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => mask != 0 && (mask & (mask - 1)) == 0;
        }

        /**
        * <summary>
        * The number of sorting layers currently included in the mask.
        * </summary>
        */
        public readonly int SortingLayersContained {
            get {
                int count = 0;
                
                for (int index = 0; index < SortingLayer.layers.Length; index++) {
                    if ((mask & (1 << index)) != 0) {
                        count++;
                    }
                }
                return count;
            }
        }
        
        #endregion

        #region Creation
        
        /**
        * <summary>
        * Constructs a new SortingLayerMask using a raw bitmask.
        * </summary>
        * <param name="mask">The bitmask representing the selected sorting layers.</param>
        */
        public SortingLayerMask(int mask) {
            this.mask = mask;
        }
        
        /**
        * <summary>
        * Creates a SortingLayerMask containing only the specified layer by index.
        * </summary>
        */
        public static SortingLayerMask FromIndex(int index) {
            return new SortingLayerMask(1 << index);
        }

        /**
        * <summary>
        * Creates a SortingLayerMask containing only the specified layer by name.
        * </summary>
        */
        public static SortingLayerMask FromName(string layerName) {
            int index = SortingLayerIndex(layerName);
            return index == -1 ? new SortingLayerMask(0) : new SortingLayerMask(1 << index);
        }

        /**
        * <summary>
        * Returns the index of the sorting layer with the specified name.
        * </summary>
        * <param name="name">The name of the sorting layer.</param>
        * <returns>The index if found, or -1 if not found.</returns>
        */
        private static int SortingLayerIndex(string name) {
            var layers = SortingLayer.layers;
            
            for (int index = 0; index < layers.Length; index++) {
                if (layers[index].name == name) {
                    return index;
                }
            }
            return -1;
        }
        
        #endregion
        
        #region Layer Queries
        
        /**
        * <summary>
        * Enumerates the names of all sorting layers included in the mask.
        * </summary>
        * <returns>An enumerable of included sorting layer names.</returns>
        */
        public readonly IEnumerable<string> EnumerateLayerNames() {
            var layers = SortingLayer.layers;

            for (int index = 0; index < layers.Length; index++) {
                if ((mask & (1 << index)) != 0) {
                    yield return layers[index].name;
                }
            }
        }
        
        /**
        * <summary>
        * Checks if the specified sorting layer ID is included in the mask.
        * </summary>
        * <param name="sortingLayerID">The ID of the sorting layer to check.</param>
        * <returns>True if the sorting layer is included, otherwise false.</returns>
        */
        public readonly bool IncludesLayerID(int sortingLayerID) {
            var layers = SortingLayer.layers;
            
            for (int index = 0; index < layers.Length; index++) {
                if (layers[index].id == sortingLayerID) {
                    return (mask & (1 << index)) != 0;
                }
            }
            return false;
        }

        /**
        * <summary>
        * Checks if the sorting layer at the given index is included in the mask.
        * </summary>
        * <param name="layerIndex">The index of the sorting layer.</param>
        * <returns>True if the layer is included, otherwise false.</returns>
        */
        public readonly bool IncludesLayer(int layerIndex) {
            if (layerIndex < 0 || layerIndex >= SortingLayer.layers.Length) {
                return false;
            }
            return (mask & (1 << layerIndex)) != 0;
        }

        /**
        * <summary>
        * Checks if the sorting layer with the given name is included in the mask.
        * </summary>
        * <param name="layerName">The name of the sorting layer.</param>
        * <returns>True if the layer is included, otherwise false.</returns>
        */
        public readonly bool IncludesLayer(string layerName) {
            int index = SortingLayerIndex(layerName);
            return index != -1 && IncludesLayer(index);
        }
        
        #endregion
        
        #region Comparison
        
        /**
        * <summary>
        * Checks if all layers of the given mask are contained in the current mask.
        * </summary>
        */
        public readonly bool ContainsAll(SortingLayerMask other) {
            return (mask & other.mask) == other.mask;
        }

        /**
        * <summary>
        * Determines whether this instance and another SortingLayerMask have the same mask value.
        * </summary>
        * <param name="other">The other SortingLayerMask to compare with.</param>
        * <returns>True if both masks are equal; otherwise, false.</returns>
        */
        public readonly bool Equals(SortingLayerMask other) {
            return mask == other.mask;
        }
        
        /**
        * <summary>
        * Checks if this mask is a superset of the other mask.
        * </summary>
        */
        public readonly bool IsSupersetOf(SortingLayerMask other) {
            return ContainsAll(other) && this != other;
        }

        /**
        * <summary>
        * Checks if this mask is a subset of the other mask.
        * </summary>
        */
        public readonly bool IsSubsetOf(SortingLayerMask other) {
            return other.ContainsAll(this) && this != other;
        }

        /**
        * <summary>
        * Checks if the current mask overlaps with another.
        * </summary>
        */
        public readonly bool Overlaps(SortingLayerMask other) {
            return (mask & other.mask) != 0;
        }
        
        #endregion
        
        #region Mask Modifications
        
        /**
        * <summary>
        * Returns a new mask that contains only layers included in both masks.
        * </summary>
        */
        public readonly SortingLayerMask Intersect(SortingLayerMask other) {
            return new SortingLayerMask(mask & other.mask);
        }

        /**
        * <summary>
        * Returns a new mask that includes the specified layer index.
        * </summary>
        */
        public readonly SortingLayerMask WithAddedLayer(int layerIndex) {
            return new SortingLayerMask(mask | (1 << layerIndex));
        }

        /**
        * <summary>
        * Returns a new mask that excludes the specified layer index.
        * </summary>
        */
        public readonly SortingLayerMask WithoutLayer(int layerIndex) {
            return new SortingLayerMask(mask & ~(1 << layerIndex));
        }
        
        #endregion

        #region ValueType
        
        /**
        * <summary>
        * Compares this instance to a generic object for equality.
        * </summary>
        */
        public override readonly bool Equals(object obj) {
            return obj is SortingLayerMask other && Equals(other);
        }
        
        /**
        * <summary>
        * Generates a hash code based on the internal bitmask.
        * </summary>
        */
        public override readonly int GetHashCode() {
            return mask;
        }
        
        /**
        * <summary>
        * Returns a string representation listing all included sorting layer names.
        * </summary>
        */
        public override readonly string ToString() {
            var layers = SortingLayer.layers;
            var names = new List<string>();

            for (int index = 0; index < layers.Length; index++) {
                if ((mask & (1 << index)) != 0) {
                    names.Add(layers[index].name);
                }
            }

            return string.Join(", ", names);
        }
        
        #endregion
        
        #region Operators
        
        public static bool operator ==(SortingLayerMask left, SortingLayerMask right) => left.Equals(right);
        public static bool operator !=(SortingLayerMask left, SortingLayerMask right) => !left.Equals(right);
        public static SortingLayerMask operator |(SortingLayerMask a, SortingLayerMask b) => new(a.mask | b.mask);
        public static SortingLayerMask operator &(SortingLayerMask a, SortingLayerMask b) => new(a.mask & b.mask);
        public static SortingLayerMask operator ~(SortingLayerMask a) => new(~a.mask);

        public static implicit operator int(SortingLayerMask sortingLayerMask) => sortingLayerMask.mask;
        public static explicit operator SortingLayerMask(int rawMask) => new(rawMask);
        
        #endregion
    }
}
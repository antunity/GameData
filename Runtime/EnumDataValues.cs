using System;
using System.Collections.Generic;

namespace antunity.GameData
{
    /// <summary>An enum entry wrapped in the form of game data.</summary>
    /// <typeparam name="TEnum">the type of the enum</typeparam>
    [Serializable]
    [GameDataDrawer(GameDataLayout.Vertical)]
    public class EnumData<TEnum> : GameData<TEnum> where TEnum : struct
    {
        public EnumData(TEnum index) : base(index) { }
    }

    /// <summary>A serializable dictionary-style list of enum-value-pairs. This structure is Unity inspector-friendly.</summary>
    /// <typeparam name="TEnum">the enum type</typeparam>
    /// <typeparam name="TValue">the value type</typeparam>
    [Serializable]
    public class EnumDataValues<TEnum, TValue> : GameDataValues<EnumData<TEnum>, TValue> where TEnum : struct
    {
        #region ICopyable

        public new EnumDataValues<TEnum, TValue> Copy()
        {
            var copy = new EnumDataValues<TEnum, TValue>();
            copy.dataValuePairs = new List<DataValuePair<EnumData<TEnum>, TValue>>(dataValuePairs);
            copy.EnsureInitialised();
            return copy;
        }

        #endregion ICopyable

        /// <summary>Adds an entry to the list.</summary>
        /// <param name="index">the enum index</param>
        /// <param name="value">the value</param>
        public void Add(TEnum index, TValue value) => base.Add(new(index), value);
    }
}
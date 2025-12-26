using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace antunity.GameData
{
    /// <summary>A struct pairing game data with a value, similar to key-value-pairs in dictionaries. Used as the building block in GameDataValues.</summary>
    /// <typeparam name="TGameData">the game data type</typeparam>
    /// <typeparam name="TValue">the value type</typeparam>
    [Serializable]
    [GameDataDrawer(GameDataLayout.Vertical)]
    public struct DataValuePair<TGameData, TValue> : IUseGameDataDrawer where TGameData : IGameDataBase
    {
        [Tooltip("An indexed data entry associated with a value.")]
        [SerializeField] private TGameData data;

        [Tooltip("A value associated with this data entry.")]
        [SerializeField] private TValue value;

        /// <summary>Use to access the data.</summary>
        public TGameData Data
        {
            get => data;
            set => data = value;
        }

        /// <summary>Use to access the value.</summary>
        public TValue Value
        {
            get => value;
            set => this.value = value;
        }

        public DataValuePair(TGameData data, TValue value = default)
        {
            this.data = data;
            this.value = value;
        }
    }

    /// <summary>A serializable dictionary-style list of data-value-pairs. This structure is Unity inspector-friendly.</summary>
    /// <typeparam name="TGameData">the type of the serialized game data</typeparam>
    /// <typeparam name="TValue">the type of the value associated with the game data</typeparam>
    [Serializable]
    public class GameDataValues<TGameData, TValue> : IEnumerable<DataValuePair<TGameData, TValue>>, ICopyable<GameDataValues<TGameData, TValue>> where TGameData : IGameDataBase
    {
        #region IEnumerable

        public IEnumerator<DataValuePair<TGameData, TValue>> GetEnumerator()
        {
            EnsureInitialised();
            return dataValuePairs.GetEnumerator();
        }
        
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion IEnumerable

        #region ICopyable

        public GameDataValues<TGameData, TValue> Copy()
        {
            var copy = new GameDataValues<TGameData, TValue>();
            copy.dataValuePairs = new List<DataValuePair<TGameData, TValue>>(dataValuePairs);
            copy.EnsureInitialised();
            return copy;
        }

        #endregion ICopyable

        [NonSerialized] private bool isInitialised = false;

        [SerializeField] protected List<DataValuePair<TGameData, TValue>> dataValuePairs = new();

        protected readonly Dictionary<object, int> itemsIndex = new();

        /// <summary>Can be used to get or set values in the list by index.</summary>
        /// <param name="index">the index</param>
        /// <returns>the associated value</returns>
        /// <exception cref="Exception">thrown if the index is of invalid type or when the index is not found in the registry</exception>
        public TValue this[object index]
        {
            get
            {
                if (!ContainsIndex(index))
                    throw new Exception($"Index `{index}` not found in list `{typeof(TGameData)}`");

                if (itemsIndex[index] >= dataValuePairs.Count)
                    throw new Exception($"Index `{index}` is out of range in list `{typeof(TGameData)}`");

                return dataValuePairs[itemsIndex[index]].Value;
            }
            set
            {
                if (!ContainsIndex(index))
                    throw new Exception($"Index `{index}` not found in list `{typeof(TGameData)}`");

                var data = GetData(index);
                AddOrUpdate(data, value);
            }
        }

        /// <summary>Can be used to get or set values in the list.</summary>
        /// <param name="asset">the game data</param>
        /// <returns>the associated value</returns>
        public TValue this[TGameData asset]
        {
            get => this[asset.GetIndex()];
            set => AddOrUpdate(asset, value);
        }

        /// <summary>The number of items in the list.</summary>
        public int Count => dataValuePairs.Count;

        /// <summary>A list of game data in the list.</summary>
        public IReadOnlyList<TGameData> Data => dataValuePairs.ConvertAll(item => item.Data).AsReadOnly();

        /// <summary>A list of the unique keys in the list.</summary>
        public IReadOnlyList<object> Keys => dataValuePairs.ConvertAll(item => item.Data.GetIndex()).AsReadOnly();

        /// <summary>A list of the values in the list.</summary>
        public IReadOnlyList<TValue> Values => dataValuePairs.ConvertAll(item => item.Value).AsReadOnly();

        /// <summary>Adds an entry to the list.</summary>
        /// <param name="item">the game data</param>
        /// <param name="value">the value associated with the game data</param>
        /// <exception cref="Exception">thrown when a duplicate index is found in the list</exception>
        public void Add(TGameData item, TValue value)
        {
            var pair = new DataValuePair<TGameData, TValue>(item, value);
            if (!TryAdd(pair))
                throw new Exception($"Index `{item.GetIndex()}` already exists in list `{typeof(TGameData)}`");
        }

        /// <summary>Clears all entries from the list.</summary>
        public void Clear()
        {
            dataValuePairs.Clear();
            itemsIndex.Clear();
        }

        /// <summary>Checks whether the provided data is found in the list.</summary>
        /// <param name="data">the game data</param>
        /// <returns>true if the data is found</returns>
        public bool ContainsData(TGameData data) => data != null && ContainsIndex(data.GetIndex());

        /// <summary>Checks if a specific index key is found in the list.</summary>
        /// <param name="index">the index</param>
        /// <returns>true if the index is found</returns>
        public bool ContainsIndex(object index)
        {
            EnsureInitialised();

            if (index == null)
                return false;
            
            if (index is TGameData data)
                throw new Exception($"Use {nameof(ContainsData)} to check for data items in list `{typeof(TGameData)}`");

            return itemsIndex.ContainsKey(index);
        }

        /// <summary>Attempts to get game data by the specified index.</summary>
        /// <param name="index">the index</param>
        /// <param name="data">the returned data</param>
        /// <returns>true of the data is found</returns>
        public bool TryGetData(object index, out TGameData data)
        {
            if (ContainsIndex(index))
            {
                data = GetData(index);
                return true;
            }

            data = default;
            return false;
        }

        /// <summary>Attempts to get a value by the specified index.</summary>
        /// <param name="index">the index</param>
        /// <param name="value">the returned value</param>
        /// <returns>true if the index was found</returns>
        public bool TryGetValue(object index, out TValue value)
        {
            if (ContainsIndex(index))
            {
                value = this[index];
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>Attempts to get a value by the specified game data entry.</summary>
        /// <param name="item">the game data</param>
        /// <param name="value">the returned value</param>
        /// <returns>true if the game data was found</returns>
        public bool TryGetValue(TGameData item, out TValue value)
        {
            if (item != null)
                return TryGetValue(item.GetIndex(), out value);

            value = default;
            return false;
        }

        /// <summary>Removes the value entry associated with the specified index.</summary>
        /// <param name="index">the index</param>
        public void Remove(object index)
        {
            if (!ContainsIndex(index))
                return;

            int i = itemsIndex[index];
            dataValuePairs.RemoveAt(i);
            itemsIndex.Remove(index);

            Validate(i);
        }

        /// <summary>Removes the value entry associated with the specified game data.</summary>
        /// <param name="item">the game data</param>
        public void Remove(TGameData item)
        {
            if (item != null)
                Remove(item.GetIndex());
        }

        protected void AddOrUpdate(TGameData data, TValue value)
        {
            var pair = new DataValuePair<TGameData, TValue>(data, value);
            if (!TryAdd(pair))
                dataValuePairs[itemsIndex[data.GetIndex()]] = pair;
        }

        private TGameData GetData(object index)
        {
            if (!ContainsIndex(index))
                throw new Exception($"Index `{index}` not found in list `{typeof(TGameData)}`");

            if (itemsIndex[index] >= dataValuePairs.Count)
                throw new Exception($"Index `{index}` is out of range in list `{typeof(TGameData)}`");

            return dataValuePairs[itemsIndex[index]].Data;
        }

        private bool TryAdd(DataValuePair<TGameData, TValue> pair)
        {
            EnsureInitialised();

            object index = pair.Data.GetIndex();

            if (itemsIndex.ContainsKey(index))
                return false;

            dataValuePairs.Add(pair);
            itemsIndex.Add(index, dataValuePairs.Count - 1);

            return true;
        }

        protected void EnsureInitialised()
        {
            if (isInitialised)
                return;

            isInitialised = true;
            Validate();
        }

        private void Validate(int start = 0)
        {
            if (dataValuePairs.Count == 0)
            {
                itemsIndex.Clear();
                return;
            }

            if (start == 0)
                itemsIndex.Clear();

            for (int i = start; i < dataValuePairs.Count; i++)
            {
                var data = dataValuePairs[i].Data;

                if (data == null)
                    continue;

                object index = dataValuePairs[i].Data.GetIndex();

                if (index == null || index.Equals(default))
                    continue;

                if (start != 0)
                    itemsIndex.Remove(index);

                if (!itemsIndex.ContainsKey(index))
                    itemsIndex.Add(index, i);
                else
                {
                    dataValuePairs[i] = new DataValuePair<TGameData, TValue>();
                    Debug.LogWarning($"Discarded item with duplicate index [{index}] at position {i} in {nameof(GameDataValues<TGameData, TValue>)}");
                }
            }
        }
    }
}
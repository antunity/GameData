using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace uGameDataCORE
{
    [Serializable]
    [GameDataDrawer(GameDataLayout.Vertical)]
    public struct DataValuePair<TGameData, TValue> where TGameData : IGameData
    {
        [Tooltip("An indexed data entry associated with a value.")]
        [SerializeField] private TGameData data;

        [Tooltip("A value associated with this data entry.")]
        [SerializeField] private TValue value;

        public TGameData Data
        {
            get => data;
            set => data = value;
        }

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

    [Serializable]
    public class GameDataValues<TGameData, TValue> : IEnumerable<DataValuePair<TGameData, TValue>>, ICopyable<GameDataValues<TGameData, TValue>> where TGameData : IGameData
    {
        #region IEnumerable

        public IEnumerator<DataValuePair<TGameData, TValue>> GetEnumerator() => dataValuePairs.ToList().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion IEnumerable

        #region ICopyable

        public GameDataValues<TGameData, TValue> Copy()
        {
            var copy = new GameDataValues<TGameData, TValue>();
            copy.dataValuePairs = new List<DataValuePair<TGameData, TValue>>(dataValuePairs);
            copy.Validate();
            return copy;
        }

        public void Validate() => Validate(0);

        #endregion ICopyable

        [SerializeField] protected List<DataValuePair<TGameData, TValue>> dataValuePairs = new();

        protected readonly Dictionary<object, int> itemsIndex = new();

        public TValue this[object index]
        {
            get
            {
                if (!ContainsKey(index))
                    throw new Exception($"Index `{index}` not found in list `{typeof(TGameData)}`");

                if (itemsIndex[index] >= dataValuePairs.Count)
                    throw new Exception($"Index `{index}` is out of range in list `{typeof(TGameData)}`");

                return dataValuePairs[itemsIndex[index]].Value;
            }
            set
            {
                if (!ContainsKey(index))
                    throw new Exception($"Index `{index}` not found in list `{typeof(TGameData)}`");

                var data = GetData(index);
                AddOrUpdate(data, value);
            }
        }

        public TValue this[TGameData asset]
        {
            get => this[asset.Index];
            set => AddOrUpdate(asset, value);
        }

        public int Count => dataValuePairs.Count;

        public IReadOnlyList<TGameData> Data => dataValuePairs.ConvertAll(item => item.Data).AsReadOnly();

        public IReadOnlyList<object> Keys => dataValuePairs.ConvertAll(item => item.Data.Index).AsReadOnly();

        public IReadOnlyList<TValue> Values => dataValuePairs.ConvertAll(item => item.Value).AsReadOnly();

        public void Add(TGameData item, TValue value)
        {
            var pair = new DataValuePair<TGameData, TValue>(item, value);
            if (!TryAdd(pair))
                throw new Exception($"Index `{item.Index}` already exists in list `{typeof(TGameData)}`");
        }

        public void Clear()
        {
            dataValuePairs.Clear();
            itemsIndex.Clear();
        }

        public bool ContainsData(TGameData data) => data != null && ContainsKey(data.Index);

        public bool ContainsKey(object index)
        {
            if (index == null)
                return false;
            
            if (index is TGameData data)
                throw new Exception($"Use {nameof(ContainsData)} to check for data items in list `{typeof(TGameData)}`");

            return itemsIndex.ContainsKey(index);
        }

        public bool TryGetData(object index, out TGameData data)
        {
            if (ContainsKey(index))
            {
                data = GetData(index);
                return true;
            }

            data = default;
            return false;
        }

        public bool TryGetValue(object index, out TValue value)
        {
            if (ContainsKey(index))
            {
                value = this[index];
                return true;
            }

            value = default;
            return false;
        }

        public bool TryGetValue(TGameData item, out TValue value)
        {
            if (item != null)
                return TryGetValue(item.Index, out value);

            value = default;
            return false;
        }

        public void Remove(object index)
        {
            if (!ContainsKey(index))
                return;

            int i = itemsIndex[index];
            dataValuePairs.RemoveAt(i);
            itemsIndex.Remove(index);

            Validate(i);
        }

        public void Remove(TGameData item)
        {
            if (item != null)
                Remove(item.Index);
        }

        public virtual void Validate(int start = 0)
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

                object index = dataValuePairs[i].Data.Index;

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

        protected void AddOrUpdate(TGameData data, TValue value)
        {
            var pair = new DataValuePair<TGameData, TValue>(data, value);
            if (!TryAdd(pair))
                dataValuePairs[itemsIndex[data.Index]] = pair;
        }

        private TGameData GetData(object index)
        {
            if (!ContainsKey(index))
                throw new Exception($"Index `{index}` not found in list `{typeof(TGameData)}`");

            if (itemsIndex[index] >= dataValuePairs.Count)
                throw new Exception($"Index `{index}` is out of range in list `{typeof(TGameData)}`");

            return dataValuePairs[itemsIndex[index]].Data;
        }

        private bool TryAdd(DataValuePair<TGameData, TValue> pair)
        {
            object index = pair.Data.Index;

            if (itemsIndex.ContainsKey(index))
                return false;

            dataValuePairs.Add(pair);
            itemsIndex.Add(index, dataValuePairs.Count - 1);

            return true;
        }
    }
}
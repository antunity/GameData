using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IndexedGameData
{
    [Serializable]
    public class GameDataRegister<TGameData> : IEnumerable<TGameData>, ICopyable<GameDataRegister<TGameData>> where TGameData : class, IGameData
    {
        #region IEnumerable

        public IEnumerator<TGameData> GetEnumerator() => items.ToList().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion IEnumerable

        #region ICopyable

        public GameDataRegister<TGameData> Copy()
        {
            var copy = new GameDataRegister<TGameData>();
            copy.items = new List<TGameData>(items);
            copy.Validate();
            return copy;
        }

        #endregion ICopyable

        [SerializeField] private List<TGameData> items = new();

        private readonly Dictionary<object, int> itemsIndex = new();

        public TGameData this[object index]
        {
            get
            {
                if (index is IGameData data)
                    throw new Exception($"`{nameof(IGameData)}` cannot be used as an index. Use '{nameof(IGameData.Index)}' a lookup in the registry of type `{typeof(TGameData)}`");

                if (!ContainsKey(index))
                    throw new Exception($"Index `{index}` not found in list `{typeof(TGameData)}`");

                if (itemsIndex[index] >= items.Count)
                    throw new Exception($"Index `{index}` is out of range in list `{typeof(TGameData)}`");

                return items[itemsIndex[index]];
            }
            set
            {
                if (index is IGameData)
                    throw new Exception($"`{nameof(IGameData)}` cannot be used as an index. Use '{nameof(IGameData.Index)}' a lookup in the registry of type `{typeof(TGameData)}`");

                if (Comparer<object>.Default.Compare(index, value.Index) != 0)
                    throw new Exception($"Index mismatch: {index} != {value.Index}");

                AddOrUpdate(value);
            }
        }

        public int Count => items.Count;

        public IReadOnlyList<TGameData> Data => items.ToList().AsReadOnly();

        public IReadOnlyList<object> Keys => items.ConvertAll(item => item.Index).AsReadOnly();

        public void Add(object index, TGameData data)
        {
            if (Comparer<object>.Default.Compare(index, data.Index) != 0)
                throw new Exception($"Index mismatch: {index} != {data.Index}");

            if (!TryAdd(data))
                throw new Exception($"Index `{data.Index}` already exists in list `{typeof(TGameData)}`");
        }

        public void Clear()
        {
            itemsIndex.Clear();
            items.Clear();
        }

        public bool ContainsData(TGameData data) => data != null && ContainsKey(data.Index);

        public bool ContainsKey(object index)
        {
            if (index == null)
                return false;

            if (index is IGameData)
                throw new Exception($"`{nameof(IGameData)}` cannot be used as an index. Use '{nameof(IGameData.Index)}' a lookup in the registry of type `{typeof(TGameData)}`");

            return itemsIndex.ContainsKey(index);
        }

        public bool TryGetData(object index, out TGameData data)
        {
            if (index is IGameData)
                throw new Exception($"`{nameof(IGameData)}` cannot be used as an index. Use '{nameof(IGameData.Index)}' a lookup in the registry of type `{typeof(TGameData)}`");

            if (ContainsKey(index))
            {
                data = GetData(index);
                return true;
            }

            data = default;
            return false;
        }

        public void Remove(object index)
        {
            if (index is IGameData)
                throw new Exception($"`{nameof(IGameData)}` cannot be used as an index. Use '{nameof(IGameData.Index)}' a lookup in the registry of type `{typeof(TGameData)}`");

            if (!ContainsKey(index))
            {
                Debug.LogWarning($"Index `{index}` not found in list `{typeof(TGameData)}`");
                return;
            }

            int i = itemsIndex[index];
            itemsIndex.Remove(index);
            items.RemoveAt(i);

            Validate(i);
        }

        public void Validate(int start = 0)
        {
            if (items.Count == 0)
            {
                itemsIndex.Clear();
                return;
            }

            if (start == 0)
                itemsIndex.Clear();

            for (int i = start; i < items.Count; i++)
            {
                object index = items[i].Index;

                if (index == null || index.Equals(default))
                    continue;

                if (start != 0)
                    itemsIndex.Remove(index);

                if (!itemsIndex.ContainsKey(index))
                    itemsIndex.Add(index, i);
                else
                {
                    items[i] = null;
                    Debug.LogWarning($"Discarded item with duplicate index [{index}] at position {i} in {nameof(GameDataRegister<TGameData>)}");
                }
            }
        }

        private void AddOrUpdate(TGameData data)
        {
            if (!TryAdd(data))
                items[itemsIndex[data.Index]] = data;
        }

        private TGameData GetData(object index)
        {
            if (!ContainsKey(index))
                throw new Exception($"Index `{index}` not found in list `{typeof(TGameData)}`");

            if (itemsIndex[index] >= items.Count)
                throw new Exception($"Index `{index}` is out of range in list `{typeof(TGameData)}`");

            return items[itemsIndex[index]];
        }

        private bool TryAdd(TGameData data)
        {
            object index = data.Index;

            if (itemsIndex.ContainsKey(index))
                return false;

            items.Add(data);
            itemsIndex.Add(index, items.Count - 1);

            return true;
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace antunity.GameData
{
    [Serializable]
    public class GameDataRegistry<TGameData> : IEnumerable<TGameData>, ICopyable<GameDataRegistry<TGameData>> where TGameData : class, IGameDataBase
    {
        #region IEnumerable

        public IEnumerator<TGameData> GetEnumerator()
        {
            EnsureInitialised();
            return items.ToList().GetEnumerator();
        }
        
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion IEnumerable

        #region ICopyable

        public GameDataRegistry<TGameData> Copy()
        {
            var copy = new GameDataRegistry<TGameData>();
            copy.items = new List<TGameData>(items);
            copy.EnsureInitialised();
            return copy;
        }

        #endregion ICopyable

        [NonSerialized] private bool isInitialised = false;

        [SerializeField] private List<TGameData> items = new();

        private readonly Dictionary<object, int> itemsIndex = new();

        public TGameData this[object index]
        {
            get
            {
                EnsureInitialised();

                if (index is IGameDataBase data)
                    throw new Exception($"`{nameof(IGameDataBase)}` cannot be used as an index. Use '{nameof(IGameDataBase.GetIndex)}' a lookup in the registry of type `{typeof(TGameData)}`");

                if (!ContainsIndex(index))
                    throw new Exception($"Index `{index}` not found in list `{typeof(TGameData)}`");

                if (itemsIndex[index] >= items.Count)
                    throw new Exception($"Index `{index}` is out of range in list `{typeof(TGameData)}`");

                return items[itemsIndex[index]];
            }
            set
            {
                if (index is IGameDataBase)
                    throw new Exception($"`{nameof(IGameDataBase)}` cannot be used as an index. Use '{nameof(IGameDataBase.GetIndex)}' a lookup in the registry of type `{typeof(TGameData)}`");

                if (Comparer<object>.Default.Compare(index, value.GetIndex()) != 0)
                    throw new Exception($"Index mismatch: {index} != {value.GetIndex()}");

                AddOrUpdate(value);
            }
        }

        public int Count => items.Count;

        public IReadOnlyList<TGameData> Data => items.ToList().AsReadOnly();

        public IReadOnlyList<object> Keys => items.ConvertAll(item => item.GetIndex()).AsReadOnly();

        public void Add(TGameData data)
        {
            if (!TryAdd(data))
                throw new Exception($"Index `{data.GetIndex()}` already exists in list `{typeof(TGameData)}`");
        }

        public void Clear()
        {
            itemsIndex.Clear();
            items.Clear();
        }

        public bool ContainsData(TGameData data) => data != null && ContainsIndex(data.GetIndex());

        public bool ContainsIndex(object index)
        {
            EnsureInitialised();

            if (index == null)
                return false;

            if (index is IGameDataBase)
                throw new Exception($"`{nameof(IGameDataBase)}` cannot be used as an index. Use '{nameof(IGameDataBase.GetIndex)}' a lookup in the registry of type `{typeof(TGameData)}`");

            return itemsIndex.ContainsKey(index);
        }

        public bool TryGetData(object index, out TGameData data)
        {
            if (index is IGameDataBase)
                throw new Exception($"`{nameof(IGameDataBase)}` cannot be used as an index. Use '{nameof(IGameDataBase.GetIndex)}' a lookup in the registry of type `{typeof(TGameData)}`");

            if (ContainsIndex(index))
            {
                data = GetData(index);
                return true;
            }

            data = default;
            return false;
        }

        public void Remove(object index)
        {
            if (index is IGameDataBase)
                throw new Exception($"`{nameof(IGameDataBase)}` cannot be used as an index. Use '{nameof(IGameDataBase.GetIndex)}' a lookup in the registry of type `{typeof(TGameData)}`");

            if (!ContainsIndex(index))
            {
                Debug.LogWarning($"Index `{index}` not found in list `{typeof(TGameData)}`");
                return;
            }

            int i = itemsIndex[index];
            itemsIndex.Remove(index);
            items.RemoveAt(i);

            Validate(i);
        }

        protected void EnsureInitialised()
        {
            if (isInitialised)
                return;

            isInitialised = true;
            Validate();
        }

        private void AddOrUpdate(TGameData data)
        {
            if (!TryAdd(data))
                items[itemsIndex[data.GetIndex()]] = data;
        }

        private TGameData GetData(object index)
        {
            if (!ContainsIndex(index))
                throw new Exception($"Index `{index}` not found in list `{typeof(TGameData)}`");

            if (itemsIndex[index] >= items.Count)
                throw new Exception($"Index `{index}` is out of range in list `{typeof(TGameData)}`");

            return items[itemsIndex[index]];
        }

        private bool TryAdd(TGameData data)
        {
            EnsureInitialised();

            object index = data.GetIndex();

            if (itemsIndex.ContainsKey(index))
                return false;

            items.Add(data);
            itemsIndex.Add(index, items.Count - 1);

            return true;
        }

        private void Validate(int start = 0)
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
                object index = items[i].GetIndex();

                if (index == null || index.Equals(default))
                    continue;

                if (start != 0)
                    itemsIndex.Remove(index);

                if (!itemsIndex.ContainsKey(index))
                    itemsIndex.Add(index, i);
                else
                {
                    items[i] = null;
                    Debug.LogWarning($"Discarded item with duplicate index [{index}] at position {i} in {nameof(GameDataRegistry<TGameData>)}");
                }
            }
        }
    }
}
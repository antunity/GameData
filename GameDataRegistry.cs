using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace antunity.GameData
{
    /// <summary>
    /// A serializable dictionary-style registry for serializing game data by their unique index.
    /// This structure is Unity inspector-friendly.
    /// </summary>
    /// <typeparam name="TGameData">the type of the serialized game data</typeparam>
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

        /// <summary>Can be used to get or set game data in the registry.</summary>
        /// <param name="index">the index</param>
        /// <returns>the game data</returns>
        /// <exception cref="Exception">thrown if the index is of invalid type or when the index is not found in the registry</exception>
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

        /// <summary>The number of items in the registry.</summary>
        public int Count => items.Count;

        /// <summary>A list of game data in the registry.</summary>
        public IReadOnlyList<TGameData> Data => items.ToList().AsReadOnly();

        /// <summary>A list of the unique keys in the registry.</summary>
        public IReadOnlyList<object> Keys => items.ConvertAll(item => item.GetIndex()).AsReadOnly();

        /// <summary>Adds a game data entry to the registry.</summary>
        /// <param name="data">the game data</param>
        /// <exception cref="Exception">thrown when data with a duplicate index key is found</exception>
        public void Add(TGameData data)
        {
            if (!TryAdd(data))
                throw new Exception($"Index `{data.GetIndex()}` already exists in list `{typeof(TGameData)}`");
        }

        /// <summary>Clears all items from the registry.</summary>
        public void Clear()
        {
            itemsIndex.Clear();
            items.Clear();
        }

        /// <summary>Checks whether the provided data is found in the registry.</summary>
        /// <param name="data">the game data</param>
        /// <returns>true if the data is found</returns>
        public bool ContainsData(TGameData data) => data != null && ContainsIndex(data.GetIndex());

        /// <summary>Checks if a specific index key is found in the registry.</summary>
        /// <param name="index">the index</param>
        /// <returns>true if the index is found</returns>
        public bool ContainsIndex(object index)
        {
            EnsureInitialised();

            if (index == null)
                return false;

            if (index is IGameDataBase)
                throw new Exception($"`{nameof(IGameDataBase)}` cannot be used as an index. Use '{nameof(IGameDataBase.GetIndex)}' a lookup in the registry of type `{typeof(TGameData)}`");

            return itemsIndex.ContainsKey(index);
        }

        /// <summary>Attempts to get game data by the specified index.</summary>
        /// <param name="index">the index</param>
        /// <param name="data">the returned data</param>
        /// <returns>true of the data is found</returns>
        /// <exception cref="Exception">thrown when an index of invalid type is used</exception>
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

        /// <summary>Removes game data associated with a specific index.</summary>
        /// <param name="index">the index</param>
        /// <exception cref="Exception">thrown if the index is of type IGameDataBase</exception>
        public void Remove(object index)
        {
            if (index is IGameDataBase)
                throw new Exception($"`{nameof(IGameDataBase)}` cannot be used as an index. Use '{nameof(IGameDataBase.GetIndex)}' to perform a lookup in the registry of type `{typeof(TGameData)}`");

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
using System;
using System.Collections.Generic;
using UnityEngine;

namespace antunity.GameData
{
    /// <summary>A type-agnostic contract for game data with a unique index.</summary>
    public interface IGameDataBase
    {
        /// <summary>Returns the index as a C# object.</summary>
        object GetIndex();
    }

    /// <summary>A type-specific contract for game data with a unique index.</summary>
    /// <typeparam name="TIndex">the index type</typeparam>
    public interface IGameData<TIndex> : IGameDataBase
    {
        /// <summary>Returns the index of the game data.</summary>
        TIndex Index { get; }

        /// <inheritdoc/>
        object IGameDataBase.GetIndex() => Index;
    }

    /// <summary>An implementation of IGameData for game data with a unique index.</summary>
    /// <typeparam name="TIndex">the index type</typeparam>
    [Serializable]
    public abstract class GameData<TIndex> : IGameData<TIndex> where TIndex : struct
    {
        [SerializeField] private TIndex index = default;

        /// <inheritdoc/>
        public TIndex Index
        {
            get => index;
            set => index = value;
        }

        public GameData(TIndex index) => this.index = index;
    }

    /// <summary>A contract for game data structs which must be copied during instantiation.</summary>
    /// <typeparam name="TValue">the type of the game data struct</typeparam>
    public interface ICopyable<TValue>
    {
        TValue Copy();
    }

    /// <summary>
    /// An implementation of an instance of game data with a struct that holds relevant game data.
    /// The provided struct represents a template for the game data instance.
    /// This class caches pairs of index and game data during first instantiation unless disabled in GameDataCacheManager.
    /// </summary>
    /// <typeparam name="TIndex">the index type</typeparam>
    /// <typeparam name="TValue">the type of the data struct</typeparam>
    public abstract class GameData<TIndex, TValue> : GameData<TIndex> where TIndex : struct where TValue : struct, ICopyable<TValue>
    {
        private readonly GameDataDefinition<TIndex, TValue> definition = default;

        /// <summary>Returns the template struct for this instance of game data.</summary>
        public TValue Template => definition?.Template ?? throw new NullReferenceException(nameof(definition));

        public GameData(TIndex index, TValue? template = null) : base(index) 
        {
            if (GameDataCacheManager.Enabled)
            {
                if (template.HasValue)
                    GameDataCache<TIndex, TValue>.RegisterTemplate(index, template.Value);

                if (!GameDataCache<TIndex, TValue>.TryGetDefinition(index, out definition))
                    throw new KeyNotFoundException(nameof(index));
            }
            else
            {
                if (template.HasValue)
                    definition = new GameDataDefinition<TIndex, TValue>(index, template.Value);
                else
                    throw new NullReferenceException($"{nameof(template)} cannot be null. Did you accidentally disable GameDataCacheManager?");
            }
        }
    }
}
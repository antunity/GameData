using UnityEngine;

namespace antunity.GameData
{
    /// <summary>A serialized form of indexed game data using ScriptableObject and implementing IGameData.</summary>
    /// <typeparam name="TIndex">the index type</typeparam>
    public abstract class GameDataAsset<TIndex> : ScriptableObject, IGameData<TIndex>
    {
        [Tooltip("A unique index associated with this game data entry.")]
        [SerializeField] protected TIndex index = default;

        /// <inheritdoc/>
        public TIndex Index => index;
    }

    /// <summary>A serialized form of an indexed game data instance with a struct to hold relevant game data.</summary>
    /// <typeparam name="TIndex">the index type</typeparam>
    /// <typeparam name="TValue">the type of the data struct</typeparam>
    public abstract class GameDataAsset<TIndex, TValue> : GameDataAsset<TIndex> where TValue : struct, ICopyable<TValue>
    {
        [Tooltip("A template for this game data entry.")]
        [SerializeField] protected TValue template = default;

        /// <summary>Returns the template struct for this instance of game data.</summary>
        public TValue Template => template;
    }
}

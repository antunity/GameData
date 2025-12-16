using UnityEngine;

namespace uGameData
{
    public abstract class GameDataAsset<TIndex> : ScriptableObject, IGameData<TIndex>
    {
        [Tooltip("A unique index associated with this entry.")]
        [SerializeField] protected TIndex index = default;

        public TIndex Index => index;
    }

    public abstract class GameDataAsset<TIndex, TValue> : GameDataAsset<TIndex> where TValue : struct, ICopyable<TValue>
    {
        [SerializeField] protected TValue template = default;

        public TValue Template => template;
    }
}

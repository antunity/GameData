using UnityEngine;

namespace uGameDataCORE
{
    public abstract class GameDataAsset<TIndex> : ScriptableObject, IGameData
    {
        public static implicit operator TIndex(GameDataAsset<TIndex> obj) => obj ? obj.index : default;

        [Tooltip("A unique index associated with this entry.")]
        [SerializeField] protected TIndex index = default;

        public object Index => index;
    }

    public abstract class GameDataAsset<TIndex, TValue> : GameDataAsset<TIndex> where TValue : struct, ICopyable<TValue>
    {
        public static implicit operator TIndex(GameDataAsset<TIndex, TValue> obj) => obj ? obj.index : default;

        [SerializeField] protected TValue template = default;

        public TValue Template => template;
    }
}

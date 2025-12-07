using System;
using System.Collections.Generic;
using UnityEngine;

namespace uGameDataCORE
{
    public interface ICopyable<T>
    {
        T Copy();
        void Validate();
    }

    public interface IGameData
    {
        object Index { get; }
    }

    [Serializable]
    public abstract class GameData<TIndex> : IGameData
    {
        public static implicit operator TIndex(GameData<TIndex> obj) => obj.index;

        [SerializeField] private TIndex index = default;

        public object Index
        {
            get => index;
            set => index = (TIndex)value;
        }

        public GameData(TIndex index) => this.index = index;
    }

    public abstract class GameDataInstance<TIndex, TValue> : GameData<TIndex> where TIndex : struct where TValue : struct, ICopyable<TValue>
    {
        private readonly GameDataDefinition<TIndex, TValue> definition = default;

        public TValue Template => definition?.Template ?? throw new NullReferenceException(nameof(definition));

        public GameDataInstance(TIndex index, TValue template) : base(index) 
        {
            GameDataCache<TIndex, TValue>.RegisterTemplate(index, template);

            if (!GameDataCache<TIndex, TValue>.TryGetDefinition(index, out definition))
                throw new KeyNotFoundException(nameof(index));
        }

        public GameDataInstance(TIndex index) : base(index)
        {
            if (!GameDataCache<TIndex, TValue>.TryGetDefinition(index, out definition))
                throw new KeyNotFoundException(nameof(index));
        }
    }
}
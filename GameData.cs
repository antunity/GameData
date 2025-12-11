using System;
using System.Collections.Generic;
using UnityEngine;

namespace uGameData
{
    public interface IGameDataBase
    {
        object GetIndex();
    }

    public interface IGameData<TIndex> : IGameDataBase
    {
        TIndex Index { get; }

        object IGameDataBase.GetIndex() => Index;
    }

    [Serializable]
    public abstract class GameData<TIndex> : IGameData<TIndex>
    {
        [SerializeField] private TIndex index = default;

        public TIndex Index
        {
            get => index;
            set => index = value;
        }

        public GameData(TIndex index) => this.index = index;
    }

    public interface ICopyable<T>
    {
        T Copy();

        void Validate();
    }

    public abstract class GameDataInstance<TIndex, TValue> : GameData<TIndex> where TValue : struct, ICopyable<TValue>
    {
        private readonly GameDataDefinition<TIndex, TValue> definition = default;

        public TValue Template => definition?.Template ?? throw new NullReferenceException(nameof(definition));

        public GameDataInstance(TIndex index, TValue? template = null) : base(index) 
        {
            if (template.HasValue)
                GameDataCache<TIndex, TValue>.RegisterTemplate(index, template.Value);

            if (!GameDataCache<TIndex, TValue>.TryGetDefinition(index, out definition))
                throw new KeyNotFoundException(nameof(index));
        }
    }
}
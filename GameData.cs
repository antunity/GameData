using System;
using UnityEngine;

namespace IndexedGameData
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

    public class GameDataDefinition<TIndex, TValue> : GameData<TIndex> where TValue : struct, ICopyable<TValue>
    {
        protected TValue template = default;

        public TValue Template => template;

        public GameDataDefinition(TIndex index, TValue template) : base(index) => this.template = template.Copy();
    }

    public abstract class GameDataInstance<TIndex, TValue> : GameData<TIndex> where TIndex : struct where TValue : struct, ICopyable<TValue>
    {
        private GameDataDefinition<TIndex, TValue> definition = default;

        public GameDataDefinition<TIndex, TValue> Definition => definition;

        public TValue Template => definition?.Template ?? default;

        public GameDataInstance(TIndex index, TValue template) : base(index) => definition = GameDataCache<TIndex, TValue>.TryRegisterGameData(index, template);
    }
}
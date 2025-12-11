using System;
using System.Collections.Generic;

namespace uGameData
{
    internal class GameDataDefinition<TIndex, TValue> : GameData<TIndex> where TValue : struct, ICopyable<TValue>
    {
        protected TValue template = default;

        public TValue Template
        {
            get => template;
            set => template = value.Copy();
        }

        public GameDataDefinition(TIndex index, TValue template) : base(index) => this.template = template.Copy();
    }

    public static class GameDataCache<TIndex, TValue> where TValue : struct, ICopyable<TValue>
    {
        private static GameDataRegistry<GameDataDefinition<TIndex, TValue>> assets = new();

        static GameDataCache() => GameDataCacheManager.RegisterCache<TIndex, TValue>();

        internal static bool TryGetDefinition(TIndex index, out GameDataDefinition<TIndex, TValue> definition)
        {
            GameDataDefinition<TIndex, TValue> asset;

            if (assets.TryGetData(index, out asset))
            {
                definition = asset;
                return true;
            }

            definition = default;
            return false;
        }

        public static void Clear() => assets.Clear();

        public static void RegisterTemplate(TIndex index, TValue template)
        {
            if (assets.ContainsIndex(index))
            {
                assets[index].Template = template;
                return;
            }

            assets.Add(new(index, template));
        }

        public static bool TryGetTemplate(TIndex index, out TValue template)
        {
            GameDataDefinition<TIndex, TValue> asset;

            if (assets.TryGetData(index, out asset))
            {
                template = asset.Template;
                return true;
            }

            template = default;
            return false;
        }
    }

    internal interface IGameDataCache
    {
        void Clear();
    }

    // Helper class to implement the interface and call the static cache
    internal sealed class GameDataCacheWrapper<TIndex, TValue> : IGameDataCache where TValue : struct, ICopyable<TValue>
    {
        // The implementation simply calls the static methods
        public void Clear() => GameDataCache<TIndex, TValue>.Clear();
    }

    public static class GameDataCacheManager
    {
        // Dictionary to hold ALL registered manager instances (one per TIndex/TValue combo)
        private static readonly Dictionary<(Type index, Type value), IGameDataCache> caches = new();

        /// <summary>
        /// Registers a specific type combination (TIndex/TValue) with the manager.
        /// This should be called once per unique cache during initialization.
        /// </summary>
        internal static void RegisterCache<TIndex, TValue>() where TValue : struct, ICopyable<TValue>
        {
            // Get the specific closed generic type of the manager helper
            (Type index, Type value) = (typeof(TIndex), typeof(TValue));

            if (caches.ContainsKey((index, value)))
                return;

            // Create the instance of the helper class
            var instance = new GameDataCacheWrapper<TIndex, TValue>();
            
            // Store the instance and its unique Type
            caches.Add((index, value), instance);
        }

        public static IReadOnlyCollection<(Type index, Type value)> RegisteredCacheTypes => caches.Keys;

        public static void ClearAll()
        {
            foreach (var cache in caches.Values)
                cache.Clear();

            caches.Clear();
        }
    }
}
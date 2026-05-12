using System;
using System.Collections.Generic;

namespace antunity.GameData
{
    // Internal helper interface
    internal interface IGameDataCache
    {
        void Clear();
    }

    // Helper class to implement the interface and call the static cache
    internal sealed class GameDataCacheWrapper<TIndex, TValue> : IGameDataCache where TIndex : struct where TValue : struct, ICopyable<TValue>
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
        internal static void RegisterCache<TIndex, TValue>() where TIndex : struct where TValue : struct, ICopyable<TValue>
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

        public static bool Enabled { get; set; } = true;

        public static IReadOnlyCollection<(Type index, Type value)> RegisteredCacheTypes => caches.Keys;

        public static void ClearAll()
        {
            foreach (var cache in caches.Values)
                cache.Clear();

            caches.Clear();
        }
    }

    internal class GameDataDefinition<TIndex, TValue> : GameData<TIndex> where TIndex : struct where TValue : struct, ICopyable<TValue>
    {
        protected TValue template = default;

        public TValue Template
        {
            get => template;
            set => template = value;
        }

        public GameDataDefinition(TIndex index, TValue template) : base(index) => this.template = template.Copy();
    }

    /// <summary>A runtime cache of instantiated game data. Designed as the game-wide source for indexed game data templates.</summary>
    /// <typeparam name="TIndex">the index type</typeparam>
    /// <typeparam name="TValue">the type of the data struct</typeparam>
    public static class GameDataCache<TIndex, TValue> where TIndex : struct where TValue : struct, ICopyable<TValue>
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

        /// <summary>Clears the cached game data.</summary>
        public static void Clear() => assets.Clear();

        /// <summary>Registers a game data template to the cache.</summary>
        /// <param name="index">the index of the game data</param>
        /// <param name="template">the template for the game data</param>
        public static void RegisterTemplate(TIndex index, TValue template)
        {
            if (assets.ContainsIndex(index))
            {
                assets[index].Template = template;
                return;
            }

            assets.Add(new(index, template));
        }

        /// <summary>Attempts to get a game data template with the provided index.</summary>
        /// <param name="index">the index of the game data</param>
        /// <param name="template">the game data template</param>
        /// <returns>true if the template was found in the cache, false if it was not.</returns>
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
}
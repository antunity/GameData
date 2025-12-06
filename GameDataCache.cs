
using System.Collections.Generic;

namespace IndexedGameData
{
    public static class GameDataCache<TAsset> where TAsset : class, IGameData
    {
        private static GameDataRegister<TAsset> assets = new();

        public static void Clear() => assets.Clear();

        public static void RegisterGameData(TAsset asset) => assets.Add(asset);

        public static TAsset GetGameData(object index)
        {
            TAsset asset;
            if (assets.TryGetData(index, out asset))
                return asset;

            throw new KeyNotFoundException(nameof(index));
        }

        public static bool TryGetGameData(object index, out TAsset asset) => assets.TryGetData(index, out asset);

        public static void Validate() => assets.Validate();
    }
}
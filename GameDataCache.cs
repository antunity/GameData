
using System.Collections.Generic;

namespace uGameDataCORE
{
    public static class GameDataCache<TIndex, TValue> where TIndex : struct where TValue : struct, ICopyable<TValue>
    {
        private static GameDataRegistry<GameDataDefinition<TIndex, TValue>> assets = new();

        public static void Clear() => assets.Clear();

        public static void RegisterGameData(TIndex index, TValue template) => assets.Add(new(index, template));

        public static TValue GetGameData(TIndex index)
        {
            GameDataDefinition<TIndex, TValue> asset;
            if (assets.TryGetData(index, out asset))
                return asset.Template;

            throw new KeyNotFoundException(nameof(index));
        }

        public static bool TryGetGameData(TIndex index, out TValue template)
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

        public static GameDataDefinition<TIndex, TValue> TryRegisterGameData(TIndex index, TValue template)
        {
            if (assets.ContainsData(new(index, template)))
                return assets[index];

            assets.Add(new(index, template));
            return assets[index];
        }

        public static void Validate() => assets.Validate();
    }
}
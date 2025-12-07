namespace uGameDataCORE
{
    internal class GameDataDefinition<TIndex, TValue> : GameData<TIndex> where TIndex : struct where TValue : struct, ICopyable<TValue>
    {
        protected TValue template = default;

        public TValue Template
        {
            get => template;
            set => template = value.Copy();
        }

        public GameDataDefinition(TIndex index, TValue template) : base(index) => this.template = template.Copy();
    }

    public static class GameDataCache<TIndex, TValue> where TIndex : struct where TValue : struct, ICopyable<TValue>
    {
        private static GameDataRegistry<GameDataDefinition<TIndex, TValue>> assets = new();

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
            if (assets.ContainsKey(index))
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
}
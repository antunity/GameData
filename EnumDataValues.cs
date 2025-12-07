using System;
using System.Collections.Generic;

namespace uGameDataCORE
{
    [Serializable]
    [GameDataDrawer(GameDataLayout.Vertical)]
    public class EnumData<TEnum> : GameData<TEnum> where TEnum : struct
    {
        public EnumData(TEnum index) : base(index) { }
    }

    [Serializable]
    public class EnumDataValues<TEnum, TValue> : GameDataValues<EnumData<TEnum>, TValue> where TEnum : struct
    {
        #region ICopyable

        public new EnumDataValues<TEnum, TValue> Copy()
        {
            var copy = new EnumDataValues<TEnum, TValue>();
            copy.dataValuePairs = new List<DataValuePair<EnumData<TEnum>, TValue>>(dataValuePairs);
            copy.Validate();
            return copy;
        }

        #endregion ICopyable
    }
}
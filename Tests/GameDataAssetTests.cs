using System;
using System.Reflection;

using NUnit.Framework;
using UnityEngine;

using uGameDataCORE;

namespace SharedTests
{
    public class GameDataAssetTests
    {
        // Definitions
        internal class TestableGameDataAsset : GameDataAsset<uint>
        {
        }

        // Extension
        public static void SetGameDataAssetIndex(GameDataAsset<uint> indexedAsset, uint index)
        {
            Type TAsset = indexedAsset.GetType();
            FieldInfo field = TAsset.GetField("index", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(indexedAsset, index);
        }

        // Methods
        // Public
        [Test]
        public void AssetInstantiate()
        {
            TestableGameDataAsset asset = ScriptableObject.CreateInstance<TestableGameDataAsset>();
            Assert.IsNotNull(asset, "Failed to instantiate object");
            Assert.IsTrue(asset == 0, "IndexedAsset has incorrect value");
        }
    }
}

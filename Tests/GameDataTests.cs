using NUnit.Framework;

using uGameData;

namespace SharedTests {
    internal class GameDataTests {
        // Definitions
        internal class TestableGameData : GameData<string>
        {
            public TestableGameData() : base(string.Empty) { }
        }

        // Methods
        // Public
        [Test]
        public void Instantiate() {
            TestableGameData entry = new();
            Assert.IsNotNull(entry, "Failed to instantiate object");

            entry.Index = "123";
            Assert.IsTrue((string)entry.Index == "123", "IndexedEntry index was not modified");
        }
    }
}

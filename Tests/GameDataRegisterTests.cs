using NUnit.Framework;
using uGameDataCORE;

namespace SharedTests {
    internal class GameDataRegisterTests {
        // Definitions
        internal class TestableGameDataClass<TIndex> : IGameData {
            public object Index {
                get; set;
            }

            public TestableGameDataClass(TIndex index) => Index = index;
        }

        // Methods
        // Public
        [Test]
        public void Instantiate() {
            GameDataRegistry<TestableGameDataClass<string>> indexedList = new();
            Assert.IsNotNull(indexedList, "Failed to instantiate object");
        }

        [Test]
        public void AddRemove() {
            GameDataRegistry<TestableGameDataClass<string>> indexedList = new();

            // Contains
            Assert.IsFalse(indexedList.ContainsKey(null), "List contains a null value");

            // IndexedClass
            string index = "TEST";
            TestableGameDataClass<string> indexedClass = new(index);

            // Add
            indexedList[(string)indexedClass.Index] = indexedClass;
            Assert.IsTrue(indexedList.ContainsKey(index), "Failed to add indexed object");
            Assert.IsTrue(indexedList.Count == 1, "Failed to add indexed object");

            // Add 2nd
            string index2 = "TEST2";
            TestableGameDataClass<string> indexedClass2 = new(index2);
            indexedList[(string)indexedClass2.Index] = indexedClass2;
            Assert.IsTrue(indexedList.Count == 2, "Failed to add second indexed object");
            Assert.IsTrue(indexedList.Data.Count == 2, "Failed to verify item list size");

            // Remove
            indexedList.Remove(index);
            Assert.IsTrue(indexedList.Count == 1, "Failed to remove item from list");

            indexedList.Remove(index2);
            Assert.IsTrue(indexedList.Count == 0, "Failed to remove item from list");

            // Clear
            indexedList[(string)indexedClass.Index] = indexedClass;
            indexedList[(string)indexedClass2.Index] = indexedClass2;
            indexedList.Clear();
            Assert.IsTrue(indexedList.Count == 0, "Failed to clear list");
        }

        [Test]
        public void AccessValue() {
            GameDataRegistry<TestableGameDataClass<string>> indexedList = new();

            // Contains
            Assert.IsFalse(indexedList.ContainsKey(null), "List contains a null value");

            // IndexedClass
            string index = "TEST";
            string index2 = "TEST2";
            TestableGameDataClass<string> indexedClass = new(index);
            TestableGameDataClass<string> indexedClass2 = new(index2);

            // Add
            indexedList[(string)indexedClass.Index] = indexedClass;
            indexedList[(string)indexedClass2.Index] = indexedClass2;

            // Access
            Assert.IsTrue(indexedList[index] == indexedClass && indexedList[index2] == indexedClass2, "Failed to access added values by index");
        }
    }
}

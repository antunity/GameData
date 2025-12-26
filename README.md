# antunity.GameData

**antunity.GameData** is a Unity toolkit designed to provide an generic architecture for game data with a unique index. It bridges the gap between Unity's serialization system and high-performance dictionary-style lookups, providing an architecture for game data implementation.

## Core Philosophy
In many game architectures, data needs to be easily retrievable via unique keys (enums or integer IDs). **antunity.GameData** supports this natively by implementing relevant interfaces such as `IGameDataBase` which provides a unique index.

## Key Features

* **Inspector-Friendly Lookups**: Implements `GameDataRegistry` and `GameDataValues`, which display as lists in the Inspector but function as dictionaries at runtime.
* **Template & Definition System**: A built-in caching layer allows you to define "template" data that can be registered and retrieved efficiently.
* **ScriptableObject Integration**: Extends `ScriptableObject` through `GameDataAsset` to allow data definitions to live as project assets.
* **Safe Data Copying**: Utilizes an `ICopyable` interface to ensure that complex data structures can be cloned without reference bleeding between instances.
* **Lazy Initialization**: Automatically rebuilds internal lookup indices only when accessed, bypassing Unity's serialization depth issues and "empty list" bugs.

---

## Core Components

### 1. The Indexing System
At the heart of the toolkit are the interfaces and classes that define how data is identified:
* **`IGameDataBase`**: The base interface requiring a `GetIndex()` method.
* **`IGameData<TIndex>`**: A generic interface for data associated with a specific struct-based index.
* **`GameData<TIndex>`**: An abstract base class implementing the standard index field.

### 2. Registries and Value Maps
The toolkit provides specialized containers to manage your data collections:
* **`GameDataRegistry<TGameData>`**: A collection of unique data items accessible via their index. It automatically manages internal dictionary lookups and discards duplicate indices.
* **`GameDataValues<TGameData, TValue>`**: Maps a data entry to a specific value.
* **`EnumDataValues<TEnum, TValue>`**: A specialized version for quick mapping between Enums and values.

### 3. Caching and Assets
* **`GameDataCache<TIndex, TValue>`**: A static cache for high-speed retrieval of game data definitions and templates.
* **`GameDataCacheManager`**: Centralized control to clear or register multiple cache types.
* **`GameDataAsset<TIndex, TValue>`**: A `ScriptableObject` base class that allows you to create data assets directly in the Unity Editor with built-in validation.

---

## Implementation Example

To create a new type of game data, inherit from `GameDataAsset`:

```csharp
using antunity.GameData;

// Define an enum - alternatively you can use integer IDs
public enum ItemType { Sword, Shield }

// Define the asset
[CreateAssetMenu]
public class ItemData : GameDataAsset<ItemType, ItemStats> { }

// ItemStats must implement ICopyable to prevent reference issues
[Serializable]
public struct ItemStats : ICopyable<ItemStats> {
    public int Power;
    public ItemStats Copy() => new ItemStats { Power = this.Power };
}
```

You can then reference these in a registry within a MonoBehaviour:

```csharp
using UnityEngine;
using antunity.GameData;

public class InventoryManager : MonoBehaviour {
    [SerializeField] private GameDataRegistry<ItemData> itemRegistry;

    void Start() {
        // Access is O(1) via the internal dictionary
        // The registry self-initializes on this first call
        var sword = itemRegistry[ItemType.Sword]; 
    }
}
```

---

## Technical Details

### Copying Logic

The `Copy()` method in `GameDataRegistry` and `GameDataValues` creates a new list instance while maintaining the indexed structure. This ensures that runtime modifications (like adding or removing items) to one registry do not affect the original source asset.

### Self-Healing Dictionary

In serialized data, when Unity reloads a scene or performs a domain reload, the internal Dictionary (used for O(1) lookups) is lost. The toolkit detects this on the next data access and automatically triggers a `Validate()` to rebuild the index from the serialized List.

using System;

namespace antunity.GameData
{
    /// <summary>Specifies a preference for vertical or horizontal display.</summary>
    public enum GameDataLayout { None, Vertical, Horizontal }

    /// <summary>Attribute to specify a layout for the property drawer.</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
    public sealed class GameDataDrawerAttribute : Attribute
    {
        /// <summary>The specified layout.</summary>
        public GameDataLayout Layout { get; }

        public GameDataDrawerAttribute(GameDataLayout layout) => Layout = layout;
    }

    /// <summary>Tag interface for types that should be rendered by the custom drawer.</summary>
    public interface IUseGameDataDrawer { };
}
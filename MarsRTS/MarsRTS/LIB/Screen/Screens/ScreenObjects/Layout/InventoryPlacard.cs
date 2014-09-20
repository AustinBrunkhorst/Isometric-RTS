using Microsoft.Xna.Framework;
using RTSAssetData.Buildings;

namespace MarsRTS.LIB.Screen.Screens.ScreenObjects.Layout
{
    public class InventoryPlacard
    {
        Building type;

        public Building Type
        {
            get { return type; }
            set { type = value; }
        }

        Vector2 position;

        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }

        Vector2 textOffset;

        public Vector2 TextOffset
        {
            get { return textOffset; }
            set { textOffset = value; }
        }

        Color color;

        public Color Color
        {
            get { return color; }
            set { color = value; }
        }

        string text;

        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        int members;

        public int Members
        {
            get { return members; }
            set { members = value; }
        }

        public InventoryPlacard(Building type, Vector2 position, Vector2 textOffset, BuildingProperties properties, int members)
        {
            this.type = type;
            this.position = position;
            this.textOffset = textOffset;
            this.text = properties.Description;
            this.members = members;
            this.color = properties.InventoryColor;
        }
    }
}

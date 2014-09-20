using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MarsRTS.LIB
{
    class Map
    {
        Texture2D tiles;

        SpriteFont font;

        public Block[][] Blocks
        {
            get;
            set;
        }

        static Random random = new Random();

        int Width;
        int Depth;

        int baseWidth = 56;
        int baseHeight = 28;

        public Map(ContentManager content, int width, int depth)
        {
            Width = width;
            Depth = depth;

            tiles = content.Load<Texture2D>("blocks");

            font = content.Load<SpriteFont>("font");

            //  Initialise block 2D array
            Blocks = new Block[Width][];

            // Position blocks
            Rectangle position = new Rectangle(0, 0, 56, tiles.Height);

            for (int x = 0; x < width; x++)
            {
                Blocks[x] = new Block[Depth];

                position.X = (baseWidth / 2) * x;
                position.Y = (baseHeight / 2) * x;

                for (int z = 0; z < depth; z++)
                {

                    Blocks[x][z] = new Block(0, position) { Tile = 8 };
                    position.Y -= (baseHeight / 2);
                    position.X += (baseWidth / 2);
                }
            }
        }

        /// <summary>
        /// Draws map
        /// </summary>
        /// <param name="batch"></param>
        public void Draw(SpriteBatch batch, Vector2 offset)
        {
            Rectangle drawPosition;

            for (int x = 0; x < Width; x++)
                for (int z = Depth - 1; z > -1; z--)
                {
                    Block block = Blocks[x][z];
                    drawPosition = block.Position;
                    drawPosition.X += (int)offset.X + block.Offset.X;
                    drawPosition.Y += (int)offset.Y + block.Offset.Y;

                    batch.Draw(tiles, drawPosition, new Rectangle(block.Tile * 56, 0, drawPosition.Width, drawPosition.Height), block.Tint);
                }
        }

        public void Reset()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Depth; y++)
                {
                    Blocks[x][y].Tile = 8;
                    Blocks[x][y].Tint = Color.White;
                    Blocks[x][y].Offset = Point.Zero;
                }
            }
        }

        public Point ScreenToWorld(Vector2 screenPosition)
        {
            Vector2 offset = screenPosition /
                new Vector2(baseWidth, baseHeight);

            return new Point()
            {
                X = (int)Math.Round(offset.X + offset.Y) - 1,
                Y = -(int)Math.Round(-offset.X + offset.Y)
            };
        }
    }

    class Block
    {
        int tile;

        public int Tile
        {
            get { return tile; }
            set
            {
                tile = value;
            }
        }


        public Rectangle Position;

        public Point Offset;

        public Color Tint = Color.White;

        public Block(int tile, Rectangle position)
        {
            Tile = tile;
            Position = position;
        }
    }
}

using MarsRTS.LIB.Extensions;
using MarsRTS.LIB.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MarsRTS.LIB.GameObjects
{
    public class CursorManager : DrawableGameComponent
    {
        SpriteBatch spriteBatch;

        Cursor cursor;

        public Cursor Cursor
        {
            get { return cursor; }
            set { cursor = value; }
        }

        Vector2 position;

        public CursorManager(Game game)
            : base(game)
        {
            Visible = true;
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        }

        public override void Update(GameTime gameTime)
        {
            position = InputManager.MousePosition.ToVector();
        }

        public override void Draw(GameTime gameTime)
        {
            if (Visible)
            {
                spriteBatch.Begin();

                spriteBatch.Draw(cursor.Texture,
                    position + cursor.Offset,
                    cursor.Tint);

                spriteBatch.End();
            }
        }
    }

    public class Cursors
    {
        static Cursor defaultCursor;

        public static Cursor Default
        {
            get { return defaultCursor; }
        }

        static Cursor pointer;

        public static Cursor Pointer
        {
            get { return pointer; }
        }

        public static void LoadContent(Game game, ContentManager content)
        {
            var arrowTexture = content.Load<Texture2D>(@"Cursors/Default");

            defaultCursor = new Cursor(arrowTexture, Vector2.Zero);

            var pointerTexture = content.Load<Texture2D>(@"Cursors/Pointer");

            pointer = new Cursor(pointerTexture, new Vector2(-4, 0));
        }
    }

    public class Cursor
    {
        Texture2D texture;

        public Texture2D Texture
        {
            get { return texture; }
            set { texture = value; }
        }

        Vector2 offset;

        public Vector2 Offset
        {
            get { return offset; }
            set { offset = value; }
        }

        Color tint = Color.White;

        public Color Tint
        {
            get { return tint; }
            set { tint = value; }
        }

        public Cursor(Texture2D texture, Vector2 offset)
        {
            this.texture = texture;
            this.offset = offset;
        }
    }
}

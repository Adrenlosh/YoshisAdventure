using Gum.Forms.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum;
using MonoGameLibrary;
using MonoGameLibrary.Scenes;
using System;

namespace Project6.Scenes
{
    public class TitleScene : Scene
    {
        private SpriteFont _font;
        // The texture used for the background pattern.
        private Texture2D _backgroundPattern;
        // The destination rectangle for the background pattern to fill.
        private Rectangle _backgroundDestination = Core.GraphicsDevice.PresentationParameters.Bounds;
        // The offset to apply when drawing the background pattern so it appears to be scrolling.
        private Vector2 _backgroundOffset = Vector2.Zero;

        public override void Initialize()
        {
            base.Initialize();

            var button = new Button();
            button.AddToRoot();
            button.Click += (_, _) =>Core.ChangeScene(new GamingScene());
        }

        public override void LoadContent()
        {
            _font = Content.Load<SpriteFont>("fonts/Roboto");
            _backgroundPattern = Content.Load<Texture2D>("images/background-pattern");
        }

        public override void Update(GameTime gameTime)
        {
            // Update the offsets for the background pattern wrapping so that it scrolls down and to the right.
            float offset = 23f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            _backgroundOffset.X += offset;
            _backgroundOffset.Y -= offset;

            // Ensure that the offsets do not go beyond the texture bounds so it is a seamless wrap.
            _backgroundOffset.X %= _backgroundPattern.Width;
            _backgroundOffset.Y %= _backgroundPattern.Height;

            GumService.Default.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            Core.GraphicsDevice.Clear(Color.CornflowerBlue);

            Core.SpriteBatch.Begin(samplerState: SamplerState.PointWrap);
            Core.SpriteBatch.Draw(_backgroundPattern, _backgroundDestination, new Rectangle(_backgroundOffset.ToPoint(), _backgroundDestination.Size), Color.White);
            Core.SpriteBatch.End();
            Core.SpriteBatch.Begin();
            Core.SpriteBatch.DrawString(_font, "Project6", new Vector2(10, 0), Color.Red);
            Core.SpriteBatch.End();
        }
    }
}
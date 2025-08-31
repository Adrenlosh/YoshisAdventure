using Gum.Forms.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;
using MonoGameGum;
using MonoGameLibrary;
using MonoGameLibrary.Scenes;
using RenderingLibrary;

namespace Project6.Scenes
{
    public class TitleScene : Scene
    {
        private SpriteFont _font;
        private Texture2D _backgroundPattern;
        private Rectangle _backgroundDestination = Core.GraphicsDevice.PresentationParameters.Bounds;
        private Vector2 _backgroundOffset = Vector2.Zero;
        private OrthographicCamera _camera;
        private ViewportAdapter _viewportAdapter;
        private readonly Point _virtualResolution = new Point(320, 240);

        public override void Initialize()
        {
            base.Initialize();
            

        }

        public override void LoadContent()
        {
            _font = Content.Load<SpriteFont>("fonts/Roboto");
            _backgroundPattern = Content.Load<Texture2D>("images/background-pattern");
            _viewportAdapter = new BoxingViewportAdapter(GameMain.Instance.Window, Core.GraphicsDevice, _virtualResolution.X, _virtualResolution.Y);
            _camera = new OrthographicCamera(_viewportAdapter);

            var button = new Button();
            button.Text = "Click to start...";
            button.AddToRoot();
            button.Click += (_, _) => Core.ChangeScene(new GamingScene());
        }

        public override void Update(GameTime gameTime)
        {
            float offset = 35f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            _backgroundOffset.X += offset;
            _backgroundOffset.Y -= offset;
            _backgroundOffset.X %= _backgroundPattern.Width;
            _backgroundOffset.Y %= _backgroundPattern.Height;
            GumService.Default.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            Core.GraphicsDevice.Clear(Color.Black);
            Core.SpriteBatch.Begin(samplerState: SamplerState.PointWrap, transformMatrix: _camera.GetViewMatrix());
            Core.SpriteBatch.Draw(_backgroundPattern, _backgroundDestination, new Rectangle(_backgroundOffset.ToPoint(), _backgroundDestination.Size), Color.White);
            Core.SpriteBatch.DrawString(_font, "Project6", new Vector2(100, 0), Color.Red);
            Core.SpriteBatch.End();
            GumService.Default.Draw();
        }
    }
}
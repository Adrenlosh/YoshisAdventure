using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.ViewportAdapters;
using MonoGameGum;
using MonoGameLibrary;
using MonoGameLibrary.Scenes;
using Project6.UI;
using RenderingLibrary;

namespace Project6.Scenes
{
    public class TitleScene : Scene
    {
        private BitmapFont _font;
        private Texture2D _backgroundPattern;
        private Rectangle _backgroundDestination = Core.GraphicsDevice.PresentationParameters.Bounds;
        private Vector2 _backgroundOffset = Vector2.Zero;
        private ViewportAdapter _viewportAdapter;
        private readonly Point _virtualResolution = new Point(320, 240);

        private TitleSceneUI _ui;

        public override void Initialize()
        {
            base.Initialize();
            _viewportAdapter = new BoxingViewportAdapter(GameMain.Instance.Window, Core.GraphicsDevice, _virtualResolution.X, _virtualResolution.Y);
            InitializeUI();
        }

        public override void LoadContent()
        {
            _font = Content.Load<BitmapFont>("fonts/ZFull-GB");
            _backgroundPattern = Content.Load<Texture2D>("images/background-pattern");
        }

        public override void Update(GameTime gameTime)
        {
            if(Core.Input.Keyboard.WasKeyJustPressed(Keys.Enter))
            {
                Core.ChangeScene(new GamingScene());
            }

            float offset = 35f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            _backgroundOffset.X += offset;
            _backgroundOffset.Y -= offset;
            _backgroundOffset.X %= _backgroundPattern.Width;
            _backgroundOffset.Y %= _backgroundPattern.Height;
            _ui.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            Matrix matrix = _viewportAdapter.GetScaleMatrix();
            Core.GraphicsDevice.Clear(Color.Black);
            Core.SpriteBatch.Begin(samplerState: SamplerState.PointWrap, transformMatrix: matrix);
            Core.SpriteBatch.Draw(_backgroundPattern, _backgroundDestination, new Rectangle(_backgroundOffset.ToPoint(), _backgroundDestination.Size), Color.White);
            Core.SpriteBatch.DrawString(_font, "Project6", new Vector2(100, 0), Color.Red);
            Core.SpriteBatch.End();
            _ui.Draw(matrix.M11);
        }

        private void InitializeUI()
        {
            _ui = new TitleSceneUI();
            _ui.Width = _viewportAdapter.VirtualWidth;
            _ui.Height = _viewportAdapter.VirtualHeight;
        }
    }
}
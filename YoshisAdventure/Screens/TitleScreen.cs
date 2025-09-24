using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;
using MonoGame.Extended.ViewportAdapters;
using MonoGameGum;
using YoshisAdventure.Systems;
using YoshisAdventure.UI;

namespace YoshisAdventure.Screens
{
    public class TitleScreen : GameScreen
    {
        private Texture2D _backgroundPattern;
        private Rectangle _backgroundDestination;
        private Vector2 _backgroundOffset = Vector2.Zero;
        private ViewportAdapter _viewportAdapter;
        private SpriteBatch _spriteBatch;
        private TitleScreenUI _ui;

        public TitleScreen(Game game) : base(game)
        {
        }<

        public override void Initialize()
        {
            base.Initialize();
            _backgroundDestination = Game.GraphicsDevice.Viewport.Bounds;
            _viewportAdapter = new BoxingViewportAdapter(Game.Window, Game.GraphicsDevice, GlobalConfig.VirtualResolution_Width, GlobalConfig.VirtualResolution_Height);
            InitializeUI();

            AudioSystem.PlaySong("title");
        }

        public override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _backgroundPattern = Content.Load<Texture2D>("Images/background-pattern");
        }

        public override void Update(GameTime gameTime)
        {
            AudioSystem.Update(gameTime);
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
            Game.GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin(samplerState: SamplerState.PointWrap, transformMatrix: matrix);
            _spriteBatch.Draw(_backgroundPattern, _backgroundDestination, new Rectangle(_backgroundOffset.ToPoint(), _backgroundDestination.Size), Color.White);
            _spriteBatch.End();
            _ui.Draw(matrix.M11);
        }

        private void InitializeUI()
        {
            GumService.Default.Root.Children.Clear();
            _ui = new TitleScreenUI();
            _ui.StartButtonClicked += (s, e) => ScreenManager.LoadScreen(new GamingScreen(Game), new FadeTransition(GraphicsDevice, Color.Black, 1.5f));
        }
    }
}
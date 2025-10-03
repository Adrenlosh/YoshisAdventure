using GameLibrary.Input;
using Gum.Forms.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;
using MonoGame.Extended.ViewportAdapters;
using MonoGameGum;
using SoundFlow.Abstracts.Devices;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Structs;
using System.Linq;
using YoshisAdventure.Screens;
using YoshisAdventure.Status;
using YoshisAdventure.Systems;

namespace YoshisAdventure
{
    public class GameMain : Game
    {
        private GraphicsDeviceManager _graphicsDeviceManager;
        private ScreenManager _screenManager;

        public ViewportAdapter ViewportAdapter { get; private set; }

        public static PlayerStatus PlayerStatus { get; set; } = new PlayerStatus();

        public static InputManager Input { get; private set; } = new InputManager();

        public GameMain()
        {
            _graphicsDeviceManager = new GraphicsDeviceManager(this);
            _graphicsDeviceManager.PreferredBackBufferWidth = GlobalConfig.VirtualResolution_Width;
            _graphicsDeviceManager.PreferredBackBufferHeight = GlobalConfig.VirtualResolution_Height;
            _graphicsDeviceManager.PreferHalfPixelOffset = false;
            _graphicsDeviceManager.ApplyChanges();

            Content.RootDirectory = "Content";
            Window.AllowUserResizing = true;
            Window.Title = Language.Strings.GameName;
            IsMouseVisible = true;
            
            _screenManager = new ScreenManager();
            Components.Add(_screenManager);
        }

        private void InitializeGum()
        {
            GumService.Default.Initialize(this);
            FrameworkElement.KeyboardsForUiControl.Add(GumService.Default.Keyboard);
            FrameworkElement.GamePadsForUiControl.AddRange(GumService.Default.Gamepads);
            FrameworkElement.TabReverseKeyCombos.Add(new KeyCombo() { PushedKey = Microsoft.Xna.Framework.Input.Keys.Up });
            FrameworkElement.TabKeyCombos.Add(new KeyCombo() { PushedKey = Microsoft.Xna.Framework.Input.Keys.Down });
            GumService.Default.ContentLoader.XnaContentManager = Content;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            GumService.Default.Draw();
            base.Draw(gameTime);
        }

        protected override void Update(GameTime gameTime)
        {
            Input.Update(gameTime);
            SFXSystem.Update(gameTime);
            GumService.Default.Update(gameTime);
            base.Update(gameTime);
        }

        protected override void LoadContent()
        {
            ViewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, GlobalConfig.VirtualResolution_Width, GlobalConfig.VirtualResolution_Height);
            InitializeGum();
            InitializeAudio(out MiniAudioEngine engine, out AudioPlaybackDevice playbackDevice);
            StageSystem.Initialize(Content);
            SFXSystem.Initialize(Content, engine, playbackDevice);
            SongSystem.Initialize(Content, engine, playbackDevice);
#if !DEBUG
            LoadScreen(new LogoScreen(this));
#else
            LoadScreen(new TitleScreen(this));
#endif
            base.LoadContent();
        }

        private void InitializeAudio(out MiniAudioEngine engine, out AudioPlaybackDevice device)
        {
            engine = new MiniAudioEngine();
            DeviceInfo defaultDevice = engine.PlaybackDevices.FirstOrDefault(x => x.IsDefault);
            device = engine.InitializePlaybackDevice(defaultDevice, AudioFormat.Dvd);
            device.Start();
        }

        protected override void UnloadContent()
        {
            SongSystem.Dispose();
            SFXSystem.Dispose();
            base.UnloadContent();
        }

        public void LoadScreen(GameScreen screen, Transition transition = null)
        {
            if (transition != null)
            {
                _screenManager.LoadScreen(screen, transition);
            }
            else
            {
                _screenManager.LoadScreen(screen);
            }
        }
    }
}
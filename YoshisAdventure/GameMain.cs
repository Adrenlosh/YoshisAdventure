using GameLibrary.Audio;
using GameLibrary.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Screens;
using MonoGame.Extended.ViewportAdapters;
using MonoGameGum;
using YoshisAdventure.Scenes;
using YoshisAdventure.Status;

namespace YoshisAdventure;

public class GameMain : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private readonly ScreenManager _screenManager;

    public ViewportAdapter ViewportAdapter { get; private set; }

    public static PlayerStatus playerStatus { get; set; } = new PlayerStatus();

    public static InputManager Input { get; private set; } = new InputManager();

    public static AudioController Audio { get; private set; } = new AudioController();

    public GameMain()
    {
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = GlobalConfig.VirtualResolution_Width,
            PreferredBackBufferHeight = GlobalConfig.VirtualResolution_Height,
            SynchronizeWithVerticalRetrace = false
        };
        Content.RootDirectory = "Content";
        IsFixedTimeStep = true;
        Window.AllowUserResizing = true;
        IsMouseVisible = true;
        Window.Title = "Yoshi's Adventure";
        _screenManager = Components.Add<ScreenManager>();
    }

    private void InitializeGum()
    {
        GumService.Default.Initialize(this);
        GumService.Default.ContentLoader.XnaContentManager = Content;
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        GumService.Default.Draw();
        base.Draw(gameTime);
    }

    protected override void Update(GameTime gameTime)
    {
        Input.Update(gameTime);
        base.Update(gameTime);
    }

    protected override void LoadContent()
    {
        InitializeGum();
        ViewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, GlobalConfig.VirtualResolution_Width, GlobalConfig.VirtualResolution_Height);
        _screenManager.LoadScreen(new TitleScreen(this));
    }
}
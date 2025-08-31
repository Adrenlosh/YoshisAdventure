using Gum.Forms.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.ViewportAdapters;
using MonoGameGum;
using MonoGameLibrary;
using Project6.Scenes;
using System;

namespace Project6;

public class GameMain : Core
{
    public ViewportAdapter ViewportAdapter { get; private set; }

    public GameMain() : base("Project6", 320, 240, false)
    {
    }

    protected override void Initialize()
    {
        base.Initialize();
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnWindowSizeChanged;
        InitializeGum();
        //Audio.PlaySong(_backgroundMusic, true);
        ChangeScene(new TitleScene());
    }

    private void OnWindowSizeChanged(object sender, EventArgs e)
    {
    }

    private void InitializeGum()
    {
        GumService.Default.Initialize(this);
        GumService.Default.ContentLoader.XnaContentManager = Core.Content;
        FrameworkElement.KeyboardsForUiControl.Add(GumService.Default.Keyboard);
        FrameworkElement.GamePadsForUiControl.AddRange(GumService.Default.Gamepads);
        FrameworkElement.TabReverseKeyCombos.Add(new KeyCombo() { PushedKey = Keys.Up });
        FrameworkElement.TabKeyCombos.Add(new KeyCombo() { PushedKey = Keys.Down });
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        base.Draw(gameTime);
        GumService.Default.Draw();
    }
    protected override void LoadContent()
    {
        // Load the background theme music.
        //_backgroundMusic = Content.Load<Song>("audio/song/title");
        ViewportAdapter = new BoxingViewportAdapter(Window, Core.GraphicsDevice, 320,240);
    }

}
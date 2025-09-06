using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.ViewportAdapters;
using MonoGameGum;
using MonoGameGum.Forms;
using MonoGameGum.Forms.Controls;
using MonoGameLibrary;
using Project6.Scenes;
using RenderingLibrary;

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

        InitializeGum();
        ChangeScene(new TitleScene());
    }

    private void InitializeGum()
    {
        GumService.Default.Initialize(this);
        GumService.Default.ContentLoader.XnaContentManager = Core.Content;
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        base.Draw(gameTime);
        GumService.Default.Draw();
    }

    protected override void LoadContent()
    {
        ViewportAdapter = new BoxingViewportAdapter(Window, Core.GraphicsDevice, 320, 240);
    }
}
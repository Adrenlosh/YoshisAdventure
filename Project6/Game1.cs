using Gum.Forms.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum;
using MonoGameLibrary;
using Project6.Scenes;
using System;

namespace Project6;

public class Game1 : Core
{
    public readonly Point targetResolution = new Point(320, 240);
    private RenderTarget2D _renderTarget;

    public Game1() : base("Project6", 640, 480, false)
    {
    }

    protected override void Initialize()
    {
        base.Initialize();
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnWindowSizeChanged;
        _renderTarget = new RenderTarget2D(GraphicsDevice, targetResolution.X, targetResolution.Y, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24);
        InitializeGum();
        UpdateViewport();
        ChangeScene(new TitleScene());
    }

    private void OnWindowSizeChanged(object sender, EventArgs e)
    {
        UpdateViewport();
    }

    private void InitializeGum()
    {
        GumService.Default.Initialize(this);
        GumService.Default.ContentLoader.XnaContentManager = Core.Content;
        FrameworkElement.KeyboardsForUiControl.Add(GumService.Default.Keyboard);
        FrameworkElement.GamePadsForUiControl.AddRange(GumService.Default.Gamepads);
        FrameworkElement.TabReverseKeyCombos.Add(
           new KeyCombo() { PushedKey = Microsoft.Xna.Framework.Input.Keys.Up });
        FrameworkElement.TabKeyCombos.Add(
           new KeyCombo() { PushedKey = Microsoft.Xna.Framework.Input.Keys.Down });
    }

    private Rectangle CalculateDestinationRectangle()
    {
        float targetAspect = targetResolution.X / (float)targetResolution.Y;
        int width = Window.ClientBounds.Width;
        int height = (int)(width / targetAspect + 0.5f);

        if (height > Window.ClientBounds.Height)
        {
            height = Window.ClientBounds.Height;
            width = (int)(height * targetAspect + 0.5f);
        }
        int x = (Window.ClientBounds.Width - width) / 2;
        int y = (Window.ClientBounds.Height - height) / 2;
        return new Rectangle(x, y, width, height);
    }

    private void UpdateViewport()
    {
        Rectangle destination = CalculateDestinationRectangle();
        GraphicsDevice.Viewport = new Viewport(destination.X, destination.Y, destination.Width, destination.Height);
    }

    protected override void Draw(GameTime gameTime)
    {
        // 设置渲染目标
        GraphicsDevice.SetRenderTarget(_renderTarget);
        GraphicsDevice.Clear(Color.Black);

        base.Draw(gameTime);

        // 重置渲染目标
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black); // 清除为黑色（黑边）

        // 将渲染目标绘制到屏幕，保持纵横比
        var destinationRect = CalculateDestinationRectangle();
        Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
        Core.SpriteBatch.Draw(_renderTarget, destinationRect, Color.White);
        Core.SpriteBatch.End();

        GumService.Default.Draw();
    }
}
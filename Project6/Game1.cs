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
    public readonly Point targetResolution = new Point(256, 224);
    private RenderTarget2D _renderTarget;

    public Game1() : base("Project6", 256, 224, false)
    {
    }

    protected override void Initialize()
    {
        base.Initialize();

        // 设置窗口可调整大小
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnWindowSizeChanged;

        // 创建渲染目标
        _renderTarget = new RenderTarget2D(
            GraphicsDevice,
            targetResolution.X,
            targetResolution.Y,
            false,
            GraphicsDevice.PresentationParameters.BackBufferFormat,
            DepthFormat.Depth24);

        InitializeGum();
        UpdateViewport();
        ChangeScene(new TitleScene());
    }

    private void OnWindowSizeChanged(object sender, EventArgs e)
    {
        // 窗口大小改变时重新计算视口
        UpdateViewport();
    }

    private void InitializeGum()
    {
        // Initialize the Gum service
        GumService.Default.Initialize(this);

        // Tell the Gum service which content manager to use.  We will tell it to
        // use the global content manager from our Core.
        GumService.Default.ContentLoader.XnaContentManager = Core.Content;

        // Register keyboard input for UI control.
        FrameworkElement.KeyboardsForUiControl.Add(GumService.Default.Keyboard);

        // Register gamepad input for Ui control.
        FrameworkElement.GamePadsForUiControl.AddRange(GumService.Default.Gamepads);

        // Customize the tab reverse UI navigation to also trigger when the keyboard
        // Up arrow key is pushed.
        FrameworkElement.TabReverseKeyCombos.Add(
           new KeyCombo() { PushedKey = Microsoft.Xna.Framework.Input.Keys.Up });

        // Customize the tab UI navigation to also trigger when the keyboard
        // Down arrow key is pushed.
        FrameworkElement.TabKeyCombos.Add(
           new KeyCombo() { PushedKey = Microsoft.Xna.Framework.Input.Keys.Down });

        // 使用目标分辨率设置Gum
        GumService.Default.CanvasWidth = targetResolution.X / 4.0f;
        GumService.Default.CanvasHeight = targetResolution.Y / 4.0f;
        GumService.Default.Renderer.Camera.Zoom = 4.0f;
    }

    private Rectangle CalculateDestinationRectangle()
    {
        // 计算保持纵横比的绘制区域
        float targetAspect = targetResolution.X / (float)targetResolution.Y;
        int width = Window.ClientBounds.Width;
        int height = (int)(width / targetAspect + 0.5f);

        if (height > Window.ClientBounds.Height)
        {
            height = Window.ClientBounds.Height;
            width = (int)(height * targetAspect + 0.5f);
        }

        // 居中显示
        int x = (Window.ClientBounds.Width - width) / 2;
        int y = (Window.ClientBounds.Height - height) / 2;

        return new Rectangle(x, y, width, height);
    }

    private void UpdateViewport()
    {
        // 更新图形设备视口以保持纵横比
        Rectangle destination = CalculateDestinationRectangle();
        GraphicsDevice.Viewport = new Viewport(
            destination.X,
            destination.Y,
            destination.Width,
            destination.Height
        );
    }

    protected override void Draw(GameTime gameTime)
    {
        // 设置渲染目标
        GraphicsDevice.SetRenderTarget(_renderTarget);
        GraphicsDevice.Clear(Color.Black);

        // 正常绘制游戏内容到渲染目标
        base.Draw(gameTime);

        // 重置渲染目标
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black); // 清除为黑色（黑边）

        // 将渲染目标绘制到屏幕，保持纵横比
        var destinationRect = CalculateDestinationRectangle();
        Core.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
        Core.SpriteBatch.Draw(_renderTarget, destinationRect, Color.White);
        Core.SpriteBatch.End();
    }
}
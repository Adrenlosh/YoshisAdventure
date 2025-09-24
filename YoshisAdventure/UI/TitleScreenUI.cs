using Gum.DataTypes;
using Gum.Forms.Controls;
using Microsoft.Xna.Framework;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using System;
using YoshisAdventure.UI.CustomControls;

namespace YoshisAdventure.UI
{
    public class TitleScreenUI : ContainerRuntime
    {
        public event EventHandler StartButtonClicked;

        private readonly Panel _titlePanel;

        public TitleScreenUI()
        {
            Dock(Gum.Wireframe.Dock.Fill);
            this.AddToRoot();

            TextRuntime titleText = new TextRuntime();
            titleText.Anchor(Gum.Wireframe.Anchor.Center);
            titleText.WidthUnits = DimensionUnitType.RelativeToChildren;
            titleText.Text = Language.Strings.GameName;
            titleText.Color = Color.Yellow;
            titleText.UseCustomFont = true;
            titleText.CustomFontFile = "Fonts/ZFull-GB.fnt";
            titleText.FontScale = 1f;

            UIButton startButton = new UIButton();
            startButton.Text = Language.Strings.Start;
            startButton.Dock(Gum.Wireframe.Dock.Bottom);
            startButton.Click += (s, e) => StartButtonClicked?.Invoke(this, EventArgs.Empty);

            UIButton startButton1 = new UIButton();
            startButton1.Text = Language.Strings.Settings;
            startButton1.Dock(Gum.Wireframe.Dock.Top);

            _titlePanel = new Panel();
            _titlePanel.Dock(Gum.Wireframe.Dock.Fill);
            _titlePanel.AddChild(startButton);
            _titlePanel.AddChild(startButton1);
            _titlePanel.AddChild(titleText);
            _titlePanel.AddToRoot();

            startButton.IsFocused = true;
        }

        public void Update(GameTime gameTime)
        {
            GumService.Default.Update(gameTime);
        }

        public void Draw(float scaleFactor = 1f)
        {
            GumService.Default.Renderer.Camera.Zoom = scaleFactor;
            GumService.Default.Draw();
        }
    }
}
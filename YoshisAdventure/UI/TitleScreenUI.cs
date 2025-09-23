using Gum.DataTypes;
using Gum.Forms.Controls;
using Microsoft.Xna.Framework;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using System;

namespace Project6.UI
{
    public class TitleScreenUI : ContainerRuntime
    {
        private static readonly string TITLE_TEXT = "Project6";

        public event EventHandler StartButtonClicked;

        private Panel _titlePanel;

        public TitleScreenUI()
        {
            Dock(Gum.Wireframe.Dock.Fill);
            this.AddToRoot();

            TextRuntime titleText = new TextRuntime();
            titleText.Anchor(Gum.Wireframe.Anchor.Center);
            titleText.WidthUnits = DimensionUnitType.RelativeToChildren;
            titleText.Text = TITLE_TEXT;
            titleText.Color = Color.Yellow;
            titleText.UseCustomFont = true;
            titleText.CustomFontFile = "Fonts/ZFull-GB.fnt";
            titleText.FontScale = 3f;

            Button startButton = new Button();
            startButton.Text = "Start Game";
            startButton.Dock(Gum.Wireframe.Dock.Bottom);
            startButton.Click += (s, e) => StartButtonClicked?.Invoke(this, EventArgs.Empty);

            _titlePanel = new Panel();
            _titlePanel.Dock(Gum.Wireframe.Dock.Fill);
            _titlePanel.AddChild(startButton);
            _titlePanel.AddChild(titleText);
            _titlePanel.AddToRoot();
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
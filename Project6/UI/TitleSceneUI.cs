using Gum.DataTypes;
using Gum.Forms.Controls;
using Microsoft.Xna.Framework;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using System;

namespace Project6.UI
{
    public class TitleSceneUI : ContainerRuntime
    {
        private static readonly string TITLE_TEXT = "Project6";

        public event EventHandler StartClicked;

        private Panel _titleScreenButtonsPanel;

        public TitleSceneUI()
        {
            Dock(Gum.Wireframe.Dock.Fill);
            this.AddToRoot();

            TextRuntime text = new TextRuntime();
            text.Anchor(Gum.Wireframe.Anchor.TopLeft);
            text.WidthUnits = DimensionUnitType.RelativeToChildren;
            text.X = 20.0f;
            text.Y = 5.0f;
            text.UseCustomFont = true;
            text.CustomFontFile = "fonts/ZFull-GB.fnt";
            text.Text = TITLE_TEXT;
            text.Color = Color.Yellow;
            text.AddToRoot();
            Button button = new Button();
            button.Text = "Start Game";
            button.X = 100f;
            button.Y = 100f;
            button.AddToRoot();
        }

        public void Update(GameTime gameTime)
        {
            UpdateLayout();
            GumService.Default.Update(gameTime);
        }

        public void Draw(float scaleFactor = 1f)
        {
            GumService.Default.Renderer.Camera.Zoom = scaleFactor;
            
            GumService.Default.Draw();
        }
    }
}
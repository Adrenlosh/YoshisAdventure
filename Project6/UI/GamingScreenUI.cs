using Gum.Forms.Controls;
using Microsoft.Xna.Framework;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using System;

namespace Project6.UI
{
    public class GamingScreenUI : ContainerRuntime
    {
        private static readonly string PLAYER_TEXT = "Player\n x {0}";
        private static readonly string EGG_TEXT = "Egg\n x {0}";
        private static readonly string SCORE_TEXT = "Score\n{0}";

        TextRuntime HPText = new TextRuntime();

        TextRuntime EggText = new TextRuntime();

        TextRuntime ScoreText = new TextRuntime();

        public event EventHandler StartButtonClicked;

        private Panel _titlePanel;

        public GamingScreenUI()
        {
            Dock(Gum.Wireframe.Dock.Fill);
            this.AddToRoot();

            
            HPText.Anchor(Gum.Wireframe.Anchor.TopLeft);
            HPText.Text = PLAYER_TEXT;
            HPText.Color = Color.DarkGreen;
            HPText.UseCustomFont = true;
            HPText.CustomFontFile = "Fonts/ZFull-GB.fnt";
            HPText.BindingContext = GameMain.playerStatus;

            
            EggText.Text = EGG_TEXT;
            EggText.Color = Color.White;
            EggText.UseCustomFont = true;
            EggText.CustomFontFile = "Fonts/ZFull-GB.fnt";
            EggText.Anchor(Gum.Wireframe.Anchor.Top);
            EggText.BindingContext = GameMain.playerStatus;

            
            ScoreText.Text = SCORE_TEXT;
            ScoreText.Color = Color.White;
            ScoreText.UseCustomFont = true;
            ScoreText.CustomFontFile = "Fonts/ZFull-GB.fnt";
            ScoreText.Anchor(Gum.Wireframe.Anchor.TopRight);

            _titlePanel = new Panel();
            _titlePanel.Dock(Gum.Wireframe.Dock.Fill);
            _titlePanel.AddChild(HPText);
            _titlePanel.AddChild(EggText);
            _titlePanel.AddChild(ScoreText);
            _titlePanel.AddToRoot();
        }

        public void Update(GameTime gameTime)
        {
            HPText.Text = string.Format(PLAYER_TEXT, GameMain.playerStatus.HP.ToString());
            EggText.Text = string.Format(EGG_TEXT, GameMain.playerStatus.Egg.ToString());
            GumService.Default.Update(gameTime);
        }

        public void Draw(float scaleFactor = 1f)
        {
            GumService.Default.Renderer.Camera.Zoom = scaleFactor;
            GumService.Default.Draw();
        }
    }
}
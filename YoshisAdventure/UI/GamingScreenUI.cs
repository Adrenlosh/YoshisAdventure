using Gum.Forms.Controls;
using Microsoft.Xna.Framework;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using System;
using YoshisAdventure.UI.CustomControls;

namespace YoshisAdventure.UI
{
    public class GamingScreenUI : ContainerRuntime
    {
        TextRuntime LifeLeftText = new TextRuntime();

        TextRuntime EggText = new TextRuntime();

        TextRuntime ScoreText = new TextRuntime();

        public bool IsReadingMessage { get; set; }

        public event EventHandler OnMessageBoxClosed;

        private Panel _titlePanel;

        private MessageBox _messageBox;

        public GamingScreenUI()
        {
            Dock(Gum.Wireframe.Dock.Fill);
            this.AddToRoot();

            LifeLeftText.Anchor(Gum.Wireframe.Anchor.TopLeft);
            LifeLeftText.Text = Language.Strings.LifeLeftOnHud;
            LifeLeftText.Color = Color.DarkGreen;
            LifeLeftText.UseCustomFont = true;
            LifeLeftText.CustomFontFile = "Fonts/ZFull-GB.fnt";
            LifeLeftText.BindingContext = GameMain.PlayerStatus;

            EggText.Text = Language.Strings.EggCountOnHud;
            EggText.Color = Color.White;
            EggText.UseCustomFont = true;
            EggText.CustomFontFile = "Fonts/ZFull-GB.fnt";
            EggText.Anchor(Gum.Wireframe.Anchor.Top);
            EggText.BindingContext = GameMain.PlayerStatus;

            ScoreText.Text = Language.Strings.ScoreOnHud;
            ScoreText.Color = Color.White;
            ScoreText.UseCustomFont = true;
            ScoreText.CustomFontFile = "Fonts/ZFull-GB.fnt";
            ScoreText.Anchor(Gum.Wireframe.Anchor.TopRight);

            _titlePanel = new Panel();
            _titlePanel.Dock(Gum.Wireframe.Dock.Fill);
            _titlePanel.AddChild(LifeLeftText);
            _titlePanel.AddChild(EggText);
            _titlePanel.AddChild(ScoreText);
            _titlePanel.AddToRoot();

            _messageBox = new MessageBox();
            _messageBox.OnClosed += () => { 
                IsReadingMessage = false; 
                OnMessageBoxClosed?.Invoke(this, EventArgs.Empty); 
            };
            _titlePanel.AddChild(_messageBox);
        }


        public void ShowMessageBox(string messageID)
        {
            IsReadingMessage = true;
            _messageBox.Show(Language.Messages.ResourceManager.GetString(messageID));
        }

        public void Update(GameTime gameTime)
        {
            LifeLeftText.Text = string.Format(Language.Strings.LifeLeftOnHud, GameMain.PlayerStatus.LifeLeft);
            EggText.Text = string.Format(Language.Strings.EggCountOnHud, GameMain.PlayerStatus.Egg);
            ScoreText.Text = string.Format(Language.Strings.ScoreOnHud, GameMain.PlayerStatus.Score);
            _messageBox.Update(gameTime);
            GumService.Default.Update(gameTime);
        }

        public void Draw(float scaleFactor = 1f)
        {
            GumService.Default.Renderer.Camera.Zoom = scaleFactor;
            GumService.Default.Draw();
        }
    }
}
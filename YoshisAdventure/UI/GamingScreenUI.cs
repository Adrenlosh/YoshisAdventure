using Microsoft.Xna.Framework;
using MLEM.Ui.Elements;
using System;
using YoshisAdventure.Enums;
using YoshisAdventure.Systems;
using YoshisAdventure.UI.CustomControls;

namespace YoshisAdventure.UI
{

    public class GamingScreenUI : Panel
    {
        private const float FadeDuration = 0.3f;
        private float _fadeTimer = -1f;
        private int _alpha;
        private TimeSpan? _remainingTime = TimeSpan.FromSeconds(1);
        private FadeStatus _fadeStatus = FadeStatus.None;
        private MessageBox _messageBox;

        private Paragraph _LifeLeftParagraph;
        private Paragraph _EggParagraph;
        private Paragraph _CoinParagraph;
        private Paragraph _ScoreParagraph;
        private Paragraph _TimeParagraph;
        private Paragraph _HealthParagraph;
        private Panel _pausePanel;

        public bool IsReadingMessage { get; set; }

        public bool IsPaused { get; set; } = false;

        public bool HandlePause { get; set; } = true;

        public event Action OnMessageBoxClosed;
        public event Action OnCancelPause;

        public GamingScreenUI() : base(MLEM.Ui.Anchor.TopLeft, new Vector2(GlobalConfig.VirtualResolution_Width, GlobalConfig.VirtualResolution_Height))
        {
            Texture = null;
            int width = (int)(Size.X / 9 * 2);
            Group paragraphs = new Group(MLEM.Ui.Anchor.TopCenter, Vector2.One) { SetHeightBasedOnChildren = true };
            _LifeLeftParagraph = new Paragraph(MLEM.Ui.Anchor.AutoInlineIgnoreOverflow, width, string.Empty) { TextColor = Color.Orange, GetTextCallback = new Paragraph.TextCallback(para => { return string.Format(Language.Strings.LifeLeftOnHud, GameMain.PlayerStatus.LifeLeft); }) };
            _EggParagraph = new Paragraph(MLEM.Ui.Anchor.AutoInlineIgnoreOverflow, width, string.Empty) { GetTextCallback = new Paragraph.TextCallback(para => { return string.Format(Language.Strings.EggCountOnHud, GameMain.PlayerStatus.Egg); }) };
            _CoinParagraph = new Paragraph(MLEM.Ui.Anchor.AutoInlineIgnoreOverflow, width, string.Empty) { GetTextCallback = new Paragraph.TextCallback(para => { return string.Format(Language.Strings.CoinOnHud, GameMain.PlayerStatus.Coin); }) };
            _ScoreParagraph = new Paragraph(MLEM.Ui.Anchor.AutoInlineIgnoreOverflow, width, string.Empty) { GetTextCallback = new Paragraph.TextCallback(para => { return string.Format(Language.Strings.ScoreOnHud, GameMain.PlayerStatus.Score); }) };
            _TimeParagraph = new Paragraph(MLEM.Ui.Anchor.AutoInlineIgnoreOverflow, width, string.Empty) { GetTextCallback = new Paragraph.TextCallback(para => { return string.Format(Language.Strings.TimeOnHud, ((int)_remainingTime.Value.TotalSeconds).ToString().PadLeft(3, '0')); }) };
            _HealthParagraph = new Paragraph(MLEM.Ui.Anchor.BottomLeft, 1, string.Empty, true) { GetTextCallback = new Paragraph.TextCallback(para => { return string.Format(Language.Strings.HealthOnHud, GameObjectsSystem.Player.Health, GameObjectsSystem.Player.MaxHealth); }) };
            paragraphs.AddChild(_LifeLeftParagraph);
            paragraphs.AddChild(_EggParagraph);
            paragraphs.AddChild(_CoinParagraph);
            paragraphs.AddChild(_ScoreParagraph);
            paragraphs.AddChild(_TimeParagraph);
            
            _pausePanel = new Panel(MLEM.Ui.Anchor.Center, Size){ IsHidden = true, DrawColor = new Color(Color.Black, 0.7f) };
            _pausePanel.AddChild(new Paragraph(MLEM.Ui.Anchor.Center, 1, Language.Strings.Paused, true));

            _messageBox = new MessageBox();
            _messageBox.OnClosed += () =>
            {
                IsReadingMessage = false;
                OnMessageBoxClosed?.Invoke();
            };

            AddChild(_pausePanel);
            AddChild(paragraphs);
            AddChild(_HealthParagraph);
            AddChild(_messageBox);
        }

        public void ShowMessageBox(string messageID)
        {
            IsReadingMessage = true;
            _messageBox.Show(Language.Messages.ResourceManager.GetString(messageID));
        }

        public void Pause()
        {
            SongSystem.Pause();
            SFXSystem.Play("pause");
            IsPaused = true;
            _fadeTimer = 0f;
            _fadeStatus = FadeStatus.In;
            _pausePanel.IsHidden = false;
        }

        public void Unpause()
        {
            SongSystem.Resume();
            SFXSystem.Play("pause");
            IsPaused = false;
            _fadeTimer = 0f;
            _fadeStatus = FadeStatus.Out;
        }

        private void HandleInput()
        {
            if (GameControllerSystem.StartPressed() && HandlePause && !IsReadingMessage)
            {
                if (IsPaused)
                {
                    Unpause();
                }
                else
                {
                    Pause();
                }
            }
        }

        public void Update(GameTime gameTime, TimeSpan? remainingTime = null)
        {
            HandleInput();
            _remainingTime = remainingTime;
            _HealthParagraph.TextColor = GameObjectsSystem.Player.Health < 2 ? Color.Red : Color.Yellow;
            _TimeParagraph.TextColor = _remainingTime.Value.TotalSeconds <= 100 ? Color.Red : Color.Yellow;
            if(IsReadingMessage)
            {
                _messageBox.Update();
            }
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_fadeTimer >= 0f)
            {
                _fadeTimer += elapsedTime;

                if (_fadeTimer >= FadeDuration)
                {
                    _fadeTimer = -1f;
                    if (_fadeStatus == FadeStatus.Out)
                    {
                        _alpha = 0;
                        _pausePanel.IsHidden = true;
                    }
                    _fadeStatus = FadeStatus.None;
                }
                else
                {
                    float progress = _fadeTimer / FadeDuration;
                    if (_fadeStatus == FadeStatus.In)
                    {
                        _alpha = (int)(255 * progress);
                    }
                    else if (_fadeStatus == FadeStatus.Out)
                    {
                        _alpha = (int)(255 * (1 - progress));
                    }
                    _pausePanel.DrawColor = new Color(Color.Black, _alpha);
                }
            }
        }
    }
}
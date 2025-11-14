using Microsoft.Xna.Framework;
using MLEM.Ui.Elements;
using System;
using YoshisAdventure.Systems;
using YoshisAdventure.UI.CustomControls;

namespace YoshisAdventure.UI
{
    enum AnimationStatus
    {
        In, Out, None
    }

    public class GamingScreenUI : Panel
    {
        private const float AnimationDuration = 0.3f;
        private float _animationTimer = -1f;
        private int _alpha;
        private TimeSpan? _remainingTime = TimeSpan.FromSeconds(1);
        private AnimationStatus _animationStatus = AnimationStatus.None;
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
            _animationTimer = 0f;
            _animationStatus = AnimationStatus.In;
            _pausePanel.IsHidden = false;
        }

        public void Unpause()
        {
            SongSystem.Resume();
            SFXSystem.Play("pause");
            IsPaused = false;
            _animationTimer = 0f;
            _animationStatus = AnimationStatus.Out;
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
            if (_animationTimer >= 0f)
            {
                _animationTimer += elapsedTime;

                if (_animationTimer >= AnimationDuration)
                {
                    _animationTimer = -1f;
                    if (_animationStatus == AnimationStatus.Out)
                    {
                        _alpha = 0;
                        _pausePanel.IsHidden = true;
                    }
                    _animationStatus = AnimationStatus.None;
                }
                else
                {
                    float progress = _animationTimer / AnimationDuration;
                    if (_animationStatus == AnimationStatus.In)
                    {
                        _alpha = (int)(255 * progress);
                    }
                    else if (_animationStatus == AnimationStatus.Out)
                    {
                        _alpha = (int)(255 * (1 - progress));
                    }
                    _pausePanel.DrawColor = new Color(Color.Black, _alpha);
                }
            }
        }
        //public GamingScreenUI(SpriteBatch spriteBatch, ContentManager content, ViewportAdapter viewportAdapter)
        //{
        //    _spriteBatch = spriteBatch;
        //    _viewportAdapter = viewportAdapter;
        //    _bitmapFont = content.Load<BitmapFont>("Fonts/ZFull-GB");

        //    _messageBox = new MessageBox(_bitmapFont);
        //    _messageBox.AutoSetPosition(_viewportAdapter);
        //    _messageBox.OnClosed += () => {
        //        IsReadingMessage = false;
        //        OnMessageBoxClosed?.Invoke();
        //    };
        //}

        //public void ShowMessageBox(string messageID)
        //{
        //    IsReadingMessage = true;
        //    _messageBox.Show(Language.Messages.ResourceManager.GetString(messageID));
        //}



        //public void Update(GameTime gameTime, TimeSpan? remainingTime = null)
        //{
        //    HandleInput();
        //    _remainingTime = remainingTime;
        //    float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        //    if (_animationTimer >= 0f)
        //    {
        //        _animationTimer += elapsedTime;

        //        if (_animationTimer >= AnimationDuration)
        //        {
        //            _animationTimer = -1f;
        //            if (_animationStatus == AnimationStatus.Out)
        //            {
        //                _alpha = 0;
        //            }
        //            _animationStatus = AnimationStatus.None;
        //        }
        //        else
        //        {
        //            float progress = _animationTimer / AnimationDuration;
        //            if (_animationStatus == AnimationStatus.In)
        //            {
        //                _alpha = (int)(230 * progress);
        //            }
        //            else if (_animationStatus == AnimationStatus.Out)
        //            {
        //                _alpha = (int)(230 * (1 - progress));
        //            }
        //        }
        //    }
        //    _messageBox.Update(gameTime);
        //}

        //private void HandleInput()
        //{
        //    if (GameControllerSystem.StartPressed() && HandlePause && !IsReadingMessage)
        //    {
        //        if (IsPaused)
        //        {
        //            Unpause();
        //        }
        //        else
        //        {
        //            Pause();
        //        }
        //    }
        //}

        //public void Draw()
        //{
        //    Matrix matrix = _viewportAdapter.GetScaleMatrix();
        //    Rectangle boundingRect = _viewportAdapter.BoundingRectangle;
        //    int denominator = 9;
        //    int paddingY = 1;
        //    int stepX = boundingRect.Width / denominator;
        //    _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: matrix);
        //    if (IsPaused || _animationStatus != AnimationStatus.None)
        //    {
        //        _spriteBatch.FillRectangle(boundingRect, new Color(Color.Black, _alpha));
        //        if (IsPaused && _animationStatus != AnimationStatus.Out)
        //        {
        //            var pauseStrSize = _bitmapFont.MeasureString(Language.Strings.Paused);
        //            Color textColor = new Color(Color.MonoGameOrange, MathHelper.Clamp(_alpha / 230f, 0f, 1f));
        //            _spriteBatch.DrawString(_bitmapFont, Language.Strings.Paused, new Vector2(boundingRect.Center.X - pauseStrSize.Width / 2, boundingRect.Center.Y - pauseStrSize.Height / 2), textColor);
        //        }
        //    }

        //    string healthStr = string.Format(Language.Strings.HealthOnHud, GameObjectsSystem.Player.Health, GameObjectsSystem.Player.MaxHealth);
        //    var healthStrSize = _bitmapFont.MeasureString(healthStr);
        //    _spriteBatch.DrawString(_bitmapFont, string.Format(Language.Strings.LifeLeftOnHud, GameMain.PlayerStatus.LifeLeft), new Vector2(1, paddingY), Color.OrangeRed);
        //    _spriteBatch.DrawString(_bitmapFont, string.Format(Language.Strings.EggCountOnHud, GameMain.PlayerStatus.Egg), new Vector2(stepX * 2, paddingY), Color.White);
        //    _spriteBatch.DrawString(_bitmapFont, string.Format(Language.Strings.CoinOnHud, GameMain.PlayerStatus.Coin), new Vector2(stepX * 4, paddingY), Color.White);
        //    _spriteBatch.DrawString(_bitmapFont, string.Format(Language.Strings.ScoreOnHud, GameMain.PlayerStatus.Score), new Vector2(stepX * 6, paddingY), Color.White);
        //    _spriteBatch.DrawString(_bitmapFont, healthStr, new Vector2(1, boundingRect.Height - 1 - healthStrSize.Height), GameObjectsSystem.Player.Health < 2 ? Color.Red : Color.Yellow);
        //    if(_remainingTime != null)
        //    {
        //        _spriteBatch.DrawString(_bitmapFont, string.Format(Language.Strings.TimeOnHud, ((int)(_remainingTime.Value.TotalSeconds)).ToString().PadLeft(3, '0')), new Vector2(stepX * 8, paddingY), _remainingTime.Value.TotalSeconds <= 100 ? Color.Red : Color.Yellow);
        //    }
        //    _messageBox.Draw(_spriteBatch);
        //    _spriteBatch.End();
        //}
    }
}
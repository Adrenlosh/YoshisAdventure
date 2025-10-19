using GameLibrary.Primitive2D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.ViewportAdapters;
using MonoGameGum;
using MonoGameGum.GueDeriving;
using System;
using YoshisAdventure.Systems;
using YoshisAdventure.UI.CustomControls;

namespace YoshisAdventure.UI
{
    enum AnimationStatus
    {
        In, Out, None
    }

    public class GamingScreenUI : ContainerRuntime
    {
        private const float AnimationDuration = 0.3f;
        private float _animationTimer = -1f;
        private int _alpha;
        private TimeSpan? _remainingTime;
        private AnimationStatus _animationStatus = AnimationStatus.None;
        private SpriteBatch _spriteBatch;
        private MessageBox _messageBox;
        private ViewportAdapter _viewportAdapter;
        private BitmapFont _bitmapFont;

        public bool IsReadingMessage { get; set; }

        public bool IsPaused { get; set; } = false;

        public bool HandlePause { get; set; } = true;

        public event Action OnMessageBoxClosed;
        public event Action OnCancelPause;

        public GamingScreenUI(SpriteBatch spriteBatch, ContentManager content, ViewportAdapter viewportAdapter)
        {
            _spriteBatch = spriteBatch;
            _viewportAdapter = viewportAdapter;
            _bitmapFont = content.Load<BitmapFont>("Fonts/ZFull-GB");

            _messageBox = new MessageBox(_bitmapFont);
            _messageBox.AutoSetPosition(_viewportAdapter);
            _messageBox.OnClosed += () => {
                IsReadingMessage = false;
                OnMessageBoxClosed?.Invoke();
            };
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
        }

        public void Unpause()
        {
            SongSystem.Resume();
            SFXSystem.Play("pause");
            IsPaused = false;
            _animationTimer = 0f;
            _animationStatus = AnimationStatus.Out;
        }

        public void Update(GameTime gameTime, TimeSpan? remainingTime = null)
        {
            HandleInput();
            _remainingTime = remainingTime;
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
                    }
                    _animationStatus = AnimationStatus.None;
                }
                else
                {
                    float progress = _animationTimer / AnimationDuration;
                    if (_animationStatus == AnimationStatus.In)
                    {
                        _alpha = (int)(230 * progress);
                    }
                    else if (_animationStatus == AnimationStatus.Out)
                    {
                        _alpha = (int)(230 * (1 - progress));
                    }
                }
            }
            _messageBox.Update(gameTime);
            GumService.Default.Update(gameTime);
        }

        private void HandleInput()
        {
            if (GameController.StartPressed() && HandlePause && !IsReadingMessage)
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

        public void Draw()
        {
            Matrix matrix = _viewportAdapter.GetScaleMatrix();
            Rectangle boundingRect = _viewportAdapter.BoundingRectangle;
            int denominator = 9;
            int paddingY = 1;
            int stepX = boundingRect.Width / denominator;
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: matrix);
            if (IsPaused || _animationStatus != AnimationStatus.None)
            {
                _spriteBatch.FillRectangle(boundingRect, new Color(Color.Black, _alpha));
                if (IsPaused && _animationStatus != AnimationStatus.Out)
                {
                    var pauseStrSize = _bitmapFont.MeasureString(Language.Strings.Paused);
                    Color textColor = new Color(Color.MonoGameOrange, MathHelper.Clamp(_alpha / 230f, 0f, 1f));
                    _spriteBatch.DrawString(_bitmapFont, Language.Strings.Paused, new Vector2(boundingRect.Center.X - pauseStrSize.Width / 2, boundingRect.Center.Y - pauseStrSize.Height / 2), textColor);
                }
            }

            string healthStr = string.Format(Language.Strings.HealthOnHud, GameObjectsSystem.Player.Health, GameObjectsSystem.Player.MaxHealth);
            var healthStrSize = _bitmapFont.MeasureString(healthStr);
            _spriteBatch.DrawString(_bitmapFont, string.Format(Language.Strings.LifeLeftOnHud, GameMain.PlayerStatus.LifeLeft), new Vector2(1, paddingY), Color.OrangeRed);
            _spriteBatch.DrawString(_bitmapFont, string.Format(Language.Strings.EggCountOnHud, GameMain.PlayerStatus.Egg), new Vector2(stepX * 2, paddingY), Color.White);
            _spriteBatch.DrawString(_bitmapFont, string.Format(Language.Strings.CoinOnHud, GameMain.PlayerStatus.Coin), new Vector2(stepX * 4, paddingY), Color.White);
            _spriteBatch.DrawString(_bitmapFont, string.Format(Language.Strings.ScoreOnHud, GameMain.PlayerStatus.Score), new Vector2(stepX * 6, paddingY), Color.White);
            _spriteBatch.DrawString(_bitmapFont, healthStr, new Vector2(1, boundingRect.Height - 1 - healthStrSize.Height), GameObjectsSystem.Player.Health < 2 ? Color.Red : Color.Yellow);
            if(_remainingTime != null)
            {
                _spriteBatch.DrawString(_bitmapFont, string.Format(Language.Strings.TimeOnHud, ((int)(_remainingTime.Value.TotalSeconds)).ToString().PadLeft(3, '0')), new Vector2(stepX * 8, paddingY), _remainingTime.Value.TotalSeconds <= 100 ? Color.Red : Color.Yellow);
            }
            _messageBox.Draw(_spriteBatch);
            _spriteBatch.End();
        }
    }
}
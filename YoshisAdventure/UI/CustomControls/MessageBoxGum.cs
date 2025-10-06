using Microsoft.Xna.Framework;
using MonoGameGum.GueDeriving;
using System;
using System.Collections.Generic;

namespace YoshisAdventure.UI.CustomControls
{
    public class MessageBoxGum : ColoredRectangleRuntime
    {
        private TextRuntime _messageText = new TextRuntime();
        private UIButton _nextButton = new UIButton();

        private List<string> _pages = new List<string>();
        private int _currentPage = 0;

        private bool _isAnimating = false;
        private bool _isOpening = false;
        private float _animationProgress = 0f;
        private const float AnimationDuration = 0.3f;
        private float _baseWidth = 190;
        private float _baseHeight = 130;

        public event Action OnClosed;

        public MessageBoxGum()
        {
            Width = _baseWidth;
            Height = _baseHeight;
            Color = Color.Black;
            Anchor(Gum.Wireframe.Anchor.Center);
            Visible = false;

            _messageText.UseCustomFont = true;
            _messageText.CustomFontFile = "Fonts/ZFull-GB.fnt";
            _messageText.Color = Color.White;
            _messageText.Dock(Gum.Wireframe.Dock.Fill);
            _messageText.HorizontalAlignment = RenderingLibrary.Graphics.HorizontalAlignment.Left;
            _messageText.VerticalAlignment = RenderingLibrary.Graphics.VerticalAlignment.Top;
            AddChild(_messageText);

            _nextButton.Dock(Gum.Wireframe.Dock.Bottom);
            _nextButton.Click += (s, e) => OnNextButtonClicked();
            AddChild(_nextButton.Visual);

            Alpha = 0;
            _messageText.Alpha = 0;
            _nextButton.Alpha = 0;
        }

        public void Show(string text, int maxCharsPerPage = 350)
        {
            _pages = Paginate(text, maxCharsPerPage);
            _currentPage = 0;
            _messageText.Text = _pages[_currentPage];
            Visible = true;
            _isAnimating = true;
            _isOpening = true;
            _animationProgress = 0f;
            Alpha = 0;
            _messageText.Alpha = 0;
            _nextButton.Alpha = 0;
            if (_pages.Count == 1)
            {
                _nextButton.Text = Language.Strings.Close;
            }
            else
            {
                _nextButton.Text = Language.Strings.NextPage;
            }
        }

        private List<string> Paginate(string text, int maxChars)
        {
            if (string.IsNullOrEmpty(text)) return new List<string> { "" };

            var pages = new List<string>();
            var currentPage = "";

            foreach (var word in text.Split(' '))
            {
                if ((currentPage + word).Length <= maxChars)
                {
                    currentPage += (currentPage == "" ? "" : " ") + word;
                }
                else
                {
                    if (!string.IsNullOrEmpty(currentPage))
                        pages.Add(currentPage);
                    if (word.Length > maxChars)
                    {
                        for (int i = 0; i < word.Length; i += maxChars)
                            pages.Add(word.Substring(i, Math.Min(maxChars, word.Length - i)));
                        currentPage = "";
                    }
                    else
                    {
                        currentPage = word;
                    }
                }
            }

            if (!string.IsNullOrEmpty(currentPage))
                pages.Add(currentPage);

            return pages.Count == 0 ? new List<string> { text } : pages;
        }

        private void OnNextButtonClicked()
        {
            if (_currentPage < _pages.Count - 1)
            {
                _currentPage++;
                _messageText.Text = _pages[_currentPage];
                if (_currentPage == _pages.Count - 1)
                    _nextButton.Text = Language.Strings.Close;
            }
            else
            {
                _isAnimating = true;
                _isOpening = false;
                _animationProgress = 1f;
            }
        }

        public void Update(GameTime gameTime)
        {
            if (_isAnimating)
            {
                float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_isOpening)
                {
                    _animationProgress += elapsedTime / AnimationDuration;
                    if (_animationProgress >= 1f)
                    {
                        _animationProgress = 1f;
                        _isAnimating = false;
                    }
                }
                else
                {
                    _animationProgress -= elapsedTime / AnimationDuration;
                    if (_animationProgress <= 0f)
                    {
                        _animationProgress = 0f;
                        _isAnimating = false;
                        Visible = false;
                        OnClosed?.Invoke();
                    }
                }
                byte alphaValue = (byte)(255 * _animationProgress);
                Alpha = alphaValue;
                _messageText.Alpha = alphaValue;
                _nextButton.Alpha = alphaValue;
            }
            else
            {
                this.Alpha = 255;
                _messageText.Alpha = 255;
                _nextButton.Alpha = 255;
            }
        }
    }
}

using Microsoft.Xna.Framework;
using MLEM.Ui;
using MLEM.Ui.Elements;
using System;
using YoshisAdventure.Systems;

namespace YoshisAdventure.UI.CustomControls
{
    //enum MessageBoxStatus
    //{
    //    Opening, Opened, Closing, Closed
    //}
    public class MessageBox : Panel
    {
        private Paragraph paragraph;
        private Button button;
        public Action OnClosed;

        public MessageBox() : base(Anchor.Center, new Vector2(180, 140))
        {
            IsHidden = true;
            paragraph = new Paragraph(Anchor.TopCenter, 1, string.Empty, false);
            button = new Button(Anchor.BottomCenter, new Vector2(64, 16), Language.Strings.Close);
            button.OnPressed += (b) =>
            {
                IsHidden = true;
                OnClosed?.Invoke();
            };
            AddChild(paragraph);
            AddChild(button);
        }

        public void Show(string msg)
        {
            paragraph.Text = msg;
            IsHidden = false;
        }

        public void Update()
        {
            if(GameControllerSystem.StartPressed())
            {
                button.OnPressed(this);
            }
        }
    }
}
    //using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using MonoGame.Extended;
//using MonoGame.Extended.BitmapFonts;
//using MonoGame.Extended.ViewportAdapters;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using YoshisAdventure.Systems;

//namespace YoshisAdventure.UI.CustomControls
//{
//    enum MessageBoxStatus
//    {
//        Opening, Opened, Closing, Closed
//    }

//    public class MessageBox
//    {
//        private const float AnimationDuration = 0.3f;
//        private float _animationTimer = -1f;
//        private int _alpha = 0;
//        private int _currentPageIndex = 0;
//        private int _padding = 2;
//        private BitmapFont _bitmapFont;
//        private List<string> _pages = new List<string>();
//        private MessageBoxStatus _status = MessageBoxStatus.Closed;

//        public Point Position { get; set; } = new Point(1);

//        public Point Size { get; set; } = new Point(150, 100);

//        public event Action OnClosed;

//        public MessageBox(BitmapFont font)
//        {
//            _bitmapFont = font;
//        }

//        public void HandleInput(GameTime gameTime)
//        {
//            if (GameControllerSystem.ActionPressed())
//            {
//                if (_status == MessageBoxStatus.Opened)
//                {
//                    if (_currentPageIndex < _pages.Count - 1)
//                    {
//                        _currentPageIndex++;
//                    }
//                    else
//                    {
//                        Close();
//                    }
//                }
//            }
//        }

//        public void Update(GameTime gameTime)
//        {
//            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
//            HandleInput(gameTime);

//            if (_animationTimer >= 0f)
//            {
//                _animationTimer += elapsedTime;

//                if (_animationTimer >= AnimationDuration)
//                {
//                    _animationTimer = -1f;

//                    if (_status == MessageBoxStatus.Opening)
//                    {
//                        _status = MessageBoxStatus.Opened;
//                        _alpha = 255;
//                    }
//                    else if (_status == MessageBoxStatus.Closing)
//                    {
//                        _status = MessageBoxStatus.Closed;
//                        _alpha = 0;
//                        OnClosed?.Invoke();
//                    }
//                }
//                else
//                {
//                    float progress = _animationTimer / AnimationDuration;

//                    switch (_status)
//                    {
//                        case MessageBoxStatus.Opening:
//                            _alpha = (int)(255 * progress);
//                            break;

//                        case MessageBoxStatus.Closing:
//                            _alpha = 255 - (int)(255 * progress);
//                            break;
//                    }
//                }
//            }
//        }

//        public void Draw(SpriteBatch spriteBatch)
//        {
//            if (_status != MessageBoxStatus.Closed && _alpha > 0)
//            {
//                Color backgroundColor = new Color(Color.Black, _alpha);
//                Color textColor = new Color(Color.White, _alpha);
//                RectangleF rect = new RectangleF(Position.ToVector2(), Size.ToVector2());
//                spriteBatch.FillRectangle(rect, backgroundColor);
//                if (_pages.Count > 0 && _currentPageIndex < _pages.Count)
//                {
//                    string currentPageText = _pages[_currentPageIndex];

//                    if (_pages.Count > 1)
//                    {
//                        currentPageText += $"\n\n({_currentPageIndex + 1}/{_pages.Count})";
//                    }

//                    DrawWrappedText(spriteBatch, currentPageText,
//                        new Vector2(Position.X + _padding, Position.Y + _padding),
//                        Size.X - _padding * 2, textColor);
//                }
//            }
//        }

//        public void Show(string messageText)
//        {
//            _status = MessageBoxStatus.Opening;
//            _animationTimer = 0f;
//            _alpha = 0;
//            ProcessTextPages(messageText);
//            _currentPageIndex = 0;
//        }

//        public void Close()
//        {
//            if (_status == MessageBoxStatus.Opened || _status == MessageBoxStatus.Opening)
//            {
//                _status = MessageBoxStatus.Closing;
//                _animationTimer = 0f;
//            }
//        }

//        public void AutoSetPosition(ViewportAdapter viewportAdapter)
//        {
//            var boundingRect = viewportAdapter.BoundingRectangle;
//            Position = new Point(boundingRect.Center.X - Size.X / 2, boundingRect.Center.Y - Size.Y / 2);
//        }

//        /// <summary>
//        /// 处理文本分页
//        /// </summary>
//        private void ProcessTextPages(string text)
//        {
//            _pages.Clear();

//            // 计算每页可容纳的最大行数
//            int maxLinesPerPage = CalculateMaxLinesPerPage();

//            // 将文本分割成单词
//            string[] words = text.Split(' ');
//            List<string> lines = new List<string>();
//            StringBuilder currentLine = new StringBuilder();

//            // 构建行
//            foreach (string word in words)
//            {
//                // 测试添加这个词后行的宽度
//                string testLine = currentLine.Length > 0 ?
//                    currentLine.ToString() + " " + word : word;

//                Vector2 lineSize = _bitmapFont.MeasureString(testLine);

//                if (lineSize.X <= Size.X - _padding * 2)
//                {
//                    // 可以添加到当前行
//                    if (currentLine.Length > 0)
//                        currentLine.Append(" ");
//                    currentLine.Append(word);
//                }
//                else
//                {
//                    // 开始新行
//                    if (currentLine.Length > 0)
//                    {
//                        lines.Add(currentLine.ToString());
//                        currentLine.Clear();
//                    }

//                    // 如果单个词就超过宽度，强制分割
//                    Vector2 wordSize = _bitmapFont.MeasureString(word);
//                    if (wordSize.X > Size.X - _padding * 2)
//                    {
//                        // 长单词需要分割
//                        string remainingWord = word;
//                        while (remainingWord.Length > 0)
//                        {
//                            int charsThatFit = CalculateCharsThatFit(remainingWord, Size.X - _padding * 2);
//                            lines.Add(remainingWord.Substring(0, charsThatFit));
//                            remainingWord = remainingWord.Substring(charsThatFit);
//                        }
//                    }
//                    else
//                    {
//                        currentLine.Append(word);
//                    }
//                }
//            }

//            // 添加最后一行
//            if (currentLine.Length > 0)
//            {
//                lines.Add(currentLine.ToString());
//            }

//            // 将行分页
//            for (int i = 0; i < lines.Count; i += maxLinesPerPage)
//            {
//                int linesToTake = Math.Min(maxLinesPerPage, lines.Count - i);
//                string[] pageLines = new string[linesToTake];
//                Array.Copy(lines.ToArray(), i, pageLines, 0, linesToTake);
//                _pages.Add(string.Join("\n", pageLines));
//            }

//            // 如果没有内容，至少添加一个空页
//            if (_pages.Count == 0)
//            {
//                _pages.Add("");
//            }
//        }

//        /// <summary>
//        /// 计算每页最多可以容纳多少行文本
//        /// </summary>
//        private int CalculateMaxLinesPerPage()
//        {
//            float lineHeight = _bitmapFont.LineHeight;
//            float availableHeight = Size.Y - _padding * 2;
//            return (int)(availableHeight / lineHeight);
//        }

//        /// <summary>
//        /// 计算在给定宽度内可以容纳多少个字符
//        /// </summary>
//        private int CalculateCharsThatFit(string text, float maxWidth)
//        {
//            for (int i = 1; i <= text.Length; i++)
//            {
//                Vector2 size = _bitmapFont.MeasureString(text.Substring(0, i));
//                if (size.X > maxWidth)
//                {
//                    return i - 1;
//                }
//            }
//            return text.Length;
//        }

//        /// <summary>
//        /// 绘制自动换行的文本
//        /// </summary>
//        private void DrawWrappedText(SpriteBatch spriteBatch, string text, Vector2 position, float maxWidth, Color color)
//        {
//            string[] lines = text.Split('\n');
//            float lineHeight = _bitmapFont.LineHeight;
//            float y = position.Y;

//            foreach (string line in lines)
//            {
//                spriteBatch.DrawString(_bitmapFont, line, new Vector2(position.X, y), color);
//                y += lineHeight;
//            }
//        }
//    }
//}
using Microsoft.Xna.Framework;
using MLEM.Ui.Elements;
using System;

namespace YoshisAdventure.UI
{
    public class TitleScreenUI : Panel
    {
        public event EventHandler StartButtonClicked;

        public TitleScreenUI() : base(MLEM.Ui.Anchor.TopLeft, new Vector2(GlobalConfig.VirtualResolution_Width, GlobalConfig.VirtualResolution_Height))
        {
            Texture = null;
            Button button = new Button(MLEM.Ui.Anchor.BottomCenter, new Vector2(1, 20), Language.Strings.Start)
            {
                OnPressed = (b) => StartButtonClicked?.Invoke(this, EventArgs.Empty)
            };
            AddChild(button);

            Paragraph title = new Paragraph(MLEM.Ui.Anchor.Center, Size.X, Language.Strings.GameName, false);
            title.Alignment = new MLEM.Ui.Style.StyleProp<MLEM.Formatting.TextAlignment>(MLEM.Formatting.TextAlignment.Center);
            AddChild(title);
            //Dock(Gum.Wireframe.Dock.Fill);
            //this.AddToRoot();

            //TextRuntime titleText = new TextRuntime();
            //titleText.Anchor(Gum.Wireframe.Anchor.Center);
            //titleText.WidthUnits = DimensionUnitType.RelativeToChildren;
            //titleText.Text = Language.Strings.GameName;
            //titleText.Color = Color.Yellow;
            //titleText.UseCustomFont = true;
            //titleText.CustomFontFile = "Fonts/ZFull-GB.fnt";
            //titleText.FontScale = 1f;

            //UIButton startButton = new UIButton();
            //startButton.Text = Language.Strings.Start;
            //startButton.Dock(Gum.Wireframe.Dock.Bottom);
            //startButton.Click += (s, e) => StartButtonClicked?.Invoke(this, EventArgs.Empty);

            //UIButton startButton1 = new UIButton();
            //startButton1.Text = Language.Strings.Settings;
            //startButton1.Dock(Gum.Wireframe.Dock.Top);

            //_titlePanel = new Panel();
            //_titlePanel.Dock(Gum.Wireframe.Dock.Fill);
            //_titlePanel.AddChild(startButton);
            //_titlePanel.AddChild(startButton1);
            //_titlePanel.AddChild(titleText);
            //_titlePanel.AddToRoot();

            //startButton.IsFocused = true;
        }
    }
}
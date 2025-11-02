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
            title.TextColor = Color.Orange;
            AddChild(title);
        }
    }
}
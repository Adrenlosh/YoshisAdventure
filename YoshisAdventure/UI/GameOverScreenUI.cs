using Microsoft.Xna.Framework;
using MLEM.Ui.Elements;
using System;

namespace YoshisAdventure.UI
{
    public class GameOverScreenUI : Panel
    {
        public event EventHandler ContinueButtonClicked;
        public event EventHandler TitleMenuButtonClicked;
        public GameOverScreenUI() : base(MLEM.Ui.Anchor.TopLeft, new Vector2(GlobalConfig.VirtualResolution_Width, GlobalConfig.VirtualResolution_Height))
        {
            Texture = null;
            Paragraph gameOverParagraph = new Paragraph(MLEM.Ui.Anchor.AutoCenter, 1, Language.Strings.GameOver, true) { TextColor = Color.LightGreen };
            Group group = new Group(MLEM.Ui.Anchor.Center, Size);
            Button continueButton = new Button(MLEM.Ui.Anchor.AutoCenter, new Vector2(180, 16), Language.Strings.Continue)
            {
                OnPressed = (b) => ContinueButtonClicked?.Invoke(this, EventArgs.Empty)
            };

            Button titleMenuButton = new Button(MLEM.Ui.Anchor.AutoCenter, new Vector2(180, 16), Language.Strings.BackToTitleMenu)
            {
                OnPressed = (b) => TitleMenuButtonClicked?.Invoke(this, EventArgs.Empty)
            };
            group.AddChild(gameOverParagraph);
            group.AddChild(new VerticalSpace(30));
            group.AddChild(continueButton);
            group.AddChild(new VerticalSpace(10));
            group.AddChild(titleMenuButton);

            AddChild(group);
        }
    }
}
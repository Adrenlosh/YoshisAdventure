using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum;
using MonoGameLibrary;
using MonoGameLibrary.Scenes;

namespace Project6.Scenes
{
    public class TitleScene : Scene
    {
        private SpriteFont _font;

        private int _sec = 0;
        public override void Initialize()
        {
            base.Initialize();
        }

        public override void LoadContent()
        {
            _font = Content.Load<SpriteFont>("fonts/SMW2");
        }

        public override void Update(GameTime gameTime)
        {
            _sec++;
            if(_sec >= 60)
            {
                Core.ChangeScene(new GamingScene());
            }
        }

        public override void Draw(GameTime gameTime)
        {
            Core.GraphicsDevice.Clear(Color.CornflowerBlue);
            Core.SpriteBatch.Begin();
            Core.SpriteBatch.DrawString(_font, "Project6", new Vector2(0, 0), Color.Red);
            Core.SpriteBatch.DrawString(_font, "Will start game in 2 sec.", new Vector2(00, 50), Color.Black);
            Core.SpriteBatch.DrawString(_font, _sec.ToString(), new Vector2(0, 100), Color.Black);
            Core.SpriteBatch.End();
            GumService.Default.Draw();
        }
    }
}
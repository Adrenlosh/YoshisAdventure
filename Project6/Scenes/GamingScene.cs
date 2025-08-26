using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameGum;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Scenes;
using Project6.GameObjects;

namespace Project6.Scenes
{
    public class GamingScene : Scene
    {
        private SpriteFont _font;

        private Yoshi _yoshi;

        private Tilemap _tilemap;

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void LoadContent()
        {
            TextureAtlas atlas = TextureAtlas.FromFile(Core.Content, "images/atlas-definition.xml");
            _font = Content.Load<SpriteFont>("fonts/Roboto");


            
            _tilemap = Tilemap.FromFile(Content, "images/tilemap-definition.xml");
            AnimatedSprite yoshiAnimatedSprite = atlas.CreateAnimatedSprite("yoshi-standing-animation");
            _yoshi = new Yoshi(yoshiAnimatedSprite, _tilemap);
        }

        public override void Update(GameTime gameTime)
        {
            _yoshi.Update(gameTime);    
        }

        public override void Draw(GameTime gameTime)
        {
            Core.GraphicsDevice.Clear(Color.CornflowerBlue);
            Core.SpriteBatch.Begin();
            _tilemap.Draw(Core.SpriteBatch);
            //Core.SpriteBatch.DrawString(_font, "Gaming Scene", new Vector2(100, 100), Color.Green);
            _yoshi.Draw();
            Core.SpriteBatch.End();
            GumService.Default.Draw();
        }
    }
}

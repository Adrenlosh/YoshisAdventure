//using Gum.Forms.Controls;
//using Gum.Graphics.Animation;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Content;
//using MonoGame.Extended.Graphics;
//using MonoGameGum;
//using MonoGameGum.GueDeriving;

//namespace YoshisAdventure.UI
//{
//    public class MapScreenUI : ContainerRuntime
//    {
//        private ContentManager _content;
//        private Panel _panel;
//        private SpriteRuntime _spriteRunTime;
//        private TextRuntime _stageNameTextRunTime;
//        private ColoredRectangleRuntime _coloredRectangleRuntime;

//        public MapScreenUI(ContentManager content)
//        {
//            _content = content;
//            Dock(Gum.Wireframe.Dock.Fill);
//            this.AddToRoot();

//            var texture = _content.Load<Microsoft.Xna.Framework.Graphics.Texture2D>("Atlas/yoshi");
//            var atlas = Texture2DAtlas.Create("TextureAtlas//yoshi", texture, 21, 31);

//            AnimationChain animationChain = new AnimationChain();
//            animationChain.Name = "Yoshi";
//            int[] frames = [0, 8, 0, 7];
//            foreach (int index in frames)
//            {
//                var textureRegion = atlas.GetRegion(index);
//                AnimationFrame frame = new AnimationFrame
//                {
//                    TopCoordinate = textureRegion.TopUV,
//                    BottomCoordinate = textureRegion.BottomUV,
//                    LeftCoordinate = textureRegion.LeftUV,
//                    RightCoordinate = textureRegion.RightUV,
//                    FrameLength = 0.3f,
//                    Texture = texture
//                };
//                animationChain.Add(frame);
//            }

//            _spriteRunTime = new SpriteRuntime();
//            _spriteRunTime.Width = 210;
//            _spriteRunTime.Height = 310;
//            _spriteRunTime.TextureHeightScale = 1;
//            _spriteRunTime.TextureWidthScale = 1;
//            _spriteRunTime.Animate = true;
//            _spriteRunTime.TextureAddress = Gum.Managers.TextureAddress.EntireTexture;
//            _spriteRunTime.AnimationChains = [animationChain];
//            _spriteRunTime.CurrentChainName = animationChain.Name;

//            _stageNameTextRunTime = new TextRuntime();
//            _stageNameTextRunTime.Text = "114514";
//            _stageNameTextRunTime.UseCustomFont = true;
//            _stageNameTextRunTime.CustomFontFile = "Fonts/ZFull-GB.fnt";

//            _coloredRectangleRuntime = new ColoredRectangleRuntime();
//            _coloredRectangleRuntime.Color = new Color(Color.Gray, 150);
//            _coloredRectangleRuntime.Dock(Gum.Wireframe.Dock.Fill);

//            _panel = new Panel();
//            _panel.X = 0;
//            _panel.Y = 0;
//            _panel.Width = GlobalConfig.VirtualResolution_Width;
//            _panel.Height = 80;
//            _panel.Height = 90;
//            _panel.AddToRoot();


//            _coloredRectangleRuntime.AddChild(_stageNameTextRunTime);
//            _coloredRectangleRuntime.AddChild(_spriteRunTime);
//            _panel.AddChild(_coloredRectangleRuntime);
//        }

//        public void Update(GameTime gameTime)
//        {
//            GumService.Default.Update(gameTime);
//        }

//        public void Draw(float scaleFactor = 1f)
//        {
//            GumService.Default.Renderer.Camera.Zoom = scaleFactor;
//            GumService.Default.Draw();
//        }
//    }
//}
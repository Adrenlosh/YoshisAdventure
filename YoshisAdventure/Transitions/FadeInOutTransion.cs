using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Screens.Transitions;
using YoshisAdventure.Enums;

namespace YoshisAdventure.Transitions
{

    public class FadeInOutTransition : Transition
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly SpriteBatch _spriteBatch;

        public Color Color { get; }

        public TransitionType Type { get; }

        public FadeInOutTransition(GraphicsDevice graphicsDevice, Color color, TransitionType type, float duration = 1f)
            : base(duration)
        {
            Color = color;
            Type = type;
            _graphicsDevice = graphicsDevice;
            _spriteBatch = new SpriteBatch(graphicsDevice);
        }

        public override void Dispose()
        {
            _spriteBatch.Dispose();
        }

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);

            if ((Type == TransitionType.Out && State == TransitionState.Out) ||
                (Type == TransitionType.In && State == TransitionState.In))
            {
                _spriteBatch.FillRectangle(0f, 0f, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height, Color * Value);
            }

            _spriteBatch.End();
        }
    }
}
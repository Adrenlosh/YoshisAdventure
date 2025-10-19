using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Screens.Transitions;

namespace YoshisAdventure.Transitions
{
    public enum FadeType
    {
        In,
        Out
    }

    public class FadeInOutTransition : Transition
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly SpriteBatch _spriteBatch;

        public Color Color { get; }

        public FadeType Type { get; }

        public FadeInOutTransition(GraphicsDevice graphicsDevice, Color color, FadeType type, float duration = 1f)
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

            if ((Type == FadeType.Out && base.State == TransitionState.Out) ||
                (Type == FadeType.In && base.State == TransitionState.In))
            {
                _spriteBatch.FillRectangle(0f, 0f, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height, Color * base.Value);
            }

            _spriteBatch.End();
        }
    }
}
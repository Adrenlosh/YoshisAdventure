using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Screens.Transitions;

namespace YoshisAdventure.Transitions;

public class FadeOutTransition : Transition
{
    private readonly GraphicsDevice _graphicsDevice;

    private readonly SpriteBatch _spriteBatch;

    public Color Color { get; }

    public FadeOutTransition(GraphicsDevice graphicsDevice, Color color, float duration = 1f)
        : base(duration)
    {
        Color = color;
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
        if (base.State == TransitionState.Out)
        {
            _spriteBatch.FillRectangle(0f, 0f, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height, Color * base.Value);
        }
        _spriteBatch.End();
    }
}
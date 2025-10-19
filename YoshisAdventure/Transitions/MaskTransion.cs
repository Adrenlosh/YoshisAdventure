using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using YoshisAdventure.Transitions;

namespace MonoGame.Extended.Screens.Transitions;

public class MaskTransition : Transition
{
    private readonly GraphicsDevice _graphicsDevice;

    private readonly SpriteBatch _spriteBatch;

    private readonly Texture2D _mask;

    public FadeType Type{ get; }

    public MaskTransition(GraphicsDevice graphicsDevice,  ContentManager content, FadeType type, float duration = 1f) : base(duration)
    {
        Type = type;
        _mask = content.Load<Texture2D>("Images/mask");
        _graphicsDevice = graphicsDevice;
        _spriteBatch = new SpriteBatch(graphicsDevice);
    }

    public override void Dispose()
    {
        _spriteBatch.Dispose();
    }

    public override void Draw(GameTime gameTime)
    {
        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
        if ((Type == FadeType.Out && base.State == TransitionState.Out) || (Type == FadeType.In && base.State == TransitionState.In))
        {
            if (Value < 0.98)
            {
                float screenDiagonal = new Vector2(_graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height).Length();
                float maskDiagonal = new Vector2(_mask.Width, _mask.Height).Length();
                float maxScale = screenDiagonal / maskDiagonal * 60f;
                _spriteBatch.Draw(_mask,
                    new Vector2(_graphicsDevice.Viewport.Width / 2,
                    _graphicsDevice.Viewport.Height / 2),
                    null,
                    Color.White,
                    0f,
                    new Vector2(_mask.Width, _mask.Height) / 2,
                    (1 - base.Value) * maxScale,
                    SpriteEffects.None,
                    0);
            }
            else
            {
                _spriteBatch.FillRectangle(0f, 0f, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height, Color.Black);
            }
        }
        _spriteBatch.End();
    }
}
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using YoshisAdventure.Enums;
using YoshisAdventure.Transitions;

namespace MonoGame.Extended.Screens.Transitions;

public class MaskTransition : Transition
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly SpriteBatch _spriteBatch;
    private readonly Effect _mask;

    public TransitionType Type { get; }

    public Vector2 Center { get; set; }

    public MaskTransition(GraphicsDevice graphicsDevice, ContentManager content, TransitionType type, float duration = 1f)
        : base(duration)
    {
        Type = type;
        _mask = content.Load<Effect>("Effects/circleMask");
        _graphicsDevice = graphicsDevice;
        _spriteBatch = new SpriteBatch(graphicsDevice);
        //Center = new Vector2(_graphicsDevice.Viewport.Width / 2f, _graphicsDevice.Viewport.Height / 2f);
    }

    public MaskTransition(GraphicsDevice graphicsDevice, ContentManager content, TransitionType type, Vector2 center, float duration = 1f)
        : this(graphicsDevice, content, type, duration)
    {
        Center = center;
    }

    public override void Dispose()
    {
        _spriteBatch.Dispose();
    }

    public override void Draw(GameTime gameTime)
    {
        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, effect:_mask);

        if ((Type.HasFlag(TransitionType.Out) && State == TransitionState.Out) ||
            (Type.HasFlag(TransitionType.In) && State == TransitionState.In))
        {
            _mask.Parameters["Radius"].SetValue(1 - Value);
            _mask.Parameters["AspectRatio"].SetValue((float)_graphicsDevice.Viewport.AspectRatio);
            _spriteBatch.FillRectangle(0f, 0f, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height, Color.Black);
        }
        _spriteBatch.End();
    }
}
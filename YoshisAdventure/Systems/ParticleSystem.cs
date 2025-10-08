using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Particles;
using MonoGame.Extended.Particles.Data;
using MonoGame.Extended.Particles.Modifiers;
using MonoGame.Extended.Particles.Modifiers.Interpolators;
using MonoGame.Extended.Particles.Profiles;

namespace YoshisAdventure.Systems
{
    public class ParticleSystem
    {
        private SpriteBatch _spriteBatch;
        private GraphicsDevice _graphicsDevice;
        private ParticleEffect _particleEffect;
        private Texture2D _particleTexture;

        public ParticleEffect ParticleEffect => _particleEffect;

        public ParticleSystem(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = new SpriteBatch(graphicsDevice);
            _particleTexture = new Texture2D(graphicsDevice, 1, 1);
            _particleTexture.SetData([Color.White]);
            CreateParticleEffect();
        }

        private void CreateParticleEffect()
        {
            Vector2 viewportCenter = _graphicsDevice.Viewport.Bounds.Center.ToVector2();

            // 创建主效果容器
            _particleEffect = new ParticleEffect("Fire")
            {
                Position = viewportCenter,

                // 自动触发粒子发射器
                AutoTrigger = true,

                // 每 0.1 秒发射一次粒子
                AutoTriggerFrequency = 0.1f
            };

            // 创建实际制造粒子的发射器。
            // 容量为 2000
            ParticleEmitter emitter = new ParticleEmitter(3000)
            {
                Name = "Fire Emitter",

                // 此发射器创建的每个粒子存活 2 秒
                LifeSpan = 2.0f,
                TextureRegion = new Texture2DRegion(_particleTexture),

                // 使用喷射配置文件 - 粒子在锥形方向发射
                Profile = Profile.Spray(-Vector2.UnitY, 1.0f),

                // 设置粒子创建时的外观
                Parameters = new ParticleReleaseParameters
                {
                    // 每次释放 10-20 个粒子
                    Quantity = new ParticleInt32Parameter(2, 4),

                    // 随机速度介于 10-40 之间
                    Speed = new ParticleFloatParameter(10.0f, 40.0f),

                    // 红色 使用 HSL 值 (色相=0°, 饱和度=100%, 亮度=60%)
                    //Color = new ParticleColorParameter(new Vector3(0.0f, 1.0f, 0.6f)),

                    // 将它们放大 10 倍
                    Scale = new ParticleVector2Parameter(new Vector2(1f, 1f))
                }
            };

            // 添加类似火焰的行为
            emitter.Modifiers.Add(new LinearGravityModifier
            {
                // 指向上方（负 Y）
                Direction = -Vector2.UnitY,

                // 使用此力使火焰上升
                Strength = 100f
            });

            // 使粒子随着年龄增长而淡出
            emitter.Modifiers.Add(new AgeModifier
            {
                Interpolators =
        {
            new OpacityInterpolator
            {
                // 开始时完全可见
                StartValue = 1.0f,

                // 在生命周期内淡化为透明
                EndValue = 0.0f
            }
        }
            });

            // 将发射器添加到我们的效果中
            _particleEffect.Emitters.Add(emitter);
        }

        public void Draw(OrthographicCamera camera)
        {
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: camera.GetViewMatrix());
            //_spriteBatch.Draw(_particleEffect);
            _spriteBatch.End();
        }

        public void Update(GameTime gameTime)
        {
            //_particleEffect.Update(gameTime);
        }
    }
}
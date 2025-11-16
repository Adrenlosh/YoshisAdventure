using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using YoshisAdventure.Enums;
using YoshisAdventure.GameObjects;
using YoshisAdventure.Models;

namespace YoshisAdventure.Systems
{
    public struct PhysicsResult
    {
        public bool HasHorizontalCollision { get; set; }

        public bool HasVerticalCollision { get; set; }

        public bool IsOnGround { get; set; }

        public CollisionDirection CollisionDirection { get; set; }
    }

    public class PhysicsConfig
    {
        public float Gravity { get; set; } = PhysicsSystem.DefaultGravity;

        public float MaxGravity { get; set; } = PhysicsSystem.DefaultMaxGravity;

        public float Friction { get; set; } = PhysicsSystem.DefaultFriction;

        public bool EnableGravity { get; set; } = true;

        public bool EnableFriction { get; set; } = true;

        public bool EnableCollision { get; set; } = true;
    }

    public class PhysicsSystem
    {
        private static PhysicsSystem _instance;
        private Dictionary<GameObject, PhysicsConfig> _particularConfig = new Dictionary<GameObject, PhysicsConfig>();

        public const float DefaultGravity = 0.5f;
        public const float DefaultMaxGravity = 8f;
        public const float DefaultFriction = 0.4f;

        public float Gravity { get; set; } = DefaultGravity;

        public float MaxGravity { get; set; } = DefaultMaxGravity;

        public float Friction { get; set; } = DefaultFriction;

        public bool EnableCollision { get; set; } = true;

        public static PhysicsSystem Instance => _instance ??= new PhysicsSystem();

        public void Register(GameObject obj, PhysicsConfig config = null)
        {
            _particularConfig[obj] = config ?? new PhysicsConfig();
        }

        public void Unregister(GameObject obj)
        {
            _particularConfig.Remove(obj);
        }

        public PhysicsResult Apply(GameObject obj, GameTime gameTime)
        {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (!_particularConfig.TryGetValue(obj, out PhysicsConfig config))
            {
                config = new PhysicsConfig();
            }
            PhysicsResult result = new PhysicsResult();
            if (config.EnableGravity && !obj.IsOnGround)
            {
                obj.Velocity = new Vector2(obj.Velocity.X, MathHelper.Clamp(obj.Velocity.Y + config.Gravity, -config.MaxGravity, config.MaxGravity));
            }
            Vector2 newPosition = obj.Position + obj.Velocity * elapsedTime;
            if (config.EnableCollision)
            {
                if (obj.Velocity.X != 0)
                {
                    newPosition = HorizontalCollision(obj, newPosition, ref result);
                }
                if (obj.Velocity.Y != 0)
                {
                    newPosition = VerticalCollision(obj, newPosition, ref result);
                }
                result.IsOnGround = GroundCollsion(obj, newPosition);
            }
            else
            {
                obj.Position = newPosition;
            }
            if(config.EnableFriction && result.IsOnGround)
            {
                ApplyFriction(obj, config.Friction);
            }
            return result;
        }

        private Vector2 HorizontalCollision(GameObject obj, Vector2 newPosition, ref PhysicsResult result)
        {
            Rectangle testRectangle = obj.GetCollisionBox(newPosition);
            if(!obj.IsOutOfTilemap(newPosition))
            {
                bool isCollided = obj.IsCollidingWithTile(testRectangle, out TileCollisionResult collisionResult);
                if(isCollided && !collisionResult.TileType.HasFlag(TileType.Penetrable))
                {
                    result.HasHorizontalCollision = true;
                    result.CollisionDirection = collisionResult.Direction;
                    obj.Velocity = new Vector2(0, obj.Velocity.Y);
                    if (collisionResult.Direction == CollisionDirection.Left)
                    {
                        newPosition = new Vector2(collisionResult.TileRectangle.Right, newPosition.Y);
                    }
                    else if (collisionResult.Direction == CollisionDirection.Right)
                    {
                        newPosition = new Vector2(collisionResult.TileRectangle.Left - obj.CollisionBox.Width, newPosition.Y);
                    }
                }
            }
            return newPosition;
        }

        private Vector2 VerticalCollision(GameObject obj, Vector2 newPosition, ref PhysicsResult result)
        {
            Vector2 verticalMove = new Vector2(0, obj.Velocity.Y);
            Vector2 testPosition = obj.Position + verticalMove;

            if (testPosition.Y < 0)
            {
                // 上边界碰撞
                newPosition.Y = 0;
                obj.Velocity = new Vector2(obj.Velocity.X, 0);
                result.HasVerticalCollision = true;
            }
            else
            {
                Rectangle testRect = obj.GetCollisionBox(testPosition);
                if (obj.IsCollidingWithTile(testRect, out TileCollisionResult tileResult))
                {
                    result.HasVerticalCollision = true;

                    if (obj.Velocity.Y > 0) // 向下碰撞
                    {
                        if (!tileResult.TileType.HasFlag(TileType.Penetrable))
                        {
                            float tileTop = tileResult.TileRectangle.Top;
                            newPosition.Y = tileTop - obj.Size.Y;
                            obj.Velocity = new Vector2(obj.Velocity.X, 0);
                            result.IsOnGround = true;
                        }
                    }
                    else if (obj.Velocity.Y < 0) // 向上碰撞
                    {
                        if (!tileResult.TileType.HasFlag(TileType.Penetrable))
                        {
                            newPosition.Y = tileResult.TileRectangle.Bottom;
                            obj.Velocity = new Vector2(obj.Velocity.X, 0);
                        }
                    }
                }
                else
                {
                    newPosition = testPosition;
                }
            }

            return newPosition;
        }

        private bool GroundCollsion(GameObject obj, Vector2 position)
        {
            Rectangle collisionBox = obj.GetCollisionBox(position);
            Rectangle testRectangle = new Rectangle(collisionBox.X, collisionBox.Y + collisionBox.Height, collisionBox.Width, 3);
            return obj.IsCollidingWithTile(testRectangle, out TileCollisionResult collisionResult) && !collisionResult.TileType.HasFlag(TileType.Penetrable);
        }

        private void ApplyFriction(GameObject obj, float friction)
        {
            if (obj.Velocity.X > 0)
            {
                obj.Velocity = new Vector2(Math.Max(0, obj.Velocity.X - friction), obj.Velocity.Y);
            }
            else if (obj.Velocity.X < 0)
            {
                obj.Velocity = new Vector2(Math.Min(0, obj.Velocity.X + friction), obj.Velocity.Y);
            }
        }
    }
}
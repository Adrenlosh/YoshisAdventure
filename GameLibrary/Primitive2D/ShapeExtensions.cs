using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace GameLibrary.Primitive2D
{
    public static class ShapeExtensions
    {
        public static void FillRectangle(this SpriteBatch spriteBatch, RectangleF rectangle,
            Color fillColor, Color borderColor, float borderThickness = 1f, float layerDepth = 0f)
        {
            spriteBatch.FillRectangle(rectangle, fillColor, layerDepth);
            spriteBatch.DrawRectangle(rectangle, borderColor, borderThickness, layerDepth + 0.001f);
        }

        public static void FillRectangle(this SpriteBatch spriteBatch, Vector2 location, SizeF size,
            Color fillColor, Color borderColor, float borderThickness = 1f, float layerDepth = 0f)
        {
            spriteBatch.FillRectangle(location, size, fillColor, layerDepth);
            spriteBatch.DrawRectangle(location, size, borderColor, borderThickness, layerDepth + 0.001f);
        }

        public static void FillRectangle(this SpriteBatch spriteBatch, float x, float y, float width, float height,
            Color fillColor, Color borderColor, float borderThickness = 1f, float layerDepth = 0f)
        {
            spriteBatch.FillRectangle(x, y, width, height, fillColor, layerDepth);
            spriteBatch.DrawRectangle(x, y, width, height, borderColor, borderThickness, layerDepth + 0.001f);
        }
    }
}

using Microsoft.Xna.Framework;

namespace MonoGameLibrary.Graphics
{
    /// <summary>
    /// 表示一个地图瓦片，包含阻挡信息。
    /// </summary>
    public class Tile
    {
        /// <summary>
        /// 瓦片的纹理区域。
        /// </summary>
        public TextureRegion Region { get; set; }

        /// <summary>
        /// 是否为阻挡瓦片（不可通过）。
        /// </summary>
        public bool IsBlocking { get; set; }

        public int TileID { get; set; }

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="region">瓦片的纹理区域。</param>
        /// <param name="isBlocking">是否阻挡。</param>
        public Tile(TextureRegion region, bool isBlocking, int tileID)
        {
            Region = region;
            IsBlocking = isBlocking;
            TileID = tileID;
        }
    }
}
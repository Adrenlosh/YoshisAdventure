using Microsoft.Xna.Framework;

namespace MonoGame.Extended.Tests;

public sealed class Matrix3x2Tests
{
    [Fact]
    public void ConstructorTest()
    {
        Vector2 x = new Vector2(1, 2);
        Vector2 y = new Vector2(3, 4);
        Vector2 z = new Vector2(5, 6);

        var matrix = new Matrix3x2(x.X, x.Y, y.X, y.Y, z.X, z.Y);

        Assert.Equal(x, matrix.X);
        Assert.Equal(y, matrix.Y);
        Assert.Equal(z, matrix.Z);
    }

    [Fact]
    public void InverseTest()
    {
        Matrix3x2 posDeterminant = new Matrix3x2(3, -7, 4, 2, 13, -2);
        Matrix3x2 singular = new Matrix3x2(2, 1, 4, 2, 3, -4);
        Matrix3x2 negDeterminant = new Matrix3x2(1, -5, 3, 2, 3, -4);

        Matrix3x2 posExpected = new Matrix3x2(1f/17, 7f/34, -2f/17, 3f/34, -1f, -5f/2);
        Matrix3x2 singularExpected = Matrix3x2.Identity;
        Matrix3x2 negExpected = new Matrix3x2(2f/17, 5f/17, -3f/17, 1f/17, -18f/17, -11f/17);
        
        Assert.Equal(Matrix3x2.Invert(posDeterminant), posExpected);
        Assert.Equal(Matrix3x2.Invert(singular), singularExpected);
        Assert.Equal(Matrix3x2.Invert(negDeterminant), negExpected);
    }
}

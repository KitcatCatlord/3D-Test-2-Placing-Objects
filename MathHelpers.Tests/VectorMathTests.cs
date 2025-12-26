using NUnit.Framework;
using Console3DEnvironment;

namespace MathHelpers.Tests
{
    [TestFixture]
    public class VectorMathTests
    {
        [Test]
        public void Cross_Product_Computes_RightVector()
        {
            var a = new Vector3(1, 0, 0);
            var b = new Vector3(0, 1, 0);
            var result = Vector3.Cross(a, b);
            Assert.AreEqual(new Vector3(0, 0, 1), result);
        }

        [Test]
        public void Dot_Product_Computes_Value()
        {
            var a = new Vector3(1, 2, 3);
            var b = new Vector3(4, 5, 6);
            var result = Vector3.Dot(a, b);
            Assert.AreEqual(32f, result);
        }

        [Test]
        public void MultiplyVector_Transforms_Vector()
        {
            var m = new Matrix4x4(
                2, 0, 0, 0,
                0, 3, 0, 0,
                0, 0, 4, 0,
                0, 0, 0, 1);
            var v = new Vector4(1, 1, 1, 1);
            var result = m.MultiplyVector(v);
            Assert.AreEqual(new Vector4(2, 3, 4, 1), result);
        }
    }
}

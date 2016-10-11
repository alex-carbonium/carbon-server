using System.Drawing;
using Carbon.Framework.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Carbon.Test.Unit.Framework
{
    [TestClass]
    public class DimensionUtilTest : BaseTest
    {
        [TestMethod]
        public void Resize_Smaller()
        {
            //arrange
            var size = new Size(10, 10);
            var constraint = new Size(100, 100);

            //act
            var result = MediaUtil.FitSize(size, constraint);

            //assert
            Assert.AreEqual(size, result, "The size must remain the same");
        }
        [TestMethod]
        public void Resize_Same()
        {
            //arrange
            var size = new Size(100, 100);
            var constraint = new Size(100, 100);

            //act
            var result = MediaUtil.FitSize(size, constraint);

            //assert
            Assert.AreEqual(size, result, "The size must remain the same");
        }
        [TestMethod]
        public void Resize_WidthBigger()
        {
            //arrange
            var size = new Size(100, 100);
            var constraint = new Size(50, 100);
            var expected = new Size(50, 50);

            //act
            var result = MediaUtil.FitSize(size, constraint);

            //assert
            Assert.AreEqual(expected, result, "The size should be changed according the constraint");
        }
        [TestMethod]
        public void Resize_HeightBigger()
        {
            //arrange
            var size = new Size(100, 100);
            var constraint = new Size(100, 50);
            var expected = new Size(50, 50);

            //act
            var result = MediaUtil.FitSize(size, constraint);

            //assert
            Assert.AreEqual(expected, result, "The size should be changed according the constraint");
        }
        [TestMethod]
        public void Resize_WidthBiggerResultStillBigger()
        {
            //arrange
            var size = new Size(100, 200);
            var constraint = new Size(50, 50);
            var expected = new Size(25, 50);

            //act
            var result = MediaUtil.FitSize(size, constraint);

            //assert
            Assert.AreEqual(expected, result, "The size should be changed according the constraint");
        }
        [TestMethod]
        public void Resize_HeightBiggerResultStillBigger()
        {
            //arrange
            var size = new Size(200, 100);
            var constraint = new Size(50, 50);
            var expected = new Size(50, 25);

            //act
            var result = MediaUtil.FitSize(size, constraint);

            //assert
            Assert.AreEqual(expected, result, "The size should be changed according the constraint");
        }
        [TestMethod]
        public void Resize_Round()
        {
            //arrange
            var size = new Size(900, 10);
            var constraint = new Size(50, 50);
            var expected = new Size(50, 1);

            //act
            var result = MediaUtil.FitSize(size, constraint);

            //assert
            Assert.AreEqual(expected, result, "The size should be changed according the constraint");
        }
    }
}
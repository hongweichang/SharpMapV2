using System;
using System.Collections;

using NUnit.Framework;

using SharpMap.Rendering;
using SharpMap.Rendering.Rendering2D;
using SharpMap.Rendering.Rendering3D;

namespace SharpMap.Tests.Rendering
{
	#region ViewPoint2D
	[TestFixture]
	public class ViewPoint2DTests
	{
		[Test]
		public void ViewPoint2DEqualityTests()
		{
			ViewPoint2D p1 = new ViewPoint2D();
			ViewPoint2D p2 = ViewPoint2D.Empty;
			ViewPoint2D p3 = ViewPoint2D.Zero;
			ViewPoint2D p4 = new ViewPoint2D(0, 0);
			ViewPoint2D p5 = new ViewPoint2D(9, 10);

			Assert.AreEqual(p1, p2);
			Assert.AreNotEqual(p1, p3);
			Assert.AreEqual(p3, p4);
			Assert.AreNotEqual(p1, p5);
			Assert.AreNotEqual(p3, p5);

			IViewVector v1 = (IViewVector)p1;
			IViewVector v2 = (IViewVector)p2;
			IViewVector v3 = (IViewVector)p3;
			IViewVector v4 = (IViewVector)p4;
			IViewVector v5 = (IViewVector)p5;

			Assert.AreEqual(v1, v2);
			Assert.AreNotEqual(v1, v3);
			Assert.AreEqual(v3, v4);
			Assert.AreNotEqual(v1, v5);
			Assert.AreNotEqual(v3, v5);

			Assert.AreEqual(v5, p5);
		}

		[Test]
		public void IViewVectorTests1()
		{
			ViewPoint2D p1 = ViewPoint2D.Empty;
			ViewPoint2D p2 = ViewPoint2D.Zero;
			ViewPoint2D p3 = new ViewPoint2D(9, 10);

			Assert.AreEqual(0, p1.Elements.Length);
			Assert.AreEqual(2, p2.Elements.Length);
			Assert.AreEqual(9, p3[0]);
			Assert.AreEqual(10, p3[1]);
		}

		[Test]
		[ExpectedException(typeof(IndexOutOfRangeException))]
		public void IViewVectorTests2()
		{
			ViewPoint2D p1 = new ViewPoint2D(9, 10);

			Assert.AreEqual(10, p1[2]);
		}
		
		[Test]
		public void CloneTest()
		{
			ViewPoint2D p1 = new ViewPoint2D(1.1, 2.2);
			ViewPoint2D p2 = (ViewPoint2D)p1.Clone();

			Assert.AreEqual(p1, p2);
			Assert.AreNotSame(p1, p2);
		}

		[Test]
		public void IEnumerableTest()
		{
			ViewPoint2D p1 = new ViewPoint2D(1.1, 2.2);

			int index = 0;
			foreach (double component in p1)
			{
				Assert.AreEqual(p1[index++], component);
			}
		}
	}
	#endregion

	#region ViewSize2D
	[TestFixture]
	public class ViewSize2DTests
	{
		[Test]
		public void ViewSize2DEqualityTests()
		{
			ViewSize2D s1 = new ViewSize2D();
			ViewSize2D s2 = ViewSize2D.Empty;
			ViewSize2D s3 = ViewSize2D.Zero;
			ViewSize2D s4 = new ViewSize2D(0, 0);
			ViewSize2D s5 = new ViewSize2D(9, 10);

			Assert.AreEqual(s1, s2);
			Assert.AreNotEqual(s1, s3);
			Assert.AreEqual(s3, s4);
			Assert.AreNotEqual(s1, s5);
			Assert.AreNotEqual(s3, s5);

			IViewVector v1 = (IViewVector)s1;
			IViewVector v2 = (IViewVector)s2;
			IViewVector v3 = (IViewVector)s3;
			IViewVector v4 = (IViewVector)s4;
			IViewVector v5 = (IViewVector)s5;

			Assert.AreEqual(v1, v2);
			Assert.AreNotEqual(v1, v3);
			Assert.AreEqual(v3, v4);
			Assert.AreNotEqual(v1, v5);
			Assert.AreNotEqual(v3, v5);

			Assert.AreEqual(v5, s5);
		}

		[Test]
		public void IViewVectorTests1()
		{
			ViewPoint2D s1 = ViewPoint2D.Empty;
			ViewPoint2D s2 = ViewPoint2D.Zero;
			ViewPoint2D s3 = new ViewPoint2D(9, 10);

			Assert.AreEqual(0, s1.Elements.Length);
			Assert.AreEqual(2, s2.Elements.Length);
			Assert.AreEqual(9, s3[0]);
			Assert.AreEqual(10, s3[1]);
		}

		[Test]
		[ExpectedException(typeof(IndexOutOfRangeException))]
		public void IViewVectorTests2()
		{
			ViewPoint2D s1 = new ViewPoint2D(9, 10);

			Assert.AreEqual(10, s1[2]);
		}

		[Test]
		public void CloneTest()
		{
			ViewPoint2D s1 = new ViewPoint2D(1.1, 2.2);
			ViewPoint2D s2 = (ViewPoint2D)s1.Clone();

			Assert.AreEqual(s1, s2);
			Assert.AreNotSame(s1, s2);
		}

		[Test]
		public void IEnumerableTest()
		{
			ViewPoint2D s1 = new ViewPoint2D(1.1, 2.2);

			int index = 0;
			foreach (double component in s1)
			{
				Assert.AreEqual(s1[index++], component);
			}
		}
	}
	#endregion

	#region ViewRectangle2D
	[TestFixture]
	public class ViewRectangle2DTests
	{
		[Test]
        public void ViewRectangle2DEqualityTests()
		{
			ViewRectangle2D r1 = new ViewRectangle2D();
			ViewRectangle2D r2 = ViewRectangle2D.Empty;
			ViewRectangle2D r3 = ViewRectangle2D.Zero;
			ViewRectangle2D r4 = new ViewRectangle2D(0, 0, 0, 0);
			ViewRectangle2D r5 = new ViewRectangle2D(9, 10, -5, -6);
            ViewRectangle2D r6 = new ViewRectangle2D(0, 10, 0, 10);
            ViewRectangle2D r7 = new ViewRectangle2D(new ViewPoint2D(0, 0), new ViewSize2D(10, 10));

			Assert.AreEqual(r1, r2);
			Assert.AreNotEqual(r1, r3);
			Assert.AreEqual(r3, r4);
			Assert.AreNotEqual(r1, r5);
			Assert.AreNotEqual(r3, r5);
            Assert.AreEqual(r6, r7);

			IViewMatrix v1 = (IViewMatrix)r1;
			IViewMatrix v2 = (IViewMatrix)r2;
			IViewMatrix v3 = (IViewMatrix)r3;
			IViewMatrix v4 = (IViewMatrix)r4;
			IViewMatrix v5 = (IViewMatrix)r5;

			Assert.AreEqual(v1, v2);
			Assert.AreNotEqual(v1, v3);
			Assert.AreEqual(v3, v4);
			Assert.AreNotEqual(v1, v5);
			Assert.AreNotEqual(v3, v5);

			Assert.AreEqual(v5, r5);
		}

		[Test]
		public void IntersectsTest()
		{
            ViewRectangle2D r1 = ViewRectangle2D.Empty;
            ViewRectangle2D r2 = ViewRectangle2D.Zero;
            ViewRectangle2D r3 = new ViewRectangle2D(0, 10, 0, 10);
            ViewRectangle2D r4 = new ViewRectangle2D(new ViewPoint2D(5, 5), new ViewSize2D(10, 10));

            Assert.IsFalse(r1.Intersects(ViewRectangle2D.Empty));
            Assert.IsFalse(r1.Intersects(r2));
            Assert.IsFalse(r2.Intersects(r1));
            Assert.IsTrue(r2.Intersects(r3));
            Assert.IsTrue(r3.Intersects(r4));
            Assert.IsTrue(r4.Intersects(r4));
		}

		[Test]
		public void CompareTest()
        {
            ViewRectangle2D r1 = ViewRectangle2D.Empty;
            ViewRectangle2D r2 = ViewRectangle2D.Zero;
            ViewRectangle2D r3 = new ViewRectangle2D(0, 10, 0, 10);
            ViewRectangle2D r4 = new ViewRectangle2D(new ViewPoint2D(11, -11), new ViewSize2D(10, 10));
            ViewRectangle2D r5 = new ViewRectangle2D(new ViewPoint2D(-11, -11), new ViewSize2D(10, 10));
            ViewRectangle2D r6 = new ViewRectangle2D(new ViewPoint2D(11, 11), new ViewSize2D(10, 10));

            Assert.AreEqual(0, r1.CompareTo(ViewRectangle2D.Empty));
            Assert.AreEqual(-1, r1.CompareTo(r2));
            Assert.AreEqual(1, r2.CompareTo(r1));
            Assert.AreEqual(0, r3.CompareTo(r3));
            Assert.AreEqual(0, r3.CompareTo(r2));
            Assert.AreEqual(1, r4.CompareTo(r3));
            Assert.AreEqual(-1, r5.CompareTo(r3));
            Assert.AreEqual(1, r6.CompareTo(r3));
		}

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
		public void ResetTest()
		{
            ViewRectangle2D r1 = new ViewRectangle2D(0, 1, 0, 1);
            r1.Reset();
		}

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
		public void InvertTest()
        {
            ViewRectangle2D r1 = new ViewRectangle2D(0, 1, 0, 1);
            r1.Invert();
		}

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
		public void IsInvertableTest()
        {
            ViewRectangle2D r1 = new ViewRectangle2D(0, 1, 0, 1);
            Assert.IsTrue(r1.IsInvertible);
		}

		[Test]
		public void ElementsTest1()
        {
            ViewRectangle2D r1 = ViewRectangle2D.Empty;
            ViewRectangle2D r2 = ViewRectangle2D.Zero;
            ViewRectangle2D r3 = new ViewRectangle2D(0, 10, 0, 10);

            Assert.AreEqual(0, r1.Elements.Length);
            Assert.AreEqual(4, r2.Elements.Length);
            Assert.AreEqual(4, r3.Elements.Length);

            double[,] expected = new double[,] { { 0, 0 }, { 10, 10 } };
            double[,] actual = r3.Elements;

            Assert.AreEqual(expected[0, 0], actual[0, 0]);
            Assert.AreEqual(expected[0, 1], actual[0, 1]);
            Assert.AreEqual(expected[1, 0], actual[1, 0]);
            Assert.AreEqual(expected[1, 1], actual[1, 1]);

            r1.Elements = expected;
            Assert.IsFalse(r1.IsEmpty);
            Assert.AreEqual(r1, r3);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ElementsTest2()
        {
            ViewRectangle2D r1 = ViewRectangle2D.Zero;
            r1.Elements = null;

		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void ElementsTest3()
        {
            ViewRectangle2D r1 = ViewRectangle2D.Zero;
            r1.Elements = new double[,] { { 1, 2, 3 }, { 2, 3, 4 } };
		}

		[Test]
		[ExpectedException(typeof(NotSupportedException))]
		public void RotateTest()
        {
            ViewRectangle2D r1 = new ViewRectangle2D(0, 1, 0, 1);
            r1.Rotate(45);
		}

		[Test]
		[ExpectedException(typeof(NotSupportedException))]
		public void RotateAtTest()
        {
            ViewRectangle2D r1 = new ViewRectangle2D(0, 1, 0, 1);
            r1.RotateAt(45, new ViewPoint2D(0.5, 0.5));
		}

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
		public void GetOffsetTest()
        {
            ViewRectangle2D r1 = new ViewRectangle2D(0, 1, 0, 1);
            r1.GetOffset(0);
		}
		
		[Test]
		public void OffsetTest()
        {
            ViewRectangle2D r1 = ViewRectangle2D.Empty;
            ViewRectangle2D r2 = ViewRectangle2D.Zero;

            ViewPoint2D offset = new ViewPoint2D(10, 10);

            r1.Offset(offset);
            Assert.IsTrue(r1.IsEmpty);
            Assert.AreEqual(0, r1.Left);
            Assert.AreEqual(0, r1.Right);
            Assert.AreEqual(0, r1.Top);
            Assert.AreEqual(0, r1.Bottom);

            r2.Offset(offset);
            Assert.AreEqual(10, r2.Left);
            Assert.AreEqual(10, r2.Right);
            Assert.AreEqual(10, r2.Top);
            Assert.AreEqual(10, r2.Bottom);
		}

		[Test]
		[Ignore("Not yet implemented")]
		public void MultiplyTest()
        {
		}
		
		[Test]
		public void ScaleTest1()
		{
            ViewRectangle2D r1 = ViewRectangle2D.Empty;
            ViewRectangle2D r2 = ViewRectangle2D.Zero;
            ViewRectangle2D r3 = new ViewRectangle2D(0, 1, 0, 1);

            r1.Scale(10);
            Assert.IsTrue(r1.IsEmpty);

            r2.Scale(10);
            Assert.AreEqual(0, r2.Width);
            Assert.AreEqual(0, r2.Height);

            r3.Scale(10);
            Assert.AreEqual(10, r3.Width);
            Assert.AreEqual(10, r3.Height);
            Assert.AreEqual(ViewPoint2D.Zero, r3.Location);

            ViewSize2D scaleSize = new ViewSize2D(-1, 5);
            
            r1.Scale(scaleSize);
            Assert.IsTrue(r1.IsEmpty);

            r2.Scale(scaleSize);
            Assert.AreEqual(0, r2.Width);
            Assert.AreEqual(0, r2.Height);

            r3.Scale(scaleSize);
            Assert.AreEqual(-10, r3.Width);
            Assert.AreEqual(50, r3.Height);
            Assert.AreEqual(ViewPoint2D.Zero, r3.Location);
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ScaleTest2()
        {
            ViewSize3D scaleSize = new ViewSize3D(10, 10, 10);
            ViewRectangle2D r2 = ViewRectangle2D.Zero;
            r2.Scale(scaleSize);
		}
		
		[Test]
		public void TranslateTest1()
		{
			ViewRectangle2D r1 = ViewRectangle2D.Empty;
			ViewRectangle2D r2 = ViewRectangle2D.Zero;

			r1.Translate(10);
			Assert.IsTrue(r1.IsEmpty);
			Assert.AreEqual(r1.X, 0);
			Assert.AreEqual(r1.Y, 0);
			Assert.AreEqual(r1.Width, 0);
			Assert.AreEqual(r1.Height, 0);

			r2.Translate(new ViewPoint2D(3, 5));
			Assert.AreEqual(r2.X, 3);
			Assert.AreEqual(r2.Y, 5);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void TranslateTest2()
		{
			ViewRectangle2D r1 = ViewRectangle2D.Zero;
			r1.Translate(new SharpMap.Rendering.Rendering3D.ViewPoint3D(3, 4, 5));
		}
		
		[Test]
		[ExpectedException(typeof(NotSupportedException))]
		public void TransformTest1()
		{
			ViewRectangle2D rect = new ViewRectangle2D(9, 10, -5, -6);
			IViewVector val = rect.Transform((IViewVector)ViewPoint2D.Zero);
		}
		
		[Test]
		[ExpectedException(typeof(NotSupportedException))]
		public void Transform2Test2()
		{
			ViewRectangle2D rect = new ViewRectangle2D(9, 10, -5, -6);
			double[] val = rect.Transform(1, 4);
		}
	}
	#endregion

	#region ViewMatrix2D
	[TestFixture]
	public class ViewMatrix2DTests
    {
        [Test]
        public void ResetTest()
        {
            ViewMatrix2D m1 = new ViewMatrix2D(1, 1, 0, 1, 1, 0, 0, 0, 1);

            m1.Reset();

            Assert.AreEqual(m1, ViewMatrix2D.Identity);
        }

        [Test]
        [Ignore("Matrix invert not implemented on ViewMatrix2D")]
        public void InvertTest()
        {
            ViewMatrix2D m1 = new ViewMatrix2D(1, 1, 0, 1, 1, 0, 0, 0, 1);

            m1.Invert();
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void IsInvertableTest()
        {
            ViewMatrix2D m1 = new ViewMatrix2D(1, 1, 0, 1, 1, 0, 0, 0, 1);
            Assert.IsTrue(m1.IsInvertible);
        }

        [Test]
        public void ElementsTest1()
        {
            ViewMatrix2D m1 = ViewMatrix2D.Identity;
            ViewMatrix2D m2 = ViewMatrix2D.Zero;
            ViewMatrix2D m3 = new ViewMatrix2D(1, 2, 3, 4, 5, 6, 7, 8, 9);

            Assert.AreEqual(9, m1.Elements.Length);
            Assert.AreEqual(9, m2.Elements.Length);

            double[,] expected = new double[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } };
            double[,] actual = m3.Elements;

            Assert.AreEqual(expected[0, 0], actual[0, 0]);
            Assert.AreEqual(expected[0, 1], actual[0, 1]);
            Assert.AreEqual(expected[0, 2], actual[0, 2]);
            Assert.AreEqual(expected[1, 0], actual[1, 0]);
            Assert.AreEqual(expected[1, 1], actual[1, 1]);
            Assert.AreEqual(expected[1, 2], actual[1, 2]);
            Assert.AreEqual(expected[2, 0], actual[2, 0]);
            Assert.AreEqual(expected[2, 1], actual[2, 1]);
            Assert.AreEqual(expected[2, 2], actual[2, 2]);

            m1.Elements = expected;
            Assert.AreEqual(m1, m3);
            Assert.IsTrue(m1.Equals(m3 as IViewMatrix));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ElementsTest2()
        {
            ViewMatrix2D m1 = ViewMatrix2D.Identity;
            m1.Elements = null;

        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ElementsTest3()
        {
            ViewMatrix2D m1 = ViewMatrix2D.Identity;
            m1.Elements = new double[,] { { 1, 2, 3 }, { 2, 3, 4 } };
        }

        [Test]
        [Ignore("Not yet implemented")]
        public void RotateTest()
        {
            ViewMatrix2D m1 = ViewMatrix2D.Identity;
        }

        [Test]
        [Ignore("Not yet implemented")]
        public void RotateAtTest()
        {
            ViewMatrix2D m1 = ViewMatrix2D.Identity;
        }

        [Test]
        [Ignore("Not yet implemented")]
        public void GetOffsetTest()
        {
            ViewMatrix2D m1 = ViewMatrix2D.Identity;
        }

        [Test]
        [Ignore("Not yet implemented")]
        public void OffsetTest()
        {
            ViewMatrix2D m1 = ViewMatrix2D.Identity;
        }

        [Test]
        [Ignore("Not yet implemented")]
        public void MultiplyTest()
        {
            ViewMatrix2D m1 = ViewMatrix2D.Identity;
        }

        [Test]
        [Ignore("Not yet implemented")]
        public void ScaleTest1()
        {
            ViewMatrix2D m1 = ViewMatrix2D.Zero;
            ViewMatrix2D m2 = ViewMatrix2D.Identity;

            m1.Scale(10);
            Assert.AreEqual(ViewMatrix2D.Zero, m1);

            m2.Scale(10);
            Assert.AreEqual(10, m2.X1);
            Assert.AreEqual(10, m2.Y2);

            ViewSize2D scaleSize = new ViewSize2D(-1, 5);

            m1.Scale(scaleSize);
            Assert.AreEqual(ViewMatrix2D.Zero, m1);

            m2.Scale(scaleSize);
            Assert.AreEqual(-10, m2.X1);
            Assert.AreEqual(50, m2.Y2);
        }

        [Test]
        [Ignore("Not yet implemented")]
        public void ScaleTest2()
        {
            ViewMatrix2D m1 = ViewMatrix2D.Identity;

            // Scale by a vector for which multiplicatio isn't defined...
        }

        [Test]
        [Ignore("Not yet implemented")]
        public void TranslateTest1()
        {
            ViewMatrix2D m1 = ViewMatrix2D.Identity;
        }

        [Test]
        [Ignore("Not yet implemented")]
        public void TranslateTest2()
        {
            ViewMatrix2D m1 = ViewMatrix2D.Identity;
            // Scale by a vector for which multiplicatio isn't defined...
        }

        [Test]
        [Ignore("Not yet implemented")]
        public void TransformTest1()
        {
            ViewMatrix2D m1 = ViewMatrix2D.Identity;
        }

        [Test]
        [Ignore("Not yet implemented")]
        public void Transform2Test2()
        {
            ViewMatrix2D m1 = ViewMatrix2D.Identity;
            // Scale by a vector for which multiplicatio isn't defined...
        }
	}
    #endregion

    #region GraphicsFigure2D
    [TestFixture]
    public class GraphicsFigure2DTests
    {
        [Test]
        public void CreateNewTest()
        {
            ViewPoint2D[] points = new ViewPoint2D[] { new ViewPoint2D(0, 0), new ViewPoint2D(1, 0), new ViewPoint2D(1, 1), new ViewPoint2D(0, 1) };
            GraphicsFigure2D f1 = new GraphicsFigure2D(points, true);
            Assert.AreEqual(4, f1.Points.Count);
            
            for(int i = 0; i < 4; i++)
            {
                Assert.AreEqual(points[i], f1.Points[i]);
            }
        }

        [Test]
        public void EqualityTest()
        {
            ViewPoint2D[] points1 = new ViewPoint2D[] { new ViewPoint2D(0, 0), new ViewPoint2D(1, 0), new ViewPoint2D(1, 1), new ViewPoint2D(0, 1) };
            ViewPoint2D[] points2 = new ViewPoint2D[] { new ViewPoint2D(0, 0), new ViewPoint2D(2, 0), new ViewPoint2D(2, 2), new ViewPoint2D(0, 2) };
            ViewPoint2D[] points3 = new ViewPoint2D[] { new ViewPoint2D(0, 0), new ViewPoint2D(2, 0), new ViewPoint2D(2, 2), new ViewPoint2D(0, 2), new ViewPoint2D(0, 2) };

            GraphicsFigure2D f1 = new GraphicsFigure2D(points1, true);
            GraphicsFigure2D f2 = new GraphicsFigure2D(points1, true);
            GraphicsFigure2D f3 = new GraphicsFigure2D(points1);
            GraphicsFigure2D f4 = new GraphicsFigure2D(points2, true);
            GraphicsFigure2D f5 = new GraphicsFigure2D(points3, true);

            Assert.AreEqual(f1, f2);
            Assert.AreNotEqual(f1, f3);
            Assert.AreNotEqual(f1, f4);
            Assert.AreNotEqual(f4, f5);
			Assert.IsFalse(f1.Equals(new object()));
			f2 = null;
			Assert.IsFalse(f1.Equals(f2));
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void AddPointsTest()
        {
            ViewPoint2D[] points1 = new ViewPoint2D[] { new ViewPoint2D(0, 0), new ViewPoint2D(1, 0), new ViewPoint2D(1, 1), new ViewPoint2D(0, 1) };

            GraphicsFigure2D f1 = new GraphicsFigure2D(points1, false);

            f1.Points.Add(new ViewPoint2D(5, 5));
		}

		[Test]
		public void ToStringTest()
		{
            ViewPoint2D[] points1 = new ViewPoint2D[] { new ViewPoint2D(0, 0), new ViewPoint2D(1, 0), new ViewPoint2D(1, 1), new ViewPoint2D(0, 1) };
			GraphicsFigure2D f1 = new GraphicsFigure2D(points1);

			string expected = String.Format("[{0}] Number of {1} points: 4; Closed: False", typeof(GraphicsFigure2D), typeof(ViewPoint2D).Name);
			Assert.AreEqual(expected, f1.ToString());
		}

		[Test]
		public void CloneTest()
		{
			ViewPoint2D[] points1 = new ViewPoint2D[] { new ViewPoint2D(0, 0), new ViewPoint2D(1, 0), new ViewPoint2D(1, 1), new ViewPoint2D(0, 1) };

			GraphicsFigure2D f1 = new GraphicsFigure2D(points1);
			GraphicsFigure<ViewPoint2D, ViewRectangle2D> f2 = f1.Clone();
			GraphicsFigure2D f3 = (f1 as ICloneable).Clone() as GraphicsFigure2D;

			Assert.AreEqual(f1, f2);
			Assert.AreEqual(f1, f3);
		}

		[Test]
		public void EnumTest()
		{
			ViewPoint2D[] points1 = new ViewPoint2D[] { new ViewPoint2D(0, 0), new ViewPoint2D(1, 0), new ViewPoint2D(1, 1), new ViewPoint2D(0, 1) };

			GraphicsFigure2D f1 = new GraphicsFigure2D(points1);

			IEnumerator e1 = (f1 as IEnumerable).GetEnumerator();

			Assert.IsNotNull(e1);
		}
    }
    #endregion

    #region GraphicsPath2D
    [TestFixture]
    public class GraphicsPath2DTests
    {
        [Test]
        public void CreateNewTest()
        {
            GraphicsPath2D p1 = new GraphicsPath2D();
            Assert.AreEqual(0, p1.Figures.Count);

            ViewPoint2D[] points = new ViewPoint2D[] { new ViewPoint2D(0, 0), new ViewPoint2D(1, 0), new ViewPoint2D(1, 1), new ViewPoint2D(0, 1) };
            GraphicsPath2D p2 = new GraphicsPath2D(points);
            Assert.AreEqual(1, p2.Figures.Count);
            Assert.AreEqual(4, p2.CurrentFigure.Points.Count);
            Assert.IsFalse(p2.CurrentFigure.IsClosed);

            GraphicsPath2D p3 = new GraphicsPath2D(points, true);
            Assert.IsTrue(p3.CurrentFigure.IsClosed);

            GraphicsFigure2D figure = new GraphicsFigure2D(points, true);
            GraphicsPath2D p4 = new GraphicsPath2D(new GraphicsFigure2D[] {figure});
            Assert.AreEqual(1, p4.Figures.Count);

			GraphicsPath2D p5 = new GraphicsPath2D(new GraphicsFigure2D[] { });
			Assert.IsNull(p5.CurrentFigure);
			Assert.AreEqual(0, p5.Points.Count);
        }

        [Test]
        public void AddFiguresTest()
        {
            ViewPoint2D[] points = new ViewPoint2D[] { new ViewPoint2D(0, 0), new ViewPoint2D(1, 0), new ViewPoint2D(1, 1), new ViewPoint2D(0, 1) };

            GraphicsPath2D p1 = new GraphicsPath2D();
            p1.NewFigure(points, false);

            Assert.AreEqual(1, p1.Figures.Count);
            Assert.IsNotNull(p1.CurrentFigure);
            Assert.AreEqual(4, p1.CurrentFigure.Points.Count);
            Assert.IsFalse(p1.CurrentFigure.IsClosed);
            Assert.AreEqual(p1.Bounds, p1.CurrentFigure.Bounds);
            Assert.AreEqual(p1.Bounds, new ViewRectangle2D(0, 1, 0, 1));
        }

        [Test]
        public void CurrentFigureTest1()
        {
            ViewPoint2D[] points = new ViewPoint2D[] { new ViewPoint2D(0, 0), new ViewPoint2D(1, 0), new ViewPoint2D(1, 1), new ViewPoint2D(0, 1) };

            GraphicsFigure2D f1 = new GraphicsFigure2D(points, true);
            GraphicsPath2D p1 = new GraphicsPath2D(f1);

            f1 = p1.CurrentFigure as GraphicsFigure2D;
			Assert.AreEqual(4, p1.Points.Count);

            p1.NewFigure(points, false);
            Assert.AreSame(p1.CurrentFigure, p1.Figures[1]);

            p1.CurrentFigure = f1;
            Assert.AreSame(p1.CurrentFigure, f1);

            GraphicsPath2D p2 = new GraphicsPath2D();
            Assert.IsNull(p2.CurrentFigure);
			Assert.AreEqual(0, p2.Points.Count);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CurrentFigureTest2()
        {
            ViewPoint2D[] points = new ViewPoint2D[] { new ViewPoint2D(0, 0), new ViewPoint2D(1, 0), new ViewPoint2D(1, 1), new ViewPoint2D(0, 1) };

            GraphicsFigure2D f1 = new GraphicsFigure2D(points, true);
            GraphicsPath2D p1 = new GraphicsPath2D();
            p1.NewFigure(points, false);

            p1.CurrentFigure = f1;
        }

        [Test]
        public void BoundsTest()
        {
            GraphicsPath2D p1 = new GraphicsPath2D();

            Assert.IsTrue(p1.Bounds == ViewRectangle2D.Empty);

            ViewPoint2D[] points1 = new ViewPoint2D[] { new ViewPoint2D(0, 0), new ViewPoint2D(1, 0), new ViewPoint2D(1, 1), new ViewPoint2D(0, 1) };
            ViewPoint2D[] points2 = new ViewPoint2D[] { new ViewPoint2D(0, 0), new ViewPoint2D(10, 0), new ViewPoint2D(10, 1), new ViewPoint2D(0, 1) };
            ViewPoint2D[] points3 = new ViewPoint2D[] { new ViewPoint2D(0, 0), new ViewPoint2D(-1, 0), new ViewPoint2D(-1, -1), new ViewPoint2D(0, -1) };

            p1.NewFigure(points1, true);
            Assert.AreEqual(p1.Bounds, new ViewRectangle2D(0, 1, 0, 1));

            p1.NewFigure(points2, true);
            Assert.AreEqual(p1.Bounds, new ViewRectangle2D(0, 10, 0, 1));

            p1.NewFigure(points3, true);
            Assert.AreEqual(p1.Bounds, new ViewRectangle2D(-1, 10, -1, 1));
        }

        [Test]
        public void CloneTest()
        {
            ViewPoint2D[] points1 = new ViewPoint2D[] { new ViewPoint2D(0, 0), new ViewPoint2D(1, 0), new ViewPoint2D(1, 1), new ViewPoint2D(0, 1) };
            GraphicsPath2D p1 = new GraphicsPath2D(points1, true);

			GraphicsPath<ViewPoint2D, ViewRectangle2D> p2 = p1.Clone();
            Assert.AreEqual(p1, p2);

			GraphicsPath2D p3 = (p1 as ICloneable).Clone() as GraphicsPath2D;
			Assert.AreEqual(p1, p3);

            p2.NewFigure(points1, false);
            Assert.AreNotEqual(p1, p2);
        }

        [Test]
        public void EqualityTest()
        {
            GraphicsPath2D p1 = new GraphicsPath2D();
            GraphicsPath2D p2 = new GraphicsPath2D();
            Assert.IsTrue(p1.Equals(p2));

            ViewPoint2D[] points1 = new ViewPoint2D[] { new ViewPoint2D(0, 0), new ViewPoint2D(1, 0), new ViewPoint2D(1, 1), new ViewPoint2D(0, 1) };
            ViewPoint2D[] points2 = new ViewPoint2D[] { new ViewPoint2D(0, 0), new ViewPoint2D(2, 0), new ViewPoint2D(2, 2), new ViewPoint2D(0, 2) };

            GraphicsPath2D p3 = new GraphicsPath2D(points1);
            GraphicsPath2D p4 = new GraphicsPath2D(points1);
            Assert.IsTrue(p3.Equals(p4));

            GraphicsPath2D p5 = new GraphicsPath2D(p3.CurrentFigure);
            Assert.IsTrue(p3.Equals(p5));

            GraphicsPath2D p6 = new GraphicsPath2D(points1, true);
            Assert.IsFalse(p3.Equals(p6));

            GraphicsFigure2D f1 = new GraphicsFigure2D(points1, false);
            GraphicsPath2D p7 = new GraphicsPath2D(new GraphicsFigure2D[] { f1 });
            Assert.IsTrue(p3.Equals(p7));

            GraphicsFigure2D f2 = new GraphicsFigure2D(points2, true);
            GraphicsPath2D p8 = new GraphicsPath2D(new GraphicsFigure2D[] {f1, f2});
            Assert.IsFalse(p3.Equals(p8));

            p3.NewFigure(points2, true);
            Assert.IsTrue(p8.Equals(p3));

			p8 = null;
			Assert.IsFalse(p3.Equals(p8));
			Assert.IsFalse(p3.Equals(new object()));
        }

		[Test]
		public void ToStringTest()
		{
			ViewPoint2D[] points1 = new ViewPoint2D[] { new ViewPoint2D(0, 0), new ViewPoint2D(1, 0), new ViewPoint2D(1, 1), new ViewPoint2D(0, 1) };
			GraphicsPath2D p1 = new GraphicsPath2D(points1);
			
			ViewRectangle2D bounds = new ViewRectangle2D(ViewPoint2D.Zero, ViewSize2D.Unit);
			string expected = String.Format("[{0}] 1 figure of ViewPoint2D points; Bounds: {1}", typeof(GraphicsPath2D), bounds);

			Assert.AreEqual(expected, p1.ToString());
		}

		[Test]
		public void GetHashCodeTest()
		{
			ViewPoint2D[] points1 = new ViewPoint2D[] { new ViewPoint2D(0, 0), new ViewPoint2D(1, 0), new ViewPoint2D(1, 1), new ViewPoint2D(0, 1) };
			GraphicsPath2D p1 = new GraphicsPath2D();
			GraphicsPath2D p2 = new GraphicsPath2D();
			GraphicsPath2D p3 = new GraphicsPath2D(points1);

			GraphicsFigure2D f1 = new GraphicsFigure2D(points1, true);
			GraphicsPath2D p4 = new GraphicsPath2D(points1, true);
			GraphicsPath2D p5 = new GraphicsPath2D(f1);

			Assert.AreEqual(p1.GetHashCode(), p2.GetHashCode());
			Assert.AreEqual(p4.GetHashCode(), p5.GetHashCode());
			Assert.AreNotEqual(p1.GetHashCode(), p3.GetHashCode());
			Assert.AreNotEqual(p3.GetHashCode(), p4.GetHashCode());
		}

		[Test]
		public void EnumPointsTest()
		{
			ViewPoint2D[] points1 = new ViewPoint2D[] { new ViewPoint2D(0, 0), new ViewPoint2D(1, 0), new ViewPoint2D(1, 1), new ViewPoint2D(0, 1) };
			GraphicsPath2D p1 = new GraphicsPath2D(points1);

			int i = 0;
			foreach (GraphicsFigure2D figure in p1)
			{
				foreach (ViewPoint2D point in figure)
				{
					Assert.AreEqual(points1[i++], point);
				}
			}

			IEnumerator e1 = p1.GetEnumerator();
			IEnumerator e2 = (p1 as IEnumerable).GetEnumerator();

			Assert.IsTrue(e1.MoveNext());
			Assert.IsTrue(e2.MoveNext());

			Assert.IsTrue(e2.Current.Equals(e1.Current));

			Assert.IsFalse(e1.MoveNext());
			Assert.IsFalse(e2.MoveNext());
		}
    }
    #endregion

    #region Symbol2D
    [TestFixture]
    public class Symbol2DTests
    {
        [Test]
        public void SizeTest()
        {
			Symbol2D s1 = new Symbol2D(new ViewSize2D(16, 16));
			Assert.AreEqual(new ViewSize2D(16, 16), s1.Size);
        }

        [Test]
        public void DataTest()
        {
        }

        [Test]
        public void OffsetTest()
        {
        }

        [Test]
        public void RotationTest()
        {
        }

        [Test]
        public void ScaleTest()
        {
        }

        [Test]
        public void CloneTest()
        {
        }
    }
    #endregion

    #region Label2D
    [TestFixture]
    public class Label2DTests
    {
        [Test]
        public void CreateTest()
        {
        }

        [Test]
        public void CompareTest()
        {
        }
    }
    #endregion

    #region LabelCollisionDetection2D
    [TestFixture]
    public class LabelCollisionDetection2DTests
    {
        [Test]
        public void SimpleCollisionDetectionTest()
        {
        }

        [Test]
        public void ThoroughCollisionDetection()
        {
        }
    }
    #endregion

	#region VectorRenderer2D
	[TestFixture]
	public class VectorRenderer2DTests
	{
	}
	#endregion

	#region RasterRenderer2D
	[TestFixture]
	public class RasterRenderer2DTests
	{
	}
	#endregion
	
	#region BasicGeometryRenderer2D
	[TestFixture]
	public class BasicGeometryRenderer2DTests
	{
        [Test]
        public void RenderFeatureTest()
        {
        }

        [Test]
        public void DrawMultiLineStringTest()
        {
        }

        [Test]
        public void DrawLineStringTest()
        {
        }

        [Test]
        public void DrawMultiPolygonTest()
        {
        }

        [Test]
        public void DrawPolygonTest()
        {
        }

        [Test]
        public void DrawPointTest()
        {
        }

        [Test]
        public void DrawMultiPointTest()
        {
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void UnsupportedGeometryTest()
        {
        }
	}
	#endregion
}
// Portions copyright 2005 - 2006: Morten Nielsen (www.iter.dk)
// Portions copyright 2006 - 2008: Rory Plaire (codekaizen@gmail.com)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using GeoAPI.Geometries;

namespace SharpMap.SimpleGeometries
{
    /// <summary>
    /// A Curve is a one-dimensional geometric object usually stored as a sequence of points,
    /// with the subtype of Curve specifying the form of the interpolation between points.
    /// </summary>
    public abstract class Curve : Geometry, ICurve
    {
        /// <summary>
        ///  The inherent dimension of this Geometry object, which must be less than or equal to the coordinate dimension.
        /// </summary>
        public override Dimensions Dimension
        {
            get { return Dimensions.Curve; }
        }

        /// <summary>
        /// The length of this Curve in its associated spatial reference.
        /// </summary>
        public abstract Double Length { get; }

        /// <summary>
        /// The start point of this Curve.
        /// </summary>
        public abstract IPoint StartPoint { get; }

        /// <summary>
        /// The end point of this Curve.
        /// </summary>
        public abstract IPoint EndPoint { get; }

        /// <summary>
        /// The position of a point on the line, parameterised by length.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public abstract IPoint Value(Double t);

        /// <summary>
        /// Returns true if this Curve is closed (StartPoint = EndPoint).
        /// </summary>
        public Boolean IsClosed
        {
            get { return (StartPoint.Equals(EndPoint)); }
        }

        /// <summary>
        /// true if this Curve is closed (StartPoint = EndPoint) and
        /// this Curve is simple (does not pass through the same point more than once).
        /// </summary>
        public abstract Boolean IsRing { get; }
    }
}
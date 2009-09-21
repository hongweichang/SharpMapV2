// Copyright 2008
//
// This file is part of ProjNet.
// ProjNet is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// ProjNet is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with ProjNet; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

/*
 *  Copyright (C) 2002 Urban Science Applications, Inc. 
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU Lesser General Public
 *  License as published by the Free Software Foundation; either
 *  version 2.1 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public
 *  License along with this library; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 */

using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Units;
using GeoAPI.DataStructures;
//using ProjNet.CoordinateSystems.Transformations;
using NPack.Interfaces;

namespace ProjNet.CoordinateSystems.Projections
{

    internal class InverseKrovakProjection<TCoordinate> : KrovakProjection<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        public InverseKrovakProjection(IEnumerable<ProjectionParameter> parameters,
                               ICoordinateFactory<TCoordinate> coordinateFactory)
            : base(parameters, coordinateFactory)
        {

        }

        public override string Name
        {
            get { return "Inverse_" + base.Name; }
        }

        public override bool IsInverse
        {
            get { return true; }
        }
    }

    /// <summary>
    /// Implemetns the Krovak Projection.
    /// </summary>
    /// <remarks>
    /// <para>The normal case of the Lambert Conformal conic is for the axis of the cone 
    /// to be coincident with the minor axis of the ellipsoid, that is the axis of the cone 
    /// is normal to the ellipsoid at a pole. For the Oblique Conformal Conic the axis 
    /// of the cone is normal to the ellipsoid at a defined location and its extension 
    /// cuts the minor axis at a defined angle. This projection is used in the Czech Republic 
    /// and Slovakia under the name "Krovak" projection.</para>
    /// </remarks>
    internal class KrovakProjection<TCoordinate> : MapProjection<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {

        double _falseEasting;
        double _falseNorthing;


        /**
         * Maximum number of iterations for iterative computations.
         */
        private const int MAXIMUM_ITERATIONS = 15;

        /**
         * When to stop the iteration.
         */
        private const double ITERATION_TOLERANCE = 1E-11;

        /**
         * Azimuth of the centre line passing through the centre of the projection.
         * This is equals to the co-latitude of the cone axis at point of intersection
         * with the ellipsoid.
         */
        protected Radians _azimuth;

        /**
         * Latitude of pseudo standard parallel.
         */
        protected Radians _pseudoStandardParallel;

        /**
         * Useful variables calculated from parameters defined by user.
         */
        private double _sinAzim, _cosAzim, _n, _tanS2, _alfa, _hae, _k1, _ka, _ro0, _rop;

        protected Radians _centralMeridian;
        protected Radians _latitudeOfOrigin;
        protected double _scaleFactor;
        //protected double _excentricitySquared;
        //protected double _excentricity;

        /**
         * Useful constant - 45� in radians.
         */
        private const double S45 = 0.785398163397448;

        #region Constructors

        /// <summary>
        /// Creates an instance of an Krovac projection object.
        /// </summary>
        /// <remarks>
        /// <para>The parameters this projection expects are listed below.</para>
        /// <list type="table">
        /// <listheader><term>Parameter</term><description>Description</description></listheader>
        /// <item><term>latitude_of_origin</term><description>The latitude of the point which is not the natural origin and at which grid coordinate values false easting and false northing are defined.</description></item>
        /// <item><term>central_meridian</term><description>The longitude of the point which is not the natural origin and at which grid coordinate values false easting and false northing are defined.</description></item>
        /// <item><term>standard_parallel_1</term><description>For a conic projection with two standard parallels, this is the latitude of intersection of the cone with the ellipsoid that is nearest the pole.  Scale is true along this parallel.</description></item>
        /// <item><term>standard_parallel_2</term><description>For a conic projection with two standard parallels, this is the latitude of intersection of the cone with the ellipsoid that is furthest from the pole.  Scale is true along this parallel.</description></item>
        /// <item><term>false_easting</term><description>The easting value assigned to the false origin.</description></item>
        /// <item><term>false_northing</term><description>The northing value assigned to the false origin.</description></item>
        /// </list>
        /// </remarks>
        /// <param name="parameters">List of parameters to initialize the projection.</param>
        /// <param name="isInverse">Indicates whether the projection forward (meters to degrees or degrees to meters).</param>
        public KrovakProjection(IEnumerable<ProjectionParameter> parameters, ICoordinateFactory<TCoordinate> coordinateFactory)
            : base(parameters, coordinateFactory)
        {
            Authority = "EPSG";
            AuthorityCode = "9819";

            //PROJCS["S-JTSK (Ferro) / Krovak",
            //GEOGCS["S-JTSK (Ferro)",
            //    DATUM["D_S_JTSK_Ferro",
            //        SPHEROID["Bessel 1841",6377397.155,299.1528128]],
            //    PRIMEM["Ferro",-17.66666666666667],
            //    UNIT["degree",0.0174532925199433]],
            //PROJECTION["Krovak"],
            //PARAMETER["latitude_of_center",49.5],
            //PARAMETER["longitude_of_center",42.5],
            //PARAMETER["azimuth",30.28813972222222],
            //PARAMETER["pseudo_standard_parallel_1",78.5],
            //PARAMETER["scale_factor",0.9999],
            //PARAMETER["false_easting",0],
            //PARAMETER["false_northing",0],
            //UNIT["metre",1]]

            ProjectionParameter par_latitude_of_center = GetParameter("latitude_of_center");
            ProjectionParameter par_longitude_of_center = GetParameter("longitude_of_center");
            ProjectionParameter par_azimuth = GetParameter("azimuth");
            ProjectionParameter par_pseudo_standard_parallel_1 = GetParameter("pseudo_standard_parallel_1");
            ProjectionParameter par_scale_factor = GetParameter("scale_factor");
            ProjectionParameter par_false_easting = GetParameter("false_easting");
            ProjectionParameter par_false_northing = GetParameter("false_northing");

            //Check for missing parameters
            if (par_latitude_of_center == null)
                throw new ArgumentException("Missing projection parameter 'latitude_of_center'");
            if (par_longitude_of_center == null)
                throw new ArgumentException("Missing projection parameter 'longitude_of_center'");
            if (par_azimuth == null)
                throw new ArgumentException("Missing projection parameter 'azimuth'");
            if (par_pseudo_standard_parallel_1 == null)
                throw new ArgumentException("Missing projection parameter 'pseudo_standard_parallel_1'");
            if (par_false_easting == null)
                throw new ArgumentException("Missing projection parameter 'false_easting'");
            if (par_false_northing == null)
                throw new ArgumentException("Missing projection parameter 'false_northing'");

            _latitudeOfOrigin = (Radians) new Degrees((Double)par_latitude_of_center.Value);
            _centralMeridian = (Radians) new Degrees((Double)(24 + (50.0 / 60)));// par_longitude_of_center.Value);
            _azimuth = (Radians)new Degrees((Double)par_azimuth.Value);
            _pseudoStandardParallel = (Radians)new Degrees((Double)par_pseudo_standard_parallel_1.Value);
            _scaleFactor = par_scale_factor.Value;

            _falseEasting = par_false_easting.Value * MetersPerUnit;
            _falseNorthing = par_false_northing.Value * MetersPerUnit;

            //_excentricitySquared = 1.0 - (SemiMinor * SemiMinor) / (SemiMajor * SemiMajor);
            //_excentricity = Math.Sqrt(_excentricitySquared);

            // Calculates useful constants.
            _sinAzim = Math.Sin(_azimuth);
            _cosAzim = Math.Cos(_azimuth);
            _n = Math.Sin(_pseudoStandardParallel);
            _tanS2 = Math.Tan(_pseudoStandardParallel / 2 + S45);

            double sinLat = Math.Sin(_latitudeOfOrigin);
            double cosLat = Math.Cos(_latitudeOfOrigin);
            double cosL2 = cosLat * cosLat;
            _alfa = Math.Sqrt(1 + ((E2 * (cosL2 * cosL2)) / (1 - E2))); // parameter B
            _hae = _alfa * E / 2;
            double u0 = Math.Asin(sinLat / _alfa);

            double esl = E * sinLat;
            double g = Math.Pow((1 - esl) / (1 + esl), (_alfa * E) / 2);
            _k1 = Math.Pow(Math.Tan(_latitudeOfOrigin / 2 + S45), _alfa) * g / Math.Tan(u0 / 2 + S45);
            _ka = Math.Pow(1 / _k1, -1 / _alfa);

            double radius = Math.Sqrt(1 - E2) / (1 - (E2 * (sinLat * sinLat)));

            _ro0 = _scaleFactor * radius / Math.Tan(_pseudoStandardParallel);
            _rop = _ro0 * Math.Pow(_tanS2, _n);
        }
        #endregion

        public override string ProjectionClassName
        {
            get { return Name; }
        }

        public override string Name
        {
            get { return "Krovak"; }
        }

        /// <summary>
        /// Converts coordinates in decimal degrees to projected meters.
        /// </summary>
        /// <param name="lonlat">The point in decimal degrees.</param>
        /// <returns>Point in projected meters</returns>
        public override TCoordinate DegreesToMeters(TCoordinate lonlat)
        {
            Radians lambda = (Radians) new Degrees((Double)lonlat[0]) - _centralMeridian;
            Radians phi = (Radians) new Degrees((Double)lonlat[1]);

            double esp = E * Math.Sin(phi);
            double gfi = Math.Pow(((1.0 - esp) / (1.0 + esp)), _hae);
            double u = 2 * (Math.Atan(Math.Pow(Math.Tan(phi / 2 + S45), _alfa) / _k1 * gfi) - S45);
            double deltav = -lambda * _alfa;
            double cosU = Math.Cos(u);
            double s = Math.Asin((_cosAzim * Math.Sin(u)) + (_sinAzim * cosU * Math.Cos(deltav)));
            double d = Math.Asin(cosU * Math.Sin(deltav) / Math.Cos(s));
            double eps = _n * d;
            double ro = _rop / Math.Pow(Math.Tan(s / 2 + S45), _n);

            /* x and y are reverted  */
            double y = -(ro * Math.Cos(eps)) * SemiMajor;
            double x = -(ro * Math.Sin(eps)) * SemiMajor;

            return CoordinateFactory.Create( x, y );
        }

        /// <summary>
        /// Converts coordinates in projected meters to decimal degrees.
        /// </summary>
        /// <param name="p">Point in meters</param>
        /// <returns>Transformed point in decimal degrees</returns>
        public override TCoordinate MetersToDegrees(TCoordinate p)
        {
            double x = (Double)p[0].Divide(SemiMajor);
            double y = (Double)p[1].Divide(SemiMajor);

            // x -> southing, y -> westing
            double ro = Math.Sqrt(x * x + y * y);
            double eps = Math.Atan2(-x, -y);
            double d = eps / _n;
            double s = 2 * (Math.Atan(Math.Pow(_ro0 / ro, 1 / _n) * _tanS2) - S45);
            double cs = Math.Cos(s);
            double u = Math.Asin((_cosAzim * Math.Sin(s)) - (_sinAzim * cs * Math.Cos(d)));
            double kau = _ka * Math.Pow(Math.Tan((u / 2.0) + S45), 1 / _alfa);
            double deltav = Math.Asin((cs * Math.Sin(d)) / Math.Cos(u));
            double lambda = -deltav / _alfa;
            double phi = 0;
            double fi1 = u;

            // iteration calculation
            for (int i = MAXIMUM_ITERATIONS; ; )
            {
                fi1 = phi;
                double esf = E * Math.Sin(fi1);
                phi = 2.0 * (Math.Atan(kau * Math.Pow((1.0 + esf) / (1.0 - esf), E / 2.0)) - S45);
                if (Math.Abs(fi1 - phi) <= ITERATION_TOLERANCE)
                {
                    break;
                }

                if (--i < 0)
                {
                    break;
                    //throw new ProjectionException(Errors.format(ErrorKeys.NO_CONVERGENCE));
                }
            }

            return CoordinateFactory.Create ( (Degrees) new Radians(lambda + _centralMeridian), (Degrees)new Radians(phi) );
        }

        public override Int32 SourceDimension
        {
            get { throw new System.NotImplementedException(); }
        }

        public override Int32 TargetDimension
        {
            get { throw new System.NotImplementedException(); }
        }

        public override Boolean IsInverse
        {
            get { return false; }
        }

        protected override IMathTransform ComputeInverse(IMathTransform setAsInverse)
        {
            IEnumerable<ProjectionParameter> parameters =
                Caster.Downcast<ProjectionParameter, Parameter>(Parameters);

            return new InverseKrovakProjection<TCoordinate>(parameters, CoordinateFactory);
        }

    }
}

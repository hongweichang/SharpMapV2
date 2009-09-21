#region License

/*
 *  The attached / following is part of ProjNet.
 *  
 *  ProjNet is free software ? 2009 Ingenieurgruppe IVV GmbH & Co. KG, 
 *  www.ivv-aachen.de; you can redistribute it and/or modify it under the terms 
 *  of the current GNU Lesser General Public License (LGPL) as published by and 
 *  available from the Free Software Foundation, Inc., 
 *  59 Temple Place, Suite 330, Boston, MA 02111-1307 USA: http://fsf.org/.
 *  This program is distributed without any warranty; 
 *  without even the implied warranty of merchantability or fitness for purpose.
 *  See the GNU Lesser General Public License for the full details. 
 *  
 *  Author: Felix Obermaier 2009
 *  
 *  This class is inspired by the functionality proj-4's 'pj_mlfn.c'-file provides.
 *  Many thanks.
 */

#endregion
using System;
using GeoAPI.Units;

namespace ProjNet.CoordinateSystems.Projections
{
    ///<summary>
    /// 
    ///</summary>
    public class DistanceAlongMeridianTool
    {
        private const Double C00 = 1.0d;
        private const Double C02 = 0.25d;
        private const Double C04 = 0.046875d;
        private const Double C06 = 0.01953125d;
        private const Double C08 = 0.01068115234375d;
        private const Double C22 = 0.75d;
        private const Double C46 = 0.01302083333333333333d;
        private const Double C44 = 0.46875d;
        private const Double C48 = 0.00712076822916666666d;
        private const Double C66 = 0.36458333333333333333d;
        private const Double C68 = 0.00569661458333333333d;
        private const Double C88 = 0.3076171875d;

        private const Double Eps = 1e-11;
        private const Int32 MaxIter = 10;

        /*
#define C88 
#define Eps 1e-11
#define MaxIter 10
#define EN_SIZE 5
	double *
pj_enfn(double es) {
	double t, *en;

	if (en = (double *)pj_malloc(EN_SIZE * sizeof(double))) {
		en[0] = C00 - es * (C02 + es * (C04 + es * (C06 + es * C08)));
		en[1] = es * (C22 - es * (C04 + es * (C06 + es * C08)));
		en[2] = (t = es * es) * (C44 - es * (C46 + es * C48));
		en[3] = (t *= es) * (C66 - es * C68);
		en[4] = t * es * C88;
	} // else return NULL if unable to allocate memory
	return en;
}
	double
pj_mlfn(double phi, double sphi, double cphi, double *en) {
	cphi *= sphi;
	sphi *= sphi;
	return(en[0] * phi - cphi * (en[1] + sphi*(en[2]
		+ sphi*(en[3] + sphi*en[4]))));
}
	double
pj_inv_mlfn(double arg, double es, double *en) {
	double s, t, phi, k = 1./(1.-es);
	int i;

	phi = arg;
	for (i = MaxIter; i ; --i) { // rarely goes over 2 iterations 
		s = sin(phi);
		t = 1. - es * s * s;
		phi -= t = (pj_mlfn(phi, s, cos(phi), en) - arg) * (t * sqrt(t)) * k;
		if (fabs(t) < Eps)
			return phi;
	}
	pj_errno = -17;
	return phi;
}
         */
        private readonly Double[] _e;
        private readonly Double _eccPow2;

        public DistanceAlongMeridianTool(Double eccPow2)
        {
            /*	if (en = (double *)pj_malloc(EN_SIZE * sizeof(double))) {
		en[0] = C00 - es * (C02 + es * (C04 + es * (C06 + es * C08)));
		en[1] = es * (C22 - es * (C04 + es * (C06 + es * C08)));
		en[2] = (t = es * es) * (C44 - es * (C46 + es * C48));
		en[3] = (t *= es) * (C66 - es * C68);
		en[4] = t * es * C88;
	}*/
            Double eccPow4 = eccPow2*eccPow2;
            Double eccPow6 = eccPow4*eccPow2;
            _e = new[]
                     {
                         C00 - eccPow2*(C02 + eccPow2*(C04 + eccPow2*(C06 + eccPow2*C08))),
                         eccPow2*(C22 - eccPow2*(C04 + eccPow2*(C06 + eccPow2*C08))),
                         eccPow4*(C44 - eccPow2*(C46 + eccPow2*C48)),
                         eccPow6*(C66 - eccPow2*C68),
                         eccPow6 * eccPow2 * C88
                     };

            _eccPow2 = eccPow2;
        }

        /// <summary>
        /// Function computes the value of M which is the distance along a meridian
        /// from the Equator to latitude <paramref name="phi"/>.
        /// </summary>
        /// <param name="phi">The measure of the latitude to measure to, in radians.</param>
        public Double Length(Radians phi)
        {
            return Length(phi, Math.Sin(phi), Math.Cos(phi));
        }

        /// <summary>
        /// Function computes the value of M which is the distance along a meridian
        /// from the Equator to latitude <paramref name="phi"/>.
        /// </summary>
        /// <param name="phi">The measure of the latitude to measure to, in radians.</param>
        public Double Length(Double phi, Double sinPhi, Double cosPhi)
        {
            /*	cphi *= sphi;
	sphi *= sphi;
	return(en[0] * phi - cphi * (en[1] + sphi*(en[2]
		+ sphi*(en[3] + sphi*en[4]))));
*/
            cosPhi *= sinPhi;
            sinPhi *= sinPhi;

            return _e[0] * phi -
                   cosPhi * (_e[1] +
                   sinPhi * (_e[2] +
                   sinPhi * (_e[3] +
                   sinPhi * _e[4])));
        }

        ///<summary>
        /// 
        ///</summary>
        ///<param name="arg"></param>
        ///<returns></returns>
        ///<exception cref="ComputationConvergenceException"></exception>
        public Radians Phi1(Double arg)
        {
/*
	double s, t, phi, k = 1./(1.-es);
	int i;

	phi = arg;
	for (i = MAX_ITER; i ; --i) { // rarely goes over 2 iterations /
		s = sin(phi);
		t = 1. - es * s * s;
		phi -= t = (pj_mlfn(phi, s, cos(phi), en) - arg) * (t * sqrt(t)) * k;
		if (fabs(t) < EPS)
			return phi;
	}*/
            Double k = 1.0d / (1.0d-_eccPow2);

            Double phi = arg;
            for (Int32 i = MaxIter; i > 0 ; --i) { // rarely goes over 2 iterations 
                Double sinPhi = Math.Sin(phi);
                Double t = 1.0d - _eccPow2 * sinPhi * sinPhi;
                t = (Length(phi, sinPhi, Math.Cos(phi)) - arg) * (t * Math.Sqrt(t)) * k;
                phi -= t;
                if (Math.Abs(t) < Eps) return new Radians(phi);
            }
            throw new ComputationConvergenceException();
        }

#region "Not really needed by the public"

        public Double this[Int32 index]
        {
            get { return _e[index]; }
        }

        ///<summary>
        /// The value "E0" which is used in a series to calculate a distance along a meridian.
        ///</summary>
        public Double E0
        {
            get { return _e[0]; }
        }

        ///<summary>
        /// The value "E1" which is used in a series to calculate a distance along a meridian.
        ///</summary>
        public Double E1
        {
            get { return _e[1]; }
        }

        ///<summary>
        /// The value "E2" which is used in a series to calculate a distance along a meridian.
        ///</summary>
        public Double E2
        {
            get { return _e[2]; }
        }

        ///<summary>
        /// The value "E3" which is used in a series to calculate a distance along a meridian.
        ///</summary>
        public Double E3
        {
            get { return _e[3]; }
        }
#endregion
    }
}

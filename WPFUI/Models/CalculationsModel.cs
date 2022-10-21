using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFUI.Models
{
    public class CalculationsModel
    {

        private const double R = 6378.1;
        private const double EPSILON = 1e-6;

        public CalculationsModel()
        {

        }

        public CoordinateModel moveGeodetic(CoordinateModel latlon, double distanceTraveled, double bearing)
        {
            //set intitial latitude and longitude and convert both to radians
            double lat1 = toRadians(latlon.getLat());
            double lon1 = toRadians(latlon.getLon());
            double d = distanceTraveled / 1000.0;
            double b = toRadians(bearing);

            double lat2 = Math.Asin(Math.Sin(lat1) * Math.Cos(d / R) +
                          Math.Cos(lat1) * Math.Sin(d / R) * Math.Cos(b));

            double lon2 = lon1 + Math.Atan2(
                                            Math.Sin(b) * Math.Sin(d / R) * Math.Cos(lat1),
                                            Math.Cos(d / R) - Math.Sin(lat1) * Math.Sin(lat2)
                                            );

            return new CoordinateModel(toDegrees(lat2), toDegrees(lon2));
        }

        public double calculateYaw(double inputYaw, double yawROC, double updateRate)
        {
            return wrapTo180(((360.0 + inputYaw) % 360.0) + yawROC * updateRate / 1000.0);
        }

        public double calculatePitch(double inputPitch, double pitchROC, double updateRate)
        {
            // pitch is limited to +/- 20 deg
            return wrapTo180(inputPitch + pitchROC * updateRate / 1000.0);
        }

        public double calculateRoll(double inputRoll, double rollROC, double updateRate)
        {
            // roll is limited to +/- 40 deg
            return wrapTo180(inputRoll + rollROC * updateRate / 1000.0);
        }

        public double calculateX(double yaw_deg, double airspeed)
        {
            double X = Math.Cos(toRadians(yaw_deg)) * airspeed;
            if (Math.Abs(X) < EPSILON)
            {
                return 0;
            }
            else return X;
        }

        public double calculateY(double yaw_deg, double airspeed)
        {
            double Y = Math.Sin(toRadians(yaw_deg)) * airspeed;
            if (Math.Abs(Y) < EPSILON)
            {
                return 0;
            }
            else return Y;
        }


        public double ConvertAirspeedUnits(string units, double unconvertedSpeed)
        {
            if (units.Equals("mph"))
            {
                return ConvertFromMPH(unconvertedSpeed);
            }
            else if (units.Equals("knots"))
            {
                return ConvertFromKnots(unconvertedSpeed);
            }
            else if (units.Equals("km/h"))
            {
                return ConvertFromKMPH(unconvertedSpeed);
            }
            else if (units.Equals("ft/s"))
            {
                return ConvertFromFTPS(unconvertedSpeed);
            }
            else
            {
                return unconvertedSpeed;
            }
        }

        public double ConvertFromMPH(double mph)
        {
            return mph * 0.44704;
        }

        public double ConvertFromKMPH(double kmph)
        {
            return kmph * 0.27777778;
        }

        public double ConvertFromFTPS(double ftps)
        {
            return ftps * 0.3048;
        }

        public double ConvertFromKnots(double knots)
        {
            return knots * 0.51444444;
        }

        public double ConvertToMeters(double feet)
        {
            return feet * 0.3048;
        }

        public double toRadians(double deg)
        {
            return Math.PI / 180.0 * deg;
        }

        public double toDegrees(double rad)
        {
            return rad * 180.0 / Math.PI;
        }

        public bool AreEqual(double a, double b)
        {
            return Math.Abs(a - b) < EPSILON;
        }

        public double wrapTo180(double input)
        {
            input = (input + 180.0) % 360;
            if (input < 0) input += 360;
            return input - 180;
        }


    }
}

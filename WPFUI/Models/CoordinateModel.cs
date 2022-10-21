using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFUI.Models
{
    public class CoordinateModel
    {

        private double lat;
        private double lon;

        public CoordinateModel()
        {
            lat = 0;
            lon = 0;
        }

        public CoordinateModel(double lat, double lon)
        {
            this.lat = lat;
            this.lon = lon;
        }

        public double getLat()
        {
            return lat;
        }

        public double getLon()
        {

            return lon;
        }

    }
}

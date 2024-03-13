using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Weather
    {
        public Weather()
        {
        }

        public Weather(double temperature, double clouds, double windSpeed)
        {
            Temperature = temperature;
            Clouds = clouds;
            WindSpeed = windSpeed;
        }

        [DataMember]
        public double Temperature { get; set; }
        [DataMember]
        public double Clouds { get; set; }
        [DataMember]
        public double WindSpeed { get; set; }
    }
}

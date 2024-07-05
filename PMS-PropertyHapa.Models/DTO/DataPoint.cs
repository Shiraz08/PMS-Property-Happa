using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PMS_PropertyHapa.Models.DTO
{
    [DataContract]
    public class DataPoint
    {
        public DataPoint(string label, double y)
        {
            this.Label = label;
            this.Y = y;
        }

        public DataPoint(string label, bool isCumulativeSum, string indexLabel)
        {
            this.Label = label;
            this.IsCumulativeSum = isCumulativeSum;
            this.IndexLabel = indexLabel;
        }

        //Explicitly setting the name to be used while serializing to JSON.
        [DataMember(Name = "label")]
        public string Label = "";

        //Explicitly setting the name to be used while serializing to JSON.
        [DataMember(Name = "y")]
        public Nullable<double> Y = null;

        //Explicitly setting the name to be used while serializing to JSON.
        [DataMember(Name = "isCumulativeSum")]
        public bool IsCumulativeSum = false;

        //Explicitly setting the name to be used while serializing to JSON.
        [DataMember(Name = "indexLabel")]
        public string IndexLabel = null;

        

    }

    [DataContract]
    public class LineChartDataPoint
    {
        public LineChartDataPoint(double x, double y)
        {
            this.x = x;
            this.Y = y;
        }

        [DataMember(Name = "x")]
        public Nullable<double> x = null;

        //Explicitly setting the name to be used while serializing to JSON.
        [DataMember(Name = "y")]
        public Nullable<double> Y = null;
    }
}

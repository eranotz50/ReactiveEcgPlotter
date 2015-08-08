using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class EcgSample
    {
        public EcgSample() { }

        public EcgSample(short value, int x, ECGFlags falgs = ECGFlags.None)
        {
            _flags = falgs;
            X = x;
            Y = (int)value;
        }

        public EcgSample(short value, ECGFlags falgs = ECGFlags.None)
        {
            _flags = falgs;
            Y = (int)value;
        }

        public int X { get; set; }
        public int Y { get; set; }
        public TimeSpan TimeStamp { get; set; }

        private ECGFlags _flags;
        public ECGFlags Flags
        {
            get
            {
                return _flags;
            }
            private set
            {
                _flags = value;
            }
        }

        public bool IsRWave
        {
            get
            {
                return Convert.ToBoolean(_flags & ECGFlags.RWave);
            }
        }

        public bool IsXRay
        {
            get
            {
                return Convert.ToBoolean(_flags & ECGFlags.XRay);
            }
        }

        public bool Leads
        {
            get
            {
                return Convert.ToBoolean(_flags & ECGFlags.Leads);
            }
        }

        public override string ToString()
        {
            return string.Format("X : {0} , Y : {1} , TS : {2}", X, Y,TimeStamp);
        }
    }
}

using System;
using System.Collections.Generic;

namespace Domain
{
    public class EcgBuffer
    {
        public IList<EcgSample> Buffer { get; set; }
        public int Index { get; set; }
        public TimeSpan TimeStamp { get; set; }

        public override string ToString()
        {
            return string.Format("---------- Buffer --  Index : {0} , TS : {1}  ----------------------", Index, TimeStamp);
        }
    }
}

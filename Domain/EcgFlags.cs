using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    [Flags]
    public enum ECGFlags : byte
    {
        None = 0x0,
        RWave = 0x01,
        Leads = 0x04,
        XRay = 0x20
    };
}

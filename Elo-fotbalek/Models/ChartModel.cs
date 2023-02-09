using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elo_fotbalek.Models
{
    public class ChartModel : BaseModel
    {
        public DateTime DateTime { get; set; }

        public int Elo { get; set; }
    }
}

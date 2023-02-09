namespace Elo_fotbalek.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class DoodleModel : BaseModel
    {
        public List<Doodle> Doodle { get; set; }

        public DoodleStats Stats { get; set; }

        public bool Disbaled { get; set; }
    }
}

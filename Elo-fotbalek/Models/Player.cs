using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elo_fotbalek.Models
{
    public class Player
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public int Elo { get; set; }

        public override string ToString()
        {
            return this.Name + "(" + this.Elo +")";
        }
    }
}

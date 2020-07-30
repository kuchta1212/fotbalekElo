using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elo_fotbalek.Models
{
    public class Player : IEquatable<Player>
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public int Elo { get; set; }

        public override string ToString()
        {
            return this.Name + "(" + this.Elo +")";
        }

        public bool Equals(Player other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Player) obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}

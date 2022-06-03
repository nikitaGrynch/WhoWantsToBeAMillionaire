using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhoWantsToBeAMillionaire.Player
{
    internal class Player
    {
        public int Balance { get; set; }
        public String Name { get; set; }
        public int FireproofAmount { get; set; }

        public Player()
        {
            Balance = 0;
            FireproofAmount = 0;
            Name = "Unlnown";
        }
        
    }
}

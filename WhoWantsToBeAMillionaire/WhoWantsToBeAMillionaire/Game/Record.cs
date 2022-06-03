using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Threading.Tasks;

namespace WhoWantsToBeAMillionaire.Game
{
    public class Record
    {
        [XmlElement("Name")]
        public String Name { get; set; }
        [XmlElement("Money")]
        public int Money { get; set; }
        public override string ToString()
        {
            return $"[{Name}] - {Money} $";
        }
    }
}

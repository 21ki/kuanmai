using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KM.JXC.BL.Models
{
    public class BEmployee
    {
        public int ID { get; set; }
        public string Department { get; set; }
        public string Name { get; set; }
        public string Duty { get; set; }
        public string Gendar { get; set; }
        public int BirthDate { get; set; }
        public int HireDate { get; set; }
        public int MatureDate { get; set; }
        public string IdentityCard { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int User_ID { get; set; }
    }
}

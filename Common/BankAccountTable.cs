using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class BankAccountTable
    {
        public BankAccountTable()
        {
        }

        public BankAccountTable(string number, double money)
        {
            Number = number;
            Money = money;
        }

        [DataMember]
        [Key]
        public string Number { get; set; }
        [DataMember]
        public double Money { get; set; }
    }
}

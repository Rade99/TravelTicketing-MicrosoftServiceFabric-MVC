using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class BankAccount
    {
        public BankAccount()
        {
        }

        public BankAccount(string number, double money)
        {
            Number = number;
            Money = money;
        }

        [DataMember]
        public string Number { get; set; }
        [DataMember]
        public double Money { get; set; }
    }
}

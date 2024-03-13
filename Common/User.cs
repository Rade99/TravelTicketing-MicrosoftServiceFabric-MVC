using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class User
    {
        [Key]
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public List<string> PurchaseList { get; set; }
        [DataMember]
        public string AccountNumber { get; set; }

        public User()
        {

        }

        public User(string username, string email, string password, List<string> purchaseList, string accountNumber)
        {
            Username = username;
            Email = email;
            Password = password;
            PurchaseList = purchaseList;
            AccountNumber = accountNumber;
        }
    }
}

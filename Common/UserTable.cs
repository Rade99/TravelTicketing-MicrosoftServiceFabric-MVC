using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class UserTable : TableEntity
    {
        [Key]
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public List<string> PurchaseList { get; set; }
        public string AccountNumber { get; set; }

        public UserTable()
        {

        }

        public UserTable(string username, string email, string password, List<string> purchaseList, string accountNumber)
        {
            Username = username;
            Email = email;
            Password = password;
            PurchaseList = purchaseList;
            AccountNumber = accountNumber;
        }
    }
}

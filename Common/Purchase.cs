using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Purchase
    {
        public Purchase()
        {
        }

        public Purchase(string id, string departureId, int ticketAmount)
        {
            Id = id;
            DepartureId = departureId;
            TicketAmount = ticketAmount;
        }

        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string DepartureId { get; set; }
        [DataMember]
        public int TicketAmount { get; set; }
    }
}

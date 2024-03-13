using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class DepartureTable
    {
        public DepartureTable()
        {
        }

        public DepartureTable(string id, string type, double price, string departurePlace, string departureDate, string purchaseDate, string returnDate, int numberOfTickets, int numberOfAvalableTickets, Weather weather)
        {
            Id = id;
            Type = type;
            Price = price;
            DeparturePlace = departurePlace;
            DepartureDate = departureDate;
            PurchaseDate = purchaseDate;
            ReturnDate = returnDate;
            NumberOfTickets = numberOfTickets;
            NumberOfAvalableTickets = numberOfAvalableTickets;
            Weather = weather;
        }

        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string Type { get; set; }
        [DataMember]
        public double Price { get; set; }
        [DataMember]
        public string DeparturePlace { get; set; }
        [DataMember]
        public string DepartureDate { get; set; }
        [DataMember]
        public string PurchaseDate { get; set; }
        [DataMember]
        public string ReturnDate { get; set; }
        [DataMember]
        public int NumberOfTickets { get; set; }
        [DataMember]
        public int NumberOfAvalableTickets { get; set; }
        [DataMember]
        public Weather Weather { get; set; }
    }
}

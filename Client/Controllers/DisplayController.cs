using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using System.Fabric;
using Common;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Client.Controllers
{
    public class DisplayController : Controller
    {
        [HttpPost]
        [Route("/Display/TryDeletePurchase")]
        public async Task<IActionResult> TryDeletePurchase(string id_Purchase)
        {
            try
            {
                string username = Request.Cookies["LoggedIn"];
                string result = "";
                FabricClient fabricClient = new System.Fabric.FabricClient();
                int partitionsNumber = (await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/SistemZaKupovinuKarata/Saver"))).Count;
                int index = 0;

                for (int i = 0; i < partitionsNumber; i++)
                {
                    var proxy = ServiceProxy.Create<ISaver>(
                    new Uri("fabric:/SistemZaKupovinuKarata/Saver"),
                    new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(index % partitionsNumber)
                    );

                    result = await proxy.DeletePurchase(username, id_Purchase);

                    TempData["Error"] = result;

                    index++;
                }

                return RedirectToAction("MyPurchase", "Display");
            }
            catch
            {
                ViewData["Error"] = "Greska!";
                return RedirectToAction("MyPurchase", "Display");
            }
        }

        [HttpGet]
        [Route("/Display/MyPurchase")]
        public async Task<IActionResult> MyPurchase()
        {
            var username = Request.Cookies["LoggedIn"];
            List<Purchase> purchases = new List<Purchase>();

            if (username == null)
            {
                ViewData["Error"] = "Niste Ulogovani";
                ViewBag.Purchases = purchases;
                return View();
            }

            ViewData["Error"] = null;
            
            try
            {
                FabricClient fabricClient = new System.Fabric.FabricClient();
                int partitionsNumber = (await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/SistemZaKupovinuKarata/PublisherSubscriber"))).Count;
                int index = 0;

                for (int i = 0; i < partitionsNumber; i++)
                {
                    var proxy = ServiceProxy.Create<IPublisherSubscriber>(
                    new Uri("fabric:/SistemZaKupovinuKarata/PublisherSubscriber"),
                    new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(index % partitionsNumber)
                    );

                    purchases = await proxy.GetUserPurchases(username);

                    index++;
                }

                ViewBag.Purchases = purchases;
                return View();
            }
            catch
            {
                ViewData["Error"] = "Servis nije dostupan";
                return View();
            }
        }


        [HttpGet]
        [Route("/Display/Purchase")]
        public async Task<IActionResult> Purchase()
        {
            ViewData["Error"] = null;
            List<Purchase> purchases = new List<Purchase>();

            try
            {
                FabricClient fabricClient = new System.Fabric.FabricClient();
                int partitionsNumber = (await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/SistemZaKupovinuKarata/PublisherSubscriber"))).Count;
                int index = 0;

                for (int i = 0; i < partitionsNumber; i++)
                {
                    var proxy = ServiceProxy.Create<IPublisherSubscriber>(
                    new Uri("fabric:/SistemZaKupovinuKarata/PublisherSubscriber"),
                    new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(index % partitionsNumber)
                    );

                    purchases = await proxy.GetPurchaseActiveData();

                    index++;
                }

                ViewBag.Purchases = purchases;
                return View();
            }
            catch
            {
                ViewData["Error"] = "Servis nije dostupan";
                return View();
            }
        }

        [HttpGet]
        [Route("/Display/Deparature")]
        public async Task<IActionResult> Deparature()
        {
            ViewData["Error"] = null;
            List<Departure> deparatures = new List<Departure>();
         
            try
            {
                FabricClient fabricClient = new System.Fabric.FabricClient();
                int partitionsNumber = (await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/SistemZaKupovinuKarata/PublisherSubscriber"))).Count;
                int index = 0;

                for (int i = 0; i < partitionsNumber; i++)
                {
                    var proxy = ServiceProxy.Create<IPublisherSubscriber>(
                    new Uri("fabric:/SistemZaKupovinuKarata/PublisherSubscriber"),
                    new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(index % partitionsNumber)
                    );

                    deparatures = await proxy.GetDeparatureActiveData();

                    index++;
                }
                deparatures = deparatures.FindAll(x => DateTime.Parse(x.DepartureDate) >= DateTime.Now);

                ViewBag.Deparatures = deparatures;
                return View();
            }
            catch
            {
                ViewData["Error"] = "Servis nije dostupan";
                return View();
            }
        }

        [HttpGet]
        [Route("/Display/BankAccount")]
        public async Task<IActionResult> BankAccount()
        {
            ViewData["Error"] = null;
            List<BankAccount> bankAccounts = new List<BankAccount>();

            try
            {
                FabricClient fabricClient = new System.Fabric.FabricClient();
                int partitionsNumber = (await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/SistemZaKupovinuKarata/PublisherSubscriber"))).Count;
                int index = 0;

                for (int i = 0; i < partitionsNumber; i++)
                {
                    var proxy = ServiceProxy.Create<IPublisherSubscriber>(
                    new Uri("fabric:/SistemZaKupovinuKarata/PublisherSubscriber"),
                    new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(index % partitionsNumber)
                    );

                    bankAccounts = await proxy.GetBankAccountActiveData();

                    index++;
                }

                ViewBag.bankAccounts = bankAccounts;
                return View();
            }
            catch
            {
                ViewData["Error"] = "Servis nije dostupan";
                return View();
            }
        }

        [HttpPost]
        [Route("/Display/Filter")]
        public async Task<IActionResult> Filter(Departure d)
        {
            ViewData["Error"] = null;
            List<Departure> deparatures = new List<Departure>();

            try
            {
                FabricClient fabricClient = new System.Fabric.FabricClient();
                int partitionsNumber = (await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/SistemZaKupovinuKarata/PublisherSubscriber"))).Count;
                int index = 0;

                for (int i = 0; i < partitionsNumber; i++)
                {
                    var proxy = ServiceProxy.Create<IPublisherSubscriber>(
                    new Uri("fabric:/SistemZaKupovinuKarata/PublisherSubscriber"),
                    new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(index % partitionsNumber)
                    );

                    deparatures = await proxy.GetDeparatureActiveData();

                    index++;
                }

                if(!string.IsNullOrEmpty(d.Type))
                {
                    deparatures = deparatures.FindAll(x => x.Type == d.Type);
                }
                if (d.NumberOfAvalableTickets != 0)
                {
                    deparatures = deparatures.FindAll(x => x.NumberOfAvalableTickets >= d.NumberOfAvalableTickets);
                }
                if (!string.IsNullOrEmpty(d.DepartureDate))
                {
                    deparatures = deparatures.FindAll(x => DateTime.Parse(x.DepartureDate) == DateTime.Parse(d.DepartureDate));
                }

                ViewBag.Deparatures = deparatures;
                return View("Deparature");
            }
            catch
            {
                ViewData["Error"] = "Servis nije dostupan";
                return View("Deparature");
            }
        }


    }
}

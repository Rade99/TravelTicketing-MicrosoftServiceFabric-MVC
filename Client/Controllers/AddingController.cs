using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System.Fabric;

namespace Client.Controllers
{
    public class AddingController : Controller
    {
        public IActionResult Deparature()
        {
            return View();
        }

        public IActionResult BankAccount()
        {
            return View();
        }

        [HttpPost]
        [Route("/Adding/TryAddDeparature")]
        public async Task<IActionResult> TryAddDeparature(Departure d)
        {
            d.Id = IdGenerator.GetIdAsString();
            if(d.DepartureDate != "" && DateTime.Parse(d.DepartureDate) >= DateTime.Parse(d.ReturnDate))
            {
                ViewData["Error"] = "Pazite Na Odabir Datuma!";
                return View("Deparature");
            }

            try
            {
                bool result = true;
                FabricClient fabricClient = new System.Fabric.FabricClient();
                int partitionsNumber = (await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/SistemZaKupovinuKarata/Saver"))).Count;
                int index = 0;

                for (int i = 0; i < partitionsNumber; i++)
                {
                    var proxy = ServiceProxy.Create<ISaver>(
                    new Uri("fabric:/SistemZaKupovinuKarata/Saver"),
                    new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(index % partitionsNumber)
                    );

                    result = await proxy.AddDeparature(d);

                    index++;
                }

                if (result)
                {
                    ViewData["Error"] = "Polazak Uspesno Dodat!";
                }
                else
                {
                    ViewData["Error"] = "Polazak NIJE Dodat!";
                }

                return View("Deparature");
            }
            catch
            {
                ViewData["Error"] = "Polazak NIJE Dodat!!";
                return View("Deparature");
            }
        }

            [HttpPost]
            [Route("/Adding/TryAddBankAccount")]
            public async Task<IActionResult> TryAddBankAccount(BankAccount b)
            {
                try
                {
                    bool result = true;
                    FabricClient fabricClient = new System.Fabric.FabricClient();
                    int partitionsNumber = (await fabricClient.QueryManager.GetPartitionListAsync(new Uri("fabric:/SistemZaKupovinuKarata/Saver"))).Count;
                    int index = 0;

                    for (int i = 0; i < partitionsNumber; i++)
                    {
                        var proxy = ServiceProxy.Create<ISaver>(
                        new Uri("fabric:/SistemZaKupovinuKarata/Saver"),
                        new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(index % partitionsNumber)
                        );

                        result = await proxy.AddBankAccount(b);

                        index++;
                    }

                    if (result)
                    {
                        ViewData["Error"] = "Broj Racuna Je Uspesno Dodat!";
                    }
                    else
                    {
                        ViewData["Error"] = "Broj Racuna NIJE Dodat!";
                    }

                    return View("BankAccount");
                }
                catch
                {
                    ViewData["Error"] = "Broj Racuna NIJE Dodat!";
                    return View("BankAccount");
                }

            }

        [HttpPost]
        [Route("/Adding/TryAddPurchase")]
        public async Task<IActionResult> TryAddPurchase(string id_deparature, int amount)
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

                    result = await proxy.AddPurchase(username, id_deparature, amount);

                    TempData["Error"] = result;

                    index++;
                }

               return RedirectToAction("Deparature","Display");
            }
            catch
            {
                ViewData["Error"] = "Greska Na Servisu!";
                return View("Deparature");
            }
        }
    }
}

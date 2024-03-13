using Common;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System.Fabric;

namespace Client.Controllers
{
    public class LogInRegisterController : Controller
    {

        public async Task<IActionResult> Index()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult Options()
        {
            return View();
        }

        [HttpPost]
        [Route("/LogInRegister/TryLogin")]
        public async Task<IActionResult> TryLogin(User u)
        {
            ViewData["Error"] = null;
            List<User> users = new List<User>();

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

                    users = await proxy.GetUserActiveData();

                    index++;
                }


                if(users.Exists(x => x.Username == u.Username && x.Password == u.Password))
                {
                    CookieOptions option = new CookieOptions();

                    option.Expires = DateTime.Now.AddMinutes(100);

                    Response.Cookies.Append("LoggedIn", u.Username, option);

                
                }
                else
                {
                    ViewData["Error"] = "Pogresno uneto korisnicko ime ili lozinka";
                    return View("Index");
                }

                return RedirectToAction("Deparature", "Display");

            }
            catch
            {
                ViewData["Error"] = "Servis ne Radi!";
                return View("Index");
            }
        }

        [HttpPost]
        [Route("/LogInRegister/Register")]
        public async Task<IActionResult> TryRegisterUser(User u)
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

                    result = await proxy.AddUser(u);

                    index++;
                }

                if (result)
                {
                    ViewData["Error"] = "Korisnik Uspesno Dodat!";
                }
                else
                {
                    ViewData["Error"] = "Korisnik NIJE Dodat!";
                }

                return View("Index");
            }
            catch
            {
                ViewData["Error"] = "Korisnik NIJE Dodat!";
                return View("Index");
            }

        }

    }
}

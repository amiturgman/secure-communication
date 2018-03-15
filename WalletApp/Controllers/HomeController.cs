using System.Diagnostics;
using System.Threading.Tasks;
using Blockchain;
using Communication;
using Microsoft.AspNetCore.Mvc;
using WalletApp.Models;

namespace WalletApp.Controllers
{
    public class HomeController : Controller
    {
        AzureQueue m_queue;
        EthereumAccount m_ethereumAccount;

        public HomeController(IQueue queue, EthereumAccount ethereumAccount)
        {
            m_queue = (AzureQueue)queue;
            m_ethereumAccount = ethereumAccount;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public virtual ActionResult SendFunds(HomeViewModel model)
        {
            var message = $"{model.Amount};{"SenderAccount"};{model.DestinationAddress}";
            m_queue.EnqueueAsync(Utils.ToByteArray(message)).Wait();
            return View("Index");
        }

        public async Task<ActionResult> GetBalanceAsync(HomeViewModel model)
        {
            var balance = await m_ethereumAccount.GetCurrentBalance(model.WalletAddress);
            ViewBag.model = new HomeViewModel() { Balance = balance,
                        WalletAddress = model.WalletAddress};

            return View("Index");
        }
    }
}

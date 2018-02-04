using System;
using Microsoft.AspNetCore.Mvc;
using Wallet.Communication;
using Wallet.Cryptography;

namespace ilanatest.Controllers
{
    public class SimplePerfController : Controller
    {
        bool isInit;
        AzureQueue m_comm;
        KeyVault kv;

        public SimplePerfController( IQueue _comm)
        {
            m_comm = (AzureQueue) _comm;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }


        public string Enqueue()
        {
            m_comm.EnqueueAsync(Utils.ToByteArray<string>("Hi Bye")).Wait();
            return "Enqueued";
        }

        public string Dequeue()
        {
            m_comm.DequeueAsync((msg) =>
            {
                Console.WriteLine(msg);
            }, (msg) =>
            {
                Console.WriteLine("Failed");
            }, TimeSpan.FromSeconds(1)).Wait();
            return "dequeue";

        }
    }
}
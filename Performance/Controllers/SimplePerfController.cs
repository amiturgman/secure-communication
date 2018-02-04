using System;
using Microsoft.AspNetCore.Mvc;
using Wallet.Communication;
using Wallet.Cryptography;

namespace Performance.Controllers
{
    public class SimplePerfController : Controller
    {
        AzureQueue m_comm;

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

        public string StartDequeue()
        {
            m_comm.DequeueAsync((msg) =>
            {
                Console.WriteLine(msg);
            }, (msg) =>
            {
                Console.WriteLine("Failed processing message from queue");
            }, TimeSpan.FromSeconds(1));

            return "Started listening";
        }

        public string StopDequeue()
        {
            m_comm.CancelListeningOnQueue();

            return "Stopped";
        }
    }
}
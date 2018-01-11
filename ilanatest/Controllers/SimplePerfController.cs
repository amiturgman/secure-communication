using System;
using Contracts;
using Microsoft.AspNetCore.Mvc;
using SecuredCommunication;

namespace ilanatest.Controllers
{
    public class SimplePerfController : Controller
    {
        bool isInit;
        AzureQueueImpl m_comm;
        KeyVault kv;

        public SimplePerfController( IQueueManager _comm)
        {
            m_comm = (AzureQueueImpl) _comm;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }


        public string Enqueue()
        {
            m_comm.EnqueueAsync("Hi Bye").Wait();
            return "Enqueued";
        }

        public string Dequeue()
        {
            m_comm.DequeueAsync((msg) =>
            {
                Console.WriteLine(msg);
            }, TimeSpan.FromSeconds(1)).Wait();
            return "dequeue";

        }
    }
}
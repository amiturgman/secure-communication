using System;
using System.Collections.Generic;
using System.Xml.Xsl;
using Contracts;
using Xunit;

namespace UnitTests
{
    public class MessageTests
    {
        [Fact]
        public void Test_Message_Created_As_Expected()
        {
            //var isEncrypted = true;
            //var 
            try
            {
                var msg = new Message(true, null, null);
            }
            catch(Exception exc) {
                Console.WriteLine(exc);
            }
        }
    }
}
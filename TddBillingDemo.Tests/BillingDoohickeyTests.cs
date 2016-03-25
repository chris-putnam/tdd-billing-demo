using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TddBillingDemo.Tests
{
    class BillingDoohickeyTests
    {
        [Fact]
        public void Monkey()
        {
            // just start writing your test without thinking too hard about the implementation
            BillingDoohickey thing = new BillingDoohickey();
            thing.ProcessMonth(2001, 8);
        }
        //Monthly billing
        //Grace period for missed payments ("dunning" status)
        //Not all customers are necessarily subscribers
        //Idle customers should be automatically unsubscribed
    }

    public class Customer
    {
        
    }
}

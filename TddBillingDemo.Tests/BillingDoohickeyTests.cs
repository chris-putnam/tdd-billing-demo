using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace TddBillingDemo.Tests
{
    class BillingDoohickeyTests
    {
        [Fact]
        public void Monkey()
        {
            // just start writing your test without thinking too hard about the implementation

            // we need a source for customers
            // we need a service to charge customers

            ICustomerRepository repo = new Mock<ICustomerRepository>();
            ICreditCardCharger charger = new Mock<ICreditCardCharger>();

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

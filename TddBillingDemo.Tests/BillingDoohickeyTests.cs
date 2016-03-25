using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace TddBillingDemo.Tests
{
    public class BillingDoohickeyTests
    {
        [Fact]
        public void CustomerWhoDoesNotHaveSubscriptionDoesNotGetCharged()
        {
            // just start writing your test without thinking too hard about the implementation

            // we need a source for customers
            // we need a service to charge customers

            var repo = new Mock<ICustomerRepository>();
            var charger = new Mock<ICreditCardCharger>();
            var customer = new Customer(); // what does it mean to not have a subscription

            BillingDoohickey thing = new BillingDoohickey(repo.Object, charger.Object);
            thing.ProcessMonth(2001, 8);

            charger.Verify(c => c.ChargeCustomer(customer), Times.Never());
        }

        //Monthly billing
        //Grace period for missed payments ("dunning" status)
        //Not all customers are necessarily subscribers
        //Idle customers should be automatically unsubscribed
    }

    public class BillingDoohickey
    {
        private ICreditCardCharger _charger;
        private ICustomerRepository _repo;

        public BillingDoohickey(ICustomerRepository repo, ICreditCardCharger charger)
        {
            _repo = repo;
            _charger = charger;
        }

        internal void ProcessMonth(int year, int month)
        {
        }
    }

    public interface ICreditCardCharger
    {
        void ChargeCustomer(Customer customer);
    }

    public interface ICustomerRepository
    {
    }

    public class Customer
    {
        
    }
}

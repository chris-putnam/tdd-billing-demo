﻿using System;
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
            var customer = new Customer(); 

            var billingProcessor = TestableBillingProcessor.Create(customer);

            billingProcessor.ProcessMonth(2001, 8);

            billingProcessor.Charger.Verify(c => c.ChargeCustomer(customer), Times.Never());
        }

        [Fact]
        public void CustomerWithSubscriptionThatIsExpiredGetsCharged()
        {
            
            var customer = new Customer {Subscribed = true};

            var billingProcessor = TestableBillingProcessor.Create(customer);

            billingProcessor.ProcessMonth(2001, 8);

            billingProcessor.Charger.Verify(c => c.ChargeCustomer(customer), Times.Once());
        }

        //Monthly billing
        //Grace period for missed payments ("dunning" status)
        //Not all customers are necessarily subscribers
        //Idle customers should be automatically unsubscribed

    }

    public class TestableBillingProcessor : BillingProcessor
    {
        public Mock<ICreditCardCharger> Charger;
        public Mock<ICustomerRepository> Repository;

        public TestableBillingProcessor(Mock<ICustomerRepository> repo, Mock<ICreditCardCharger> charger) : base(repo.Object, charger.Object)
        {
            Repository = repo;
            Charger = charger;
        }

        // factory function for creating instances of the testable billing processor
        public static TestableBillingProcessor Create(params Customer[] customers)
        {
            Mock<ICustomerRepository> repo = new Mock<ICustomerRepository>();
            repo.Setup(r => r.Customers)
                .Returns(customers);

            return new TestableBillingProcessor(repo, new Mock<ICreditCardCharger>());
        }
    }

    public class BillingProcessor
    {
        private ICreditCardCharger _charger;
        private ICustomerRepository _repo;

        public BillingProcessor(ICustomerRepository repo, ICreditCardCharger charger)
        {
            _repo = repo;
            _charger = charger;
        }

        internal void ProcessMonth(int year, int month)
        {
            var customer = _repo.Customers.Single();
            if (customer.Subscribed)
            {
                _charger.ChargeCustomer(customer);
            }
        }
    }

    public interface ICreditCardCharger
    {
        void ChargeCustomer(Customer customer);
    }

    public interface ICustomerRepository
    {
        IEnumerable<Customer> Customers { get; set; }
    }

    public class Customer
    {
        public bool Subscribed { get; set; }
    }
}

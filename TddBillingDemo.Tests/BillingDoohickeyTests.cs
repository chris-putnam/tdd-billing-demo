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
            var customer = new Customer(); 

            var billingProcessor = TestableBillingProcessor.Create(customer);

            billingProcessor.ProcessMonth(2011, 8);

            billingProcessor.Charger.Verify(c => c.ChargeCustomer(customer), Times.Never());
        }

        [Fact]
        public void CustomerWithSubscriptionThatIsExpiredGetsCharged()
        {
            
            var customer = new Customer {Subscribed = true};

            var billingProcessor = TestableBillingProcessor.Create(customer);

            billingProcessor.ProcessMonth(2011, 8);

            billingProcessor.Charger.Verify(c => c.ChargeCustomer(customer), Times.Once());
        }

        [Fact]
        public void CustomersWithSubscriptionThatIsCurrentDoesNotGetCharged()
        {
            var customer = new Customer { Subscribed = true, PaidThroughYear = 2011, PaidThroughMonth = 8};

            var billingProcessor = TestableBillingProcessor.Create(customer);

            billingProcessor.ProcessMonth(2011, 8);

            billingProcessor.Charger.Verify(c => c.ChargeCustomer(customer), Times.Never());
        }

        [Fact]
        public void CustomersWithSubscriptionThatIsPaidThroughNextYearDoesNotGetCharged()
        {
            var customer = new Customer { Subscribed = true, PaidThroughYear = 2012, PaidThroughMonth = 8 };

            var billingProcessor = TestableBillingProcessor.Create(customer);

            billingProcessor.ProcessMonth(2011, 8);

            billingProcessor.Charger.Verify(c => c.ChargeCustomer(customer), Times.Never());
        }

        [Fact]
        public void CustomerWhoIsSubscribedAndDueToPayButFailsOnceIsStillSubscribed()
        {
            var customer = new Customer { Subscribed = true, PaidThroughYear = 2012, PaidThroughMonth = 8 };

            var billingProcessor = TestableBillingProcessor.Create(customer);

            billingProcessor.Charger.Setup(c => c.ChargeCustomer(It.IsAny<Customer>())).Returns(false);

            billingProcessor.ProcessMonth(2011, 8);

            Assert.True(customer.Subscribed);
        }

        [Fact]
        public void CustomerWhoIsSubscribedAndDueToPayButFailsThreeTimesIsNoLongerSubscribed()
        {
            var customer = new Customer { Subscribed = true };

            var billingProcessor = TestableBillingProcessor.Create(customer);

            billingProcessor.Charger.Setup(c => c.ChargeCustomer(It.IsAny<Customer>())).Returns(false);


            for (int i = 0; i < BillingProcessor.MAX_FAILURES; i++)
            {
                billingProcessor.ProcessMonth(2011, 8);
            }

            Assert.False(customer.Subscribed);
        }

        //Monthly billing
        //Grace period for missed payments ("dunning" status)
        //Not all customers are necessarily subscribers
        //Idle customers should be automatically unsubscribed
        //What about customers who sign up today?

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
        public const int MAX_FAILURES = 3;
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
            if (customer.Subscribed && 
                (customer.PaidThroughYear <= year &&
                customer.PaidThroughMonth < month))
            {
                bool charged = _charger.ChargeCustomer(customer);
                if (!charged)
                {
                    if (++customer.PaymentFailures >= MAX_FAILURES)
                    {
                        customer.Subscribed = false;
                    }
                }
            }
        }
    }

    public interface ICreditCardCharger
    {
        bool ChargeCustomer(Customer customer);
    }

    public interface ICustomerRepository
    {
        IEnumerable<Customer> Customers { get; set; }
    }

    public class Customer
    {
        // Is this really customer data or subscription data?
        public int PaidThroughMonth { get; set; }
        public int PaidThroughYear { get; set; }
        public int PaymentFailures { get; set; }
        public bool Subscribed { get; set; }
    }
}

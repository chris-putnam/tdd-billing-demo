using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace TddBillingDemo.Tests
{
    public class BillingProcessorTests
    {
        public class NoSubscription
        {
            [Fact]
            public void CustomerWhoDoesNotHaveSubscriptionDoesNotGetCharged()
            {
                var customer = new Customer();

                var billingProcessor = TestableBillingProcessor.Create(customer);

                billingProcessor.ProcessMonth(2011, 8);

                billingProcessor.Charger.Verify(c => c.ChargeCustomer(customer), Times.Never());
            }
        }

        public class Monthly
        {
            [Fact]
            public void CustomerWithSubscriptionThatIsExpiredGetsCharged()
            {

                var subscription = new MonthlySubscription();
                var customer = new Customer { Subscription = subscription };

                var billingProcessor = TestableBillingProcessor.Create(customer);

                billingProcessor.ProcessMonth(2011, 8);

                billingProcessor.Charger.Verify(c => c.ChargeCustomer(customer), Times.Once());
            }

            [Fact]
            public void CustomersWithSubscriptionThatIsCurrentDoesNotGetCharged()
            {
                var subscription = new MonthlySubscription { PaidThroughYear = 2011, PaidThroughMonth = 8 };

                var customer = new Customer { Subscription = subscription };

                var billingProcessor = TestableBillingProcessor.Create(customer);

                billingProcessor.ProcessMonth(2011, 8);

                billingProcessor.Charger.Verify(c => c.ChargeCustomer(customer), Times.Never());
            }

            [Fact]
            public void CustomersWithSubscriptionThatIsPaidThroughNextYearDoesNotGetCharged()
            {
                var subscription = new MonthlySubscription { PaidThroughYear = 2012, PaidThroughMonth = 1 };

                var customer = new Customer { Subscription = subscription };

                var billingProcessor = TestableBillingProcessor.Create(customer);

                billingProcessor.ProcessMonth(2011, 8);

                billingProcessor.Charger.Verify(c => c.ChargeCustomer(customer), Times.Never());
            }

            [Fact]
            public void CustomerWhoIsSubscribedAndDueToPayButFailsOnceIsStillCurrent()
            {
                var subscription = new MonthlySubscription();

                var customer = new Customer { Subscription = subscription };

                var billingProcessor = TestableBillingProcessor.Create(customer);

                billingProcessor.Charger.Setup(c => c.ChargeCustomer(It.IsAny<Customer>())).Returns(false);

                billingProcessor.ProcessMonth(2011, 8);

                Assert.True(customer.Subscription.IsCurrent);
            }

            [Fact]
            public void CustomerWhoIsSubscribedAndDueToPayButFailsThreeTimesIsNoLongerSubscribed()
            {
                var subscription = new MonthlySubscription();

                var customer = new Customer { Subscription = subscription };

                var billingProcessor = TestableBillingProcessor.Create(customer);

                billingProcessor.Charger.Setup(c => c.ChargeCustomer(It.IsAny<Customer>())).Returns(false);


                for (int i = 0; i < MonthlySubscription.MaxFailures; i++)
                {
                    billingProcessor.ProcessMonth(2011, 8);
                }

                Assert.False(customer.Subscription.IsCurrent);
            }
        }

        public class Annual
        {
            
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
            if(NeedsBilling(year, month, customer))
            {
                bool charged = _charger.ChargeCustomer(customer);
                customer.Subscription.RecordChargedResult(charged);
            }
        }

        private static bool NeedsBilling(int year, int month, Customer customer)
        {
            return customer.Subscription != null 
                && customer.Subscription.NeedsBilling(year, month);
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

    public abstract class Subscription
    {
        public abstract bool IsRecurring { get; }
        public abstract bool IsCurrent { get; }
        public abstract bool NeedsBilling(int year, int month);

        public virtual void RecordChargedResult(bool charged)
        {
        }
    }

    public class AnnualSubscription : Subscription
    {
        public override bool IsRecurring => false;
        public override bool IsCurrent { get { throw new NotImplementedException(); } }

        public override bool NeedsBilling(int year, int month)
        {
            throw new NotImplementedException();
        }
    }

    public class MonthlySubscription : Subscription
    {
        public const int MaxFailures = 3;
        private int _failureCounter;
        public override bool IsRecurring => true;
        public override bool IsCurrent => _failureCounter < MaxFailures;

        public int PaidThroughMonth { get; set; }
        public int PaidThroughYear { get; set; }

        public override bool NeedsBilling(int year, int month)
        {
            return PaidThroughYear <= year &&
                    PaidThroughMonth < month;
        }

        public override void RecordChargedResult(bool charged)
        {

            if (!charged)
            {
                _failureCounter++;
            }
            base.RecordChargedResult(charged);
        }
    }

    public class Customer
    {
        /*public int PaymentFailures { get; set; }
        public bool Subscribed { get; set; }*/
        public Subscription Subscription { get; internal set; }
    }
}

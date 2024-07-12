using HogWildSystem.DAL;
using HogWildSystem.Entities;
using HogWildSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HogWildSystem.BLL
{
    public class CustomerService
    {
        private readonly HogWildContext _hogWildContext;

        internal CustomerService(HogWildContext hogWildContext)
        {
            _hogWildContext = hogWildContext;
        }

        public List<CustomerSearchView> GetCustomers(string lastName, string phone)
        {
            if (string.IsNullOrWhiteSpace(lastName) && string.IsNullOrWhiteSpace(phone))
            {
                throw new ArgumentException
                ("Please enter a last name and/or a phone number.");
            }

            if (string.IsNullOrWhiteSpace(lastName))
            {
                lastName = Guid.NewGuid().ToString();
            }

            if (string.IsNullOrWhiteSpace(phone))
            {
                phone = Guid.NewGuid().ToString();
            }

            return _hogWildContext.Customers
                .Where(customer => (customer.LastName.Contains(lastName)
                    || customer.Phone.Contains(phone))
                    && !customer.RemoveFromViewFlag)
                .Select(customer => new CustomerSearchView
                {
                    CustomerID = customer.CustomerID,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    City = customer.City,
                    Phone = customer.Phone,
                    Email = customer.Email,
                    StatusID = customer.StatusID,
                    TotalSales = customer.Invoices.Sum(customer => customer.SubTotal + customer.Tax)
                })
                .OrderBy(customer => customer.LastName)
                .ToList();
        }
    }
}

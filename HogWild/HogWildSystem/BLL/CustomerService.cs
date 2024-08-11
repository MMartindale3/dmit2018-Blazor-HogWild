using HogWildSystem.DAL;
using HogWildSystem.Entities;
using HogWildSystem.ViewModels;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable disable
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
            if (string.IsNullOrWhiteSpace(lastName)
                    && string.IsNullOrWhiteSpace(phone))
            {
                throw new ArgumentException
                ("Enter a last name and/or a phone number!");
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
                .Where(x => (x.LastName.Contains(lastName)
                            || x.Phone.Contains(phone))
                            && !x.RemoveFromViewFlag)
                .Select(x => new CustomerSearchView
                {
                    CustomerID = x.CustomerID,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    City = x.City,
                    Phone = x.Phone,
                    Email = x.Email,
                    StatusID = x.StatusID,
                    TotalSales = x.Invoices.Sum(x => x.SubTotal + x.Tax)
                })
                .OrderBy(x => x.LastName)
                .ToList();
        }

        public CustomerEditView GetCustomer(int customerID)
        {
            if (customerID == 0)
            {
                throw new ArgumentException("Please provide a valid customer ID");
            }

            return _hogWildContext.Customers
                    .Where(customer => customer.CustomerID == customerID
                            && !customer.RemoveFromViewFlag)
                    .Select(customer => new CustomerEditView
                    {
                        CustomerID = customer.CustomerID,
                        FirstName = customer.FirstName,
                        LastName = customer.LastName,
                        Address1 = customer.Address1,
                        Address2 = customer.Address2,
                        City = customer.City,
                        ProvStateID = customer.ProvStateID,
                        CountryID = customer.CountryID,
                        PostalCode = customer.PostalCode,
                        Phone = customer.Phone,
                        Email = customer.Email,
                        StatusID = customer.StatusID,
                        RemoveFromViewFlag = customer.RemoveFromViewFlag
                    })
                    .FirstOrDefault();
        }

        public CustomerEditView Save(CustomerEditView editCustomer)
        {
            #region Business Logic and Parameter Exceptions
            List<Exception> errorList = new List<Exception>();

            if (editCustomer == null)
            {
                throw new ArgumentNullException("No customer was supplied!");
            }

            if (string.IsNullOrWhiteSpace(editCustomer.FirstName))
            {
                errorList.Add(new Exception("First name is required!"));
            }

            if (string.IsNullOrWhiteSpace(editCustomer.LastName))
            {
                errorList.Add(new Exception("Last name is required!"));
            }

            if (string.IsNullOrWhiteSpace(editCustomer.Phone))
            {
                errorList.Add(new Exception("Phone is required!"));
            }

            if (string.IsNullOrWhiteSpace(editCustomer.Email))
            {
                errorList.Add(new Exception("Email is required!"));
            }

            if (editCustomer.CustomerID == 0)
            {
                if (_hogWildContext.Customers.Where(customer => customer.FirstName == editCustomer.FirstName
                                                && customer.LastName == editCustomer.LastName
                                                && customer.Phone == editCustomer.Phone)
                            .Any())
                {
                    errorList.Add(new Exception("Customer already exisits!"));
                }
            }

            if (errorList.Count > 0)
            {
                throw new AggregateException
                    ("Please check error message(s): ", errorList);
            }
            #endregion

            Customer customer = _hogWildContext.Customers
                        .Where(customer => customer.CustomerID == editCustomer.CustomerID)
                        .Select(customer => customer).FirstOrDefault();

            if (customer == null)
            {
                customer = new Customer();
            }

            customer.FirstName = editCustomer.FirstName;
            customer.LastName = editCustomer.LastName;
            customer.Address1 = editCustomer.Address1;
            customer.Address2 = editCustomer.Address2;
            customer.City = editCustomer.City;
            customer.ProvStateID = editCustomer.ProvStateID;
            customer.CountryID = editCustomer.CountryID;
            customer.PostalCode = editCustomer.PostalCode;
            customer.Phone = editCustomer.Phone;
            customer.Email = editCustomer.Email;
            customer.StatusID = editCustomer.StatusID;
            customer.RemoveFromViewFlag = editCustomer.RemoveFromViewFlag;

            if (errorList.Count > 0)
            {
                _hogWildContext.ChangeTracker.Clear();

                throw new AggregateException
                    ("Please check error message(s): ", errorList);
            }
            else
            {
                if (customer.CustomerID == 0)
                {
                    _hogWildContext.Customers.Add(customer);
                }
                else
                {
                    _hogWildContext.Customers.Update(customer);
                }

                _hogWildContext.SaveChanges();
            }

            editCustomer.CustomerID = customer.CustomerID;
            return editCustomer;
        }
    }
}

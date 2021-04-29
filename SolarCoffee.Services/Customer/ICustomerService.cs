using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolarCoffee.Services.Customer
{
    public interface ICustomerService
    {
        List<Data.Models.Customer> GetAllCustomers();
        ServiceResponse<Data.Models.Customer> CreateCustomer(Data.Models.Customer customer);
        ServiceResponse<bool> DeleteCustomer(int id);
        Data.Models.Customer GetById(int customerId);
    }
}

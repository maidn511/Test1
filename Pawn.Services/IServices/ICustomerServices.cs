using Pawn.ViewModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawn.Services
{
    public interface ICustomerServices
    {
        List<CustomerModels> LoadDataCustomer(string strKeyword, int intStatusId, int intPageSize, int intPageIndex, int storeId = -1);
        CustomerModels LoadDetailCustomer(int? intCustomerId);
        IEnumerable<CustomerModels> GetCustomerByStoreId(int? storeId);
        int AddCustomer(CustomerModels pawnCustomer);
        int DeleteCustomer(int idCustomer, string strUserDelete);
    }
}

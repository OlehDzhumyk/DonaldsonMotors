using DonaldsonMotors.API.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DonaldsonMotors.API.Interfaces.Repositories
{
    public interface IInvoiceRepository
    {
        Task<InvoiceEntity?> GetByIdAsync(int id);
        Task<IEnumerable<InvoiceEntity>> ListAsync();
        Task AddAsync(InvoiceEntity invoiceEntity);
        void Update(InvoiceEntity invoiceEntity);
        void Delete(InvoiceEntity invoiceEntity);
        Task SaveChangesAsync();
    }
}
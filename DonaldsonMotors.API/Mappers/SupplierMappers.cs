using DonaldsonMotors.API.Domain.Models;
using DonaldsonMotors.API.Data.Entities;

namespace DonaldsonMotors.API.Mappers
{
    public static class SupplierMappers
    {
        public static Supplier ToDomainModel(this SupplierEntity entity)
        {
            return new Supplier
            {
                Id = entity.Id,
                Name = entity.Name,
                AddressLine1 = entity.AddressLine1,
                AddressLine2 = entity.AddressLine2,
                Postcode = entity.Postcode,
                Telephone = entity.Telephone,
                EmailContact = entity.EmailContact
            };
        }

        public static SupplierEntity ToEntity(this Supplier supplier)
        {
            return new SupplierEntity
            {
                Id = supplier.Id,
                Name = supplier.Name,
                AddressLine1 = supplier.AddressLine1,
                AddressLine2 = supplier.AddressLine2,
                Postcode = supplier.Postcode,
                Telephone = supplier.Telephone,
                EmailContact = supplier.EmailContact
            };
        }
    }
}
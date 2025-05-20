using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.Domain.Models;

namespace DonaldsonMotors.API.Mappers
{
    public static class PaymentMapper
    {
        // From Entity to Domain Model
        public static Payment ToDomainModel(this PaymentEntity entity)
        {
            return new Payment
            {
                Id = entity.Id,
                Method = entity.Method,
                Amount = entity.Amount,
                DatePaid = entity.DatePaid,
                Invoice = entity.Invoice?.ToDomainModel()
            };
        }

        // From Domain Model to Entity
        public static PaymentEntity ToEntity(this Payment payment)
        {
            return new PaymentEntity
            {
                Id = payment.Id,
                Method = payment.Method,
                Amount = payment.Amount,
                DatePaid = payment.DatePaid,
                InvoiceId = payment.Invoice?.Id ?? 0,
                Invoice = payment.Invoice?.ToEntity()
            };
        }
    }
}
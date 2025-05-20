using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.Domain.Models;

namespace DonaldsonMotors.API.Mappers
{
    public static class InvoiceMapper
    {
        // From Entity to Domain Model
        public static Invoice ToDomainModel(this InvoiceEntity entity)
        {
            return new Invoice
            {
                Id = entity.Id,
                TotalCost = entity.TotalCost,
                DateIssued = entity.DateIssued,
                Booking = entity.Booking.ToDomainModel(),
                Payments = entity.Payments?.Select(p => p.ToDomainModel()).ToList() ?? new List<Payment>()
            };
        }

        // From Domain Model to Entity
        public static InvoiceEntity ToEntity(this Invoice invoice)
        {
            return new InvoiceEntity
            {
                Id = invoice.Id,
                TotalCost = invoice.TotalCost,
                DateIssued = invoice.DateIssued,
                BookingId = invoice.Booking.Id,
                Booking = invoice.Booking.ToEntity(), // Assumes BookingMapper.ToEntity exists
                Payments = invoice.Payments?.Select(p => p.ToEntity()).ToList() ?? new List<PaymentEntity>()
            };
        }
    }
}
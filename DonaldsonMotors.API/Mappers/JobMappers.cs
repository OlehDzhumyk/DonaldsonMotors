using DonaldsonMotors.API.Domain.Models;
using DonaldsonMotors.API.Data.Entities;

namespace DonaldsonMotors.API.Mappers
{
    public static class JobMappers
    {
        public static Job ToDomainModel(this JobEntity entity)
        {
            return new Job
            {
                Id = entity.Id,
                Description = entity.Description,
                LabourCost = entity.LabourCost,
                PartsCost = entity.PartsCost,
                StartDate = entity.StartDate,
                CompletionDate = entity.CompletionDate,
                Booking = entity.Booking?.ToDomainModel(),
                Technician = entity.Technician?.ToDomainModel(),
                Items = entity.Items?.Select(ji => new JobItem
                {
                    Item = ji.Item?.ToDomainModel(),
                    QuantityUsed = ji.QuantityUsed
                }).ToList() ?? new List<JobItem>()
            };
        }

        public static JobEntity ToEntity(this Job job)
        {
            var entity = new JobEntity
            {
                Id = job.Id,
                Description = job.Description,
                LabourCost = job.LabourCost,
                PartsCost = job.PartsCost,
                StartDate = job.StartDate,
                CompletionDate = job.CompletionDate,
                BookingId = job.Booking?.Id ?? 0,
                TechnicianId = job.Technician?.Id ?? 0,
                Booking = job.Booking?.ToEntity(),
                Technician = job.Technician?.ToEntity(),
                Items = job.Items?.Select(ji => new JobItemEntity
                {
                    JobEntityId = job.Id,
                    ItemEntityId = ji.Item?.Id ?? 0,
                    QuantityUsed = ji.QuantityUsed,
                    Item = ji.Item?.ToEntity()
                }).ToList() ?? new List<JobItemEntity>()
            };
            return entity;
        }
    }
}
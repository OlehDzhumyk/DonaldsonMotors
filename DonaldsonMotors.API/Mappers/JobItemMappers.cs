using DonaldsonMotors.API.Domain.Models;
using DonaldsonMotors.API.Data.Entities;

public static class JobItemMappers
{
    public static JobItem ToDomainModel(this JobItemEntity entity)
    {
        return new JobItem
        {
            Item = entity.Item?.ToDomainModel(),
            QuantityUsed = entity.QuantityUsed
        };
    }

    public static JobItemEntity ToEntity(this JobItem jobItem)
    {
        return new JobItemEntity
        {
            ItemEntityId = jobItem.Item?.Id ?? 0,
            Item = jobItem.Item?.ToEntity(),
            QuantityUsed = jobItem.QuantityUsed
        };
    }
}
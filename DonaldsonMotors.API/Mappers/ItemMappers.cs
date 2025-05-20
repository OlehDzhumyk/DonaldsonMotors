using DonaldsonMotors.API.Domain.Models;
using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.Mappers;

public static class ItemMappers
{
    public static Item ToDomainModel(this ItemEntity entity)
    {
        return new Item
        {
            Id = entity.Id,
            Name = entity.Name,
            Price = entity.Price,
            CurrentStock = entity.CurrentStock,
            Supplier = entity.Supplier?.ToDomainModel()
        };
    }

    public static ItemEntity ToEntity(this Item item)
    {
        return new ItemEntity
        {
            Id = item.Id,
            Name = item.Name,
            Price = item.Price,
            CurrentStock = item.CurrentStock,
            SupplierId = item.Supplier?.Id ?? 0,
            Supplier = item.Supplier?.ToEntity()
        };
    }
}
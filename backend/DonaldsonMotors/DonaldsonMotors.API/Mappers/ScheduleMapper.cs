using DonaldsonMotors.API.Data.Entities;
using DonaldsonMotors.API.DTOs.Schedule;

namespace DonaldsonMotors.API.Mappers
{
    public static class ScheduleMapper
    {
        /// <summary>
        /// Maps a ScheduleException entity to a ScheduleExceptionResponseDto.
        /// </summary>
        public static ScheduleExceptionResponseDto ToScheduleExceptionResponseDto(this ScheduleException entity)
        {
            if (entity == null) return null!;

            return new ScheduleExceptionResponseDto
            {
                Id = entity.Id,
                // Ensure Date is correctly mapped if it's DateOnly in entity and DateTime in DTO
                Date = new DateTime(entity.Date.Year, entity.Date.Month, entity.Date.Day),
                Description = entity.Description
            };
        }

        /// <summary>
        /// Maps a CreateScheduleExceptionDto to a ScheduleException entity.
        /// </summary>
        public static ScheduleException ToScheduleExceptionEntity(this CreateScheduleExceptionDto dto)
        {
            if (dto == null) return null!;

            return new ScheduleException
            {
                // DateOnly.FromDateTime handles the conversion from DateTime to DateOnly
                Date = DateOnly.FromDateTime(dto.Date),
                Description = dto.Description
            };
        }
    }
}
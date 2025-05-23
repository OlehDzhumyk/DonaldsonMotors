namespace DonaldsonMotors.API.Data.Entities
{
    public enum BookingStatus
    {
        Pending,          // Створено клієнтом, очікує на призначення механіка
        Assigned,         // Механіка призначено
        InProgress,       // Механік розпочав роботу
        AwaitingPayment,  // Роботу завершено, очікується оплата
        Paid,             // Оплачено
        Cancelled         // Скасовано клієнтом або менеджером
    }
}
// Domain/Models/Job.cs
using DonaldsonMotors.API.Data.Entities;

namespace DonaldsonMotors.API.Domain.Models
{
    public class Job
    {
        public int Id { get; set; }
        public Booking Booking { get; set; } = null!;
        public Employee Technician { get; set; } = null!; // Employee domain model
        public string Description { get; set; } = null!;
        public decimal LabourCost { get; set; }
        public decimal PartsCost { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public List<JobItem> Items { get; set; } = new();
    }
}

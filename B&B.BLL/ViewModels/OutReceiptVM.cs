using B_B.DAL.Entity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace B_B.BLL.ViewModels
{
    public class OutReceiptVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; } = DateTime.Now;

        // Either existing client OR new client must be provided
        public int? ClientId { get; set; }

        [Display(Name = "New Client Name")]
        public string? NewClientName { get; set; }

        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "New Client Phone")]
        public string? NewClientPhone { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Paid amount must be >= 0")]
        public decimal PaidAmount { get; set; }
        public decimal RefundAmount { get; set; }

        // At least one product must be chosen
        [MinLength(1, ErrorMessage = "At least one product must be added")]
        public List<ReceiptDetailVM> ReceiptDetails { get; set; } = new();

        public int? PlumberId { get; set; }
        public string? NewPlumberName { get; set; }
        public string? NewPlumberPhone { get; set; }

        // Dropdown lists (populated in controller)
        public IEnumerable<SelectListItem>? Clients { get; set; }
        public IEnumerable<SelectListItem>? Products { get; set; }
        public IEnumerable<SelectListItem>? Plumbers { get; set; }
    }
}

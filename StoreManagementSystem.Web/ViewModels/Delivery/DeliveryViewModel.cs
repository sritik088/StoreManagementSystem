using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace StoreManagementSystem.Web.ViewModels.Delivery
{
    public class DeliveryViewModel
    {
        public int Id { get; set; }

        public string DeliveryNumber { get; set; }
            = string.Empty;

        [Required]
        public int SalesOrderId { get; set; }

        public string SalesOrderNumber { get; set; }
            = string.Empty;

        [Required]
        public int WarehouseId { get; set; }

        public string WarehouseName { get; set; }
            = string.Empty;

        [Required]
        public DateTime DeliveryDate { get; set; }
            = DateTime.Today;

        public string? Remarks { get; set; }

        public List<SelectListItem> SalesOrders
        { get; set; }
            = new();

        public List<SelectListItem> Warehouses
        { get; set; }
            = new();

        public List<DeliveryItemViewModel> Items
        { get; set; }
            = new();

        public decimal TotalAmount =>
            Items.Sum(x => x.Total);
    }
}
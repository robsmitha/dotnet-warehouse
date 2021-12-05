using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SqlServer.App.Models
{
    public class Sale
    {
        public int Id { get; set; }
        public DateTime SaleDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public decimal TotalSaleAmount { get; set; }

        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }
}

using System.ComponentModel.DataAnnotations.Schema;

namespace Order.API.Models
{
    public class OrderItem
    {
        //Id => OrderItemId de yazılabilirdi. EF bu iki yazım kuralından primary key olduğunu anlar. Bu yüzden Key attribute kullanmadık
        public int Id { get; set; }

        public int ProductId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        //OrderId yazılımından EF anlayacağı için aşağıdaki Order property de ForeinKey attribute kullanmaya gerek duymadık.
        public int OrderId { get; set; }

        public Order Order { get; set; }

        public int Count { get; set; }


    }
}

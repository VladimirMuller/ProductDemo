using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ProductDemo.Models
{
    public sealed class Product
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; } = null;
    }
}

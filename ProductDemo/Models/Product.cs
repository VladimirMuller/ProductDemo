using Microsoft.EntityFrameworkCore;
using ProductDemo.Dto;
using System.ComponentModel.DataAnnotations;

namespace ProductDemo.Models
{
    public class Product : ProductDto
    {
        [Key] public int Id { get; set; }

        public Product(int id, ProductDto dto) : base(dto)
        {
            Id = id;
        }
        public Product() : base() { }

    }
}

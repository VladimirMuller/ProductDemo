namespace ProductDemo.Dto
{
    public class ProductDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; } = null;

        public ProductDto() { } 

        public ProductDto(string name, string? description)
        {
            Name = name;
            Description = description;
        }

        public ProductDto(ProductDto dto) : this(dto.Name, dto.Description) { }
    };

}

using System;
using System.Collections.Generic;

namespace RadarProdutos.Application.DTOs
{
    public class ProductAnalysisDto
    {
        public Guid Id { get; set; }
        public string Keyword { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public List<ProductDto> Products { get; set; } = new List<ProductDto>();
    }
}

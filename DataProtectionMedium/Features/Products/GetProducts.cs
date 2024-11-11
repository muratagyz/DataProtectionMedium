using Carter;
using DataProtectionMedium.Contracts;
using DataProtectionMedium.Entities;
using MediatR;
using Microsoft.AspNetCore.DataProtection;

namespace DataProtectionMedium.Features.Products;

public static class GetProducts
{
    public class Query : IRequest<List<ProductResponse>>
    {
        internal sealed class Handler : IRequestHandler<Query, List<ProductResponse>>
        {
            private readonly IDataProtector _protector;
            
            public Handler(IDataProtectionProvider provider)
            {
                _protector = provider.CreateProtector("Key");
            }
            
            public async Task<List<ProductResponse>> Handle(Query request, CancellationToken cancellationToken)
            {
                var products = GenerateFakeProducts(10);

                var response = products.Select(x => new ProductResponse
                {
                    Id = _protector.Protect(x.id.ToString()),
                    Name = x.name,
                    Price = x.price,
                    Stock = x.stock,
                }).ToList();

                return response;
            }
        }
    }
    
    public static List<Product> GenerateFakeProducts(int count)
    {
        var random = new Random();
        var products = new List<Product>();

        for (int i = 0; i < count; i++)
        {
            var product = new Product(
                id: Guid.NewGuid(),
                name: "Product " + (i + 1),
                stock: random.Next(0, 1000),   
                price: Math.Round((decimal)(random.NextDouble() * 1000), 2) 
            );
            products.Add(product);
        }

        return products;
    }
}

public class GetProductsEndPoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("api/product", async (ISender sender) =>
        {
            var query = new GetProducts.Query { };

            var result = await sender.Send(query);

            return result as List<ProductResponse>;
        });
    }
}


using Itm.Discount.Api.Dtos;
using Microsoft.AspNetCore.Mvc.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Servicios
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Zona de datos
var discountDB = new List<DiscountDto>
{
    new ("ITM50", 0.5m),
};

// Endpoints
app.MapGet("/api/discounts/{code}", (string code) =>
{
    var discount = discountDB.FirstOrDefault(p => p.Code == code);
    return discount is not null ? Results.Ok(discount) : Results.NotFound();
});

app.Run();
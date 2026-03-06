using System.Net;
using System.Net.Http.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddHttpClient("EventClient", client =>
    {    
        client.BaseAddress = new Uri("http://localhost:5259");
        client.Timeout = TimeSpan.FromSeconds(5);
    })
    .AddStandardResilienceHandler();

builder.Services
    .AddHttpClient("DiscountClient", client =>
    {
        client.BaseAddress = new Uri("http://localhost:5071");
        client.Timeout = TimeSpan.FromSeconds(5);
    })
    .AddStandardResilienceHandler();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/api/bookings", async (BookingRequest request, IHttpClientFactory factory) =>
{
    var eventClient = factory.CreateClient("EventClient");
    var discountClient = factory.CreateClient("DiscountClient");

    // 1. Validación paralela: consultamos evento y descuento en paralelo
    var eventResponseTask = eventClient.GetAsync($"/api/events/{request.EventId}");
    var discountResponseTask = discountClient.GetAsync($"/api/discounts/{request.DiscountCode}");

    await Task.WhenAll(eventResponseTask, discountResponseTask);

    var eventResponse = await eventResponseTask;
    var discountResponse = await discountResponseTask;

    if (!eventResponse.IsSuccessStatusCode)
        return Results.BadRequest("El evento no existe.");

    var eventDto = await eventResponse.Content.ReadFromJsonAsync<EventDto>();

    DiscountDto? discountDto = null;
    if (discountResponse.IsSuccessStatusCode)
    {
        discountDto = await discountResponse.Content.ReadFromJsonAsync<DiscountDto>();
    }
    else if (discountResponse.StatusCode == HttpStatusCode.NotFound)
    {
        // Código no existe -> sin descuento
        discountDto = null;
    }
    else
    {
        // Error en servicio de descuentos -> continuar sin descuento
        Console.WriteLine($"[Booking] Error al consultar descuentos: {discountResponse.StatusCode}");
    }

    // Matemáticas: calcular total
    decimal subtotal = eventDto!.BasePrice * request.Tickets;
    decimal discountAmount = discountDto is not null ? subtotal * discountDto.Percentage : 0m;
    decimal total = subtotal - discountAmount;

    // 2. Reserva (Paso 1 de la SAGA)
    var reserveResponse = await eventClient.PostAsJsonAsync("/api/events/reserve",
        new { EventId = request.EventId, Quantity = request.Tickets });

    if (!reserveResponse.IsSuccessStatusCode)
        return Results.BadRequest("No hay sillas suficientes o el evento no existe.");

    try
    {
        // 3. Simulación de pago
        bool paymentSuccess = new Random().Next(1, 10) > 5;
        if (!paymentSuccess) throw new Exception("Fondos insuficientes en la tarjeta de crédito.");

        // Pago exitoso -> retornar factura
        return Results.Ok(new
        {
            Status = "Éxito",
            Invoice = new
            {
                Event = eventDto!.Name,
                Tickets = request.Tickets,
                Subtotal = subtotal,
                Discount = discountAmount,
                Total = total
            }
        });
    }
    catch (Exception ex)
    {
        // 4. Compensación: liberar sillas
        Console.WriteLine($"[SAGA] Error en pago: {ex.Message}. Liberando sillas...");

        await eventClient.PostAsJsonAsync("/api/events/release",
            new { EventId = request.EventId, Quantity = request.Tickets });

        return Results.Problem("El pago falló, las sillas fueron liberadas.");
    }
});

app.Run();

public record BookingRequest(int EventId, int Tickets, string DiscountCode);
public record DiscountDto(string Code, decimal Percentage);
public record EventDto(int Id, string Name, int BasePrice, int AvailableChairs);
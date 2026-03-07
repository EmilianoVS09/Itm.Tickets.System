// Itm.Event.Api
// Microservicio responsable por gestionar el aforo del evento.
// Endpoints:
//   GET  /api/events/{id}       -> Obtener información del evento
//   POST /api/events/reserve     -> Reservar sillas (resta cantidad)
//   POST /api/events/release     -> Liberar sillas (compensación / suma cantidad)
// Datos simulados en memoria: Event Id=1, Nombre="Concierto ITM", Precio=50000, SillasDisponibles=100
using Itm.Event.Api.Dtos;

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
var eventsDB = new List<EventDto>
{
    new (1, "Concierto ITM", 50000, 100),
};

// Endpoints
app.MapGet("/api/events/{id}", (int id) =>
{
    var events = eventsDB.FirstOrDefault(p => p.Id == id);
    return events is not null ? Results.Ok(events) : Results.NotFound();
});

app.MapPost("/api/events/reserve", (ReduceQuantityDto reduce) =>
{
    if (reduce.Quantity <= 0)
        return Results.BadRequest(new { Error = "La cantidad debe ser mayor que cero." });
    var events = eventsDB.FirstOrDefault(p => p.Id == reduce.EventId);

    if (events is null)
        return Results.NotFound(new { Error = "No hay eventos programados actualmente con ese código" });
    if (events.AvailableChairs < reduce.Quantity)
        return Results.BadRequest(new { Error = "No hay suficientes sillas disponibles.", CurrentQuantity = events.AvailableChairs });
    var index = eventsDB.IndexOf(events);
    eventsDB[index] = events with { AvailableChairs = events.AvailableChairs - reduce.Quantity };
    return Results.Ok(new { Message = "Cantidad de sillas actualizada", NewQuantity = eventsDB[index].AvailableChairs });
});

app.MapPost("/api/events/release", (ReduceQuantityDto reduce) =>
{
    if (reduce.Quantity <= 0)
        return Results.BadRequest(new { Error = "La cantidad debe ser mayor que cero." });
    var events = eventsDB.FirstOrDefault(p => p.Id == reduce.EventId);
    if (events is null)
        return Results.NotFound(new { Error = "No hay eventos programados actualmente con ese código" });
    var index = eventsDB.IndexOf(events);
    eventsDB[index] = events with { AvailableChairs = events.AvailableChairs + reduce.Quantity };
    Console.WriteLine($"[COMPENSACIÓN] Se devolvieron {reduce.Quantity} sillas al evento {events.Name}. Cantidad actual: {eventsDB[index].AvailableChairs}");
    return Results.Ok(new { Message = "Cantidad de sillas reestablecida por fallo de transacción", NewQuantity = eventsDB[index].AvailableChairs });
});

app.Run();
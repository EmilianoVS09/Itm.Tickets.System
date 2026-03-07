namespace Itm.Booking.Api.Dtos;

// DTO que representa la petición de reserva enviada al Orquestador (Booking.Api).
// Campos:
//  - EventId: identificador del evento a comprar
//  - Tickets: cantidad de boletos solicitados (debe ser > 0)
//  - DiscountCode: código de descuento opcional (ej. "ITM50")
// Nota: este DTO se serializa como JSON en la petición POST /api/bookings.
public record BookingRequest(int EventId, int Tickets, string DiscountCode);

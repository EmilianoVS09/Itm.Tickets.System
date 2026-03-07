namespace Itm.Event.Api.Dtos;

// DTO usado para operaciones que modifican la cantidad de sillas de un evento.
// Se utiliza en los endpoints POST /api/events/reserve y POST /api/events/release.
// Campos:
//  - EventId: id del evento al que se aplica la modificación
//  - Quantity: cantidad de sillas a reservar o liberar (debe ser > 0)
public record ReduceQuantityDto(int EventId, int Quantity);

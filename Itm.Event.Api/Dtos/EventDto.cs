namespace Itm.Event.Api.Dtos;

// DTO usado por Itm.Event.Api para exponer la información de un evento.
// Nota: este DTO es parte del contrato HTTP JSON. Campos:
//  - Id: identificador del evento
//  - Name: nombre del evento
//  - BasePrice: precio base por ticket (entero en esta implementación)
//  - AvailableChairs: cantidad de sillas disponibles actualmente
// En producción se recomienda usar decimal para el precio y un origen de datos persistente.
public record EventDto(int Id, string Name, int BasePrice, int AvailableChairs);

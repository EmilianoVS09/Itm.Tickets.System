namespace Itm.Discount.Api.Dtos;

// DTO utilizado por Itm.Discount.Api para representar un descuento.
// Campos:
//  - Code: código identificador del descuento (ej. "ITM50")
//  - Percentage: fracción a descontar (ej. 0.5m = 50%)
// Comportamiento: si el código no existe, el endpoint devuelve 404 y Booking lo trata como "sin descuento".
public record DiscountDto(string Code, decimal Percentage);

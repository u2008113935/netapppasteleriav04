namespace apppasteleriav04.Models.Enums
{
    /// <summary>
    /// Represents the possible statuses of an order
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// Order has been placed but not yet processed
        /// </summary>
        Pendiente,

        /// <summary>
        /// Order is being prepared in the kitchen
        /// </summary>
        EnPreparacion,

        /// <summary>
        /// Order is ready for pickup or delivery
        /// </summary>
        Listo,

        /// <summary>
        /// Order is out for delivery
        /// </summary>
        EnCamino,

        /// <summary>
        /// Order has been delivered to the customer
        /// </summary>
        Entregado,

        /// <summary>
        /// Order has been cancelled
        /// </summary>
        Cancelado
    }

    /// <summary>
    /// Helper class for OrderStatus enum
    /// </summary>
    public static class OrderStatusExtensions
    {
        /// <summary>
        /// Convert OrderStatus enum to database string value
        /// </summary>
        public static string ToDbString(this OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pendiente => "pendiente",
                OrderStatus.EnPreparacion => "en_preparacion",
                OrderStatus.Listo => "listo",
                OrderStatus.EnCamino => "en_camino",
                OrderStatus.Entregado => "entregado",
                OrderStatus.Cancelado => "cancelado",
                _ => "pendiente"
            };
        }

        /// <summary>
        /// Convert database string value to OrderStatus enum
        /// </summary>
        public static OrderStatus FromDbString(string status)
        {
            return status?.ToLower() switch
            {
                "pendiente" => OrderStatus.Pendiente,
                "en_preparacion" => OrderStatus.EnPreparacion,
                "listo" => OrderStatus.Listo,
                "en_camino" => OrderStatus.EnCamino,
                "entregado" => OrderStatus.Entregado,
                "cancelado" => OrderStatus.Cancelado,
                _ => OrderStatus.Pendiente
            };
        }

        /// <summary>
        /// Get display text for OrderStatus
        /// </summary>
        public static string GetDisplayText(this OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pendiente => "Pendiente",
                OrderStatus.EnPreparacion => "En Preparación",
                OrderStatus.Listo => "Listo",
                OrderStatus.EnCamino => "En Camino",
                OrderStatus.Entregado => "Entregado",
                OrderStatus.Cancelado => "Cancelado",
                _ => "Desconocido"
            };
        }
    }
}

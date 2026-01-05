namespace apppasteleriav04.Constants
{
    /// <summary>
    /// Constants for database table names and common values
    /// </summary>
    public static class DatabaseConstants
    {
        // Table names
        public const string TABLE_PRODUCTS = "productos";
        public const string TABLE_ORDERS = "pedidos";
        public const string TABLE_ORDER_ITEMS = "pedido_items";
        public const string TABLE_PROFILES = "profiles";
        public const string TABLE_EMPLOYEES = "employees";
        public const string TABLE_ORDER_LOCATIONS = "order_locations";

        // Order status values
        public const string ORDER_STATUS_PENDING = "pendiente";
        public const string ORDER_STATUS_PREPARING = "en_preparacion";
        public const string ORDER_STATUS_READY = "listo";
        public const string ORDER_STATUS_IN_DELIVERY = "en_camino";
        public const string ORDER_STATUS_DELIVERED = "entregado";
        public const string ORDER_STATUS_CANCELLED = "cancelado";

        // Employee roles
        public const string ROLE_KITCHEN = "cocina";
        public const string ROLE_DELIVERY = "reparto";
        public const string ROLE_BACKOFFICE = "backoffice";
        public const string ROLE_VENDOR = "vendedor";
        public const string ROLE_MANAGER = "gerente";
        public const string ROLE_CUSTOMER = "cliente";

        // Cache keys
        public const string CACHE_KEY_PRODUCTS = "cached_products";
        public const string CACHE_KEY_ORDERS = "cached_orders";
        public const string CACHE_KEY_EMPLOYEE_PROFILE = "cached_employee_profile";
    }
}


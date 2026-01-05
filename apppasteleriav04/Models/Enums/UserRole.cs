namespace apppasteleriav04.Models.Enums
{
    /// <summary>
    /// Represents the different user roles in the system
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// Regular customer who can browse and order products
        /// </summary>
        Cliente,

        /// <summary>
        /// Vendor who manages their products and sales
        /// </summary>
        Vendedor,

        /// <summary>
        /// Employee who works in the kitchen
        /// </summary>
        Cocina,

        /// <summary>
        /// Employee who handles deliveries
        /// </summary>
        Reparto,

        /// <summary>
        /// Employee who manages backoffice operations
        /// </summary>
        Backoffice,

        /// <summary>
        /// Manager with administrative privileges
        /// </summary>
        Gerente
    }

    /// <summary>
    /// Helper class for UserRole enum
    /// </summary>
    public static class UserRoleExtensions
    {
        /// <summary>
        /// Convert UserRole enum to database string value
        /// </summary>
        public static string ToDbString(this UserRole role)
        {
            return role switch
            {
                UserRole.Cliente => "cliente",
                UserRole.Vendedor => "vendedor",
                UserRole.Cocina => "cocina",
                UserRole.Reparto => "reparto",
                UserRole.Backoffice => "backoffice",
                UserRole.Gerente => "gerente",
                _ => "cliente"
            };
        }

        /// <summary>
        /// Convert database string value to UserRole enum
        /// </summary>
        public static UserRole FromDbString(string role)
        {
            return role?.ToLower() switch
            {
                "cliente" => UserRole.Cliente,
                "vendedor" => UserRole.Vendedor,
                "cocina" => UserRole.Cocina,
                "reparto" => UserRole.Reparto,
                "backoffice" => UserRole.Backoffice,
                "gerente" => UserRole.Gerente,
                _ => UserRole.Cliente
            };
        }

        /// <summary>
        /// Check if role is an employee role
        /// </summary>
        public static bool IsEmployee(this UserRole role)
        {
            return role == UserRole.Cocina || 
                   role == UserRole.Reparto || 
                   role == UserRole.Backoffice;
        }
    }
}


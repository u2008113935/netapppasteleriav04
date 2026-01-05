using SQLite;
using System;

namespace apppasteleriav04.Models.Local
{
    /// <summary>
    /// Local SQLite entity for caching products offline
    /// </summary>
    [Table("products")]
    public class LocalProduct
    {
        [PrimaryKey]
        [Column("id")]
        public string Id { get; set; } = string.Empty;

        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("descripcion")]
        public string? Descripcion { get; set; }

        [Column("categoria")]
        public string? Categoria { get; set; }

        [Column("imagen_url")]
        public string? ImagenUrl { get; set; }

        [Column("precio")]
        public decimal Precio { get; set; }

        [Column("last_synced")]
        public DateTime LastSynced { get; set; } = DateTime.UtcNow;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;
    }
}

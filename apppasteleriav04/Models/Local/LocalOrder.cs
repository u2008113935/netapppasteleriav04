using SQLite;
using System;
using System.Collections.Generic;

namespace apppasteleriav04.Models.Local
{
    /// <summary>
    /// Local SQLite entity for storing orders offline
    /// </summary>
    [Table("orders")]
    public class LocalOrder
    {
        [PrimaryKey, AutoIncrement]
        [Column("id")]
        public int Id { get; set; }

        [Column("remote_id")]
        public string? RemoteId { get; set; }

        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        [Column("total")]
        public decimal Total { get; set; }

        [Column("status")]
        public string Status { get; set; } = "pendiente";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("synced")]
        public bool Synced { get; set; } = false;

        [Column("synced_at")]
        public DateTime? SyncedAt { get; set; }

        [Column("repartidor_asignado")]
        public string? RepartidorAsignado { get; set; }

        [Column("latitud_actual")]
        public double? LatitudActual { get; set; }

        [Column("longitud_actual")]
        public double? LongitudActual { get; set; }

        [Column("hora_est_llegada")]
        public DateTime? HoraEstimadaLlegada { get; set; }

        [Column("entregado_en")]
        public DateTime? EntregadoEn { get; set; }

        /// <summary>
        /// Navigation property for order items (not stored in SQLite, loaded separately)
        /// </summary>
        [Ignore]
        public List<LocalOrderItem> Items { get; set; } = new List<LocalOrderItem>();
    }
}

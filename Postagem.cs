using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace Postagens.Models
{
    [Table("Postagens")]
    public class Postagem
    {
        [Key]
        [JsonIgnore]
        public int PostagemId { get; set; }

        [Required]
        [StringLength(50)]
        public string Titulo { get; set; }

        [Required]
        [StringLength(50)]
        public string Mensagem { get; set; }

        [JsonIgnore]
        public byte[]? Imagem { get; set; } // Campo para armazenar a imagem binária

    }

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Postagem>? Postagems { get; set; }
    }

}

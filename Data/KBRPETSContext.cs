using KBRPETS.Models;
using Microsoft.EntityFrameworkCore;

namespace KBRPETS.Data {
    public class KBRPETSContext : DbContext {

        // Classe de configuração do Banco de Dados

        public KBRPETSContext(DbContextOptions<KBRPETSContext> options) : base(options) {
        }

        // Construtor de Opções e conexão do MySQL
        // Utilizando o Banco de Dados gratuito do Db4free.net
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            optionsBuilder.UseMySql("Server=db4free.net;Port=3306;Database=kbrpets_db;User=kbrtec_dbadmin;Password=TR#*rm87SP5me5k", new MySqlServerVersion(new Version(8, 0, 21)));
        }


        // DBSets
        public DbSet<Pet> Pets { get; set; }
        public DbSet<Solicitations> Solicitations { get; set; }
        public DbSet<User> Users { get; set; }

    }
    
}

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Groepsreizen_team_tet.Data
{
    public class GroepsreizenContext : IdentityDbContext<CustomUser, CustomRole, int>
    {
        public GroepsreizenContext(DbContextOptions<GroepsreizenContext> options) : base(options)
        { }

        public DbSet<Activiteit> Activiteiten { get; set; }
        public DbSet<Programma> Programmas { get; set; }
        public DbSet<Bestemming> Bestemmingen { get; set; }
        public DbSet<Foto> Fotos { get; set; }
        public DbSet<Groepsreis> Groepsreizen { get; set; }
        public DbSet<Onkosten> OnkostenLijst { get; set; }
        public DbSet<Deelnemer> Deelnemers { get; set; }
        public DbSet<Models.Monitor> Monitoren { get; set; }
        public DbSet<Kind> Kinderen { get; set; }
        public DbSet<Opleiding> Opleidingen { get; set; }
        public DbSet<CustomUser> CustomUsers { get; set; }
        public DbSet<Wachtlijst> Wachtlijst { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuratie voor de precisie van 'Prijs' in de Groepsreis-entiteit
            modelBuilder.Entity<Groepsreis>()
                .Property(g => g.Prijs)
                .HasPrecision(18, 2); // Maximaal 18 cijfers in totaal, 2 decimalen

            // Configuratie voor de precisie van 'Bedrag' in Onkosten
            modelBuilder.Entity<Onkosten>()
                  .Property(o => o.Bedrag)
                  .HasPrecision(18, 2);

            // Eén-op-veel-relatie tussen Activiteit en Programma
            modelBuilder.Entity<Programma>()
                .HasOne(p => p.Activiteit)
                .WithMany(a => a.Programmas)
                .HasForeignKey(p => p.ActiviteitId);

            // Eén-op-veel-relatie tussen Groepsreis en Programma
            modelBuilder.Entity<Programma>()
                .HasOne(p => p.Groepsreis)
                .WithMany(g => g.Programmas)
                .HasForeignKey(p => p.GroepsreisId);

            // Eén-op-veel-relatie tussen Bestemming en Foto
            modelBuilder.Entity<Foto>()
                .HasOne(f => f.Bestemming)
                .WithMany(b => b.Fotos)
                .HasForeignKey(f => f.BestemmingId);

            // Eén-op-veel-relatie tussen Bestemming en Groepsreis
            modelBuilder.Entity<Groepsreis>()
                .HasOne(g => g.Bestemming)
                .WithMany(b => b.Groepsreizen)
                .HasForeignKey(g => g.BestemmingId);

            // Eén-op-veel-relatie tussen Groepsreis en Monitor
            modelBuilder.Entity<Models.Monitor>()
                .HasOne(m => m.GroepsReis)
                .WithMany(g => g.Monitoren)
                .HasForeignKey(m => m.GroepsreisDetailsId);

            // Eén-op-veel-relatie tussen Groepsreis en Deelnemer
            modelBuilder.Entity<Deelnemer>()
                .HasOne(d => d.Groepsreis)
                .WithMany(g => g.Deelnemers)
                .HasForeignKey(d => d.GroepsreisDetailsId);

            // Eén-op-veel-relatie tussen Groepsreis en Onkosten
            modelBuilder.Entity<Onkosten>()
                .HasOne(o => o.Groepsreis)
                .WithMany(g => g.OnkostenLijst)
                .HasForeignKey(o => o.GroepsreisId);

            // Eén-op-veel-relatie tussen CustomUser en Kind
            modelBuilder.Entity<Kind>()
                .HasOne(k => k.Ouder)
                .WithMany(u => u.Kinderen)
                .HasForeignKey(k => k.PersoonId);

            // Eén-op-één-relatie tussen CustomUser en Monitor
            modelBuilder.Entity<Models.Monitor>()
                .HasOne(m => m.Persoon)
                .WithOne(u => u.Monitor)
                .HasForeignKey<Models.Monitor>(m => m.PersoonId);

            // Verwijder de unieke index op PersoonId
            modelBuilder.Entity<Models.Monitor>()
                .HasIndex(m => m.PersoonId)
                .IsUnique(false);

            // Voeg een samengestelde unieke index toe op PersoonId en GroepsreisDetailsId
            modelBuilder.Entity<Models.Monitor>()
                .HasIndex(m => new { m.PersoonId, m.GroepsreisDetailsId })
                .IsUnique(true);

            // Veel-op-veel-relatie tussen CustomUser en Opleiding
            modelBuilder.Entity<CustomUser>()
                .HasMany(c => c.Opleidingen)
                .WithMany(o => o.Personen)
                .UsingEntity(j => j.ToTable("OpleidingPersoon"));

            // Configuratie voor de Wachtlijst
            modelBuilder.Entity<Wachtlijst>()
                .HasOne(w => w.Groepsreis)
                .WithMany(g => g.Wachtlijst)
                .HasForeignKey(w => w.GroepsreisId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Wachtlijst>()
                .HasOne(w => w.Kind)
                .WithMany(k => k.Wachtlijst)
                .HasForeignKey(w => w.KindId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

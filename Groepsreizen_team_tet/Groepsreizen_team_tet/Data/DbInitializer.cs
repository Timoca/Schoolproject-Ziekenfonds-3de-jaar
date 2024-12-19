namespace Groepsreizen_team_tet.Data
{
    public class DbInitializer
    {
        public static async Task Initialize(GroepsreizenContext context, UserManager<CustomUser> userManager, RoleManager<CustomRole> roleManager)
        {
            // Zorg dat de database is aangemaakt
            context.Database.EnsureCreated();

            Console.WriteLine("Seeding database");

            // Definieer de gewenste rollen
            string[] roleNames = { "Gebruiker", "Deelnemer", "Monitor", "Hoofdmonitor", "Verantwoordelijke", "Beheerder" };

            foreach (var roleName in roleNames)
            {
                // Controleer of de rol al bestaat
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var role = new CustomRole { Name = roleName };
                    await roleManager.CreateAsync(role);
                }
            }

            #region Seed Gebruikers
            // Seed gebruikers voor elke rol
            var users = new[]
            {
                new { UserName = "gebruiker@gr.be", Role = "Gebruiker", Straat = "Kleinhoefstraat", Huisnummer = "4" },
                new { UserName = "deelnemer@gr.be", Role = "Deelnemer", Straat = "Veldweg", Huisnummer = "28" },
                new { UserName = "monitor@gr.be", Role = "Monitor", Straat = "Dorpstraat", Huisnummer = "18" },
                new { UserName = "monitor2@gr.be", Role = "Monitor", Straat = "Dorpstraat", Huisnummer = "19" },
                new { UserName = "monitor3@gr.be", Role = "Monitor", Straat = "Dorpstraat", Huisnummer = "20" },
                new { UserName = "monitor4@gr.be", Role = "Monitor", Straat = "Dorpstraat", Huisnummer = "21" },
                new { UserName = "monitor5@gr.be", Role = "Monitor", Straat = "Dorpstraat", Huisnummer = "22" },
                new { UserName = "hoofdmonitor@gr.be", Role = "Hoofdmonitor", Straat = "Leon Dumortierstraat", Huisnummer = "67" },
                new { UserName = "verantwoordelijke@gr.be", Role = "Verantwoordelijke", Straat = "Stationstraat", Huisnummer = "9" },
                new { UserName = "admin@admin.com", Role = "Beheerder", Straat = "Antwerpsesteenweg", Huisnummer = "42" }
            };

            foreach (var user in users)
            {
                if (await userManager.FindByNameAsync(user.UserName) == null)
                {
                    var newUser = new CustomUser
                    {
                        UserName = user.UserName,
                        Email = user.UserName,
                        Naam = user.Role,
                        Voornaam = "A",
                        Straat = user.Straat,
                        Huisnummer = user.Huisnummer,
                        Gemeente = "Stad",
                        Postcode = "1000",
                        Geboortedatum = new DateTime(1990, 1, 1),
                        PhoneNumber = "0123456789",
                        Huisdokter = "geen",
                        ContractNummer = "CN" + user.Role,
                        RekeningNummer = "BE12345678901234",
                        IsActief = true
                    };

                    var result = await userManager.CreateAsync(newUser, "Password@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(newUser, user.Role);
                    }
                }
            }
            #endregion

            #region Seed Bestemmingen
            // Seed Bestemmingen
            if (!context.Bestemmingen.Any())
            {
                Console.WriteLine("Seeding Bestemmingen");
                var bestemmingen = new[]
                {
                    new Bestemming { Code = "BE01", Naam = "Gent", Beschrijving = "Stad in België", MinLeeftijd = 10, MaxLeeftijd = 12 },
                    new Bestemming { Code = "FR02", Naam = "Parijs", Beschrijving = "Hoofdstad van Frankrijk", MinLeeftijd = 13, MaxLeeftijd = 15 },
                    new Bestemming { Code = "IT03", Naam = "Rome", Beschrijving = "Hoofdstad van Italië", MinLeeftijd = 16, MaxLeeftijd = 18 }
                };

                foreach (var bestemming in bestemmingen)
                {
                    context.Bestemmingen.Add(bestemming);
                }

                await context.SaveChangesAsync();
            }
            #endregion

            #region Seed Bestemmingen
            // Haal de bestemmingen op voor toewijzing aan groepsreizen
            var allBestemmingen = context.Bestemmingen.ToList();
            Console.WriteLine($"Aantal bestemmingen: {allBestemmingen.Count}");

            // Seed Groepsreizen als er genoeg bestemmingen zijn
            if (allBestemmingen.Count >= 3 && !context.Groepsreizen.Any())
            {
                Console.WriteLine("Seeding Groepsreizen");
                var groepsreizen = new[]
                {
                    new Groepsreis { Begindatum = new DateTime(2025, 7, 10), Einddatum = new DateTime(2025, 7, 20), Prijs = 130, BestemmingId = allBestemmingen[1].Id, Bestemming = allBestemmingen[1] },
                    new Groepsreis { Begindatum = new DateTime(2025, 9, 15), Einddatum = new DateTime(2025, 9, 25), Prijs = 150, BestemmingId = allBestemmingen[2].Id, Bestemming = allBestemmingen[2] },
                    new Groepsreis { Begindatum = new DateTime(2025, 10, 1), Einddatum = new DateTime(2025, 10, 10), Prijs = 200, BestemmingId = allBestemmingen[0].Id, Bestemming = allBestemmingen[0] }
                };

                foreach (var reis in groepsreizen)
                {
                    context.Groepsreizen.Add(reis);
                }

                await context.SaveChangesAsync();
            }
            else
            {
                Console.WriteLine("LOG: Niet genoeg bestemmingen om aan groepsreizen toe te wijzen.");
            }
            #endregion

            #region Seed Activiteiten
            // Seed Activiteiten
            if (!context.Activiteiten.Any())
            {
                Console.WriteLine("LOG: Seeding Activiteiten");
                var activiteiten = new[]
                {
                    new Activiteit { Naam = "Fietsen", Beschrijving = "Een fietstocht door het platteland" },
                    new Activiteit { Naam = "Museumbezoek", Beschrijving = "Bezoek aan een lokaal museum" },
                    new Activiteit { Naam = "Stranddag", Beschrijving = "Een dagje naar het strand" }
                };

                foreach (var activiteit in activiteiten)
                {
                    context.Activiteiten.Add(activiteit);
                }

                await context.SaveChangesAsync();
            }
            #endregion

            #region Seed Programma's
            // Seed Programma's (koppeling tussen Activiteiten en Groepsreizen)
            if (!context.Programmas.Any())
            {
                Console.WriteLine("LOG: Programma's Seeden");
                var activiteiten = context.Activiteiten.ToList();
                var groepsreizen = context.Groepsreizen.ToList();

                var programmas = new[]
                {
                    new Programma { ActiviteitId = activiteiten[0].Id, GroepsreisId = groepsreizen[0].Id },
                    new Programma { ActiviteitId = activiteiten[1].Id, GroepsreisId = groepsreizen[1].Id },
                    new Programma { ActiviteitId = activiteiten[2].Id, GroepsreisId = groepsreizen[2].Id }
                };

                foreach (var programma in programmas)
                {
                    context.Programmas.Add(programma);
                }

                await context.SaveChangesAsync();
            }
            #endregion

            #region Seed Monitoren
            // Seed Monitoren
            if (!context.Monitoren.Any())
            {
                Console.WriteLine("LOG: Monitoren Seeden");

                // Haal alle gebruikers op met de rol 'Monitor' en 'Hoofdmonitor'
                var monitorUsers = await userManager.GetUsersInRoleAsync("Monitor");
                var hoofdMonitorUsers = await userManager.GetUsersInRoleAsync("Hoofdmonitor");

                // Combineer de lijsten en vermijd duplicaten
                var allMonitorUsers = monitorUsers.Union(hoofdMonitorUsers).ToList();

                // Haal alle groepsreizen op
                var groepsreizen = context.Groepsreizen.ToList();

                foreach (var user in allMonitorUsers)
                {
                    // Controleer of er al een Monitor entiteit bestaat voor deze gebruiker
                    if (!context.Monitoren.Any(m => m.PersoonId == user.Id))
                    {
                        // Optioneel: Wijs een groepsreis toe als er beschikbaar zijn
                        var groepsreis = groepsreizen.FirstOrDefault(); // Hier kun je logica toevoegen om een specifieke groepsreis toe te wijzen

                        var isHoofdMonitor = await userManager.IsInRoleAsync(user, "Hoofdmonitor");

                        var monitor = new Models.Monitor
                        {
                            PersoonId = user.Id,
                            GroepsreisDetailsId = groepsreis?.Id, // Toewijzen als groepsreis beschikbaar is
                            IsHoofdMonitor = isHoofdMonitor ? 1 : 0 // Stel in op 1 als hoofdmonitor, anders 0
                        };

                        context.Monitoren.Add(monitor);
                        Console.WriteLine($"Seeding Monitor: {user.UserName} - IsHoofdMonitor: {monitor.IsHoofdMonitor}");
                    }
                }

                await context.SaveChangesAsync();
            }
            #endregion

            #region Seed Opleidingen
            // Seed Opleidingen
    //        if (!context.Opleidingen.Any())
    //        {
    //            Console.WriteLine("Seeding Opleidingen");

    //            var opleidingen = new[]
    //            {
    //                new Opleiding
    //                {
    //                    Naam = "EHBO Basis",
    //                    Beschrijving = "In deze cursus worden de basistechnieken van de EHBO aangeleerd. Er is geen voorkennis vereist.",
    //                    Begindatum = new DateTime(2025, 2, 1),
    //                    Einddatum = new DateTime(2025, 2, 3),
    //                    AantalPlaatsen = 20,
    //                    OpleidingVereistId = null,
    //                    FotoUrl = "https://images.unsplash.com/photo-1563260324-5ebeedc8af7c?q=80&w=2274&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
    //                    Locatie = "EHBO-centrum Geel"
    //                },
    //                new Opleiding
    //                {
    //                    Naam = "Leiderschapscursus Basis",
    //                    Beschrijving = "In deze cursus leer je tips & tricks om jezelf als een goede leider en monitor neer te zetten. Je leert hoe je een groep als een echte leider kan managen en conflicten ontmijnt.",
    //                    Begindatum = new DateTime(2025, 3, 1),
    //                    Einddatum = new DateTime(2025, 3, 5),
    //                    AantalPlaatsen = 15,
    //                    OpleidingVereistId = null,
    //                    FotoUrl = "https://images.unsplash.com/photo-1470753323753-3f8091bb0232?q=80&w=2340&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
    //                    Locatie = "Ontmoetingscentrum DRZ"
    //                },
    //                new Opleiding
    //                {
    //                    Naam = "EHBO Gevorderd",
    //                    Beschrijving = "In deze cursus worden de gevorderde technieken aangeleerd. Je moet hiervoor eerst de cursus EHBO Basis hebben afgerond.",
    //                    Begindatum = new DateTime(2025, 4, 1),
    //                    Einddatum = new DateTime(2025, 4, 3),
    //                    AantalPlaatsen = 15,
    //                    OpleidingVereistId = context.Opleidingen.FirstOrDefault(o => o.Naam == "EHBO Basis")?.Id,
    //                    FotoUrl = "https://images.unsplash.com/photo-1563260324-5ebeedc8af7c?q=80&w=2274&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
    //                    Locatie = "EHBO-centrum Geel"
    //                },
    //                new Opleiding
    //                {
    //                    Naam = "Leiderschapscursus Gevorderd",
    //                    Beschrijving = "In deze cursus bouw je verder op de skills die je geleerd hebt tijdens de basiscursus. Je leert hoe je met gedragsproblemen moet omgaan.",
    //                    Begindatum = new DateTime(2025, 5, 1),
    //                    Einddatum = new DateTime(2025, 5, 6),
    //                    AantalPlaatsen = 10,
    //                    OpleidingVereistId = context.Opleidingen.FirstOrDefault(o => o.Naam == "Leiderschapscursus Beginners")?.Id,
    //                    FotoUrl = "https://images.unsplash.com/photo-1470753323753-3f8091bb0232?q=80&w=2340&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
    //                    Locatie = "Ontmoetingscentrum DRZ"
    //                },
    //                new Opleiding
    //                {
    //                    Naam = "Basisopleiding tot monitor",
    //                    Beschrijving = "Iedereen die als monitor mee op groepsreis wil gaan, moet deze opleiding succesvol afronden.",
    //                    Begindatum = new DateTime(2025, 5, 1),
    //                    Einddatum = new DateTime(2025, 5, 6),
    //                    AantalPlaatsen = 15,
    //                    OpleidingVereistId = null,
    //                    FotoUrl = "https://images.unsplash.com/photo-1526232761682-d26e03ac148e?q=80&w=2258&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
    //                    Locatie = "Ontmoetingscentrum DRZ"
    //                },
    //                new Opleiding
    //                {
    //                    Naam = "Testcursus in het verleden",
    //                    Beschrijving = "Deze cursus dient als test om te kijken of opleidingen in het verleden wel of niet worden weergegeven.",
    //                    Begindatum = new DateTime(2020, 5, 1),
    //                    Einddatum = new DateTime(2020, 5, 1),
    //                    AantalPlaatsen = 15,
    //                    OpleidingVereistId = null,
    //                    FotoUrl = null,
    //                    Locatie = "Het Verleden"
    //                },
    //                 new Opleiding
    //                {
    //                    Naam = "Testcursus voor 1 persoon",
    //                    Beschrijving = "Cursus om te testen op beschikbaarheid en volzet.",
    //                    Begindatum = new DateTime(2025, 5, 1),
    //                    Einddatum = new DateTime(2025, 5, 1),
    //                    AantalPlaatsen = 1,
    //                    OpleidingVereistId = null,
    //                    FotoUrl = null,
    //                    Locatie = "Online"
    //                },
    //                  new Opleiding
    //                {
    //                    Naam = "Testcursus voor 2 personen",
    //                    Beschrijving = "Deze cursus test de beschikbaarheid op basis van twee personen.",
    //                    Begindatum = new DateTime(2025, 5, 1),
    //                    Einddatum = new DateTime(2025, 5, 1),
    //                    AantalPlaatsen = 2,
    //                    OpleidingVereistId = null,
    //                    FotoUrl = null,
    //                    Locatie = "Online"
    //                },
    //                   new Opleiding
    //                {
    //                    Naam = "Testcursus ver in het verleden",
    //                    Beschrijving = "Heel lang geleden was er eens een cursus.",
    //                    Begindatum = new DateTime(2019, 5, 1),
    //                    Einddatum = new DateTime(2019, 5, 1),
    //                    AantalPlaatsen = 15,
    //                    OpleidingVereistId = null,
    //                    FotoUrl = null,
    //                    Locatie = "Archief"
    //                },
    //                    new Opleiding
    //                {
    //                    Naam = "Testcursus Basis",
    //                    Beschrijving = "Deze cursus dient lay-up om te testen of een monitor zich kan inschrijven zonder basiscursus.",
    //                    Begindatum = new DateTime(2025, 5, 1),
    //                    Einddatum = new DateTime(2025, 5, 1),
    //                    AantalPlaatsen = 15,
    //                    OpleidingVereistId = null,
    //                    FotoUrl = null,
    //                    Locatie = "In de toekomst"
    //                },
    //                    new Opleiding
    //                {
    //                    Naam = "Testcursus Gevorderd",
    //                    Beschrijving = "Deze cursus dient als test om te kijken of een monitor zich hiervoor kan inschrijven zonder eerst de basiscursus gedaan te hebben.",
    //                    Begindatum = new DateTime(2025, 5, 1),
    //                    Einddatum = new DateTime(2025, 5, 1),
    //                    AantalPlaatsen = 15,
    //                    OpleidingVereistId = context.Opleidingen.FirstOrDefault(o => o.Naam == "Testcursus Basis")?.Id,
    //                    FotoUrl = null,
    //                    Locatie = "In de toekomst"
    //                },
    //};

    //            foreach (var opleiding in opleidingen)
    //            {
    //                context.Opleidingen.Add(opleiding);
    //            }

    //            await context.SaveChangesAsync();
    //        }
            #endregion

            #region Seed Deelnemers
            // Seed Deelnemers voor elke groepsreis
            if (!context.Deelnemers.Any())
            {
                Console.WriteLine("LOG: Seeding Deelnemers");

                var groepsreizen = context.Groepsreizen.ToList();
                var kinderen = context.Kinderen.ToList(); // Controleer of kinderen zijn geseed
                var deelnemers = new List<Deelnemer>();

                foreach (var groepsreis in groepsreizen)
                {
                    // Voeg minstens 5 deelnemers toe per groepsreis
                    for (int i = 0; i < 5; i++)
                    {
                        // Zorg ervoor dat er genoeg kinderen beschikbaar zijn
                        if (i >= kinderen.Count) break;

                        var kind = kinderen[i];

                        var deelnemer = new Deelnemer
                        {
                            KindId = kind.Id,
                            GroepsreisDetailsId = groepsreis.Id,
                            Opmerkingen = $"Opmerking voor kind {kind.Voornaam}",
                            ReviewScore = null,
                            Review = null
                        };

                        deelnemers.Add(deelnemer);
                    }
                }

                context.Deelnemers.AddRange(deelnemers);
                await context.SaveChangesAsync();
            }
            #endregion

            #region Seed Kinderen
            // Seed Kinderen
            if (!context.Kinderen.Any())
            {
                Console.WriteLine("LOG: Seeding Kinderen");

                var ouders = context.CustomUsers.Where(u => u.UserName.Contains("ouder")).ToList();

                // Voeg een standaard ouder toe als er geen ouders in de database aanwezig zijn
                if (!ouders.Any())
                {
                    Console.WriteLine("LOG: Geen ouders beschikbaar. Voeg een standaard ouder toe.");

                    var defaultOuder = new CustomUser
                    {
                        UserName = "default@ouder.com",
                        Email = "default@ouder.com",
                        Naam = "Default",
                        Voornaam = "Ouder",
                        Straat = "Straat",
                        Huisnummer = "1",
                        Gemeente = "Stad",
                        Postcode = "1000",
                        Geboortedatum = new DateTime(1980, 1, 1),
                        Huisdokter = "Geen",
                        ContractNummer = "CN000",
                        RekeningNummer = "BE12345678901234",
                        IsActief = true
                    };

                    var result = await userManager.CreateAsync(defaultOuder, "Password@123");
                    if (result.Succeeded)
                    {
                        ouders.Add(defaultOuder);
                        await userManager.AddToRoleAsync(defaultOuder, "Gebruiker");
                    }
                }

                var kinderen = new List<Kind>();

                for (int i = 1; i <= 10; i++)
                {
                    var ouder = ouders[i % ouders.Count]; // Gebruik beschikbare ouders, inclusief de standaard ouder

                    kinderen.Add(new Kind
                    {
                        Naam = $"Kind{i}",
                        Voornaam = $"Voornaam{i}",
                        Geboortedatum = new DateTime(2010, 1, i),
                        Allergieën = i % 2 == 0 ? "Geen" : "Pinda-allergie",
                        Medicatie = i % 3 == 0 ? "Paracetamol" : "Geen",
                        PersoonId = ouder.Id
                    });
                }

                context.Kinderen.AddRange(kinderen);
                await context.SaveChangesAsync();
            }
            #endregion
        }
    }
}

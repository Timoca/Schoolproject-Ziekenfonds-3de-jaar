using Groepsreizen_team_tet.Services;
using Groepsreizen_team_tet.Filters;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<GroepsreizenContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddRazorPages();
builder.Services.AddLogging();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Swagger registreren
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Antiforgery
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN"; // Header configureren voor AJAX-verzoeken
});

//NewtonJSonSoft registreren
builder.Services.AddControllersWithViews().AddNewtonsoftJson(Options => Options
    .SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

// Voeg Identity toe met het CustomUser model
builder.Services.AddIdentity<CustomUser, CustomRole>()
    .AddEntityFrameworkStores<GroepsreizenContext>()
    .AddDefaultTokenProviders();

// Voeg sessie-ondersteuning toe
builder.Services.AddDistributedMemoryCache(); // Dit is nodig voor sessiebeheer. Sessiegegevens worden in het geheugen opgeslagen.
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Nodig voor privacyregelgeving (GDPR)
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton<EmailService>();


// Stel de standaardcultuur in op Nederlands (België)
var defaultCulture = new CultureInfo("nl-BE");
CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

// Voeg services toe aan de container.
builder.Services.AddControllersWithViews(options =>
{
    // Voeg de BreadcrumbActionFilter toe als een service
    options.Filters.Add<BreadcrumbActionFilter>();
});

// Registreer IUrlHelperFactory
builder.Services.AddSingleton<IUrlHelperFactory, UrlHelperFactory>();

var app = builder.Build();

// Configureer verzoeklokalisatie
var supportedCultures = new[] { defaultCulture };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(defaultCulture),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

// Swagger middleware koppelen aan app
app.UseSwagger();

// SwaggerUI instellen met juiste JSON endpoint 
app.UseSwaggerUI(x =>
{
    x.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); 
app.UseAuthorization();

// Voeg UseSession toe om sessies in de app te kunnen gebruiken. Wordt toegevoegd vóór ControllerRoute zodat controllers toegang hebben tot sessies.
app.UseSession();

// Seed de database met rollen en gebruikers
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<GroepsreizenContext>();
    var userManager = services.GetRequiredService<UserManager<CustomUser>>();
    var roleManager = services.GetRequiredService<RoleManager<CustomRole>>();
    await DbInitializer.Initialize(context, userManager, roleManager);
}

app.MapRazorPages();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();

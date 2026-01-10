using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Radzen;
using System.Text;
using TruckScalesWeb.Components;
using TruckScalesWeb.DAO;
using TruckScalesWeb.Models;
using TruckScalesWeb.ScaleConnection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
      .AddInteractiveServerComponents().AddHubOptions(options => options.MaximumReceiveMessageSize = 10 * 1024 * 1024);

builder.Services.AddControllers();
builder.Services.AddRadzenComponents();


builder.Services.AddDbContextFactory<SqlContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IDataService, DataService>();
builder.Services.AddMemoryCache();


builder.Services.AddSingleton<ScaleService>();
builder.Services.AddSignalR();

// Для Windows Authentication в Server-Side Blazor
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate();
builder.Services.AddAuthorization();

var app = builder.Build();

 CreateBaseSeed(app);

var forwardingOptions = new ForwardedHeadersOptions()
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
};
forwardingOptions.KnownNetworks.Clear();
forwardingOptions.KnownProxies.Clear();

app.UseForwardedHeaders(forwardingOptions);
    

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication(); 
app.UseAuthorization();  
app.UseAntiforgery();


app.MapControllers();

app.MapHub<ScaleHub>("/api/scale");
app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

app.Run();


static void CreateBaseSeed(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;

        try
        {
            var context = services.GetRequiredService<SqlContext>();
            if (!context.Roles.Any())
            {
                context.Roles.AddRange(
                    new Role { Name = "Оператор" },
                    new Role { Name = "Старший оператор" },
                    new Role { Name = "Администратор"}
                );
                context.SaveChanges();
            }

            if (!context.Users.Any())
            {
                var mainUser = new User
                {
                    Account = "DESKTOP-LT9RR72\\Леново",
                    DisplayName = "Шевченко А.К.",
                    IsActive = true,
                    LastLogin = DateTime.UtcNow
                };
                mainUser.Roles = context.Roles.ToList();
                context.Users.Add(mainUser);
                var secondUser = new User
                {
                    Account = "WIN-10QOTERAGNC\\Administrator",
                    DisplayName = "Шевченко А.К.",
                    IsActive = true,
                    LastLogin = DateTime.UtcNow
                };
                secondUser.Roles = context.Roles.ToList();
                context.Users.Add(secondUser);
                
                context.SaveChanges();
            }
            Country rf = null, be = null;
            if (!context.Countries.Any())
            {
                rf = new Country
                {
                    ShortName = "rus",
                    FullName = "Россия",
                    Sorter = 0
                };
                context.Countries.Add(rf);
                be = new Country
                {
                    ShortName = "be",
                    FullName = "Беларусь",
                    Sorter = 1
                };
                context.Countries.Add(be);
                context.SaveChanges();
            }
            CarType cargo = null, truck = null, trailer = null;
            if (!context.CarTypes.Any())
            {
                cargo = new CarType {Name = "Грузовик", CanBePrimary = true, Sorter = 0 };
                truck = new CarType {Name = "Тягач", CanBePrimary = true, Sorter = 1 };
                var tractor = new CarType {Name = "Трактор", CanBePrimary = true, Sorter = 2 };
                trailer = new CarType {Name = "Полуприцеп", CanBeSecondary = true, Sorter = 3 };
                var liquid = new CarType {Name = "Бочка", CanBePrimary = true, Sorter = 4 };
                context.CarTypes.AddRange(cargo, truck, tractor, trailer, liquid);
                context.SaveChanges();
            }
            if (!context.Cars.Any())
            {
                var car1 = new Car { CarType = cargo, Country = rf, GosNumber = "Y404TO34", Model = "Toyota Corolla", IsActive=true };
                var car2 = new Car { CarType = truck, Country = be, GosNumber = "Y030XM34", Model = "Santa Fe", IsActive=true };
                var car3 = new Car { CarType = trailer, Country = rf, GosNumber = "A111AA34", Model = "Test", IsActive = true };
                var car4 = new Car { CarType = trailer, Country = rf, GosNumber = "B222BB34", Model = "Test", IsActive = true };
                context.Cars.AddRange(car1, car2, car3, car4);
                context.SaveChanges();
            }
            if (!context.Materials.Any())
            {
                var material1 = new Material { Name = "Металлолом", CanBeTransit = true, IsActive = true, Sorter = 0 };
                var material2 = new Material { Name = "Песок", IsActive = true, Sorter = 1 };
                var material3 = new Material { Name = "Сырая нефть", CanBeSingle = true, IsActive = true, Sorter = 2 };
                context.Materials.AddRange(material1, material2, material3);
                context.SaveChanges();
            }
        }
        catch (Exception ex)
        {
        }
    }
}
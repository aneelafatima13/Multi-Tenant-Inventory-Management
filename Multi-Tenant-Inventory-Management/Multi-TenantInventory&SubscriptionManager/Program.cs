using Multi_TenantInventory_SubscriptionManager;

var builder = WebApplication.CreateBuilder(args);

// 1. Standard MVC & Razor Services
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// 2. HTTP Client for API Communication
builder.Services.AddHttpClient<ApiService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7286/");
});

// 3. Infrastructure Services
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Good for security
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// --- IMPORTANT: Session must come BEFORE Authorization and Endpoints ---
app.UseSession();

app.UseAuthorization();

// 4. Routing logic
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}");

app.MapRazorPages();

app.Run();
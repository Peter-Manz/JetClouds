using Azure.Core;
using System;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using JetCloud.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection.Repositories;

var builder = WebApplication.CreateBuilder(args);

//Start of AdaptedCode from https://stackoverflow.com/questions/69722872/asp-net-core-6-how-to-access-configuration-during-startup
ConfigurationManager configuration = builder.Configuration;
IWebHostEnvironment enviroment = builder.Environment;
//end of adapted code 

builder.Services.Configure<IISServerOptions>(options => options.MaxRequestBodySize = 104857600);

builder.Services.AddRazorPages();
builder.Services.AddMvc();
builder.Services.AddControllersWithViews();

//start of adapted Code from (msmbaldwin, 2023)
var kvUrl = new String(configuration["KeyVaultUrl"]);
var secretClient = new SecretClient(new Uri(kvUrl), new DefaultAzureCredential());
var sqlConnString = await secretClient.GetSecretAsync("AppDbContext");
var connectionString = sqlConnString.Value.Value;
//end of adapted Code 

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddDataProtection()
    .PersistKeysToDbContext<AppDbContext>();

builder.Services.AddScoped<IServiceProvider, ServiceProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseAuthentication();
app.MapRazorPages();
app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorPages();
});
app.Run();

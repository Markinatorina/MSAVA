using M_SAVA_DAL.Contexts;
using M_SAVA_DAL.Models;
using M_SAVA_DAL.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register main database context
builder.Services.AddDbContext<BaseDataContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MainDatabaseConnection")));

// Register repositories
builder.Services.AddScoped<IIdentifiableRepository<UserDB>, IdentifiableRepository<UserDB>>();
builder.Services.AddScoped<IIdentifiableRepository<JWTDB>, IdentifiableRepository<JWTDB>>();
builder.Services.AddScoped<IIdentifiableRepository<InviteCodeDB>, IdentifiableRepository<InviteCodeDB>>();
builder.Services.AddScoped<ISavedFileRepository, SavedFileRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

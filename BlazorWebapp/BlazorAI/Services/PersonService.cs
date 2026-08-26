using System.ComponentModel;
using BlazorAI.Domain.Context;
using BlazorAI.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorAI.Services;

public class PersonService(IDbContextFactory<AppDbContext> dbContext) : IPersonService
{
    public async Task<IEnumerable<Person>> GetAll()
    {
        using var context = dbContext.CreateDbContext();
        return await context.People.ToListAsync();
    }
}

[Description("Servicio para interactuar con personas")]
public interface IPersonService
{
    [Description("Obtiene listado de todas las personas")]
    Task<IEnumerable<Person>> GetAll();
}
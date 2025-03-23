using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OpenApi;
using WebRadioImpl;
namespace DbWebApi.Controllers;

public static class WebRadioEndpoints
{
    public static void MapWebRadioEndpoints (this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/WebRadio").WithTags(nameof(WebRadio));

        group.MapGet("/", async (MyDataContext db) =>
        {
            return await db.WebRadios.ToListAsync();
        })
        .WithName("GetAllWebRadios")
        .WithOpenApi();

        group.MapGet("/{id}", async Task<Results<Ok<WebRadio>, NotFound>> (int id, MyDataContext db) =>
        {
            return await db.WebRadios.AsNoTracking()
                .FirstOrDefaultAsync(model => model.Id == id)
                is WebRadio model
                    ? TypedResults.Ok(model)
                    : TypedResults.NotFound();
        })
        .WithName("GetWebRadioById")
        .WithOpenApi();

        group.MapPut("/{id}", async Task<Results<Ok, NotFound>> (int id, WebRadio webRadio, MyDataContext db) =>
        {
            var affected = await db.WebRadios
                .Where(model => model.Id == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(m => m.Id, webRadio.Id)
                    .SetProperty(m => m.Name, webRadio.Name)
                    .SetProperty(m => m.StreamingUrl, webRadio.StreamingUrl)
                    .SetProperty(m => m.Description, webRadio.Description)
                    );
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("UpdateWebRadio")
        .WithOpenApi();

        group.MapPost("/", async (WebRadio webRadio, MyDataContext db) =>
        {
            db.WebRadios.Add(webRadio);
            await db.SaveChangesAsync();
            return TypedResults.Created($"/api/WebRadio/{webRadio.Id}",webRadio);
        })
        .WithName("CreateWebRadio")
        .WithOpenApi();

        group.MapDelete("/{id}", async Task<Results<Ok, NotFound>> (int id, MyDataContext db) =>
        {
            var affected = await db.WebRadios
                .Where(model => model.Id == id)
                .ExecuteDeleteAsync();
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("DeleteWebRadio")
        .WithOpenApi();
    }
}

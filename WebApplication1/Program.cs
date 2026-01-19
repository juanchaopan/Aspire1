using Microsoft.EntityFrameworkCore;
using System.Data;
using WebApplication1;
using WebApplication1.Features.Users;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("db-1")));
builder.EnrichNpgsqlDbContext<AppDbContext>();

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapPost("/users", CreateUserEndpoint.CreateUser);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    using var scope = app.Services.CreateScope();
    var serviceProvider = scope.ServiceProvider;


    var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
    var db = serviceProvider.GetRequiredService<AppDbContext>();

    // Sanitize connection string for logs (remove password)
    string Sanitize(string cs)
    {
        if (string.IsNullOrEmpty(cs)) return "(empty)";
        try
        {
            var parts = cs.Split(';', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
            {
                var kv = parts[i].Split('=', 2);
                if (kv.Length == 2)
                {
                    var key = kv[0].Trim().ToLowerInvariant();
                    if (key is "password" or "pwd")
                        parts[i] = $"{kv[0]}=*****";
                }
            }
            return string.Join(';', parts);
        }
        catch
        {
            return "(unparsable)";
        }
    }

    try
    {
        var conn = db.Database.GetDbConnection();
        logger.LogInformation("Sanitized connection string: {Conn}", Sanitize(conn.ConnectionString ?? string.Empty));

        try
        {
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT current_user, current_schema(), current_setting('search_path')";
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                logger.LogInformation("DB user={User} current_schema={Schema} search_path={SearchPath}",
                    reader.IsDBNull(0) ? "(null)" : reader.GetString(0),
                    reader.IsDBNull(1) ? "(null)" : reader.GetString(1),
                    reader.IsDBNull(2) ? "(null)" : reader.GetString(2));
            }
            reader.Close();

            // Explicitly check for migrations history in public schema
            cmd.CommandText = "SELECT count(*) FROM public.\"__EFMigrationsHistory\"";
            try
            {
                var count = cmd.ExecuteScalar();
                logger.LogInformation("public.__EFMigrationsHistory count = {Count}", count ?? "(null)");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Querying public.__EFMigrationsHistory failed (table may not exist in public schema).");
            }
        }
        finally
        {
            if (db.Database.GetDbConnection().State == ConnectionState.Open)
                db.Database.GetDbConnection().Close();
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Pre-migration DB checks failed.");
    }

    try
    {
        logger.LogInformation("Applying EF Core migrations...");
        serviceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
        logger.LogInformation("Migrations applied.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database.Migrate() threw an exception while applying migrations.");
        throw;
    }


}

app.UseHttpsRedirection();

app.UseAuthorization();

app.Run();

using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using APBD6.DTOs;
using APBD6.Validators;
using FluentValidation;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddConnections();
builder.Services.AddValidatorsFromAssemblyContaining<CreateAnimalRequestValidator>();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/api/animals", (IConfiguration configuration, string? orderBy ) =>
{
    if (string.IsNullOrEmpty(orderBy))
    {
        orderBy = "name";
    }

    orderBy = orderBy.ToLower();
    
    var validOrderByValues = new[] { "name", "description", "category", "area" };
    if (!validOrderByValues.Contains(orderBy))
    {
        return Results.BadRequest();
    }
    
    
    var animals = new List<GetAllAnimalsResponse>();
    using (var sqlConnection = new SqlConnection(configuration.GetConnectionString("Default")))
    {
        var sqlCommand = new SqlCommand("SELECT * FROM Animal ORDER BY " + orderBy, sqlConnection);
        sqlCommand.Connection.Open();
        var reader = sqlCommand.ExecuteReader();

        while (reader.Read())
        {
            var idAnimal = reader.GetInt32(0);
            var name = reader.GetString(1);
            var description = !reader.IsDBNull(2) ? reader.GetString(2) : null;
            var category = reader.GetString(3);
            var area = reader.GetString(4);

            animals.Add(new GetAllAnimalsResponse(idAnimal, name, description, category, area));
        }

        return Results.Ok(animals);
    }
});

app.MapPost("/api/animals", (IConfiguration configuration, CreateAnimalsRequest request, IValidator<CreateAnimalsRequest> validator) =>
{
    var validation = validator.Validate(request);
    if (!validation.IsValid) return Results.ValidationProblem(validation.ToDictionary());
    
    using (var sqlConnection = new SqlConnection(configuration.GetConnectionString("Default")))
    {
        var sqlCommand = new SqlCommand("INSERT INTO Animal(Name, Description, Category, Area) VALUES (@n, @d, @c, @a) ", sqlConnection);
        sqlCommand.Parameters.AddWithValue("@n", request.Name);
        if (request.Description != null)
        {
            sqlCommand.Parameters.AddWithValue("@d", request.Description);
        }
        else
        {
            sqlCommand.Parameters.AddWithValue("@d", DBNull.Value);
        }
        sqlCommand.Parameters.AddWithValue("@c", request.Category);
        sqlCommand.Parameters.AddWithValue("@a", request.Area);
        sqlCommand.Connection.Open();

        sqlCommand.ExecuteNonQuery();
        return Results.Created("", null);
    }
});

app.MapPut("/api/animals/{id:int}", (IConfiguration configuration, int id, CreateAnimalsRequest request,
    IValidator<CreateAnimalsRequest> validator) =>
{
    var validation = validator.Validate(request);
    if (!validation.IsValid) return Results.ValidationProblem(validation.ToDictionary());
    
    using (var sqlConnection = new SqlConnection(configuration.GetConnectionString("Default")))
    {
        var sqlCommand = new SqlCommand("UPDATE Animal SET " +
                                        "Name = @n, " +
                                        "Description = @d, " +
                                        "Category = @c, " +
                                        "Area = @a " +
                                        "WHERE IdAnimal = @id", sqlConnection);
        sqlCommand.Parameters.AddWithValue("@n", request.Name);
        if (request.Description != null)
        {
            sqlCommand.Parameters.AddWithValue("@d", request.Description);
        }
        else
        {
            sqlCommand.Parameters.AddWithValue("@d", DBNull.Value);
        }
        sqlCommand.Parameters.AddWithValue("@c", request.Category);
        sqlCommand.Parameters.AddWithValue("@a", request.Area);
        sqlCommand.Parameters.AddWithValue("@id", id);
        sqlCommand.Connection.Open();

        sqlCommand.ExecuteNonQuery();
        var rowsAffected = sqlCommand.ExecuteNonQuery();
        return rowsAffected == 0 ? Results.NotFound() : Results.NoContent();
    }
});

app.MapDelete("/api/animals/{id:int}", (IConfiguration configuration, int id) =>
{
    
    using (var sqlConnection = new SqlConnection(configuration.GetConnectionString("Default")))
    {
        var sqlCommand = new SqlCommand("DELETE FROM Animal WHERE IdAnimal = @id", sqlConnection);
        sqlCommand.Parameters.AddWithValue("@id", id);
        sqlCommand.Connection.Open();
        var rowsAffected = sqlCommand.ExecuteNonQuery();
        return rowsAffected == 0 ? Results.NotFound() : Results.NoContent();
    }
});


app.Run();


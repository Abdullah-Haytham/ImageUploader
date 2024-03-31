using ImageUploader;
using Microsoft.AspNetCore.Builder;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseStaticFiles();


app.MapPost("/upload", async (HttpContext context) =>
{
    var form = await context.Request.ReadFormAsync();
    var file = form.Files.GetFile("file");
    var title = form["title"];

    if (file == null || file.Length == 0)
    {
        return Results.BadRequest("No file uploaded.");
    }
    if (title == "")
    {
        return Results.BadRequest("Title not Received");
    }

    var fileId = Guid.NewGuid().ToString();
    Directory.CreateDirectory("uploads");
    var filePath = Path.Combine("uploads", fileId + Path.GetExtension(file.FileName));
    using (var stream = new FileStream(filePath, FileMode.Create)) { await file.CopyToAsync(stream); }

    var imageData = new ImageInfo
    {
        Id = fileId,
        Title = title,
        Image = file.FileName
    };

    var imageJson = JsonSerializer.Serialize(imageData);
    var jsonPath = Path.Combine(app.Environment.ContentRootPath, "images.json");
    await File.WriteAllTextAsync(jsonPath, imageJson);

    return Results.Json(new { id = fileId });
});

app.MapGet("/uploads/{id}", async (string id, HttpContext context) => {
    var imageJsonPath = Path.Combine(app.Environment.ContentRootPath, "images.json");

    var imageJson = await File.ReadAllTextAsync(imageJsonPath);
    var imageData = JsonSerializer.Deserialize<ImageInfo>(imageJson);

    var imgExtension = Path.GetExtension(imageData.Image);
    var imgPath = Path.Combine("uploads", id + imgExtension);

    if (File.Exists(imgPath))
    {
        var image = File.OpenRead(imgPath);
        return Results.File(image, imgExtension);
    }
    return Results.NotFound("No image with this ID exists");
});

app.MapGet("picture/{id}", async (string id, HttpContext context) =>
{
    var imageJsonPath = Path.Combine(app.Environment.ContentRootPath, "images.json");
    if (!File.Exists(imageJsonPath))
    {
        return Results.NotFound("image does not exist");
    }

    var imageJson = await File.ReadAllTextAsync(imageJsonPath);
    var imageData = JsonSerializer.Deserialize<ImageInfo>(imageJson);

    var imgPath = $"/uploads/{id}";

    var htmlContent = $@"
        <!DOCTYPE html>
        <html lang='en'>
        <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <title>Picture</title>
        </head>
        <body>
            <h1>{imageData.Title}</h1>
            <img src='{imgPath}'>
        </body>
        </html>";
    return Results.Content(htmlContent, "text/html");
});

app.Run();


using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Postagens.Models;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

string mySqlConnection = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(opitions =>
                    opitions.UseMySql(mySqlConnection,
                    ServerVersion.AutoDetect(mySqlConnection)));

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "blog_minimal_API", Description = "Making the Pizzas you love", Version = "v1" });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "blog_minimal_API");
    });
}



// Método GET para tudo
app.MapGet("/post", async (AppDbContext DB) => await DB.Postagems.AsNoTracking().ToListAsync());

// metodo GET po id
app.MapGet("/post/{id:int:min(1)}", async (AppDbContext DB, int id) => await DB.Postagems.FindAsync(id));

//metodo get para as imagens
app.MapGet("/post/imagem/{id:int:min(1)}", async (AppDbContext DB, int id) =>
{
    var Post = await DB.Postagems.FindAsync(id);
    if (Post == null || Post.Imagem == null)
    {
        return Results.NotFound("Imagem não encontrada");
    }
    // Converte a imagem binária para um array de bytes
    byte[] imagemBytes = Post.Imagem;

    // Retorna a imagem como um arquivo para a resposta HTTP
    return Results.File(imagemBytes, "image/jpeg"); // Altere o tipo de conteúdo conforme necessário
});

//metodo POST
/*app.MapPost("/post", async (AppDbContext DB, Postagem postagem) =>
{
    await DB.Postagems.AddAsync(postagem);
    await DB.SaveChangesAsync();
    return Results.Created($"/post/{postagem.PostagemId}", postagem);
});*/
// Método POST
app.MapPost("/post", async (AppDbContext DB, string titulo, string mensagem,  IFormFile Arquivo) =>
{
    Postagem postagem = new Postagem()
    {
        Titulo = titulo,
        Mensagem = mensagem
    };
    // Verifica se há uma imagem enviada
    if (Arquivo != null && Arquivo.Length > 0)
    {
        // Lê o conteúdo do arquivo de imagem em um array de bytes
        using (var ms = new MemoryStream())
        {
            Arquivo.CopyTo(ms);
            postagem.Imagem = ms.ToArray();
        }
    }

    await DB.Postagems.AddAsync(postagem);
    await DB.SaveChangesAsync();
    return Results.Created($"/post/{postagem.PostagemId}", postagem);
});

//metodo DELETE
app.MapDelete("/post/{id:int:min(1)}", async (AppDbContext DB, int id) =>
{
    var postagem = await DB.Postagems.FindAsync(id);
    if(postagem is null)
    {
        return Results.NotFound("Post não encontrado");
    }
    DB.Postagems.Remove(postagem);
    DB.SaveChanges();
    return Results.Ok();
});

//PUT
bool PostagemExists(AppDbContext DB, int id)
{
    return DB.Postagems.Any(e => e.PostagemId == id);
}

app.MapPut("/post/image/{id:int:min(1)}", async (AppDbContext DB, int id, IFormFile arquivo) =>
{
    if (arquivo == null || arquivo.Length == 0)
    {
        return Results.BadRequest("Arquivo não enviado.");
    }

    var postagem = await DB.Postagems.FindAsync(id);
    if (postagem == null)
    {
        return Results.NotFound("Postagem não encontrada.");
    }

    using (var memoryStream = new System.IO.MemoryStream())
    {
        await arquivo.CopyToAsync(memoryStream);

        // Se a imagem for maior que um determinado tamanho, você pode querer rejeitá-la ou redimensioná-la aqui
        postagem.Imagem = memoryStream.ToArray();
    }

    DB.Entry(postagem).State = EntityState.Modified;
    try
    {
        await DB.SaveChangesAsync();
    }
    catch (DbUpdateConcurrencyException)
    {
        if (!PostagemExists(DB, id))
        {
            return Results.NotFound();
        }
        else
        {
            throw;
        }
    }
    return Results.NoContent(); // Ou retorne um status 200 OK com alguma informação se preferir
});

app.Run();

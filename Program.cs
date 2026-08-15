using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. ПОДКЛЮЧАЕМ БАЗУ ДАННЫХ SQLite
// Файл базы данных "tickets.db" создастся сам прямо в папке с проектом
builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseSqlite("Data Source=tickets.db"));

var app = builder.Build();

// Автоматически создаем базу данных при старте приложения, если её еще нет
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// 2. СТРАНИЦА ПОЛЬЗОВАТЕЛЯ (Главная "/") - Форма отправки заявки
app.MapGet("/", async (HttpContext context) =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.WriteAsync(@"
        <style>body { font-family: sans-serif; max-width: 400px; margin: 50px auto; padding: 20px; line-height: 1.6; }</style>
        <h2>Подать заявку сисадмину 🛠️</h2>
        <form action='/send' method='POST'>
            <label>Ваше имя:</label><br>
            <input type='text' name='employee' required style='width:100%; margin-bottom:10px;'><br>
            <label>Что сломалось:</label><br>
            <textarea name='description' required style='width:100%; height:100px; margin-bottom:10px;'></textarea><br>
            <button type='submit' style='padding: 10px 20px; background: #007bff; color: white; border: none; cursor: pointer;'>Отправить</button>
        </form>
    ");
});

// ОБРАБОТЧИК ФОРМЫ (Прием данных из формы и запись в базу)
app.MapPost("/send", async (HttpContext context, AppDbContext db) =>
{
    var form = await context.Request.ReadFormAsync();
    
    var ticket = new Ticket
    {
        EmployeeName = form["employee"],
        Description = form["description"]
    };

    db.Tickets.Add(ticket);
    await db.SaveChangesAsync();

    context.Response.ContentType = "text/html; charset=utf-8";
    await context.Response.WriteAsync("<h3>Заявка принята! Сисадмин уже идет. 🚀</h3><a href='/'>Назад</a>");
});

// 3. СТРАНИЦА АДМИНА ("/admin") - Список активных заявок
app.MapGet("/admin", async (HttpContext context, AppDbContext db) => 
{
    context.Response.ContentType = "text/html; charset=utf-8";
    
    // Берем из базы только открытые заявки
    var activeTickets = await db.Tickets.Where(t => !t.IsClosed).ToListAsync();

    var html = "<style>body { font-family: sans-serif; max-width: 600px; margin: 50px auto; } table { width:100%; border-collapse:collapse; } th, td { padding: 10px; border: 1px solid #ddd; text-align: left; }</style>";
    html += "<h2>Активные заявки 🖥️</h2>";
    html += "<table><tr><th>Кто</th><th>Проблема</th><th>Действие</th></tr>";

    foreach (var ticket in activeTickets)
    {
        html += $"<tr>" +
                $"<td>{ticket.EmployeeName}</td>" +
                $"<td>{ticket.Description}</td>" +
                $"<td><form action='/close/{ticket.Id}' method='POST'><button type='submit' style='background:red; color:white; border:none; padding:5px 10px; cursor:pointer;'>Выполнено</button></form></td>" +
                $"</tr>";
    }
    html += "</table>";

    if (!activeTickets.Any()) html += "<p>Все проблемы решены! Кофе-брейк ☕</p>";

    await context.Response.WriteAsync(html);
});

// ОБРАБОТЧИК ЗАКРЫТИЯ ЗАЯВКИ АДМИНОМ
app.MapPost("/close/{id:int}", async (int id, AppDbContext db, HttpContext context) =>
{
    var ticket = await db.Tickets.FindAsync(id);
    if (ticket != null)
    {
        ticket.IsClosed = true; // Закрываем заявку
        await db.SaveChangesAsync();
    }
    context.Response.Redirect("/admin"); // Перенаправляем обратно на панель админа
});

app.Run();

// 4. ОПИСАНИЕ СТРУКТУРЫ ДАННЫХ И БАЗЫ
public class Ticket
{
    public int Id { get; set; }
    public string EmployeeName { get; set; } = "";
    public string Description { get; set; } = "";
    public bool IsClosed { get; set; } = false;
}

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Ticket> Tickets => Set<Ticket>();
}

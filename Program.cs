using MvpProject;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// ==========================================
// БАЗА ДАННЫХ (в памяти)
// ==========================================

// Пустой список для заявок
var tickets = new List<Ticket>();

// Список возможных проблем (для выпадающего списка)
var problemOptions = new List<string>
{
    "Не работает принтер",
    "Не включается компьютер",
    "Проблемы с Wi-Fi",
    "Зависает программа",
    "Не могу войти в систему",
    "Нужна новая мышь",
    "Сломался монитор",
    "Другое (указать в комментарии)"
};

// ==========================================
// ГЛАВНАЯ СТРАНИЦА (список заявок)
// ==========================================

app.MapGet("/", () =>
{
    var html = @"
    <!DOCTYPE html>
    <html>
    <head>
        <meta charset='utf-8' />
        <title>Тикет-трекер</title>
    </head>
    <body>
        <h1>Список заявок</h1>
        <ul>";

    // Если заявок нет, показываем сообщение
    if (tickets.Count == 0)
    {
        html += "<li><em>Пока нет заявок. Создайте первую!</em></li>";
    }
    else
    {
        foreach (var ticket in tickets)
        {
            html += $@"
            <li>
                <strong>{ticket.Name} {ticket.SecondName}</strong><br/>
                Проблема: {ticket.Problem}<br/>";

            if (!string.IsNullOrEmpty(ticket.Comment))
            {
                html += $"Описание: {ticket.Comment}<br/>";
            }

            html += $"</li>";
        }
    }

    html += @"
        </ul>
        <a href='/create'>➕ Создать заявку</a>
    </body>
    </html>";

    return Results.Content(html, "text/html; charset=utf-8");
});

// ==========================================
// ФОРМА СОЗДАНИЯ ЗАЯВКИ
// ==========================================

app.MapGet("/create", () =>
{
    // Генерируем HTML-код для выпадающего списка
    var optionsHtml = "";
    foreach (var problem in problemOptions)
    {
        optionsHtml += $"<option value='{problem}'>{problem}</option>";
    }

    var html = $@"
    <!DOCTYPE html>
    <html>
    <head>
        <meta charset='utf-8' />
        <title>Создать заявку</title>
        <style>
            body {{ font-family: Arial; margin: 20px; }}
            label {{ display: inline-block; width: 150px; }}
            input, select, textarea {{ margin-bottom: 10px; width: 300px; }}
            textarea {{ height: 100px; }}
            button {{ padding: 8px 20px; background: #4CAF50; color: white; border: none; cursor: pointer; }}
        </style>
    </head>
    <body>
        <h1>Создать заявку</h1>
        <form method='post' action='/create'>
            <label>Имя:</label> <input name='name' required/><br/>
            <label>Фамилия:</label> <input name='secondName' required/><br/>
            <label>Проблема:</label>
            <select name='problem' required>
                {optionsHtml}
            </select><br/>
            <label>Описание проблемы:</label>
            <textarea name='comment' placeholder='Опишите подробно...'></textarea><br/>
            <button type='submit'>Отправить</button>
        </form>
        <br/>
        <a href='/'>← На главную</a>
    </body>
    </html>";

    return Results.Content(html, "text/html; charset=utf-8");
});

// ==========================================
// ОБРАБОТКА СОЗДАНИЯ ЗАЯВКИ
// ==========================================

app.MapPost("/create", (string name, string secondName, string problem, string comment) =>
{
    // Создаём новую заявку
    var newTicket = new Ticket
    {
        Id = tickets.Count + 1,           // ID = следующий номер
        Name = name,
        SecondName = secondName,
        Problem = problem,
        Comment = comment ?? ""            // Если комментарий не заполнен — пустая строка
    };

    // Добавляем в список
    tickets.Add(newTicket);

    // Перенаправляем на главную
    return Results.Redirect("/");
});

// ==========================================
// ЗАПУСК ПРИЛОЖЕНИЯ
// ==========================================

app.Run();
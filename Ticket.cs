namespace MvpProject;

// Модель (чертеж) заявки
public class Ticket
{
    public int Id { get; set; }              // Номер заявки (уникальный)
    public string Name { get; set; }         // Имя клиента
    public string SecondName { get; set; }   // Фамилия клиента
    public string Problem { get; set; }      // Проблема (выбор из списка)
    public string Comment { get; set; }      // Подробное описание проблемы
}
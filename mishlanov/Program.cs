var builder = WebApplication.CreateBuilder();

builder.Services.AddCors();

var app = builder.Build();

app.UseCors(o => o
.AllowAnyOrigin()
.AllowAnyMethod()
.AllowAnyHeader());


List<Order> repo =
[
    new( 1,new (2023,06,06),"���������","DEXP Aquilon O286","�������� ��������","�������", "�������", "�����","89210563128","� �������� �������"),
    new( 2,new (2023,05,05),"���������","DEXP Atlas H388","�������� ��������","����", "��������", "�����������","89210563128","� �������� �������"),
    new( 3,new (2023,07,07),"�������","MSI GF76 Katana 11UC-879XRU ������","�����������","������ ", "������", "�������������","89210563128","������ � ������"),
    new( 4,new (2023,08,02),"�������","MSI Modern 15 B12M-211RU ������","�����������","����", "��������", "�����������","89210563128","����� ������"),
    new( 5,new (2023,08,02),"�������","HP LaserJet Pro M404dn","��������� ����������","������", "������", "�������������","89210563128","����� ������"),
];

var order = repo.FirstOrDefault(x => x.Number == 3);
if (order != null)
{
    order.EndDate = new DateOnly(2023, 01, 01);
}

string message = "";

app.MapGet("orders", (int param = 0) =>
{
    string buffer = message;
    message = "";
    if (param != 0)
        return new { repo = repo.FindAll(x => x.Number == param), message = buffer };
    return new { repo, message = buffer };
});

app.MapGet("create", ([AsParameters] Order dto) =>
{
    if (dto.Number <= 0)
        return Results.BadRequest();

    if (repo.Any(x => x.Number == dto.Number))
        return Results.BadRequest();

    repo.Add(dto);
    return Results.Ok();
});

app.MapGet("update", ([AsParameters] UpdateOrder dto) =>
{
    var o = repo.Find(x => x.Number == dto.Number);
    if (o == null)
        return;
    if (dto.Status != o.Status && dto.Status != "")
    {
        o.Status = dto.Status;
        message += $"������ ������ �{o.Number} �������\n";
        if (o.Status == "���������")
        {
            message += $"������ �{o.Number} ���������\n";
            o.EndDate = DateOnly.FromDateTime(DateTime.Now);
        }
    }
    if (dto.ProblemDysc != "")
        o.ProblemDysc = dto.ProblemDysc;
    if (dto.Master != "")
        o.Master = dto.Master;
    if (dto.Comment != "")
        o.Comments.Add(dto.Comment);
});


int complete_count() => repo.FindAll(x => x.Status == "���������").Count;

Dictionary<string, int> get_problem_type_stat() =>
    repo.GroupBy(x => x.ProblemDysc)
    .Select(x => (x.Key, x.Count()))
    .ToDictionary(k => k.Key, v => v.Item2);

double get_average_time_to_complete() =>
    complete_count() == 0 ? 0 :
    repo.FindAll(x => x.Status == "���������")
    .Select(x => x.EndDate.Value.DayNumber - x.StartDate.DayNumber)
    .Sum() / complete_count();

app.MapGet("/stat", () => new {
    complete_count = complete_count(),
    problem_type_stat = get_problem_type_stat(),
    average_time_to_complete = get_average_time_to_complete()
});


app.Run();

public class Order
{
    public Order(int number, DateOnly startDate, string kind, string model, string problemDysc, string name, string surname, string lastname, string phone, string status)
    {
        Number = number;
        StartDate = startDate;
        Kind = kind;
        Model = model;
        ProblemDysc = problemDysc;
        Name = name;
        Surname = surname;
        Lastname = lastname;
        Phone = phone;
        Status = status;
    }

    public int Number { get; set; }
    public DateOnly StartDate { get; set; }
    public string Kind { get; set; }
    public string Model { get; set; }
    public string ProblemDysc { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Lastname { get; set; }
    public string Phone { get; set; }
    public string Status { get; set; }
    public DateOnly? EndDate { get; set; } = null;
    public string Master { get; set; } = "�� ��������";
    public List<string>? Comments { get; set; } = [];
};

record class UpdateOrder(int Number, string? Status = "", string? ProblemDysc = "", string? Master = "", string? Comment = "");
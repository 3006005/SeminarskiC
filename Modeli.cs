using System.Text.Json.Serialization;

namespace SeminarskiC;

/// <summary>Zajednička klasa Student (zadaci 7, 13, 14, 17, 18, 19).</summary>
public class Student
{
    private int _godina = 1;
    private double _prosjek;

    public int Id { get; set; }
    public string Ime { get; set; } = "";
    public string Prezime { get; set; } = "";
    public string Smjer { get; set; } = "";

    public int GodinaStudija
    {
        get => _godina;
        set
        {
            if (value < 1 || value > 5)
                throw new ArgumentOutOfRangeException(nameof(GodinaStudija), "Godina studija mora biti od 1 do 5.");
            _godina = value;
        }
    }

    public double Prosjek
    {
        get => _prosjek;
        set
        {
            if (value < 0 || value > 10)
                throw new ArgumentOutOfRangeException(nameof(Prosjek), "Prosjek mora biti između 0 i 10.");
            _prosjek = value;
        }
    }

    [JsonIgnore]
    public string PunoIme => $"{Ime} {Prezime}";

    public Student() { }

    public Student(int id, string ime, string prezime, int godina, double prosjek, string smjer)
    {
        Id = id;
        Ime = ime;
        Prezime = prezime;
        GodinaStudija = godina;
        Prosjek = prosjek;
        Smjer = smjer;
    }

    public void UpisiSljedecuGodinu()
    {
        if (_godina >= 5)
            throw new InvalidOperationException("Student je već na posljednjoj godini.");
        _godina++;
    }

    public override string ToString() =>
        $"[{Id}] {PunoIme}, {GodinaStudija}. godina, {Smjer}, prosjek {Prosjek:F2}";
}

public static class Podaci
{
    public static List<Student> ListaStudenata() => new()
    {
        new Student(1,  "Amar",   "Hodžić", 2, 8.4, "Informatika"),
        new Student(2,  "Lejla",  "Kovač",  3, 9.1, "Informatika"),
        new Student(3,  "Emir",   "Begić",  1, 7.2, "Elektrotehnika"),
        new Student(4,  "Sara",   "Mujić",  3, 8.8, "Elektrotehnika"),
        new Student(5,  "Haris",  "Zukić",  4, 6.9, "Mašinstvo"),
        new Student(6,  "Amina",  "Delić",  2, 9.5, "Informatika"),
        new Student(7,  "Tarik",  "Šabić",  3, 7.9, "Mašinstvo"),
        new Student(8,  "Nejra",  "Alić",   1, 8.1, "Informatika"),
        new Student(9,  "Kenan",  "Ibrić",  4, 6.4, "Elektrotehnika"),
        new Student(10, "Dalila", "Smajić", 2, 8.0, "Mašinstvo"),
    };
}

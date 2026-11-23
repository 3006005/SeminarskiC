using Microsoft.Data.Sqlite;

namespace SeminarskiC;

public static class Baza
{
    public const string StudentiConn = "Data Source=studenti.db";
    public const string SkolaConn = "Data Source=skola.db;Foreign Keys=True";

    public static void KreirajTabeluStudenti(SqliteConnection conn)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            DROP TABLE IF EXISTS Studenti;
            CREATE TABLE Studenti (
                Id            INTEGER PRIMARY KEY AUTOINCREMENT,
                Ime           TEXT    NOT NULL,
                Prezime       TEXT    NOT NULL,
                GodinaStudija INTEGER NOT NULL,
                Prosjek       REAL    NOT NULL,
                Smjer         TEXT    NOT NULL
            );
            """;
        cmd.ExecuteNonQuery();
    }

    public static Student CitajStudenta(SqliteDataReader r) =>
        new Student(r.GetInt32(0), r.GetString(1), r.GetString(2), r.GetInt32(3), r.GetDouble(4), r.GetString(5));
}

// ===================== Zadatak 19: slojevita arhitektura =====================

public interface IStudentRepository
{
    void Dodaj(Student student);
    Student? NadjiPoId(int id);
    List<Student> Pretrazi(string tekst);
    List<Student> Svi();
    bool Obrisi(int id);
}

public class SqliteStudentRepository : IStudentRepository
{
    private readonly string _connectionString;

    public SqliteStudentRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void InicijalizirajSemu()
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        Baza.KreirajTabeluStudenti(conn);
    }

    public void Dodaj(Student student)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Studenti (Ime, Prezime, GodinaStudija, Prosjek, Smjer)
            VALUES ($ime, $prezime, $godina, $prosjek, $smjer)
            """;
        cmd.Parameters.AddWithValue("$ime", student.Ime);
        cmd.Parameters.AddWithValue("$prezime", student.Prezime);
        cmd.Parameters.AddWithValue("$godina", student.GodinaStudija);
        cmd.Parameters.AddWithValue("$prosjek", student.Prosjek);
        cmd.Parameters.AddWithValue("$smjer", student.Smjer);
        cmd.ExecuteNonQuery();

        using var idCmd = conn.CreateCommand();
        idCmd.CommandText = "SELECT last_insert_rowid()";
        student.Id = Convert.ToInt32(idCmd.ExecuteScalar());
    }

    public Student? NadjiPoId(int id)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Ime, Prezime, GodinaStudija, Prosjek, Smjer FROM Studenti WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", id);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? Baza.CitajStudenta(reader) : null;
    }

    public List<Student> Pretrazi(string tekst)
    {
        var rezultat = new List<Student>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT Id, Ime, Prezime, GodinaStudija, Prosjek, Smjer FROM Studenti
            WHERE Ime LIKE $q OR Prezime LIKE $q
            ORDER BY Prezime, Ime
            """;
        cmd.Parameters.AddWithValue("$q", $"%{tekst}%");
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            rezultat.Add(Baza.CitajStudenta(reader));
        return rezultat;
    }

    public List<Student> Svi()
    {
        var rezultat = new List<Student>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Ime, Prezime, GodinaStudija, Prosjek, Smjer FROM Studenti ORDER BY Id";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            rezultat.Add(Baza.CitajStudenta(reader));
        return rezultat;
    }

    public bool Obrisi(int id)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Studenti WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", id);
        return cmd.ExecuteNonQuery() > 0;
    }
}

public class StudentService
{
    private readonly IStudentRepository _repo;

    public StudentService(IStudentRepository repo)
    {
        _repo = repo;
    }

    public Student Upisi(string ime, string prezime, int godina, double prosjek, string smjer)
    {
        if (string.IsNullOrWhiteSpace(ime) || string.IsNullOrWhiteSpace(prezime))
            throw new ArgumentException("Ime i prezime su obavezni.");

        // Konstruktor klase Student validira godinu i prosjek.
        var student = new Student(0, ime.Trim(), prezime.Trim(), godina, prosjek, smjer);
        _repo.Dodaj(student);
        return student;
    }

    public List<Student> Pretrazi(string tekst) => _repo.Pretrazi(tekst);

    public Student? Najbolji() => _repo.Svi().MaxBy(s => s.Prosjek);

    public Dictionary<string, double> ProsjekPoSmjeru() =>
        _repo.Svi().GroupBy(s => s.Smjer).ToDictionary(g => g.Key, g => g.Average(s => s.Prosjek));
}

// ===================== Zadatak 20: mini-sistem =====================

public sealed class SkolskiSistem : IDisposable
{
    private readonly SqliteConnection _conn;

    public SkolskiSistem(string connectionString)
    {
        _conn = new SqliteConnection(connectionString);
        _conn.Open();
    }

    public void KreirajSemu()
    {
        Izvrsi("""
            DROP TABLE IF EXISTS Ocjene;
            DROP TABLE IF EXISTS Predmeti;
            DROP TABLE IF EXISTS Studenti;

            CREATE TABLE Studenti (
                Id            INTEGER PRIMARY KEY AUTOINCREMENT,
                Ime           TEXT    NOT NULL,
                Prezime       TEXT    NOT NULL,
                GodinaStudija INTEGER NOT NULL,
                Prosjek       REAL    NOT NULL DEFAULT 0,
                Smjer         TEXT    NOT NULL
            );

            CREATE TABLE Predmeti (
                Id    INTEGER PRIMARY KEY AUTOINCREMENT,
                Naziv TEXT    NOT NULL UNIQUE,
                Ects  INTEGER NOT NULL
            );

            CREATE TABLE Ocjene (
                Id        INTEGER PRIMARY KEY AUTOINCREMENT,
                StudentId INTEGER NOT NULL REFERENCES Studenti(Id),
                PredmetId INTEGER NOT NULL REFERENCES Predmeti(Id),
                Ocjena    INTEGER NOT NULL CHECK (Ocjena BETWEEN 5 AND 10),
                Datum     TEXT    NOT NULL,
                UNIQUE (StudentId, PredmetId)
            );
            """);
    }

    public int DodajStudenta(string ime, string prezime, int godina, string smjer)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "INSERT INTO Studenti (Ime, Prezime, GodinaStudija, Smjer) VALUES ($i, $p, $g, $s)";
        cmd.Parameters.AddWithValue("$i", ime);
        cmd.Parameters.AddWithValue("$p", prezime);
        cmd.Parameters.AddWithValue("$g", godina);
        cmd.Parameters.AddWithValue("$s", smjer);
        cmd.ExecuteNonQuery();
        return PosljednjiId();
    }

    public int DodajPredmet(string naziv, int ects)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "INSERT INTO Predmeti (Naziv, Ects) VALUES ($n, $e)";
        cmd.Parameters.AddWithValue("$n", naziv);
        cmd.Parameters.AddWithValue("$e", ects);
        cmd.ExecuteNonQuery();
        return PosljednjiId();
    }

    /// <summary>Upisuje sve ocjene u jednoj transakciji: ili se upišu sve, ili nijedna.</summary>
    public void UpisiOcjene(IEnumerable<(int StudentId, int PredmetId, int Ocjena)> ocjene)
    {
        using var transakcija = _conn.BeginTransaction();
        using var cmd = _conn.CreateCommand();
        cmd.Transaction = transakcija;
        cmd.CommandText = "INSERT INTO Ocjene (StudentId, PredmetId, Ocjena, Datum) VALUES ($s, $p, $o, $d)";
        SqliteParameter pStudent = cmd.Parameters.Add("$s", SqliteType.Integer);
        SqliteParameter pPredmet = cmd.Parameters.Add("$p", SqliteType.Integer);
        SqliteParameter pOcjena = cmd.Parameters.Add("$o", SqliteType.Integer);
        SqliteParameter pDatum = cmd.Parameters.Add("$d", SqliteType.Text);

        try
        {
            foreach (var (studentId, predmetId, ocjena) in ocjene)
            {
                pStudent.Value = studentId;
                pPredmet.Value = predmetId;
                pOcjena.Value = ocjena;
                pDatum.Value = DateTime.Now.ToString("yyyy-MM-dd");
                cmd.ExecuteNonQuery();
            }
            transakcija.Commit();
        }
        catch
        {
            transakcija.Rollback();
            throw;
        }
    }

    public void AzurirajProsjeke() =>
        Izvrsi("""
            UPDATE Studenti
            SET Prosjek = COALESCE((SELECT AVG(Ocjena) FROM Ocjene WHERE StudentId = Studenti.Id), 0)
            """);

    public long BrojOcjena()
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Ocjene";
        return Convert.ToInt64(cmd.ExecuteScalar());
    }

    public void Ispisi(string naslov, string sql)
    {
        Console.WriteLine($"\n== {naslov} ==");
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = sql;
        using var reader = cmd.ExecuteReader();

        for (int i = 0; i < reader.FieldCount; i++)
            Console.Write($"{reader.GetName(i),-22}");
        Console.WriteLine();
        Console.WriteLine(new string('-', 22 * reader.FieldCount));

        while (reader.Read())
        {
            for (int i = 0; i < reader.FieldCount; i++)
                Console.Write($"{Convert.ToString(reader.GetValue(i), System.Globalization.CultureInfo.InvariantCulture),-22}");
            Console.WriteLine();
        }
    }

    private void Izvrsi(string sql)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }

    private int PosljednjiId()
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SELECT last_insert_rowid()";
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void Dispose() => _conn.Dispose();
}

// ===================== Zadaci 18-20 =====================

public static class BazaZadaci
{
    // ZADATAK 18 — CRUD nad SQLite bazom (ADO.NET)
    // TEKST: Povezati se na SQLite bazu, kreirati tabelu Studenti i implementirati operacije Create, Read,
    // Update i Delete pomoću ADO.NET-a (SqliteConnection i SqliteCommand) s parametrizovanim upitima.
    // OBRAZLOŽENJE: SqliteConnection otvara vezu s bazom, a SqliteCommand izvršava SQL. ExecuteNonQuery se koristi
    // za INSERT/UPDATE/DELETE, ExecuteReader za čitanje redova, a ExecuteScalar za jednu vrijednost. Parametri
    // ($ime, $id...) sprečavaju SQL injection. Blok "using" osigurava da se veza zatvori čak i kada nastane greška.
    public static void Z18_CrudSqlite()
    {
        using var conn = new SqliteConnection(Baza.StudentiConn);
        conn.Open();
        Baza.KreirajTabeluStudenti(conn);

        Console.WriteLine("CREATE:");
        int id1 = Dodaj(conn, new Student(0, "Amar", "Hodžić", 2, 8.4, "Informatika"));
        int id2 = Dodaj(conn, new Student(0, "Lejla", "Kovač", 3, 9.1, "Informatika"));
        int id3 = Dodaj(conn, new Student(0, "Emir", "Begić", 1, 7.2, "Elektrotehnika"));
        Console.WriteLine($"  Dodani studenti s Id: {id1}, {id2}, {id3}");

        Console.WriteLine("READ:");
        foreach (Student s in SviStudenti(conn))
            Console.WriteLine("  " + s);

        Console.WriteLine("UPDATE:");
        int izmijenjeno = AzurirajProsjek(conn, id3, 8.0);
        Console.WriteLine($"  Izmijenjeno redova: {izmijenjeno}");

        Console.WriteLine("DELETE:");
        int obrisano = Obrisi(conn, id1);
        Console.WriteLine($"  Obrisano redova: {obrisano}");

        Console.WriteLine("Stanje nakon izmjena:");
        foreach (Student s in SviStudenti(conn))
            Console.WriteLine("  " + s);
    }

    private static int Dodaj(SqliteConnection conn, Student s)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Studenti (Ime, Prezime, GodinaStudija, Prosjek, Smjer)
            VALUES ($ime, $prezime, $godina, $prosjek, $smjer)
            """;
        cmd.Parameters.AddWithValue("$ime", s.Ime);
        cmd.Parameters.AddWithValue("$prezime", s.Prezime);
        cmd.Parameters.AddWithValue("$godina", s.GodinaStudija);
        cmd.Parameters.AddWithValue("$prosjek", s.Prosjek);
        cmd.Parameters.AddWithValue("$smjer", s.Smjer);
        cmd.ExecuteNonQuery();

        using var idCmd = conn.CreateCommand();
        idCmd.CommandText = "SELECT last_insert_rowid()";
        return Convert.ToInt32(idCmd.ExecuteScalar());
    }

    private static List<Student> SviStudenti(SqliteConnection conn)
    {
        var lista = new List<Student>();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Ime, Prezime, GodinaStudija, Prosjek, Smjer FROM Studenti ORDER BY Id";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            lista.Add(Baza.CitajStudenta(reader));
        return lista;
    }

    private static int AzurirajProsjek(SqliteConnection conn, int id, double prosjek)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Studenti SET Prosjek = $prosjek WHERE Id = $id";
        cmd.Parameters.AddWithValue("$prosjek", prosjek);
        cmd.Parameters.AddWithValue("$id", id);
        return cmd.ExecuteNonQuery();
    }

    private static int Obrisi(SqliteConnection conn, int id)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Studenti WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", id);
        return cmd.ExecuteNonQuery();
    }

    // ZADATAK 19 — Repository pattern
    // TEKST: Organizovati pristup bazi kroz slojeve: interfejs IStudentRepository, implementaciju
    // SqliteStudentRepository i klasu StudentService s poslovnom logikom (validacija, najbolji student, prosjek po smjeru).
    // OBRAZLOŽENJE: Repository skriva SQL od ostatka programa; servis ovisi samo o interfejsu, pa se baza može
    // zamijeniti (npr. SQL Server ili testna lista u memoriji) bez izmjene poslovne logike. Servis validira ulaz prije
    // upisa i računa statistiku LINQ-om nad podacima iz repozitorija. To je osnova slojevite arhitekture.
    public static void Z19_Repository()
    {
        var repo = new SqliteStudentRepository(Baza.StudentiConn);
        repo.InicijalizirajSemu();
        var servis = new StudentService(repo);

        foreach (Student s in Podaci.ListaStudenata().Take(6))
            servis.Upisi(s.Ime, s.Prezime, s.GodinaStudija, s.Prosjek, s.Smjer);

        Console.WriteLine("Pretraga \"am\":");
        foreach (Student s in servis.Pretrazi("am"))
            Console.WriteLine("  " + s);

        Student? najbolji = servis.Najbolji();
        Console.WriteLine($"Najbolji student: {najbolji?.PunoIme} ({najbolji?.Prosjek:F2})");

        Console.WriteLine("Prosjek po smjeru:");
        foreach (var par in servis.ProsjekPoSmjeru())
            Console.WriteLine($"  {par.Key}: {par.Value:F2}");

        Student? pronadjen = repo.NadjiPoId(2);
        Console.WriteLine($"NadjiPoId(2): {pronadjen}");
        Console.WriteLine($"Obrisi(2): {repo.Obrisi(2)}, NadjiPoId(2) nakon brisanja: {(repo.NadjiPoId(2) is null ? "nema" : "postoji")}");

        try
        {
            servis.Upisi("", "Prazno", 1, 7.0, "Informatika");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Greška: {ex.Message}");
        }
    }

    // ZADATAK 20 — Mini-sistem: studenti, predmeti i ocjene
    // TEKST: Napraviti bazu s tabelama Studenti, Predmeti i Ocjene (strani ključevi). Unijeti podatke u transakciji,
    // izračunati prosjeke, a zatim izdati izvještaje JOIN upitima: ocjene po studentu, rang lista, statistika po
    // predmetu i studenti bez negativne ocjene. Pokazati da transakcija s greškom poništava sve izmjene.
    // OBRAZLOŽENJE: Tabela Ocjene povezuje studente i predmete (veza više-na-više) preko stranih ključeva, a CHECK i
    // UNIQUE ograničenja čuvaju ispravnost podataka. JOIN spaja tri tabele, GROUP BY s AVG/MIN/MAX/COUNT daje agregate,
    // a HAVING filtrira grupe. Transakcija osigurava atomarnost: kad jedna ocjena krši ograničenje, rollback poništava i
    // ranije upisane ocjene iz iste transakcije. Klasa SkolskiSistem kapsulira cijeli rad s bazom.
    public static void Z20_MiniSistem()
    {
        using var sistem = new SkolskiSistem(Baza.SkolaConn);
        sistem.KreirajSemu();

        string[,] studenti =
        {
            { "Amar", "Hodžić", "Informatika" }, { "Lejla", "Kovač", "Informatika" },
            { "Emir", "Begić", "Elektrotehnika" }, { "Sara", "Mujić", "Elektrotehnika" },
            { "Haris", "Zukić", "Mašinstvo" }
        };
        var idStudenata = new List<int>();
        for (int i = 0; i < studenti.GetLength(0); i++)
            idStudenata.Add(sistem.DodajStudenta(studenti[i, 0], studenti[i, 1], 2, studenti[i, 2]));

        var idPredmeta = new List<int>
        {
            sistem.DodajPredmet("Programiranje", 6),
            sistem.DodajPredmet("Baze podataka", 6),
            sistem.DodajPredmet("Matematika", 5),
            sistem.DodajPredmet("Računarske mreže", 5)
        };

        int[,] ocjene =
        {
            { 9, 8, 7, 8 },    // Amar
            { 10, 9, 9, 10 },  // Lejla
            { 7, 6, 5, 7 },    // Emir (pao Matematiku)
            { 8, 9, 8, 9 },    // Sara
            { 6, 7, 6, 6 }     // Haris
        };
        var zaUpis = new List<(int, int, int)>();
        for (int s = 0; s < ocjene.GetLength(0); s++)
            for (int p = 0; p < ocjene.GetLength(1); p++)
                zaUpis.Add((idStudenata[s], idPredmeta[p], ocjene[s, p]));

        sistem.UpisiOcjene(zaUpis);
        sistem.AzurirajProsjeke();
        Console.WriteLine($"Upisano ocjena: {sistem.BrojOcjena()}");

        sistem.Ispisi("Ocjene po studentu (JOIN 3 tabele)", """
            SELECT s.Ime || ' ' || s.Prezime AS Student, p.Naziv AS Predmet, o.Ocjena
            FROM Ocjene o
            JOIN Studenti s ON s.Id = o.StudentId
            JOIN Predmeti p ON p.Id = o.PredmetId
            ORDER BY s.Prezime, p.Naziv
            """);

        sistem.Ispisi("Rang lista studenata (GROUP BY, AVG)", """
            SELECT s.Ime || ' ' || s.Prezime AS Student,
                   ROUND(AVG(o.Ocjena), 2)   AS Prosjek,
                   COUNT(*)                  AS Ispita,
                   SUM(p.Ects)               AS Ects
            FROM Studenti s
            JOIN Ocjene o  ON o.StudentId = s.Id
            JOIN Predmeti p ON p.Id = o.PredmetId
            GROUP BY s.Id
            ORDER BY AVG(o.Ocjena) DESC
            """);

        sistem.Ispisi("Statistika po predmetu", """
            SELECT p.Naziv AS Predmet,
                   ROUND(AVG(o.Ocjena), 2) AS Prosjek,
                   MIN(o.Ocjena) AS Min,
                   MAX(o.Ocjena) AS Max,
                   ROUND(SUM(CASE WHEN o.Ocjena >= 6 THEN 1 ELSE 0 END) * 100.0 / COUNT(*), 0) AS ProlaznostPosto
            FROM Predmeti p
            JOIN Ocjene o ON o.PredmetId = p.Id
            GROUP BY p.Id
            ORDER BY p.Naziv
            """);

        sistem.Ispisi("Studenti bez negativne ocjene (HAVING)", """
            SELECT s.Ime || ' ' || s.Prezime AS Student, MIN(o.Ocjena) AS NajnizaOcjena
            FROM Studenti s
            JOIN Ocjene o ON o.StudentId = s.Id
            GROUP BY s.Id
            HAVING MIN(o.Ocjena) >= 6
            ORDER BY s.Prezime
            """);

        Console.WriteLine("\n== Test transakcije ==");
        long prije = sistem.BrojOcjena();
        int probni = sistem.DodajStudenta("Probni", "Student", 1, "Informatika");
        try
        {
            sistem.UpisiOcjene(new List<(int StudentId, int PredmetId, int Ocjena)>
            {
                (probni, idPredmeta[0], 9),   // ispravna ocjena
                (9999, idPredmeta[1], 8)      // nepostojeći student -> krši strani ključ
            });
        }
        catch (SqliteException ex)
        {
            Console.WriteLine($"Greška baze: {ex.Message}. Transakcija je poništena.");
        }
        Console.WriteLine($"Broj ocjena prije: {prije}, poslije: {sistem.BrojOcjena()}");
    }
}

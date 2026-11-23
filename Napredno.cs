using System.Diagnostics;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace SeminarskiC;

/// <summary>Zadaci 13-17: LINQ, fajlovi, delegati i događaji, async/await, JSON.</summary>
public static class Napredno
{
    // ZADATAK 13 — LINQ upiti
    // TEKST: Nad listom od 10 studenata napisati LINQ upite: filtriranje, sortiranje, top 3 po prosjeku,
    // grupisanje po smjeru s brojem studenata i prosjekom, te upite Any, All i MaxBy.
    // OBRAZLOŽENJE: LINQ omogućava deklarativno pisanje upita umjesto ručnih petlji. Where filtrira, OrderBy
    // sortira, Take uzima prvih n, a GroupBy pravi grupe iz kojih Select gradi anonimne objekte s agregatima
    // (Count, Average). Any i All vraćaju bool, a MaxBy vraća cijeli objekt s najvećom vrijednošću ključa.
    public static void Z13_Linq()
    {
        List<Student> studenti = Podaci.ListaStudenata();

        Console.WriteLine("1) Studenti 3. godine s prosjekom >= 8:");
        foreach (Student st in studenti.Where(x => x.GodinaStudija == 3 && x.Prosjek >= 8).OrderBy(x => x.Prezime))
            Console.WriteLine($"   {st.PunoIme} ({st.Prosjek:F2})");

        Console.WriteLine("2) Top 3 po prosjeku:");
        foreach (Student st in studenti.OrderByDescending(x => x.Prosjek).Take(3))
            Console.WriteLine($"   {st.PunoIme} ({st.Prosjek:F2})");

        Console.WriteLine("3) Statistika po smjeru:");
        var poSmjeru = studenti
            .GroupBy(x => x.Smjer)
            .Select(g => new { Smjer = g.Key, Broj = g.Count(), Prosjek = g.Average(x => x.Prosjek) })
            .OrderByDescending(x => x.Prosjek);
        foreach (var grupa in poSmjeru)
            Console.WriteLine($"   {grupa.Smjer,-15} studenata: {grupa.Broj}, prosjek: {grupa.Prosjek:F2}");

        Console.WriteLine($"4) Ukupni prosjek: {studenti.Average(x => x.Prosjek):F2}");
        Console.WriteLine($"   Ima li studenata s prosjekom ispod 6.5? {studenti.Any(x => x.Prosjek < 6.5)}");
        Console.WriteLine($"   Imaju li svi prosjek iznad 6? {studenti.All(x => x.Prosjek > 6)}");
        Student najbolji = studenti.MaxBy(x => x.Prosjek)!;
        Console.WriteLine($"   Najbolji student: {najbolji.PunoIme} ({najbolji.Prosjek:F2})");

        string prvaGodina = string.Join(", ", studenti.Where(x => x.GodinaStudija == 1).Select(x => x.Ime));
        Console.WriteLine($"5) Studenti prve godine: {prvaGodina}");
    }

    // ZADATAK 14 — Čitanje i pisanje fajlova
    // TEKST: Spremiti listu studenata u tekstualni CSV fajl, učitati je nazad u objekte i u log fajl dopisati
    // zapis o učitavanju. Obraditi slučaj da fajl ne postoji.
    // OBRAZLOŽENJE: File.WriteAllLines upisuje linije u fajl, a File.ReadAllLines ih čita. Svaki red se razdvaja
    // znakom ';' (Split) i parsira u Student objekat; decimalni brojevi koriste InvariantCulture da zarez ne bi
    // pokvario format. StreamWriter s append=true dopisuje log, a FileNotFoundException se hvata u catch bloku.
    public static void Z14_Fajlovi()
    {
        string folder = "podaci";
        Directory.CreateDirectory(folder);
        string putanja = Path.Combine(folder, "studenti.csv");
        string logPutanja = Path.Combine(folder, "log.txt");

        IEnumerable<string> linije = Podaci.ListaStudenata().Select(s => string.Join(';',
            s.Id, s.Ime, s.Prezime, s.GodinaStudija,
            s.Prosjek.ToString("F2", CultureInfo.InvariantCulture), s.Smjer));
        File.WriteAllLines(putanja, linije);
        Console.WriteLine($"Zapisano u {putanja}");

        var ucitani = new List<Student>();
        foreach (string linija in File.ReadAllLines(putanja))
        {
            string[] d = linija.Split(';');
            ucitani.Add(new Student(int.Parse(d[0]), d[1], d[2], int.Parse(d[3]),
                double.Parse(d[4], CultureInfo.InvariantCulture), d[5]));
        }
        Console.WriteLine($"Učitano studenata: {ucitani.Count}");
        foreach (Student s in ucitani.Take(3))
            Console.WriteLine("  " + s);

        using (var pisac = new StreamWriter(logPutanja, append: true))
        {
            pisac.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - učitano {ucitani.Count} studenata");
        }
        Console.WriteLine($"Log fajl ima {File.ReadAllLines(logPutanja).Length} zapisa.");

        try
        {
            File.ReadAllText(Path.Combine(folder, "nepostojeci.txt"));
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("Greška: traženi fajl ne postoji.");
        }
    }

    // ZADATAK 15 — Delegati i događaji
    // TEKST: Proširiti bankovni račun događajem PromjenaStanja. Na događaj pretplatiti metodu koja simulira SMS
    // obavijest i lambdu koja upozorava na nisko stanje, a zatim se odjaviti. Prikazati i vlastiti delegat te Func i Action.
    // OBRAZLOŽENJE: Događaj (event) je siguran oblik delegata: izvana se mogu samo dodati (+=) ili ukloniti (-=)
    // pretplatnici, a pozvati ga može samo klasa koja ga posjeduje. Račun ne zna ko ga sluša, što smanjuje povezanost
    // klasa. Delegat Operacija opisuje potpis metode, dok su Func i Action ugrađeni generički delegati.
    public delegate decimal Operacija(decimal a, decimal b);

    public static void Z15_DelegatiIDogadjaji()
    {
        var racun = new BankovniRacun("Amar Hodžić", "BA39 1234 5678", 100m);

        racun.PromjenaStanja += PosaljiSms;
        racun.PromjenaStanja += (sender, e) =>
        {
            if (e.NovoStanje < 50m)
                Console.WriteLine("   [UPOZORENJE] Stanje računa je ispod 50 KM!");
        };

        racun.Uplati(200m);
        racun.Isplati(260m);

        racun.PromjenaStanja -= PosaljiSms;
        Console.WriteLine("Odjavljen SMS pretplatnik:");
        racun.Uplati(10m);

        Operacija zbir = (a, b) => a + b;
        Func<decimal, decimal> porez = iznos => iznos * 0.17m;
        Action<string> ispis = Console.WriteLine;
        ispis($"Delegat: 120 + 80 = {zbir(120m, 80m)}, PDV na 200 = {porez(200m):F2}");
    }

    private static void PosaljiSms(object? sender, PromjenaStanjaEventArgs e)
    {
        string vlasnik = (sender as BankovniRacun)?.Vlasnik ?? "nepoznato";
        Console.WriteLine($"   [SMS za {vlasnik}] {e.Vrsta}: {e.Iznos:F2} KM, novo stanje {e.NovoStanje:F2} KM");
    }

    // ZADATAK 16 — Async/await
    // TEKST: Simulirati preuzimanje podataka iz tri izvora s različitim trajanjem. Uporediti sekvencijalno i
    // paralelno izvršavanje i prikazati prekid operacije zbog isteka vremena (timeout).
    // OBRAZLOŽENJE: Metoda označena kao async vraća Task, a await čeka rezultat bez blokiranja niti. Sekvencijalno
    // trajanje je zbir svih čekanja, dok Task.WhenAll pokreće sva preuzimanja istovremeno pa traje koliko najduže.
    // CancellationTokenSource s vremenskim ograničenjem prekida operaciju, što se hvata kao OperationCanceledException.
    public static void Z16_AsyncAwait() => Z16Async().GetAwaiter().GetResult();

    private static async Task Z16Async()
    {
        var sat = Stopwatch.StartNew();
        Console.WriteLine("Sekvencijalno preuzimanje:");
        await PreuzmiPodatke("Korisnici", 700);
        await PreuzmiPodatke("Narudžbe", 500);
        await PreuzmiPodatke("Proizvodi", 600);
        Console.WriteLine($"Ukupno trajanje: {sat.ElapsedMilliseconds} ms\n");

        sat.Restart();
        Console.WriteLine("Paralelno preuzimanje:");
        string[] rezultati = await Task.WhenAll(
            PreuzmiPodatke("Korisnici", 700),
            PreuzmiPodatke("Narudžbe", 500),
            PreuzmiPodatke("Proizvodi", 600));
        Console.WriteLine($"Ukupno trajanje: {sat.ElapsedMilliseconds} ms ({rezultati.Length} izvora)\n");

        Console.WriteLine("Preuzimanje s ograničenjem od 300 ms:");
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));
        try
        {
            await PreuzmiPodatke("Izvještaji", 2000, cts.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("  Operacija prekinuta: isteklo je vrijeme.");
        }
    }

    private static async Task<string> PreuzmiPodatke(string izvor, int trajanjeMs, CancellationToken token = default)
    {
        Console.WriteLine($"  Počinje: {izvor}");
        await Task.Delay(trajanjeMs, token);
        Console.WriteLine($"  Završeno: {izvor}");
        return $"{izvor} ({trajanjeMs} ms)";
    }

    // ZADATAK 17 — JSON serijalizacija
    // TEKST: Listu studenata serijalizovati u JSON fajl, zatim je deserijalizovati i ispisati studente s prosjekom
    // iznad 8.5. Obraditi neispravan JSON.
    // OBRAZLOŽENJE: JsonSerializer.Serialize pretvara objekte u JSON tekst, a Deserialize ga vraća u objekte
    // istog tipa. Opcija WriteIndented daje čitljiv ispis, a atribut [JsonIgnore] na svojstvu PunoIme sprečava
    // da se izvedeno svojstvo upiše. Neispravan JSON baca JsonException koja se hvata u try/catch.
    public static void Z17_Json()
    {
        Directory.CreateDirectory("podaci");
        string putanja = Path.Combine("podaci", "studenti.json");

        var opcije = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        string json = JsonSerializer.Serialize(Podaci.ListaStudenata(), opcije);
        File.WriteAllText(putanja, json);
        Console.WriteLine($"Prvih nekoliko redova JSON-a ({putanja}):");
        foreach (string red in json.Split('\n').Take(9))
            Console.WriteLine("  " + red.TrimEnd());

        List<Student>? ucitani = JsonSerializer.Deserialize<List<Student>>(File.ReadAllText(putanja));
        if (ucitani is null) return;

        Console.WriteLine("Studenti s prosjekom iznad 8.5:");
        foreach (Student s in ucitani.Where(x => x.Prosjek > 8.5))
            Console.WriteLine("  " + s);

        try
        {
            JsonSerializer.Deserialize<List<Student>>("{ ovo nije ispravan json ");
        }
        catch (JsonException)
        {
            Console.WriteLine("Greška: JSON nije ispravan.");
        }
    }
}

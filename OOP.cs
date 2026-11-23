namespace SeminarskiC;

// ===================== Klase koje koriste zadaci 7-12 (i 15) =====================

public class NedovoljnoSredstavaException : Exception
{
    public decimal Stanje { get; }
    public decimal Trazeno { get; }

    public NedovoljnoSredstavaException(decimal stanje, decimal trazeno)
        : base($"Nedovoljno sredstava: stanje {stanje:F2} KM, traženo {trazeno:F2} KM.")
    {
        Stanje = stanje;
        Trazeno = trazeno;
    }
}

public class PromjenaStanjaEventArgs : EventArgs
{
    public string Vrsta { get; }
    public decimal Iznos { get; }
    public decimal NovoStanje { get; }

    public PromjenaStanjaEventArgs(string vrsta, decimal iznos, decimal novoStanje)
    {
        Vrsta = vrsta;
        Iznos = iznos;
        NovoStanje = novoStanje;
    }
}

public class BankovniRacun
{
    private decimal _stanje;
    private readonly List<string> _historija = new();

    public string Vlasnik { get; }
    public string BrojRacuna { get; }
    public decimal Stanje => _stanje;
    public IReadOnlyList<string> Historija => _historija;

    public event EventHandler<PromjenaStanjaEventArgs>? PromjenaStanja;

    public BankovniRacun(string vlasnik, string brojRacuna, decimal pocetnoStanje = 0m)
    {
        if (pocetnoStanje < 0)
            throw new ArgumentException("Početno stanje ne može biti negativno.", nameof(pocetnoStanje));
        Vlasnik = vlasnik;
        BrojRacuna = brojRacuna;
        _stanje = pocetnoStanje;
    }

    public void Uplati(decimal iznos)
    {
        if (iznos <= 0)
            throw new ArgumentException("Iznos uplate mora biti pozitivan.", nameof(iznos));
        _stanje += iznos;
        Zabiljezi("Uplata", iznos);
    }

    public void Isplati(decimal iznos)
    {
        if (iznos <= 0)
            throw new ArgumentException("Iznos isplate mora biti pozitivan.", nameof(iznos));
        if (iznos > _stanje)
            throw new NedovoljnoSredstavaException(_stanje, iznos);
        _stanje -= iznos;
        Zabiljezi("Isplata", iznos);
    }

    private void Zabiljezi(string vrsta, decimal iznos)
    {
        _historija.Add($"{vrsta} {iznos:F2} KM, stanje {_stanje:F2} KM");
        PromjenaStanja?.Invoke(this, new PromjenaStanjaEventArgs(vrsta, iznos, _stanje));
    }
}

public abstract class Oblik
{
    public abstract string Naziv { get; }
    public abstract double Povrsina();
    public abstract double Obim();

    public override string ToString() => $"{Naziv}: P = {Povrsina():F2}, O = {Obim():F2}";
}

public class Krug : Oblik
{
    private readonly double _r;
    public Krug(double r)
    {
        if (r <= 0) throw new ArgumentException("Poluprečnik mora biti pozitivan.");
        _r = r;
    }
    public override string Naziv => "Krug";
    public override double Povrsina() => Math.PI * _r * _r;
    public override double Obim() => 2 * Math.PI * _r;
}

public class Pravougaonik : Oblik
{
    private readonly double _a, _b;
    public Pravougaonik(double a, double b)
    {
        if (a <= 0 || b <= 0) throw new ArgumentException("Stranice moraju biti pozitivne.");
        _a = a;
        _b = b;
    }
    public override string Naziv => "Pravougaonik";
    public override double Povrsina() => _a * _b;
    public override double Obim() => 2 * (_a + _b);
}

public class Trougao : Oblik
{
    private readonly double _a, _b, _c;
    public Trougao(double a, double b, double c)
    {
        if (a <= 0 || b <= 0 || c <= 0 || a + b <= c || a + c <= b || b + c <= a)
            throw new ArgumentException("Zadate stranice ne čine trougao.");
        _a = a;
        _b = b;
        _c = c;
    }
    public override string Naziv => "Trougao";
    public override double Obim() => _a + _b + _c;
    public override double Povrsina()
    {
        double s = Obim() / 2; // Heronova formula
        return Math.Sqrt(s * (s - _a) * (s - _b) * (s - _c));
    }
}

public interface IPlativ
{
    decimal IzracunajPlatu();
}

public abstract class Zaposlenik : IPlativ
{
    public string Ime { get; }
    protected decimal OsnovnaPlata { get; }

    protected Zaposlenik(string ime, decimal osnovnaPlata)
    {
        Ime = ime;
        OsnovnaPlata = osnovnaPlata;
    }

    public abstract decimal IzracunajPlatu();
    public virtual string Opis() => $"{GetType().Name} {Ime}";
}

public class Programer : Zaposlenik
{
    private readonly int _prekovremeniSati;
    public Programer(string ime, decimal osnovna, int prekovremeniSati) : base(ime, osnovna)
        => _prekovremeniSati = prekovremeniSati;

    public override decimal IzracunajPlatu() => OsnovnaPlata + _prekovremeniSati * 20m;
}

public class Menadzer : Zaposlenik
{
    private readonly List<Zaposlenik> _tim = new();
    public Menadzer(string ime, decimal osnovna) : base(ime, osnovna) { }

    public void DodajUTim(Zaposlenik z) => _tim.Add(z);
    public override decimal IzracunajPlatu() => OsnovnaPlata + 100m * _tim.Count;
    public override string Opis() => $"{base.Opis()} (tim: {_tim.Count})";
}

public class Honorarac : Zaposlenik
{
    private readonly int _sati;
    private readonly decimal _satnica;
    public Honorarac(string ime, int sati, decimal satnica) : base(ime, 0m)
    {
        _sati = sati;
        _satnica = satnica;
    }

    public override decimal IzracunajPlatu() => _sati * _satnica;
}

public class MojStek<T>
{
    private T[] _elementi = new T[4];
    private int _broj;

    public int Broj => _broj;
    public bool Prazan => _broj == 0;

    public void Push(T element)
    {
        if (_broj == _elementi.Length)
            Array.Resize(ref _elementi, _elementi.Length * 2);
        _elementi[_broj++] = element;
    }

    public T Pop()
    {
        if (Prazan) throw new InvalidOperationException("Stek je prazan.");
        T element = _elementi[--_broj];
        _elementi[_broj] = default!;
        return element;
    }

    public T Peek()
    {
        if (Prazan) throw new InvalidOperationException("Stek je prazan.");
        return _elementi[_broj - 1];
    }
}

public class Knjiga
{
    public string Naslov { get; }
    public string Autor { get; }
    public int Godina { get; }
    public bool Dostupna { get; set; } = true;

    public Knjiga(string naslov, string autor, int godina)
    {
        Naslov = naslov;
        Autor = autor;
        Godina = godina;
    }

    public override string ToString() =>
        $"{Naslov} ({Autor}, {Godina}) - {(Dostupna ? "dostupna" : "posuđena")}";
}

public class Biblioteka
{
    private readonly List<Knjiga> _knjige = new();
    private readonly Dictionary<string, string> _posudbe = new(StringComparer.OrdinalIgnoreCase); // naslov -> član

    public void Dodaj(Knjiga knjiga) => _knjige.Add(knjiga);

    private Knjiga Nadji(string naslov) =>
        _knjige.FirstOrDefault(k => k.Naslov.Equals(naslov, StringComparison.OrdinalIgnoreCase))
        ?? throw new KeyNotFoundException($"Knjiga '{naslov}' ne postoji u biblioteci.");

    public void Posudi(string naslov, string clan)
    {
        Knjiga k = Nadji(naslov);
        if (!k.Dostupna)
            throw new InvalidOperationException($"Knjiga '{k.Naslov}' je već posuđena ({_posudbe[k.Naslov]}).");
        k.Dostupna = false;
        _posudbe[k.Naslov] = clan;
    }

    public void Vrati(string naslov)
    {
        Knjiga k = Nadji(naslov);
        k.Dostupna = true;
        _posudbe.Remove(k.Naslov);
    }

    public IEnumerable<Knjiga> TraziPoAutoru(string autor) =>
        _knjige.Where(k => k.Autor.Contains(autor, StringComparison.OrdinalIgnoreCase));

    public Dictionary<string, int> BrojKnjigaPoAutoru()
    {
        var rezultat = new Dictionary<string, int>();
        foreach (Knjiga k in _knjige)
        {
            if (rezultat.ContainsKey(k.Autor)) rezultat[k.Autor]++;
            else rezultat[k.Autor] = 1;
        }
        return rezultat;
    }

    public IReadOnlyList<Knjiga> Sve => _knjige;
}

// ===================== Zadaci 7-12 =====================

public static class OOP
{
    // ZADATAK 7 — Klasa Student
    // TEKST: Napraviti klasu Student s poljima Id, Ime, Prezime, GodinaStudija, Prosjek i Smjer.
    // Prosjek mora biti od 0 do 10, a godina od 1 do 5. Klasa ima dva konstruktora i metodu za upis naredne godine.
    // OBRAZLOŽENJE: Privatna polja _godina i _prosjek zaštićena su svojstvima čiji set dio provjerava
    // vrijednost i baca ArgumentOutOfRangeException (enkapsulacija). Konstruktor bez parametara omogućava
    // object initializer, a drugi konstruktor postavlja sva svojstva odjednom. Metoda UpisiSljedecuGodinu
    // mijenja stanje objekta i štiti ga od nevalidnog prelaza.
    public static void Z07_KlasaStudent()
    {
        var s1 = new Student(1, "Amar", "Hodžić", 2, 8.4, "Informatika");
        var s2 = new Student { Id = 2, Ime = "Lejla", Prezime = "Kovač", GodinaStudija = 3, Smjer = "Informatika" };
        s2.Prosjek = 9.1;

        Console.WriteLine(s1);
        Console.WriteLine(s2);

        s1.UpisiSljedecuGodinu();
        Console.WriteLine($"Nakon upisa naredne godine: {s1}");

        try
        {
            s2.Prosjek = 11;
        }
        catch (ArgumentOutOfRangeException ex)
        {
            Console.WriteLine($"Greška: {ex.Message}");
        }
    }

    // ZADATAK 8 — Bankovni račun
    // TEKST: Napraviti klasu BankovniRacun s uplatom i isplatom. Isplata veća od stanja mora baciti vlastiti
    // izuzetak NedovoljnoSredstavaException, a nevalidan iznos ArgumentException. Čuvati historiju transakcija.
    // OBRAZLOŽENJE: Stanje je privatno i mijenja se samo metodama Uplati i Isplati, koje prvo validiraju
    // ulaz. Vlastiti izuzetak nasljeđuje Exception i nosi dodatne podatke (stanje i traženi iznos), pa pozivalac
    // može precizno reagovati. Svaka uspješna operacija upisuje se u listu historije.
    public static void Z08_BankovniRacun()
    {
        var racun = new BankovniRacun("Amar Hodžić", "BA39 1234 5678", 100m);
        racun.Uplati(500m);
        racun.Isplati(200m);

        try
        {
            racun.Isplati(1000m);
        }
        catch (NedovoljnoSredstavaException ex)
        {
            Console.WriteLine($"Greška: {ex.Message}");
        }

        try
        {
            racun.Uplati(-5m);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Greška: {ex.Message}");
        }

        Console.WriteLine($"Stanje: {racun.Stanje:F2} KM");
        foreach (string stavka in racun.Historija)
            Console.WriteLine("  " + stavka);
    }

    // ZADATAK 9 — Geometrijski oblici
    // TEKST: Napraviti apstraktnu klasu Oblik s metodama Povrsina i Obim, te klase Krug, Pravougaonik i Trougao.
    // U listi oblika ispisati svaki oblik i pronaći onaj s najvećom površinom.
    // OBRAZLOŽENJE: Apstraktna klasa definira zajednički ugovor, a izvedene klase ga implementiraju s
    // override. Lista List<Oblik> sadrži različite tipove, a poziv oblik.Povrsina() izvršava odgovarajuću
    // verziju metode u vrijeme izvršavanja (polimorfizam). Trougao koristi Heronovu formulu i validira stranice.
    public static void Z09_GeometrijskiOblici()
    {
        var oblici = new List<Oblik>
        {
            new Krug(3),
            new Pravougaonik(4, 6),
            new Trougao(3, 4, 5)
        };

        Oblik najveci = oblici[0];
        foreach (Oblik o in oblici)
        {
            Console.WriteLine(o);
            if (o.Povrsina() > najveci.Povrsina()) najveci = o;
        }
        Console.WriteLine($"Najveću površinu ima: {najveci.Naziv}");

        try
        {
            _ = new Trougao(1, 2, 10);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Greška: {ex.Message}");
        }
    }

    // ZADATAK 10 — Zaposlenici
    // TEKST: Definisati interfejs IPlativ i apstraktnu klasu Zaposlenik. Izvedene klase Programer, Menadzer i
    // Honorarac računaju platu na različite načine. Ispisati plate i ukupan trošak plata.
    // OBRAZLOŽENJE: Interfejs IPlativ opisuje samo sposobnost obračuna plate, pa ga mogu implementirati i klase
    // koje nisu zaposlenici. Svaka izvedena klasa override-uje IzracunajPlatu() po vlastitoj formuli, a Menadzer
    // još redefinira Opis(). Ukupan iznos se računa nad listom IPlativ, bez znanja o konkretnom tipu.
    public static void Z10_Zaposlenici()
    {
        var programer = new Programer("Lejla", 2500m, 10);
        var honorarac = new Honorarac("Tarik", 80, 25m);
        var menadzer = new Menadzer("Sara", 3500m);
        menadzer.DodajUTim(programer);
        menadzer.DodajUTim(honorarac);

        var zaposlenici = new List<Zaposlenik> { programer, honorarac, menadzer };
        decimal ukupno = 0m;
        foreach (Zaposlenik z in zaposlenici)
        {
            IPlativ plativ = z;
            decimal plata = plativ.IzracunajPlatu();
            ukupno += plata;
            Console.WriteLine($"{z.Opis(),-28} plata: {plata,9:F2} KM");
        }
        Console.WriteLine($"Ukupan trošak plata: {ukupno:F2} KM");
    }

    // ZADATAK 11 — Generička klasa Stack<T>
    // TEKST: Implementirati vlastiti generički stek MojStek<T> s metodama Push, Pop, Peek. Stek koristiti za
    // int, string i za provjeru ispravnosti zagrada u izrazu.
    // OBRAZLOŽENJE: Klasa je parametrizovana tipom T pa radi s bilo kojim tipom bez konverzija. Interno koristi
    // niz koji se udvostručuje kad se napuni (Array.Resize). Pop i Peek na praznom steku bacaju izuzetak.
    // Provjera zagrada je klasična primjena: otvorena zagrada ide na stek, a zatvorena mora odgovarati vrhu.
    public static void Z11_GenerickiStek()
    {
        var brojevi = new MojStek<int>();
        for (int i = 1; i <= 6; i++) brojevi.Push(i * i);
        Console.Write("Int stek (Pop): ");
        while (!brojevi.Prazan) Console.Write(brojevi.Pop() + " ");
        Console.WriteLine();

        var rijeci = new MojStek<string>();
        foreach (string r in "jedan dva tri".Split(' ')) rijeci.Push(r);
        Console.WriteLine($"String stek, vrh: {rijeci.Peek()}, broj elemenata: {rijeci.Broj}");

        try
        {
            new MojStek<double>().Pop();
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Greška: {ex.Message}");
        }

        foreach (string izraz in new[] { "(a + b) * [c - {d}]", "(a + b]", "((x)" })
            Console.WriteLine($"{izraz,-22} -> {(ZagradeUredne(izraz) ? "ispravno" : "neispravno")}");
    }

    private static bool ZagradeUredne(string izraz)
    {
        var stek = new MojStek<char>();
        foreach (char c in izraz)
        {
            switch (c)
            {
                case '(':
                case '[':
                case '{':
                    stek.Push(c);
                    break;
                case ')':
                case ']':
                case '}':
                    if (stek.Prazan) return false;
                    char otvorena = stek.Pop();
                    bool odgovara = (otvorena == '(' && c == ')') ||
                                    (otvorena == '[' && c == ']') ||
                                    (otvorena == '{' && c == '}');
                    if (!odgovara) return false;
                    break;
            }
        }
        return stek.Prazan;
    }

    // ZADATAK 12 — Biblioteka
    // TEKST: Napraviti sistem biblioteke s klasama Knjiga i Biblioteka. Omogućiti posudbu i vraćanje knjiga,
    // pretragu po autoru i brojanje knjiga po autoru.
    // OBRAZLOŽENJE: List<Knjiga> čuva fond, a Dictionary<string,string> bilježi ko je posudio koju knjigu, pa je
    // pristup po naslovu brz. Dictionary<string,int> se koristi za brojanje knjiga po autoru. Greške (nepostojeća ili
    // već posuđena knjiga) prijavljuju se izuzecima, a demonstracija ih hvata u try/catch blokovima.
    public static void Z12_Biblioteka()
    {
        var biblioteka = new Biblioteka();
        biblioteka.Dodaj(new Knjiga("Na Drini ćuprija", "Ivo Andrić", 1945));
        biblioteka.Dodaj(new Knjiga("Prokleta avlija", "Ivo Andrić", 1954));
        biblioteka.Dodaj(new Knjiga("Derviš i smrt", "Meša Selimović", 1966));
        biblioteka.Dodaj(new Knjiga("Tvrđava", "Meša Selimović", 1970));
        biblioteka.Dodaj(new Knjiga("Hasanaginica", "Narodna balada", 1774));

        biblioteka.Posudi("Derviš i smrt", "Lejla Kovač");
        Pokusaj(() => biblioteka.Posudi("Derviš i smrt", "Amar Hodžić"));
        Pokusaj(() => biblioteka.Posudi("Nepostojeća knjiga", "Amar Hodžić"));

        Console.WriteLine("Knjige autora 'Andrić':");
        foreach (Knjiga k in biblioteka.TraziPoAutoru("Andrić"))
            Console.WriteLine("  " + k);

        Console.WriteLine("Broj knjiga po autoru:");
        foreach (KeyValuePair<string, int> par in biblioteka.BrojKnjigaPoAutoru())
            Console.WriteLine($"  {par.Key}: {par.Value}");

        biblioteka.Vrati("Derviš i smrt");
        Console.WriteLine("Nakon vraćanja: " + biblioteka.Sve[2]);
    }

    private static void Pokusaj(Action akcija)
    {
        try
        {
            akcija();
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is KeyNotFoundException)
        {
            Console.WriteLine($"Greška: {ex.Message}");
        }
    }
}

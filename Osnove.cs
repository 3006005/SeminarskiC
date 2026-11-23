namespace SeminarskiC;

/// <summary>Zadaci 1-6: kontrola toka, uslovi, petlje, nizovi, stringovi, metode.</summary>
public static class Osnove
{
    // ZADATAK 1 — Kalkulator
    // TEKST: Napisati program koji učitava dva broja i operator (+, -, *, /, %), te ispisuje rezultat.
    // Dijeljenje nulom i nepoznat operator moraju biti obrađeni izuzecima.
    // OBRAZLOŽENJE: Operacija se bira switch izrazom, koji vraća vrijednost za svaki operator.
    // Za nedozvoljene slučajeve bacaju se izuzeci, a try/catch u metodi ih hvata i ispisuje poruku
    // umjesto rušenja programa. Učitavanje broja ponavlja se petljom dok unos ne bude ispravan.
    public static void Z01_Kalkulator()
    {
        double a = UcitajBroj("Prvi broj: ");
        Console.Write("Operator (+ - * / %): ");
        string op = (Console.ReadLine() ?? "").Trim();
        double b = UcitajBroj("Drugi broj: ");

        try
        {
            Console.WriteLine($"{a} {op} {b} = {Izracunaj(a, op, b)}");
        }
        catch (DivideByZeroException)
        {
            Console.WriteLine("Greška: dijeljenje nulom nije dozvoljeno.");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Greška: {ex.Message}");
        }
    }

    private static double UcitajBroj(string poruka)
    {
        while (true)
        {
            Console.Write(poruka);
            if (double.TryParse(Console.ReadLine(), out double vrijednost))
                return vrijednost;
            Console.WriteLine("Neispravan broj, pokušajte ponovo.");
        }
    }

    private static double Izracunaj(double a, string op, double b) => op switch
    {
        "+" => a + b,
        "-" => a - b,
        "*" => a * b,
        "/" => b == 0 ? throw new DivideByZeroException() : a / b,
        "%" => b == 0 ? throw new DivideByZeroException() : a % b,
        _ => throw new ArgumentException($"Nepoznat operator '{op}'.")
    };

    // ZADATAK 2 — Prosti brojevi i Fibonaccijev niz
    // TEKST: Ispisati sve proste brojeve do 50 i prvih 15 članova Fibonaccijevog niza.
    // OBRAZLOŽENJE: Za proste brojeve vanjska for petlja prolazi kroz kandidate, a metoda JeProst
    // provjerava djeljivost samo do kvadratnog korijena broja, što je znatno brže od provjere do n.
    // Fibonaccijev niz računa se iterativno: svaki član je zbir prethodna dva, pa nije potrebna rekurzija.
    public static void Z02_ProstiIFibonacci()
    {
        const int granica = 50;
        var prosti = new List<int>();
        for (int i = 2; i <= granica; i++)
            if (JeProst(i)) prosti.Add(i);
        Console.WriteLine($"Prosti brojevi do {granica}: {string.Join(", ", prosti)}");

        var fibonacci = new List<long>();
        long prethodni = 0, trenutni = 1;
        for (int i = 0; i < 15; i++)
        {
            fibonacci.Add(prethodni);
            long sljedeci = prethodni + trenutni;
            prethodni = trenutni;
            trenutni = sljedeci;
        }
        Console.WriteLine($"Prvih 15 Fibonaccijevih brojeva: {string.Join(", ", fibonacci)}");
    }

    private static bool JeProst(int n)
    {
        if (n < 2) return false;
        for (int d = 2; (long)d * d <= n; d++)
            if (n % d == 0) return false;
        return true;
    }

    // ZADATAK 3 — Palindrom i samoglasnici
    // TEKST: Za više unesenih rečenica provjeriti da li su palindrom (ignorišući razmake, interpunkciju
    // i velika/mala slova) i izbrojati koliko sadrže samoglasnika.
    // OBRAZLOŽENJE: Tekst se prvo očisti (ostaju samo slova i cifre, sve malim slovima), a zatim se
    // dva pokazivača kreću s oba kraja prema sredini i porede znakove. Samoglasnici se broje foreach
    // petljom i switch naredbom koja grupiše pet slučajeva u jednu granu.
    public static void Z03_PalindromISamoglasnici()
    {
        string[] tekstovi =
        {
            "Ana voli Milovana",
            "Programiranje",
            "A man, a plan, a canal: Panama",
            "Kazak"
        };

        foreach (string t in tekstovi)
        {
            string palindrom = JePalindrom(t) ? "DA" : "NE";
            Console.WriteLine($"\"{t}\" -> palindrom: {palindrom}, samoglasnika: {BrojSamoglasnika(t)}");
        }
    }

    private static bool JePalindrom(string tekst)
    {
        char[] slova = tekst.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray();
        int i = 0, j = slova.Length - 1;
        while (i < j)
        {
            if (slova[i] != slova[j]) return false;
            i++;
            j--;
        }
        return true;
    }

    private static int BrojSamoglasnika(string tekst)
    {
        int broj = 0;
        foreach (char c in tekst.ToLowerInvariant())
        {
            switch (c)
            {
                case 'a':
                case 'e':
                case 'i':
                case 'o':
                case 'u':
                    broj++;
                    break;
            }
        }
        return broj;
    }

    // ZADATAK 4 — Bubble sort i binarna pretraga
    // TEKST: Generisati niz od 10 slučajnih brojeva, sortirati ga bubble sortom, a zatim binarnom
    // pretragom pronaći jedan postojeći i jedan nepostojeći broj.
    // OBRAZLOŽENJE: Bubble sort poredi susjedne elemente i zamjenjuje ih; zastavica "zamijenjeno"
    // prekida algoritam čim je niz sortiran. Binarna pretraga radi samo nad sortiranim nizom: u svakom
    // koraku odbacuje polovinu prostora pretrage, pa je složenost O(log n) naspram O(n) kod linearne.
    public static void Z04_SortiranjeIPretraga()
    {
        var rnd = new Random(42);
        int[] niz = new int[10];
        for (int i = 0; i < niz.Length; i++)
            niz[i] = rnd.Next(1, 100);

        Console.WriteLine("Originalni niz: " + string.Join(" ", niz));
        BubbleSort(niz);
        Console.WriteLine("Sortirani niz:  " + string.Join(" ", niz));

        foreach (int trazeni in new[] { niz[3], 1000 })
        {
            int pozicija = BinarnaPretraga(niz, trazeni);
            Console.WriteLine(pozicija >= 0
                ? $"Broj {trazeni} pronađen na indeksu {pozicija}."
                : $"Broj {trazeni} nije u nizu.");
        }
    }

    private static void BubbleSort(int[] niz)
    {
        for (int prolaz = 0; prolaz < niz.Length - 1; prolaz++)
        {
            bool zamijenjeno = false;
            for (int j = 0; j < niz.Length - 1 - prolaz; j++)
            {
                if (niz[j] > niz[j + 1])
                {
                    (niz[j], niz[j + 1]) = (niz[j + 1], niz[j]);
                    zamijenjeno = true;
                }
            }
            if (!zamijenjeno) break;
        }
    }

    private static int BinarnaPretraga(int[] sortiran, int trazeni)
    {
        int lijevo = 0, desno = sortiran.Length - 1;
        while (lijevo <= desno)
        {
            int sredina = lijevo + (desno - lijevo) / 2;
            if (sortiran[sredina] == trazeni) return sredina;
            if (sortiran[sredina] < trazeni) lijevo = sredina + 1;
            else desno = sredina - 1;
        }
        return -1;
    }

    // ZADATAK 5 — Matrice
    // TEKST: Implementirati sabiranje i množenje matrica. Metode moraju provjeriti da su dimenzije
    // matrica odgovarajuće i u suprotnom baciti izuzetak.
    // OBRAZLOŽENJE: Matrice su dvodimenzionalni nizovi (int[,]). Sabiranje koristi dvije ugniježđene
    // petlje, a množenje tri: za element (i, j) rezultata sabiraju se proizvodi i-tog reda prve i
    // j-te kolone druge matrice. Množenje je moguće samo ako je broj kolona prve jednak broju redova druge.
    public static void Z05_Matrice()
    {
        int[,] a = { { 1, 2, 3 }, { 4, 5, 6 } };
        int[,] b = { { 7, 8 }, { 9, 10 }, { 11, 12 } };
        int[,] c = { { 1, 1, 1 }, { 2, 2, 2 } };

        Console.WriteLine("A + C:");
        Ispisi(Saberi(a, c));
        Console.WriteLine("A * B:");
        Ispisi(Pomnozi(a, b));

        try
        {
            Saberi(a, b);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Očekivana greška: {ex.Message}");
        }
    }

    private static int[,] Saberi(int[,] x, int[,] y)
    {
        if (x.GetLength(0) != y.GetLength(0) || x.GetLength(1) != y.GetLength(1))
            throw new ArgumentException("Matrice moraju imati iste dimenzije za sabiranje.");

        var rezultat = new int[x.GetLength(0), x.GetLength(1)];
        for (int i = 0; i < x.GetLength(0); i++)
            for (int j = 0; j < x.GetLength(1); j++)
                rezultat[i, j] = x[i, j] + y[i, j];
        return rezultat;
    }

    private static int[,] Pomnozi(int[,] x, int[,] y)
    {
        if (x.GetLength(1) != y.GetLength(0))
            throw new ArgumentException("Broj kolona prve matrice mora biti jednak broju redova druge.");

        var rezultat = new int[x.GetLength(0), y.GetLength(1)];
        for (int i = 0; i < x.GetLength(0); i++)
            for (int j = 0; j < y.GetLength(1); j++)
                for (int k = 0; k < x.GetLength(1); k++)
                    rezultat[i, j] += x[i, k] * y[k, j];
        return rezultat;
    }

    private static void Ispisi(int[,] m)
    {
        for (int i = 0; i < m.GetLength(0); i++)
        {
            for (int j = 0; j < m.GetLength(1); j++)
                Console.Write($"{m[i, j],5}");
            Console.WriteLine();
        }
    }

    // ZADATAK 6 — Igra "Pogodi broj"
    // TEKST: Računar zamisli broj od 1 do 100, a korisnik ima najviše 7 pokušaja da ga pogodi.
    // Nakon svakog pogrešnog pokušaja ispisuje se da li je traženi broj veći ili manji.
    // OBRAZLOŽENJE: Glavna petlja je while sa dva uslova (ima li pokušaja i da li je broj pogođen).
    // Random generiše tajni broj, a if/else daje pomoć igraču. Neispravan unos ne troši pokušaj
    // zahvaljujući naredbi continue. Sedam pokušaja je dovoljno jer binarna strategija pokriva 100 brojeva.
    public static void Z06_PogodiBroj()
    {
        var rnd = new Random();
        int tajni = rnd.Next(1, 101);
        const int maxPokusaja = 7;
        int pokusaj = 0;
        bool pogodjeno = false;

        Console.WriteLine("Zamislio sam broj od 1 do 100. Imate 7 pokušaja.");
        while (pokusaj < maxPokusaja && !pogodjeno)
        {
            Console.Write($"Pokušaj {pokusaj + 1}/{maxPokusaja}: ");
            if (!int.TryParse(Console.ReadLine(), out int unos))
            {
                Console.WriteLine("Unesite cijeli broj.");
                continue;
            }

            pokusaj++;
            if (unos == tajni)
                pogodjeno = true;
            else
                Console.WriteLine(unos < tajni ? "Traženi broj je veći." : "Traženi broj je manji.");
        }

        Console.WriteLine(pogodjeno
            ? $"Bravo! Pogodili ste u {pokusaj}. pokušaju."
            : $"Kraj igre. Traženi broj je bio {tajni}.");
    }
}

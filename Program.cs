using SeminarskiC;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var zadaci = new (string Naziv, Action Pokreni)[]
{
    ("Kalkulator (switch, izuzeci)",                    Osnove.Z01_Kalkulator),
    ("Prosti brojevi i Fibonaccijev niz (petlje)",      Osnove.Z02_ProstiIFibonacci),
    ("Palindrom i samoglasnici (stringovi)",            Osnove.Z03_PalindromISamoglasnici),
    ("Bubble sort i binarna pretraga (nizovi)",         Osnove.Z04_SortiranjeIPretraga),
    ("Matrice: sabiranje i množenje",                   Osnove.Z05_Matrice),
    ("Igra \"Pogodi broj\" (while)",                    Osnove.Z06_PogodiBroj),
    ("Klasa Student (enkapsulacija)",                   OOP.Z07_KlasaStudent),
    ("Bankovni račun (izuzeci)",                        OOP.Z08_BankovniRacun),
    ("Geometrijski oblici (apstraktna klasa)",          OOP.Z09_GeometrijskiOblici),
    ("Zaposlenici (interfejsi, override)",              OOP.Z10_Zaposlenici),
    ("Generička klasa Stack<T>",                        OOP.Z11_GenerickiStek),
    ("Biblioteka (List<T>, Dictionary)",                OOP.Z12_Biblioteka),
    ("LINQ upiti nad studentima",                       Napredno.Z13_Linq),
    ("Čitanje i pisanje fajlova",                       Napredno.Z14_Fajlovi),
    ("Delegati i događaji",                             Napredno.Z15_DelegatiIDogadjaji),
    ("Async/await",                                     Napredno.Z16_AsyncAwait),
    ("JSON serijalizacija",                             Napredno.Z17_Json),
    ("CRUD nad SQLite bazom (ADO.NET)",                 BazaZadaci.Z18_CrudSqlite),
    ("Repository pattern",                              BazaZadaci.Z19_Repository),
    ("Mini-sistem: studenti, predmeti, ocjene",         BazaZadaci.Z20_MiniSistem),
};

while (true)
{
    Console.WriteLine("\n===== 20 ZADATAKA IZ C# =====");
    for (int i = 0; i < zadaci.Length; i++)
        Console.WriteLine($"{i + 1,2}. {zadaci[i].Naziv}");
    Console.WriteLine(" 0. Izlaz");
    Console.Write("Izbor: ");

    string? unos = Console.ReadLine();
    if (unos is null) break;
    if (!int.TryParse(unos, out int izbor) || izbor < 0 || izbor > zadaci.Length)
    {
        Console.WriteLine("Neispravan izbor.");
        continue;
    }
    if (izbor == 0) break;

    Console.WriteLine($"\n--- Zadatak {izbor}: {zadaci[izbor - 1].Naziv} ---");
    try
    {
        zadaci[izbor - 1].Pokreni();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Neočekivana greška: {ex.Message}");
    }
    Console.WriteLine("\nPritisnite Enter za povratak u meni...");
    Console.ReadLine();
}

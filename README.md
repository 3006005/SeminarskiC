# 20 zadataka iz C#

Seminarski rad: konzolna aplikacija s menijem u kojoj je rješeno 20 zadataka (kontrola toka, uslovi, petlje, klase, metode, rad s fajlovima i SQLite bazom).
Tekst zadatka i obrazloženje rješenja nalaze se u komentaru iznad svake metode u izvornom kodu.

## Pokretanje

Potreban je [.NET 8 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet run
```

U meniju se bira broj zadatka (1-20). Zadaci 1 i 6 traže unos s tastature. Zadaci 14 i 17 kreiraju folder `podaci/`, a zadaci 18-20 kreiraju SQLite fajlove (`studenti.db`, `skola.db`) u radnom folderu; zadaci 18-20 pri svakom pokretanju brišu i ponovo kreiraju tabele.

## Struktura

| Fajl | Sadržaj |
|---|---|
| `Program.cs` | Meni za izbor zadatka |
| `Osnove.cs` | Zadaci 1-6 |
| `OOP.cs` | Klase i zadaci 7-12 (te klase koje koristi zadatak 15) |
| `Napredno.cs` | Zadaci 13-17 |
| `Baza.cs` | Zadaci 18-20, repository i klasa SkolskiSistem |
| `Modeli.cs` | Klasa Student i test podaci |

## Zadaci

| # | Naziv | Tekst zadatka |
|---|---|---|
| 1 | Kalkulator (switch i izuzeci) | Učitati dva broja i operator (+, -, *, /, %) i ispisati rezultat. Dijeljenje nulom i nepoznat operator obraditi izuzecima. |
| 2 | Prosti brojevi i Fibonaccijev niz | Ispisati sve proste brojeve do 50 i prvih 15 članova Fibonaccijevog niza. |
| 3 | Palindrom i samoglasnici | Za više rečenica provjeriti da li su palindrom (bez razmaka, interpunkcije i razlike velikih i malih slova) i izbrojati samoglasnike. |
| 4 | Bubble sort i binarna pretraga | Generisati niz od 10 slučajnih brojeva, sortirati ga bubble sortom i binarnom pretragom naći jedan postojeći i jedan nepostojeći broj. |
| 5 | Matrice: sabiranje i množenje | Implementirati sabiranje i množenje matrica. Ako dimenzije nisu odgovarajuće, metoda baca izuzetak. |
| 6 | Igra "Pogodi broj" | Računar zamisli broj od 1 do 100, a igrač ima 7 pokušaja. Nakon svakog promašaja ispisuje se da li je broj veći ili manji. |
| 7 | Klasa Student (enkapsulacija) | Napraviti klasu Student s dva konstruktora. Prosjek mora biti od 0 do 10, a godina studija od 1 do 5. |
| 8 | Bankovni račun i vlastiti izuzetak | Napraviti klasu BankovniRacun s uplatom i isplatom. Isplata veća od stanja baca vlastiti izuzetak, a račun čuva historiju transakcija. |
| 9 | Geometrijski oblici (polimorfizam) | Apstraktna klasa Oblik s metodama Povrsina i Obim, te klase Krug, Pravougaonik i Trougao. U listi pronaći oblik s najvećom površinom. |
| 10 | Zaposlenici (interfejsi i override) | Interfejs IPlativ i apstraktna klasa Zaposlenik. Programer, Menadzer i Honorarac računaju platu na različite načine; ispisati ukupan trošak plata. |
| 11 | Generička klasa Stack<T> | Implementirati generički stek s metodama Push, Pop i Peek i njime provjeriti ispravnost zagrada u izrazu. |
| 12 | Biblioteka (List i Dictionary) | Klase Knjiga i Biblioteka: posudba i vraćanje knjiga, pretraga po autoru i brojanje knjiga po autoru. |
| 13 | LINQ upiti nad studentima | Nad listom od 10 studenata: filtriranje, sortiranje, top 3 po prosjeku, grupisanje po smjeru te upiti Any, All i MaxBy. |
| 14 | Čitanje i pisanje fajlova | Spremiti listu studenata u CSV fajl, učitati je nazad u objekte i dopisati zapis u log. Obraditi fajl koji ne postoji. |
| 15 | Delegati i događaji (events) | Dodati bankovnom računu događaj PromjenaStanja. Pretplatiti metodu (SMS) i lambdu (upozorenje), pa se odjaviti. Prikazati i Func i Action. |
| 16 | Async/await i paralelni zadaci | Simulirati preuzimanje podataka iz tri izvora. Uporediti sekvencijalno i paralelno izvršavanje te prikazati prekid zbog isteka vremena. |
| 17 | JSON serijalizacija | Serijalizovati listu studenata u JSON fajl, deserijalizovati je i ispisati studente s prosjekom iznad 8.5. Obraditi neispravan JSON. |
| 18 | CRUD nad SQLite bazom (ADO.NET) | Povezati se na SQLite bazu, kreirati tabelu Studenti i implementirati Create, Read, Update i Delete parametrizovanim upitima. |
| 19 | Repository pattern (slojevi) | Pristup bazi organizovati kroz interfejs IStudentRepository, implementaciju za SQLite i klasu StudentService s poslovnom logikom. |
| 20 | Mini-sistem: studenti i ocjene | Baza sa tabelama Studenti, Predmeti i Ocjene. Transakcijski unos, prosjeci i izvještaji JOIN upitima; test poništavanja transakcije. |

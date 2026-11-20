# LifeManagementApp – koodiopas

Avaa projekti Visual Studiossa ja pidä tämä tiedosto sen vieressä. Aloita yhdestä toiminnosta. Kun osaat seurata yhden tehtävän matkan ruudulta tietokantaan ja takaisin, samat käsitteet toistuvat muualla.

## 1. Seuraa yhtä tallennusta alusta loppuun

| Hetki | Katso tätä kohtaa | Mitä siellä tapahtuu |
| --- | --- | --- |
| Valitset **Lisää tehtävä**. | `Views/TodoListPage.xaml` → `TodoListViewModel.AddTodoAsync` | Painikkeen `Command` on sidottu ViewModelin `AddTodoCommand`-ominaisuuteen. Komento pyytää Shelliä avaamaan lomakkeen reitillä `todo-form`. |
| Lomake avautuu. | `AppShell.xaml.cs` → `TodoFormPage.xaml.cs` | Shell tunnistaa reitin sivun tyypiksi. DI luo sivun ja antaa sille uuden ViewModelin. Sivun konstruktori asettaa sen `BindingContextiksi`. |
| Kirjoitat otsikon, valitset prioriteetin ja annat eräpäivän. | `Views/TodoFormPage.xaml` → `TodoFormViewModel` | `TwoWay`-sidonta siirtää kenttien arvot ominaisuuksiin `Title`, `Priority` ja `DueDate`. Kenttien oletukset tulevat samojen sidontojen kautta toiseen suuntaan. |
| Painat **Tallenna**. | `TodoFormViewModel.SaveAsync` | `IsNullOrWhiteSpace` tarkistaa otsikon. Virheessä asetetaan näyttöön tuleva teksti ja `return` lopettaa metodin. Hyväksytystä syötteestä rakennetaan `TodoItem`. |
| Tiedot menevät SQLiteen. | `TodoRepository.SaveAsync` → `AppDbContext` | Repository avaa tälle operaatiolle kontekstin. Uuden tehtävän `Id` on oletusarvossa 0, joten `Add` merkitsee olion lisättäväksi. `SaveChangesAsync` kirjoittaa sen tietokantaan. EF saa tietokannan antaman tunnisteen. |
| Palaat tehtävälistaan. | `TodoFormViewModel.SaveAsync` → `TodoListPage.OnAppearing` | Reitti `..` poistaa lomakesivun navigointipinosta. Listasivun `OnAppearing` käynnistää haun. Uusi tehtävä lisätään havaittavaan kokoelmaan, jolloin lista päivittyy. |

Voit seurata tätä debuggerissa asettamalla F9:llä pysäytyskohdat `SaveAsync`-metodin alkuun sekä ViewModelissa että repositoryssa. F5 jatkaa seuraavaan pysäytyskohtaan. Katso Locals-ikkunasta `Title`, `todoId` ja `todo`. Kokeile sama uudelleen käyttämällä **Muokkaa valittua**. Siinä `todoId` on jo olemassa oleva positiivinen tunniste.

## 2. Projektin käynnistys ja tiedostot

- **`LifeManagementApp.csproj`** kertoo kääntäjälle kohteen ja riippuvuudet. `net9.0-windows10.0.19041.0` valitsee .NET 9:n ja Windows-kohteen. `UseMaui` ottaa käyttöön MAUI:n build-vaiheet, esimerkiksi XAML:n käsittelyn. `WindowsPackageType=None` tekee Windowsista ajettavan ilman MSIX-paketointia. Kolme `PackageReference`-riviä tuovat MAUI-kontrollit, ViewModelien Toolkitin ja EF Coren SQLite-ajurin. `Nullable` auttaa kääntäjää huomaamaan, missä viittaus voi olla `null`.
- **`global.json`** ohjaa .NET CLI:n valitsemaan .NET 9:n SDK:n. `latestFeature` sallii uudemman 9.0-sarjan SDK:n asentamisen. **`.gitignore`** jättää käännöstiedostot ja paikallisen tietokannan pois Gitistä. **`Properties/launchSettings.json`** antaa Visual Studiolle Windows Machine -käynnistysprofiilin.
- **`Platforms/Windows/App.xaml` ja `App.xaml.cs`** ovat Windowsin WinUI-käynnistyspiste. `InitializeComponent()` alustaa tämän XAML:n. `CreateMauiApp()` kutsuu yhteistä `MauiProgramia`. **`app.manifest`** kertoo Windowsille muun muassa ikkunan DPI-skaalauksesta.
- **Projektin juuressa oleva `App.xaml`** määrittelee kaksi nimettyä tyyliä ja converterin. `StaticResource Heading` hakee samasta sanakirjasta otsikkotyylin. `AppThemeBinding` antaa virhetekstille eri värin Windowsin vaaleassa ja tummassa teemassa. **`App.xaml.cs`** luo ikkunan, jonka juurena on `AppShell`.
- **`AppShell.xaml`** kuvaa kaksi välilehteä. `ShellContent` yhdistää välilehden nimen, reitin ja sivutyypin. `ContentTemplate` mahdollistaa sivun luonnin, kun välilehteä tarvitaan. **`AppShell.xaml.cs`** rekisteröi lomakkeen erilliseksi reitiksi, joten se avautuu listan päälle ja sieltä voi palata takaisin.

### Mitä MauiProgram tekee?

`MauiApp.CreateBuilder()` luo sovelluksen rakentajan. `UseMauiApp<App>()` ilmoittaa, minkä MAUI-sovellusluokan se käynnistää. `Path.Combine` muodostaa tiedostopolun käyttäjän sovellusdatakansioon. `UseSqlite` kertoo EF Corelle, että tämä tiedosto on tietokanta.

`AddDbContextFactory<AppDbContext>` rekisteröi tehtaan, joka valmistaa uuden kontekstin jokaista operaatiota varten. `AddSingleton<ITodoRepository, TodoRepository>()` tarkoittaa, että kun luokan konstruktori pyytää `ITodoRepositorya`, DI antaa sille yhteisen `TodoRepositoryn`. Sääpalvelu käyttää samaa mallia. `HttpClient` ja sen 10 sekunnin aikakatkaisu luodaan kerran.

Etusivu, tehtävälista ja niiden ViewModelit ovat sovelluskohtaisia `Singleton`-olioita. Esimerkiksi listan viimeksi valittu lajittelu säilyy välilehteä vaihdettaessa. Lomake ja sen ViewModel ovat `Transient`, joten uuden lomakkeen avaus saa uuden olion ilman edellisen tehtävän tietoja. `Build()` viimeistelee sovelluksen. Tarkista konstruktoreista itse, mitkä riippuvuudet kukin luokka pyytää.

## 3. Malli ja SQLite

### Models

`TodoItem.cs` sisältää tehtävän viisi tallennettavaa ominaisuutta. `Id` on avain, `Title` on otsikko, `Priority` on luku 1–5, `DueDate` on eräpäivä ja `IsDone` kertoo valmistumisesta. `DateTime` voi sisältää kellonajan. Tässä sovelluksessa `DueDate.Date` ottaa siitä päivän ja asettaa kellonajaksi keskiyön.

`WeatherResponse.cs` kuvaa JSON-vastauksen juuren. Sen `Current` sisältää `WeatherInfo.cs`-tiedoston mallin, joka kuvaa sisäkkäisen `current`-olion. `[JsonPropertyName("wind_speed_10m")]` yhdistää JSON-nimen C#-ominaisuuteen `WindSpeed`. `double?` tarkoittaa, että arvo voi olla myös `null`. Näin puuttuva tuuli erotetaan oikeasta arvosta 0 m/s.

### AppDbContext ja ITodoRepository

`DbContext` on EF Coren olio, joka muodostaa kyselyitä ja seuraa yhden operaation muutoksia. `DbSet<TodoItem> Todos` antaa pääsyn tehtäväriveihin. EF päättelee luokan ominaisuuksista sarakkeet ja `Id`-nimestä pääavaimen. Tätä kutsutaan Code Firstiksi, koska malliluokka määrää tietokannan rakenteen.

`ITodoRepository` listaa tehtävätoiminnot, joita ViewModel saa käyttää. Esimerkiksi `Task<TodoItem?>` tarkoittaa, että haun voi odottaa `await`-sanalla ja tuloksena voi olla yksi tehtävä tai `null`. ViewModel saa jo haetun tehtävän käsiteltäväksi. Repository tuntee EF Coren.

### TodoRepository, metodi kerrallaan

| Koodikohta | Tehtävä ja perustelu |
| --- | --- |
| `CreateDbContextAsync()` | Luodaan konteksti yhtä operaatiota varten. `await using` vapauttaa sen metodin lopussa. Eri operaatiot eivät jaa samaa muutosten seurantaa. |
| `EnsureDatabaseAsync()` | `WaitAsync` ottaa lukon. Ensimmäinen kutsuja luo kannan tarvittaessa `EnsureCreatedAsync`-metodilla. Se asettaa `databaseReady`-lipun vasta onnistuttuaan. `finally` vapauttaa lukon myös virheessä. Näin listan ja etusivun yhtäaikaiset ensimmäiset haut odottavat samaa alustusta. |
| `GetTopTodosAsync()` | `Where(todo => !todo.IsDone)` rajaa mukaan keskeneräiset rivit. `OrderByPriority` järjestää ne ja `Take(3)` poimii kolme ensimmäistä. `ToListAsync` pyytää EF Corea suorittamaan kyselyn ja palauttaa listan. |
| `OrderByPriority()` | `OrderByDescending` tuo suuret prioriteetit ensin. `ThenBy(DueDate)` ratkaisee tasapelin aikaisimmalla eräpäivällä. Jos päivätkin ovat samat, `ThenBy(Id)` antaa pysyvän järjestyksen. |
| `GetAllAsync()` | Käyttää samaa prioriteettijärjestystä tai lajittelee ensiksi eräpäivän mukaan. Päivälajittelussa samalla päivällä suurempi prioriteetti tulee ensin. Haku sisältää kaikki tehtävät. |
| `FindAsync()` | Hakee lomakkeelle yksittäisen tehtävän ID:n perusteella. Palauttaa `null`, jos rivi puuttuu. `AsNoTracking` riittää näihin lukuhakuihin, koska olioiden muutoksia ei tallenneta siinä kontekstissa. |
| `SaveAsync()` | ID 0 tarkoittaa uutta tehtävää. Olemassa oleva haetaan seurantaan kontekstin omalla `FindAsync`-kutsulla. Muokattavat ominaisuudet kopioidaan seurattuun olioon. `SaveChangesAsync` toteuttaa lisäyksen tai päivityksen. Paluuarvo `false` kertoo, ettei päivitettävää riviä löytynyt. |

`IQueryable` kuvaa tässä hakua, jota EF Core rakentaa. Sen jälkeen tulevat `Where`, `ThenBy` ja `Take` täydentävät hakua. Siirtyminen varsinaiseen listaan tapahtuu `ToListAsync`-kutsussa. `await` antaa kutsujalle mahdollisuuden jatkaa muuta käsittelyä, jos odotettu tehtävä on vielä kesken. SQLite-ajuri voi ajaa tietokantatyön synkronisesti myös async-rajapinnan takana. Verkkohaku puolestaan odottaa vastauksen valmistumista asynkronisesti.

`EnsureCreatedAsync` luo tyhjän tietokannan ensimmäisellä kerralla. Jos lisäät myöhemmin malliin sarakkeen, vanha tiedosto säilyttää vanhan rakenteensa. Käytä esittelyharjoituksessa uutta tietokantatiedostoa kuten `todos-demo.db`, tai tee tietokantamigraatio, jos vanhat tiedot täytyy siirtää uuteen rakenteeseen.

## 4. Sää ja etusivu

`IWeatherService` määrittelee yhden haun. `OpenMeteoWeatherService` saa konstruktorissa `HttpClientin`. URL pyytää Vaasan koordinaateilla lämpötilaa kahden metrin korkeudelta ja tuulta kymmenen metrin korkeudelta. `wind_speed_unit=ms` pyytää nopeuden yksikössä m/s.

`GetFromJsonAsync<WeatherResponse>` lähettää GET-pyynnön, tarkistaa HTTP-vastauksen ja muuntaa JSONin malliksi. `response?.Current` palauttaa sisemmän olion, jos juurivastaus on olemassa. Jos mittaus puuttuu, metodi ilmoittaa siitä `InvalidDataExceptionilla`.

`HomeViewModel.LoadAsync` käynnistää tehtävähaun ja säähaun ja odottaa molempia `Task.WhenAll`-komennolla. Kumpikin metodi päivittää omat kenttänsä heti valmistuttuaan. Näin tehtävät voivat näkyä jo säähakua odotettaessa.

1. `LoadTopTodosAsync` hakee keskeneräisten kärjen. `Clear` tyhjentää aiemmat rivit ja `foreach` lisää uudet. Mahdollinen SQLite-virhe näkyy `TodoError`-kentässä.
2. `LoadWeatherAsync` näyttää latausmerkin ja tyhjentää edellisen säätilan. Onnistuttuaan se muotoilee luvut yhden desimaalin tarkkuudella ja vertaa tuulen alkuperäistä arvoa rajaan.
3. `weather.WindSpeed > KitesurfWindLimit` tarkoittaa, että 5,1 antaa ilmoituksen ja täsmälleen 5,0 ei anna. `ShowKitesurfNotice` on sidottu ilmoituksen `IsVisible`-ominaisuuteen.
4. `catch`-haarat käsittelevät aikakatkaisun, verkkovirheen ja lukukelvottoman vastauksen. `finally` piilottaa latausmerkin jokaisessa lopputuloksessa.

`HomePage.xaml` käyttää pientä `BindableLayoutia` näyttämään enintään kolme riviä samassa vieritettävässä näkymässä. `ItemTemplate` määrittelee yhden rivin. `EmptyView` tulee näkyviin, kun listassa ei ole rivejä.

## 5. Lista, navigointi ja lomake

**`TodoListViewModel`**

- `LoadAsync(string? order)` vastaanottaa painikkeelta joko `"date"` tai `"priority"`. Kun sivu avautuu, code-behind kutsuu samaa komentoa parametrilla `null`. Silloin `sortByDueDate` jätetään entiseen arvoon. Yksi komento vastaa kaikista listahauista. Toolkitin asynkroninen komento estää normaalit uudet painallukset, kun haku on käynnissä.
- `SelectedTodo` päivittyy, kun valitset `CollectionView`-rivin. `CanEditTodo` palauttaa `true`, kun rivi on valittu. `NotifyCanExecuteChangedFor` käskee painiketta tarkistamaan tämän uudestaan valinnan muuttuessa.
- `AddTodoAsync` avaa lomakereitin ilman ID:tä. `EditTodoAsync` avaa saman reitin ja lisää `ShellNavigationQueryParameters`-sanakirjaan avaimen `"id"` ja valitun tehtävän tunnisteen.

**`TodoFormViewModel`**

- `ApplyQueryAttributes` kuuluu `IQueryAttributable`-rajapintaan. Shell kutsuu sitä ja välittää reitin parametrit. `TryGetValue` tarkistaa, onko `"id"` mukana, ja `value is int id` ottaa tunnisteen käyttöön, jos arvo on kokonaisluku.
- `LoadAsync` sallii tallennuksen heti lisäystilassa. Muokkaustilassa se hakee tehtävän ja kopioi sen kentät lomakkeen ominaisuuksiin. `IsReady` avaa tallennuspainikkeen, kun käytössä on uusi lomake tai onnistuneesti haettu tehtävä.
- `SaveAsync` tarkistaa otsikon, poistaa sen reunoilta välilyönnit `Trim()`-kutsulla ja rakentaa tallennettavan olion. Epäonnistunut tallennus näyttää virheen samalla sivulla, jotta syöte säilyy. Onnistunut tallennus lukitsee painikkeen ja palaa listaan.

**Näkymät `TodoListPage.xaml` ja `TodoFormPage.xaml`**

Listan `Grid` käyttää ensimmäiseen riviin hallintapainikkeiden tarvitseman tilan (`Auto`). Toinen rivi (`*`) antaa listalle jäljellä olevan tilan. `CollectionView.ItemsSource` lukee `Todos`-kokoelman. `DataTemplate` näyttää jokaisen rivin otsikon, prioriteetin, päivämäärän ja tilan. `DoneTextConverter` muuttaa `bool`-arvon tekstiksi **Tehty** tai **Kesken**. Se on käytössä `OneWay`-sidonnassa, joten vastakkaisen suunnan `ConvertBack` on tarkoituksella toteuttamatta.

Lomakkeen `Entry` ohjaa otsikkoa, `Picker` valitsee prioriteetin, `DatePicker` eräpäivän ja `CheckBox` tehdyn tilan. Jokaisella on kaksisuuntainen sidonta vastaavaan ViewModel-ominaisuuteen. `StringFormat` näyttää listan päivämäärän suomenkieliseen käyttöön sopivassa muodossa, vaikka mallissa päivämäärä säilyy `DateTime`-arvona.

## 6. Mitä Toolkit ja XAML generoivat?

- **`partial`** tarkoittaa, että luokan määritelmä voi jatkua generoidussa tiedostossa. XAML-generointi tuottaa näkymälle `InitializeComponent`-metodin. Toolkit käyttää samaa ideaa ViewModeleissa.
- **`ObservableObject`** tarjoaa ominaisuusmuutoksista ilmoittavan mekanismin. Kun kirjoitat `[ObservableProperty] private string title = "";`, Toolkit luo julkisen **`Title`**-ominaisuuden. Kun käytät `Title = "Raportti";`, sen setteri päivittää kentän ja ilmoittaa sidonnalle. Siksi ViewModel käyttää generoituja ominaisuuksia, kun näkymän pitää reagoida muutokseen.
- **`[RelayCommand]`** muodostaa esimerkiksi metodista `SaveAsync` ominaisuuden **`SaveCommand`**. `Button.Command` voi silloin käynnistää metodin XAML-sidonnan kautta. `CanExecute` ilmaisee, voiko komennon ajaa juuri nyt.
- **`ObservableCollection<TodoItem>`** ilmoittaa listalle, kun kokoelmasta poistetaan rivejä tai siihen lisätään niitä. `List<TodoItem>` kelpaa repositoryn hakutulokseksi. Kokoelma ja yksittäisen tehtävän ominaisuudet ovat eri asioita. Muokkauksen jälkeen listan uusi haku tuo päivitetyn tehtäväolion näkyville.
- **`BindingContext`** on se olio, josta XAML lukee ominaisuuksien arvot. **`x:DataType`** kertoo kääntäjälle sen odotetun tyypin. Sivun tasolla se on ViewModel. `DataTemplatessa` se on yksittäinen `TodoItem`. `x:DataType` auttaa löytämään esimerkiksi väärin kirjoitetun ominaisuuden nimen jo käännettäessä.
- **`*.xaml.cs`** asettaa ViewModelin `BindingContextiksi` konstruktorissa. `OnAppearing` kuuntelee sivun elinkaaritapahtumaa ja käynnistää ViewModelin haun, jos komento on käytettävissä. Itse haku, tietokantakysely ja validointi pysyvät omissa luokissaan.

## 7. Harjoittele esittelyn pientä koodimuutosta

1. **Top 2.** Vaihda repositoryn `Take(3)` arvoon `Take(2)`. Päivitä myös etusivun otsikko XAML:ssa. Tarkista tehtävillä, että järjestys toimii edelleen.
2. **Tuuliraja.** Vaihda `KitesurfWindLimit` hetkeksi nykyistä mitattua tuulta pienemmäksi. Käynnistä sovellus uudelleen ja tarkista ilmoitus. Palauta lopuksi 5.
3. **Näytä tunniste listassa.** Lisää `TodoListPage.xaml`-tiedoston `DataTemplateen` rivi `<Label Text="{Binding Id, StringFormat='Tunniste {0}'}" />`. Selitä, miksi sidonta löytää mallin `Id`-kentän.
4. **Lisää muistiinpano**, jos haluat harjoitella muutosta useassa kerroksessa. Lisää `Notes` tehtävämalliin, lomakkeen ViewModeliin ja lomakkeen XAML:iin. Kopioi se molempiin suuntiin lomakkeen latauksessa ja tallennuksessa sekä repositoryn päivityshaarassa. Näytä se listan mallipohjassa. Käytä harjoitellessa uutta DB-tiedostonimeä ja tarkista myös päivitys käynnistysten välillä.

Harjoittele vastaamaan ääneen näihin kysymyksiin.

- Miksi etusivu käyttää `ThenBy`-kutsua eräpäivälle? Mitä toinen `OrderBy` tekisi?
- Mitä tapahtuu, jos tyhjä otsikko syötetään? Missä kohdassa metodi päättyy?
- Miten Shell saa tietää, avataanko vanha tehtävä vai uusi?
- Miksi repository voi olla yhteinen, vaikka `AppDbContext` on operaatiokohtainen?
- Missä kohdassa JSONin nimi muuttuu C#-ominaisuudeksi? Mistä tuulen yksikkö tulee?
- Kuka ilmoittaa ruudulle, kun tehtävälistan sisältö vaihtuu? Kuka ilmoittaa, kun virheteksti muuttuu?

## 8. Laajuus ja Git

Projektissa on kuvissa määritellyt perustoiminnot, Shell-parametrin välitys muokkauslomakkeelle, yhden taulun SQLite-tallennus ja compiled bindingit. Muokkaus ja valmiiksi merkitseminen käyttävät samaa lomaketta. Yhden taulun rakenne on tietoinen rajaus, joten arvioinnin relaatiota edellyttävä ylin tallennustaso jää tämän version ulkopuolelle.

Tee Git-committeja omista työvaiheistasi sitä mukaa, kun kokeilet ja ymmärrät koodin. Kuvien esimerkin mukaisessa palautuksessa tarvitaan oma VAMK Git- tai GitHub-repo, palautettava linkki ja opettajan pääsy repoon. Tarkista myös mahdollinen kurssin P7-README-pohja. Mukana oleva README käyttää kuvissa näkyviä sisältövaatimuksia.

## Dokumentaatio

- [Microsoft Learn – .NET MAUI 9](https://learn.microsoft.com/dotnet/maui/whats-new/dotnet-9?view=net-maui-9.0)
- [Microsoft Learn – Shell-navigointi](https://learn.microsoft.com/dotnet/maui/fundamentals/shell/navigation?view=net-maui-9.0)
- [Open-Meteo – Weather Forecast API](https://open-meteo.com/en/docs)

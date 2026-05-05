# DocSystem Domain Leiras

Ez a dokumentum laikusoknak keszult. A celja, hogy akkor is ertheto kepet adjon a rendszerrol, ha valaki nem fejleszto, nem ismeri a .NET kodot, es egyszeruen csak azt szeretne megerteni, hogyan mukodik az iratkezeles es az iktatas ebben az alkalmazasban.

## Mi ez a rendszer

A DocSystem egy iratkezelo rendszer. Arra valo, hogy a szervezetben keletkezo ugyeket es dokumentumokat rendezett, visszakeresheto modon nyilvantartsa.

Rovid, hetkoznapi megfogalmazasban:

- vannak **iktatokonyvek**
- azokban vannak **iktatoszamok**
- egy ugyet egy **ugyirat** fog ossze
- az ugyirathoz tartoznak az egyes **iratok**

Ha valaki uj ugyet indit, azt a rendszer iktatja.
Ha egy meglevo ugyhoz uj dokumentum erkezik, azt is iktatja, csak mas modon.

## A legfontosabb fogalmak egyszeruen

### Iktatokonyv

Az iktatokonyv olyan, mint egy gyujto vagy "mappa-rendszer" egy adott evre es egy adott rovid kodra.

Peldak:

- `AI20262`
- `TIK2026`

Egy iktatokonyvnek tipikusan van:

- neve
- kodja
- evszama

Es ebben tarolodnak az oda tartozo iktatasok.

Gyakorlatban ugy lehet ra gondolni, mint egy adott ev adott nyilvantartasi "konyvere".

### Iktatoszam

Az iktatoszam az a hivatalos azonosito, amit a rendszer egy ugyhoz vagy irathoz rendel.

Ez az a szam, amire a felhasznalok a legtobbszor hivatkoznak, peldaul:

```text
0003/1-AI20262/2026
```

Ez az azonosito reszekbol all:

- `0003` -> a foszam
- `1` -> az alszam
- `AI20262` -> az iktatokonyv kodja
- `2026` -> az ev

Ez azert fontos, mert a felhasznalo sokszor nem rendszerbeli `id`-t mond, hanem ezt a szoveges iktatoszamot.

### Ugyirat

Az ugyirat egy teljes ugyet fog ossze.

Ugy kepzeld el, mint egy "fo ugy-dosszie":

- ez kepviseli magat az ugyet
- ehhez tartozik egy fo iktatoszam
- ez alatt jelennek meg a kapcsolodo dokumentumok

Ha valaki uj ugyet indit, jellemzoen egy uj ugyirat jon letre.

### Irat

Az irat egy konkret dokumentum vagy bejegyzes az ugyon belul.

Ilyen lehet peldaul:

- egy beerkezo level
- egy rogzitett kerelem
- egy ujabb kapcsolodo dokumentum

Fontos:

- egy ugyirathoz tobb irat is tartozhat
- amikor azt kerdezzuk, hogy "hany irat tartozik egy ugyirathoz", akkor valojaban azt nezzuk, hany elem van az ugyirat ala tartozo iratlistaban

## Hogyan kapcsolodnak egymashoz

Rovid verzio:

- egy **iktatokonyvben** sok iktatoszam lehet
- egy **ugyiratnak** van egy fo iktatoszama
- egy **ugyirathoz** tobb irat tartozhat
- minden **irat** egy meglevo ugyirathoz kapcsolodik

Ezt legegyszerubben igy lehet elkepzelni:

1. van egy iktatokonyv
2. abban nyilik egy uj ugy
3. az uj ugy kap egy foszamos iktatoszamot
4. letrejon az ugyirat
5. kesobb ugyanahhoz az ugyhoz tovabbi iratok erkezhetnek
6. ezek mar alszamos iktatassal kerulnek be

## Mi a kulonbseg a főszámos es alszámos iktatas kozott

Ez a rendszer egyik legfontosabb szabalyhalmaza.

### Főszámos iktatas

Ez akkor kell, amikor **uj ugy indul**.

Ilyenkor a rendszer:

- uj foszamot ad
- letrehoz egy uj ugyiratot
- letrehozza az ugy elso iratat is

Egyszeruen fogalmazva:

**a foszamos iktatas megnyit egy uj ugyet**

Peldakepp:

- `0003/1-AI20262/2026`

Itt a `0003` a foszam, az `1` pedig az elso bejegyzes ugyanebben az ugyben.

### Alszámos iktatas

Ez akkor kell, amikor **mar van egy meglevo ugy**, es ahhoz ujabb dokumentumot akarunk rogziteni.

Ilyenkor a rendszer:

- nem nyit uj ugyet
- a meglevo ugyhoz tesz hozza uj iratot
- ugyanaz marad a foszam
- csak az alszam no

Pelda:

- `0003/2-AI20262/2026`
- `0003/3-AI20262/2026`

Itt latszik, hogy ugyanaz a fo ugy, mert a foszam ugyanaz: `0003`.
Az elteres az, hogy mas-mas alszamu iratok tartoznak ala.

## Hogyan erdemes erteni a foszamot es az alszamot

Egyszeru szabaly:

- **foszamos ugy-azonosito**: ugyanazon ugy elso, fo bejegyzese
- **alszamos bejegyzes**: ugyanahhoz az ugyhoz tartozo tovabbi irat

Ebben a rendszerben hasznos gyakorlati ertelmezes:

- ha `alszam = 1`, az jellemzoen a foszamos alapbejegyzes
- ha `alszam >= 2`, az jellemzoen alszamos tovabbi irat

Ezert:

- ha valaki **főszámos iktatószámot** ker, akkor tipikusan az `1`-es alszamu fo bejegyzesre gondol
- ha valaki **alszámos iktatószámokat** ker, akkor a `2` vagy nagyobb alszamu rekordokra gondol

## Mit jelent ez egy valos ugyben

Pelda:

1. Letrejon egy uj ugy
2. Kap egy ilyen iktatoszamot:

```text
0003/1-AI20262/2026
```

3. Kesőbb ugyanahhoz az ugyhoz beerkezik meg ket dokumentum
4. Ezek ilyen szamokat kapnak:

```text
0003/2-AI20262/2026
0003/3-AI20262/2026
```

Ebbol mar latszik:

- ugyanaz az ugy, mert a foszam mindharomnal `0003`
- harom kulon bejegyzes van az ugy alatt
- az ugyirathoz osszesen 3 irat tartozik

## Hogyan talalja meg a rendszer a dolgokat

Ez azert fontos, mert a felhasznalo maskepp beszel, mint a rendszer.

### Amikor a user iktatokonyvre hivatkozik

A user altalaban nem belso rendszerazonositot mond, hanem:

- kodot
- nevet

Pelda:

- "az AI20262 kodu iktatokonyv"

Ilyenkor a rendszert ugy kell ertelmezni, hogy meg kell keresni azt az iktatokonyvet, amelynek `kod` mezője `AI20262`.

### Amikor a user egy ugyiratra hivatkozik

Ilyenkor gyakran nem azt mondja, hogy "ennek az ugyiratnak az id-ja...", hanem ezt:

```text
0003/1-AI20262/2026
```

Ez **nem** iktatokonyv-kod, hanem **szoveges iktatoszam**.

Tehat ilyen esetben nem `kod` alapjan kell keresni, hanem a szoveges iktatoszam alapjan.

Ez nagyon fontos kulonbseg:

- `AI20262` -> inkabb iktatokonyv-kod
- `0003/1-AI20262/2026` -> inkabb ugy- vagy iratazonosito, szoveges iktatoszam

## Mit jelent az, hogy nincs kulon vegpont, de az adat megis elerheto

Ez a rendszerben gyakori helyzet.

Pelda:

- nincs kulon "listazd ki az osszes iktatast" vegpont
- de egy `GET /api/Iktatokonyv` valaszban az iktatoszamok mar ott lehetnek beagyazva

Ez laikusan annyit jelent:

**nem mindig kell kulon helyen keresni valamit, mert lehet, hogy mar benne van egy masik valaszban**

Ezert ha valaki azt kerdezi:

- "milyen iktatoszamok vannak ebben az iktatokonyvben?"

akkor nem biztos, hogy uj adatot kell letrehozni vagy kulon keresot kell keresni. Lehet, hogy eleg a megfelelo iktatokonyv adatait megnezni.

## Tipikus kerdesek es azok ertelmezese

### "Listazd ki az elerheto iktatokonyvek kodjait"

Ez azt jelenti:

- az osszes iktatokonyv kozul a `kod` mezoket kell osszegyujteni

Nem ezt jelenti:

- nem az iktatoszamokat kell visszaadni

### "Add vissza a AI20262 kodu iktatokonyvben levo iktatoszamokat"

Ez azt jelenti:

- keresd meg a `kod = AI20262` iktatokonyvet
- annak a beagyazott iktatoszamait listazd

### "Iktass alszamosan ez ala az ugyirat ala: 0003/1-AI20262/2026"

Ez azt jelenti:

- meg kell keresni azt az ugyiratot, amelynek a fo iktatoszama `0003/1-AI20262/2026`
- ehhez kell uj iratot rogziteni

### "Hany irat tartozik a 0003/1-AI20262/2026 iktatoszamu ugyirat ala?"

Ez azt jelenti:

- meg kell keresni a megfelelo ugyiratot a szoveges iktatoszama alapjan
- meg kell nezni, hany irat van az ugyirat `irats` listajaban

Ez nem pusztan egy "szamold meg a hasonlo szamu rekordokat" feladat, hanem az ugyirat ala kapcsolt iratok szamat kell nezni.

## Mit erdemes megjegyezni roviden

Ha valaki csak 6 dolgot akar megjegyezni a rendszerrol, ezek legyenek azok:

1. Az **iktatokonyv** egy adott evhez es kodhoz tartozo gyujtoegyseg.
2. Az **ugyirat** maga az ugy.
3. Az **irat** az ugyhoz tartozo konkret dokumentum.
4. A **foszamos iktatas** uj ugyet nyit.
5. Az **alszamos iktatas** meglevo ugyet bovit.
6. A `0003/1-AI20262/2026` tipusu ertek nem iktatokonyv-kod, hanem szoveges iktatoszam.

## Mibol keszult ez a leiras

Ez a leiras a backend valos kodja alapjan keszult. A fobb forrasfajlok:

- [IktatasService.cs](/Users/iamendor/Documents/Programming/DocSystem/DocSystem/Services/IktatasService.cs:1)
- [IktatokonyvService.cs](/Users/iamendor/Documents/Programming/DocSystem/DocSystem/Services/IktatokonyvService.cs:1)
- [UgyiratService.cs](/Users/iamendor/Documents/Programming/DocSystem/DocSystem/Services/UgyiratService.cs:1)
- [IratService.cs](/Users/iamendor/Documents/Programming/DocSystem/DocSystem/Services/IratService.cs:1)
- [ContractMapper.cs](/Users/iamendor/Documents/Programming/DocSystem/DocSystem/Controllers/ContractMapper.cs:1)
- [IktatoszamFormatter.cs](/Users/iamendor/Documents/Programming/DocSystem/DocSystem/Services/IktatoszamFormatter.cs:1)

Ez azt jelenti, hogy a szoveg nem altalanos iratkezelesi tankonyv, hanem kifejezetten ennek a rendszernek a mukodeset magyarazza.

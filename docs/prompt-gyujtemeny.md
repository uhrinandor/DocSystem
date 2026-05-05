# Prompt Gyujtemeny

Ez a gyujtemeny olyan pelda promptokat tartalmaz, amelyeket egy ugyintezo a rendszerben termeszetes nyelvu utasitaskent adhat meg az agentnek.

A promptok celja:

- uj ugy nyitasa
- meglevo ugy bovitese
- iktatokonyvek lekerdezese
- ugyiratok es iratok visszakeresese
- egyszerubb adminisztracios feladatok tamogatasa

## Hogyan erdemes promptot irni

Minel jobb eredmenyt ad az agent, ha a prompt tartalmazza:

- mit kell csinalni
- melyik iktatokonyvben vagy melyik ugyirat alatt
- mi legyen a targy
- mi legyen a `Details`
- mit szeretnel visszakapni eredmenykent

Jo altalanos minta:

```text
[Művelet] + [melyik iktatókönyvben / melyik ügyirat alatt] + [tárgy] + [Details] + [milyen eredményt kérsz vissza]
```

## 1. Uj ugy nyitasa

```text
Indíts egy új ügyet a AI20262 kódú iktatókönyvben "Lakossági bejelentés" tárggyal, üres Details mezővel.
```

```text
Nyiss új ügyet a TIK2026 kódú iktatókönyvben "Belső adminisztráció" tárggyal, a Details maradjon üres.
```

```text
Hozz létre egy új főszámos ügyet a legutoljára létrehozott iktatókönyvben "Szerződés módosítás" tárggyal, üres Details mezővel.
```

```text
Indíts új ügyet az AI20262 kódú iktatókönyvben "Panaszbejelentés" tárggyal, és add vissza a létrejött iktatószámot.
```

## 2. Meglevo ugy bovitese alszamos iktatassal

```text
Iktass alszámosan ez alá az ügyirat alá: 0003/1-AI20262/2026, "Hiánypótlás érkezett" tárggyal, üres Details mezővel.
```

```text
Hozz létre új alszámos iratot a 0001/1-TIK2026/2026 iktatószámú ügyirat alatt "Telefonos egyeztetés" tárggyal, a Details legyen üres.
```

```text
A 0004/1-AI20262/2026 ügyirathoz rögzíts egy új iratot "További melléklet" tárggyal, üres megjegyzéssel.
```

```text
Iktass alszámosan a 0003/1-AI20262/2026 ügyirat alá "Beérkezett válasz" tárggyal, üres Details mezővel, és add vissza az új iktatószámot.
```

## 3. Iktatokonyvek lekerdezese

```text
Listázd ki az elérhető iktatókönyvek kódjait.
```

```text
Add vissza az elérhető iktatókönyvek nevét és kódját.
```

```text
Keresd meg az AI20262 kódú iktatókönyvet, és add vissza az azonosítóját, nevét és évszámát.
```

```text
Add vissza a legutoljára létrehozott iktatókönyv adatait.
```

```text
Melyik a legutóbb létrehozott iktatókönyv? A teljes alapadatait add vissza.
```

## 4. Iktatoszamok lekerdezese

```text
Add vissza az AI20262 kódú iktatókönyvben található összes iktatószámot.
```

```text
Add vissza a TIK2026 kódú iktatókönyvben található főszámos iktatószámokat.
```

```text
Add vissza az AI20262 kódú iktatókönyvben található alszámos iktatószámokat.
```

```text
A legutoljára létrehozott iktatókönyvben listázd ki az összes iktatószámot.
```

```text
Listázd ki a AI20262 kódú iktatókönyv főszámos és alszámos iktatószámait külön.
```

## 5. Ugyiratokkal kapcsolatos kerdesek

```text
Listázd ki az összes ügyirat iktatószámát az AI20262 kódú iktatókönyvben.
```

```text
Keresd meg a 0002/1-TIK2026/2026 iktatószámú ügyiratot, és add vissza az adatait.
```

```text
Hány irat tartozik a 0003/1-AI20262/2026 iktatószámú ügyirat alá? Csak a darabszámot add vissza.
```

```text
Add vissza a 0003/1-AI20262/2026 iktatószámú ügyirat alá tartozó iratok tárgyait.
```

```text
Keresd meg a 0003/1-AI20262/2026 iktatószámú ügyiratot, és add vissza, hány irat tartozik hozzá.
```

## 6. Iratokkal kapcsolatos kerdesek

```text
Listázd ki az összes irat tárgyát a 0003/1-AI20262/2026 ügyirat alatt.
```

```text
Add vissza a legutóbb létrehozott irat adatait a 0003/1-AI20262/2026 ügyirat alatt.
```

```text
Keresd meg az összes olyan iratot, amelynek a tárgya "tesztelési tárgy".
```

```text
Add vissza a 0003/1-AI20262/2026 ügyirat alá tartozó iratok azonosítóit és tárgyait.
```

## 7. Felhasznalokkal kapcsolatos promptok

```text
Listázd ki az összes elérhető felhasználó nevét és felhasználónevét.
```

```text
Hozz létre egy új felhasználót Kovács Péter névvel, kovacsp felhasználónévvel és Teszt123 jelszóval.
```

```text
Add vissza az összes felhasználó nevét abc sorrendben.
```

## 8. Osszetettebb workflow-k

```text
Hozz létre egy új iktatókönyvet "AI Mintakönyv" néven AI2027 kóddal, 2027-es évszámmal. Ezután indíts benne egy új ügyet "Próba ügy" tárggyal, üres Details mezővel, és add vissza a létrejött iktatószámot.
```

```text
Keresd meg az AI20262 kódú iktatókönyvet, majd add vissza benne az összes főszámos ügy iktatószámát.
```

```text
Keresd meg a 0003/1-AI20262/2026 ügyiratot, majd iktass alá egy új alszámos iratot "Új melléklet" tárggyal, végül add vissza az új iktatószámot.
```

```text
Listázd ki az elérhető iktatókönyvek kódjait, majd keresd meg az AI20262 kódú iktatókönyvet, és add vissza benne az összes alszámos iktatószámot.
```

## 9. Eredmenyre vonatkozo hasznos kiegeszitesek

Ha fontos, hogy a rendszer mit adjon vissza, a prompt vegere erdemes ilyen mondatokat tenni:

```text
Csak a létrejött iktatószámot add vissza.
```

```text
Csak a darabszámot add vissza.
```

```text
Add vissza a létrejött rekord azonosítóját is.
```

```text
Rövid, ügyintéző számára olvasható összefoglalót adj vissza.
```

```text
Listaként add vissza az eredményt.
```

## 10. Jo promptok, amiket erdemes masolni

### Uj ugy nyitasa

```text
Indíts egy új ügyet a AI20262 kódú iktatókönyvben "Nagyon fontos ügy" tárggyal, minden egyéb mező üres legyen, és add vissza a létrejött iktatószámot.
```

### Alszamos bovites

```text
Iktass alszámosan a 0003/1-AI20262/2026 ügyirat alá "tesztelési tárgy" tárggyal, üres Details mezővel, és add vissza az új iktatószámot.
```

### Iktatokonyvbol iktatoszamok

```text
Add vissza a AI20262 kódú iktatókönyvben szereplő összes iktatószámot.
```

### Ugyirat alatti iratok szama

```text
Hány irat tartozik a 0003/1-AI20262/2026 iktatószámú ügyirat alá? Csak a darabszámot add vissza.
```

### Iktatokonyv kodok

```text
Listázd ki az elérhető iktatókönyvek kódjait.
```

## 11. Mire figyelj promptiras kozben

- Ha iktatokonyvre hivatkozol, inkabb `kod`-ot adj meg.
- Ha ugyiratra hivatkozol, gyakran a teljes szoveges iktatoszam a legjobb azonosito.
- Ha uj ugyet akarsz, `főszámos` logikara van szukseg.
- Ha meglevo ugyet akarsz bovitni, `alszámos` logikara van szukseg.
- Ha pontos eredmenyt varsz, mondd meg, hogy listat, darabszamot vagy uj iktatoszamot kersz vissza.

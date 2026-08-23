# Belgeden koda satir atfi kapisi: Docs/ icindeki her "Dosya.cs:SATIR" COZULMELI.
#
# NEDEN VAR: Tools/check-cross-file-refs.py yalniz Assets/**/*.cs geziyor.
# Olculdu 2026-08-23: Docs/ altinda 387 adet "Dosya.cs:SATIR" atfi vardi, 16 ayri
# belgede, ve HICBIRI hicbir kapinin denetiminde degildi. Bes kapi yesil yaniyor
# ve bu 387 atfa bakmiyordu. (Sayi bir an fotografi; her yeni belge onu buyutuyor,
# guncel deger her kosumda ciktinin ust satirinda yazar.) Docs/ogrenme/README.md'nin
# yazdigi gibi: sessizce yesil yanan bir kapi hic olmayan kapidan daha kotudur.
#
# ASIL TEHLIKE SINIR DEGIL KAYMADIR. "Dosya 395 satir, atif 554" turu imkansiz
# atif bir tanedir ve gozle de gorulur. Tehlikeli olan sessizce kaymis atiftir:
# dosya var, satir var, ama artik baska bir sey soyluyor. Onceki turlarda satir
# numaralari 6 satir kaydigi icin yanlis olculer yazildi ve hicbir kapi
# kizarmadi. Bu yuzden asagida uc katman var ve ucuncusu asil istir.
#
# NE TARAR (uc katman, artan gucte):
#   KATMAN 1 varlik : anilan .cs Assets/ altinda var mi. Iki yerde ayni ad
#                     varsa AYRI bir durum olarak bildirilir, sessizce ilki
#                     secilmez. Yollu atifta yol da dogrulanir.
#   KATMAN 2 sinir  : satir numarasi dosyanin satir sayisini asiyor mu.
#                     Aralik atfinda (Dosya.cs:10-20) iki uc da sinanir.
#   KATMAN 3 kayma  : (a) ALINTI -- atfin yaninda duran kod metni anilan
#                     dosyada AYNEN geciyorsa, o metnin gercek satir numarasi
#                     atifla ayni mi. Iki bicim de taninir: atiftan sonra AYNI
#                     satirda gelen kod (sema listelerinde 10 ornek) ve atif
#                     tek basina bir satirdayken ALTINDAKI satir (4 ornek).
#                     Metin dosyada hic gecmiyorsa bu bir alinti degil (sema,
#                     elenmis kod, dumen cumlesi) ve denetlenmez -- sahte
#                     pozitif uretmemek icin.
#                     (b) YAKIN AD -- atifin +-2 satirinda gecen ters tirnakli
#                     tanimlayicilardan EN AZ BIRI, anilan dosyanin N. satirinin
#                     +-15 satirinda geciyor mu.
#
# ██ KAPSAM ABARTILMAZ ██ Katman 3 her atfa uygulanamaz; uygulanamayanin sayisi
# ve NEDENI ciktida yazilir. Denetlenmemis bir atfi denetlenmis gibi gostermek
# kapinin kendisini yalan yapar.
#
# ██ IKI SINYALIN GUCU ESIT DEGIL -- OLCULDU ██ Gecen her atif yapay olarak
# kaydirilip kapiya geri verildi (153 atif):
#
#     kayma     ALINTI (17 atif)     YAKIN AD (136 atif)
#      3 satir       %100                  %1
#      6 satir       %100                  %1
#     20 satir       %100                 %41
#     80 satir       %100                 %71
#
# Yani bu projede yasanan 6 SATIRLIK kaymayi yalniz ALINTI katmani gorur, ve o
# katman bugun 17 atfi kapsiyor. YAKIN AD katmani kucuk kaymaya KORDUR: kayma
# uyenin icinde kalir ve mesafe esigi asilmaz. Bu bir kusur degil sinirdir, ama
# GIZLENIRSE kusur olur -- bu yuzden kapi bunu her kosumda kendi ciktisina yazar.
# Kapsami buyutmenin tek dogru yolu belgelere daha cok ALINTI yazmaktir.
#
# Kullanim:
#   python Tools/check-doc-code-refs.py
#   python Tools/check-doc-code-refs.py <belge-koku> <kaynak-koku>   # negatif test

import pathlib
import re
import sys

DOCS_DEFAULT = "Docs"
ASSETS_DEFAULT = "Assets"

# "BoardAdapter.cs:225" . "Assets/Game/Unity/BoardAdapter.cs:9-97"
REF = re.compile(r"((?:[A-Za-z0-9_.-]+/)*)([A-Za-z0-9_]+\.cs):(\d+)(?:-(\d+))?")
FENCE = re.compile(r"^\s*```")
BACKTICK = re.compile(r"`([^`]+)`")
IDENT = re.compile(r"^[A-Za-z_][A-Za-z0-9_]*(?:\.[A-Za-z_][A-Za-z0-9_]*)*$")
TOKEN = re.compile(r"[A-Za-z_][A-Za-z0-9_]*")

# MUAFIYET: bu isaretciyi tasiyan satirdaki atiflar denetlenmez. Acilis cit
# satirinda gecerse butun blok muaf olur. Muaf tutulan atif sayisi ciktida
# RAPORLANIR -- sessiz muafiyet kapiyi korlestiren seyin ta kendisidir.
MUAF = "ATIF-MUAF"

# Yakin ad toplama penceresi (belge tarafi). Tablo satirlarinda 0'a duser:
# olculdu, bir tablonun komsu SATIRLARI birbiriyle ilgisiz kayitlar ve pencere
# oraya tasinca alakasiz adlar toplaniyor (Docs/ogrenme/03-kavram-borc-defteri.md
# uzerinde 6 sahte pozitif tam olarak bundan cikti).
SPAN = 2

# Kayma esigi (kod tarafi). GEREKCE: bu projede bir uye govdesinin uzunlugu
# olculdu -- 499 uye, medyan 11 satir, p75 14, p90 23. K=15 "adin gectigi yer
# ile anilan satir en fazla BIR uye kadar uzakta" demektir. Olcum:
#   K=10 -> bugun 19 ihlal, elle bakildiginda 6'si sahte (d=11..19, hepsi ayni
#           ya da bitisik uyenin icinde)
#   K=15 -> 13 ihlal, sahte pozitif kalmadi
#   K=20 -> 11 ihlal ama yapay 20 satirlik kaymanin yalniz %4'unu yakaliyor
# Korluk olcumu (gecen atiflar yapay olarak kaydirilip kapiya geri verildi):
#   K=10: kayma 20 -> %60 . 40 -> %59 . 80 -> %74
#   K=15: kayma 20 -> %42 . 40 -> %50 . 80 -> %70
#   K=20: kayma 20 -> %4  . 40 -> %48 . 80 -> %65
# K=20'deki cokus esigin tipik uye boyunu asmasindan: kayma uyenin icinde
# kaliyor ve gorunmez oluyor. K=15 kapsam ile guc arasindaki son dengeli nokta.
K = 15

# C# anahtar sozcukleri yakin-ad sinyalinde ise yaramaz: her dosyada gecerler.
KEYWORDS = set("""abstract as base bool break byte case catch char checked class const continue
decimal default delegate do double else enum event explicit extern false finally fixed float for
foreach goto if implicit in int interface internal is lock long namespace new null object operator
out override params private protected public readonly ref return sbyte sealed short sizeof
stackalloc static string struct switch this throw true try typeof uint ulong unchecked unsafe
ushort using var virtual void volatile while yield get set value
add remove async await dynamic from where select group into orderby join let
ascending descending equals partial when nameof record init global args""".split())
# Ikinci satir baglama duyarli anahtar sozcukler. OLCULDU: bunlar olmadan
# `add`/`remove` (olay erisimcileri) ve `from` (LINQ) iki sahte pozitif uretti --
# ikisi de dil sozcugu, proje adi degil, ve dosyanin her yerinde gecebilirler.

# Uc harften kisa adlar ayirt edici degil (x, id, a) -- elenir.
IDENT_MIN = 3


def normalize(text):
    """Bosluklari tek bosluga indirger; alinti karsilastirmasi bunun uzerinde."""
    return " ".join(text.split())


class Source:
    """Anilan .cs dosyalarinin kaynagi.

    Neden bir sinif: oz-sinama diske DOKUNMADAN kosmali. Diskteki bir dosyaya
    bagimli oz-sinama, o dosya tasindigi gun kapiyi da sessizce oldururdu
    (Tools/check-curriculum-coverage.py'nin oz-sinama notunun ayni gerekcesi).
    Gercek kosumda from_disk, oz-sinamada duz bir sozluk kullanilir.
    """

    def __init__(self, files):
        self.files = {k.replace("\\", "/"): v for k, v in files.items()}
        self._lines = {}
        self._words = {}
        self._texts = {}
        self._by_name = {}
        for key in self.files:
            self._by_name.setdefault(key.rsplit("/", 1)[-1], []).append(key)
        for value in self._by_name.values():
            value.sort()

    @classmethod
    def from_disk(cls, root):
        files = {}
        base = pathlib.Path(root)
        for path in base.rglob("*.cs"):
            files[path.as_posix()] = path.read_text(encoding="utf-8", errors="replace")
        return cls(files)

    def by_name(self, name):
        return list(self._by_name.get(name, []))

    def by_suffix(self, relative):
        """Yollu atfi cozer: tam eslesme yoksa yol SONEKI ile eslestirir.

        Belgelerde iki bicim de gecti -- 'Assets/Game/Unity/BoardAdapter.cs' ve
        kok-disi kisaltmalar. Sonek eslesmesi '/' sinirinda yapilir, yoksa
        'View.cs' ile 'UnitView.cs' birbirine karisirdi.
        """
        relative = relative.replace("\\", "/").lstrip("./")
        if relative in self.files:
            return [relative]
        return sorted(k for k in self.files if k.endswith("/" + relative))

    def lines(self, key):
        if key not in self._lines:
            parts = self.files[key].split("\n")
            if parts and parts[-1] == "":
                parts.pop()
            self._lines[key] = parts
        return self._lines[key]

    def count(self, key):
        return len(self.lines(key))

    def words(self, key):
        """tanimlayici -> gectigi satir numaralari"""
        if key not in self._words:
            table = {}
            for number, line in enumerate(self.lines(key), 1):
                for word in TOKEN.findall(line):
                    table.setdefault(word, []).append(number)
            self._words[key] = table
        return self._words[key]

    def texts(self, key):
        """normalize edilmis satir metni -> gectigi satir numaralari"""
        if key not in self._texts:
            table = {}
            for number, line in enumerate(self.lines(key), 1):
                flat = normalize(line)
                if flat:
                    table.setdefault(flat, []).append(number)
            self._texts[key] = table
        return self._texts[key]


def is_table_row(line):
    stripped = line.strip()
    return stripped.startswith("|") and stripped.endswith("|")


def nearby_identifiers(lines, index):
    """Atif satirinin cevresindeki ters tirnakli tanimlayicilari toplar.

    Yalniz TAMAMI tanimlayici olan ters tirnak icerigi alinir: `Battle.AddUnit`
    evet, `new Battle(w, h)` hayir. Olculdu: ters tirnak icindeki her token'i
    toplayan zengin surum kapsami 145'ten 234'e cikariyor ama kapiyi
    KORLESTIRIYOR -- yapay 80 satirlik kaymanin yalniz %15'ini yakaliyor
    (dar surumde %70). Sebep acik: `width`, `height`, `Color` gibi adlar
    dosyanin her yerinde geciyor ve en yakin gecis her zaman yakin cikiyor.
    Kapsam degil GUC secildi.
    """
    low, high = index, index
    if not is_table_row(lines[index]):
        low = max(0, index - SPAN)
        high = min(len(lines) - 1, index + SPAN)

    found = set()
    for j in range(low, high + 1):
        for chunk in BACKTICK.findall(lines[j]):
            chunk = chunk.strip()
            if not IDENT.match(chunk) or chunk.endswith(".cs"):
                continue
            tail = chunk.split(".")[-1]
            if tail in KEYWORDS or len(tail) < IDENT_MIN:
                continue
            found.add(tail)
    return found


def quoted_text(lines, index, ref_text):
    """Atfin yanindaki kod alintisini doner, yoksa None.

    Iki bicim taninir, ikisi de belgelerde olculdu:

      AYNI SATIR (10 ornek, sema listeleri)
          BoardAdapter.cs:943   ApplyStateVisual(Unit unit, UnitState state)

      ALT SATIR (4 ornek)
          UnitView.cs:77
          private Color authoredColor = Color.white;

    Iki durumda da alinti sayilmaz: metin bos kalirsa, ya da metnin kendisi
    baska bir atif ise. Olculdu -- iki atfin alt alta yazildigi listeler var
    ve orada alt satir kod degil baska bir atiftir.
    """
    stripped = lines[index].strip()
    head = stripped.find(ref_text)
    if head < 0:
        return None

    # Atif satirin BASINDA olmali (en fazla bir ters tirnak onunde).
    if stripped[:head].strip().strip("`"):
        return None

    rest = stripped[head + len(ref_text):].strip()
    rest = rest.strip("`").strip()
    for tail in (":", "-", "—", "."):
        if rest.endswith(tail):
            rest = rest[:-1].strip()

    if rest:
        if REF.search(rest):
            return None
        return normalize(rest)

    # Atif tek basina: alintiyi ALTINDAKI satirda ara.
    if index + 1 >= len(lines):
        return None
    below = lines[index + 1]
    if not below.strip() or FENCE.match(below) or REF.search(below):
        return None
    return normalize(below)


def audit(documents, source):
    """documents: [(ad, metin)] . source: Source . -> (ihlaller, sayaclar)

    ihlal sekli: (belge_adi, satir, tur, mesaj). tur oz-sinamada aranir.
    """
    problems = []
    stat = {
        "atif": 0, "muaf": 0,
        "k1": 0, "k1_yok": 0, "k1_coklu": 0,
        "k2": 0, "k2_asim": 0,
        "k3a": 0, "k3a_kayma": 0,
        "k3b": 0, "k3b_kayma": 0,
        "k3_yok_ad": 0, "k3_yok_dosyada": 0, "k3_yok_sinir": 0,
    }

    for name, text in documents:
        lines = text.split("\n")
        in_fence = False
        fence_exempt = False

        for index, line in enumerate(lines):
            number = index + 1

            if FENCE.match(line):
                if in_fence:
                    in_fence, fence_exempt = False, False
                else:
                    in_fence = True
                    fence_exempt = MUAF in line
                continue

            exempt = fence_exempt or (MUAF in line)

            for match in REF.finditer(line):
                stat["atif"] += 1
                if exempt:
                    stat["muaf"] += 1
                    continue

                prefix, base, first, second = match.groups()
                ref_text = match.group(0)

                # ── KATMAN 1: varlik ──────────────────────────────────────
                stat["k1"] += 1
                if prefix:
                    targets = source.by_suffix(prefix + base)
                    if not targets:
                        elsewhere = source.by_name(base)
                        if elsewhere:
                            problems.append((name, number, "YOK",
                                             "YOL YANLIS: %s (gercek yeri: %s)"
                                             % (ref_text, " . ".join(elsewhere))))
                        else:
                            problems.append((name, number, "YOK",
                                             "DOSYA YOK: %s" % ref_text))
                        stat["k1_yok"] += 1
                        continue
                else:
                    targets = source.by_name(base)
                    if not targets:
                        problems.append((name, number, "YOK",
                                         "DOSYA YOK: %s" % ref_text))
                        stat["k1_yok"] += 1
                        continue

                if len(targets) > 1:
                    # SESSIZCE ILKINI SECMEK YASAK: iki farkli dizinde ayni ad
                    # varsa atif zaten belirsizdir ve yazan kisi yolu yazmali.
                    problems.append((name, number, "COKLU",
                                     "AD COKLU: %s -> %s"
                                     % (ref_text, " . ".join(targets))))
                    stat["k1_coklu"] += 1
                    continue

                key = targets[0]

                # ── KATMAN 2: sinir ───────────────────────────────────────
                stat["k2"] += 1
                low = int(first)
                high = int(second) if second else low
                total = source.count(key)
                if low < 1 or high > total or high < low:
                    problems.append((name, number, "SINIR",
                                     "SATIR DOSYAYI ASIYOR: %s (%s %d satir)"
                                     % (ref_text, key, total)))
                    stat["k2_asim"] += 1
                    stat["k3_yok_sinir"] += 1
                    continue

                # ── KATMAN 3a: alinti ─────────────────────────────────────
                audited = False
                quote = quoted_text(lines, index, ref_text)
                if quote:
                    where = source.texts(key).get(quote)
                    if where:
                        # Metin dosyada var: artik nerede oldugu SORULABILIR.
                        # Dosyada hic yoksa bu bir alinti degil (sema, dumen
                        # cumlesi, elenmis kod) ve denetlenmez.
                        stat["k3a"] += 1
                        audited = True
                        if not any(low <= w <= high for w in where):
                            problems.append((name, number, "ALINTI",
                                             "ALINTI KAYMIS: %s -- bu metin %s"
                                             % (ref_text,
                                                ", ".join(str(w) for w in where)
                                                + ". satirda")))
                            stat["k3a_kayma"] += 1

                # ── KATMAN 3b: yakin ad ───────────────────────────────────
                idents = nearby_identifiers(lines, index)
                if not idents:
                    if not audited:
                        stat["k3_yok_ad"] += 1
                    continue

                table = source.words(key)
                best, best_name = None, None
                for ident in sorted(idents):
                    for where in table.get(ident, ()):
                        if low <= where <= high:
                            distance = 0
                        else:
                            distance = min(abs(where - low), abs(where - high))
                        if best is None or distance < best:
                            best, best_name = distance, ident

                if best is None:
                    if not audited:
                        stat["k3_yok_dosyada"] += 1
                    continue

                stat["k3b"] += 1
                if best > K:
                    problems.append((name, number, "AD",
                                     "YAKIN AD UZAKTA: %s -- `%s` en yakin %d "
                                     "satir otede (esik %d)"
                                     % (ref_text, best_name, best, K)))
                    stat["k3b_kayma"] += 1

    return problems, stat


# ── OZ-SINAMA ORNEKLERI ──────────────────────────────────────────────────
# Ikisi de bu dosyanin icinde ve SANAL bir kaynak uzerinde kosuyor: diske
# dokunmayan bir oz-sinama, projedeki hicbir dosya tasinsa da olmez.

SELF_FILES = {
    "Assets/Sanal/IkinciTip.cs": "\n".join([
        "namespace Sanal",                                     # 1
        "{",                                                   # 2
        "    public static class IkinciTip",                   # 3
        "    {",                                               # 4
        "        public const int Esik = 3;",                   # 5
        "",                                                    # 6
        "        public static int HesaplaMesafe(int a, int b)",  # 7
        "        {",                                           # 8
        "            return a > b ? a - b : b - a;",           # 9
        "        }",                                           # 10
        "",                                                    # 11
        "        public static bool Yakin(int a, int b)",       # 12
        "        {",                                           # 13
        "            return HesaplaMesafe(a, b) <= Esik;",     # 14
        "        }",                                           # 15
        "    }",                                               # 16
        "}",                                                   # 17
    ] + ["    // dolgu %d" % i for i in range(18, 91)]),        # 18..90
    "Assets/Sanal/BirinciTip.cs": "\n".join([
        "namespace Sanal",
        "{",
        "    public sealed class BirinciTip",
        "    {",
        "    }",
        "}",
    ]),
    "Assets/Sanal/Alt/BirinciTip.cs": "\n".join([
        "namespace Sanal.Alt",
        "{",
        "    public sealed class BirinciTip",
        "    {",
        "    }",
        "}",
    ]),
}

SELF_GOOD = """
Yol ile cozulen atif: `Assets/Sanal/BirinciTip.cs:3` iki ayni adli dosya
arasindan dogru olani secilmeli.

`HesaplaMesafe` (`IkinciTip.cs:7`) mesafenin tek sahibi.

IkinciTip.cs:9
            return a > b ? a - b : b - a;

IkinciTip.cs:14   return HesaplaMesafe(a, b) <= Esik;
"""

SELF_BAD = """
`HesaplaMesafe` (`OlmayanTip.cs:5`) -- boyle bir dosya yok.

`Esik` (`IkinciTip.cs:500`) -- dosya bu kadar uzun degil.

`BirinciTip.cs:3` -- iki dizinde ayni ad var, yol yazilmali.

IkinciTip.cs:9
            return HesaplaMesafe(a, b) <= Esik;

IkinciTip.cs:12   return a > b ? a - b : b - a;

`HesaplaMesafe` (`IkinciTip.cs:80`) -- ad dosyada var ama cok uzakta.
"""

SELF_MUAF = """
```text ATIF-MUAF
OlmayanTip.cs:5
```
<!-- ATIF-MUAF --> `BasakOlmayanTip.cs:9` satir ici muafiyet.
"""


def self_check():
    """Ayristiricinin CALISTIGINI once kendi uzerinde kanitlar.

    Iki yari da zorunlu. Yalniz iyi ornegi sinayan bir oz-sinama, audit()
    fonksiyonu bosa dusse (her zaman 0 ihlal donse) bile GECERDI -- bu projede
    bir kapi tam olarak bu yuzden dort kez yanlislikla "temiz" dedi.
    """
    source = Source(SELF_FILES)

    good, gstat = audit([("iyi", SELF_GOOD)], source)
    if good:
        return "bilinen-iyi ornek ihlal uretti: %s" % (good[0][3],)
    if gstat["atif"] != 4:
        return "bilinen-iyi ornekte 4 atif bekleniyordu, %d bulundu" % gstat["atif"]
    if gstat["k3a"] != 2:
        return ("bilinen-iyi ornekte alinti katmaninin iki bicimi de calismadi "
                "(k3a=%d, 2 bekleniyordu: ayni satir + alt satir)" % gstat["k3a"])
    if gstat["k3b"] < 1:
        return "bilinen-iyi ornekte yakin ad katmani calismadi (k3b=%d)" % gstat["k3b"]

    bad, bstat = audit([("kotu", SELF_BAD)], source)
    kinds = set(kind for _, _, kind, _ in bad)
    for wanted in ("YOK", "SINIR", "COKLU", "ALINTI", "AD"):
        if wanted not in kinds:
            return ("bilinen-kotu ornekte %s YAKALANMADI -- kapi bu katmanda kor "
                    "(yakalananlar: %s)" % (wanted, ", ".join(sorted(kinds)) or "hicbiri"))
    if bstat["atif"] != 6:
        return "bilinen-kotu ornekte 6 atif bekleniyordu, %d bulundu" % bstat["atif"]
    if bstat["k3a_kayma"] != 2:
        return ("bilinen-kotu ornekte alinti kaymasinin iki bicimi de "
                "yakalanmadi (k3a_kayma=%d, 2 bekleniyordu)" % bstat["k3a_kayma"])

    muaf, mstat = audit([("muaf", SELF_MUAF)], source)
    if muaf:
        return "muafiyet isaretcisi calismadi: %s" % (muaf[0][3],)
    if mstat["muaf"] != 2:
        return "muafiyet sayaci yanlis: 2 bekleniyordu, %d cikti" % mstat["muaf"]

    return None


def main(argv):
    docs_root = pathlib.Path(argv[1] if len(argv) > 1 else DOCS_DEFAULT)
    assets_root = pathlib.Path(argv[2] if len(argv) > 2 else ASSETS_DEFAULT)

    broken = self_check()
    if broken is not None:
        print("KAPI BOZUK: %s" % broken)
        return 2

    if not docs_root.is_dir():
        print("KAPI BOZUK: belge koku bulunamadi -> %s" % docs_root)
        return 2
    if not assets_root.is_dir():
        print("KAPI BOZUK: kaynak koku bulunamadi -> %s" % assets_root)
        return 2

    source = Source.from_disk(assets_root)
    if not source.files:
        print("KAPI BOZUK: kaynak kokunde tek bir .cs yok -> %s" % assets_root)
        return 2

    md_files = sorted(docs_root.rglob("*.md"))
    documents = [(p.as_posix(), p.read_text(encoding="utf-8", errors="replace"))
                 for p in md_files]

    problems, stat = audit(documents, source)

    for name, number, _, text in problems:
        print("%s:%d\n    %s" % (name, number, text))

    unaudited = stat["k3_yok_ad"] + stat["k3_yok_dosyada"] + stat["k3_yok_sinir"]
    print("")
    print("belge: %d  .cs: %d  atif: %d  (muaf: %d)"
          % (len(md_files), len(source.files), stat["atif"], stat["muaf"]))
    print("KATMAN 1 varlik : %d denetlendi . %d dosya YOK . %d ad COKLU"
          % (stat["k1"], stat["k1_yok"], stat["k1_coklu"]))
    print("KATMAN 2 sinir  : %d denetlendi . %d SINIR asimi"
          % (stat["k2"], stat["k2_asim"]))
    print("KATMAN 3 kayma  : %d denetlendi (alinti %d . yakin ad %d) . %d KAYMA"
          % (stat["k3a"] + stat["k3b"], stat["k3a"], stat["k3b"],
             stat["k3a_kayma"] + stat["k3b_kayma"]))
    # Kapinin kendi sinirini SOYLEMESI zorunlu: bu satirlar olmasa cikti
    # "153 atif kayma icin denetlendi" der ve okuyan bunu tek bir guvence
    # sanirdi. Iki sinyalin gucu esit degil ve fark on kattan buyuk.
    print("                  ALINTI her kaymayi gorur   (olculdu: 3 satirlik "
          "kaymada bile %100)")
    print("                  YAKIN AD kucuk kaymaya KOR (olculdu: 6 satir %1 . "
          "20 satir %41 . 80 satir %71)")
    print("                  %d atif KAYMA icin HIC DENETLENEMEDI" % unaudited)
    print("                    %d yakininda ters tirnakli ad yok" % stat["k3_yok_ad"])
    print("                    %d anilan ad dosyada hic gecmiyor" % stat["k3_yok_dosyada"])
    print("                    %d zaten sinir ihlali" % stat["k3_yok_sinir"])
    print("")
    print("ihlal: %d" % len(problems))
    return 1 if problems else 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))

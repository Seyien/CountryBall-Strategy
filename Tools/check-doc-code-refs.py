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
#                     pozitif uretmemek icin. Metnin BENZERSIZ olup olmadigi
#                     ayrica sorulur; asagidaki bloga bak.
#                     (b) YAKIN AD -- atifin +-2 satirinda gecen ters tirnakli
#                     tanimlayicilardan EN AZ BIRI, anilan dosyanin N. satirinin
#                     +-15 satirinda geciyor mu.
#
# ██ KAPSAM ABARTILMAZ ██ Katman 3 her atfa uygulanamaz; uygulanamayanin sayisi
# ve NEDENI ciktida yazilir. Denetlenmemis bir atfi denetlenmis gibi gostermek
# kapinin kendisini yalan yapar.
#
# ══ ALINTI BENZERSIZ MI ══════════════════════════════════════════════════
# GATE-KOR SINIF, olculdu 2026-08-24: katman 3a "bu metin dosyada geciyor mu?"
# diye soruyordu, "bir KEZ mi geciyor?" diye SORMUYORDU. Ikiz span tasiyan bir
# kaynakta -- Assets/Game/Core/Combat/AttackAction.cs'te 71. ve 145. satir
# birebir ayni, ayni sey 82/153 ve 87/158 icin de dogru -- YANLIS asiri
# yuklemeye atif yapan bir belge SESSIZCE YESIL YANARDI: metin dosyada var,
# kapi memnun, kayma gorunmez.
#
# ██ NEDEN IHLAL DEGIL, SAYAC: OLCULDU ██ Denetlenen 393 alintinin dagilimi:
#     N=1 -> 365   N=2 -> 18   N=3 -> 5   N=4 -> 3   N=5 -> 2
# Yani 28 alinti coklu esliyor ve ██ 28'inin de anilan satiri eslesmelerden
# BIRIDIR ██ (bugun sifir yanlis). Demek ki coklu eslesme bir belge kusuru
# DEGIL; kaynakta ikiz span oldugunu soyler. Ustelik bir kismi BILEREK
# boyledir: Docs/ogrenme/06-ilkeler-ve-kokenleri.md:577, :579 ve :581 ayni
# `battle.Turn.EndTurn();` satirinin UC AYRI yerini (143 . 216 . 304) ayri ayri
# aniyor -- belgenin anlattigi sey zaten tekrarin kendisi. Ayni desen
# 07-oop-dortlusu.md:78/:80'de iki asiri yukleme icin var.
# Coklu eslesmeyi DOGRUDAN ihlal yapmak bu 28 DOGRU atfi kizartir, kapiyi 0
# ihlalden 28 ihlale tasir ve okuyani ihlal listesini gormezden gelmeye egitir.
# Kapinin kendi doktrini bunu zaten soyluyor: kapsam degil DOGRULUK. Bu yuzden
# coklu eslesme AYRI BIR SAYAC'tir ve ██ yalnizca anilan satir eslesmelerden
# HICBIRI degilse ihlaldir ██.
#
# ██ BU ALT KATMANIN KENDI KORLUGU ██ Coklu eslesen bir alintinin guvencesi
# benzersizinkinden ZAYIFTIR: "metin dogru yerde" demez, "metin eslesmelerden
# birinde" der. Kaynak kayarsa anilan satir OBUR ikizin uzerine dusebilir ve
# kapi yesil kalir. En zayif ornek bugun BoardAdapter.cs:361 --
# `if (selectedUnit == null)` dosyada BES kez geciyor (361 . 398 . 804 . 837 .
# 1031) ve iki belge ona dayaniyor. Bu yuzden coklu sayilanlar ciktida AYRICA
# basilir: sayi buyurse cozum kapiyi sertlestirmek degil, belgeye ikizi ayirt
# eden daha uzun bir alinti yazdirmaktir.
#
# KISAYOL katmani bu alt katmandan BILEREK muaftir: kisayolun ALINTI'si zaten
# neredeyse hic olusmuyor (asagida yazili) ve olcumu yok. Olculmemis bir yere
# katman koymak, kapsami sisirip dogrulugu dusurmek olurdu.
#
# ══ KISAYOL KATMANI ══════════════════════════════════════════════════════
# GATE-KOR SINIF, olculdu 2026-08-23: yukaridaki REF deseni dosya ADI ister.
# Ama belgeler dosya adini bir kez yazip sonrasini kisaltiyor -- `:992` gibi.
# Boyle 69 atif vardi, bes belgede, ve HICBIRI hicbir kapinin denetiminde
# degildi. Kapi 507 atif icin yesil yaniyor ve bu 69'a bakmiyordu.
#
# SAHIP KURALI: bir `:N` kisayolunun sahibi, AYNI PARAGRAFTA ve kisayoldan
# ONCE anilan EN SON .cs dosyasidir.
#
# PARAGRAF SINIRI = bos satir . cit satiri (```) . baslik satiri (#) . tablo
# satiri. Kisayolun kendisi bir tablo satirindaysa paragraf O TEK SATIRDIR
# (ayni olculmus gerekce SPAN'de de var: bir tablonun komsu satirlari
# birbiriyle ilgisiz kayitlardir).
#
# ██ NEDEN DAR: UC ADAY OLCULDU ██ (56 tek parcali kisayol uzerinde)
#     P-BLANK  (bos satir siniri)          35 sahip  .  0 YANLIS baglama
#     P-FENCE  (ustteki cit blogunu da yut) 42 sahip  .  1 YANLIS baglama
#     P-W10    (10 satir geriye pencere)    45 sahip  .  1 YANLIS baglama
# Yanlis baglamanin somut adi: 02-sonraki-asamalar.md:434 --
#   "`Battle.Tick` (`:366`)". Cumlenin bir ust blogu BoardAdapter.cs:268 ile
#   biten bir cit. Genis kurallar `:366`'yi BoardAdapter.cs'e bagliyor; orada
#   366. satir yerlestirme kipinin LogError'u, Tick ile ilgisi yok.
# ██ Iki hata esit degil ██: cikarilamayan sahip SAYILIR ve BASILIR, yani
# gorunur bir bosluktur. Yanlis baglanan sahip ya sahte ihlal uretir (biri
# DOGRU sayiyi yanlisiyla degistirir) ya da bayat bir atfi yanlis dosyaya
# karsi dogrular. Kapinin kendi doktrini bunu zaten soyluyor: sessizce yesil
# yanan kapi hic olmayan kapidan kotudur. Kapsam degil DOGRULUK secildi.
#
# ██ NEDEN GERIYE ██ "Paragrafta en son anilan" ileriye degil GERIYE taranir.
# Olculdu: 02-sonraki-asamalar.md:168-176 paragrafi once BoardAdapter.cs'i
# (168), sonra UnitView.cs'i (172) aniyor; 169 ve 170'teki `:728` ve `:992`
# BoardAdapter'a aittir. Ileri-son kural onlari UnitView.cs'e baglardi ve
# UnitView.cs 130 satir -- iki sahte SINIR ihlali. Ayni sey
# 04-yok-olan-mekanizmalar-unity.md:552-556'da da var.
#
# Kisayol atiflar ayni uc katmandan gecer (varlik . sinir . kayma) ama
# ALINTI katmani onlara neredeyse hic uygulanamaz: alinti bicimi atfin satir
# BASINDA olmasini ister, kisayol ise cumlenin ORTASINDA yasar. Yani kisayol
# atifin tek gercek kayma sinyali YAKIN AD'dir ve o sinyal kucuk kaymaya
# KORDUR. ██ Bu yuzden kisayolun kalici cozumu denetlemek degil, TAM BICIME
# (Dosya.cs:N) cevirmektir; kapi yalnizca borcu gorunur tutar. ██
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

# Kisayol atif: ters tirnak icinde YALNIZ ":N" simgeleri. Ayrac olarak bosluk,
# virgul ve orta nokta kabul edilir cunku belgede uc bicim de gecti:
#   `:992`   .   `:51` . `:59`   .   `:109 :110 :113 ...`
# Ters tirnak icinde baska HICBIR sey olmamali; `Ad :5` bilerek disarida
# birakildi, orada sahip zaten satirda yaziyor ve ayri bir lehcedir.
SHORT = re.compile(r"`((?::\d+(?:-\d+)?)(?:[ ,·]+:\d+(?:-\d+)?)*)`")
SHORT_ONE = re.compile(r":(\d+)(?:-(\d+))?")
# Kisayolun sahibini ararken kullanilan ciplak dosya adi deseni: satir numarasi
# ZORUNLU DEGIL. Olculdu -- belgeler sahibi cogu kez numarasiz yaziyor:
# "`Assets/Game/Core/Combat/Combatant.cs` kendisi. `TryRevive` (`:186`)".
CSNAME = re.compile(r"((?:[A-Za-z0-9_.-]+/)*)([A-Za-z0-9_]+\.cs)")
HEADING = re.compile(r"^\s{0,3}#")
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


def paragraph_start(lines, index):
    """Kisayolun paragrafinin BASLADIGI satir indeksini doner.

    Sinir: bos satir . cit (```) . baslik (#) . tablo satiri. Kisayolun kendi
    satiri bir tablo satiriysa paragraf o tek satirdir -- komsu tablo
    satirlari birbiriyle ilgisiz kayitlardir (ayni olcum SPAN'de yazili).
    """
    if is_table_row(lines[index]):
        return index
    start = index
    while start > 0:
        above = lines[start - 1]
        if not above.strip():
            break
        if FENCE.match(above) or is_table_row(above) or HEADING.match(above):
            break
        start -= 1
    return start


def shortcut_owner(lines, index, column):
    """Kisayolun sahibi: paragrafta ve kisayoldan ONCE anilan son .cs adi.

    Yoksa None. Sahibi cikarilamayan kisayol IHLAL DEGILDIR -- sayilir ve
    ciktida basilir; sessizce atlamak kapiyi kendi kapsami konusunda
    yalanci yapardi.
    """
    start = paragraph_start(lines, index)
    region = "\n".join(lines[start:index]) + "\n" + lines[index][:column]
    found = list(CSNAME.finditer(region))
    if not found:
        return None
    return found[-1].group(1) + found[-1].group(2)


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


def quote_verdict(where, low, high):
    """Alinti kararini veren TEK yer. where: metnin gectigi satirlar (bos degil).

    -> "benzersiz"    : tek eslesme, anilan satir O
       "kaymis"       : tek eslesme ama anilan satir baska  -> IHLAL (eski dal)
       "coklu_dogru"  : cok eslesme, anilan satir eslesmelerden BIRI
       "coklu_yanlis" : cok eslesme, anilan satir HICBIRI DEGIL -> IHLAL

    Neden ayri bir fonksiyon: oz-sinamanin ucuncu yarisi bu fonksiyonu bilerek
    BOZUK bir surumle degistirip kapinin bunu fark ettigini kanitliyor. Karar
    audit()'in icine gomulu kalsaydi sabote edilecek tek bir yer olmazdi.
    """
    hit = any(low <= w <= high for w in where)
    if len(where) == 1:
        return "benzersiz" if hit else "kaymis"
    return "coklu_dogru" if hit else "coklu_yanlis"


def mutant_hep_tek(where, low, high):
    """SABOTAJ 1: benzersizlik HEP 1 doner -- ikiz span'i hic gormez."""
    first = where[0]
    return "benzersiz" if low <= first <= high else "kaymis"


def mutant_yumusak(where, low, high):
    """SABOTAJ 2: coklu-ve-yanlis HIC ihlal uretmez -- hepsini dogru sayar."""
    hit = any(low <= w <= high for w in where)
    if len(where) == 1:
        return "benzersiz" if hit else "kaymis"
    return "coklu_dogru"


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
        # Benzersizlik alt katmani. DEGISMEZ: benzersiz + coklu_dogru +
        # coklu_yanlis == k3a. Ucu birden basilir, yoksa "393 alinti
        # denetlendi" satiri okuyana tek bir guvence gibi gorunurdu.
        "k3a_benzersiz": 0, "k3a_coklu_dogru": 0, "k3a_coklu_yanlis": 0,
        "k3b": 0, "k3b_kayma": 0,
        "k3_yok_ad": 0, "k3_yok_dosyada": 0, "k3_yok_sinir": 0,
        # Kisayol katmani AYRI sayilir: ana katmanin sayilarina karismaz,
        # yoksa kapi kendi kapsamini kendi ciktisinda sisirmis olurdu.
        "ks": 0, "ks_muaf": 0, "ks_sahip": 0, "ks_ihlal": 0,
        "ks_yok_sahip": 0, "ks_yok_dosya": 0,
        "ks_kayma_denetlendi": 0, "ks_kayma_yok": 0,
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
                        verdict = quote_verdict(where, low, high)
                        if verdict in ("benzersiz", "kaymis"):
                            stat["k3a_benzersiz"] += 1
                        elif verdict == "coklu_dogru":
                            stat["k3a_coklu_dogru"] += 1
                        else:
                            stat["k3a_coklu_yanlis"] += 1

                        if verdict == "kaymis":
                            problems.append((name, number, "ALINTI",
                                             "ALINTI KAYMIS: %s -- bu metin %s"
                                             % (ref_text,
                                                ", ".join(str(w) for w in where)
                                                + ". satirda")))
                            stat["k3a_kayma"] += 1
                        elif verdict == "coklu_yanlis":
                            # Eslesme satirlarini LISTELEMEK sart: okuyan ancak
                            # boyle "hangi ikizi kastetmistim" diye sorabilir.
                            problems.append((name, number, "ALINTI-COKLU",
                                             "ALINTI COKLU VE YANLIS: %s -- bu "
                                             "metin dosyada %d kez geciyor "
                                             "(%s. satirlar) ve anilan satir "
                                             "bunlardan HICBIRI DEGIL"
                                             % (ref_text, len(where),
                                                ", ".join(str(w)
                                                          for w in where))))
                            # KAYMA toplamina da yazilir: coklu-ve-yanlis da bir
                            # kaymadir ve "%d KAYMA" satiri eksik sayarsa kapi
                            # kendi yakalamasi hakkinda yalan soylemis olur.
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

            # ── KISAYOL KATMANI ───────────────────────────────────────────
            for span in SHORT.finditer(line):
                owner = shortcut_owner(lines, index, span.start())
                targets = []
                if owner:
                    if "/" in owner:
                        targets = source.by_suffix(owner)
                    else:
                        targets = source.by_name(owner)

                for piece in SHORT_ONE.finditer(span.group(1)):
                    short_text = piece.group(0)
                    stat["ks"] += 1
                    if exempt:
                        stat["ks_muaf"] += 1
                        continue

                    if owner is None:
                        stat["ks_yok_sahip"] += 1
                        continue
                    if not targets:
                        # Sahip ADI cikarildi ama diskte karsiligi yok. Bu bir
                        # kisayol ihlali DEGIL: ciplak dosya adi kapinin ana
                        # deseninin disinda ve onu burada ihlal saymak,
                        # denetlemedigimiz bir metinden ihlal uretmek olurdu.
                        stat["ks_yok_dosya"] += 1
                        continue
                    if len(targets) > 1:
                        problems.append((name, number, "KS-COKLU",
                                         "KISAYOL SAHIBI COKLU: `%s` -> %s -> %s"
                                         % (short_text, owner,
                                            " . ".join(targets))))
                        stat["ks_sahip"] += 1
                        stat["ks_ihlal"] += 1
                        stat["ks_kayma_yok"] += 1
                        continue

                    stat["ks_sahip"] += 1
                    key = targets[0]
                    low = int(piece.group(1))
                    high = int(piece.group(2)) if piece.group(2) else low
                    total = source.count(key)
                    if low < 1 or high > total or high < low:
                        problems.append((name, number, "KS-SINIR",
                                         "KISAYOL SATIRI DOSYAYI ASIYOR: `%s` "
                                         "-> %s (%d satir)"
                                         % (short_text, key, total)))
                        stat["ks_ihlal"] += 1
                        stat["ks_kayma_yok"] += 1
                        continue

                    audited = False
                    quote = quoted_text(lines, index, short_text)
                    if quote:
                        where = source.texts(key).get(quote)
                        if where:
                            audited = True
                            if not any(low <= w <= high for w in where):
                                problems.append((name, number, "KS-ALINTI",
                                                 "KISAYOL ALINTISI KAYMIS: `%s`"
                                                 " -> %s -- bu metin %s. satirda"
                                                 % (short_text, key,
                                                    ", ".join(str(w)
                                                              for w in where))))
                                stat["ks_ihlal"] += 1

                    # ██ KISAYOLUN PENCERESI SPAN DEGIL, KENDI SATIRIDIR ██
                    # OLCULDU (48 sahibi cikarilmis kisayol uzerinde):
                    #   +-2 satir  -> 27 denetlendi . 0 gercek yakalama .
                    #                 1 SAHTE POZITIF
                    #   kendi satiri -> 21 denetlendi . 0 gercek yakalama .
                    #                 0 sahte pozitif
                    # Sahte pozitifin adi: 04-yok-olan-mekanizmalar-unity.md:911.
                    # Ustteki satir `AllocatingGCMemory` adini tasiyor ama o ad
                    # AYNI CUMLEDEKI BASKA bir atfa (:103) aittir; pencere onu
                    # `:69` uzerine tasiyor ve DOGRU olan `:69`'u kizartiyor.
                    # Sebep yapisal: tam atif cogu kez satirinda yalnizdir,
                    # kisayol ise cumle ORTASINDA ve kardesleriyle YAN YANA
                    # yasar -- pencere komsu atfin adini calar. Ayni sey
                    # 02-sonraki-asamalar.md:170'te de olculdu: pencere `:992`
                    # icin iki satir asagidaki `UnitView` adini seciyor, oysa
                    # o satirin adi `Destroy`.
                    idents = nearby_identifiers([line], 0)
                    table = source.words(key)
                    best, best_name = None, None
                    for ident in sorted(idents):
                        for where in table.get(ident, ()):
                            if low <= where <= high:
                                distance = 0
                            else:
                                distance = min(abs(where - low),
                                               abs(where - high))
                            if best is None or distance < best:
                                best, best_name = distance, ident

                    if best is None:
                        if audited:
                            stat["ks_kayma_denetlendi"] += 1
                        else:
                            stat["ks_kayma_yok"] += 1
                        continue

                    stat["ks_kayma_denetlendi"] += 1
                    if best > K:
                        problems.append((name, number, "KS-AD",
                                         "KISAYOL YAKIN AD UZAKTA: `%s` -> %s "
                                         "-- `%s` en yakin %d satir otede "
                                         "(esik %d)"
                                         % (short_text, key, best_name,
                                            best, K)))
                        stat["ks_ihlal"] += 1

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
    # ██ IKIZ SPAN ██ Iki asiri yukleme birebir ayni govdeyi tasiyor:
    # "if (a < 0)" 7 VE 17'de, "return false;" 9 VE 19'da, "return true;"
    # 12 VE 22'de. Gercek kaynaktaki desenin (AttackAction.cs 71/145, 82/153,
    # 87/158) sanal ikizidir; benzersizlik katmani ancak boyle bir dosya
    # uzerinde sinanabilir.
    "Assets/Sanal/IkizTip.cs": "\n".join([
        "namespace Sanal",                                     # 1
        "{",                                                   # 2
        "    public static class IkizTip",                     # 3
        "    {",                                               # 4
        "        public static bool Once(int a)",              # 5
        "        {",                                           # 6
        "            if (a < 0)",                              # 7
        "            {",                                       # 8
        "                return false;",                       # 9
        "            }",                                       # 10
        "",                                                    # 11
        "            return true;",                            # 12
        "        }",                                           # 13
        "",                                                    # 14
        "        public static bool Sonra(int a)",             # 15
        "        {",                                           # 16
        "            if (a < 0)",                              # 17
        "            {",                                       # 18
        "                return false;",                       # 19
        "            }",                                       # 20
        "",                                                    # 21
        "            return true;",                            # 22
        "        }",                                           # 23
        "    }",                                               # 24
        "}",                                                   # 25
    ]),
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

IkizTip.cs:17
            if (a < 0)
"""

SELF_BAD = """
`HesaplaMesafe` (`OlmayanTip.cs:5`) -- boyle bir dosya yok.

`Esik` (`IkinciTip.cs:500`) -- dosya bu kadar uzun degil.

`BirinciTip.cs:3` -- iki dizinde ayni ad var, yol yazilmali.

IkinciTip.cs:9
            return HesaplaMesafe(a, b) <= Esik;

IkinciTip.cs:12   return a > b ? a - b : b - a;

`HesaplaMesafe` (`IkinciTip.cs:80`) -- ad dosyada var ama cok uzakta.

IkizTip.cs:12
            if (a < 0)
"""

SELF_MUAF = """
```text ATIF-MUAF
OlmayanTip.cs:5
```
<!-- ATIF-MUAF --> `BasakOlmayanTip.cs:9` satir ici muafiyet.
"""

# Kisayol katmaninin iki yarisi. IYI ornek bilerek IKI dosya aniyor, dogru
# olan kisayoldan ONCE ve yanlis olan SONRA duruyor -- ve ikisi AYNI SATIRDA,
# cunku sahip kuralinin yonu ancak boyle sinanir. Kural "ileri-son ad" haline
# gelirse BirinciTip.cs secilir, o ad iki dizinde birden gecer ve ornek COKLU
# ihlali uretir. Yani bu yari yalniz "ihlal yok" demiyor, sahibin DOGRU
# secildigini de kanitliyor. OLCULDU: kural bilerek ileri-son'a cevrildi ve
# bu yari KAPI BOZUK dedi; iki dosya ayri SATIRLARDA dururken DEMIYORDU.
SELF_KS_GOOD = """
`IkinciTip.cs:5` esigi tasir, `Yakin` (`:12`) onu okur, `BirinciTip.cs` okumaz.
"""

SELF_KS_BAD = """
Bu paragrafta hicbir kaynak dosyasi anilmiyor, yalniz bir kisayol var (`:12`).

`IkinciTip.cs:5` sonrasinda `Esik` (`:500`) dosyanin sonunu asiyor.

`BirinciTip.cs` ardindan gelen `:3` iki dizinde birden cozuluyor.

`IkinciTip.cs:5` ve `HesaplaMesafe` (`:80`) -- ad dosyada var ama cok uzakta.
"""


def self_check(sabotaj=False):
    """Ayristiricinin CALISTIGINI once kendi uzerinde kanitlar.

    Uc yari da zorunlu. Yalniz iyi ornegi sinayan bir oz-sinama, audit()
    fonksiyonu bosa dusse (her zaman 0 ihlal donse) bile GECERDI -- bu projede
    bir kapi tam olarak bu yuzden dort kez yanlislikla "temiz" dedi. Ucuncu
    yari (sabotaj) oz-sinamanin KENDISINI sinar; sabotaj=True ile cagrildiginda
    o yari atlanir, yoksa sonsuz ozyineleme olurdu.
    """
    source = Source(SELF_FILES)

    good, gstat = audit([("iyi", SELF_GOOD)], source)
    if good:
        return "bilinen-iyi ornek ihlal uretti: %s" % (good[0][3],)
    if gstat["atif"] != 5:
        return "bilinen-iyi ornekte 5 atif bekleniyordu, %d bulundu" % gstat["atif"]
    if gstat["k3a"] != 3:
        return ("bilinen-iyi ornekte alinti katmaninin bicimleri calismadi "
                "(k3a=%d, 3 bekleniyordu: ayni satir + alt satir + ikiz span)"
                % gstat["k3a"])
    if gstat["k3b"] < 1:
        return "bilinen-iyi ornekte yakin ad katmani calismadi (k3b=%d)" % gstat["k3b"]
    # ── BENZERSIZLIK: iki bilinen-iyi durum ──────────────────────────────
    if gstat["k3a_benzersiz"] != 2:
        return ("bilinen-iyi ornekte 2 BENZERSIZ alinti bekleniyordu, %d cikti"
                % gstat["k3a_benzersiz"])
    # ██ ASIL YARI ██ ikiz span, anilan satir eslesmelerden BIRI: gecerli ama
    # SAYILIR. Bu sinama olmadan katman ya hepsini ihlal sayardi (28 dogru atif
    # kizarirdi) ya da coklu eslesmeyi hic saymazdi (borc gorunmez olurdu).
    if gstat["k3a_coklu_dogru"] != 1:
        return ("ikiz span'li DOGRU alinti 'coklu-ama-dogru' sayacina dusmedi "
                "(k3a_coklu_dogru=%d, 1 bekleniyordu)" % gstat["k3a_coklu_dogru"])
    if gstat["k3a_coklu_yanlis"] != 0:
        return ("bilinen-iyi ornekte COKLU VE YANLIS uretildi (%d) -- ikiz span "
                "sahte pozitif veriyor" % gstat["k3a_coklu_yanlis"])
    if gstat["k3a_benzersiz"] + gstat["k3a_coklu_dogru"] + \
            gstat["k3a_coklu_yanlis"] != gstat["k3a"]:
        return "benzersizlik sayaclarinin toplami k3a'ya esit degil"

    bad, bstat = audit([("kotu", SELF_BAD)], source)
    kinds = set(kind for _, _, kind, _ in bad)
    for wanted in ("YOK", "SINIR", "COKLU", "ALINTI", "ALINTI-COKLU", "AD"):
        if wanted not in kinds:
            return ("bilinen-kotu ornekte %s YAKALANMADI -- kapi bu katmanda kor "
                    "(yakalananlar: %s)" % (wanted, ", ".join(sorted(kinds)) or "hicbiri"))
    if bstat["atif"] != 7:
        return "bilinen-kotu ornekte 7 atif bekleniyordu, %d bulundu" % bstat["atif"]
    if bstat["k3a_kayma"] != 3:
        return ("bilinen-kotu ornekte alinti kaymasinin uc bicimi de "
                "yakalanmadi (k3a_kayma=%d, 3 bekleniyordu: ayni satir . alt "
                "satir . ikiz span yanlis)" % bstat["k3a_kayma"])
    if bstat["k3a_coklu_yanlis"] != 1:
        return ("ikiz span'li YANLIS alinti 'COKLU VE YANLIS' sayacina dusmedi "
                "(k3a_coklu_yanlis=%d, 1 bekleniyordu)"
                % bstat["k3a_coklu_yanlis"])

    muaf, mstat = audit([("muaf", SELF_MUAF)], source)
    if muaf:
        return "muafiyet isaretcisi calismadi: %s" % (muaf[0][3],)
    if mstat["muaf"] != 2:
        return "muafiyet sayaci yanlis: 2 bekleniyordu, %d cikti" % mstat["muaf"]

    # ── KISAYOL KATMANI: iki yari da zorunlu ─────────────────────────────
    kgood, kgstat = audit([("kisayol-iyi", SELF_KS_GOOD)], source)
    kgood = [p for p in kgood if p[2].startswith("KS-")]
    if kgood:
        return "bilinen-iyi kisayol ihlal uretti: %s" % (kgood[0][3],)
    if kgstat["ks"] != 1:
        return ("bilinen-iyi kisayol ornekte 1 kisayol bekleniyordu, %d bulundu"
                % kgstat["ks"])
    if kgstat["ks_sahip"] != 1:
        return ("bilinen-iyi kisayolun SAHIBI cikarilamadi -- sahip kurali "
                "bosa dustu (ks_sahip=%d)" % kgstat["ks_sahip"])
    if kgstat["ks_kayma_denetlendi"] != 1:
        return ("bilinen-iyi kisayol kayma icin denetlenmedi "
                "(ks_kayma_denetlendi=%d)" % kgstat["ks_kayma_denetlendi"])

    kbad, kbstat = audit([("kisayol-kotu", SELF_KS_BAD)], source)
    kkinds = set(kind for _, _, kind, _ in kbad if kind.startswith("KS-"))
    for wanted in ("KS-SINIR", "KS-COKLU", "KS-AD"):
        if wanted not in kkinds:
            return ("bilinen-kotu kisayol ornekte %s YAKALANMADI -- kapi bu "
                    "katmanda kor (yakalananlar: %s)"
                    % (wanted, ", ".join(sorted(kkinds)) or "hicbiri"))
    if kbstat["ks"] != 4:
        return ("bilinen-kotu kisayol ornekte 4 kisayol bekleniyordu, %d bulundu"
                % kbstat["ks"])
    # ██ ASIL YARI ██ sahibi cikarilamayan kisayol IHLAL DEGIL, sayilan bir
    # bosluktur. Bu sinama olmadan kapi "gormedigim her sey ihlaldir" diyen
    # bir kapiya donusur ve dogru yazilmis belgeleri kizartirdi.
    if kbstat["ks_yok_sahip"] != 1:
        return ("sahipsiz kisayol 'cikarilamadi' sayacina dusmedi "
                "(ks_yok_sahip=%d, 1 bekleniyordu)" % kbstat["ks_yok_sahip"])
    if kbstat["ks_ihlal"] != 3:
        return ("bilinen-kotu kisayol ornekte 3 ihlal bekleniyordu, %d cikti "
                "-- sahipsiz kisayol ihlal sayilmis olabilir"
                % kbstat["ks_ihlal"])

    # ── UCUNCU YARI: SABOTAJ ─────────────────────────────────────────────
    if not sabotaj:
        broken = sabotage_check()
        if broken is not None:
            return broken

    return None


def sabotage_check():
    """██ UCUNCU YARI ██ Yeni katmani bilerek bozar ve OZ-SINAMANIN BUNU
    GORDUGUNU kanitlar.

    Ikinci yari "bilinen-kotu ornek yakalandi" der. Ama o ornek kapinin BASKA
    bir dalindan da yakalanabilir; o zaman benzersizlik katmani bosa dusmus
    olmasina ragmen oz-sinama YESIL yanardi. Bu yari tam olarak onu kapatir:
    ██ bozuk bir surumle oz-sinama GECIYORSA, oz-sinamanin kendisi ise
    yaramiyor demektir ██. Ayni disiplin bu lane'in olcum betiginde de
    kullanildi: dogrulama komutunun kendisi de bir kapidir ve sinanmalidir.

    Iki mutant, katmanin iki yarisini ayri ayri oldurur -- tek mutant yeterli
    degil: "hep tek" sayaclari bozar, "yumusak" ihlali bozar; biri gecerken
    digeri yakalanabilirdi.
    """
    global quote_verdict
    real = quote_verdict
    for label, mutant in (("benzersizlik hep 1 doner", mutant_hep_tek),
                          ("coklu-ve-yanlis hic ihlal uretmez", mutant_yumusak)):
        quote_verdict = mutant
        try:
            result = self_check(sabotaj=True)
        finally:
            quote_verdict = real
        if result is None:
            return ("SABOTAJ GECTI (%s) -- benzersizlik katmani bilerek "
                    "bozuldu ve oz-sinama BUNU FARK ETMEDI" % label)
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
    # ██ BENZERSIZLIK KENDI SAYISINI KENDI YAZAR ██ Bu satir olmasa "alinti
    # 393" tek bir guvence gibi okunurdu; oysa 28'i ikiz span uzerinde duruyor
    # ve orada guvence "metin dogru yerde" degil "metin eslesmelerden birinde".
    print("                  ALINTI benzersizlik : %d benzersiz . %d "
          "coklu-ama-dogru . %d COKLU VE YANLIS"
          % (stat["k3a_benzersiz"], stat["k3a_coklu_dogru"],
             stat["k3a_coklu_yanlis"]))
    print("                  SINIR: coklu eslesme bir belge kusuru DEGIL; "
          "kaynakta ikiz")
    print("                         span oldugunu soyler. Kapi yalniz YANLIS "
          "olani kizartir.")
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
    # ██ KISAYOL KATMANI KENDI KAPSAMINI KENDI YAZAR ██ Bu satirlar olmasa
    # cikti "atif: N" der ve okuyan `:992` bicimindeki atiflarin da o sayinin
    # icinde oldugunu sanirdi. Sahibi cikarilamayan kisayol ihlal degildir --
    # ama SAYILIR, cunku sayilmayan bir bosluk yok sayilmis bir bosluktur.
    print("KISAYOL `:N`    : %d tarandi . %d sahip cikarildi . %d IHLAL . "
          "%d sahip CIKARILAMADI  (muaf: %d)"
          % (stat["ks"], stat["ks_sahip"], stat["ks_ihlal"],
             stat["ks_yok_sahip"] + stat["ks_yok_dosya"], stat["ks_muaf"]))
    print("                    %d paragrafinda hic dosya adi anilmiyor"
          % stat["ks_yok_sahip"])
    print("                    %d anilan sahip diskte cozulemedi"
          % stat["ks_yok_dosya"])
    print("                  sahibi cikarilanin %d'i KAYMA icin denetlendi, "
          "%d'i denetlenemedi"
          % (stat["ks_kayma_denetlendi"], stat["ks_kayma_yok"]))
    print("                  ALINTI kisayola neredeyse hic uygulanamaz "
          "(atfin satir BASINDA olmasini ister)")
    print("")
    print("ihlal: %d" % len(problems))
    return 1 if problems else 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))

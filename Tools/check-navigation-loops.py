# Gezinme dongusu kapisi: belgeler arasi yonlendirme TEK YONLU KALMAMALI.
#
# NEDEN VAR: belgeler bugun tek yonlu isaretci tasiyor -- "once oku: 02". O
# isaretciyi izleyen okuyucu 02'nin icinde NEREYE DONECEGINI ogrenemez; geldigi
# yeri hatirlamak zorunda kalir ve cogu kez hatirlamaz. Operator bunu bildirdi:
# kusur okuyanda degil METINDE. Kurali belgeler tarafinda baska bir lane
# yaziyor; bu dosya o kuralin MAKINE KAPISIDIR.
#
# Kapinin cekirdegi UCUNCU MADDEDIR (KARSILIK). Digerleri "isaretci cozuluyor
# mu" diye sorar; ucuncusu "gidilen yer geri donmeyi BILIYOR mu" diye sorar.
# Bir ARA DURAK isaretcisi hedefine sagsalim varip orada karsiligini bulamazsa
# dongu kapanmamistir ve okuyucu tam da bildirilen yere duser.
#
# ══ DENETLENEN BICIM ═════════════════════════════════════════════════════
# Bicim ana ajan tarafindan sabitlendi, kapi onu DEGISTIRMEZ:
#
#     > **[ileri-ucgen] ARA DURAK:** [02-assembly-duvari.md](02-assembly-duvari.md#uc-ayri-sey)
#     > **NEDEN:** <tek cumle>
#     > **DONUS:** bu dosyanin [«Birinci durak»](#birinci-durak) bolumu
#
#     > **[geri-ucgen] DONUS:** [01-olay-zinciri.md](01-olay-zinciri.md#birinci-durak) — <nereden geldiysen>
#
#     > **[klavye] KODU AC:** `Assets/Game/Core/Combat/UnitLifecycle.cs` -> `SetState`
#     > **BAK:** <ne goreceksin> · **DONUS:** bu dosyanin «<bolum>» bolumu
#
# ══ AYRISTIRICI NEYI KABUL EDER (BILEREK ESNEK) ══════════════════════════
# Bicim sabit ama ayristirici kirilgan olursa kapi bicimin degil BOSLUGUN
# kapisi olur. Kabul edilenler, her biri bilerek:
#
#   1. SATIR SONU BOSLUKLARI  -- satir sonundaki bosluklar yok sayilir.
#   2. ALINTI ON EKI          -- satir basindaki bosluk ve '>' isaretleri
#                                (kac tane olursa olsun, arada bosluk olsun ya
#                                da olmasin) soyulur.
#   3. TEMBEL DEVAM SATIRI    -- markdown alintisi '>' yazilmadan da devam
#                                eder. Bir isaretcinin GOVDESI kendi satirinda
#                                bitmek zorunda degil: bos satira, bir sonraki
#                                etikete, bir basliga ya da bir cite kadar
#                                sonraki satirlardan devam eder.
#                                ██ OLCULDU: 02-assembly-duvari.md:129'daki
#                                KODU AC isaretcisinin ikinci hedefi ve uyesi
#                                130-131. satirlarda yaziyor. Tek satirlik bir
#                                govde o iki hedefi HIC gormezdi ve satiri
#                                "uye yok" diye SAHTE ihlal ederdi -- ilk
#                                surumunde tam olarak bu oldu. ██
#   4. VURGU VARYASYONLARI    -- '**', '*', '__', '_', '***' hepsi kabul; iki
#                                nokta vurgunun ICINDE ('**DONUS:**') ya da
#                                DISINDA ('**DONUS**:') olabilir.
#   5. SIMGE                  -- ileri/geri ucgen ve klavye simgeleri
#                                etiketten once, aralarinda bosluk olsun ya da
#                                olmasin.
#   6. TURKCE YAZIM           -- etiketin aksansiz ve aksanli yazimi esittir
#                                (DONUS ve aksanlisi, KODU AC ve aksanlisi);
#                                buyuk/kucuk harf de farketmez. Kabul edilen
#                                harflerin tam listesi DONUS_RE ve KODU_RE
#                                desenlerinin karakter kumelerinde yazili.
#   7. OK ISARETI             -- KODU AC satirinda '->' ve tek karakterlik ok
#                                esittir.
#   8. SATIRDA IKI ETIKET     -- 'BAK: ... · DONUS: ...' gibi ayni satirda iki
#                                etiket varsa ikisi de ayri ayri taninir ve
#                                birincinin govdesi ikincinin basladigi yerde
#                                biter.
#   9. BIR ISARETCIDE COK HEDEF -- bir KODU AC satiri birden fazla dosya ve
#                                birden fazla uye anabilir; hepsi ayri ayri
#                                denetlenir ve ayri ayri SAYILIR.
#
# ██ ISARETCI SAYILMA ESIGI ██ Bir etiket ancak SIMGESI ya da MARKDOWN VURGUSU
# varsa isaretci sayilir. Gerekce: duz yazi icinde gecen "donus:" ya da "ara
# durak:" sozcukleri isaretci degildir; simge/vurgu esigi olmadan kapi kendi
# kural belgesini denetlemeye kalkar ve her acikamayi ihlale cevirir.
#
# ██ 'DONUS:' ETIKETI IKI AYRI ISI YAPAR -- AYIRT EDICI GERI UCGENDIR ██
#     geri ucgen VAR  -> BAGIMSIZ donus isaretcisi. Hedefi BASKA bir dosyadir
#                        ('01-olay-zinciri.md#birinci-durak'). MADDE 3 ve 6
#                        yalniz bunlari sayar.
#     geri ucgen YOK  -> ARA DURAK / KODU AC blogunun ALT SATIRI. Hedefi KENDI
#                        dosyasidir ('#birinci-durak' ya da «Bolum adi»).
# Bu ayrimi simge disinda bir seye dayandirmak mumkun degil: iki satirin
# etiketi ayni sozcuk. Bicim sabitlenirken simge tam da bu yuzden konuldu.
#
# ══ CIT BLOKLARI ATLANIR ═════════════════════════════════════════════════
# Bir belge bu bicimi ORNEK olarak gosterirse (kural belgesi tam da bunu
# yapacak) ornek satirlari gercek isaretci degildir. Ayni gerekce
# Tools/check-curriculum-coverage.py'de de yazili: kapi kendi kullanim
# kilavuzunu denetlemeye kalkmamali.
#
# ══ CAPA COZUMU: SLUG UYUMU ZORUNLU ══════════════════════════════════════
# slug() asagida Tools/check-doc-links.py'den AYNEN kopyalandi. Kopya, sozle
# degil OLCUYLE korunuyor: main() calisirken o dosyayi yukler ve
#   (a) slug() ciktilarini bir sinama demeti uzerinde,
#   (b) her .md dosyasinin TAM CAPA KUMESINI
# karsilastirir. Bir tek fark bile 'KAPI BOZUK' verir. Iki kapi ayni slug
# kuralini kullanmazsa biri otekinin temiz dedigine ihlal der; bu, ayri ayri
# dogru olan iki kapinin BIRLIKTE yalan soylemesi demektir.
#
# ██ IKI AYRI BASLIK KUMESI VAR, VE BU BILEREK BOYLE ██
#   CAPA KUMESI  (madde 1 ve 2)  : cite BAKMAZ -- check-doc-links.py ile
#                                  birebir ayni kume, uyum icin.
#   BOLUM KUMESI (madde 3)       : citi ATLAR -- cit icindeki '# ...' satiri
#                                  bir bolum baslatmaz.
# OLCULDU (2026-08-23, Docs/): cit ICINDE baslik gorunumlu 9 satir var (hepsi
# 08-unity-altyapisi.md icindeki kabuk yorumlari). Tek kume kullansaydik ya
# check-doc-links.py ile ayrisirdik ya da o 9 satir bolum sinirini yanlis
# yerden keserdi. Ayrilan durum (capa var ama bolumu yok) MADDE 3'te
# 'denetlenemedi' olarak SAYILIR, sessizce gecilmez.
#
# ══ BOLUM SINIRI: "capanin bolumu" nedir ═════════════════════════════════
# Bir capanin bolumu = baslik satirindan SONRAKI satirdan baslar, AYNI ya da
# DAHA UST duzeyli bir sonraki basliga (ya da dosya sonuna) kadar surer.
# Alt basliklar (daha derin '#'ler) bolumun ICINDEDIR.
#
# ██ UC ADAY OLCULDU (Docs/, 1682 adet H2+ baslik) ██
#   A  bir sonraki HERHANGI baslik : B'nin gordugu satirlarin %40.5'ini
#                                    GORMEZ (23174 / 57219 satir; 273 baslikta
#                                    A ile B ayrisiyor)
#   B  ayni ya da daha UST duzey   : SECILEN
#   C  sabit 20 satirlik pencere   : 1682 bolumun 786'sinda (%46.7) KOMSU
#                                    bolume tasar -> sahte "karsilik var";
#                                    848'inde (%50.4) bolumun sonunu gormez
#                                    -> sahte "karsilik yok"
#
# A'nin somut zarari: 01-olay-zinciri.md'de "## Ucuncu durak ..." bolumu 136.
# satirda basliyor, icinde 173. satirda bir "### Ve iste sozlugun dogdugu an"
# alt basligi var, bir sonraki '##' 205'te. A kurali bolumu 172'de keser ve
# bolumun 32 satirini kapiya gostermez. Bir donus isaretcisinin dogal yeri
# bolumun BASI ya da SONUDUR; sonu A'nin gormedigi yerdedir.
# C'nin zarari iki yonlu: hem komsu bolumun donus satirini calip sahte temiz
# uretir, hem uzun bolumun sonundaki gercek donusu kacirir. Iki yonlu yanilan
# bir olcu, olcu degildir.
# B'nin sinirini de yazmak gerekir: cok derin bir alt bolume gomulmus donus
# satiri da bolumun icinde sayilir. Kabul edildi -- okuyucu zaten o bolumun
# tamamini kaydirarak geciyor; A'yi secmek "gormedigimi ihlal sayarim" demek
# olurdu ve dogru yazilmis belgeleri kizartirdi.
#
# ══ MADDE 2: BOLUM ADI «...» ILE YAZILDIGINDA ════════════════════════════
# KODU AC blogunun donus satirinda baglanti YOKTUR, bolum adi «...» icinde
# yazilir. Bu satirlar denetim disi birakilirsa MADDE 2'nin kapsami olculebilir
# bicimde cokuyor (bugunku fotografta 19 donus satirinin 7'si bu bicimde).
# Kural: once TAM slug eslesmesi; olmazsa TIRE SINIRINDA ONEK eslesmesi.
# ██ NEDEN ONEK ██ Olculdu, 02-assembly-duvari.md:320: donus satiri bolum adini
# KISALTILMIS yaziyor ("Ikinci fatura"), gercek baslik ise "Ikinci fatura: bir
# enum, sahibinin uretemedigi bir deger tasiyor" (belgede aksanli, capa slug'i
# ikisinde de ayni). Okuyucu bu adla bolumu bulur; tam eslesme istemek, bicimin hic
# koymadigi bir uslup kuralini dayatip dogru yazilmis satiri kizartmak olurdu.
# Onek BIRDEN COK baslikla eslesirse ad BELIRSIZDIR ve bu bir ihlaldir: o
# durumda okuyucu da hangisi oldugunu bilemez.
#
# ══ MADDE 4: SATIR NUMARASI IHLALDIR ═════════════════════════════════════
# KODU AC isaretcisi satir numarasi TASIYAMAZ. Gerekce olculdu: bugun 52
# kisayol atif 4-15 satir kaydi ve hicbiri yakalanmadi. Uye adi kaymaz, satir
# numarasi kayar. Kapi burada "cozuluyor mu" diye sormaz, VARLIGINI ihlal
# sayar -- cozulen bir satir numarasi da yarin kayacaktir.
#
# ══ MADDE 5: HEDEF '.cs' DEGIL 'Assets/' ALTIDIR ═════════════════════════
# ██ OLCULDU: 02-assembly-duvari.md:187 iki '.asmdef' dosyasina isaret ediyor
# ve ilk surum ".cs yolu yok" diye SAHTE ihlal uretti. ██ Assembly duvarini
# anlatan bir belgenin acacagi dosya cogu kez bir asmdef'tir. Kapi bu yuzden
# Assets/ altindaki HER dosyayi cozer; uye denetimi ise yalniz METIN
# dosyalarinda yapilabilir, digerleri 'denetlenemedi' olarak SAYILIR.
#
# ██ MADDE 5'IN SINIRI, ABARTILMADAN ██ Uye denetimi "bu ad dosyada GECIYOR"
# der, "bu dosya bu uyeyi TANIMLIYOR" demez. Somut ornek: 'SetState' hem
# UnitLifecycle.cs'te tanimli hem de StructureLifecycle.cs'in yorumunda anilir;
# ikinciye isaret eden bir isaretciyi bu kapi yakalamaz. Tanim ayristirmasi
# (ozellik, ifade govdeli uye, alan, enum ogesi, partial) sahte pozitif
# uretirdi. Sinir GIZLENMEZ: her kosumda ciktiya yazilir.
#
# ══ MUAFIYET ═════════════════════════════════════════════════════════════
# Satirda 'NAV-MUAF' geciyorsa o satirdaki isaretciler denetlenmez. Muaf
# sayisi ciktida RAPORLANIR -- sessiz muafiyet kapiyi korlestiren seyin ta
# kendisidir.
#
# Kullanim:
#   python Tools/check-navigation-loops.py
#   python Tools/check-navigation-loops.py <belge-koku> <kaynak-koku>  # negatif test

import importlib.util
import pathlib
import re
import sys
import unicodedata

DOCS_DEFAULT = "Docs"
ASSETS_DEFAULT = "Assets"
LINKS_GATE = "Tools/check-doc-links.py"

MUAF = "NAV-MUAF"

# Simgeler kaynakta bir kez tanimlanir; asagidaki desenler bunlari kullanir.
ILERI = "▶"     # siyah ileri ucgen
GERI = "◀"      # siyah geri ucgen
KLAVYE = "⌨"    # klavye

HEADING = re.compile(r"^(#{1,6})\s+(.*?)\s*$")
FENCE = re.compile(r"^\s*```")
# Satir basindaki bosluk ve alinti isaretleri. Tembel devam satirinda hicbiri
# olmayabilir; desen bos eslesmeye de izin verir.
QUOTE = re.compile(r"^[ \t>]*")

# Isaretci etiketleri. 'pre' simgeyi ve on vurguyu, 'mid' etiket ile iki nokta
# arasindaki vurguyu, 'post' iki noktadan sonraki vurguyu yakalar. Isaretci
# sayilma esigi (simge ya da vurgu) bu uc parcadan olculur.
ARA_RE = re.compile(
    r"(?P<pre>[*_\s" + ILERI + r"]{0,8})ARA\s+DURAK(?P<mid>[*_\s]{0,4}):(?P<post>[*_]{0,3})",
    re.IGNORECASE)
KODU_RE = re.compile(
    r"(?P<pre>[*_\s" + KLAVYE + r"]{0,8})KODU\s+A[CÇç](?P<mid>[*_\s]{0,4}):(?P<post>[*_]{0,3})",
    re.IGNORECASE)
DONUS_RE = re.compile(
    r"(?P<pre>[*_\s" + GERI + r"]{0,8})D[OÖö]N[UÜü][SŞş]"
    r"(?P<mid>[*_\s]{0,4}):(?P<post>[*_]{0,3})",
    re.IGNORECASE)
# NEDEN ve BAK isaretci DEGILDIR; yalniz bir govdenin nerede bittigini
# soylerler. Onlar olmadan ARA DURAK'in govdesi NEDEN cumlesini de yutardi.
NEDEN_RE = re.compile(
    r"(?P<pre>[*_\s]{0,8})NEDEN(?P<mid>[*_\s]{0,4}):(?P<post>[*_]{0,3})", re.IGNORECASE)
BAK_RE = re.compile(
    r"(?P<pre>[*_\s]{0,8})BAK(?P<mid>[*_\s]{0,4}):(?P<post>[*_]{0,3})", re.IGNORECASE)

ETIKETLER = (
    ("ARA", ARA_RE),
    ("KODU", KODU_RE),
    ("DONUS", DONUS_RE),
    ("NEDEN", NEDEN_RE),
    ("BAK", BAK_RE),
)
ISARETCI_TURLERI = ("ARA", "KODU", "DONUS")

# Govde en cok bu kadar parca yutar. Bos satir / etiket / baslik / cit zaten
# durdurur; bu ust sinir yalnizca bunlarin hicbiri gelmezse (bicimsiz yazilmis
# uzun bir paragraf) hasari sinirlar.
# ██ OLCULDU (Docs/, sinir 1'den 11'e cikarilip her adimda kac govdenin
# degistigi sayildi) ██
#     1 -> 2 : 14 govde degisti     4 -> 5 : 0
#     2 -> 3 : 11 govde degisti     5 -> 6 : 0
#     3 -> 4 :  4 govde degisti     6 -> 11: 0
# Yani bugunku en uzun govde 4 parca (kendi satiri + 3 devam satiri) ve
# 4'ten sonra hicbir sey degismiyor. 6 secildi: olcunun uzerinde iki parca pay,
# ama bicimsiz bir paragrafi sinirsiz yutmayacak kadar dar.
DEVAM_SINIRI = 6

# Markdown baglantisi: [metin](yol#capa) . [metin](#capa) . [metin](yol)
MDLINK = re.compile(
    r"\[[^\]]*\]\(\s*(?!https?:|mailto:)(?P<yol>[^)\s#]*)\s*(?:#(?P<capa>[^)\s]*))?\s*\)")
# Baglanti yoksa bolum adi tirnak icinde yazilir: «Birinci durak»
GUILLEMET = re.compile(r"«([^»]*)»")

TIRNAKLI = re.compile(r"`([^`]+)`")
OK_RE = re.compile(r"→|->")
# Yol gorunumu: ya '/' iceren bir yol, ya bilinen bir kod uzantisi. 'Battle.Tick'
# bilerek disarida: nokta iceren her ters tirnakli parcayi yol saymak, cozulemeyen
# her uye adini "dosya YOK" ihlaline cevirirdi.
KOD_UZANTILARI = "cs|asmdef|asmref|json|shader|cginc|hlsl|uss|uxml|unity|prefab|asset|meta|txt|md"
YOL_GORUNUMU = re.compile(
    r"(?:[A-Za-z0-9_.\-]+/)+[A-Za-z0-9_.\-]+\.[A-Za-z0-9_]+"
    r"|[A-Za-z0-9_.\-]+\.(?:" + KOD_UZANTILARI + r")",
    re.IGNORECASE)
YOL_KUYRUK = re.compile(r"[:#]\s*L?\d+(?:\s*-\s*\d+)?$")
UYE_ADI = re.compile(r"[A-Za-z_][A-Za-z0-9_]*(?:\.[A-Za-z_][A-Za-z0-9_]*)*")
TOKEN = re.compile(r"[A-Za-z_][A-Za-z0-9_]*")

# Uye denetimi yalniz metin dosyalarinda yapilabilir. Disaridakiler (png, dll)
# 'denetlenemedi' olarak SAYILIR; sessizce gecmek kapiyi kapsami konusunda
# yalanci yapardi.
METIN_UZANTILARI = {
    ".cs", ".asmdef", ".asmref", ".json", ".txt", ".md", ".xml", ".yaml", ".yml",
    ".shader", ".cginc", ".hlsl", ".uss", ".uxml", ".unity", ".prefab", ".asset",
    ".meta",
}

# MADDE 4: satir numarasinin butun lehceleri. Her bicim belgelerde ya da
# kardes kapilarda gorulmus olandir.
SATIR_NO = re.compile(
    r"\.[A-Za-z]{2,8}\s*[:#]\s*L?\d+"   # UnitLifecycle.cs:90 . .cs#L90
    r"|`\s*:\d+(?:-\d+)?\s*`"           # `:90` -- check-doc-code-refs.py'nin bildigi lehce
    r"|\bL\d+\b"                        # GitHub L90
    r"|\bsat[iı]r\s*\d+"                # satir 90 (aksanli yazimi da kumede)
    r"|\d+\s*\.\s*sat[iı]r",            # 90. satir
    re.IGNORECASE)


def slug(text):
    """GitHub baslik -> capa donusumu (bu repoda gecerli alt kume).

    ██ Tools/check-doc-links.py'den AYNEN kopyalandi ██ ve slug_uyumu() her
    kosumda kopyanin hala ayni oldugunu olcer.

    Turkce buyuk I (U+0130) kucultuldugunde 'i' + birlesen nokta uretir ve
    capayi bozar; bu yuzden onu once sadelestiriyoruz.
    """
    text = text.replace("İ", "I").replace("ı", "i")
    text = unicodedata.normalize("NFKD", text)
    text = "".join(c for c in text if not unicodedata.combining(c))
    text = text.lower()
    text = re.sub(r"[^\w\s-]", "", text)
    text = re.sub(r"[\s_]+", "-", text)
    return text.strip("-")


def yaz(text):
    """Konsolun kodlamasina sigmayan karakterleri kirmadan basar.

    Neden gerekli: Windows konsolu cogu kez cp1254 ve isaretci simgeleri o
    kumede yok. Ham 'print' bir UnicodeEncodeError ile kapiyi CALISMADAN
    oldururdu -- yani kapi ihlal buldugu anda kendi kendini susturur.
    """
    enc = getattr(sys.stdout, "encoding", None) or "ascii"
    try:
        text.encode(enc)
    except (UnicodeEncodeError, LookupError):
        text = text.encode(enc, "replace").decode(enc, "replace")
    print(text)


class Belgeler:
    """Denetlenen .md dosyalarinin kaynagi.

    Neden bir sinif: oz-sinama diske DOKUNMADAN kosmali. Diskteki bir dosyaya
    bagimli oz-sinama, o dosya tasindigi gun kapiyi da sessizce oldururdu
    (ayni gerekce Tools/check-doc-code-refs.py'de yazili). Gercek kosumda
    from_disk, oz-sinamada duz bir sozluk kullanilir. Yol cozumu de bu yuzden
    pathlib.resolve() ile degil duz metin uzerinde yapilir.
    """

    def __init__(self, files):
        self.files = {k.replace("\\", "/"): v for k, v in files.items()}
        self._lines = {}
        self._capalar = {}
        self._basliklar = {}

    @classmethod
    def from_disk(cls, root):
        files = {}
        for path in pathlib.Path(root).rglob("*.md"):
            files[path.as_posix()] = path.read_text(encoding="utf-8", errors="replace")
        return cls(files)

    def keys(self):
        return sorted(self.files)

    def lines(self, key):
        if key not in self._lines:
            self._lines[key] = self.files[key].split("\n")
        return self._lines[key]

    def capalar(self, key):
        """MADDE 1 ve 2'nin kullandigi capa kumesi -- cite BAKMAZ.

        check-doc-links.py'nin headings_of() fonksiyonuyla BIREBIR ayni kume.
        Uyum olcusu slug_uyumu() icinde her kosumda yapilir.
        """
        if key not in self._capalar:
            out = set()
            for line in self.lines(key):
                m = HEADING.match(line)
                if m:
                    out.add(slug(m.group(2)))
            self._capalar[key] = out
        return self._capalar[key]

    def basliklar(self, key):
        """MADDE 3'un kullandigi baslik listesi -- citi ATLAR.

        -> [(satir_indeksi, duzey, capa)]
        """
        if key not in self._basliklar:
            out = []
            in_fence = False
            for index, line in enumerate(self.lines(key)):
                if FENCE.match(line):
                    in_fence = not in_fence
                    continue
                if in_fence:
                    continue
                m = HEADING.match(line)
                if m:
                    out.append((index, len(m.group(1)), slug(m.group(2))))
            self._basliklar[key] = out
        return self._basliklar[key]

    def coz(self, base, rel):
        """Goreli .md yolunu anahtara cevirir; cozulemezse None.

        rel bos ise hedef kaynak dosyanin kendisidir (ayni dosyaya capa).
        """
        if not rel:
            return base
        parts = base.split("/")[:-1]
        for piece in rel.replace("\\", "/").split("/"):
            if piece in ("", "."):
                continue
            if piece == "..":
                if parts:
                    parts.pop()
            else:
                parts.append(piece)
        key = "/".join(parts)
        return key if key in self.files else None

    def bolum(self, key, capa):
        """Capanin bolum sinirini doner.

        -> ((bas, son), "TAMAM") . (None, "YOK") . (None, "COKLU")
        bas ve son satir INDEKSLERIDIR; son disaridadir (yarim acik aralik).
        """
        heads = self.basliklar(key)
        yerler = [i for i, (_, _, sl) in enumerate(heads) if sl == capa]
        if not yerler:
            return None, "YOK"
        if len(yerler) > 1:
            # Ayni capayi ureten iki baslik varsa bolum BELIRSIZDIR. Ilkini
            # sessizce secmek, denetlemedigimiz bir bolgeden hukum uretmek
            # olurdu; bu durum MADDE 3'te 'denetlenemedi' olarak sayilir.
            return None, "COKLU"
        k = yerler[0]
        index, level, _ = heads[k]
        son = len(self.lines(key))
        for j in range(k + 1, len(heads)):
            if heads[j][1] <= level:
                son = heads[j][0]
                break
        return (index + 1, son), "TAMAM"


class Kaynak:
    """Assets/ altindaki dosyalarin kaynagi. Belgeler ile ayni gerekce: sanal.

    Iceriksiz (None) bir deger "dosya var ama metin degil" demektir; uye
    denetimi orada yapilamaz ve SAYILIR.
    """

    def __init__(self, files):
        self.files = {k.replace("\\", "/"): v for k, v in files.items()}
        self._words = {}
        self._by_name = {}
        for key in self.files:
            self._by_name.setdefault(key.rsplit("/", 1)[-1], []).append(key)
        for value in self._by_name.values():
            value.sort()

    @classmethod
    def from_disk(cls, root):
        # Bu projede Assets/ agaci 201 dosya; hepsini bir kez okumak olculebilir
        # bir maliyet degil ve tembel okuma kodu iki kat karmasik yapardi.
        files = {}
        for path in pathlib.Path(root).rglob("*"):
            if not path.is_file():
                continue
            if path.suffix.lower() in METIN_UZANTILARI:
                files[path.as_posix()] = path.read_text(encoding="utf-8", errors="replace")
            else:
                files[path.as_posix()] = None
        return cls(files)

    def coz(self, yol):
        """-> (hedefler, durum) ; durum: TAM . YOL-YANLIS . YOK

        Sonek eslesmesi '/' sinirinda yapilir, yoksa 'View.cs' ile
        'UnitView.cs' birbirine karisirdi. Yol verilmis ama o yolda dosya
        yoksa ve ad baska bir yerde varsa, bu SESSIZ bir eslesme degil ayri
        bir durumdur (YOL-YANLIS) -- ayni ayrim check-doc-code-refs.py'de de
        var.
        """
        yol = yol.replace("\\", "/").lstrip("./")
        if yol in self.files:
            return [yol], "TAM"
        if "/" in yol:
            sonek = sorted(k for k in self.files if k.endswith("/" + yol))
            if sonek:
                return sonek, "TAM"
            ad = yol.rsplit("/", 1)[-1]
            baska = sorted(self._by_name.get(ad, []))
            if baska:
                return baska, "YOL-YANLIS"
            return [], "YOK"
        hedef = sorted(self._by_name.get(yol, []))
        if hedef:
            return hedef, "TAM"
        return [], "YOK"

    def kelimeler(self, key):
        """Dosyadaki tanimlayici kumesi; metin degilse None."""
        if self.files.get(key) is None:
            return None
        if key not in self._words:
            self._words[key] = set(TOKEN.findall(self.files[key]))
        return self._words[key]


class Isaretci(object):
    __slots__ = ("tur", "key", "index", "govde", "muaf")

    def __init__(self, tur, key, index, govde, muaf):
        self.tur = tur
        self.key = key
        self.index = index
        self.govde = govde
        self.muaf = muaf


def isaretci_mi(match):
    """Etiket gercekten bir isaretci mi: simge ya da vurgu tasiyor mu.

    Duz yazi icinde gecen 'donus:' sozcugu isaretci degildir. Esik olmadan
    kapi kendi kural belgesini denetlemeye kalkar.
    """
    cevre = match.group("pre") + match.group("mid") + match.group("post")
    if ILERI in cevre or GERI in cevre or KLAVYE in cevre:
        return True
    return any(c in "*_" for c in cevre)


def etiketleri_bul(govde):
    """Bir satirdaki butun etiketler: [(bas, son, tur, match)] -- sirali."""
    bulunan = []
    for tur, desen in ETIKETLER:
        for m in desen.finditer(govde):
            if isaretci_mi(m):
                bulunan.append((m.start(), m.end(), tur, m))
    bulunan.sort(key=lambda e: e[0])
    return bulunan


def isaretcileri_bul(docs):
    """Butun belgelerdeki isaretcileri toplar.

    Cit bloklari ATLANIR: ornek olarak gosterilen bir isaretci gercek degildir.
    Govde tembel devam satirlarindan devam eder (yukarida madde 3).
    """
    hepsi = []
    for key in docs.keys():
        lines = docs.lines(key)
        # Once her satirin cit durumu ve govdesi cikarilir; devam satirlarini
        # toplarken satirlara ILERI dogru bakmak gerekiyor.
        cit = []
        govdeler = []
        etiketler = []
        in_fence = False
        for line in lines:
            if FENCE.match(line):
                in_fence = not in_fence
                cit.append("SINIR")
                govdeler.append("")
                etiketler.append([])
                continue
            cit.append("ICI" if in_fence else "DIS")
            govde = QUOTE.sub("", line.rstrip())
            govdeler.append(govde)
            etiketler.append([] if in_fence else etiketleri_bul(govde))

        for index, line in enumerate(lines):
            if cit[index] != "DIS":
                continue
            muaf = MUAF in line
            satir_etiketleri = etiketler[index]
            for sira, (bas, son, tur, _) in enumerate(satir_etiketleri):
                if tur not in ISARETCI_TURLERI:
                    continue
                # Ayni satirdaki bir sonraki etiket govdeyi keser.
                kesim = (satir_etiketleri[sira + 1][0]
                         if sira + 1 < len(satir_etiketleri) else len(govdeler[index]))
                parcalar = [govdeler[index][son:max(son, kesim)]]
                # Yalniz satirin SON etiketi devam satiri yutabilir.
                if sira + 1 == len(satir_etiketleri):
                    j = index + 1
                    while j < len(lines) and len(parcalar) <= DEVAM_SINIRI:
                        if cit[j] != "DIS":
                            break
                        if not govdeler[j].strip():
                            break
                        if HEADING.match(lines[j]):
                            break
                        if etiketler[j]:
                            break
                        parcalar.append(govdeler[j])
                        j += 1
                govde = " ".join(p.strip() for p in parcalar if p.strip())

                if tur == "DONUS":
                    geri = GERI in satir_etiketleri[sira][3].group("pre")
                    tur = "DONUS-GERI" if geri else "DONUS-ICE"
                hepsi.append(Isaretci(tur, key, index, govde, muaf))
    return hepsi


def baglanti_bul(govde):
    """-> (yol, capa) . capa None olabilir . hic baglanti yoksa None.

    Birden cok baglanti varsa CAPASI OLAN ilki yeglenir: bicimde metin
    parcasi da bir baglanti olabiliyor ve asil hedef capayi tasiyandir.
    """
    ilk = None
    for m in MDLINK.finditer(govde):
        cift = (m.group("yol") or "", m.group("capa"))
        if ilk is None:
            ilk = cift
        if cift[1]:
            return cift
    return ilk


def donus_hedefi(govde):
    """DONUS satirinin hedefi.

    -> (yol, capa, "BAG") . (None, ad, "AD") . (None, None, "YOK")
    'AD' bicimindeki capa henuz slug'lanmamis BOLUM ADIDIR; onek eslesmesi
    audit() icinde yapilir.
    """
    bag = baglanti_bul(govde)
    if bag is not None and bag[1]:
        return bag[0], bag[1], "BAG"
    ad = GUILLEMET.search(govde)
    if ad is not None and ad.group(1).strip():
        return None, ad.group(1).strip(), "AD"
    if bag is not None:
        return bag[0], None, "BAG"
    return None, None, "YOK"


def ad_capaya(ad, capalar):
    """Bolum adini capaya cevirir.

    -> (capa, "TAM") . (capa, "ONEK") . (None, "BELIRSIZ") . (None, "YOK")
    Onek eslesmesi TIRE SINIRINDA yapilir: 'ikinci-fatura' ile
    'ikinci-fatura-bir-enum-...' eslesir, 'ikinci' ile eslesmez. Sinir olmasa
    her kisa ad rastgele bir bolume baglanirdi.
    """
    hedef = slug(ad)
    if not hedef:
        return None, "YOK"
    if hedef in capalar:
        return hedef, "TAM"
    adaylar = sorted(c for c in capalar if c.startswith(hedef + "-"))
    if len(adaylar) == 1:
        return adaylar[0], "ONEK"
    if len(adaylar) > 1:
        return None, "BELIRSIZ"
    return None, "YOK"


def kod_hedefleri(govde):
    """KODU AC govdesinden (yol, uye) ciftlerini cikarir.

    ██ SAHIP KURALI: bir uyenin sahibi, oktan ONCE anilan EN SON yoldur. ██
    Ayni gerekce check-doc-code-refs.py'de olculdu ve yazili: ileriye degil
    GERIYE taramak gerekiyor, cunku bir satir once dosyayi sonra uyesini
    yazar. Somut ornek (02-assembly-duvari.md:129-131):
        `...AttackProfile.cs` -> `namespace` satiri; sonra yanindaki
        `...GridStrategy.Combat.asmdef` -> `references` dizisi
    Ileri-son kural 'namespace' uyesini asmdef'e baglardi.

    Uye YALNIZ ters tirnak icinde aranir. Olculdu: ok'tan sonraki ciplak
    sozcugu uye saymak yukaridaki satirda 'dosyanin' sozcugunu uye sanip
    SAHTE ihlal uretti (ilk surum).
    """
    yollar = []
    adlar = []
    for m in TIRNAKLI.finditer(govde):
        parca = m.group(1).strip()
        temiz = YOL_KUYRUK.sub("", parca)
        if YOL_GORUNUMU.fullmatch(temiz):
            yollar.append((m.start(), temiz))
        elif UYE_ADI.fullmatch(parca):
            adlar.append((m.start(), parca))

    if not yollar:
        # Ters tirnaksiz yazilmis yol: yalnizca hicbir ters tirnakli yol
        # yokken bakilir, yoksa duz yazidaki bir dosya adi asil hedefi ezerdi.
        for m in YOL_GORUNUMU.finditer(govde):
            temiz = YOL_KUYRUK.sub("", m.group(0))
            yollar.append((m.start(), temiz))
            break
    if not yollar:
        return []

    atanan = {}
    for ok in OK_RE.finditer(govde):
        sahip = None
        for pos, parca in yollar:
            if pos < ok.start():
                sahip = pos
            else:
                break
        if sahip is None:
            continue
        sonraki_yol = min([pos for pos, _ in yollar if pos > ok.end()] or [len(govde)])
        for pos, ad in adlar:
            if ok.end() <= pos < sonraki_yol:
                atanan.setdefault(sahip, ad)
                break

    return [(parca, atanan.get(pos)) for pos, parca in yollar]


def uye_adi(uye):
    """`Battle.AddUnit` -> 'AddUnit' . `SetState(UnitState)` -> 'SetState'."""
    if not uye:
        return None
    m = UYE_ADI.search(uye.strip().strip("`"))
    if not m:
        return None
    return m.group(0).split(".")[-1]


def bos_sayac():
    return {
        "isaretci": 0, "muaf": 0,
        "ara": 0, "kodu": 0, "donus_geri": 0, "donus_ice": 0,
        # MADDE 1
        "m1": 0, "m1_bagsiz": 0, "m1_capasiz": 0, "m1_hedef_yok": 0, "m1_capa_yok": 0,
        # MADDE 2
        "m2": 0, "m2_kendi": 0, "m2_capraz": 0,
        "m2_ad_tam": 0, "m2_ad_onek": 0, "m2_ad_belirsiz": 0,
        "m2_bagsiz": 0, "m2_capasiz": 0, "m2_hedef_yok": 0, "m2_capa_yok": 0,
        # MADDE 3
        "m3": 0, "m3_yok": 0, "m3_baska": 0, "m3_denetlenemedi": 0,
        "m3_ned_capa": 0, "m3_ned_coklu": 0,
        # MADDE 4
        "m4": 0, "m4_ihlal": 0,
        # MADDE 5
        "m5_dosya": 0, "m5_dosya_yok": 0, "m5_yol_yanlis": 0, "m5_coklu": 0,
        "m5_uye": 0, "m5_uye_yok": 0, "m5_uye_yazilmamis": 0, "m5_metin_degil": 0,
        "m5_yolsuz": 0,
        # MADDE 6
        "m6_oksuz": 0,
    }


def audit(docs, source):
    """-> (ihlaller, sayaclar) ; ihlal: (belge, satir, tur, mesaj)."""
    problems = []
    stat = bos_sayac()

    hepsi = isaretcileri_bul(docs)
    aralar = []
    donusler = []
    kodlar = []

    for isaret in hepsi:
        stat["isaretci"] += 1
        if isaret.muaf:
            stat["muaf"] += 1
            continue
        if isaret.tur == "ARA":
            stat["ara"] += 1
            aralar.append(isaret)
        elif isaret.tur == "KODU":
            stat["kodu"] += 1
            kodlar.append(isaret)
        elif isaret.tur == "DONUS-GERI":
            stat["donus_geri"] += 1
            donusler.append(isaret)
        else:
            stat["donus_ice"] += 1

    # Bagimsiz donus isaretcilerinin hedef DOSYASI onceden cozulur: MADDE 3
    # "geri KAYNAGA mi isaret ediyor" diye sorar, yalniz "bir donus satiri var
    # mi" diye degil.
    donus_kaynagi = {}
    for isaret in donusler:
        yol, _, _ = donus_hedefi(isaret.govde)
        donus_kaynagi[id(isaret)] = docs.coz(isaret.key, yol or "")

    sahiplenen = set()

    # ── MADDE 1 + MADDE 3 ────────────────────────────────────────────────
    for isaret in aralar:
        yer = (isaret.key, isaret.index + 1)
        bag = baglanti_bul(isaret.govde)
        if bag is None:
            problems.append(yer + ("ARA-BAG-YOK",
                                   "ARA DURAK satirinda cozulebilir baglanti yok"))
            stat["m1_bagsiz"] += 1
            continue
        yol, capa = bag
        if not capa:
            problems.append(yer + ("ARA-CAPASIZ",
                                   "ARA DURAK capasiz: %s (okuyucu dosyanin basina duser)"
                                   % (yol or "(ayni dosya)")))
            stat["m1_capasiz"] += 1
            continue

        stat["m1"] += 1
        hedef = docs.coz(isaret.key, yol)
        if hedef is None:
            problems.append(yer + ("ARA-HEDEF-YOK", "ARA DURAK hedefi YOK: %s" % yol))
            stat["m1_hedef_yok"] += 1
            continue
        if slug(capa) not in docs.capalar(hedef):
            problems.append(yer + ("ARA-CAPA-YOK",
                                   "ARA DURAK capasi YOK: %s#%s" % (yol or hedef, capa)))
            stat["m1_capa_yok"] += 1
            continue

        # MADDE 3: karsilik. Yalniz hedefi VE capasi cozulmus isaretciler icin
        # sorulabilir; cozulemeyen zaten yukarida ihlal olarak yazildi.
        sinir, durum = docs.bolum(hedef, slug(capa))
        if durum != "TAMAM":
            stat["m3_denetlenemedi"] += 1
            if durum == "COKLU":
                stat["m3_ned_coklu"] += 1
            else:
                stat["m3_ned_capa"] += 1
            continue

        stat["m3"] += 1
        bas, son = sinir
        icerdekiler = [d for d in donusler if d.key == hedef and bas <= d.index < son]
        for d in icerdekiler:
            sahiplenen.add(id(d))
        if not icerdekiler:
            problems.append(yer + ("KARSILIK-YOK",
                                   "KARSILIKSIZ: %s#%s bolumunde geri donus "
                                   "isaretcisi yok" % (yol or hedef, capa)))
            stat["m3_yok"] += 1
            continue
        if not any(donus_kaynagi.get(id(d)) == isaret.key for d in icerdekiler):
            nereye = sorted(set(donus_kaynagi.get(id(d)) or "(cozulemedi)"
                                for d in icerdekiler))
            problems.append(yer + ("KARSILIK-BASKA",
                                   "KARSILIK BASKA DOSYAYA: %s#%s bolumundeki donus "
                                   "%s diyor, buraya (%s) donmuyor"
                                   % (yol or hedef, capa, " . ".join(nereye),
                                      isaret.key)))
            stat["m3_baska"] += 1

    # ── MADDE 2: butun DONUS satirlarinin capasi ─────────────────────────
    for isaret in hepsi:
        if isaret.muaf or not isaret.tur.startswith("DONUS"):
            continue
        yer = (isaret.key, isaret.index + 1)
        yol, capa, bicim = donus_hedefi(isaret.govde)
        if bicim == "YOK":
            problems.append(yer + ("DONUS-BAG-YOK",
                                   "DONUS satirinda ne baglanti ne bolum adi var"))
            stat["m2_bagsiz"] += 1
            continue
        if not capa:
            problems.append(yer + ("DONUS-CAPASIZ",
                                   "DONUS capasiz: %s (okuyucu geldigi bolume "
                                   "degil dosyanin basina doner)" % (yol or "")))
            stat["m2_capasiz"] += 1
            continue

        stat["m2"] += 1
        if yol:
            stat["m2_capraz"] += 1
        else:
            stat["m2_kendi"] += 1

        hedef = docs.coz(isaret.key, yol or "")
        if hedef is None:
            problems.append(yer + ("DONUS-HEDEF-YOK", "DONUS hedefi YOK: %s" % yol))
            stat["m2_hedef_yok"] += 1
            continue

        nerede = "kendi dosyasinda" if not yol else hedef
        if bicim == "AD":
            _, durum = ad_capaya(capa, docs.capalar(hedef))
            if durum == "TAM":
                stat["m2_ad_tam"] += 1
            elif durum == "ONEK":
                stat["m2_ad_onek"] += 1
            elif durum == "BELIRSIZ":
                stat["m2_ad_belirsiz"] += 1
                problems.append(yer + ("DONUS-AD-BELIRSIZ",
                                       "DONUS bolum adi BELIRSIZ: «%s» %s birden "
                                       "cok baslikla eslesiyor" % (capa, nerede)))
            else:
                problems.append(yer + ("DONUS-CAPA-YOK",
                                       "DONUS bolumu %s YOK: «%s»" % (nerede, capa)))
                stat["m2_capa_yok"] += 1
            continue

        if slug(capa) not in docs.capalar(hedef):
            problems.append(yer + ("DONUS-CAPA-YOK",
                                   "DONUS capasi %s YOK: #%s" % (nerede, capa)))
            stat["m2_capa_yok"] += 1

    # ── MADDE 4 + MADDE 5 ────────────────────────────────────────────────
    for isaret in kodlar:
        yer = (isaret.key, isaret.index + 1)

        stat["m4"] += 1
        satir_no = SATIR_NO.search(isaret.govde)
        if satir_no:
            problems.append(yer + ("KODU-SATIR-NO",
                                   "KODU AC SATIR NUMARASI TASIYOR: %s -- uye adi "
                                   "kaymaz, satir numarasi kayar"
                                   % satir_no.group(0).strip()))
            stat["m4_ihlal"] += 1

        ciftler = kod_hedefleri(isaret.govde)
        if not ciftler:
            problems.append(yer + ("KODU-YOL-YOK",
                                   "KODU AC satirinda dosya yolu yok"))
            stat["m5_yolsuz"] += 1
            continue

        for yol, uye in ciftler:
            stat["m5_dosya"] += 1
            hedefler, durum = source.coz(yol)
            if durum == "YOK":
                problems.append(yer + ("KODU-YOL-YOK", "KODU AC dosyasi YOK: %s" % yol))
                stat["m5_dosya_yok"] += 1
                continue
            if durum == "YOL-YANLIS":
                problems.append(yer + ("KODU-YOL-YANLIS",
                                       "KODU AC yolu YANLIS: %s (gercek yeri: %s)"
                                       % (yol, " . ".join(hedefler))))
                stat["m5_yol_yanlis"] += 1
                continue
            if len(hedefler) > 1:
                problems.append(yer + ("KODU-AD-COKLU",
                                       "KODU AC adi COKLU: %s -> %s"
                                       % (yol, " . ".join(hedefler))))
                stat["m5_coklu"] += 1
                continue

            ad = uye_adi(uye)
            if not ad:
                # Uye yazilmamis bir isaretci IHLAL DEGIL: bicim uyeyi ister ama
                # butun bir dosyaya isaret etmek de mesru olabilir (bir asmdef'in
                # tamamini acmak gibi). Denetlenemeyen bu durum SAYILIR.
                stat["m5_uye_yazilmamis"] += 1
                continue

            kelimeler = source.kelimeler(hedefler[0])
            if kelimeler is None:
                stat["m5_metin_degil"] += 1
                continue

            stat["m5_uye"] += 1
            if ad not in kelimeler:
                problems.append(yer + ("KODU-UYE-YOK",
                                       "KODU AC uyesi dosyada GECMIYOR: %s -> %s"
                                       % (hedefler[0], ad)))
                stat["m5_uye_yok"] += 1

    # ── MADDE 6: oksuz donus ─────────────────────────────────────────────
    # IHLAL DEGIL, ayri bir sayac. Bir bolume birden fazla yerden gelinebilir
    # ve donus satiri elle de yazilmis olabilir.
    for isaret in donusler:
        if id(isaret) not in sahiplenen:
            stat["m6_oksuz"] += 1

    return problems, stat


# ── OZ-SINAMA ORNEKLERI ──────────────────────────────────────────────────
# Hepsi bu dosyanin icinde ve SANAL bir sozluk uzerinde kosar: diske
# dokunmayan bir oz-sinama, projedeki hicbir dosya tasinsa da olmez.

SELF_CS = {
    "Assets/Sanal/Ornek.cs": "\n".join([
        "namespace Sanal",
        "{",
        "    public static class Ornek",
        "    {",
        "        public static void Calistir()",
        "        {",
        "        }",
        "    }",
        "}",
    ]),
    "Assets/Sanal/Sanal.Ornek.asmdef": "{ \"name\": \"Sanal.Ornek\", \"references\": [] }",
    "Assets/Sanal/resim.png": None,
}

# ██ IYI ORNEK: TAM BIR DONGU ██
#     A.md «Birinci durak: sayac konusuyor»  --ARA DURAK-->  B.md «Ikinci durak»
#     B.md «Ikinci durak»                    --geri DONUS--> A.md «Birinci durak...»
# Bicimin her esnek varyasyonu bilerek FARKLI bir satirda kullanildi; ornek
# yalniz "ihlal yok" demiyor, ayristiricinin neyi kabul ettigini de kanitliyor:
#   - alinti on eki ve fazladan bosluk                   (ARA DURAK satiri)
#   - alintinin TEMBEL devam satiri, '>' yok             (ARA DURAK alt satiri)
#   - iki nokta vurgunun DISINDA                         (ARA DURAK alt satiri)
#   - tek yildizli vurgu, '->' oku, '>' yok              (birinci KODU AC)
#   - ayni satirda iki etiket (BAK ve DONUS)             (birinci KODU AC alti)
#   - COK SATIRA yayilan govde: ikinci hedef ve uyesi
#     bir sonraki satirda                                (ikinci KODU AC)
#   - metin OLMAYAN bir dosyaya isaret (png)             (ikinci KODU AC)
#   - uc yildizli vurgu, Turkce yazim, satir sonu boslugu (geri DONUS)
# ██ Geri donus satiri bilerek bir ALT BASLIGIN altina konuldu ██: bolum
# siniri "bir sonraki HERHANGI baslik" olsaydi bu yari KAPI BOZUK derdi.
# ██ Bolum adlarinin ikisi de bilerek farkli ██: «Birinci durak» kisaltilmis
# (ONEK eslesmesi), «Alt bolum» tam (TAM eslesme).
SELF_IYI = {
    "iyi/A.md": "\n".join([
        "# A basligi",
        "",
        "## Birinci durak: sayac konusuyor",
        "",
        "Sayac konusuyor.",
        "",
        ">   **" + ILERI + " ARA DURAK:**   [B.md](B.md#ikinci-durak)",
        "> **NEDEN:** olcunun sahibi orada.",
        "**DONUS**: bu dosyanin",
        "[«Birinci durak: sayac konusuyor»](#birinci-durak-sayac-konusuyor) bolumu   ",
        "",
        "### Alt bolum",
        "",
        "*" + KLAVYE + " KODU AC:* `Assets/Sanal/Ornek.cs` -> `Calistir`",
        "*BAK:* tek giris noktasi · ***DONUS:*** bu dosyanin «Birinci durak» bolumu",
        "",
        "> **" + KLAVYE + " KODU AÇ:** `Assets/Sanal/Sanal.Ornek.asmdef` → `references`",
        "> dizisi; sonra yanindaki `Assets/Sanal/resim.png` → `Sprite`, sonra butun",
        "> `Assets/Sanal/Ornek.cs` dosyasi",
        "> **BAK:** uc hedef, ucu de AYRI sayilir",
        "> **DONUS:** bu dosyanin «Alt bolum» bolumu",
        "",
    ]),
    "iyi/B.md": "\n".join([
        "# B basligi",
        "",
        "## Ikinci durak",
        "",
        "Duvarin ucuncu faturasi.",
        "",
        "### Daha derin bir alt baslik",
        "",
        ">  ***" + GERI + " DÖNÜŞ:***  [A.md](A.md#birinci-durak-sayac-konusuyor) "
        "— olcunun sahibinden geldiysen   ",
        "",
        "## Ucuncu durak",
        "",
        "Ilgisiz.",
        "",
    ]),
}

# ██ KOTU ORNEK: ALTI MADDENIN HER BIRI AYRI AYRI ██
SELF_KOTU = {
    "kotu/K.md": "\n".join([
        "# K basligi",
        "",
        "## Kaynak bolum",
        "",
        "> **" + ILERI + " ARA DURAK:** [olmayan.md](olmayan.md#capa)",
        "> **NEDEN:** MADDE 1 -- hedef dosya yok.",
        "> **DONUS:** bu dosyanin [«Kaynak bolum»](#kaynak-bolum) bolumu",
        "",
        "> **" + ILERI + " ARA DURAK:** [H.md](H.md#olmayan-capa)",
        "> **NEDEN:** MADDE 1 -- capa hedefte yok.",
        "> **DONUS:** bu dosyanin [«Kaynak bolum»](#kaynak-bolum) bolumu",
        "",
        "> **" + ILERI + " ARA DURAK:** [H.md](H.md#karsiliksiz-bolum)",
        "> **NEDEN:** MADDE 3 -- gidilen bolum geri donmeyi bilmiyor.",
        "> **DONUS:** bu dosyanin [«Kaynak bolum»](#kaynak-bolum) bolumu",
        "",
        "> **" + ILERI + " ARA DURAK:** [H.md](H.md#karsilikli-bolum)",
        "> **NEDEN:** MADDE 2 -- donus capasi kaynak dosyada yok.",
        "> **DONUS:** bu dosyanin [«Olmayan bolum»](#olmayan-bolum) bolumu",
        "",
        "> **" + KLAVYE + " KODU AC:** `Assets/Sanal/Ornek.cs:5` -> `Calistir`",
        "> **BAK:** MADDE 4 · **DONUS:** bu dosyanin «Kaynak bolum» bolumu",
        "",
        "> **" + KLAVYE + " KODU AC:** `Assets/Sanal/Olmayan.cs` -> `Calistir`",
        "> **BAK:** MADDE 5 · **DONUS:** bu dosyanin «Kaynak bolum» bolumu",
        "",
        "> **" + KLAVYE + " KODU AC:** `Assets/Sanal/Ornek.cs` -> `OlmayanUye`",
        "> **BAK:** MADDE 5 · **DONUS:** bu dosyanin «Belirsiz» bolumu",
        "",
        "## Belirsiz bolum bir",
        "",
        "Bos.",
        "",
        "## Belirsiz bolum iki",
        "",
        "Bos.",
        "",
        "## Oksuz bolum",
        "",
        "> **" + GERI + " DONUS:** [H.md](H.md#karsilikli-bolum) — MADDE 6: "
        "buraya kimse isaret etmiyor",
        "",
    ]),
    "kotu/H.md": "\n".join([
        "# H basligi",
        "",
        "## Karsiliksiz bolum",
        "",
        "Burada hicbir geri donus isaretcisi yok.",
        "",
        "## Karsilikli bolum",
        "",
        "> **" + GERI + " DONUS:** [K.md](K.md#kaynak-bolum) — K'dan geldiysen",
        "",
    ]),
}

# Muafiyet ve cit: ikisi de "kapi kendi kural belgesini denetlemesin" icin var.
SELF_MUAF = {
    "muaf/M.md": "\n".join([
        "# M basligi",
        "",
        "## Muaf bolum",
        "",
        "> **" + ILERI + " ARA DURAK:** [yok.md](yok.md#yok) <!-- NAV-MUAF -->",
        "",
        "Asagidaki blok bir ORNEKTIR, gercek isaretci degildir:",
        "",
        "```text",
        "> **" + ILERI + " ARA DURAK:** [hic.md](hic.md#hic)",
        "> **" + GERI + " DONUS:** [hic.md](hic.md#hic)",
        "```",
        "",
        "Duz yazida gecen bir donus: sozcugu de isaretci degildir.",
        "",
    ]),
}


def self_check():
    """Ayristiricinin CALISTIGINI once kendi uzerinde kanitlar.

    ██ IKI YARI DA ZORUNLU ██ Yalniz iyi ornegi sinayan bir oz-sinama,
    audit() bosa dusse (her zaman 0 ihlal donse) bile GECERDI. Bu projede bir
    kapi tam olarak bu yuzden dort kez yanlislikla "temiz" dedi, ve bugun
    baska bir kapinin oz-sinamasi tam olarak bu yuzden bosa dustu.
    """
    source = Kaynak(SELF_CS)

    # ── BILINEN-IYI: tam bir dongu, SIFIR ihlal + beklenen sayaclar ──────
    iyi, istat = audit(Belgeler(SELF_IYI), source)
    if iyi:
        return "bilinen-iyi ornek ihlal uretti: %s -- %s" % (iyi[0][2], iyi[0][3])
    beklenen = {
        "isaretci": 7, "ara": 1, "kodu": 2, "donus_geri": 1, "donus_ice": 3,
        "m1": 1, "m1_hedef_yok": 0, "m1_capa_yok": 0,
        "m2": 4, "m2_kendi": 3, "m2_capraz": 1,
        "m2_ad_tam": 1, "m2_ad_onek": 1, "m2_ad_belirsiz": 0, "m2_capa_yok": 0,
        "m3": 1, "m3_yok": 0, "m3_baska": 0, "m3_denetlenemedi": 0,
        "m4": 2, "m4_ihlal": 0,
        "m5_dosya": 4, "m5_uye": 2, "m5_uye_yok": 0, "m5_metin_degil": 1,
        "m5_uye_yazilmamis": 1,
        "m6_oksuz": 0,
    }
    for anahtar, deger in sorted(beklenen.items()):
        if istat[anahtar] != deger:
            return ("bilinen-iyi ornekte sayac yanlis: %s=%d, %d bekleniyordu "
                    "-- esnek ayristiricinin bir varyasyonu bosa dusmus olabilir"
                    % (anahtar, istat[anahtar], deger))

    # ── BILINEN-KOTU: alti maddenin HER BIRI ayri ayri ───────────────────
    kotu, kstat = audit(Belgeler(SELF_KOTU), source)
    turler = {}
    for _, _, tur, _ in kotu:
        turler[tur] = turler.get(tur, 0) + 1

    madde_turleri = (
        ("MADDE 1 ara durak hedefi", ("ARA-HEDEF-YOK", "ARA-CAPA-YOK")),
        ("MADDE 2 donus capasi", ("DONUS-CAPA-YOK", "DONUS-AD-BELIRSIZ")),
        ("MADDE 3 karsilik", ("KARSILIK-YOK",)),
        ("MADDE 4 satir numarasi", ("KODU-SATIR-NO",)),
        ("MADDE 5 kod hedefi", ("KODU-YOL-YOK", "KODU-UYE-YOK")),
    )
    for baslik, isteneler in madde_turleri:
        for istenen in isteneler:
            if istenen not in turler:
                return ("bilinen-kotu ornekte %s YAKALANMADI (%s) -- kapi bu "
                        "maddede kor (yakalananlar: %s)"
                        % (baslik, istenen, ", ".join(sorted(turler)) or "hicbiri"))

    kotu_beklenen = {
        "ARA-HEDEF-YOK": 1, "ARA-CAPA-YOK": 1, "DONUS-CAPA-YOK": 1,
        "DONUS-AD-BELIRSIZ": 1, "KARSILIK-YOK": 1, "KODU-SATIR-NO": 1,
        "KODU-YOL-YOK": 1, "KODU-UYE-YOK": 1,
    }
    if turler != kotu_beklenen:
        return ("bilinen-kotu ornekte ihlal dagilimi yanlis: %s, beklenen %s"
                % (sorted(turler.items()), sorted(kotu_beklenen.items())))

    # ██ MADDE 6 ASIL SINAMASI ██ oksuz donus IHLAL DEGILDIR, sayilan bir
    # bosluktur. Bu sinama olmadan kapi "sahiplenmedigim her donus ihlaldir"
    # diyen bir kapiya donusur ve dogru yazilmis belgeleri kizartirdi.
    if kstat["m6_oksuz"] != 1:
        return ("MADDE 6: oksuz donus sayaci yanlis (m6_oksuz=%d, 1 bekleniyordu)"
                % kstat["m6_oksuz"])
    # Karsiligi bulunan ARA DURAK ihlal uretmemeli: 4. ARA DURAK'in hedefi olan
    # H.md#karsilikli-bolum K.md'ye geri donuyor ve KARSILIK ihlali TAM 1 olmali.
    if kstat["m3"] != 2:
        return ("bilinen-kotu ornekte 2 karsilik denetimi bekleniyordu, %d oldu "
                "-- hedefi cozulmus ARA DURAK sayisi tutmuyor" % kstat["m3"])

    # ── MUAFIYET VE CIT ──────────────────────────────────────────────────
    muaf, mstat = audit(Belgeler(SELF_MUAF), source)
    if muaf:
        return "muafiyet/cit calismadi: %s -- %s" % (muaf[0][2], muaf[0][3])
    if mstat["muaf"] != 1:
        return "muafiyet sayaci yanlis: 1 bekleniyordu, %d cikti" % mstat["muaf"]
    if mstat["isaretci"] != 1:
        return ("cit icindeki ORNEK ya da duz yazidaki isaretciler sayilmis "
                "(isaretci=%d, 1 bekleniyordu)" % mstat["isaretci"])

    return None


def slug_uyumu(gate_path, docs):
    """check-doc-links.py ile capa kuralinin AYNI oldugunu olcer.

    -> (durum, mesaj) ; durum: TAMAM . ATLANDI . FARK

    Iki katman: once slug() ciktilari bir sinama demeti uzerinde, sonra her
    gercek .md dosyasinin TAM CAPA KUMESI. Ikincisi olmadan kopya slug()
    dogru olsa bile baslik TOPLAMA kurali ayrisabilirdi (cit, duzey, bosluk).

    Kardes kapi yoksa ATLANIR ve bu ciktida YAZILIR: kopya kendi kendine
    yeterlidir, ama uyum artik OLCULMEMISTIR ve bunu gizlemek yanlis olurdu.
    """
    path = pathlib.Path(gate_path)
    if not path.is_file():
        return "ATLANDI", "kardes kapi bulunamadi -> %s" % gate_path

    spec = importlib.util.spec_from_file_location("kardes_kapi", str(path))
    if spec is None or spec.loader is None:
        return "ATLANDI", "kardes kapi yuklenemedi -> %s" % gate_path
    modul = importlib.util.module_from_spec(spec)
    try:
        spec.loader.exec_module(modul)
    except Exception as hata:                                   # noqa: BLE001
        return "ATLANDI", "kardes kapi yuklenemedi (%s)" % hata
    if not hasattr(modul, "slug") or not hasattr(modul, "headings_of"):
        return "ATLANDI", "kardes kapida slug()/headings_of() yok"

    demet = [
        "Bir askerin düşüşü — olayın dört durağı",
        "Birinci durak: sayaç konuşuyor",
        "Üç ayrı şey, üç ayrı iş",
        "Duvarın ürünü: `Battle` var olmak zorunda",
        "③ Bir abone FIRLARSA — sökülme değil",
        "A — yeni bir tip yazıyorum, nereye koyacağım?",
        "İkinci fatura: bir enum, sahibinin üretemediği",
        "ı ve İ tuzağı",
        "   bosluk   testi   ",
        "alt_cizgi ve-tire",
        "",
        "###",
    ]
    for metin in demet:
        if slug(metin) != modul.slug(metin):
            return "FARK", ("slug ayrisiyor: %r -> bizde %r, kardeste %r"
                            % (metin, slug(metin), modul.slug(metin)))

    for key in docs.keys():
        bizim = docs.capalar(key)
        onun = modul.headings_of(pathlib.Path(key))
        if bizim != onun:
            eksik = sorted(onun - bizim)[:3]
            fazla = sorted(bizim - onun)[:3]
            return "FARK", ("capa kumesi ayrisiyor: %s (bizde eksik: %s . bizde "
                            "fazla: %s)" % (key, eksik, fazla))

    return "TAMAM", "slug() ve capa kumesi %s ile birebir ayni" % gate_path


def main(argv):
    docs_root = pathlib.Path(argv[1] if len(argv) > 1 else DOCS_DEFAULT)
    assets_root = pathlib.Path(argv[2] if len(argv) > 2 else ASSETS_DEFAULT)

    broken = self_check()
    if broken is not None:
        yaz("KAPI BOZUK: %s" % broken)
        return 2

    if not docs_root.is_dir():
        yaz("KAPI BOZUK: belge koku bulunamadi -> %s" % docs_root)
        return 2
    if not assets_root.is_dir():
        yaz("KAPI BOZUK: kaynak koku bulunamadi -> %s" % assets_root)
        return 2

    docs = Belgeler.from_disk(docs_root)
    if not docs.files:
        yaz("KAPI BOZUK: belge kokunde tek bir .md yok -> %s" % docs_root)
        return 2
    source = Kaynak.from_disk(assets_root)
    if not source.files:
        yaz("KAPI BOZUK: kaynak kokunde tek bir dosya yok -> %s" % assets_root)
        return 2

    uyum, uyum_mesaji = slug_uyumu(LINKS_GATE, docs)
    if uyum == "FARK":
        # Iki kapi ayni slug kuralini kullanmiyorsa biri otekinin temiz dedigine
        # ihlal der. Bu, ayri ayri dogru olan iki kapinin BIRLIKTE yalan
        # soylemesidir; kapi burada durur.
        yaz("KAPI BOZUK: %s" % uyum_mesaji)
        return 2

    problems, stat = audit(docs, source)

    for key, number, _, text in problems:
        yaz("%s:%d\n    %s" % (key, number, text))

    yaz("")
    yaz("belge: %d  Assets dosyasi: %d  isaretci: %d  (muaf: %d)"
        % (len(docs.files), len(source.files), stat["isaretci"], stat["muaf"]))
    yaz("  ARA DURAK %d  .  geri DONUS %d  .  blok ici DONUS %d  .  KODU AC %d"
        % (stat["ara"], stat["donus_geri"], stat["donus_ice"], stat["kodu"]))
    yaz("SLUG UYUMU      : %s -- %s" % (uyum, uyum_mesaji))
    yaz("")
    yaz("MADDE 1 ara durak hedefi : %d denetlendi . %d hedef YOK . %d capa YOK"
        % (stat["m1"], stat["m1_hedef_yok"], stat["m1_capa_yok"]))
    yaz("                           %d baglantisiz . %d capasiz (ikisi de IHLAL "
        "yazildi, denetime giremedi)" % (stat["m1_bagsiz"], stat["m1_capasiz"]))
    yaz("MADDE 2 donus capasi     : %d denetlendi (kendi dosyasina %d . capraz %d)"
        % (stat["m2"], stat["m2_kendi"], stat["m2_capraz"]))
    yaz("                           bolum adiyla («...»): %d TAM eslesme . %d "
        "ONEK eslesmesi . %d BELIRSIZ"
        % (stat["m2_ad_tam"], stat["m2_ad_onek"], stat["m2_ad_belirsiz"]))
    yaz("                           %d hedef YOK . %d capa YOK . %d baglantisiz "
        ". %d capasiz"
        % (stat["m2_hedef_yok"], stat["m2_capa_yok"], stat["m2_bagsiz"],
           stat["m2_capasiz"]))
    yaz("MADDE 3 karsilik         : %d denetlendi . %d KARSILIKSIZ . %d BASKA "
        "dosyaya donuyor" % (stat["m3"], stat["m3_yok"], stat["m3_baska"]))
    yaz("                           %d DENETLENEMEDI (%d capanin bolumu yok "
        "(cit ici baslik) . %d capa dosyada COKLU)"
        % (stat["m3_denetlenemedi"], stat["m3_ned_capa"], stat["m3_ned_coklu"]))
    yaz("MADDE 4 satir numarasi   : %d isaretci denetlendi . %d SATIR NUMARASI "
        "TASIYOR" % (stat["m4"], stat["m4_ihlal"]))
    yaz("MADDE 5 kod hedefi       : %d dosya denetlendi . %d dosya YOK . %d yol "
        "YANLIS . %d ad COKLU . %d isaretcide yol yok"
        % (stat["m5_dosya"], stat["m5_dosya_yok"], stat["m5_yol_yanlis"],
           stat["m5_coklu"], stat["m5_yolsuz"]))
    yaz("                           %d uye denetlendi . %d uye GECMIYOR . %d uye "
        "yazilmamis . %d hedef metin dosyasi degil"
        % (stat["m5_uye"], stat["m5_uye_yok"], stat["m5_uye_yazilmamis"],
           stat["m5_metin_degil"]))
    # ██ SINIRI KAPI KENDI YAZAR ██ Bu satir olmasa cikti "N uye denetlendi"
    # der ve okuyan bunu bir tanim guvencesi sanirdi.
    yaz("                           SINIR: uye denetimi \"ad dosyada GECIYOR\" "
        "der, \"dosya bu uyeyi TANIMLIYOR\" demez")
    yaz("                           (ornek: SetState hem UnitLifecycle.cs'te "
        "tanimli hem StructureLifecycle.cs'in yorumunda anilir)")
    yaz("MADDE 6 oksuz donus      : %d adet -- IHLAL DEGIL, sayilan bir bosluk "
        "(bir bolume birden fazla yerden gelinebilir)" % stat["m6_oksuz"])
    yaz("")
    yaz("ihlal: %d" % len(problems))
    return 1 if problems else 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))

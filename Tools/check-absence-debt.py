# Yokluk borcu kapisi: "bu projede karsiligi YOK" hukmu YALNIZ BASINA KALMAMALI.
#
# NEDEN VAR: belgeler bugun bir yokluk hukmu veriyor -- "bu projede karsiligi
# YOK". Hukum DOGRU, ama tek basina bir olu sondur: okuyan "peki ne zaman?"
# diye sorar ve cevap hicbir yerde yazmaz. Operator kalici bir kural istedi ve
# en onemli kisiti koydu: ██ karsilik SIRF OGRENMEK ICIN eklenmemeli ██ --
# gercek bir oyun ozelligi onu zorunlu kilmadikca YOK kalir. Kurali belgeler
# tarafinda baska bir lane yaziyor; bu dosya o kuralin MAKINE KAPISIDIR.
#
# ══ DENETLENEN BICIM ═════════════════════════════════════════════════════
# Bicim ana ajan tarafindan sabitlendi, kapi onu DEGISTIRMEZ. Her yokluk
# hukmunun altinda bes alan durur:
#
#     > **HANGI OZELLIK:** <oyunda gorunen somut ozellik, OYNANIS diliyle>
#     > **NEREYE BAGLANIR:** `Assets/Game/.../Dosya.cs` → `Uye`
#     > **NE KIRAR:** <hangi mevcut karar coker>
#     > **KARARMETRE:** <"bu ozellik, mekanizma HIC VAR OLMASAYDI da istenir
#     >   miydi?" cevabi>
#     > **ARASTIRMA BORCU:** <performance-research sorusu, ya da "gerekmiyor">
#
# ══ AYRISTIRICI NEYI KABUL EDER (BILEREK ESNEK) ══════════════════════════
# Bicimi baska bir lane kesinlestiriyor; bu yuzden ayristirici KIRILGAN
# OLMAMALI. Kirilgan bir ayristirici bicimin degil BOSLUGUN kapisi olur.
# Kabul edilenler, her biri bilerek:
#
#   1. SATIR SONU BOSLUKLARI  -- satir sonundaki bosluklar yok sayilir.
#   2. ALINTI ON EKI          -- satir basindaki bosluk ve '>' isaretleri
#                                (kac tane olursa olsun) soyulur.
#   3. TEMBEL DEVAM SATIRI    -- bir alanin GOVDESI kendi satirinda bitmek
#                                zorunda degil: bos satira, bir sonraki alan
#                                etiketine, bir basliga ya da bir cite kadar
#                                sonraki satirlardan devam eder.
#   4. VURGU VARYASYONLARI    -- '**', '*', '__', '_', '***' hepsi kabul; iki
#                                nokta vurgunun ICINDE ('**NE KIRAR:**') ya da
#                                DISINDA ('**NE KIRAR**:') olabilir.
#   5. TURKCE YAZIM           -- etiketin aksansiz ve aksanli yazimi ESITTIR
#                                (HANGI OZELLIK ve aksanlisi, NEREYE BAGLANIR
#                                ve aksanlisi, ARASTIRMA BORCU ve aksanlisi);
#                                buyuk/kucuk harf de farketmez. ci() bu isi
#                                harf harf yapar, re.IGNORECASE'e GUVENMEZ:
#                                Turkce 'I/i/ı/İ' dortlusunde IGNORECASE
#                                platformdan platforma ayrisir.
#   6. ALAN SIRASI            -- bes alan HERHANGI bir sirada yazilabilir;
#                                kapi sira denetlemez, VARLIK denetler.
#   7. SATIRDA IKI ETIKET     -- 'NE KIRAR: ... · KARARMETRE: ...' gibi ayni
#                                satirda iki etiket varsa ikisi de ayri ayri
#                                taninir; birincinin govdesi ikincinin
#                                basladigi yerde biter.
#
# ██ ALAN SAYILMA ESIGI ██ Bir etiket ancak MARKDOWN VURGUSU tasiyorsa ya da
# satiri bir ALINTI ('>') satiriysa alan sayilir. Gerekce: duz yazi icinde
# gecen "ne kirar:" sozcukleri bir alan degildir; esik olmadan kapi kendi
# kural belgesini denetlemeye kalkar.
#
# ══ ADIM 0: ONCE KAPSAM OLCULDU, SONRA TASARLANDI ════════════════════════
# ██ Bu kapinin en buyuk riski YANLIS KAPSAMDIR. ██ Docs/ altinda yokluk
# birden cok LEHCE ile yaziliyor ve hepsi ayni sozlesmeyi borclu DEGIL. 100
# isaretcinin hepsine bes alan dayatmak belgeleri okunamaz kilardi -- ve
# kuralin kendi reject-gate'ini ihlal ederdi.
#
# ██ OLCULDU (2026-08-24, Docs/, 68 .md dosyasi, kapinin KENDI ciktisi) ██
# Bes lehce ailesi, 221 anma:
#
#   LEHCE AILESI              ANMA   KAPSAM  GEREKCE
#   ---------------------------------------------------------------------
#   karsilik ... YOK/yok/       34   KISMEN  hukum ailesi; ucu birden saglayan
#   YOKTUR / karsilik gelen                  11 anma kapsam ICI
#   HENUZ YOK                  111   DISI    ISARET; kendi hafif sozlesmesi var
#   ██ YOK ██                   22   DISI    FIGUR/TABLO isaretcisi
#   **YOK** / **yok**           43   DISI    cumlenin CEVAP sozcugu
#   ASAMA: oneki                16   DISI    check-curriculum-coverage.py'nin alani
#
# Bu tablo operatorun bildirdigi ilk fotografla ("28 adet . 19 dosyada") ayni
# seyi olcmuyor ve fark BILEREK yazili: o sayim SATIR sayiyordu, bu tablo
# ANMA sayiyor (bir satirda iki anma olabilir) ve 'karsilik gelen' bicimini de
# aileye katiyor.
#
# ══ KAPSAM ICI: YALNIZ VURGULU DUZ YAZI HUKMU ════════════════════════════
# Kapsam ici = ilk uc lehce (KARSILIK ailesi) ve UC KOSULUN UCU BIRDEN:
#   (a) cit blogunun DISINDA  -- cit icindeki satir bir ASCII figur ya da bir
#       ORNEKTIR, hukum degil. Olculdu: KARSILIK ailesinin 34 anmasinin 12'si
#       cit icinde ve hicbiri hukum degil (bir OLUMSUZLAMA da orada: "Bos
#       hucrenin karsiligi yok DEGIL").
#   (b) markdown tablo satiri DEGIL -- tablo hucresi bir ISARET sutunudur ve
#       sozlesmesi tablonun KENDI sutunlaridir.
#   (c) VURGULU -- anmayi '**...**' ya da '██...██' bir vurgu alani oruyor.
#       ██ Vurgu, yazarin kendi "bu bir HUKUMDUR" isaretidir. ██ Vurgusuz
#       "karsiligi yok" cumleleri olculdu: 8 adet, ve icinde bir OLUMSUZLAMA
#       ("karsiligi yok degil"), bir META CUMLE ("durust cevap 'bu projede
#       karsiligi yok' ve o da yazili") ve bir CALISMA ANI OLGUSU ("sozlukte
#       o Unit'in karsiligi yok") var. Vurgu esigi olmasaydi bu ucu de hukum
#       sayilir ve dogru yazilmis belgeler kizartilirdi.
#
# Bugunku fotografta kapsam ici hukum sayisi 11. ██ Bu KUCUK bir sayidir ve
# bilerek oyle. ██ Kapi genisletilebilir: asagidaki YOK-HUKUM isaretcisi
# HERHANGI bir anmayi kapsama SOKAR.
#
# ██ VURGU ESIGININ FATURASI DA YAZILI ██ Esik "yazarin isareti"ne guvenir,
# CUMLENIN ANLAMINA degil. Bugunku 11 hukmun 2'si bu yuzden yanlis kapsama
# giriyor: 03-tahta-sahipligi.md:196 bir CALISMA ANI olgusu ("sozlukte o
# Unit'in karsiligi yok") ve 08-unity-altyapisi.md:1063 BASKA BIR OYUN
# hakkinda ("o oyun Unity kullanmiyor"). Ikisi de YOK-MUAF ile kapatilir.
# ELENEN ALTERNATIF: anmanin satirinda bir PROJE CAPASI ("bu projede",
# "kaynakta", "Assets", "uretimde", "kodda", "bugun") aramak.
# ██ OLCULDU, 11 hukum tek tek tarandi ██: capa tasiyan 4 . tasimayan 7. Yani
# capa kurali 7 hukmu kapsam disi birakirdi ve bu 7'nin 5'i GERCEK hukumdur
# (03-hata:210 . 06-delege:616 . 02-assembly:483 . 04-yok-olan:752 .
# 09-ecs:566). Iki yanlis pozitifi elemek icin bes gercek hukmu kaybetmek
# kotu bir takas: yanlis pozitif bir YOK-MUAF isaretiyle kapanir, kapsam disi
# kalan gercek hukum ise hic gorunmez.
#
# ══ KAPSAM DISI, HER BIRININ GEREKCESI ILE ═══════════════════════════════
#   HENUZ YOK   -- ISARET, hukum degil. Cogu bir uc-oyun tablosunda ya da bir
#                  ayna tablosunun ucuncu sutununda. Ustelik KENDI, DAHA HAFIF
#                  sozlesmesi var ve yaygin olarak odenmis: 'HENUZ YOK → <o
#                  gunu getirecek kosul>'. Olculdu: 108 anmanin 38'i bir ok
#                  tasiyor. Bunlarin hepsine bes alan dayatmak, bir tablo
#                  hucresine bes satirlik alinti blogu koymak demekti.
#   ██ YOK ██   -- FIGUR / TABLO isaretcisi. Olculdu: 21 anmanin 21'i ya bir
#                  cit ici ASCII figurde (durum gecis tablosu, yasam dongusu
#                  cizelgesi) ya bir markdown tablosunda. Duz yazida SIFIR.
#   **YOK**     -- bir cumlenin CEVAP sozcugu ("...yeniden uretilemez, cunku
#                  ..."), bir mekanizma yoklugu hukmu degil.
#   ASAMA:      -- ██ CAKISMA BOLGESI ██ Bu on ekin kapisi VAR:
#                  Tools/check-curriculum-coverage.py. O kapi defterin 4
#                  sutunlu KAVRAM tablolarini tarar ve 'HENUZ YOK' satirinda
#                  'ASAMA:' on ekini, asama adinin uzunlugunu, sahip dosyanin
#                  varligini ve satir numarasini denetler. Bu kapi oraya HIC
#                  girmez. Olculdu: 16 ASAMA: anmasinin 16'si
#                  Docs/ogrenme/03-kavram-borc-defteri.md icinde.
#
# ══ DOSYA DUZEYINDE MUAFIYET, HER BIRININ GEREKCESI ILE ══════════════════
#   Docs/ogrenme/02-sonraki-asamalar.md
#       ██ BORCU ZATEN ODEMIS. ██ Dosya kendi basliginda bes alanli bir sablon
#       ilan ediyor ve her asamada uyguluyor: 'A · BUGUNKU KARSILIGI',
#       'B · TETIKLEYICI KOSUL', 'C · ILK ADIM', 'D · NE KIRAR',
#       'E · ON KOSUL'. Ikinci kez, baska adlarla ayni borcu istemek bir
#       kapinin yapabilecegi en zararli sey olurdu: dogru yazilmis tek dosyayi
#       kizartmak.
#   Docs/ogrenme/03-kavram-borc-defteri.md
#       check-curriculum-coverage.py'nin alani (yukarida).
#   Docs/handoff/
#       Calisma gunlugu; ogretici belge degil, okuyucusu operatordur.
#
# ══ IKI ISARETCI: MUAFIYET VE ZORLAMA ════════════════════════════════════
#   YOK-MUAF   satirdaki anmalar denetlenmez. ██ Muaf sayisi her kosumda
#              BASILIR ██ -- sessiz muafiyet kapiyi korlestiren seyin ta
#              kendisidir.
#   YOK-HUKUM  satirdaki anmalar lehcesi ve yeri ne olursa olsun KAPSAMA
#              GIRER. Kapinin varsayilan kapsami dar; bu isaretci onu
#              yazarin elinde genisletir.
#   ONCELIK: YOK-MUAF > YOK-HUKUM > dosya muafiyeti > lehce > yer > vurgu.
#
# ══ MADDE 4'UN SINIRI -- KAPI CUMLEYI GORUR, HUKMU GOREMEZ ═══════════════
# ██ "Bu ozellik gercekten mesru mu" MAKINEYLE DENETLENEMEZ. ██ Kapi
# KARARMETRE alaninin BOS olmadigini ve yalnizca "ogrenmek icin" demedigini
# olcer. Bunlarin ikisi de bir CUMLE olcusudur. Yazar "hayir, ozellik kendi
# basina istenirdi" yazip yalan soylerse kapi bunu goremez ve gormedigini
# BILDIRIR: her kosumda ciktinin MADDE 4 satirinin altinda yazili.
#
# ══ BLOK NEREDE ARANIR: HUKMUN BOLUMU ════════════════════════════════════
# Bir hukmun bes alani, hukum satirindan SONRA baslar ve su ucunden hangisi
# once gelirse orada biter: (a) bir sonraki HERHANGI baslik, (b) bir sonraki
# kapsam ici hukum, (c) dosya sonu.
#
# ██ UC ADAY TARTILDI ██
#   A  bir sonraki HERHANGI baslik  : SECILEN
#   B  ayni ya da daha UST duzey baslik (check-navigation-loops.py'nin bolum
#      kurali)                      : ELENDI -- o kapi bir bolume DAGILMIS
#      donus satirlarini ariyor; burada aranan sey hukmun HEMEN ALTINA
#      yazilan bir bloktur. Araya bir alt baslik girdiyse blok artik o alt
#      bolumun malidir, hukmun degil.
#   C  sabit N satirlik pencere     : ELENDI -- pencere darsa uzun bir
#      paragrafin altindaki blogu gormez, genisse komsu hukmun blogunu calar.
#      Iki yonlu yanilan bir olcu, olcu degildir.
# ██ SINIR YAZILI ██ Blok hukmun USTUNE yazilirsa kapi onu gormez; ayni
# bolumdeki iki hukumden ikincisi araya girerse birincinin blogu bulunmaz.
# Ikisi de bilincli: bicim blogu hukmun ALTINA koyuyor.
#
# ══ MADDE 2'NIN SINIRI, ABARTILMADAN ═════════════════════════════════════
# Uye denetimi "bu ad dosyada GECIYOR" der, "bu dosya bu uyeyi TANIMLIYOR"
# demez. Ayni sinir Tools/check-navigation-loops.py'de olculdu ve yazili:
# 'SetState' hem UnitLifecycle.cs'te tanimli hem StructureLifecycle.cs'in
# yorumunda anilir. Tanim ayristirmasi (ozellik, ifade govdeli uye, alan, enum
# ogesi, partial) sahte pozitif uretirdi. Sinir GIZLENMEZ: her kosumda basilir.
#
# ══ CIKIS KODU SOZLESMESI (kardes kapilarla ayni) ════════════════════════
#   0  ihlal yok . 1  ihlal var . 2  KAPI BOZUK
#
# Kullanim:
#   python Tools/check-absence-debt.py
#   python Tools/check-absence-debt.py <belge-koku> <kaynak-koku>  # negatif test

import pathlib
import re
import sys

DOCS_DEFAULT = "Docs"
ASSETS_DEFAULT = "Assets"

MUAF = "YOK-MUAF"
ZORLA = "YOK-HUKUM"

# Dosya duzeyinde muafiyet: (on ek, gerekce). On ek '/' ile biterse dizin.
DOSYA_MUAF = (
    ("Docs/ogrenme/02-sonraki-asamalar.md",
     "borcu ODEMIS: A-BUGUNKU KARSILIGI . B-TETIKLEYICI KOSUL . C-ILK ADIM . "
     "D-NE KIRAR . E-ON KOSUL sablonunu kendi basliginda ilan ediyor"),
    ("Docs/ogrenme/03-kavram-borc-defteri.md",
     "check-curriculum-coverage.py'nin alani (4 sutunlu KAVRAM tablolari)"),
    ("Docs/handoff/",
     "calisma gunlugu, ogretici belge degil"),
)


def ci(word):
    """Sozcugu harf harf buyuk/kucuk kabul eden desene cevirir.

    ██ NEDEN ELLE, re.IGNORECASE ILE DEGIL ██ Turkce 'I/i/ı/İ' dortlusunde
    IGNORECASE guvenilmez: 'ı'.upper() 'I' verir ama 'İ'.lower() iki
    karakterlik bir dizi verir, ve desenin bir parcasini duyarli birakmak
    ('ASAMA' yalniz BUYUK harf) global bayrakla mumkun degil. Bu tablo
    aksansiz yazimi da esitler: 'ozellik' deseni 'ÖZELLİK'i de tanir.
    """
    esler = {
        "i": "iıIİ", "s": "sşSŞ", "g": "gğGĞ", "o": "oöOÖ",
        "u": "uüUÜ", "c": "cçCÇ",
    }
    out = []
    for ch in word:
        low = ch.lower()
        if low in esler:
            out.append("[" + esler[low] + "]")
        elif ch.isalpha():
            out.append("[" + low + ch.upper() + "]")
        else:
            out.append(re.escape(ch))
    return "".join(out)


HEADING = re.compile(r"^\s{0,3}#{1,6}\s+")
FENCE = re.compile(r"^\s*```")
TABLO = re.compile(r"^\s*\|.*\|\s*$")
QUOTE = re.compile(r"^[ \t>]*")

# ── YOKLUK ANMASI: yedi lehce, TEK bir taramada ──────────────────────────
# Siralama ONEMLIDIR: alternatifler soldan saga denenir ve 'karsilik'
# alternatifi en sonda durur. 'karsiligi ██ HENUZ YOK ██' gibi bir satirda
# olumsuz ileri-bakis (?!HENUZ) 'karsilik' alternatifinin HENUZ'un uzerinden
# atlamasini engeller; boylece anma HENUZ lehcesi olarak sayilir, KARSILIK
# olarak degil. Bu iki ornek Docs/ altinda gercekten var (00-iskelet.md:587 ve
# 08-motor-cagri-dongusu.md:884) ve ilk surumde tam olarak yanlis sayildilar.
#
# ██ EK SINIRLI TUTULDU ██ Yokluk sozcugunun eki yalnizca '-tur' olabilir
# (YOK . YOKTUR). Ilk surumde ek '\w*' idi ve kapsam OLCULEBILIR bicimde
# SISTI: '**YOK**' lehcesi 5 yerine 42 sayildi, cunku '**yokluk**' ve
# '**yoklugu**' sozcukleri de anma sayildi. Kapsamini abartan bir kapi, kendi
# raporunda yalan soyler.
_EK = r"(?:['’]?" + ci("tur") + r")?"
_YOK = r"\b" + ci("yok") + _EK + r"\b"
# 'karsilik' govdesi hem '-k' hem '-g/ğ' ile biter: 'karsilik gelen bir sey
# yok' ve 'karsiligi yok' ayni ailedir. Olculdu: '-k' bicimi Docs/ altinda
# TEK bir anma getiriyor (08-unity-altyapisi.md:1058, bir tablo hucresi).
_KARSILIK = ci("karsili") + r"[gğGĞkK]"
ANMA = re.compile(
    r"(?P<asama>A[SŞ]AMA\s*:)"
    r"|(?P<henuz>" + ci("henuz") + r"\s+" + ci("yok") + _EK + r")"
    r"|(?P<blok>██\s*" + ci("yok") + _EK + r"\s*██)"
    r"|(?P<vurgu>\*\*\s*" + ci("yok") + _EK + r"\s*\*\*)"
    r"|(?P<karsilik>" + _KARSILIK + r"\w*"
    r"(?:(?!" + ci("henuz") + r")[^.\n]){0,40}?" + _YOK + r")"
)
LEHCELER = ("asama", "henuz", "blok", "vurgu", "karsilik")
# Kapsama girebilen tek aile. Digerleri lehce gerekcesiyle disarida.
KARSILIK_AILESI = ("karsilik",)

# ── VURGU ALANLARI: DOSYA duzeyinde eslenir ──────────────────────────────
# ██ OLCULDU: 03-hata-bildirme-ve-dogrulama.md:209-210 ██ Bir vurgu 209'da
# aciliyor ve 210'da kapaniyor ('**Bu projede gozlemlenebilir / karsiligi
# YOK**'). Satir satir eslenen bir vurgu ayristirici bu gercek hukmu
# "vurgusuz" sayip kapsam disi birakirdi -- ilk surumde tam olarak bu oldu.
# Ust sinir (400 karakter) baibos bir '**' isaretinin butun dosyayi tek bir
# vurgu alanina cevirmesini engeller; olculdu, Docs/ altindaki en uzun gercek
# vurgu alani 260 karakter.
VURGU_ALANI = re.compile(r"\*\*[\s\S]{1,400}?\*\*|██[\s\S]{1,400}?██")

# ── BES ALAN ────────────────────────────────────────────────────────────
# 'pre' etiketten onceki vurguyu, 'mid' etiket ile iki nokta arasindakini,
# 'post' iki noktadan sonrakini yakalar. Alan sayilma esigi bu uc parcadan
# ya da satirin alinti olmasindan olculur.
def _etiket(*parcalar):
    govde = r"\s+".join(parcalar)
    return re.compile(r"(?P<pre>[*_\s]{0,8})" + govde + r"(?P<mid>[*_\s]{0,4}):(?P<post>[*_]{0,3})")


ALANLAR = (
    ("OZELLIK", "HANGI OZELLIK", _etiket(ci("hangi"), ci("ozellik"))),
    ("BAGLANIR", "NEREYE BAGLANIR", _etiket(ci("nereye"), ci("baglanir"))),
    ("KIRAR", "NE KIRAR", _etiket(ci("ne"), ci("kirar"))),
    ("KARARMETRE", "KARARMETRE", _etiket(ci("kararmetre"))),
    ("BORC", "ARASTIRMA BORCU", _etiket(ci("arastirma"), ci("borcu"))),
)
ALAN_SIRASI = tuple(a[0] for a in ALANLAR)
ALAN_ADI = {a[0]: a[1] for a in ALANLAR}

# Govde en cok bu kadar parca yutar. Bos satir / etiket / baslik / cit zaten
# durdurur; bu ust sinir yalnizca bunlarin hicbiri gelmezse (bicimsiz yazilmis
# uzun bir paragraf) hasari sinirlar. Kardes kapi check-navigation-loops.py
# ayni sayiyi Docs/ uzerinde olcup 6'da birakti; ayni sayiyi kullanmak iki
# kapinin ayni belgede ayni govdeyi gormesini saglar.
DEVAM_SINIRI = 6

TIRNAKLI = re.compile(r"`([^`]+)`")
KOD_UZANTILARI = "cs|asmdef|asmref|json|shader|cginc|hlsl|uss|uxml|unity|prefab|asset|meta|txt|md"
YOL_GORUNUMU = re.compile(
    r"(?:[A-Za-z0-9_.\-]+/)+[A-Za-z0-9_.\-]+\.[A-Za-z0-9_]+"
    r"|[A-Za-z0-9_.\-]+\.(?:" + KOD_UZANTILARI + r")",
    re.IGNORECASE)
YOL_KUYRUK = re.compile(r"[:#]\s*L?\d+(?:\s*-\s*\d+)?$")
UYE_ADI = re.compile(r"[A-Za-z_][A-Za-z0-9_]*(?:\.[A-Za-z_][A-Za-z0-9_]*)*")
TOKEN = re.compile(r"[A-Za-z_][A-Za-z0-9_]*")

# ── MADDE 3: SATIR NUMARASININ DORT LEHCESI ──────────────────────────────
# Kapi burada "cozuluyor mu" diye SORMAZ, VARLIGINI ihlal sayar. Gerekce
# kardes kapida olculdu ve yazili: uye adi kaymaz, satir numarasi kayar.
SATIR_NO = re.compile(
    r"\.[A-Za-z]{2,8}\s*[:#]\s*L?\d+"          # Dosya.cs:5 . Dosya.cs#L5
    r"|`\s*:\d+(?:\s*-\s*\d+)?\s*`"            # `:5` -- check-doc-code-refs.py lehcesi
    r"|\bL\d+\b"                               # GitHub L5
    r"|\b" + ci("satir") + r"\s*\d+"           # satir 5
    r"|\d+\s*\.\s*" + ci("satir")              # 5. satir
)

# ── MADDE 4: "SIRF OGRENMEK ICIN" ────────────────────────────────────────
# KARARMETRE govdesi katlanip harfe indirgenir; dolgu sozcukler atilir. Geriye
# yalnizca ogrenme kokenli bir-iki sozcuk kalirsa cevap "ogrenmek icin"dir.
# Esik olculdu degil TASARIM: "sirf ogrenmek icin" -> {ogrenmek} (1 sozcuk),
# "hayir; ikinci birim turu dogdugu gun zorunlu olur" -> 7 sozcuk.
OGRENME_KOKU = ("ogrenmek", "ogrenme", "ogrenim", "ogretici", "ogrenmis", "ogren")
DOLGU = {
    "bu", "su", "o", "bir", "ve", "de", "da", "ile", "icin", "sirf", "sadece",
    "yalniz", "yalnizca", "diye", "amaciyla", "adina", "amac", "amaci",
    "ozellik", "ozelligi", "mekanizma", "mekanizmayi", "cunku", "ki", "ise",
}
OGRENME_ESIGI = 2


def fold(text):
    """Turkce aksanlari sadelestirir ve kucuk harfe cevirir."""
    for source, target in (
        ("İ", "i"), ("I", "i"), ("ı", "i"),
        ("Ş", "s"), ("ş", "s"), ("Ğ", "g"), ("ğ", "g"),
        ("Ç", "c"), ("ç", "c"), ("Ö", "o"), ("ö", "o"),
        ("Ü", "u"), ("ü", "u"), ("Â", "a"), ("â", "a"),
    ):
        text = text.replace(source, target)
    return text.lower()


def yaz(text):
    """Konsolun kodlamasina sigmayan karakterleri kirmadan basar.

    Neden gerekli: Windows konsolu cogu kez cp1254 ve '██' o kumede yok. Ham
    'print' bir UnicodeEncodeError ile kapiyi CALISMADAN oldururdu -- yani
    kapi ihlal buldugu anda kendi kendini susturur.
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
    bagimli oz-sinama, o dosya tasindigi gun kapiyi da sessizce oldururdu.
    Ayni gerekce Tools/check-doc-code-refs.py ve
    Tools/check-navigation-loops.py icinde de yazili; desen oradan alindi.
    """

    def __init__(self, files):
        self.files = {k.replace("\\", "/"): v for k, v in files.items()}
        self._cache = {}

    @classmethod
    def from_disk(cls, root):
        files = {}
        for path in pathlib.Path(root).rglob("*.md"):
            files[path.as_posix()] = path.read_text(encoding="utf-8", errors="replace")
        return cls(files)

    def keys(self):
        return sorted(self.files)

    def coz(self, key):
        """-> (lines, cit, basliklar, vurgu_araliklari)

        cit[i]      : 'DIS' . 'ICI' . 'SINIR'
        basliklar   : cit DISINDAKI baslik satirlarinin indeksleri
        vurgu       : [(satir, bas_sutun, son_sutun)] -- vurgu alanlari
        """
        if key in self._cache:
            return self._cache[key]

        lines = self.files[key].split("\n")
        cit = []
        in_fence = False
        for line in lines:
            if FENCE.match(line):
                in_fence = not in_fence
                cit.append("SINIR")
                continue
            cit.append("ICI" if in_fence else "DIS")

        basliklar = [i for i, line in enumerate(lines)
                     if cit[i] == "DIS" and HEADING.match(line)]

        # ██ VURGU ALANLARI DOSYA DUZEYINDE ESLENIR ██ (yukarida gerekcesi).
        # Cit satirlari BOSALTILIR: bir cit blogunun icindeki '**' disaridaki
        # bir '**' ile eslesirse ortaya butun bloklari yutan sahte bir vurgu
        # alani cikardi.
        maskeli = [line if cit[i] == "DIS" else "" for i, line in enumerate(lines)]
        metin = "\n".join(maskeli)
        basi = []
        toplam = 0
        for line in maskeli:
            basi.append(toplam)
            toplam += len(line) + 1

        vurgu = [[] for _ in lines]
        for m in VURGU_ALANI.finditer(metin):
            bas, son = m.start(), m.end()
            # Alanin dokundugu her satira kendi sutun araligini yaz.
            for i, ofset in enumerate(basi):
                satir_son = ofset + len(maskeli[i])
                if son <= ofset or bas >= satir_son + 1:
                    continue
                vurgu[i].append((max(bas - ofset, 0), min(son - ofset, len(maskeli[i]))))

        self._cache[key] = (lines, cit, basliklar, vurgu)
        return self._cache[key]


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
        files = {}
        metin = {
            ".cs", ".asmdef", ".asmref", ".json", ".txt", ".md", ".xml", ".yaml",
            ".yml", ".shader", ".cginc", ".hlsl", ".uss", ".uxml", ".unity",
            ".prefab", ".asset", ".meta",
        }
        for path in pathlib.Path(root).rglob("*"):
            if not path.is_file():
                continue
            if path.suffix.lower() in metin:
                files[path.as_posix()] = path.read_text(encoding="utf-8", errors="replace")
            else:
                files[path.as_posix()] = None
        return cls(files)

    def coz(self, yol):
        """-> (hedefler, durum) ; durum: TAM . YOL-YANLIS . YOK

        Sonek eslesmesi '/' sinirinda yapilir, yoksa 'View.cs' ile
        'UnitView.cs' birbirine karisirdi. Yol verilmis ama o yolda dosya
        yoksa ve ad baska bir yerde varsa, bu SESSIZ bir eslesme degil AYRI
        bir durumdur (YOL-YANLIS) -- ayni ayrim kardes kapilarda da var.
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


class Anma(object):
    """Bir yokluk anmasi: lehcesi, yeri, kapsam karari."""

    __slots__ = ("key", "index", "lehce", "yer", "vurgulu", "kapsam", "metin")

    def __init__(self, key, index, lehce, yer, vurgulu, kapsam, metin):
        self.key = key
        self.index = index
        self.lehce = lehce
        self.yer = yer
        self.vurgulu = vurgulu
        self.kapsam = kapsam
        self.metin = metin


def dosya_muafi(key):
    """-> (on ek, gerekce) . muaf degilse None.

    On ek de doner cunku ciktidaki dagilim SATIRI onunla gruplaniyor. Ilk
    surumde dagilim ayri bir on ek eslesmesi kullaniyordu ve asagidaki sonek
    kuralindan AYRISTI: negatif testte muafiyet calisti ama dagilim satiri
    "0" gosterdi. Tek bir eslesme fonksiyonu, tek bir dogru.

    ██ ESLESME '/' SINIRINDA SONEKTIR, DUZ ON EK DEGIL ██ Gerekce olculdu:
    kapi negatif testte baska bir belge kokuyle kosuyor ('sinama/Docs/...') ve
    duz on ek eslesmesi orada HICBIR dosyayi muaf tutmazdi -- yani muafiyet
    mekanizmasi tam da sinandigi yerde sessizce kapali olurdu. Sinir '/' cunku
    'Docs/handoff/' ile 'BaskaDocs/handoff/' ayni sey degildir.
    """
    for onek, gerekce in DOSYA_MUAF:
        if onek.endswith("/"):
            if ("/" + key).find("/" + onek) >= 0:
                return onek, gerekce
        elif key == onek or key.endswith("/" + onek):
            return onek, gerekce
    return None


def vurgulu_mu(araliklar, bas, son):
    """Anma bir vurgu alaniyla KESISIYOR mu."""
    for a, b in araliklar:
        if bas < b and son > a:
            return True
    return False


def anmalari_bul(docs):
    """Butun belgelerdeki yokluk anmalarini toplar ve KAPSAMINI belirler.

    Kapsam onceligi (yukarida yazili): YOK-MUAF > YOK-HUKUM > dosya muafiyeti
    > lehce > yer > vurgu.
    """
    hepsi = []
    for key in docs.keys():
        lines, cit, _, vurgu = docs.coz(key)
        dosya_kaydi = dosya_muafi(key)
        for index, line in enumerate(lines):
            if cit[index] == "SINIR":
                continue
            satir_muaf = MUAF in line
            satir_zorla = ZORLA in line
            if cit[index] == "ICI":
                yer = "CIT"
            elif TABLO.match(line):
                yer = "TABLO"
            else:
                yer = "DUZ"
            for m in ANMA.finditer(line):
                lehce = next(ad for ad in LEHCELER if m.group(ad) is not None)
                vurgulu = vurgulu_mu(vurgu[index], m.start(), m.end())
                if satir_muaf:
                    kapsam = "MUAF"
                elif satir_zorla:
                    kapsam = "ICI"
                elif dosya_kaydi is not None:
                    kapsam = "DOSYA-MUAF"
                elif lehce not in KARSILIK_AILESI:
                    kapsam = "LEHCE-" + lehce.upper()
                elif yer != "DUZ":
                    kapsam = yer
                elif not vurgulu:
                    kapsam = "VURGUSUZ"
                else:
                    kapsam = "ICI"
                hepsi.append(Anma(key, index, lehce, yer, vurgulu, kapsam,
                                  m.group(0).strip()))
    return hepsi


def alanlari_bul(govde):
    """Bir satirdaki butun alan etiketleri: [(bas, son, tur)] -- sirali."""
    bulunan = []
    for tur, _, desen in ALANLAR:
        for m in desen.finditer(govde):
            cevre = m.group("pre") + m.group("mid") + m.group("post")
            bulunan.append((m.start(), m.end(), tur, any(c in "*_" for c in cevre)))
    bulunan.sort(key=lambda e: e[0])
    return bulunan


def blogu_oku(docs, key, bas, son):
    """[bas, son) araligindaki alan etiketlerini ve govdelerini toplar.

    -> {tur: govde} ; ayni alan iki kez yazilmissa ILKI tutulur (ikincisi
    'yinelenen' olarak sayilir ve ciktida gorunur).
    """
    lines, cit, _, _ = docs.coz(key)
    govdeler = []
    etiketler = []
    for index in range(len(lines)):
        if cit[index] != "DIS":
            govdeler.append("")
            etiketler.append([])
            continue
        govde = QUOTE.sub("", lines[index].rstrip())
        alinti = lines[index].lstrip().startswith(">")
        govdeler.append(govde)
        # ESIK: vurgu VAR ya da satir bir ALINTI satiri.
        etiketler.append([(b, s, t) for b, s, t, v in alanlari_bul(govde)
                          if v or alinti])

    bulunan = {}
    yinelenen = 0
    for index in range(bas, min(son, len(lines))):
        if cit[index] != "DIS":
            continue
        satir_etiketleri = etiketler[index]
        for sira, (_, etiket_sonu, tur) in enumerate(satir_etiketleri):
            kesim = (satir_etiketleri[sira + 1][0]
                     if sira + 1 < len(satir_etiketleri) else len(govdeler[index]))
            parcalar = [govdeler[index][etiket_sonu:max(etiket_sonu, kesim)]]
            # Yalniz satirin SON etiketi tembel devam satiri yutabilir.
            if sira + 1 == len(satir_etiketleri):
                j = index + 1
                while j < min(son, len(lines)) and len(parcalar) <= DEVAM_SINIRI:
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
            metin = " ".join(p.strip() for p in parcalar if p.strip())
            if tur in bulunan:
                yinelenen += 1
            else:
                bulunan[tur] = metin
    return bulunan, yinelenen


def kod_hedefleri(govde):
    """NEREYE BAGLANIR govdesinden (yol, uye) ciftlerini cikarir.

    Bicim tek bir hedef yaziyor ('`Dosya.cs` → `Uye`') ama ayristirici
    coklu hedefi de kabul eder: her yol, KENDISINDEN SONRA ve bir sonraki
    yoldan ONCE gelen ilk uye adini sahiplenir.

    Uye YALNIZ ters tirnak icinde aranir. Gerekce kardes kapida olculdu ve
    yazili: ok'tan sonraki ciplak sozcugu uye saymak, duz yazidaki 'dosyanin'
    sozcugunu uye sanip SAHTE ihlal uretmisti.
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
            yollar.append((m.start(), YOL_KUYRUK.sub("", m.group(0))))
            break
    if not yollar:
        return []

    ciftler = []
    for sira, (pos, yol) in enumerate(yollar):
        sonraki = yollar[sira + 1][0] if sira + 1 < len(yollar) else len(govde)
        uye = None
        for ad_pos, ad in adlar:
            if pos < ad_pos < sonraki:
                uye = ad
                break
        ciftler.append((yol, uye))
    return ciftler


def uye_adi(uye):
    """`Battle.AddUnit` -> 'AddUnit' . `SetState(UnitState)` -> 'SetState'."""
    if not uye:
        return None
    m = UYE_ADI.search(uye.strip().strip("`"))
    if not m:
        return None
    return m.group(0).split(".")[-1]


def ogrenmek_icin_mi(govde):
    """MADDE 4: KARARMETRE yalnizca 'ogrenmek icin' mi diyor.

    ██ SINIR ██ Bu bir CUMLE olcusudur. Yazar "hayir, ozellik kendi basina
    istenirdi" yazip yalan soylerse kapi goremez. Gordugu tek sey: govdenin
    icinde ogrenme kokenli bir sozcuk var ve ondan baska soylenmis bir sey yok.
    """
    sade = re.sub(r"[^a-z0-9 ]", " ", fold(govde))
    sozcukler = [w for w in sade.split() if w and w not in DOLGU]
    if not sozcukler:
        return False
    if not any(w in OGRENME_KOKU for w in sozcukler):
        return False
    return len(sozcukler) <= OGRENME_ESIGI


def bos_sayac():
    return {
        "anma": 0, "hukum": 0, "muaf": 0, "disi": 0,
        "satir_coklu": 0, "yinelenen_alan": 0,
        # Lehce dagilimi
        "l_karsilik": 0, "l_henuz": 0, "l_blok": 0, "l_vurgu": 0, "l_asama": 0,
        # Kapsam disi gerekceleri
        "d_cit": 0, "d_tablo": 0, "d_vurgusuz": 0, "d_dosya": 0,
        "d_lehce_henuz": 0, "d_lehce_blok": 0, "d_lehce_vurgu": 0,
        "d_lehce_asama": 0,
        # MADDE 1
        "m1": 0, "m1_eksik": 0,
        "m1_ozellik": 0, "m1_baglanir": 0, "m1_kirar": 0,
        "m1_kararmetre": 0, "m1_borc": 0,
        # MADDE 2
        "m2": 0, "m2_yolsuz": 0, "m2_dosya": 0, "m2_dosya_yok": 0,
        "m2_yol_yanlis": 0, "m2_coklu": 0,
        "m2_uye": 0, "m2_uye_yok": 0, "m2_uye_yazilmamis": 0, "m2_metin_degil": 0,
        # MADDE 3
        "m3": 0, "m3_ihlal": 0,
        # MADDE 4
        "m4": 0, "m4_bos": 0, "m4_ogrenmek": 0,
        # MADDE 5
        "m5": 0, "m5_bos": 0,
        # Dosya muafiyetinin DOSYA DOSYA dagilimi. ██ NEDEN AYRI ██ Kapsam
        # onceligi dosya muafiyetini lehceden ONCE uyguluyor; bu yuzden
        # 'ASAMA:' lehcesinin kapsam disi sayaci SIFIR gorunur -- 16 anmanin
        # 16'si zaten dosya muafiyetinde yutulmustur. Toplami ayrica basmak,
        # kapinin cakisma sinirini gorunur tutmanin tek yolu.
        "dosya_dagilim": {},
    }


def audit(docs, source):
    """-> (ihlaller, sayaclar) ; ihlal: (belge, satir, tur, mesaj)."""
    problems = []
    stat = bos_sayac()

    anmalar = anmalari_bul(docs)
    hukum_satirlari = {}

    for anma in anmalar:
        stat["anma"] += 1
        stat["l_" + anma.lehce] += 1
        if anma.kapsam == "MUAF":
            stat["muaf"] += 1
            continue
        if anma.kapsam != "ICI":
            stat["disi"] += 1
            stat[{
                "CIT": "d_cit", "TABLO": "d_tablo", "VURGUSUZ": "d_vurgusuz",
                "DOSYA-MUAF": "d_dosya", "LEHCE-HENUZ": "d_lehce_henuz",
                "LEHCE-BLOK": "d_lehce_blok", "LEHCE-VURGU": "d_lehce_vurgu",
                "LEHCE-ASAMA": "d_lehce_asama",
            }[anma.kapsam]] += 1
            if anma.kapsam == "DOSYA-MUAF":
                onek = dosya_muafi(anma.key)[0]
                dagilim = stat["dosya_dagilim"]
                dagilim[onek] = dagilim.get(onek, 0) + 1
            continue
        # ██ SATIRDA IKI HUKUM ██ Ayni satirdaki ikinci anma AYRI bir hukum
        # sayilmaz: bes alanlik blok satirin ALTINA yazilir ve iki anmayi
        # ayirmanin yolu yoktur. Ikisi de SAYILIR, denetlenen ILKIDIR.
        if (anma.key, anma.index) in hukum_satirlari:
            stat["satir_coklu"] += 1
            continue
        hukum_satirlari[(anma.key, anma.index)] = anma
        stat["hukum"] += 1

    # Hukumleri dosya ve satira gore sirala: blok siniri "bir sonraki hukum"
    # kuralini kullanabilmek icin sira gerekiyor.
    sirali = {}
    for (key, index), anma in hukum_satirlari.items():
        sirali.setdefault(key, []).append(index)
    for value in sirali.values():
        value.sort()

    for key in sorted(sirali):
        lines, _, basliklar, _ = docs.coz(key)
        indeksler = sirali[key]
        for sira, index in enumerate(indeksler):
            # ── BLOK SINIRI: baslik . sonraki hukum . dosya sonu ──────────
            son = len(lines)
            for b in basliklar:
                if b > index:
                    son = min(son, b)
                    break
            if sira + 1 < len(indeksler):
                son = min(son, indeksler[sira + 1])

            alanlar, yinelenen = blogu_oku(docs, key, index + 1, son)
            stat["yinelenen_alan"] += yinelenen

            # ── MADDE 1: bes alanin hepsi var mi ─────────────────────────
            # "Var" = etiketi yazilmis VE govdesi bos degil. Yalniz etiketin
            # varligini aramak, bos birakilmis bir alani odenmis sayardi.
            stat["m1"] += 1
            eksik = []
            for tur in ALAN_SIRASI:
                govde = alanlar.get(tur)
                if govde is None:
                    eksik.append(ALAN_ADI[tur])
                    stat["m1_" + tur.lower()] += 1
                elif not govde.strip() and tur in ("OZELLIK", "BAGLANIR", "KIRAR"):
                    # KARARMETRE ve ARASTIRMA BORCU'nun BOS hali MADDE 4 ve
                    # MADDE 5'in isidir; burada iki kez sayilmasin.
                    eksik.append(ALAN_ADI[tur] + " (govde bos)")
                    stat["m1_" + tur.lower()] += 1
            if eksik:
                stat["m1_eksik"] += 1
                problems.append((key, index + 1, "ALAN-EKSIK",
                                 "yokluk hukmunun bes alani tamam degil -- EKSIK: %s"
                                 % " . ".join(eksik)))

            baglanir = alanlar.get("BAGLANIR")
            if baglanir is not None and baglanir.strip():
                # ── MADDE 3: satir numarasi IHLALDIR ─────────────────────
                stat["m3"] += 1
                bulundu = SATIR_NO.search(baglanir)
                if bulundu:
                    stat["m3_ihlal"] += 1
                    problems.append((key, index + 1, "BAGLANIR-SATIR-NO",
                                     "NEREYE BAGLANIR satir numarasi tasiyor "
                                     "(%s) -- uye adi kaymaz, satir numarasi "
                                     "kayar" % bulundu.group(0).strip()))

                # ── MADDE 2: dosya cozuluyor mu + uye geciyor mu ─────────
                stat["m2"] += 1
                ciftler = kod_hedefleri(baglanir)
                if not ciftler:
                    stat["m2_yolsuz"] += 1
                    problems.append((key, index + 1, "BAGLANIR-YOLSUZ",
                                     "NEREYE BAGLANIR icinde Assets/ altinda "
                                     "cozulebilecek bir yol yok: %s"
                                     % baglanir.strip()[:80]))
                for yol, uye in ciftler:
                    stat["m2_dosya"] += 1
                    hedefler, durum = source.coz(yol)
                    if durum == "YOK":
                        stat["m2_dosya_yok"] += 1
                        problems.append((key, index + 1, "BAGLANIR-YOL-YOK",
                                         "NEREYE BAGLANIR dosyasi Assets/ "
                                         "altinda YOK: %s" % yol))
                        continue
                    if durum == "YOL-YANLIS":
                        stat["m2_yol_yanlis"] += 1
                        problems.append((key, index + 1, "BAGLANIR-YOL-YANLIS",
                                         "NEREYE BAGLANIR yolu yanlis: %s -> "
                                         "ayni adli dosya %s"
                                         % (yol, ", ".join(hedefler[:3]))))
                        continue
                    if len(hedefler) > 1:
                        stat["m2_coklu"] += 1
                    if uye is None:
                        stat["m2_uye_yazilmamis"] += 1
                        continue
                    ad = uye_adi(uye)
                    kumeler = [source.kelimeler(h) for h in hedefler]
                    if all(k is None for k in kumeler):
                        stat["m2_metin_degil"] += 1
                        continue
                    stat["m2_uye"] += 1
                    if not any(k is not None and ad in k for k in kumeler):
                        stat["m2_uye_yok"] += 1
                        problems.append((key, index + 1, "BAGLANIR-UYE-YOK",
                                         "NEREYE BAGLANIR uyesi dosyada "
                                         "GECMIYOR: %s -> %s" % (yol, ad)))

            # ── MADDE 4: KARARMETRE ─────────────────────────────────────
            kararmetre = alanlar.get("KARARMETRE")
            if kararmetre is not None:
                stat["m4"] += 1
                if not kararmetre.strip():
                    stat["m4_bos"] += 1
                    problems.append((key, index + 1, "KARARMETRE-BOS",
                                     "KARARMETRE alani BOS -- 'bu ozellik, "
                                     "mekanizma HIC VAR OLMASAYDI da istenir "
                                     "miydi?' sorusu cevapsiz"))
                elif ogrenmek_icin_mi(kararmetre):
                    stat["m4_ogrenmek"] += 1
                    problems.append((key, index + 1, "KARARMETRE-OGRENMEK",
                                     "KARARMETRE yalnizca 'ogrenmek icin' "
                                     "diyor: %s -- ogrenme bir ozelligi "
                                     "zorunlu KILMAZ" % kararmetre.strip()[:60]))

            # ── MADDE 5: ARASTIRMA BORCU ────────────────────────────────
            borc = alanlar.get("BORC")
            if borc is not None:
                stat["m5"] += 1
                if not borc.strip():
                    stat["m5_bos"] += 1
                    problems.append((key, index + 1, "BORC-BOS",
                                     "ARASTIRMA BORCU alani BOS -- "
                                     "'gerekmiyor' gecerli bir cevaptir, "
                                     "bosluk degildir"))

    problems.sort(key=lambda p: (p[0], p[1], p[2]))
    return problems, stat


# ── OZ-SINAMA ORNEKLERI ──────────────────────────────────────────────────
# Hepsi bu dosyanin icinde ve SANAL bir sozluk uzerinde kosar: diske
# dokunmayan bir oz-sinama, projedeki hicbir dosya tasinsa da olmez. Ayni
# desen Tools/check-doc-code-refs.py ve Tools/check-navigation-loops.py'de.

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
    "Assets/Sanal/resim.png": None,
}

# ██ IYI ORNEK: TAM BES ALAN ██
# Bicimin her esnek varyasyonu bilerek FARKLI bir satirda kullanildi; ornek
# yalniz "ihlal yok" demiyor, ayristiricinin neyi kabul ettigini KANITLIYOR:
#   - alinti on eki ve fazladan bosluk                (HANGI OZELLIK)
#   - AKSANLI etiket yazimi                           (NEREYE BAGLANIR)
#   - iki nokta vurgunun DISINDA, tek yildiz          (NE KIRAR)
#   - uc yildizli vurgu + TEMBEL devam satiri ('>' yok)(KARARMETRE)
#   - alinti satiri ama HIC vurgu yok (esigin ikinci yarisi) (ARASTIRMA BORCU)
#   - ALAN SIRASI bicimdekinden farkli                (ikinci hukum)
#   - COKLU hedef: bir .cs + uye, bir .png (metin degil), bir uyesiz .cs
#   - vurgu IKI SATIRA yayilmis hukum ('**' 03-hata-bildirme'deki gibi bir
#     satirda acilip digerinde kapaniyor)             (ikinci hukum)
SELF_IYI = {
    "iyi/A.md": "\n".join([
        "# A basligi",
        "",
        "## Birinci hukum",
        "",
        "`GetInvocationList()` yarisinin **bu projede karşılığı YOK**: Assets",
        "altinda sifir eslesme.",
        "",
        ">   **HANGI OZELLIK:**   bir aboneyi adiyla teshis eden hata ekrani",
        "> **NEREYE BAĞLANIR:** `Assets/Sanal/Ornek.cs` → `Calistir`",
        "> *NE KIRAR*: bugunku sessiz abone listesi coker",
        "> ***KARARMETRE***: evet -- oyuncu kimin patladigini gormek ister,",
        "bu mekanizma hic var olmasaydi da o ekran istenirdi",
        "> ARASTIRMA BORCU: gerekmiyor",
        "",
        "## Ikinci hukum",
        "",
        "Bu tablonun **ECS tarafinda",
        "karsiligi yok**: tahtanin kendisi.",
        "",
        "> **ARAŞTIRMA BORCU:** yol bulma maliyeti olculmeli",
        "> **KARARMETRE:** hayir demek zor -- ikinci birim turu dogdugu gun",
        "> zaten istenir",
        "> **NE KIRAR:** tek tahta sahipligi karari",
        "> **HANGİ ÖZELLİK:** ayni anda iki tahtali bir kusatma modu",
        "> **NEREYE BAGLANIR:** `Assets/Sanal/Ornek.cs` → `Calistir`;",
        "> yanindaki `Assets/Sanal/resim.png` → `Sprite`, ve butun",
        "> `Assets/Sanal/Ornek.cs` dosyasi",
        "",
    ]),
}

# ██ KOTU ORNEK: ALTI MADDENIN HER BIRI AYRI AYRI ██
# Her bolumde YALNIZ bir madde bozuk; digerleri bilerek dogru yazildi ki
# ihlaller birbirine karismasin ve dagilim tam olarak sayilabilsin.
_TAM = [
    "> **HANGI OZELLIK:** bir ozellik",
    "> **NEREYE BAGLANIR:** `Assets/Sanal/Ornek.cs` -> `Calistir`",
    "> **NE KIRAR:** bir karar",
    "> **KARARMETRE:** hayir, ozellik kendi basina istenirdi",
    "> **ARASTIRMA BORCU:** gerekmiyor",
]


def _kotu_bolum(baslik, degisiklik):
    """Bir bolum uretir: tam blogun bir satiri degistirilmis ya da atilmis."""
    govde = ["## " + baslik, "", "Bir mekanizmanin **bu projede karsiligi YOK**.", ""]
    for sira, satir in enumerate(_TAM):
        yeni = degisiklik.get(sira, satir)
        if yeni is not None:
            govde.append(yeni)
    govde.append("")
    return govde


_KOTU_BOLUMLER = [
    # MADDE 1: iki alan hic yazilmamis
    ("Alan eksik", {3: None, 4: None}),
    # MADDE 2: yol Assets altinda YOK
    ("Yol yok", {1: "> **NEREYE BAGLANIR:** `Assets/Sanal/Hicyok.cs` -> `Calistir`"}),
    # MADDE 2: yol yanlis (ad baska yerde var)
    ("Yol yanlis", {1: "> **NEREYE BAGLANIR:** `Assets/Yanlis/Ornek.cs` -> `Calistir`"}),
    # MADDE 2: uye dosyada gecmiyor
    ("Uye yok", {1: "> **NEREYE BAGLANIR:** `Assets/Sanal/Ornek.cs` -> `OlmayanUye`"}),
    # MADDE 2: govdede hic yol yok
    ("Yolsuz", {1: "> **NEREYE BAGLANIR:** cekirdek tarafi"}),
    # MADDE 3: satir numarasinin DORT lehcesi, her biri ayri bolumde
    ("Satir no bir", {1: "> **NEREYE BAGLANIR:** `Assets/Sanal/Ornek.cs:5` -> `Calistir`"}),
    ("Satir no iki", {1: "> **NEREYE BAGLANIR:** `Assets/Sanal/Ornek.cs` `:5` -> `Calistir`"}),
    ("Satir no uc", {1: "> **NEREYE BAGLANIR:** `Assets/Sanal/Ornek.cs` L5 -> `Calistir`"}),
    ("Satir no dort", {1: "> **NEREYE BAGLANIR:** `Assets/Sanal/Ornek.cs` 5. satir -> `Calistir`"}),
    # MADDE 4: KARARMETRE bos
    ("Kararmetre bos", {3: "> **KARARMETRE:**"}),
    # MADDE 4: KARARMETRE yalnizca "ogrenmek icin"
    ("Kararmetre ogrenmek", {3: "> **KARARMETRE:** sirf ogrenmek icin"}),
    # MADDE 5: ARASTIRMA BORCU bos
    ("Borc bos", {4: "> **ARASTIRMA BORCU:**"}),
]


def _kotu_belge():
    satirlar = ["# K basligi", ""]
    for baslik, degisiklik in _KOTU_BOLUMLER:
        satirlar.extend(_kotu_bolum(baslik, degisiklik))
    return "\n".join(satirlar)


SELF_KOTU = {"kotu/K.md": _kotu_belge()}

# ██ MADDE 6: MUAFIYET VE KAPSAM DISI ██ Hicbiri IHLAL degildir; hepsi
# SAYILIR ve gerekcesi ciktida yazilidir.
SELF_MUAF = {
    "muaf/M.md": "\n".join([
        "# M basligi",
        "",
        "## Muaf bolum",
        "",
        "Bunun **bu projede karsiligi YOK**. <!-- YOK-MUAF -->",
        "",
        "Su satir bir tablo hucresi, hukum degil:",
        "",
        "| kavram | durum |",
        "|---|---|",
        "| bir sey | **karsiligi YOK** |",
        "",
        "Su satirin vurgusu yok, hukum degil: bos hucrenin karsiligi yok degil.",
        "",
        "Su bir ISARETTIR, hukum degil: ██ HENÜZ YOK ██ → ikinci sahne geldigi gun.",
        "",
        "Su bir figur isaretcisi: ██ YOK ██",
        "",
        "Ekin de tanindigini kanitlar: ██ YOKTUR ██",
        "",
        "Su bir cevap sozcugu: **YOK** — yeniden uretilemez.",
        "",
        "Su check-curriculum-coverage.py'nin alani: `AŞAMA: bir asama adi`",
        "",
        "Asagidaki blok bir ORNEKTIR, gercek hukum degildir:",
        "",
        "```text",
        "Bunun **bu projede karsiligi YOK**.",
        "```",
        "",
    ]),
    # Dizin on eki dali.
    "Docs/handoff/HANDOFF.md": "\n".join([
        "# Gunluk",
        "",
        "Bunun **bu projede karsiligi YOK** -- dosya duzeyinde muaf.",
        "",
    ]),
    # Tam dosya dali, ve BILEREK baska bir kok altinda: eslesme '/' sinirinda
    # SONEKTIR. Duz on ek eslesmesi bu dosyayi muaf tutamazdi ve muafiyet
    # mekanizmasi negatif testte sessizce kapali olurdu.
    "sinama/Docs/ogrenme/02-sonraki-asamalar.md": "\n".join([
        "# Sonraki asamalar",
        "",
        "Bunun **bu projede karsiligi YOK** -- borcunu baska sablonla odemis.",
        "",
    ]),
}

# Zorlama isaretcisi: kapsam disi kalacak bir anmayi kapsama SOKAR.
SELF_ZORLA = {
    "zorla/Z.md": "\n".join([
        "# Z basligi",
        "",
        "## Zorlanan bolum",
        "",
        "| kavram | ██ HENÜZ YOK ██ |  <!-- YOK-HUKUM -->",
        "",
    ]),
}


def self_check(audit_fn):
    """Ayristiricinin CALISTIGINI once kendi uzerinde kanitlar.

    ██ IKI YARI DA ZORUNLU ██ Yalniz iyi ornegi sinayan bir oz-sinama,
    audit() bosa dusse (her zaman 0 ihlal donse) bile GECERDI. Bu projede bir
    kapi tam olarak bu yuzden dort kez yanlislikla "temiz" dedi, ve bir
    baskasinin oz-sinamasi tam olarak bu yuzden bosa dustu.

    audit_fn parametre olarak alinir; meta_self_check() buraya SABOTE EDILMIS
    surumler gecirip oz-sinamanin kendisini sinar.
    """
    source = Kaynak(SELF_CS)

    # ── BILINEN-IYI: tam bes alan, SIFIR ihlal + beklenen sayaclar ───────
    iyi, istat = audit_fn(Belgeler(SELF_IYI), source)
    if iyi:
        return "bilinen-iyi ornek ihlal uretti: %s -- %s" % (iyi[0][2], iyi[0][3])
    beklenen = {
        "anma": 2, "hukum": 2, "muaf": 0, "disi": 0,
        "l_karsilik": 2, "l_henuz": 0, "l_blok": 0, "l_vurgu": 0, "l_asama": 0,
        "m1": 2, "m1_eksik": 0,
        "m2": 2, "m2_yolsuz": 0, "m2_dosya": 4, "m2_dosya_yok": 0,
        "m2_uye": 2, "m2_uye_yok": 0, "m2_uye_yazilmamis": 1, "m2_metin_degil": 1,
        "m3": 2, "m3_ihlal": 0,
        "m4": 2, "m4_bos": 0, "m4_ogrenmek": 0,
        "m5": 2, "m5_bos": 0,
    }
    for anahtar, deger in sorted(beklenen.items()):
        if istat[anahtar] != deger:
            return ("bilinen-iyi ornekte sayac yanlis: %s=%d, %d bekleniyordu "
                    "-- esnek ayristiricinin bir varyasyonu bosa dusmus olabilir"
                    % (anahtar, istat[anahtar], deger))

    # ── BILINEN-KOTU: ALTI MADDENIN HER BIRI AYRI AYRI ──────────────────
    kotu, kstat = audit_fn(Belgeler(SELF_KOTU), source)
    turler = {}
    for _, _, tur, _ in kotu:
        turler[tur] = turler.get(tur, 0) + 1

    madde_turleri = (
        ("MADDE 1 bes alan", ("ALAN-EKSIK",)),
        ("MADDE 2 kod hedefi", ("BAGLANIR-YOL-YOK", "BAGLANIR-YOL-YANLIS",
                                "BAGLANIR-UYE-YOK", "BAGLANIR-YOLSUZ")),
        ("MADDE 3 satir numarasi", ("BAGLANIR-SATIR-NO",)),
        ("MADDE 4 kararmetre", ("KARARMETRE-BOS", "KARARMETRE-OGRENMEK")),
        ("MADDE 5 arastirma borcu", ("BORC-BOS",)),
    )
    for baslik, istenenler in madde_turleri:
        for istenen in istenenler:
            if istenen not in turler:
                return ("bilinen-kotu ornekte %s YAKALANMADI (%s) -- kapi bu "
                        "maddede kor (yakalananlar: %s)"
                        % (baslik, istenen, ", ".join(sorted(turler)) or "hicbiri"))

    kotu_beklenen = {
        "ALAN-EKSIK": 1,
        "BAGLANIR-YOL-YOK": 1, "BAGLANIR-YOL-YANLIS": 1, "BAGLANIR-UYE-YOK": 1,
        "BAGLANIR-YOLSUZ": 1,
        # ██ DORT LEHCE, DORT IHLAL ██ Biri bile dusarse kapi o lehceye kor.
        "BAGLANIR-SATIR-NO": 4,
        "KARARMETRE-BOS": 1, "KARARMETRE-OGRENMEK": 1,
        "BORC-BOS": 1,
    }
    if turler != kotu_beklenen:
        return ("bilinen-kotu ornekte ihlal dagilimi yanlis: %s, beklenen %s"
                % (sorted(turler.items()), sorted(kotu_beklenen.items())))
    if kstat["hukum"] != len(_KOTU_BOLUMLER):
        return ("bilinen-kotu ornekte %d hukum bekleniyordu, %d sayildi"
                % (len(_KOTU_BOLUMLER), kstat["hukum"]))

    # ── MADDE 6: MUAFIYET, CIT, TABLO, VURGUSUZ, LEHCE ──────────────────
    muaf, mstat = audit_fn(Belgeler(SELF_MUAF), source)
    if muaf:
        return "kapsam disi ornek ihlal uretti: %s -- %s" % (muaf[0][2], muaf[0][3])
    muaf_beklenen = {
        "hukum": 0, "muaf": 1, "d_tablo": 1, "d_vurgusuz": 1, "d_cit": 1,
        "d_dosya": 2, "d_lehce_henuz": 1, "d_lehce_blok": 2,
        "d_lehce_vurgu": 1, "d_lehce_asama": 1,
    }
    for anahtar, deger in sorted(muaf_beklenen.items()):
        if mstat[anahtar] != deger:
            return ("kapsam disi ornekte sayac yanlis: %s=%d, %d bekleniyordu "
                    "-- muafiyet ya da lehce ayrimi bosa dusmus olabilir"
                    % (anahtar, mstat[anahtar], deger))
    # ██ DAGILIM SATIRI DA SINANIR ██ Ilk surumde dagilim ayri bir eslesme
    # kurali kullaniyordu ve muafiyet CALISIRKEN dagilim satiri "0" basiyordu:
    # dogru calisan bir kapinin YANLIS rapor vermesi. Bu sinama olmasa o hata
    # yalniz elle okumayla gorulurdu.
    dagilim_beklenen = {"Docs/handoff/": 1, "Docs/ogrenme/02-sonraki-asamalar.md": 1}
    if mstat["dosya_dagilim"] != dagilim_beklenen:
        return ("dosya muafiyeti DAGILIMI yanlis: %s, beklenen %s -- muafiyet "
                "calissa bile ciktidaki dagilim satiri yalan soyler"
                % (sorted(mstat["dosya_dagilim"].items()),
                   sorted(dagilim_beklenen.items())))

    # ── ZORLAMA: YOK-HUKUM kapsam disini kapsama sokar ──────────────────
    zorla, zstat = audit_fn(Belgeler(SELF_ZORLA), source)
    if zstat["hukum"] != 1:
        return ("YOK-HUKUM isaretcisi calismadi: %d hukum sayildi, 1 "
                "bekleniyordu" % zstat["hukum"])
    if not zorla:
        return "YOK-HUKUM ile kapsama alinan anma hic ihlal uretmedi"

    return None


# ── UCUNCU YARI: OZ-SINAMANIN KENDI SINAMASI ─────────────────────────────
# ██ Bir oz-sinama da bosa dusebilir. ██ Bu projede tam olarak bu oldu: bir
# kapinin oz-sinamasi yalniz IYI ornegi siniyordu ve audit() korlestiginde
# hicbir sey demiyordu. Asagidaki uc sabotaj audit()'i bilerek bozar ve
# self_check()'in UCUNU DE yakaladigini KANITLAR. Yakalamazsa KAPI BOZUKTUR --
# ve bozuk olan denetim degil, DENETIMIN DENETIMIDIR.

def _sabotaj_hep_temiz(docs, source):
    """Hicbir sey gormeyen audit: ne ihlal ne sayac."""
    return [], bos_sayac()


def _sabotaj_liste_yutuk(docs, source):
    """Sayaclari DOGRU veren ama ihlal listesini yutan audit.

    En sinsi sabotaj: iyi ornek yarisi bunu ASLA goremez (orada zaten sifir
    ihlal bekleniyor) ve butun sayaclar dogrudur. Yalniz bilinen-KOTU yarisi
    yakalayabilir.
    """
    _, stat = audit(docs, source)
    return [], stat


def _sabotaj_madde_kor(docs, source):
    """Tek bir maddede korlesen audit: MADDE 3'un ihlallerini duser.

    Sayaclar dogru, oteki bes madde calisiyor. Ihlal dagilimini satir satir
    denetlemeyen bir oz-sinama bunu KACIRIRDI.
    """
    problems, stat = audit(docs, source)
    return [p for p in problems if p[2] != "BAGLANIR-SATIR-NO"], stat


SABOTAJLAR = (
    ("hep-temiz", _sabotaj_hep_temiz),
    ("sayac-dogru-liste-yutuk", _sabotaj_liste_yutuk),
    ("bir-madde-kor", _sabotaj_madde_kor),
)


def meta_self_check():
    """Oz-sinamanin KENDISINI sinar. -> None . hata mesaji."""
    for ad, sabotaj in SABOTAJLAR:
        if self_check(sabotaj) is None:
            return ("oz-sinama '%s' sabotajini YAKALAMADI -- oz-sinama bosa "
                    "dusmus, kapinin denetimi denetlenmiyor" % ad)
    # Saglama: sabotajsiz surum GECMELI. Bu satir olmasa yukaridaki dongu
    # "her seyi reddeden" bir self_check ile de gecerdi.
    if self_check(audit) is not None:
        return ("sabotajsiz audit oz-sinamayi gecemedi: %s" % self_check(audit))
    return None


def main(argv):
    docs_root = pathlib.Path(argv[1] if len(argv) > 1 else DOCS_DEFAULT)
    assets_root = pathlib.Path(argv[2] if len(argv) > 2 else ASSETS_DEFAULT)

    broken = self_check(audit)
    if broken is not None:
        yaz("KAPI BOZUK: %s" % broken)
        return 2
    broken = meta_self_check()
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

    problems, stat = audit(docs, source)

    for key, number, _, text in problems:
        yaz("%s:%d\n    %s" % (key, number, text))

    yaz("")
    yaz("belge: %d  Assets dosyasi: %d" % (len(docs.files), len(source.files)))
    yaz("KAPSAM  : %d yokluk anmasi tarandi -> %d KAPSAM ICI hukum . %d muaf "
        ". %d kapsam disi" % (stat["anma"], stat["hukum"], stat["muaf"], stat["disi"]))
    yaz("  lehce : karsilik %d . HENUZ YOK %d . ██YOK██ %d . **YOK** %d . ASAMA: %d"
        % (stat["l_karsilik"], stat["l_henuz"], stat["l_blok"], stat["l_vurgu"],
           stat["l_asama"]))
    yaz("  ayni satirda ikinci hukum: %d (sayildi, denetlenen ILKIDIR)"
        % stat["satir_coklu"])
    yaz("")
    yaz("MADDE 6 KAPSAM DISI -- ██ IHLAL DEGIL, sayilan bosluk ██  toplam %d"
        % (stat["muaf"] + stat["disi"]))
    yaz("  %4d  satir muafiyeti (%s)" % (stat["muaf"], MUAF))
    yaz("  %4d  cit ici          -- ASCII figur ya da ORNEK, hukum degil"
        % stat["d_cit"])
    yaz("  %4d  tablo hucresi    -- ISARET sutunu; sozlesmesi tablonun kendi sutunlari"
        % stat["d_tablo"])
    yaz("  %4d  vurgusuz duz yazi-- yazar 'bu bir HUKUMDUR' isareti koymamis"
        % stat["d_vurgusuz"])
    yaz("  %4d  dosya muafiyeti  -- dosya dosya:" % stat["d_dosya"])
    for onek, gerekce in DOSYA_MUAF:
        yaz("        %4d  %s" % (stat["dosya_dagilim"].get(onek, 0), onek))
        yaz("              %s" % gerekce)
    yaz("  %4d  lehce HENUZ YOK  -- ISARET; kendi hafif sozlesmesi var "
        "('HENUZ YOK -> o gunu getirecek kosul')" % stat["d_lehce_henuz"])
    yaz("  %4d  lehce ██ YOK ██  -- FIGUR/TABLO isaretcisi" % stat["d_lehce_blok"])
    yaz("  %4d  lehce **YOK**    -- bir cumlenin CEVAP sozcugu, mekanizma "
        "yoklugu hukmu degil" % stat["d_lehce_vurgu"])
    yaz("  %4d  lehce ASAMA:     -- ██ check-curriculum-coverage.py'nin ALANI ██, "
        "buraya girilmez" % stat["d_lehce_asama"])
    # ██ ONCELIK SIRASI SAYACLARI CARPITIR, VE BU YAZILIR ██ Dosya muafiyeti
    # lehceden ONCE uygulaniyor; bir lehcenin butun anmalari muaf bir dosyada
    # ise o lehcenin satiri SIFIR gorunur. Asagidaki satir farki kapatir.
    yaz("  ██ NOT ██ yukaridaki lehce satirlari dosya muafiyetinden SONRAKI "
        "artigi sayar. Lehce TOPLAMLARI (muaf dosyalar dahil):")
    yaz("            karsilik %d . HENUZ YOK %d . ██YOK██ %d . **YOK** %d . ASAMA: %d"
        % (stat["l_karsilik"], stat["l_henuz"], stat["l_blok"], stat["l_vurgu"],
           stat["l_asama"]))
    yaz("")
    yaz("MADDE 1 bes alan       : %d hukum denetlendi . %d EKSIK ALAN tasiyor"
        % (stat["m1"], stat["m1_eksik"]))
    yaz("                         eksik dagilimi: HANGI OZELLIK %d . NEREYE "
        "BAGLANIR %d . NE KIRAR %d . KARARMETRE %d . ARASTIRMA BORCU %d"
        % (stat["m1_ozellik"], stat["m1_baglanir"], stat["m1_kirar"],
           stat["m1_kararmetre"], stat["m1_borc"]))
    yaz("                         yinelenen alan etiketi: %d (ilki tutuldu)"
        % stat["yinelenen_alan"])
    yaz("MADDE 2 kod hedefi     : %d alan denetlendi . %d hic YOL tasimiyor"
        % (stat["m2"], stat["m2_yolsuz"]))
    yaz("                         %d dosya cozuldu . %d dosya YOK . %d yol "
        "YANLIS . %d ad COKLU" % (stat["m2_dosya"], stat["m2_dosya_yok"],
                                  stat["m2_yol_yanlis"], stat["m2_coklu"]))
    yaz("                         %d uye denetlendi . %d uye GECMIYOR . %d uye "
        "yazilmamis . %d hedef metin dosyasi degil"
        % (stat["m2_uye"], stat["m2_uye_yok"], stat["m2_uye_yazilmamis"],
           stat["m2_metin_degil"]))
    # ██ SINIRI KAPI KENDI YAZAR ██ Bu satir olmasa cikti "N uye denetlendi"
    # der ve okuyan bunu bir TANIM guvencesi sanirdi.
    yaz("                         SINIR: uye denetimi \"ad dosyada GECIYOR\" der, "
        "\"dosya bu uyeyi TANIMLIYOR\" demez")
    yaz("MADDE 3 satir numarasi : %d alan denetlendi . %d SATIR NUMARASI TASIYOR "
        "(dort lehce: Dosya.cs:5 . `:5` . L5 . 5. satir)"
        % (stat["m3"], stat["m3_ihlal"]))
    yaz("MADDE 4 kararmetre     : %d alan denetlendi . %d BOS . %d yalnizca "
        "'ogrenmek icin'" % (stat["m4"], stat["m4_bos"], stat["m4_ogrenmek"]))
    # ██ MADDE 4'UN SINIRI, GIZLENMEDEN ██
    yaz("                         ██ SINIR: KAPI CUMLEYI GORUR, HUKMU GOREMEZ ██")
    yaz("                         'bu ozellik gercekten mesru mu' makineyle "
        "denetlenemez; kapi yalnizca alanin BOS olmadigini ve yalnizca")
    yaz("                         'ogrenmek icin' demedigini olcer. Yazar "
        "'hayir, kendi basina istenirdi' yazip yalan soylerse kapi goremez.")
    yaz("MADDE 5 arastirma borcu: %d alan denetlendi . %d BOS "
        "('gerekmiyor' GECERLI bir cevaptir)" % (stat["m5"], stat["m5_bos"]))
    yaz("")
    yaz("ihlal: %d" % len(problems))
    return 1 if problems else 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))

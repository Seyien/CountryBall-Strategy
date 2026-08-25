# Varlik envanteri kapisi: bir ozellik UC katmanda birden var olmali --
# kod, serilesmis bag, varlik. Bu kapi ucuncu katmanin sessizligini kirar.
#
# NEDEN VAR: 2026-08-25'e kadar bu projede iki gun boyunca "ne var, ne yok"
# sorusu olculdu -- kod satirlari, yorum orani, test sayisi, belge atiflari --
# ve HICBIRI varliklara bakmadi. "Bu projede X yok" hukmu hep .cs ve .md
# sayiyordu. Olculdugunde: dusman yapi sprite'i YOKTU, Structure prefabi
# YOKTU, placementGhost nesnesi YOKTU, .asset dosyasi 0, font 0. Operator
# soylemek zorunda kaldi. Kok sebep yapisal: Tools/ altindaki dokuz kapidan
# hicbiri varlik denetlemiyordu -- sprite adini yalniz metin uzantisi olarak
# taniyan 2 kapi vardi, "prefab VAR MI" diye soran 0, Art/ dizinine bakan 0,
# atanmamis alan arayan 0. Korluk araclar tarafindan KORUNUYORDU: yesil dokuz
# kapi, varliklari olmayan bir oyunu yesil geciriyordu.
#
# NE TARAR (uc denetim, ucu de bu projede olculebilirligi dogrulanarak
# secildi):
#   1. GUID COZUMU: .unity/.prefab/.asset icindeki her `guid:` atfi
#      Assets/ altinda bir .meta ile cozulmeli. Unity'nin yerlesik
#      GUID'leri muaf (asagida gerekcesiyle). Assets'te cozulmeyen guid
#      once Library/PackageCache .meta'larinda aranir (paket varligi
#      ihlal degildir); orada da yoksa KIRIK.
#   2. CreateAssetMenu -> .asset: menu tasiyan her ScriptableObject tipi
#      icin o tipin script GUID'ini baglayan en az bir .asset dosyasi
#      olmali. Menu var ama ornek yoksa ozellik yalniz KOD katmaninda
#      yasiyor demektir (bu projede olculen hal: 2 tip, 0 .asset).
#   3. REFERANS TIPLI ALAN -> ANAHTAR: dogrudan MonoBehaviour'dan tureyen
#      bir sinifin [SerializeField] alani Sprite / GameObject / bilesen
#      tipindeyse, o sinifin baglandigi HER sahne/prefab blogu o anahtari
#      tasimali. Sinif hicbir sahne/prefaba baglanmamissa bu, bagin
#      butunuyle yoklugudur ve tek satirda raporlanir.
#
# YERLESIK GUID MUAFIYETI, GEREKCESIYLE: 0000000000000000f000000000000000
# (unity_builtin_extra: UISprite gibi yerlesik varliklar) ve
# 0000000000000000e000000000000000 (unity default resources) Unity kurulumu
# icinde yasar; Assets/ altinda .meta'lari TASARIM GEREGI yoktur. Olcum
# 2026-08-25: f000... bu projede 5 kez geciyor (Unit.prefab:55 dahil) ve
# hicbiri kirik degil. Muafiyetsiz kapi ilk gununde 6 yanlis alarm verirdi.
#
# OZ-SINAMA: bu oturumda ON UC kez bir kontrol yanlis cevap verdi ve her
# seferinde yakalayan sey ayni oldu: bilinen-iyi ve bilinen-KOTU girdi.
# (Sonuncusu: yol onekini kesen bir desen uc saglam isaretciyi "kirik"
# gosterdi.) Bu yuzden main() once ayristiriciyi bellek-ici sanal disk
# uzerinde calistirir: bilinen-iyi ornek 0 ihlal, bilinen-kotu ornek tam 4
# ihlal vermeli; ve ucuncu yari, muafiyet kumesine kirik guid'i bilerek
# ekleyip OZ-SINAMANIN KENDISININ bunu yakaladigini kanitlar. Herhangi biri
# sasarsa cikti "KAPI BOZUK" ve cikis kodu 2'dir.
#
# Cikis kodu sozlesmesi: 0 temiz / 1 ihlal / 2 KAPI BOZUK.
#
# Kullanim (proje kokunden):
#   python Tools/check-asset-inventory.py
#   python Tools/check-asset-inventory.py <baska-kok>   # negatif test icin

import pathlib
import re
import sys

ROOT = pathlib.Path(".")

# Unity kurulumunda yasayan, Assets/ altinda .meta'si TASARIM GEREGI olmayan
# iki yerlesik kutuk. Gerekce ve olcum: dosya basligindaki muafiyet notu.
BUILTIN_GUIDS = frozenset((
    "0000000000000000f000000000000000",   # Resources/unity_builtin_extra
    "0000000000000000e000000000000000",   # Library/unity default resources
))

GUID_REF = re.compile(r"guid: ([0-9a-f]{32})")
META_GUID = re.compile(r"^guid: ([0-9a-f]{32})", re.MULTILINE)
CLASS_DECL = re.compile(r"\bclass\s+([A-Za-z_]\w*)\s*:\s*([A-Za-z_][\w.]*)")
BLOCK_KEY = re.compile(r"^  ([A-Za-z_]\w*):")

# Serilesme acisindan SATIR ICI yazilan tipler: motor bunlari nesne atfi
# olarak degil deger olarak gomer, bu yuzden 3. denetimin disindadir.
# LayerMask/AnimationCurve/Gradient/enum'lar da satir ici serilesir.
# SINIR (bilerek kabul edildi): proje-yerel bir enum bu listede olamaz ve
# referans sanilir. Olcum 2026-08-25: bu projedeki [SerializeField]
# alanlarinda tek enum KeyCode; yerel enum alani sifir.
VALUE_TYPES = frozenset((
    "int", "float", "double", "bool", "string", "byte", "sbyte", "long",
    "ulong", "short", "ushort", "uint", "char", "decimal",
    "Color", "Color32", "Vector2", "Vector3", "Vector4", "Vector2Int",
    "Vector3Int", "Rect", "RectInt", "Bounds", "BoundsInt", "Quaternion",
    "Matrix4x4", "LayerMask", "AnimationCurve", "Gradient", "KeyCode",
))

# Metin olarak okunan uzantilar; geri kalani (png, ttf, ...) yalniz AD
# olarak sayilir -- bu kapi hicbir ikili dosyayi ACMAZ ve bunu asagida
# GOREMEDIKLERI bolumunde kendisi soyler.
TEXT_SUFFIXES = frozenset((".cs", ".meta", ".unity", ".prefab", ".asset"))
ART_SUFFIXES = frozenset((".png", ".jpg", ".jpeg", ".psd", ".ttf", ".otf",
                          ".wav", ".mp3", ".ogg", ".anim", ".spriteatlas"))


class Disk:
    """path -> metin sozlugu.

    Neden bir sinif: oz-sinama diske DOKUNMADAN kosmali (ayni gerekce:
    Tools/check-curriculum-coverage.py ve check-doc-code-refs.py). Gercek
    kosumda from_disk, oz-sinamada duz bir sozluk kullanilir. Paket
    GUID'leri gercek diskte TEMBEL yuklenir (PackageCache ~6000 .meta;
    yalniz Assets'te cozulmeyen bir guid cikarsa okunur), sanal diskte
    kurucuya verilen kumedir.
    """

    def __init__(self, files, art_names=(), package_guids=frozenset()):
        self.files = {k.replace("\\", "/"): v for k, v in files.items()}
        self.art_names = sorted(str(a).replace("\\", "/") for a in art_names)
        self._package_guids = set(package_guids)
        self._package_root = None
        self._package_loaded = True

    @classmethod
    def from_disk(cls, root):
        files = {}
        art = []
        base = pathlib.Path(root)
        for path in sorted((base / "Assets").rglob("*")):
            if not path.is_file():
                continue
            suffix = path.suffix.lower()
            relative = path.relative_to(base).as_posix()
            if suffix in TEXT_SUFFIXES:
                files[relative] = path.read_text(encoding="utf-8",
                                                 errors="replace")
            elif suffix in ART_SUFFIXES:
                art.append(relative)
        disk = cls(files, art)
        disk._package_root = base / "Library" / "PackageCache"
        disk._package_loaded = False
        return disk

    def paths(self, *suffixes):
        return sorted(k for k in self.files
                      if k.lower().endswith(suffixes))

    def text(self, path):
        return self.files[path]

    def in_packages(self, guid):
        """Guid bir paketin .meta'sinda mi? Gercek diskte tembel tarama."""
        if not self._package_loaded:
            self._package_loaded = True
            if self._package_root is not None and self._package_root.is_dir():
                for meta in self._package_root.rglob("*.meta"):
                    match = META_GUID.search(
                        meta.read_text(encoding="utf-8", errors="replace"))
                    if match:
                        self._package_guids.add(match.group(1))
        return guid in self._package_guids


def strip_comments(text):
    """// yorumlarini satir satir soyar.

    Neden once soyma: kural 52'nin sayimi olctu -- bu projede iki yorum
    satiri [SerializeField] lafini geciriyor ve biri tam olarak "serilesmis
    DEGIL" demek icin yaziliyor. Soyulmadan sayan kapi, serilesmeyi REDDEDEN
    yorumu serilesmis alan sayardi.
    SINIRLAR (yamalanmadi, beyan edildi): /* */ blok yorumlari soyulmaz
    (projede yok); dizge icindeki // satiri erken keser (projede yok).
    """
    return "\n".join(line.split("//", 1)[0] for line in text.split("\n"))


def declared_fields(text):
    """[SerializeField] alanlarini (tip, ad) ciftleri olarak cikarir.

    Kural 52'nin kapisiyla ayni duzlestirme: satirlar birlesir, ';' ile
    bolunur, '=' sonrasi atilir, son iki sozcuk tip ve addir. Kural 52'nin
    ilk taslagi ACGOZLU geri izlemeyle 'KeyCode.B'yi ad sanmisti; burada
    '=' bolmesi ILK esittir isaretinde yapilir.
    """
    flat = strip_comments(text).replace("\n", " ")
    pairs = []
    for chunk in flat.split(";"):
        if "[SerializeField" not in chunk:
            continue
        head = chunk.split("=", 1)[0]
        words = head.split()
        if len(words) < 2:
            continue
        pairs.append((words[-2], words[-1]))
    return pairs


def element_type(type_name):
    """Dizi/list sargisini soyar: Sprite[] -> Sprite, List<Text> -> Text."""
    bare = type_name.strip()
    if bare.endswith("[]"):
        bare = bare[:-2]
    match = re.match(r"List<\s*([\w.]+)\s*>", bare)
    if match:
        bare = match.group(1)
    return bare.rsplit(".", 1)[-1]


def is_reference(type_name):
    return element_type(type_name) not in VALUE_TYPES


def first_base(text):
    """Dosyadaki ilk sinif bildiriminin (ad, ilk_taban, satir_no) uclusu.

    SINIR (beyan): dosyada birden cok sinif varsa yalniz ilki taninir, ve
    MonoBehaviour'dan DOLAYLI tureyen (ara taban uzerinden) siniflar
    gorulmez. Olcum 2026-08-25: Assets/ altinda [SerializeField] tasiyan 8
    dosyanin 8'i tek sinifli ve tabani dogrudan yaziyor.
    """
    stripped = strip_comments(text)
    match = CLASS_DECL.search(stripped)
    if not match:
        return None
    line = stripped[:match.start()].count("\n") + 1
    return match.group(1), match.group(2).rsplit(".", 1)[-1], line


def meta_guid_of(disk, cs_path):
    meta = cs_path + ".meta"
    if meta not in disk.files:
        return None
    match = META_GUID.search(disk.text(meta))
    return match.group(1) if match else None


def script_blocks(text):
    """Bir sahne/prefabin MonoBehaviour bloklarini cozer.

    Donen her oge: (script_guid, anahtar_kumesi, m_Script_satir_no).
    Blok siniri '--- !u!' belge ayracidir; 114 MonoBehaviour'dur. Anahtarlar
    iki bosluk girintili en-ust-duzey alanlardir; motorun kendi m_* ve
    serializedVersion anahtarlari elenir (kural 52'nin kapisiyla ayni
    suzgec). Unity'nin yazicisi m_Script'i kullanici alanlarindan once
    yazar; elle yazilmis prefablar da bu projede ayni sirayi koruyor.
    """
    blocks = []
    guid = None
    keys = set()
    line_of_script = None
    in_mono = False
    for number, line in enumerate(text.split("\n"), 1):
        if line.startswith("--- !u!"):
            if in_mono and guid:
                blocks.append((guid, keys, line_of_script))
            in_mono = line.startswith("--- !u!114")
            guid = None
            keys = set()
            line_of_script = None
            continue
        if not in_mono:
            continue
        if "m_Script:" in line:
            match = GUID_REF.search(line)
            if match:
                guid = match.group(1)
                line_of_script = number
            continue
        match = BLOCK_KEY.match(line)
        if match:
            key = match.group(1)
            if not key.startswith("m_") and key != "serializedVersion":
                keys.add(key)
    if in_mono and guid:
        blocks.append((guid, keys, line_of_script))
    return blocks


def audit(disk, exempt=BUILTIN_GUIDS):
    """Uc denetimi kosar; (ihlal listesi, sayac sozlugu) doner.

    Ihlal sekli: (yol, satir_no|None, tur, mesaj). tur oz-sinamada aranir --
    yalniz sayi denetleyen bir oz-sinama, dogru SAYIDA yanlis TURDE ihlali
    gecirirdi (kural 52'nin kapisi tam boyle bir taslagi yakalamisti).
    """
    problems = []
    stat = {
        "guid_ref": 0, "guid_ok": 0, "guid_builtin": 0, "guid_paket": 0,
        "guid_kirik": 0,
        "menu_tip": 0, "menu_ornekli": 0, "menu_orneksiz": 0,
        "mono": 0, "mono_ref_alanli": 0, "mono_bagli": 0, "mono_bagsiz": 0,
        "alan_ref": 0, "alan_eksik": 0,
        "meta_yok": 0,
    }

    metas = set()
    for meta_path in disk.paths(".meta"):
        match = META_GUID.search(disk.text(meta_path))
        if match:
            metas.add(match.group(1))

    carriers = disk.paths(".unity", ".prefab", ".asset")

    # ── 1. GUID COZUMU ──────────────────────────────────────────────────
    for carrier in carriers:
        for number, line in enumerate(disk.text(carrier).split("\n"), 1):
            for match in GUID_REF.finditer(line):
                guid = match.group(1)
                stat["guid_ref"] += 1
                if guid in exempt:
                    stat["guid_builtin"] += 1
                elif guid in metas:
                    stat["guid_ok"] += 1
                elif disk.in_packages(guid):
                    stat["guid_paket"] += 1
                else:
                    stat["guid_kirik"] += 1
                    problems.append((carrier, number, "KIRIK-GUID",
                                     "hicbir .meta bu guid'i tasimiyor: %s"
                                     % guid))

    # ── 2. CreateAssetMenu -> .asset ────────────────────────────────────
    asset_texts = [(p, disk.text(p)) for p in disk.paths(".asset")]
    for cs in disk.paths(".cs"):
        text = disk.text(cs)
        if "[CreateAssetMenu" not in strip_comments(text):
            continue
        stat["menu_tip"] += 1
        decl = first_base(text)
        name = decl[0] if decl else cs.rsplit("/", 1)[-1]
        line = decl[2] if decl else None
        guid = meta_guid_of(disk, cs)
        if guid is None:
            stat["meta_yok"] += 1
            problems.append((cs, line, "META-YOK",
                             "%s: script .meta tasimiyor, hicbir .asset ona "
                             "baglanamaz" % name))
            continue
        needle = "guid: " + guid
        if any(needle in body for _, body in asset_texts):
            stat["menu_ornekli"] += 1
        else:
            stat["menu_orneksiz"] += 1
            problems.append((cs, line, "ASSET-YOK",
                             "%s: CreateAssetMenu VAR, onu baglayan .asset "
                             "YOK -- ozellik yalniz kod katmaninda" % name))

    # ── 3. REFERANS TIPLI ALAN -> ANAHTAR ───────────────────────────────
    blocks_by_guid = {}
    for carrier in disk.paths(".unity", ".prefab"):
        for guid, keys, line in script_blocks(disk.text(carrier)):
            blocks_by_guid.setdefault(guid, []).append((carrier, keys, line))

    for cs in disk.paths(".cs"):
        text = disk.text(cs)
        decl = first_base(text)
        if decl is None or decl[1] != "MonoBehaviour":
            continue
        stat["mono"] += 1
        fields = declared_fields(text)
        refs = [(t, n) for t, n in fields if is_reference(t)]
        if not refs:
            continue
        stat["mono_ref_alanli"] += 1
        stat["alan_ref"] += len(refs)
        name, _, line = decl
        guid = meta_guid_of(disk, cs)
        if guid is None:
            stat["meta_yok"] += 1
            problems.append((cs, line, "META-YOK",
                             "%s: script .meta tasimiyor, hicbir sahne/prefab "
                             "ona baglanamaz" % name))
            continue
        bindings = blocks_by_guid.get(guid, [])
        if not bindings:
            stat["mono_bagsiz"] += 1
            problems.append((cs, line, "BAG-YOK",
                             "%s: %d referans alani var ama HICBIR sahne/"
                             "prefab bu script'i tasimiyor -- bag katmani "
                             "butunuyle yok" % (name, len(refs))))
            continue
        stat["mono_bagli"] += 1
        for carrier, keys, block_line in bindings:
            for type_name, field in refs:
                if field not in keys:
                    stat["alan_eksik"] += 1
                    problems.append((carrier, block_line, "ANAHTAR-YOK",
                                     "%s.%s (%s) anahtari bu blokta yok -- "
                                     "alan TIP VARSAYILANI (null) yukler"
                                     % (name, field, type_name)))

    return problems, stat


# ── OZ-SINAMA ORNEKLERI ──────────────────────────────────────────────────
# Ikisi de bellek-ici sanal disk; diske dokunulmaz. Bir dis dosyaya bagimli
# oz-sinama, o dosya tasindigi gun kapiyi da sessizce oldururdu.

GOOD_META = "fileFormatVersion: 2\nguid: %s\n"

SELF_GOOD = {
    # Iyi MonoBehaviour: bir referans alani (prefabta anahtari VAR), bir
    # deger alani, ve iki tuzak yorum -- soyulmazlarsa sahte alan/menu
    # cikarir (kural 52'nin sayim dersi).
    "Assets/Iyi.cs": (
        "using UnityEngine;\n"
        "// [SerializeField] DEGIL: bu satir bir tuzak\n"
        "// [CreateAssetMenu] lafi gecen bir yorum, menu degil\n"
        "public sealed class Iyi : MonoBehaviour\n"
        "{\n"
        "    [SerializeField] private Sprite yuzey;\n"
        "    [SerializeField] private int adet = 3;\n"
        "}\n"),
    "Assets/Iyi.cs.meta": GOOD_META % ("aa" * 16),
    # Iyi ScriptableObject: menusu VAR, .asset ornegi VAR.
    "Assets/IyiVeri.cs": (
        "using UnityEngine;\n"
        "[CreateAssetMenu(menuName = \"Iyi/Veri\")]\n"
        "public sealed class IyiVeri : ScriptableObject\n"
        "{\n"
        "    [SerializeField] private string ad = \"veri\";\n"
        "}\n"),
    "Assets/IyiVeri.cs.meta": GOOD_META % ("bb" * 16),
    "Assets/Veri.asset": (
        "--- !u!114 &11400000\n"
        "MonoBehaviour:\n"
        "  m_Script: {fileID: 11500000, guid: %s, type: 3}\n"
        "  ad: veri\n" % ("bb" * 16)),
    "Assets/Veri.asset.meta": GOOD_META % ("dd" * 16),
    # Iyi prefab: Iyi'ye baglanir, referans anahtari tasir, bir sprite
    # guid'i COZULUR, bir de YERLESIK guid gecer -- muafiyet burada sinanir.
    "Assets/Iyi.prefab": (
        "--- !u!1 &100\n"
        "GameObject:\n"
        "  m_Name: Iyi\n"
        "--- !u!114 &200\n"
        "MonoBehaviour:\n"
        "  m_Script: {fileID: 11500000, guid: %s, type: 3}\n"
        "  yuzey: {fileID: 21300000, guid: %s, type: 3}\n"
        "  adet: 3\n"
        "--- !u!212 &300\n"
        "SpriteRenderer:\n"
        "  m_Sprite: {fileID: 10754, guid: 0000000000000000f000000000000000, "
        "type: 0}\n" % ("aa" * 16, "cc" * 16)),
    "Assets/Iyi.prefab.meta": GOOD_META % ("ee" * 16),
    "Assets/Art/kare.png.meta": GOOD_META % ("cc" * 16),
}

# Kirik guid: hicbir .meta "99"*16 tasimiyor.
BAD_BROKEN_GUID = "9" * 32

SELF_BAD = {
    # 1) BAG-YOK: referans alanli MonoBehaviour, hicbir yerde bagli degil.
    "Assets/Kotu.cs": (
        "using UnityEngine;\n"
        "public sealed class Kotu : MonoBehaviour\n"
        "{\n"
        "    [SerializeField] private GameObject hedef;\n"
        "}\n"),
    "Assets/Kotu.cs.meta": GOOD_META % ("11" * 16),
    # 2) ASSET-YOK: menu var, .asset yok.
    "Assets/KotuVeri.cs": (
        "using UnityEngine;\n"
        "[CreateAssetMenu(menuName = \"Kotu/Veri\")]\n"
        "public sealed class KotuVeri : ScriptableObject\n"
        "{\n"
        "    [SerializeField] private int adet;\n"
        "}\n"),
    "Assets/KotuVeri.cs.meta": GOOD_META % ("22" * 16),
    # 3) ANAHTAR-YOK + 4) KIRIK-GUID ayni prefabta: Bagli.cs'in referans
    # alani 'gorunum' blokta YOK (deger alani 'sayi' da yok ama o ihlal
    # DEGIL -- negatif-negatif sinama tam bunu olcer), ve prefab cozulmeyen
    # bir guid'e atif yapar.
    "Assets/Bagli.cs": (
        "using UnityEngine;\n"
        "public sealed class Bagli : MonoBehaviour\n"
        "{\n"
        "    [SerializeField] private SpriteRenderer gorunum;\n"
        "    [SerializeField] private float sayi = 1f;\n"
        "}\n"),
    "Assets/Bagli.cs.meta": GOOD_META % ("33" * 16),
    "Assets/Kotu.prefab": (
        "--- !u!114 &200\n"
        "MonoBehaviour:\n"
        "  m_Script: {fileID: 11500000, guid: %s, type: 3}\n"
        "  baskaAnahtar: {fileID: 21300000, guid: %s, type: 3}\n"
        % ("33" * 16, BAD_BROKEN_GUID)),
    "Assets/Kotu.prefab.meta": GOOD_META % ("44" * 16),
}

# Bilinen-kotu ornekten beklenen ihlal turleri, tam kume olarak.
BAD_EXPECTED = ("BAG-YOK", "ASSET-YOK", "ANAHTAR-YOK", "KIRIK-GUID")


def self_check(sabotage=False):
    """Ayristirici ve denetciyi once kendi uzerlerinde kanitlar.

    Iki yari: bilinen-iyi disk 0 ihlal, bilinen-kotu disk tam 4 ihlal ve
    tam 4 TUR vermeli (sayi degil TUR karsilastirilir -- dogru sayida
    yanlis turde ihlal de yakalanmali). UCUNCU YARI (sabotage=True):
    muafiyet kumesine kirik guid bilerek eklenir; KIRIK-GUID ihlali
    kaybolmali ve bu fonksiyon bunu HATA olarak raporlamali. Sabotajli
    kosum None donerse oz-sinamanin kendisi bosa dusmus demektir.
    """
    good_disk = Disk(SELF_GOOD)
    good_problems, good_stat = audit(good_disk)
    if good_problems:
        return ("bilinen-iyi ornek ihlal uretti: %s" %
                (good_problems[0][3],))
    if good_stat["guid_builtin"] != 1:
        return ("bilinen-iyi ornekte yerlesik-guid muafiyeti 1 kez "
                "islemeliydi, %d kez isledi" % good_stat["guid_builtin"])
    if good_stat["menu_ornekli"] != 1 or good_stat["menu_tip"] != 1:
        return ("bilinen-iyi ornekte menu sayimi yanlis: tip %d, ornekli %d"
                % (good_stat["menu_tip"], good_stat["menu_ornekli"]))
    if good_stat["alan_ref"] != 1 or good_stat["alan_eksik"] != 0:
        return ("bilinen-iyi ornekte alan sayimi yanlis: ref %d, eksik %d"
                % (good_stat["alan_ref"], good_stat["alan_eksik"]))

    exempt = BUILTIN_GUIDS | ({BAD_BROKEN_GUID} if sabotage else set())
    bad_disk = Disk(SELF_BAD)
    bad_problems, _ = audit(bad_disk, exempt=exempt)
    kinds = sorted(kind for _, _, kind, _ in bad_problems)
    if kinds != sorted(BAD_EXPECTED):
        return ("bilinen-kotu ornek %s vermeliydi, %s verdi"
                % (sorted(BAD_EXPECTED), kinds))

    return None


def main(argv):
    root = pathlib.Path(argv[1]) if len(argv) > 1 else ROOT

    if not (root / "Assets").is_dir():
        print("KAPI BOZUK: denetlenecek Assets koku yok -> %s" % root)
        return 2

    # Ucuncu yari ONCE kosar: sabotajli oz-sinama HATA DONDURMEK ZORUNDA.
    # None donerse ihlal yutulmus ve oz-sinama gormemis demektir -- boyle
    # bir oz-sinama hicbir seyi kanitlamaz.
    if self_check(sabotage=True) is None:
        print("KAPI BOZUK: sabotajli oz-sinama hata vermedi -- oz-sinamanin "
              "kendisi bosa dusmus")
        return 2

    broken = self_check()
    if broken is not None:
        print("KAPI BOZUK: %s" % broken)
        return 2

    disk = Disk.from_disk(root)

    # Bos taramayi kutlama danisi: tek bir tasiyici ya da tek bir .meta
    # yoksa sorun projede degil kapidadir (yol yanlis, uzanti degismis).
    if not disk.paths(".unity", ".prefab", ".asset"):
        print("KAPI BOZUK: tek bir sahne/prefab/.asset bulunamadi -> %s"
              % (root / "Assets"))
        return 2
    if not disk.paths(".meta"):
        print("KAPI BOZUK: tek bir .meta bulunamadi -> %s" % (root / "Assets"))
        return 2

    problems, stat = audit(disk)

    for path, number, kind, message in problems:
        where = "%s:%s" % (path, number) if number else path
        print("%s\n    %s: %s" % (where, kind, message))

    art_count = len(disk.art_names)
    font_count = sum(1 for a in disk.art_names
                     if a.lower().endswith((".ttf", ".otf")))
    print(
        "\nKATMAN SAYIMI (uc katman, ayri ayri -- kural: hukum ucunu birden"
        " sayar)"
        "\n  kod    : %d .cs"
        "\n  bag    : %d .unity . %d .prefab . %d .asset"
        "\n  varlik : %d sanat dosyasi (%d font)"
        % (len(disk.paths(".cs")),
           len(disk.paths(".unity")), len(disk.paths(".prefab")),
           len(disk.paths(".asset")), art_count, font_count))
    print(
        "\nDENETIM 1 guid cozumu   : %d atif . %d cozuldu . %d yerlesik-muaf"
        " . %d pakette . %d KIRIK"
        % (stat["guid_ref"], stat["guid_ok"], stat["guid_builtin"],
           stat["guid_paket"], stat["guid_kirik"]))
    print(
        "DENETIM 2 menu -> .asset: %d CreateAssetMenu tipi . %d ornekli . "
        "%d ORNEKSIZ"
        % (stat["menu_tip"], stat["menu_ornekli"], stat["menu_orneksiz"]))
    print(
        "DENETIM 3 alan -> anahtar: %d MonoBehaviour . %d referans-alanli . "
        "%d bagli . %d BAGSIZ . %d anahtar EKSIK (%d referans alani tarandi)"
        % (stat["mono"], stat["mono_ref_alanli"], stat["mono_bagli"],
           stat["mono_bagsiz"], stat["alan_eksik"], stat["alan_ref"]))
    if stat["meta_yok"]:
        print("  .meta'siz script: %d" % stat["meta_yok"])

    # ██ GOREMEDIKLERI -- YESIL KOSUDA DA BASILIR ██ Bu bolum olmasa yesil
    # cikti "varlik katmani saglam" diye okunurdu; kapi yalniz VARLIGIN
    # VARLIGINI olcer, icerigini ve dogrulugunu olcmez.
    print(
        "\nBU KAPININ GOREMEDIKLERI (yesilken de gecerli):"
        "\n  1. Dolu bir alanin DOGRU nesneyi tuttugunu goremez --"
        "\n     unitPrefab'in YANLIS prefaba isaret etmesi yesildir."
        "\n  2. Bir sprite'in anlamli bir goruntu tasidigini goremez --"
        "\n     kapi hicbir ikili dosyayi ACMAZ; 1x1 seffaf PNG yesildir."
        "\n  3. Calisma zamaninda atanan alani unutulmus alandan ayiramaz --"
        "\n     GetComponent/OnValidate ile dolan alan, bos alanla ozdes"
        " gorunur."
        "\n  4. Prefab ORNEGI uzerindeki m_Modifications girdilerini anahtar"
        "\n     taramasi bulamaz -- propertyPath kayitlari bu desenin"
        " disindadir."
        "\n  5. VAR olan bir anahtarin degerini yorumlamaz -- 'terrainSprites:'"
        "\n     anahtari BOS bir diziyle de doluyla da ayni gorunur."
        "\n  6. Dolayli MonoBehaviour turevlerini ve dosyadaki ikinci sinifi"
        "\n     gormez; proje-yerel enum alanini referans sanir (bugun: 0)."
        "\n  7. ScriptableObject alanlarinin .asset ICINDEKI anahtarlarini"
        "\n     taramaz -- denetim 2 yalniz orneklerin varligini sayar.")

    print("\nihlal: %d" % len(problems))
    return 1 if problems else 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))

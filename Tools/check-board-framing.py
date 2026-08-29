# Cerceve bayatlama kapisi: sahnedeki kamera cercevesi ile tahtanin
# olcusu ayni tahtayi mi tarif ediyor?
#
# NEDEN VAR: 2026-08-29'da operator tahtayi 100x50'den 5x10'a indirdi.
# BoardAdapter.width/height degisti; BoardCameraRig.boardRect ile
# homeCentre/homeHalfHeight ise sahnede 100x50 olarak KALDI, cunku o dort
# sayiyi yazan tek yer bir Editor menusuydu (SceneSetupTool.AttachCameraRig)
# ve menu kosmadi. Kamera artik var olmayan bir adayi cerceveliyordu,
# oynanabilir alan sol alt kosede kaliyordu ve ekrani 100x50 icin serilmis
# kumsal plakasi kapliyordu. Bunu HICBIR SEY yakalamadi: derleyici sustu,
# yesil EditMode testleri sustu, dokuz kapi sustu -- cunku hicbiri sahne
# YAML'ini bir SAYIYLA baska bir sayiya karsi okumuyordu.
#
# NE TARAR (tek denetim, bilerek tek):
#   1. CERCEVE TUTARLILIGI: ayni sahnede hem BoardAdapter hem BoardCameraRig
#      varsa, rig'in `boardRect` genisligi/yuksekligi tahtanin
#      `width`/`height` degeriyle ayni olmali.
#
# BU KAPI BUGUN NEYI KORUYOR: ayni turdaki hata artik OYUNU bozmuyor --
# BoardAdapter.Awake cerceveyi calisma zamaninda BoardFraming.Frame ile
# yeniden turetiyor ve rig'e yaziyor. Geriye kalan sey SAHNE GORUNUMU:
# Play'e basilmadan Scene penceresinde gorulen cerceve. Bu kapinin urunu
# bir onizleme dogrulugudur ve ayrica turetmenin geri alinip alinmadiginin
# nobetcisidir -- birisi Awake'teki cagriyi silerse bu kapi yine kirmizi
# dondugu gun haber verir.
#
# OZ-SINAMA: main() once ayristiriciyi bellek-ici sanal disk uzerinde
# kosturur. Bilinen-iyi ornek 0 ihlal, bilinen-kotu ornek tam 1 ihlal ve
# tam 1 TUR vermeli. UCUNCU YARI (sabotage=True): karsilastirma tolerans
# 1e9 ile korlestirilir; ihlal kaybolmali ve OZ-SINAMA BUNU YAKALAMALI.
# Sabotajli kosum None donerse oz-sinama bosa dusmus demektir.
#
# Cikis kodu sozlesmesi: 0 temiz / 1 ihlal / 2 KAPI BOZUK.
#
# Kullanim (proje kokunden):
#   python Tools/check-board-framing.py
#   python Tools/check-board-framing.py <baska-kok>   # negatif test icin

import pathlib
import re
import sys

ROOT = pathlib.Path(".")

# Ayristirma bicimi check-asset-inventory.py'den alindi ve tek yerde
# genisletildi: orada blok ANAHTARLARI toplaniyordu, burada DEGERLERI de
# gerekiyor. Yeni bir ayristirici icat edilmedi.
META_GUID = re.compile(r"^guid: ([0-9a-f]{32})", re.MULTILINE)
GUID_REF = re.compile(r"guid: ([0-9a-f]{32})")
TOP_KEY = re.compile(r"^  ([A-Za-z_]\w*):(.*)$")
NESTED_KEY = re.compile(r"^    ([A-Za-z_]\w*): (.*)$")

# Tahtayi ve rig'i tasiyan kaynak dosyalar. Guid'leri .meta'dan okunuyor;
# guid'i buraya YAZMAK, dosya yeniden olusturuldugu gun sessizce olurdu.
BOARD_SCRIPT = "Assets/Game/Unity/BoardAdapter.cs"
RIG_SCRIPT = "Assets/Game/Unity/BoardCameraRig.cs"

# Kayan noktali YAML degerlerinde makul tolerans. Tahta olculeri TAM SAYI
# oldugu icin gercek kullanimda 0 da yeterdi; tolerans, rig'in Rect alanini
# 5 yerine 5.0000001 yazan bir Unity surumune karsi duruyor.
TOLERANCE = 0.001


class Disk:
    """path -> metin sozlugu.

    Neden bir sinif: oz-sinama diske DOKUNMADAN kosmali. Ayni gerekce
    Tools/check-asset-inventory.py ve check-doc-code-refs.py icinde de
    yazili; kalibi oradan geliyor.
    """

    TEXT_SUFFIXES = (".cs", ".meta", ".unity", ".prefab")

    def __init__(self, files):
        self.files = {k.replace("\\", "/"): v for k, v in files.items()}

    @classmethod
    def from_disk(cls, root):
        files = {}
        base = pathlib.Path(root)
        for path in sorted((base / "Assets").rglob("*")):
            if not path.is_file():
                continue
            if path.suffix.lower() not in cls.TEXT_SUFFIXES:
                continue
            relative = path.relative_to(base).as_posix()
            files[relative] = path.read_text(encoding="utf-8", errors="replace")
        return cls(files)

    def paths(self, *suffixes):
        return sorted(k for k in self.files if k.lower().endswith(suffixes))

    def text(self, path):
        return self.files[path]


def meta_guid_of(disk, cs_path):
    meta = cs_path + ".meta"
    if meta not in disk.files:
        return None
    match = META_GUID.search(disk.text(meta))
    return match.group(1) if match else None


def script_blocks(text):
    """Bir sahnenin MonoBehaviour bloklarini cozer.

    Donen her oge: (script_guid, degerler, m_Script_satir_no). degerler
    sozlugunde duz anahtarlar skaler dize, ic ice anahtarlar (boardRect
    gibi) alt sozluk tasir.

    SINIR (beyan): diziler ('  - {fileID: ...}' satirlari) atlanir; bu
    kapinin okudugu iki alanin ikisi de dizi degil.
    """
    blocks = []
    guid = None
    values = {}
    nested_into = None
    line_of_script = None
    in_mono = False

    for number, line in enumerate(text.split("\n"), 1):
        if line.startswith("--- !u!"):
            if in_mono and guid:
                blocks.append((guid, values, line_of_script))
            in_mono = line.startswith("--- !u!114")
            guid = None
            values = {}
            nested_into = None
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

        nested = NESTED_KEY.match(line)
        if nested is not None and nested_into is not None:
            values[nested_into][nested.group(1)] = nested.group(2).strip()
            continue

        top = TOP_KEY.match(line)
        if top is None:
            continue

        key = top.group(1)
        rest = top.group(2).strip()
        if rest:
            values[key] = rest
            nested_into = None
        else:
            values[key] = {}
            nested_into = key

    if in_mono and guid:
        blocks.append((guid, values, line_of_script))
    return blocks


def number_of(raw):
    """YAML skalerini sayiya cevirir; cevrilemezse None."""
    try:
        return float(raw)
    except (TypeError, ValueError):
        return None


def audit(disk, tolerance=TOLERANCE):
    """Tek denetimi kosar; (ihlal listesi, sayac sozlugu) doner.

    Ihlal sekli: (yol, satir_no|None, tur, mesaj). tur oz-sinamada aranir --
    yalniz SAYI denetleyen bir oz-sinama, dogru sayida yanlis turde ihlali
    gecirirdi.
    """
    problems = []
    stat = {
        "sahne": 0, "tahtali": 0, "rigli": 0, "karsilastirilan": 0,
        "celiskili": 0, "okunamayan": 0,
    }

    board_guid = meta_guid_of(disk, BOARD_SCRIPT)
    rig_guid = meta_guid_of(disk, RIG_SCRIPT)
    if board_guid is None or rig_guid is None:
        problems.append((BOARD_SCRIPT, None, "META-YOK",
                         "BoardAdapter ya da BoardCameraRig .meta tasimiyor; "
                         "hicbir sahne blogu onlara baglanamaz"))
        return problems, stat

    for scene in disk.paths(".unity"):
        stat["sahne"] += 1
        boards = []
        rigs = []
        for guid, values, line in script_blocks(disk.text(scene)):
            if guid == board_guid:
                boards.append((values, line))
            elif guid == rig_guid:
                rigs.append((values, line))

        if boards:
            stat["tahtali"] += 1
        if rigs:
            stat["rigli"] += 1

        # TEK TARAF VARSA SESSIZ: rig'siz bir sahne gezinemez ama bozuk
        # degildir, tahtasiz bir rig ise karsilastirilacak bir sayi
        # bulamaz. Ikisi de bu kapinin soyleyecegi bir sey birakmiyor.
        if not boards or not rigs:
            continue

        for board_values, _ in boards:
            width = number_of(board_values.get("width"))
            height = number_of(board_values.get("height"))
            if width is None or height is None:
                stat["okunamayan"] += 1
                problems.append((scene, None, "OKUNAMADI",
                                 "BoardAdapter blogunda width/height yok ya da "
                                 "sayi degil"))
                continue

            for rig_values, rig_line in rigs:
                rect = rig_values.get("boardRect")
                rect_width = number_of(rect.get("width")) if isinstance(rect, dict) else None
                rect_height = number_of(rect.get("height")) if isinstance(rect, dict) else None
                if rect_width is None or rect_height is None:
                    stat["okunamayan"] += 1
                    problems.append((scene, rig_line, "OKUNAMADI",
                                     "BoardCameraRig blogunda boardRect yok ya da "
                                     "genislik/yukseklik sayi degil"))
                    continue

                stat["karsilastirilan"] += 1
                if (abs(rect_width - width) > tolerance
                        or abs(rect_height - height) > tolerance):
                    stat["celiskili"] += 1
                    problems.append((
                        scene, rig_line, "CERCEVE-BAYAT",
                        "boardRect %gx%g, tahta ise %gx%g -- kamera cercevesi "
                        "baska bir tahtayi tarif ediyor"
                        % (rect_width, rect_height, width, height)))

    return problems, stat


# ---- OZ-SINAMA ORNEKLERI -------------------------------------------------
# Ikisi de bellek-ici sanal disk; diske dokunulmaz. Bir dis dosyaya bagimli
# oz-sinama, o dosya tasindigi gun kapiyi da sessizce oldururdu.

BOARD_GUID = "a" * 32
RIG_GUID = "b" * 32

GOOD_META = "fileFormatVersion: 2\nguid: %s\n"

SCRIPTS = {
    BOARD_SCRIPT: "public sealed class BoardAdapter : MonoBehaviour { }\n",
    BOARD_SCRIPT + ".meta": GOOD_META % BOARD_GUID,
    RIG_SCRIPT: "public sealed class BoardCameraRig : MonoBehaviour { }\n",
    RIG_SCRIPT + ".meta": GOOD_META % RIG_GUID,
}


def scene_text(board_width, board_height, rect_width, rect_height):
    """Iki blokluk en kucuk gecerli sahne.

    Tuzak satirlari BILEREK var: 'width' adli bir anahtar RIG blogunda da
    gecebilir (boardRect'in ICINDE) ve iki bosluk girintili bir dizi de
    bulunur. Ayristirici ikisini de dogru ayirmali.
    """
    return (
        "--- !u!114 &100\n"
        "MonoBehaviour:\n"
        "  m_Script: {fileID: 11500000, guid: %s, type: 3}\n"
        "  width: %d\n"
        "  height: %d\n"
        "  terrainSprites:\n"
        "  - {fileID: 21300000, guid: %s, type: 3}\n"
        "  borderThickness: 1\n"
        "--- !u!114 &200\n"
        "MonoBehaviour:\n"
        "  m_Script: {fileID: 11500000, guid: %s, type: 3}\n"
        "  boardRect:\n"
        "    serializedVersion: 2\n"
        "    x: 0\n"
        "    y: 0\n"
        "    width: %d\n"
        "    height: %d\n"
        "  homeHalfHeight: 8.49\n"
        "  panButton: 1\n"
        % (BOARD_GUID, board_width, board_height, "c" * 32,
           RIG_GUID, rect_width, rect_height))


SELF_GOOD = dict(SCRIPTS)
SELF_GOOD["Assets/Scenes/Iyi.unity"] = scene_text(5, 10, 5, 10)

SELF_BAD = dict(SCRIPTS)
SELF_BAD["Assets/Scenes/Kotu.unity"] = scene_text(5, 10, 100, 50)

BAD_EXPECTED = ("CERCEVE-BAYAT",)


def self_check(sabotage=False):
    """Ayristiriciyi ve denetciyi once kendi uzerlerinde kanitlar."""
    good_problems, good_stat = audit(Disk(SELF_GOOD))
    if good_problems:
        return "bilinen-iyi ornek ihlal uretti: %s" % (good_problems[0][3],)
    if good_stat["karsilastirilan"] != 1:
        return ("bilinen-iyi ornekte 1 karsilastirma olmaliydi, %d oldu"
                % good_stat["karsilastirilan"])

    tolerance = 1e9 if sabotage else TOLERANCE
    bad_problems, bad_stat = audit(Disk(SELF_BAD), tolerance=tolerance)
    kinds = sorted(kind for _, _, kind, _ in bad_problems)
    if kinds != sorted(BAD_EXPECTED):
        return ("bilinen-kotu ornek %s vermeliydi, %s verdi"
                % (sorted(BAD_EXPECTED), kinds))
    if bad_stat["karsilastirilan"] != 1:
        return ("bilinen-kotu ornekte 1 karsilastirma olmaliydi, %d oldu"
                % bad_stat["karsilastirilan"])

    return None


def main(argv):
    root = pathlib.Path(argv[1]) if len(argv) > 1 else ROOT

    if not (root / "Assets").is_dir():
        print("KAPI BOZUK: denetlenecek Assets koku yok -> %s" % root)
        return 2

    # UCUNCU YARI ONCE KOSAR: sabotajli oz-sinama HATA DONDURMEK ZORUNDA.
    # None donerse ihlal yutulmus ve oz-sinama gormemis demektir.
    if self_check(sabotage=True) is None:
        print("KAPI BOZUK: sabotajli oz-sinama hata vermedi -- oz-sinamanin "
              "kendisi bosa dusmus")
        return 2

    broken = self_check()
    if broken is not None:
        print("KAPI BOZUK: %s" % broken)
        return 2

    disk = Disk.from_disk(root)

    # Bos taramayi kutlama danisi: tek bir sahne yoksa sorun projede degil
    # kapidadir (yol yanlis, uzanti degismis).
    if not disk.paths(".unity"):
        print("KAPI BOZUK: tek bir sahne bulunamadi -> %s" % (root / "Assets"))
        return 2

    problems, stat = audit(disk)

    for path, number, kind, message in problems:
        where = "%s:%s" % (path, number) if number else path
        print("%s\n    %s: %s" % (where, kind, message))

    print(
        "\nDENETIM cerceve tutarliligi: %d sahne . %d tahtali . %d rigli . "
        "%d karsilastirma . %d CELISKILI . %d okunamayan"
        % (stat["sahne"], stat["tahtali"], stat["rigli"],
           stat["karsilastirilan"], stat["celiskili"], stat["okunamayan"]))

    # BU KAPININ GOREMEDIKLERI -- YESIL KOSUDA DA BASILIR. Bu bolum olmasa
    # yesil cikti "cerceve dogru" diye okunurdu; kapi yalniz IKI SAYININ
    # BIRBIRINE UYDUGUNU olcer.
    print(
        "\nBU KAPININ GOREMEDIKLERI (yesilken de gecerli):"
        "\n  1. homeCentre ve homeHalfHeight'i HIC denetlemez -- boardRect"
        "\n     dogru, merkez yanlis olabilir ve bu kapi yesil kalir."
        "\n  2. 0. katman plakalarinin (OpenSea/Shoal/Beach) boyuna bakmaz;"
        "\n     ekrani kaplayan kumsalin kendisini goremez."
        "\n  3. Calisma zamanini goremez: BoardAdapter.Awake'teki turetme"
        "\n     silinse bile, sahnedeki iki sayi uyustugu surece yesildir."
        "\n  4. Panel paylarinin gercek panellerle uyustugunu goremez --"
        "\n     ScreenMargin degisirse iki sayi yine birbirini tutar."
        "\n  5. Prefab ORNEGI uzerindeki m_Modifications girdilerini gormez;"
        "\n     propertyPath kayitlari bu desenin disindadir."
        "\n  6. Sahnede AYNI tipten birden cok blok varsa hepsini herkesle"
        "\n     karsilastirir; hangi rig'in hangi tahtaya ait oldugunu"
        "\n     bilmez (bugun: 1 tahta, 1 rig).")

    print("\nihlal: %d" % len(problems))
    return 1 if problems else 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))

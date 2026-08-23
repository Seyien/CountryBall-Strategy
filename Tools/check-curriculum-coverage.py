# Kavram borc defteri kapisi: her satir bir SAHIP ya da bir ASAMA tasimali.
#
# NEDEN VAR: Docs/ogrenme/03-kavram-borc-defteri.md bir kapsama denetimi.
# Icindeki tek olumcul hata, VAR OLMAYAN bir dosyaya "KAPALI" yazmaktir:
# ogreneni tam bir guvenle bosluga gonderir, hicbir yerde kirmizi gorunmez ve
# defter kendini dogrulamis gibi durur. Bayat bir satir numarasi da aynidir --
# dosya acilir, orada baska bir sey yazar, okuyan kendini sucalar.
#
# NE TARAR (yalniz defterdeki 4 sutunlu KAVRAM tablolari):
#   1. DURUM hucresi bos mu, ya da uc degerden biri degil mi
#   2. KAPALI / KISMI ise: SAHIP DOSYA gercekten var mi
#   3. Satir numarasi verilmisse: dosya o kadar satir tasiyor mu
#   4. HENUZ YOK ise: onu yaratacak ASAMA yazili mi
#   5. KAVRAM, SAHIP DOSYA ve KANIT hucrelerinden biri bos mu
#
# OZ-SINAMA (bu dosyanin en pahali satirlari): bu projede bir kapi, sozluk
# anahtarlarini yanlis normalize ettigi icin DORT KEZ yanlislikla "temiz" dedi
# (kaydi: Tools/check-doc-links.py:65). Sessizce yesil yanan bir kapi, hic
# olmayan bir kapidan DAHA KOTUDUR: birincisi guven uretir. Bu yuzden asagidaki
# main() once kendi ayristiricisini bilinen-iyi ve bilinen-KOTU iki ornek
# uzerinde calistirir; ikisinden biri beklenmedik cevap verirse "KAPI BOZUK"
# der ve sifir disi kodla cikar.
#
# Kullanim:
#   python Tools/check-curriculum-coverage.py
#   python Tools/check-curriculum-coverage.py <baska-bir-defter.md>   # negatif test

import pathlib
import sys
import unicodedata

ROOT = pathlib.Path(".")
DEFAULT_LEDGER = "Docs/ogrenme/03-kavram-borc-defteri.md"

# Defter tablosunun basligi. Katlanmis (aksansiz, buyuk harf) hali aranir.
HEADER = ("KAVRAM", "SAHIP DOSYA", "DURUM", "KANIT")

# DURUM hucresinin kabul edilen uc degeri -- katlanmis hali.
STATES = {"KAPALI", "KISMI", "HENUZ YOK"}
NEEDS_OWNER = {"KAPALI", "KISMI"}

# HENUZ YOK satirinda SAHIP DOSYA hucresinin tasimasi gereken on ek.
STAGE_PREFIX = "ASAMA:"

# Asamanin adi bu kadar karakterden kisaysa yazilmamis sayilir. Olcu keyfi
# degil: "ASAMA: yok" gibi bir hucre kapiyi gecerdi ve hicbir sey soylemezdi.
STAGE_MIN = 8


def fold(text):
    """Turkce aksanlari sadelestirir ve buyuk harfe cevirir.

    Neden elle harf esleme: Turkce noktasiz i (U+0131) ve noktali buyuk I
    (U+0130) Unicode katlamasinda beklenmedik davranir. "KISMI" ile "KISMI"
    (noktali) ayni deger sayilmali, ama bunu casefold'a birakmak guvenilmez.
    Bu yuzden once elle degistiriyoruz, sonra kalan aksanlari NFKD ile
    ayirip birlesen isaretleri atiyoruz.
    """
    for source, target in (
        ("İ", "I"), ("ı", "i"),
        ("Ş", "S"), ("ş", "s"),
        ("Ğ", "G"), ("ğ", "g"),
        ("Ç", "C"), ("ç", "c"),
        ("Ö", "O"), ("ö", "o"),
        ("Ü", "U"), ("ü", "u"),
    ):
        text = text.replace(source, target)
    text = unicodedata.normalize("NFKD", text)
    text = "".join(c for c in text if not unicodedata.combining(c))
    return text.upper().strip()


def cells_of(line):
    """Bir markdown tablo satirini hucrelere boler; tablo degilse None."""
    stripped = line.strip()
    if not stripped.startswith("|") or not stripped.endswith("|"):
        return None
    parts = stripped.split("|")
    # Bas ve son bos parcalar bordurden geliyor.
    return [p.strip() for p in parts[1:-1]]


def is_separator(cells):
    """--- satiri mi (baslikla govdeyi ayiran)."""
    return bool(cells) and all(set(c) <= set("-: ") and "-" in c for c in cells)


def parse(text):
    """Defter metnini satir listesine cevirir.

    Donen her ogenin sekli: (satir_numarasi, kavram, sahip, durum, kanit).
    Yalniz HEADER basligini gormus bir tablonun govdesindeki 4 hucreli
    satirlar alinir; dosyadaki diger tablolar (2 ve 6 sutunlu) disarida kalir.
    """
    rows = []
    in_table = False
    in_fence = False

    for number, line in enumerate(text.split("\n"), 1):
        # KOD BLOKLARI ATLANIR: defterin kendisi bir tablo satirini ORNEK
        # olarak gosteriyor ve o satir gercek bir kayit degil. Cit takibi
        # olmasaydi kapi kendi kullanim kilavuzunu denetlemeye kalkardi.
        if line.lstrip().startswith("```"):
            in_fence = not in_fence
            in_table = False
            continue

        if in_fence:
            continue

        cells = cells_of(line)

        if cells is None:
            in_table = False
            continue

        if len(cells) == len(HEADER) and tuple(fold(c) for c in cells) == HEADER:
            in_table = True
            continue

        if not in_table:
            continue

        if is_separator(cells):
            continue

        if len(cells) != len(HEADER):
            # Defter tablosunun ortasinda sutun sayisi degisiyorsa bu bir
            # bicim hatasidir; satiri BOS DURUM ile geciriyoruz ki kapi
            # sessizce atlamasin.
            cells = (cells + ["", "", "", ""])[:len(HEADER)]

        rows.append((number, cells[0], cells[1], cells[2], cells[3]))

    return rows


def split_owner(owner):
    """`yol` ya da `yol:satir` hucresini (yol, satir|None) olarak ayirir."""
    text = owner.strip().strip("`").strip()
    if ":" in text:
        head, _, tail = text.rpartition(":")
        if head and tail.isdigit():
            return head.strip(), int(tail)
    return text, None


def line_count(path):
    """wc -l ile ayni sayiyi verir: sondaki bos parca sayilmaz."""
    lines = path.read_text(encoding="utf-8", errors="replace").split("\n")
    if lines and lines[-1] == "":
        lines.pop()
    return len(lines)


def audit(rows, root):
    """Satirlari denetler; (ihlal listesi, durum sayaci) doner."""
    problems = []
    counter = {"KAPALI": 0, "KISMI": 0, "HENUZ YOK": 0}

    for number, concept, owner, state, proof in rows:
        folded = fold(state)

        if not concept:
            problems.append((number, "KAVRAM hucresi BOS"))

        if not proof:
            problems.append((number, "KANIT hucresi BOS"))

        if not state:
            problems.append((number, "DURUM hucresi BOS"))
            continue

        if folded not in STATES:
            problems.append((number, "DURUM tanimsiz: %s" % state))
            continue

        counter[folded] += 1

        if not owner:
            problems.append((number, "SAHIP DOSYA hucresi BOS"))
            continue

        # TERS TIRNAK ONCE SOYULUR: defterde hucreler `...` icinde yaziliyor
        # ve on ek denetimi ham metne uygulandiginda ASLA eslesmez. Bu satir
        # bir uslup duzeltmesi degil -- oz-sinama tam olarak burayi yakaladi.
        bare = owner.strip().strip("`").strip()
        starts_with_stage = fold(bare).startswith(STAGE_PREFIX)

        if folded == "HENUZ YOK":
            if not starts_with_stage:
                problems.append((
                    number,
                    "HENUZ YOK satirinda asama yazili degil: %s" % owner))
                continue
            rest = bare[bare.find(":") + 1:].strip()
            if len(rest) < STAGE_MIN:
                problems.append((
                    number,
                    "HENUZ YOK satirinin asama adi cok kisa: %s" % owner))
            continue

        # Buradan asagisi yalniz KAPALI ve KISMI.
        if starts_with_stage:
            problems.append((
                number,
                "%s satirinda dosya bekleniyordu, asama yazilmis: %s" % (state, owner)))
            continue

        relative, cited = split_owner(owner)
        if not relative:
            problems.append((number, "SAHIP DOSYA cozulemedi: %s" % owner))
            continue

        target = root / relative
        if not target.is_file():
            problems.append((number, "SAHIP DOSYA YOK: %s" % relative))
            continue

        if cited is not None:
            total = line_count(target)
            if cited < 1 or cited > total:
                problems.append((
                    number,
                    "SATIR NUMARASI DOSYAYI ASIYOR: %s:%d (dosya %d satir)"
                    % (relative, cited, total)))

    return problems, counter


# ── OZ-SINAMA ORNEKLERI ──────────────────────────────────────────────────
# Ikisi de bu dosyanin icinde duruyor, diskte degil: bir dis dosyaya
# bagimli oz-sinama, o dosya silindigi gun kapiyi da sessizce oldururdu.

SELF_GOOD = """
| KAVRAM | SAHİP DOSYA | DURUM | KANIT |
|---|---|---|---|
| Bilinen iyi bir kavram | `%(ledger)s:1` | KAPALI | ilk satir |
| Yarim kalmis bir kavram | `%(ledger)s` | KISMİ | satir numarasi verilmedi |
| Henuz olmayan bir kavram | `AŞAMA: bu satiri yaratacak asama burada yazili` | HENÜZ YOK | eksik |
"""

SELF_BAD = """
| KAVRAM | SAHİP DOSYA | DURUM | KANIT |
|---|---|---|---|
| Olmayan dosyaya KAPALI | `Docs/ogrenme/bu-dosya-asla-yok.md:1` | KAPALI | yakalanmali |
"""


def self_check(root, ledger_relative):
    """Ayristiricinin CALISTIGINI once kendi uzerinde kanitlar.

    Iki sinav: bilinen-iyi metin tam 3 satir ve 0 ihlal vermeli;
    bilinen-KOTU metin tam 1 satir ve EN AZ 1 ihlal vermeli. Ikincisi,
    kapinin bir "her zaman temiz" fonksiyonuna donusmedigini kanitlar.
    """
    good = parse(SELF_GOOD % {"ledger": ledger_relative})
    if len(good) != 3:
        return "bilinen-iyi ornekten 3 satir bekleniyordu, %d cikti" % len(good)

    good_problems, good_counter = audit(good, root)
    if good_problems:
        return "bilinen-iyi ornek ihlal uretti: %s" % (good_problems[0][1],)
    if good_counter != {"KAPALI": 1, "KISMI": 1, "HENUZ YOK": 1}:
        return "bilinen-iyi ornekte durum sayaci yanlis: %s" % (good_counter,)

    bad = parse(SELF_BAD)
    if len(bad) != 1:
        return "bilinen-kotu ornekten 1 satir bekleniyordu, %d cikti" % len(bad)

    bad_problems, _ = audit(bad, root)
    if not bad_problems:
        return "bilinen-kotu ornek YAKALANMADI -- kapi her zaman temiz diyor"

    return None


def main(argv):
    relative = argv[1] if len(argv) > 1 else DEFAULT_LEDGER
    ledger = ROOT / relative

    if not ledger.is_file():
        print("KAPI BOZUK: denetlenecek defter bulunamadi -> %s" % relative)
        return 2

    broken = self_check(ROOT, DEFAULT_LEDGER)
    if broken is not None:
        print("KAPI BOZUK: %s" % broken)
        return 2

    rows = parse(ledger.read_text(encoding="utf-8"))

    # Defterde tek bir satir bile ayristirilamiyorsa sorun defterde degil
    # burada: tablo basligi degismis ya da ayristirici bozulmus olabilir.
    # Bu dal olmasaydi kapi bos bir listeyi "sifir ihlal" diye kutlardi.
    if not rows:
        print("KAPI BOZUK: defterde tek bir KAVRAM satiri ayristirilamadi -> %s"
              % relative)
        return 2

    problems, counter = audit(rows, ROOT)

    for number, text in problems:
        print("%s:%d\n    %s" % (relative, number, text))

    print(
        "\nkavram satiri: %d  (KAPALI %d . KISMI %d . HENUZ YOK %d)"
        "\nihlal: %d"
        % (len(rows), counter["KAPALI"], counter["KISMI"],
           counter["HENUZ YOK"], len(problems))
    )
    return 1 if problems else 0


if __name__ == "__main__":
    sys.exit(main(sys.argv))

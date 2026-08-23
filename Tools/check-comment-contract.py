# Yorum sozlesmesi kapisi — COMMENT_CONTRACT.md'nin makine karsiligi.
#
# NEDEN VAR: kural 38 "reddedilen secenek KOD olarak gosterilir" diyordu ama
# SEKLI hic tarif etmemisti, ve tarif edilmeyen sey serbestce sisti. 2026-08-20
# olcumu: 161 blok, ortalama 19,5 satir, en uzunu 48 satir, toplam ~3100 satir
# yorum. Ogrenenin ifadesi: "ders kitaplarinda ezber niteliginde duygusuz bir
# tanim gibi".
#
# UC SERT SINIR:
#   1  KIRILAN en fazla 6 satir
#   2  numarali liste ((1) (2) (3)) bir blogun icinde YASAK — o blok aslinda
#      birden fazla karardir ve bolunmelidir
#   3  her blok TEK CUMLE ile biter — blogun URUNU o satirdir
#
# Kullanim:
#   python Tools/check-comment-contract.py            ihlalleri listeler
#   python Tools/check-comment-contract.py --stats    yalnizca ozet

import argparse
import pathlib
import re
import sys

HEADER = re.compile(r"^\s*//\s*REDDEDILEN\s*-")
KIRILAN = re.compile(r"^\s*//\s*KIRILAN\s*:")
KAZANIRDI = re.compile(r"^\s*//\s*KAZANIRDI\s*:")
TEK_CUMLE = re.compile(r"^\s*//\s*TEK CUMLE\s*:")
KARSILASTIRMA = re.compile(r"^\s*//\s*KARSILASTIRMA\s*:")
COMMENT = re.compile(r"^\s*//")

# "(1)" "(2)" ... ya da satir basinda "1." "2." — blok icinde numarali liste.
# Numarali liste SATIR BASINDA durur: "// (1) ..." ya da "// 1. ...".
# Eski desen ciplak "(\d)" ariyordu ve yorumda gecen her API cagrisini
# yakaliyordu: [Min(1)], [Range(0,1)], Input.GetMouseButton(0). Iki ayri
# worker bunu bagimsiz olarak bildirdi ve ikisi de metni BOZARAK gecti --
# yani kapi dogru yazilmis yorumu cezalandiriyordu.
NUMBERED = re.compile(r"^\s*//\s*(\(\d\)|\d[.)])\s")

KIRILAN_MAX = 6


def blocks(lines):
    """Her REDDEDILEN blogunu (baslangic, bitis) olarak dondurur."""
    i = 0
    while i < len(lines):
        if HEADER.match(lines[i]):
            # Blok, bir sonraki BASLIKTA da biter. Yoksa arka arkaya duran iki
            # blok (aralarinda kod satiri olmayan) tek blok sayilir ve hem
            # sayim hem uzunluk olcumu yanlis cikar — olculdu: 161 blok 148
            # gorunuyordu.
            j = i + 1
            while j < len(lines) and COMMENT.match(lines[j]) and not HEADER.match(lines[j]):
                j += 1
            yield i, j
            i = j
        else:
            i += 1


def section_length(lines, start, end, marker):
    """Isaretli satirdan sonraki bolumun satir sayisi (isaretli satir dahil)."""
    for i in range(start, end):
        if marker.match(lines[i]):
            j = i + 1
            while j < end and not (
                KAZANIRDI.match(lines[j])
                or TEK_CUMLE.match(lines[j])
                or KARSILASTIRMA.match(lines[j])
            ):
                j += 1
            return j - i
    return None


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--stats", action="store_true")
    args = parser.parse_args()

    violations = {"uzun": [], "numarali": [], "teksiz": []}
    total = 0
    lengths = []

    for path in sorted(pathlib.Path("Assets").rglob("*.cs")):
        lines = path.read_text(encoding="utf-8").split("\n")
        for start, end in blocks(lines):
            total += 1
            lengths.append(end - start)
            where = f"{path}:{start + 1}"

            length = section_length(lines, start, end, KIRILAN)
            if length is not None and length > KIRILAN_MAX:
                violations["uzun"].append(f"{where}  KIRILAN {length} satir (en fazla {KIRILAN_MAX})")

            if any(NUMBERED.search(lines[i]) for i in range(start, end)):
                violations["numarali"].append(f"{where}  numarali liste — blok bolunmeli")

            if not any(TEK_CUMLE.match(lines[i]) for i in range(start, end)):
                violations["teksiz"].append(f"{where}  TEK CUMLE yok")

    if not args.stats:
        for name, items in violations.items():
            for item in items:
                print(f"{name.upper():9} {item}")

    count = sum(len(v) for v in violations.values())
    average = sum(lengths) / len(lengths) if lengths else 0
    print(
        f"\nblok: {total}  ortalama: {average:.1f} satir  en uzun: {max(lengths) if lengths else 0}"
        f"\nihlal: {count}  (uzun {len(violations['uzun'])} . "
        f"numarali {len(violations['numarali'])} . TEK CUMLE'siz {len(violations['teksiz'])})"
    )
    return 1 if count else 0


if __name__ == "__main__":
    sys.exit(main())

# -*- coding: utf-8 -*-
"""Olcek tavani kapisi.

NE ARIYOR: maliyeti, tasarimcinin degistirebildigi bir SAYIYLA buyuyen ama o
sayinin buyuyecegi hic yazilmamis kod. Ornegi olculdu ve bu kapinin var olma
sebebi o: `BoardAdapter.BuildCellVisuals` hucre basina bir GameObject
kuruyordu. 10x5 tahtada (50 nesne) kusursuzdu; operator tahtayi 100x50 yapinca
5000 nesne dogdu ve HICBIR SEY kirmiziya donmedi -- ne derleyici, ne 700 test,
ne dokuz kapi. Kusuru bulan sey Console'daki bir bilgi satiriydi.

NE ARAMIYOR: hizi. Bu kapi hicbir sey olcmez ve olcemez. Yalnizca sunu sorar:
olcekle buyuyen bir tahsis noktasinin YANINDA, o olcegin tavani YAZILI MI.

SINIR (yesilken de gecerli):
  1. Yazili tavanin DOGRU olup olmadigini goremez. "OLCEK: 5000 hucre" yazip
     10 milyon hucre calistirmak bu kapida yesildir.
  2. Dolayli tahsisi goremez: dongu bir metot cagiriyorsa ve tahsis o metodun
     icindeyse, kapi donguyu temiz sanir.
  3. LINQ, koleksiyon buyumesi ve rekursiyon bu desenin disinda.
  4. Yalnizca `for` donguleri taranir; `foreach` ve `while` disarida, cunku
     onlarin sinirini metinden okumak guvenilir degil.
"""

import io
import pathlib
import re
import sys

ROOT = pathlib.Path("Assets")

# Olcekle buyuyen sinir: tahtanin boyu ya da hucre sayisi.
SCALE_TOKENS = ("Width", "Height", "CellCount", "width", "height")

# Dongu govdesinde pahali olan sey.
ALLOC_PATTERNS = (
    "new GameObject",
    "AddComponent<",
    "AddComponent(",
    "Instantiate(",
    "CreateInstance<",
)

# Tavanin yazili hali. Buyuk harf zorunlu -- bir yorumun icinde gecen siradan
# bir kelimeyle karisMASIN diye.
CEILING = re.compile(r"OLCEK|ÖLÇEK")

# Yorum satirini tanir. KURAL 14: yorumlar SAYILMAZ -- bu repo reddedilen
# alternatifleri yorum olarak kodda tutuyor, yani yorumdaki `new GameObject`
# bir bulgu degildir.
COMMENT = re.compile(r"^\s*(//|/\*|\*)")

# Tavanin dongunun kac satir ustunde aranacagi. Genis, cunku bu repoda gerekce
# bloklari uzun.
LOOKBACK = 30


def strip_comment(line):
    """Satirin kod kismini verir; tamami yorumsa bos dizge."""
    if COMMENT.match(line):
        return ""
    # Satir ici yorumu kes. Dizge literali icindeki `//` nadirdir ve bu kapinin
    # arayacagi desenlerde hic yok.
    cut = line.find("//")
    return line if cut < 0 else line[:cut]


def loop_is_scale_bound(code_line):
    if "for (" not in code_line and "for(" not in code_line:
        return False
    return any(token in code_line for token in SCALE_TOKENS)


def body_allocates(lines, start):
    """Dongunun govdesinde tahsis var mi. Suslu parantez sayarak sinir bulunur."""
    depth = 0
    seen_open = False

    for index in range(start, min(start + 60, len(lines))):
        code = strip_comment(lines[index])
        depth += code.count("{")
        if code.count("{"):
            seen_open = True
        depth -= code.count("}")

        if any(pattern in code for pattern in ALLOC_PATTERNS):
            return index

        if seen_open and depth <= 0:
            break

    return -1


def has_ceiling(lines, loop_index):
    start = max(0, loop_index - LOOKBACK)
    for index in range(start, loop_index):
        if COMMENT.match(lines[index]) and CEILING.search(lines[index]):
            return True
    return False


def main():
    if not ROOT.exists():
        print("KAPI BOZUK: kaynak koku bulunamadi -> Assets")
        return 2

    scanned = 0
    loops = 0
    findings = []

    for path in sorted(ROOT.rglob("*.cs")):
        text = io.open(path, encoding="utf-8", errors="ignore").read()
        lines = text.split("\n")
        scanned += 1

        for index, raw in enumerate(lines):
            code = strip_comment(raw)
            if not loop_is_scale_bound(code):
                continue

            loops += 1
            alloc = body_allocates(lines, index)
            if alloc < 0:
                continue

            if has_ceiling(lines, index):
                continue

            findings.append((path, index + 1, alloc + 1, code.strip()))

    for path, loop_line, alloc_line, code in findings:
        print("{}:{}".format(path, loop_line))
        print("    TAVANSIZ: olcekle buyuyen dongu, {}. satirda tahsis yapiyor".format(alloc_line))
        print("    {}".format(code))

    print("")
    print("dosya: {} . olcek-bagli dongu: {} . TAVANSIZ TAHSIS: {}".format(
        scanned, loops, len(findings)))
    print("")
    print("BU KAPININ GOREMEDIKLERI (yesilken de gecerli):")
    print("  1. Yazili tavanin DOGRU oldugunu goremez -- yalnizca yazili")
    print("     oldugunu gorur.")
    print("  2. Dolayli tahsisi goremez: dongu bir metot cagirip tahsisi ona")
    print("     yaptiriyorsa bu kapi donguyu temiz sanir.")
    print("  3. foreach ve while taranmaz; sinirlarini metinden okumak")
    print("     guvenilir degil.")
    print("  4. Hiçbir sey OLCMEZ. Bu bir profiler degil, bir yazim kapisi.")

    return 1 if findings else 0


if __name__ == "__main__":
    sys.exit(main())

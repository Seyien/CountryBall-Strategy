# §15 kapisi: yorumlar TAM AKSANLI Turkce yazilir (LANE_PROTOCOL §15).
#
# NEDEN VAR: "dogrulamasi" ile "dogrulaması" derleyici icin ayni, okuyan icin
# degil. §15 mutlak bir kural ve mutlak kurallarin makine kapisi olmazsa
# worker'lar arasinda sessizce asinir.
#
# OLCULMUS TUZAK — bu scriptin var olma bicimini bu belirledi:
# "grep -i" ile aramak 385 SAHTE POZITIF uretti. Sebep Turkce noktasiz i:
# UTF-8 yerelinde buyuk/kucuk katlama "ı" (U+0131) ile "i" harfini AYNI kabul
# eder, yani DOGRU yazilmis "aynı" kelimesi "ayni" desenine eslesir. Kural:
# bu taramada buyuk/kucuk duyarsiz eslesme KULLANILMAZ.
#
# Kullanim:
#   python Tools/check-comment-language.py

import pathlib
import re
import sys

# Dogru Turkce yazilisinda MUTLAKA aksanli harf tasiyan kelimeler.
# Aksansiz da dogru olan kelimeler (birim, kural, silinir, kaynak) bilerek YOK.
ASCII_TURKISH = [
    "icin", "degil", "cunku", "oldugu", "ayni", "dogru", "yanlis",
    "calisir", "calisma", "calisan", "degisir", "degistir", "degisen",
    "kirilir", "kazanir", "sozluk", "cagiran", "olculdu", "olcum", "kanit",
    "ogrenen", "yazilir", "gecerli", "tasir", "dusuk", "buyuk", "kucuk",
    "gorsel", "hucre", "savas", "saldiri", "yasam", "dongu", "sinir",
    "deger", "gerekce", "karsilik", "bagimsiz", "bagli", "gecis", "basarisiz",
    "bayragi", "kelepce", "sozlesme", "varsayim", "tiklama", "bosluk",
    "araligi", "disarida", "yukseklik", "genislik", "sonuc",
]

# re.IGNORECASE BILEREK YOK — yukaridaki noktasiz-i tuzagi.
PATTERN = re.compile(r"//.*\b(" + "|".join(ASCII_TURKISH) + r")\b")

COMMENT = re.compile(r"^\s*(//|///)")


def main():
    root = pathlib.Path("Assets")
    hits = []

    for path in sorted(root.rglob("*.cs")):
        for number, line in enumerate(path.read_text(encoding="utf-8").split("\n"), start=1):
            if PATTERN.search(line):
                hits.append((path, number, line.strip()))

    for path, number, text in hits:
        print(f"{path}:{number}\n    {text[:100]}")

    print(f"\naksansiz Turkce yorum satiri: {len(hits)}")
    return 1 if hits else 0


if __name__ == "__main__":
    sys.exit(main())

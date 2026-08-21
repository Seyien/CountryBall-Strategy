# Yorumlarda anilan TEST ADLARI ve TIPLER gercekten var mi (LANE_PROTOCOL §16 ailesi).
#
# NEDEN VAR: 2026-08-20'de bir worker, blok metninde anilan
# "TargetingRulesTests.CanBeAttacked_AllStates" testinin HIC VAR OLMADIGINI
# buldu. Var olmayan bir teste atif, bayat bir satir numarasi kadar kotudur:
# ogreneni tam bir guvenle bosluga gonderir, ve yesil ekran bunu HIC gostermez.
#
# NE TARAR:
#   1  yorum icinde gecen test adi bicimindeki tanimlayicilar
#      (Word_Word_Word — en az iki alt cizgi, CamelCase parcalar)
#   2  <see cref="X"/> ve <seealso cref="X"/> hedefleri
#
# YANLIS POZITIF POLITIKASI: bu tarama IDDIA denetler, kod denetlemez. Bir ad
# projede HERHANGI bir yerde geciyorsa gecerli sayilir; nerede gectigine
# bakilmaz. Amac "uydurulmus ad" yakalamak, yerini dogrulamak degil.
#
# Kullanim:
#   python Tools/check-cited-names.py

import pathlib
import re
import sys

COMMENT = re.compile(r"^\s*///?")

# REDDEDILEN blogunun ICINDEKI KOD: "//" + bes ya da daha fazla bosluk. O satirlar
# bir ATIF degil, reddedilen secenegin KENDISIDIR — icinde gecen test adi zaten
# var olmayan bir seyi tarif eder ve var olmamasi DOGRUDUR. Olculdu: 12 adayin
# ikisi buydu (DamageRulesTests, MoveActionTests).
REJECTED_CODE = re.compile(r"^\s*//\s{5,}\S")

# Ornek: Execute_DeadTargetOutOfRange_PrefersInvalidTargetOverOutOfRange
TEST_NAME = re.compile(r"\b([A-Z][A-Za-z0-9]*(?:_[A-Za-z0-9]+){2,})\b")

# Ornek: <see cref="TargetingRules.CanBeRevived(UnitState)"/>
CREF = re.compile(r'cref="([^"(]+)')

# Bu adlar tanimlayici degil; taramanin disinda.
IGNORE = {"UNITY_INCLUDE_TESTS"}

# BILEREK ANILAN SILINMIS TEST: bir borcu sabitleyen test kapandigi gun silinir
# ve yerini alan test onu ADIYLA anar — gecis sessiz olmasin diye. O ad artik
# kodda yoktur ve olmamasi DOGRUDUR. Ama okuyan biri onu aramaya kalkar, bu
# yuzden isaretlenmek zorunda:
#
#     ... yerini aldigi test: Move_DownedUnit_StillMoves_... (SILINDI)
#
# Isaretci hem taramayi susturur hem OKUYANI uyarir. Ikinci is birincisinden
# onemli.
# Iki ayri olgu, iki ayri isaretci:
#   (SILINDI)  test artik YOK ve olmamasi dogru
#   (ESKI AD)  test VAR ama baska adla — okuyan yeni adi aramali
# Ikisini tek isaretciye indirmek, okuyana yanlis bilgi verir.
DELETED_MARK = re.compile(r"\((SILINDI|ESKI AD)\)")


def main():
    root = pathlib.Path("Assets")
    sources = {}
    for path in root.rglob("*.cs"):
        sources[path] = path.read_text(encoding="utf-8")
    haystack = "\n".join(sources.values())

    missing = []

    for path, text in sources.items():
        lines = text.split("\n")
        for number, line in enumerate(lines, start=1):
            if not COMMENT.match(line) or REJECTED_CODE.match(line):
                continue

            # SATIR SONU BOLUNMESI: uzun bir test adi iki yoruma bolunmus
            # olabilir ("..._StillRevives_Because" + "NoRuleOwnsReviverState").
            # Ardisik yorum satirlarini birlestirip oyle tara, yoksa tarama
            # kendi bicimlendirmemizi uydurma ad sanar — olculdu: 12 adayin
            # ikisi buydu.
            joined = line.rstrip()
            look = number
            while look < len(lines) and COMMENT.match(lines[look]):
                nxt = lines[look]
                # Yalnizca bolunmus GORUNEN satirlari yapistir: onceki satir
                # alt cizgi ya da harf ile bitiyor ve sonraki yorum isaretinden
                # hemen sonra harfle basliyor.
                if joined.endswith("_") or joined.endswith("-"):
                    joined = joined.rstrip("-") + nxt.lstrip().lstrip("/").strip()
                    look += 1
                    continue
                break

            names = set(TEST_NAME.findall(joined))
            for target in CREF.findall(line):
                # "TargetingRules.CanBeRevived" -> son parca yeter
                names.add(target.split(".")[-1].strip())

            # Isaretci PENCEREYLE aranir: uzun bir test adi satiri doldurdugunda
            # "(SILINDI)" dogal olarak BIR SONRAKI satira duser. Yalniz kendi
            # satirina bakan bir kapi, dogru isaretlenmis bir atifi uydurma
            # sanar — olculdu.
            window = chr(10).join(lines[max(0, number - 2):number + 2])
            if DELETED_MARK.search(window):
                continue

            for name in names:
                if name in IGNORE or len(name) < 4:
                    continue
                # Yorum satirlarinin DISINDA gecmeli — yoksa bir yorumun kendisi
                # kendini dogrular ve tarama hicbir sey yakalamaz.
                pattern = re.compile(r"\b" + re.escape(name) + r"\b")
                found = False
                for other_text in sources.values():
                    for other_line in other_text.split("\n"):
                        if COMMENT.match(other_line):
                            continue
                        if pattern.search(other_line):
                            found = True
                            break
                    if found:
                        break
                if not found:
                    missing.append((path, number, name))

    for path, number, name in missing:
        print(f"{path}:{number}\n    UYDURMA ADAY: {name}")

    print(f"\nkodda karsiligi OLMAYAN anilan ad: {len(missing)}")
    return 1 if missing else 0


if __name__ == "__main__":
    sys.exit(main())

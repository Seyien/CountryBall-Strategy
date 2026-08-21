# CAPRAZ DOSYA SATIR ATFI YASAK (LANE_PROTOCOL §16 ve §26 ailesi).
#
# NEDEN VAR: 2026-08-20'de bir surme, 71 capraz iddianin 16'sinin yanlis
# oldugunu buldu ve DOKUZU tek bir siniftandi — baska bir dosyanin SATIRINA
# yapilan duz yazi atfi:
#
#     "gerekcesi BoardAdapter.cs:205-230'da"  -> orada moveRange alani var
#     "MoveAction.cs:206 = board.MoveUnit"     -> MoveUnit 186'da
#     "GridDistance.cs:55-78"                  -> dosya 75 SATIR
#
# §16 yalniz KENDI dosyasindaki REDDEDILEN capalarini tazeler. Baska bir
# dosyanin satirina yapilan atfi HICBIR SEY tazelemez: hedef dosya her
# duzenlendiginde atif cürür, ve curudugunu kimse gormez.
#
# KURAL: capraz dosya satir atfi YAZILMAZ. Yerine UYE ADI yazilir.
#
#     YASAK : "gerekcesi BoardAdapter.cs:277-285'te"
#     DOGRU : "gerekcesi BoardAdapter'in unitViews alaninin ustunde"
#
# Uye adi da degisebilir — ama degistigi gun DERLEYICI onu gosterir, cunku ad
# kodda da geciyordur. Satir numarasini hicbir sey gostermez. Ayrim budur.
#
# ISTISNA: kendi dosyasina yapilan atiflar (REDDEDILEN capalari dahil). Onlarin
# sahibi resolve-rejected-anchors.py ve her gecis sonunda tazeleniyorlar.
#
# Muafiyet SATIRA degil ATFIN KENDISINE ait — bir capa satirina binmis capraz
# atif da yasaktir ve gorulur.
#
# Kullanim:
#   python Tools/check-cross-file-refs.py

import pathlib
import re
import sys

REF = re.compile(r"([A-Za-z0-9_]+\.cs):(\d+)(?:-(\d+))?")
COMMENT = re.compile(r"^\s*///?")
OWN_ANCHOR = re.compile(r"^\s*//\s*REDDEDILEN\s*-")


def main():
    hits = []

    for path in sorted(pathlib.Path("Assets").rglob("*.cs")):
        lines = path.read_text(encoding="utf-8").split("\n")
        for number, line in enumerate(lines, start=1):
            if not COMMENT.match(line):
                continue

            # KOR NOKTA, olculdu 2026-08-20: bir REDDEDILEN basligi ile AYNI
            # SATIRA binmis capraz atif gorunmez kaliyordu, cunku kapi basligi
            # taniyip SATIRIN TAMAMINI atliyordu. Gercek ornek:
            #
            #     // REDDEDILEN - TargetingRules.cs:60 yerine (Combatant.cs:127 . State
            #                     ^^^ kendi capasi, muaf     ^^^ CAPRAZ ATIF, yasak
            #
            # Ve o atif curumustu: Combatant.cs:127 bir yorum satirinin ortasiydi.
            # Muafiyet artik SATIRA degil, capanin KENDISINE ait.
            for match in REF.finditer(line):
                if match.group(1) == path.name:
                    # Kendi dosyasina atif: capa sahibi onu tazeliyor.
                    continue
                hits.append((path, number, match.group(0), line.strip()))

    for path, number, ref, text in hits:
        print(f"{path}:{number}\n    CAPRAZ ATIF: {ref}\n    {text[:96]}")

    print(f"\ncapraz dosya satir atfi: {len(hits)}  (olmasi gereken: 0)")
    return 1 if hits else 0


if __name__ == "__main__":
    sys.exit(main())

# Kural 38 bloklarindaki satir referanslarini tazeler (LANE_PROTOCOL §16).
#
# NEDEN VAR: her REDDEDILEN blogu, reddedilen kodun DURACAGI satiri gosterir.
# Ustte tek bir satir eklendiginde bu numaralarin hepsi kayar, ve bayat bir
# satir numarasi hic numara vermemekten DAHA kotudur — ogreneni tam bir
# guvenle YANLIS mekanizmaya gonderir.
#
# OLCULMUS SINIR — bu scriptin butun sekli bundan dogdu: capa MEKANIK DEGIL.
# 98 blogun cogu blogun hemen altindaki uyeyi gosteriyor, ama bir kismi
# gostermiyor:
#   Battle.cs:38     -> alttaki alani degil, 30 satir asagidaki KURUCUYU
#   BoardAdapter:528 -> metodu degil, icindeki tek bir DEYIMI
#   BoardAdapter:117 -> alani degil, ustundeki [Header]/[Tooltip] atlanarak alani
#   TurnRulesTests   -> metodu degil, ustundeki [TestCase] listesini
# Yani "blogun altindaki ilk kod satiri" kurali bu dosyalarin dordunu de
# sessizce YANLIS yere tasirdi. Bu yuzden varsayilan yol ICERIK esleme:
#
#   --snapshot   her blogun su an gosterdigi satirin METNINI kaydeder
#   --reanchor   duzenlemeden sonra ayni metni yeni dosyada bulup numarayi tazeler
#
# Yeni bloklar ":000" yer tutucusuyla yazilir (worker sozlesmesi) ve onlarin
# snapshot karsiligi yoktur; onlar mekanik tahminle cozulur ve rapora
# "TAHMIN" olarak dusler — insan gozuyle dogrulanmak uzere.
#
# Kullanim:
#   python Tools/resolve-rejected-anchors.py --snapshot
#   python Tools/resolve-rejected-anchors.py --reanchor --check
#   python Tools/resolve-rejected-anchors.py --reanchor --write

import argparse
import json
import pathlib
import re
import sys

# ornek: "        // REDDEDILEN - Health.cs:42 yerine:"
# ornek: "        // REDDEDILEN - AttackOutcome.cs:59 yerine (enum tamamen kalkar):"
HEADER = re.compile(
    r"^(?P<pre>\s*//\s*REDDEDILEN\s*-\s*)(?P<file>[A-Za-z0-9_.]+\.cs):(?P<line>\d+)(?P<post>\s.*)?$"
)

COMMENT_OR_BLANK = re.compile(r"^\s*(//.*)?$")

# Mekanik tahminde ATLANAN oznitelikler: bunlar uyeyi SUSLER, uyenin kendisi
# degildir. NUnit oznitelikleri ([Test], [TestCase], [TestCaseSource]) bilerek
# DISARIDA: bir test blogunda reddedilen sey cogu zaman TestCase listesinin
# kendisidir, ve capa oraya bakar.
DECORATION = re.compile(
    r"^\s*\[(Header|Tooltip|Space|SerializeField|RequireComponent|TextArea|HideInInspector|Min|Range)\b[^]]*\]\s*$"
)

STRUCTURAL = re.compile(r"^\s*[{}]\s*$")

SNAPSHOT_PATH = pathlib.Path("Tools/.rejected-anchors.json")


def is_skippable(line):
    return bool(COMMENT_OR_BLANK.match(line) or DECORATION.match(line) or STRUCTURAL.match(line))


def mechanical_anchor(lines, header_index):
    """Blogun altindaki ilk bildirim satiri — yalnizca TAHMIN, yetki degil.

    GERIYE DUSME: reddedilen secenek bazen var olan bir satirin yerine degil,
    HIC VAR OLMAYAN bir uyenin yerine gecer (MovementRules'taki takimli asiri
    yukleme boyle). O blok sinifin sonunda durur ve altinda kod satiri YOKTUR.
    Boyle bir blogun dogru capasi, yanina oturacagi uyedir — yani yukaridaki
    ilk kod satiri. Numarasiz birakmak degil, cunku §16 ogrenene tiklanabilir
    bir hedef borclu.
    """
    i = header_index + 1
    while i < len(lines) and is_skippable(lines[i]):
        i += 1
    if i < len(lines):
        return i + 1

    i = header_index - 1
    while i >= 0 and is_skippable(lines[i]):
        i -= 1
    return i + 1 if i >= 0 else None


def fingerprint(lines, header_index):
    """Blogun REDDEDILEN KODU — govdesi degil.

    OLCULMUS DUZELTME, 2026-08-20: parmak izi once blogun butun govdesiydi
    (KIRILAN + KAZANIRDI metni dahil). Yorum sozlesmesi geldigi gun her blogun
    govdesi yeniden yazildi ve snapshot TAM DA ISE YARAYACAGI ANDA gecersizlesti:
    144 blogun 144'u eslesmedi, icerik eslemesi hic calismadi, hepsi mekanik
    tahmine dustu.

    Dogru parmak izi, blogun EN KARARLI parcasi: baslikin hemen altindaki
    reddedilen kod satirlari ("//     <kod>"). Onlar bir kararin kendisidir ve
    yeniden yazilmazlar; gerekce metni yeniden yazilir.
    """
    body = []
    i = header_index + 1
    while i < len(lines):
        stripped = lines[i].strip()
        if not stripped.startswith("//"):
            break
        # Yalniz girintili kod satirlari: "//" + bes ya da daha fazla bosluk.
        # "KIRILAN"/"KAZANIRDI"/"TEK CUMLE" satirlari parmak izine GIRMEZ.
        if re.match(r"^\s*//\s{5,}\S", lines[i]):
            body.append(stripped.lstrip("/").strip())
        elif body:
            break
        i += 1
    return "\n".join(body)


def blocks_in(lines):
    for index, line in enumerate(lines):
        match = HEADER.match(line)
        if match:
            yield index, match


def line_text(lines, number):
    if number is None or number < 1 or number > len(lines):
        return "<yok>"
    return lines[number - 1].strip()


def read_lines(path):
    return path.read_text(encoding="utf-8").split("\n")


def do_snapshot(root):
    records = []
    for path in sorted(root.rglob("*.cs")):
        lines = read_lines(path)
        for index, match in blocks_in(lines):
            number = int(match.group("line"))
            records.append(
                {
                    "file": path.as_posix(),
                    "fingerprint": fingerprint(lines, index),
                    "target": number,
                    "targetText": line_text(lines, number),
                }
            )
    SNAPSHOT_PATH.parent.mkdir(parents=True, exist_ok=True)
    SNAPSHOT_PATH.write_text(json.dumps(records, ensure_ascii=False, indent=1), encoding="utf-8")
    print(f"snapshot: {len(records)} blok -> {SNAPSHOT_PATH}")
    return 0


def find_by_text(lines, text, near):
    """Metni tasiyan satir — ama yalnizca metin AYIRT EDICI ise.

    OLCULMUS SINIR, 2026-08-20: "break;" gibi bir satir dosyada onlarca kez
    gecer. Ona gore eslesen bir capa, blok basligina en yakin "break;"e atlar
    ve bu tesadufen dogru ya da tesadufen yanlis olur — ikisi de guvenilmez.
    Ayirt edici olmayan metin icin icerik eslemesi YAPILMAZ; blok "INCELE"ye
    duser ve insan gozune gider. Sessizce yanlis capalamaktansa raporlamak.
    """
    if not text or text == "<yok>":
        return None
    if len(text) < 16:
        return None
    hits = [i + 1 for i, line in enumerate(lines) if line.strip() == text]
    if len(hits) != 1:
        return None
    return hits[0]


def do_reanchor(root, write):
    snapshot = {}
    if SNAPSHOT_PATH.exists():
        for record in json.loads(SNAPSHOT_PATH.read_text(encoding="utf-8")):
            snapshot[(record["file"], record["fingerprint"])] = record

    stats = {"blok": 0, "tazelendi": 0, "tahmin": 0, "degismedi": 0, "incele": 0, "yabanci": 0}
    reports = []

    for path in sorted(root.rglob("*.cs")):
        lines = read_lines(path)
        dirty = False

        for index, match in blocks_in(lines):
            stats["blok"] += 1
            current = int(match.group("line"))

            if match.group("file") != path.name:
                stats["yabanci"] += 1
                reports.append(f"YABANCI  {path}:{index + 1} -> {match.group('file')} (elle bak)")
                continue

            record = snapshot.get((path.as_posix(), fingerprint(lines, index)))
            target = None
            kind = None

            if record is not None:
                target = find_by_text(lines, record["targetText"], index + 1)
                kind = "TAZELENDI"

            if target is None:
                target = mechanical_anchor(lines, index)
                # Snapshot'i olan ama capa metni kaybolan blok: kod degismis
                # demektir ve tahmin RAPORLANIR, sessizce uygulanmaz.
                kind = "TAHMIN" if current == 0 else "INCELE"

            if target is None:
                stats["incele"] += 1
                reports.append(f"INCELE   {path}:{index + 1} capa bulunamadi")
                continue

            if kind == "INCELE":
                stats["incele"] += 1
                reports.append(
                    f"INCELE   {path}:{index + 1}  {current} -> {target}?\n"
                    f"         tahmin  : {line_text(lines, target)}"
                )
                continue

            if current == target:
                stats["degismedi"] += 1
                continue

            stats["tazelendi" if kind == "TAZELENDI" else "tahmin"] += 1
            reports.append(
                f"{kind:8} {path}:{index + 1}  {current} -> {target}\n"
                f"         capa    : {line_text(lines, target)}"
            )
            post = match.group("post") or ""
            lines[index] = f"{match.group('pre')}{match.group('file')}:{target}{post}"
            dirty = True

        if dirty and write:
            path.write_text("\n".join(lines), encoding="utf-8")

    for report in reports:
        print(report)

    verb = "YAZILDI" if write else "kuru kosu"
    print("\n" + "  ".join(f"{k}: {v}" for k, v in stats.items()) + f"  ({verb})")

    if stats["incele"] or stats["yabanci"]:
        return 1
    return 0


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--snapshot", action="store_true")
    parser.add_argument("--reanchor", action="store_true")
    parser.add_argument("--write", action="store_true")
    parser.add_argument("--check", action="store_true")
    parser.add_argument("--root", default="Assets")
    args = parser.parse_args()

    root = pathlib.Path(args.root)

    if args.snapshot:
        return do_snapshot(root)
    if args.reanchor:
        return do_reanchor(root, write=args.write)
    parser.error("--snapshot ya da --reanchor gerekli")


if __name__ == "__main__":
    sys.exit(main())

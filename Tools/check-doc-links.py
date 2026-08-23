# Ayna belge kapisi: koddan belgeye ve belgeden belgeye giden her atif COZULMELI.
#
# NEDEN VAR: gerekcelerin cogunlugu artik Assets/ altinda degil Docs/deep/
# altinda. Diger dort check-*.py yalnizca Assets/**/*.cs tariyor, yani bu
# gecisin urettigi ~200 capa ve goreli yol HICBIR kapinin denetiminde degildi.
# Uc bagimsiz worker ayni boslugu bildirdi; biri de canli bir kirik capa buldu
# (AttackProfile.cs -> AttackProfile.md#menzil-en-az-1, belgede yok).
#
# Bir capa sessizce curudugunde kimse fark etmez: kod derlenir, testler yesil
# kalir, yalnizca okuyan kisi bos bir sayfaya duser. Bu yuzden makine kapisi.
#
# NE TARAR:
#   1. Assets/**/*.cs icindeki "// -> Tip.md#capa" isaretcileri
#   2. Assets/**/*.cs icindeki "Docs/deep/..." yollari
#   3. Docs/**/*.md icindeki goreli [metin](yol.md) ve [metin](yol.md#capa)
#
# Kullanim:
#   python Tools/check-doc-links.py

import pathlib
import re
import sys
import unicodedata

ROOT = pathlib.Path(".")
DOCS = ROOT / "Docs"
ASSETS = ROOT / "Assets"

# "// -> AttackProfile.md#damage"  ya da  "// → AttackProfile.md#damage"
POINTER = re.compile(r"//.*?(?:->|→)\s*([A-Za-z0-9_.-]+\.md)#([A-Za-z0-9_-]+)")
# yorumda gecen tam yol
DOCPATH = re.compile(r"(Docs/[A-Za-z0-9_./-]+\.md)")
# markdown baglantisi: [metin](yol) -- yalniz goreli .md hedefleri
MDLINK = re.compile(r"\[[^\]]*\]\((?!https?:)([^)#]*\.md)(?:#([^)]*))?\)")
HEADING = re.compile(r"^#{1,6}\s+(.*?)\s*$")


def slug(text):
    """GitHub baslik -> capa donusumu (bu repoda gecerli alt kume).

    Turkce buyuk I (U+0130) kucultuldugunde 'i' + birlesen nokta uretir ve
    capayi bozar; bu yuzden onu once sadelestiriyoruz. Worker'lar zaten
    ASCII baslik kullandi, bu satir yalnizca ileride biri unutursa var.
    """
    text = text.replace("İ", "I").replace("ı", "i")
    text = unicodedata.normalize("NFKD", text)
    text = "".join(c for c in text if not unicodedata.combining(c))
    text = text.lower()
    text = re.sub(r"[^\w\s-]", "", text)
    text = re.sub(r"[\s_]+", "-", text)
    return text.strip("-")


def headings_of(path):
    out = set()
    for line in path.read_text(encoding="utf-8").split("\n"):
        m = HEADING.match(line)
        if m:
            out.add(slug(m.group(1)))
    return out


def main():
    md_files = sorted(DOCS.rglob("*.md"))
    # ANAHTAR MUTLAK YOL OLMALI: asagida hedef (md.parent / rel).resolve() ile
    # uretiliyor. Goreli anahtarla kurulan bir sozluk orada ASLA eslesmez ve
    # kapi her capayi "YOK" sayar -- ilk surumunde tam olarak bu oldu, 50 sahte
    # pozitif uretti. Kapinin kendi kapisi: asagidaki oz-sinama.
    anchors = {p.resolve(): headings_of(p) for p in md_files}
    by_name = {}
    for p in md_files:
        by_name.setdefault(p.name, []).append(p)

    problems = []

    # 1 + 2: kodun belgeye atiflari
    for cs in sorted(ASSETS.rglob("*.cs")):
        for number, line in enumerate(cs.read_text(encoding="utf-8").split("\n"), 1):
            for name, anchor in POINTER.findall(line):
                targets = by_name.get(name, [])
                if not targets:
                    problems.append((cs, number, "belge YOK: %s" % name))
                elif len(targets) > 1:
                    problems.append((cs, number, "belge adi COKLU: %s" % name))
                elif anchor not in anchors[targets[0].resolve()]:
                    problems.append((cs, number, "capa YOK: %s#%s" % (name, anchor)))
            for rel in DOCPATH.findall(line):
                if not (ROOT / rel).exists():
                    problems.append((cs, number, "yol YOK: %s" % rel))

    # 3: belgeden belgeye
    for md in md_files:
        for number, line in enumerate(md.read_text(encoding="utf-8").split("\n"), 1):
            for rel, anchor in MDLINK.findall(line):
                target = (md.parent / rel).resolve()
                if not target.exists():
                    problems.append((md, number, "hedef YOK: %s" % rel))
                elif anchor and slug(anchor) not in anchors.get(target, set()):
                    problems.append((md, number, "capa YOK: %s#%s" % (rel, anchor)))

    # OZ-SINAMA: her belge kendi ilk basligina cozulebilmeli. Cozulemiyorsa
    # sorun belgede degil bu betikte demektir (yol normalizasyonu, slug kurali).
    for p in md_files:
        hs = anchors[p.resolve()]
        if hs and next(iter(hs)) not in anchors.get(p.resolve(), set()):
            print("KAPI BOZUK: kendi basligini cozemiyor -> %s" % p)
            return 2

    for path, number, text in problems:
        print("%s:%d\n    %s" % (path, number, text))

    print("\nbelge: %d  cozulmeyen atif: %d" % (len(md_files), len(problems)))
    return 1 if problems else 0


if __name__ == "__main__":
    sys.exit(main())

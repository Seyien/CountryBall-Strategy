namespace GridStrategy.Core
{
    // ═══ ROL: VARLIK (Entity) ════════════════════════════════════════
    // kimlik : var — aynı isimli iki Unit ayrı birimdir; BoardAdapter'ın
    //          sözlüğü tam olarak bu referans kimliğini anahtar yapıyor
    // hafıza : var (bugün ince) — tuttuğu tek durum Name; Health ve
    //          UnitLifecycle buraya bağlandığında durum büyüyecek
    // Unity  : gerekmez — sahne, prefab, koordinat bilmez
    // karar  : vermez — kim olduğunu bilir, ne yapacağını bilmez
    public sealed class Unit
    {
        public Unit(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}

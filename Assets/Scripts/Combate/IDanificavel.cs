public interface IDanificavel
{
    bool estaMorto { get; }
    void receberDano(InfoDano dano);
}

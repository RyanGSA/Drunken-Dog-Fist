using UnityEngine;

// toca a musica da cena em loop. quando um chefe/mini-boss entra, troca pra musica de chefe.
[RequireComponent(typeof(AudioSource))]
public class GerenteMusica : MonoBehaviour
{
    public static GerenteMusica instancia;

    [SerializeField] private AudioClip musicaAmbiente;
    [SerializeField] private AudioClip musicaChefe;

    private AudioSource fonte;

    void Awake()
    {
        instancia = this;
        fonte = GetComponent<AudioSource>();
    }

    void Start() => tocar(musicaAmbiente);

    // chamado pelo InimigoBase quando um chefe/mini-boss aparece
    public static void tocarChefe()
    {
        if (instancia != null) instancia.tocar(instancia.musicaChefe);
    }

    void tocar(AudioClip clipe)
    {
        if (clipe == null || fonte.clip == clipe) return;
        fonte.clip = clipe;
        fonte.loop = true;
        fonte.Play();
    }
}

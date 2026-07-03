using UnityEngine;
using UnityEngine.UI;

// barra de vida do hud. liga numa Vida e atualiza o preenchimento quando leva dano.
public class BarraDeVida : MonoBehaviour
{
    [SerializeField] private Vida vida;
    [SerializeField] private Image preenchimento; // Image com type = Filled (horizontal)

    void OnEnable()
    {
        if (vida != null) vida.aoReceberDano += quandoLevaDano;
    }

    void OnDisable()
    {
        if (vida != null) vida.aoReceberDano -= quandoLevaDano;
    }

    void Start() => atualizar();

    void quandoLevaDano(InfoDano dano) => atualizar();

    void atualizar()
    {
        if (vida == null || preenchimento == null) return;
        preenchimento.fillAmount = vida.vidaAtual / vida.vidaTotal;
    }
}

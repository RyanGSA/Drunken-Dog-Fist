using UnityEngine;
using UnityEngine.SceneManagement;

// botoes da cena de menu
public class MenuPrincipal : MonoBehaviour
{
    [SerializeField] private string cenaDoJogo = "Fase1";
    [SerializeField] private GameObject painelCreditos;

    // esconde os creditos no inicio (pode deixar ligado no editor pra ajustar)
    void Awake()
    {
        if (painelCreditos != null) painelCreditos.SetActive(false);
    }

    public void jogar() => SceneManager.LoadScene(cenaDoJogo);

    public void sair() => Application.Quit();

    // usado pelo botao "Menu" da tela de vitoria
    public void irParaMenu() => SceneManager.LoadScene("MenuPrincipal");

    public void abrirCreditos() => painelCreditos.SetActive(true);

    public void fecharCreditos() => painelCreditos.SetActive(false);
}

using UnityEngine;
using UnityEngine.SceneManagement;

// botoes da cena de menu
public class MenuPrincipal : MonoBehaviour
{
    [SerializeField] private string cenaDoJogo = "Fase1";

    public void jogar() => SceneManager.LoadScene(cenaDoJogo);

    public void sair() => Application.Quit();

    // usado pelo botao "Menu" da tela de vitoria
    public void irParaMenu() => SceneManager.LoadScene("MenuPrincipal");
}

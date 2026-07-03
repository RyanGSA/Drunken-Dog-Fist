using UnityEngine;
using UnityEngine.SceneManagement;

// botoes da cena de menu
public class MenuPrincipal : MonoBehaviour
{
    [SerializeField] private string cenaDoJogo = "Cena1";

    public void jogar() => SceneManager.LoadScene(cenaDoJogo);

    public void sair() => Application.Quit();
}

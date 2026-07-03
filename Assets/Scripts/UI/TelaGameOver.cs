using UnityEngine;
using UnityEngine.SceneManagement;

public class TelaGameOver : MonoBehaviour
{
    [SerializeField] private GameObject painel;

    // esconde no inicio (pode deixar ligado no editor pra ajustar; some sozinho no Play)
    void Awake() => painel.SetActive(false);

    public void mostrar() => painel.SetActive(true);

    public void tentarDeNovo()
    {
        Time.timeScale = 1f; // caso tenha morrido com o jogo pausado
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void voltarMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuPrincipal");
    }
}

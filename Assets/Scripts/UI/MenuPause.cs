using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPause : MonoBehaviour
{
    [SerializeField] private GameObject painel;
    [SerializeField] private GameObject painelGameOver; // esc nao pausa em cima do game over

    private bool pausado;

    // esconde no inicio (pode deixar ligado no editor pra ajustar; some sozinho no Play)
    void Awake() => painel.SetActive(false);

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;
        if (painelGameOver != null && painelGameOver.activeSelf) return;

        if (pausado) continuar();
        else pausar();
    }

    void pausar()
    {
        pausado = true;
        Time.timeScale = 0f;
        painel.SetActive(true);
    }

    public void continuar()
    {
        pausado = false;
        Time.timeScale = 1f;
        painel.SetActive(false);
    }

    public void voltarMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuPrincipal");
    }
}

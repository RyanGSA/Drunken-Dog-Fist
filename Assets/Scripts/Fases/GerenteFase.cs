using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// controla o fim da fase: quando o OndaSpawner limpa a ultima onda (o chefe morre),
// o aoLimparSala chama avancar() e a gente carrega a proxima cena.
public class GerenteFase : MonoBehaviour
{
    [SerializeField] private string proximaCena = "Fase2";
    [SerializeField] private float atrasoAntesDeAvancar = 2f;
    [SerializeField] private GameObject painelCompleta; // opcional: um "Fase Completa!" pra aparecer antes de trocar

    // ligar isso no aoLimparSala do OndaSpawner
    public void avancar() => StartCoroutine(rotina());

    IEnumerator rotina()
    {
        if (painelCompleta != null) painelCompleta.SetActive(true);
        yield return new WaitForSeconds(atrasoAntesDeAvancar);
        SceneManager.LoadScene(proximaCena);
    }
}

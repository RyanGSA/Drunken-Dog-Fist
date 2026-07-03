using UnityEngine;
using UnityEngine.SceneManagement;

public class MudarCena : MonoBehaviour
{
    public string nomeDaCena;
    
    void Start()
    {
        Invoke("TrocarCena", 5f);
    }

    void TrocarCena()
    {
        SceneManager.LoadScene(nomeDaCena);
    }
}
using UnityEngine;

public class ProtagScript : MonoBehaviour
{
    public float velocidade = 5f;

    void Start()
    {

    }

    void Update()
    {

        if (Input.GetKey(KeyCode.W))
        {
            GetComponent<Animator>().SetBool("andando", true);
            transform.Translate(Vector2.up * velocidade * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.S))
        {
            GetComponent<Animator>().SetBool("andando", true);
            transform.Translate(Vector2.down * velocidade * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.A))
        {
            GetComponent<Animator>().SetBool("andando", true);
            transform.Translate(Vector2.left * velocidade * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.D))
        {   
            GetComponent<Animator>().SetBool("andando", true);
            transform.Translate(Vector2.right * velocidade * Time.deltaTime);
        }
            if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D))
            {
                GetComponent<Animator>().SetBool("andando", false);
            }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            GetComponent<Animator>().SetBool("socando", true);
        }
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            GetComponent<Animator>().SetBool("socando", false);
        }


        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            GetComponent<Animator>().SetBool("chutando", true);
        }
        if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            GetComponent<Animator>().SetBool("chutando", false);
        }
    }
}

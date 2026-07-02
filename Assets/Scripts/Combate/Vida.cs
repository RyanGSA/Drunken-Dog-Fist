using System;
using UnityEngine;

public class Vida : MonoBehaviour
{
    [SerializeField] private float vidaMaxima = 30f;

    public float vidaAtual { get; private set; }
    public bool estaMorto => vidaAtual <= 0f;

    public event Action<InfoDano> aoReceberDano;
    public event Action aoMorrer;

    void Awake()
    {
        vidaAtual = vidaMaxima;
    }

    public void aplicar(InfoDano dano)
    {
        if (estaMorto) return;

        vidaAtual = Mathf.Max(0f, vidaAtual - dano.valor);
        aoReceberDano?.Invoke(dano);

        if (vidaAtual <= 0f)
            aoMorrer?.Invoke();
    }
}

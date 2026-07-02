using UnityEngine;

public struct InfoDano
{
    public float valor;
    public Vector2 origem;
    public GameObject atacante;
    public float empurrao;

    public InfoDano(float valor, Vector2 origem, GameObject atacante, float empurrao = 0f)
    {
        this.valor = valor;
        this.origem = origem;
        this.atacante = atacante;
        this.empurrao = empurrao;
    }
}

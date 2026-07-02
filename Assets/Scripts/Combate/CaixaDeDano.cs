using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CaixaDeDano : MonoBehaviour
{
    [SerializeField] private LayerMask alvos;
    [SerializeField] private float dano = 5f;
    [SerializeField] private float empurrao = 3f;
    [SerializeField] private GameObject dono;

    private bool ligada;
    private HashSet<IDanificavel> jaAcertados = new HashSet<IDanificavel>();

    public float valorDano { get => dano; set => dano = value; }

    void Awake()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
        if (dono == null) dono = transform.root.gameObject;
    }

    public void ativar()
    {
        jaAcertados.Clear();
        ligada = true;
    }

    public void desativar()
    {
        ligada = false;
    }

    void OnTriggerStay2D(Collider2D outro)
    {
        if (!ligada) return;
        if ((alvos.value & (1 << outro.gameObject.layer)) == 0) return;
        if (dono != null && outro.transform.IsChildOf(dono.transform)) return;

        IDanificavel alvo = outro.GetComponentInParent<IDanificavel>();
        if (alvo == null || alvo.estaMorto) return;
        if (!jaAcertados.Add(alvo)) return;

        alvo.receberDano(new InfoDano(dano, transform.position, dono, empurrao));
    }
}

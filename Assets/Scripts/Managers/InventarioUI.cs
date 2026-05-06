using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ============================================================
// InventarioUI.cs — fix aplicado
// ============================================================
// FIX: En Awake() el script se registra en PlayerMovement
// ANTES de desactivarse. Awake() se ejecuta aunque el objeto
// esté marcado como desactivado en el Inspector — esa es la
// clave que resuelve el problema del Tab.
// ============================================================
public class InventarioUI : MonoBehaviour
{
    [Header("Contenedor de la grilla")]
    public Transform contenedorGrilla;

    [Header("Panel derecho")]
    public Image    imagenIcono;
    public TMP_Text textNombre;
    public TMP_Text textDesc;
    public TMP_Text textEfecto;
    public Button   botonUsar;

    [Header("Prefab de celda")]
    public GameObject prefabCelda;

    [Header("Colores de las celdas")]
    public Color colorNormal    = new Color(0.08f, 0.12f, 0.18f, 1f);
    public Color colorSeleccion = new Color(0.05f, 0.25f, 0.15f, 1f);

    private bool inventarioAbierto  = false;
    private int  indiceSeleccionado = -1;
    private List<GameObject> celdasActivas = new List<GameObject>();
    private Item itemSeleccionado = null;

    // ----------------------------------------------------------
    // Awake — FIX PRINCIPAL
    // Awake() se ejecuta UNA VEZ al cargar la escena,
    // incluso si el objeto está desactivado en el Inspector.
    // Aprovechamos ese momento para registrarnos en
    // PlayerMovement antes de desactivarnos.
    // ----------------------------------------------------------
    void Awake()
    {
        if (botonUsar != null)
            botonUsar.onClick.AddListener(UsarItemSeleccionado);

        // Nos registramos en PlayerMovement mientras estamos activos
        PlayerMovement pm = FindAnyObjectByType<PlayerMovement>();
        if (pm != null)
        {
            pm.inventarioUI = this;
            Debug.Log("[INVENTARIO] Registrado en PlayerMovement");
        }
        else
        {
            Debug.LogWarning("[INVENTARIO] No se encontró PlayerMovement");
        }

        // Ahora sí nos desactivamos
        gameObject.SetActive(false);
    }

    // Update solo detecta E — el Tab lo maneja PlayerMovement
    void Update()
    {
        if (inventarioAbierto && Input.GetKeyDown(KeyCode.E))
            UsarItemSeleccionado();
    }

    public void AbrirInventario()
    {
        inventarioAbierto = true;
        gameObject.SetActive(true);
        Time.timeScale = 0f;
        ConstruirGrilla();
        LimpiarPanelInfo();

        Debug.Log("[INVENTARIO] Abierto con " + Inventario.Instance?.ObtenerItems().Count + " items");
    }

    public void CerrarInventario()
    {
        inventarioAbierto = false;
        gameObject.SetActive(false);
        Time.timeScale = 1f;
        LimpiarCeldas();
        indiceSeleccionado = -1;
        itemSeleccionado   = null;

        Debug.Log("[INVENTARIO] Cerrado");
    }

    void ConstruirGrilla()
    {
        LimpiarCeldas();
        if (Inventario.Instance == null) return;

        LinkedListNode<Item> nodo = Inventario.Instance.ObtenerItems().First;
        int indice = 0;

        while (nodo != null)
        {
            CrearCelda(nodo.Value, indice);
            nodo = nodo.Next;
            indice++;
        }

        for (int i = indice; i < 10; i++)
            CrearCeldaVacia();
    }

    void CrearCelda(Item item, int indice)
    {
        if (prefabCelda == null) return;

        GameObject celda = Instantiate(prefabCelda, contenedorGrilla);
        celdasActivas.Add(celda);

        Image fondo = celda.GetComponent<Image>();
        if (fondo != null) fondo.color = colorNormal;

        Image icono = celda.transform.Find("IconoItem")?.GetComponent<Image>();
        if (icono != null)
        {
            icono.enabled = true;
            if (item.icono != null) icono.sprite = item.icono;
            else icono.color = ObtenerColorTipo(item.tipo);
        }

        TMP_Text texto = celda.transform.Find("TextoNombre")?.GetComponent<TMP_Text>();
        if (texto != null) texto.text = item.nombre;

        int  indiceLocal = indice;
        Item itemLocal   = item;

        Button boton = celda.GetComponent<Button>();
        if (boton != null)
            boton.onClick.AddListener(() => SeleccionarCelda(indiceLocal, itemLocal));
    }

    void CrearCeldaVacia()
    {
        if (prefabCelda == null) return;

        GameObject celda = Instantiate(prefabCelda, contenedorGrilla);
        celdasActivas.Add(celda);

        Image fondo = celda.GetComponent<Image>();
        if (fondo != null) fondo.color = new Color(0.06f, 0.09f, 0.14f, 0.5f);

        Image icono = celda.transform.Find("IconoItem")?.GetComponent<Image>();
        if (icono != null) icono.enabled = false;

        TMP_Text texto = celda.transform.Find("TextoNombre")?.GetComponent<TMP_Text>();
        if (texto != null) texto.text = "";

        Button boton = celda.GetComponent<Button>();
        if (boton != null) boton.interactable = false;
    }

    void SeleccionarCelda(int indice, Item item)
    {
        indiceSeleccionado = indice;
        itemSeleccionado   = item;

        for (int i = 0; i < celdasActivas.Count; i++)
        {
            Image fondo = celdasActivas[i].GetComponent<Image>();
            if (fondo == null) continue;
            fondo.color = (i == indice) ? colorSeleccion : colorNormal;
        }

        MostrarInfoItem(item);
    }

    void MostrarInfoItem(Item item)
    {
        if (textNombre  != null) textNombre.text      = item.nombre;
        if (textDesc    != null) textDesc.text         = item.descripcion;
        if (textEfecto  != null) textEfecto.text       = ObtenerTextoEfecto(item);
        if (botonUsar   != null) botonUsar.interactable = true;

        if (imagenIcono != null)
        {
            imagenIcono.enabled = true;
            if (item.icono != null) imagenIcono.sprite = item.icono;
            else imagenIcono.color = ObtenerColorTipo(item.tipo);
        }
    }

    void LimpiarPanelInfo()
    {
        if (textNombre  != null) textNombre.text        = "Selecciona un ítem";
        if (textDesc    != null) textDesc.text           = "";
        if (textEfecto  != null) textEfecto.text         = "";
        if (imagenIcono != null) imagenIcono.enabled     = false;
        if (botonUsar   != null) botonUsar.interactable  = false;
    }

    void UsarItemSeleccionado()
    {
        if (itemSeleccionado == null) return;

        bool usado = Inventario.Instance.Usar(itemSeleccionado.nombre);
        if (usado)
        {
            Debug.Log("[INVENTARIO] Usado: " + itemSeleccionado.nombre);
            itemSeleccionado   = null;
            indiceSeleccionado = -1;
            ConstruirGrilla();
            LimpiarPanelInfo();
        }
    }

    void LimpiarCeldas()
    {
        foreach (GameObject celda in celdasActivas)
            if (celda != null) Destroy(celda);
        celdasActivas.Clear();
    }

    string ObtenerTextoEfecto(Item item)
    {
        switch (item.tipo)
        {
            case Item.TipoItem.Curacion:  return "Restaura " + item.valor + " HP al instante";
            case Item.TipoItem.Defensa:   return "Reduce daño 50% por " + item.valor + " segundos";
            case Item.TipoItem.Velocidad: return "Velocidad x2 por " + item.valor + " segundos";
            case Item.TipoItem.Puntos:    return "+" + item.valor + " puntos al puntaje total";
            default:                      return "";
        }
    }

    Color ObtenerColorTipo(Item.TipoItem tipo)
    {
        switch (tipo)
        {
            case Item.TipoItem.Curacion:  return new Color(0.9f, 0.2f, 0.2f, 1f);
            case Item.TipoItem.Defensa:   return new Color(0.8f, 0.6f, 0.1f, 1f);
            case Item.TipoItem.Velocidad: return new Color(0.2f, 0.4f, 0.9f, 1f);
            case Item.TipoItem.Puntos:    return new Color(0.2f, 0.8f, 0.3f, 1f);
            default:                      return Color.white;
        }
    }
}
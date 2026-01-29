using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    #region Campos
    #region Referencias
    private GameManager gameManager;
    #endregion
    
    #region Paneles
    private GameObject panelMenu;
    private GameObject panelJuego;
    private GameObject panelDescanso;
    private GameObject panelTienda;
    private GameObject panelPostPelea;
    private GameObject panelMapa;
    private GameObject panelCiudad;
    private GameObject panelMorada;
    private GameObject panelTiendaCiudad;
    #endregion
    
    #region UI - Menu
    private Button btnJugar;
    private Button btnSalir;
    #endregion
    
    #region UI - Juego
    private Text lblVida1;
    private Slider pbVida1;
    private Text lblEscudo1;
    private Slider pbEscudo1;
    private Text lblVida2;
    private Slider pbVida2;
    private Text lblEscudo2;
    private Slider pbEscudo2;
    [SerializeField] private Image imgJugador;
    [SerializeField] private Image imgCaballero;
    private Image imgEnemigo;
    private Image animacionAtaque;
    private Coroutine corrutinaAnimacion;
    private Coroutine corrutinaIdleJugador;
    private Coroutine corrutinaIdleEnemigo;
    private Coroutine corrutinaIdleCaballero;
    private Button btnAtacar;
    private Button btnHuir;
    private Button btnAutomatico;
    private Button btnManual;
    private Button btnDefensa;
    private Text lblContadorAtaque;
    private Text lblInfoJugador;
    private Text lblInfoEnemigo;
    private Text lblClemencias;
    private Text lblMuertes;
    private GameObject panelStatsJugador;
    private GameObject panelStatsEnemigo;
    private Text lblNombreEnemigo;
    private Dictionary<string, GameObject> tarjetasJugador = new Dictionary<string, GameObject>();
    private GameObject tarjetaDañoEnemigo;
    
    [Header("Ajustes de Posición HUD")]
    [SerializeField] private RectTransform panelStatsJugadorRectSerialized;
    [SerializeField] private RectTransform panelHudEnemigo;
    [SerializeField] private Canvas hudCanvas;
    
    
    [Header("Prefabs UI Batalla")]
    [SerializeField] private GameObject tarjetaVidaPrefab;
    [SerializeField] private GameObject tarjetaEscudoPrefab;
    [SerializeField] private GameObject tarjetaOroPrefab;
    [SerializeField] private GameObject tarjetaDañoPrefab;
    [SerializeField] private GameObject tarjetaBatallaPrefab;
    [SerializeField] private GameObject tarjetaDañoEnemigoPrefab;
    
    [Header("Animación Daño UI")]
    [SerializeField] private float duracionAnimacionDaño = 2f;
    [SerializeField] private float desplazamientoDaño = 40f;
    [SerializeField] private Color colorDañoVida = new Color(1f, 0.3f, 0.3f, 1f);
    [SerializeField] private Color colorDañoEscudo = new Color(0.4f, 0.8f, 1f, 1f);
    
    [Header("Iconos Botones Batalla")]
    [SerializeField] private Sprite iconoAtaque;
    [SerializeField] private Sprite iconoParar;
    [SerializeField] private Sprite iconoDefensa;
    [SerializeField] private Sprite iconoEscudoRoto;
    [SerializeField] private Vector2 tamañoBotonAtaque = new Vector2(120f, 120f);
    [SerializeField] private Vector2 tamañoBotonDefensa = new Vector2(120f, 120f);
    [SerializeField] private Vector2 offsetBotonAtaque = new Vector2(-120f, 0f);
    [SerializeField] private Vector2 offsetBotonDefensa = new Vector2(120f, 0f);
    
    [SerializeField] private float guerreroRunFrameTime = 0.1f;
    [SerializeField] private float guerreroAttackFrameTime = 0.11f;
    private bool guerreroAtaqueEnCurso = false;
    
    [Header("Caballero SpriteSheet (Idle + Ataque)")]
    [SerializeField] private Sprite caballeroSheet;
    [SerializeField] private float caballeroIdleFrameTime = 0.12f;
    [SerializeField] private float caballeroAttackFrameTime = 0.1f;
    [SerializeField] private int caballeroIdleFrameCount = 18;
    [SerializeField] private int caballeroAttackFrameCount = 7;
    [SerializeField] private int[] caballeroIdleFrames;
    [SerializeField] private int[] caballeroAttackFrames;
    [SerializeField] private Sprite[] caballeroFrames;
    // caballeroFrames contendrá los 32 sprites del caballero: idle (0..15) y ataque (16..31)
    private Vector2 caballeroMaxFrameSize;
    private bool caballeroMaxFrameSizeReady = false;
    private Sprite[] caballeroFramesOrdered;
    private bool caballeroFramesOrderedReady = false;
    private float? caballeroPosicionYReferencia = null; // Posición Y de referencia fija para todos los sprites
    private Coroutine corrutinaSuavizarY = null; // Coroutine para suavizar cambios de posición Y
    
    private RectTransform panelStatsJugadorRect;
    private bool _offsetAplicado = false;
    private Coroutine animacionDañoJugador;
    private Coroutine animacionDañoEnemigo;
    #endregion
    
    #region UI - Descanso
    #endregion
    
    #region UI - Tienda
    #endregion
    
    #region Canvas
    private Canvas canvas;
    #endregion
    #endregion
    
    #region Inicializacion
    
    public void Initialize(GameManager gm)
    {
        gameManager = gm;
        
        if (canvas == null)
        {
            canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1536f, 672f);
                scaler.matchWidthOrHeight = 0.5f;
                
                canvasObj.AddComponent<GraphicRaycaster>();
                
                Image canvasBg = canvasObj.GetComponent<Image>();
                if (canvasBg != null)
                {
                    Destroy(canvasBg);
                }
                
                if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() == null)
                {
                    GameObject eventSystem = new GameObject("EventSystem");
                    eventSystem.AddComponent<EventSystem>();
                    
                    #if ENABLE_INPUT_SYSTEM
                    if (eventSystem.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>() == null)
                    {
                        eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
                    }
                    #else
                    if (eventSystem.GetComponent<StandaloneInputModule>() == null)
                    {
                        eventSystem.AddComponent<StandaloneInputModule>();
                    }
                    #endif
                }
            }
        }
        
        CrearPaneles();
        MostrarPanelMenu();
        StartCoroutine(IniciarIdleCaballeroDespuesDeInit());
    }
    #endregion
    
    #region Paneles - Creacion
    private void CrearPaneles()
    {
        panelMenu = CrearPanel("PanelMenu", new Color(0.1f, 0.1f, 0.15f, 1f));
        CrearPanelMenu();
        
        panelJuego = CrearPanel("PanelJuego", new Color(0.15f, 0.15f, 0.2f, 1f));
        CrearPanelJuego();
        
        panelPostPelea = CrearPanel("PanelPostPelea", new Color(0.1f, 0.1f, 0.15f, 1f));
        CrearPanelPostPelea();
        
        panelMapa = CrearPanel("PanelMapa", new Color(0.12f, 0.12f, 0.2f, 1f));
        CrearPanelMapa();
        
        panelCiudad = CrearPanel("PanelCiudad", new Color(0.12f, 0.12f, 0.2f, 1f));
        CrearPanelCiudad();
        
        panelMorada = CrearPanel("PanelMorada", new Color(0.12f, 0.12f, 0.2f, 1f));
        CrearPanelMorada();
        
        panelTiendaCiudad = CrearPanel("PanelTiendaCiudad", new Color(0.12f, 0.12f, 0.2f, 1f));
        CrearPanelTiendaCiudad();
    }
    
    private GameObject CrearPanel(string nombre, Color color)
    {
        GameObject panel = new GameObject(nombre);
        panel.transform.SetParent(canvas.transform, false);
        
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        
        Image img = panel.AddComponent<Image>();
        img.color = color;
        img.sprite = null;
        
        return panel;
    }
    
    private void CrearPanelMenu()
    {
        Text titulo = CrearTexto("Titulo", "⚔️ CAZADOR VS ⚔️", 48, new Color(1f, 0.8f, 0.2f, 1f));
        titulo.rectTransform.anchorMin = new Vector2(0.5f, 0.7f);
        titulo.rectTransform.anchorMax = new Vector2(0.5f, 0.7f);
        titulo.rectTransform.anchoredPosition = Vector2.zero;
        titulo.rectTransform.sizeDelta = new Vector2(400, 60);
        titulo.transform.SetParent(panelMenu.transform, false);
        
        btnJugar = CrearBotonConColor("BtnJugar", "🎮 JUGAR", 24, new Color(0.2f, 0.7f, 0.3f, 1f));
        btnJugar.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
        btnJugar.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        btnJugar.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 50);
        btnJugar.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 50);
        btnJugar.transform.SetParent(panelMenu.transform, false);
        btnJugar.onClick.AddListener(() => gameManager.BtnJugar_Click());
        
        btnSalir = CrearBotonConColor("BtnSalir", "❌ SALIR", 24, new Color(0.7f, 0.2f, 0.2f, 1f));
        btnSalir.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
        btnSalir.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        btnSalir.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -20);
        btnSalir.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 50);
        btnSalir.transform.SetParent(panelMenu.transform, false);
        btnSalir.onClick.AddListener(() => Application.Quit());
    }
    
    private void CrearPanelJuego()
    {
        Text lblJugador = CrearTexto("LblJugador", "👤 JUGADOR", 18, new Color(0.4f, 0.7f, 1f, 1f));
        lblJugador.rectTransform.anchorMin = new Vector2(0.1f, 0.85f);
        lblJugador.rectTransform.anchorMax = new Vector2(0.1f, 0.85f);
        lblJugador.rectTransform.anchoredPosition = Vector2.zero;
        lblJugador.rectTransform.sizeDelta = new Vector2(200, 30);
        lblJugador.transform.SetParent(panelJuego.transform, false);
        
        lblVida1 = CrearTexto("LblVida1", "❤️ Vida: 100/100", 14, Color.white);
        lblVida1.rectTransform.anchorMin = new Vector2(0.1f, 0.8f);
        lblVida1.rectTransform.anchorMax = new Vector2(0.1f, 0.8f);
        lblVida1.rectTransform.anchoredPosition = Vector2.zero;
        lblVida1.rectTransform.sizeDelta = new Vector2(200, 25);
        lblVida1.transform.SetParent(panelJuego.transform, false);
        
        pbVida1 = CrearSlider("PbVida1", Color.green);
        RectTransform pbVida1Rect = pbVida1.GetComponent<RectTransform>();
        pbVida1Rect.anchorMin = new Vector2(0.1f, 0.75f);
        pbVida1Rect.anchorMax = new Vector2(0.1f, 0.75f);
        pbVida1Rect.anchoredPosition = Vector2.zero;
        pbVida1Rect.sizeDelta = new Vector2(200, 20);
        pbVida1.transform.SetParent(panelJuego.transform, false);
        
        lblEscudo1 = CrearTexto("LblEscudo1", "🛡️ Escudo: 100/100", 14, Color.white);
        lblEscudo1.rectTransform.anchorMin = new Vector2(0.1f, 0.7f);
        lblEscudo1.rectTransform.anchorMax = new Vector2(0.1f, 0.7f);
        lblEscudo1.rectTransform.anchoredPosition = Vector2.zero;
        lblEscudo1.rectTransform.sizeDelta = new Vector2(200, 25);
        lblEscudo1.transform.SetParent(panelJuego.transform, false);
        
        pbEscudo1 = CrearSlider("PbEscudo1", Color.blue);
        RectTransform pbEscudo1Rect = pbEscudo1.GetComponent<RectTransform>();
        pbEscudo1Rect.anchorMin = new Vector2(0.1f, 0.65f);
        pbEscudo1Rect.anchorMax = new Vector2(0.1f, 0.65f);
        pbEscudo1Rect.anchoredPosition = Vector2.zero;
        pbEscudo1Rect.sizeDelta = new Vector2(200, 20);
        pbEscudo1.transform.SetParent(panelJuego.transform, false);
        
        GameObject imgObj = new GameObject("ImgJugador");
        imgObj.transform.SetParent(panelJuego.transform, false);
        imgJugador = imgObj.AddComponent<Image>();
        imgJugador.rectTransform.anchorMin = new Vector2(0.1f, 0.3f);
        imgJugador.rectTransform.anchorMax = new Vector2(0.1f, 0.3f);
        imgJugador.rectTransform.anchoredPosition = Vector2.zero;
        imgJugador.rectTransform.sizeDelta = new Vector2(150, 150);
        imgJugador.color = Color.white;
        
        lblNombreEnemigo = CrearTexto("LblNombreEnemigo", "", 14, Color.white);
        lblNombreEnemigo.rectTransform.anchorMin = new Vector2(1f, 1f);
        lblNombreEnemigo.rectTransform.anchorMax = new Vector2(1f, 1f);
        lblNombreEnemigo.rectTransform.pivot = new Vector2(1f, 1f);
        lblNombreEnemigo.rectTransform.anchoredPosition = new Vector2(-10f, -10f);
        lblNombreEnemigo.rectTransform.sizeDelta = new Vector2(200f, 30f);
        lblNombreEnemigo.alignment = TextAnchor.MiddleRight;
        lblNombreEnemigo.horizontalOverflow = HorizontalWrapMode.Overflow;
        lblNombreEnemigo.transform.SetParent(panelJuego.transform, false);
        
        Text lblEnemigo = CrearTexto("LblEnemigo", "👹 ENEMIGO", 18, new Color(1f, 0.4f, 0.4f, 1f));
        lblEnemigo.rectTransform.anchorMin = new Vector2(0.9f, 0.85f);
        lblEnemigo.rectTransform.anchorMax = new Vector2(0.9f, 0.85f);
        lblEnemigo.rectTransform.anchoredPosition = Vector2.zero;
        lblEnemigo.rectTransform.sizeDelta = new Vector2(200, 30);
        lblEnemigo.transform.SetParent(panelJuego.transform, false);
        
        lblVida2 = CrearTexto("LblVida2", "❤️ Vida: 100/100", 14, Color.white);
        lblVida2.rectTransform.anchorMin = new Vector2(0.9f, 0.8f);
        lblVida2.rectTransform.anchorMax = new Vector2(0.9f, 0.8f);
        lblVida2.rectTransform.anchoredPosition = Vector2.zero;
        lblVida2.rectTransform.sizeDelta = new Vector2(200, 25);
        lblVida2.transform.SetParent(panelJuego.transform, false);
        
        pbVida2 = CrearSlider("PbVida2", Color.green);
        RectTransform pbVida2Rect = pbVida2.GetComponent<RectTransform>();
        pbVida2Rect.anchorMin = new Vector2(0.9f, 0.75f);
        pbVida2Rect.anchorMax = new Vector2(0.9f, 0.75f);
        pbVida2Rect.anchoredPosition = Vector2.zero;
        pbVida2Rect.sizeDelta = new Vector2(200, 20);
        pbVida2.transform.SetParent(panelJuego.transform, false);
        
        lblEscudo2 = CrearTexto("LblEscudo2", "🛡️ Escudo: 100/100", 14, Color.white);
        lblEscudo2.rectTransform.anchorMin = new Vector2(0.9f, 0.7f);
        lblEscudo2.rectTransform.anchorMax = new Vector2(0.9f, 0.7f);
        lblEscudo2.rectTransform.anchoredPosition = Vector2.zero;
        lblEscudo2.rectTransform.sizeDelta = new Vector2(200, 25);
        lblEscudo2.transform.SetParent(panelJuego.transform, false);
        
        pbEscudo2 = CrearSlider("PbEscudo2", Color.blue);
        RectTransform pbEscudo2Rect = pbEscudo2.GetComponent<RectTransform>();
        pbEscudo2Rect.anchorMin = new Vector2(0.9f, 0.65f);
        pbEscudo2Rect.anchorMax = new Vector2(0.9f, 0.65f);
        pbEscudo2Rect.anchoredPosition = Vector2.zero;
        pbEscudo2Rect.sizeDelta = new Vector2(200, 20);
        pbEscudo2.transform.SetParent(panelJuego.transform, false);
        
        GameObject imgObj2 = new GameObject("ImgEnemigo");
        imgObj2.transform.SetParent(panelJuego.transform, false);
        imgEnemigo = imgObj2.AddComponent<Image>();
        imgEnemigo.rectTransform.anchorMin = new Vector2(0.9f, 0.3f);
        imgEnemigo.rectTransform.anchorMax = new Vector2(0.9f, 0.3f);
        imgEnemigo.rectTransform.anchoredPosition = Vector2.zero;
        imgEnemigo.rectTransform.sizeDelta = new Vector2(150, 150);
        imgEnemigo.color = Color.white;
        
        GameObject animObj = new GameObject("AnimacionAtaque");
        animObj.transform.SetParent(panelJuego.transform, false);
        animacionAtaque = animObj.AddComponent<Image>();
        animacionAtaque.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        animacionAtaque.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        animacionAtaque.rectTransform.anchoredPosition = Vector2.zero;
        animacionAtaque.rectTransform.sizeDelta = new Vector2(300, 300);
        animacionAtaque.color = new Color(1f, 1f, 1f, 0f);
        animacionAtaque.gameObject.SetActive(false);
        
        lblContadorAtaque = CrearTexto("LblContadorAtaque", "", 36, Color.yellow);
        lblContadorAtaque.rectTransform.anchorMin = new Vector2(0.5f, 0.2f);
        lblContadorAtaque.rectTransform.anchorMax = new Vector2(0.5f, 0.2f);
        lblContadorAtaque.rectTransform.anchoredPosition = Vector2.zero;
        lblContadorAtaque.rectTransform.sizeDelta = new Vector2(100, 50);
        lblContadorAtaque.transform.SetParent(panelJuego.transform, false);
        lblContadorAtaque.gameObject.SetActive(false);
        
        btnAtacar = CrearBotonConIcono("BtnAtaque", iconoAtaque, "ATAQUE");
        RectTransform ataqueRect = btnAtacar.GetComponent<RectTransform>();
        ataqueRect.anchorMin = new Vector2(0.5f, 0.15f);
        ataqueRect.anchorMax = new Vector2(0.5f, 0.15f);
        ataqueRect.anchoredPosition = offsetBotonAtaque;
        ataqueRect.sizeDelta = tamañoBotonAtaque;
        btnAtacar.transform.SetParent(panelJuego.transform, false);
        btnAtacar.gameObject.SetActive(true);
        btnAtacar.onClick.AddListener(() => gameManager.BtnAtaque_Click());
        
        btnManual = null;
        btnAutomatico = null;
        
        btnDefensa = CrearBotonConIcono("BtnDefensa", iconoDefensa, "DEFENSA");
        RectTransform defensaRect = btnDefensa.GetComponent<RectTransform>();
        defensaRect.anchorMin = new Vector2(0.5f, 0.15f);
        defensaRect.anchorMax = new Vector2(0.5f, 0.15f);
        defensaRect.anchoredPosition = offsetBotonDefensa;
        defensaRect.sizeDelta = tamañoBotonDefensa;
        btnDefensa.transform.SetParent(panelJuego.transform, false);
        btnDefensa.gameObject.SetActive(true);
        btnDefensa.onClick.AddListener(() => gameManager.BtnDefensa_Click());
        
        btnHuir = CrearBotonConColor("BtnHuir", "🏃 HUIR (50%) - 💰10", 18, new Color(0.9f, 0.7f, 0.2f, 1f));
        RectTransform huirRect = btnHuir.GetComponent<RectTransform>();
        huirRect.anchorMin = new Vector2(0.5f, 0.05f);
        huirRect.anchorMax = new Vector2(0.5f, 0.05f);
        huirRect.anchoredPosition = new Vector2(0, 0);
        huirRect.sizeDelta = new Vector2(220, 40);
        btnHuir.transform.SetParent(panelJuego.transform, false);
        btnHuir.onClick.AddListener(() => gameManager.BtnHuir_Click());
        
        panelStatsJugador = new GameObject("HUD_Player");
        panelStatsJugador.transform.SetParent(panelJuego.transform, false);
        RectTransform panelJugadorRect = panelStatsJugador.AddComponent<RectTransform>();
        panelJugadorRect.anchorMin = new Vector2(0f, 0f);
        panelJugadorRect.anchorMax = new Vector2(0f, 0f);
        panelJugadorRect.pivot = new Vector2(0f, 0f);
        panelJugadorRect.anchoredPosition = Vector2.zero;
        panelJugadorRect.sizeDelta = new Vector2(800f, 100f);
        panelStatsJugadorRect = panelJugadorRect;
        
        if (panelStatsJugadorRectSerialized == null)
        {
            panelStatsJugadorRectSerialized = panelJugadorRect;
        }
        
        panelStatsEnemigo = new GameObject("HUD_Enemy");
        panelStatsEnemigo.transform.SetParent(panelJuego.transform, false);
        RectTransform panelEnemigoRect = panelStatsEnemigo.AddComponent<RectTransform>();
        panelEnemigoRect.anchorMin = new Vector2(1f, 0f);
        panelEnemigoRect.anchorMax = new Vector2(1f, 0f);
        panelEnemigoRect.pivot = new Vector2(1f, 0f);
        panelEnemigoRect.anchoredPosition = Vector2.zero;
        panelEnemigoRect.sizeDelta = new Vector2(200f, 100f);
        if (panelHudEnemigo == null)
        {
            panelHudEnemigo = panelEnemigoRect;
        }
        
        if (hudCanvas == null)
        {
            hudCanvas = canvas;
        }
        
        lblInfoJugador = CrearTexto("LblInfoJugador", "", 12, Color.white);
        lblInfoJugador.rectTransform.anchorMin = new Vector2(0f, 0f);
        lblInfoJugador.rectTransform.anchorMax = new Vector2(0.5f, 0.1f);
        lblInfoJugador.rectTransform.anchoredPosition = Vector2.zero;
        lblInfoJugador.rectTransform.sizeDelta = Vector2.zero;
        lblInfoJugador.gameObject.SetActive(false);
        lblInfoJugador.transform.SetParent(panelJuego.transform, false);
        
        lblInfoEnemigo = CrearTexto("LblInfoEnemigo", "", 12, Color.white);
        lblInfoEnemigo.rectTransform.anchorMin = new Vector2(0.5f, 0f);
        lblInfoEnemigo.rectTransform.anchorMax = new Vector2(1f, 0.1f);
        lblInfoEnemigo.rectTransform.anchoredPosition = Vector2.zero;
        lblInfoEnemigo.rectTransform.sizeDelta = Vector2.zero;
        lblInfoEnemigo.gameObject.SetActive(false);
        lblInfoEnemigo.transform.SetParent(panelJuego.transform, false);
        
        lblClemencias = CrearTexto("LblClemencias", "Clemencia: 0", 14, Color.white);
        lblClemencias.rectTransform.anchorMin = new Vector2(0f, 1f);
        lblClemencias.rectTransform.anchorMax = new Vector2(0f, 1f);
        lblClemencias.rectTransform.pivot = new Vector2(0f, 1f);
        lblClemencias.rectTransform.anchoredPosition = new Vector2(-5f, -10f);
        lblClemencias.rectTransform.sizeDelta = new Vector2(120f, 20f);
        lblClemencias.transform.SetParent(panelJuego.transform, false);
        
        lblMuertes = CrearTexto("LblMuertes", "Muertes: 0", 14, Color.white);
        lblMuertes.rectTransform.anchorMin = new Vector2(0f, 1f);
        lblMuertes.rectTransform.anchorMax = new Vector2(0f, 1f);
        lblMuertes.rectTransform.pivot = new Vector2(0f, 1f);
        lblMuertes.rectTransform.anchoredPosition = new Vector2(-5f, -28f);
        lblMuertes.rectTransform.sizeDelta = new Vector2(120f, 20f);
        lblMuertes.transform.SetParent(panelJuego.transform, false);
    }
    
    private void CrearPanelDescanso()
    {
        // Panel eliminado
    }
    
    private void CrearPanelTienda()
    {
        // Panel eliminado
    }
    
    private void CrearPanelPostPelea()
    {
        Text titulo = CrearTexto("TituloPostPelea", "MENU POST PELEA", 28, new Color(1f, 0.9f, 0.2f, 1f));
        titulo.rectTransform.anchorMin = new Vector2(0.5f, 0.7f);
        titulo.rectTransform.anchorMax = new Vector2(0.5f, 0.7f);
        titulo.rectTransform.anchoredPosition = Vector2.zero;
        titulo.rectTransform.sizeDelta = new Vector2(400, 60);
        titulo.transform.SetParent(panelPostPelea.transform, false);
        
        Text info = CrearTexto("TextoPostPelea", "Pendiente de implementar", 18, Color.white);
        info.rectTransform.anchorMin = new Vector2(0.5f, 0.55f);
        info.rectTransform.anchorMax = new Vector2(0.5f, 0.55f);
        info.rectTransform.anchoredPosition = Vector2.zero;
        info.rectTransform.sizeDelta = new Vector2(400, 40);
        info.transform.SetParent(panelPostPelea.transform, false);
        
        Button btnAvanzar = CrearBotonConColor("BtnPostPeleaAvanzar", "AVANZAR", 18, new Color(0.2f, 0.7f, 0.3f, 1f));
        btnAvanzar.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.45f);
        btnAvanzar.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.45f);
        btnAvanzar.GetComponent<RectTransform>().anchoredPosition = new Vector2(-80, 0);
        btnAvanzar.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 40);
        btnAvanzar.transform.SetParent(panelPostPelea.transform, false);
        btnAvanzar.onClick.AddListener(() => {
            MostrarMapa();
        });
        
        Button btnCiudad = CrearBotonConColor("BtnPostPeleaCiudad", "CIUDAD", 18, new Color(0.3f, 0.6f, 0.8f, 1f));
        btnCiudad.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.45f);
        btnCiudad.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.45f);
        btnCiudad.GetComponent<RectTransform>().anchoredPosition = new Vector2(80, 0);
        btnCiudad.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 40);
        btnCiudad.transform.SetParent(panelPostPelea.transform, false);
        btnCiudad.onClick.AddListener(() => {
            MostrarCiudad();
        });
    }
    
    private void CrearPanelMapa()
    {
        Text titulo = CrearTexto("TituloMapa", "MAPA", 28, new Color(1f, 0.9f, 0.2f, 1f));
        titulo.rectTransform.anchorMin = new Vector2(0.5f, 0.7f);
        titulo.rectTransform.anchorMax = new Vector2(0.5f, 0.7f);
        titulo.rectTransform.anchoredPosition = Vector2.zero;
        titulo.rectTransform.sizeDelta = new Vector2(300, 40);
        titulo.transform.SetParent(panelMapa.transform, false);
        
        Button btnCiudad = CrearBotonConColor("BtnMapaCiudad", "IR A CIUDAD", 18, new Color(0.3f, 0.6f, 0.8f, 1f));
        btnCiudad.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.45f);
        btnCiudad.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.45f);
        btnCiudad.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        btnCiudad.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 40);
        btnCiudad.transform.SetParent(panelMapa.transform, false);
        btnCiudad.onClick.AddListener(() => {
            MostrarCiudad();
        });
    }
    
    private void CrearPanelCiudad()
    {
        Text titulo = CrearTexto("TituloCiudad", "CIUDAD", 28, new Color(1f, 0.9f, 0.2f, 1f));
        titulo.rectTransform.anchorMin = new Vector2(0.5f, 0.8f);
        titulo.rectTransform.anchorMax = new Vector2(0.5f, 0.8f);
        titulo.rectTransform.anchoredPosition = Vector2.zero;
        titulo.rectTransform.sizeDelta = new Vector2(300, 40);
        titulo.transform.SetParent(panelCiudad.transform, false);
        
        Button btnMorada = CrearBotonConColor("BtnCiudadMorada", "MORADA", 18, new Color(0.2f, 0.7f, 0.3f, 1f));
        btnMorada.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.6f);
        btnMorada.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.6f);
        btnMorada.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        btnMorada.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 40);
        btnMorada.transform.SetParent(panelCiudad.transform, false);
        btnMorada.onClick.AddListener(() => {
            MostrarMorada();
        });
        
        Button btnTienda = CrearBotonConColor("BtnCiudadTienda", "TIENDA", 18, new Color(0.9f, 0.6f, 0.2f, 1f));
        btnTienda.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
        btnTienda.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        btnTienda.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        btnTienda.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 40);
        btnTienda.transform.SetParent(panelCiudad.transform, false);
        btnTienda.onClick.AddListener(() => {
            MostrarTiendaCiudad();
        });
        
        Button btnSalir = CrearBotonConColor("BtnCiudadSalir", "SALIR DE LA CIUDAD", 18, new Color(0.7f, 0.2f, 0.2f, 1f));
        btnSalir.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.35f);
        btnSalir.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.35f);
        btnSalir.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        btnSalir.GetComponent<RectTransform>().sizeDelta = new Vector2(240, 40);
        btnSalir.transform.SetParent(panelCiudad.transform, false);
        btnSalir.onClick.AddListener(() => {
            MostrarMapa();
        });
    }
    
    private void CrearPanelMorada()
    {
        Text titulo = CrearTexto("TituloMorada", "MORADA", 28, new Color(1f, 0.9f, 0.2f, 1f));
        titulo.rectTransform.anchorMin = new Vector2(0.5f, 0.7f);
        titulo.rectTransform.anchorMax = new Vector2(0.5f, 0.7f);
        titulo.rectTransform.anchoredPosition = Vector2.zero;
        titulo.rectTransform.sizeDelta = new Vector2(300, 40);
        titulo.transform.SetParent(panelMorada.transform, false);
        
        Button btnVolver = CrearBotonConColor("BtnMoradaVolver", "VOLVER A LA CIUDAD", 18, new Color(0.3f, 0.6f, 0.8f, 1f));
        btnVolver.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.45f);
        btnVolver.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.45f);
        btnVolver.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        btnVolver.GetComponent<RectTransform>().sizeDelta = new Vector2(240, 40);
        btnVolver.transform.SetParent(panelMorada.transform, false);
        btnVolver.onClick.AddListener(() => {
            MostrarCiudad();
        });
    }
    
    private void CrearPanelTiendaCiudad()
    {
        Text titulo = CrearTexto("TituloTiendaCiudad", "TIENDA", 28, new Color(1f, 0.9f, 0.2f, 1f));
        titulo.rectTransform.anchorMin = new Vector2(0.5f, 0.7f);
        titulo.rectTransform.anchorMax = new Vector2(0.5f, 0.7f);
        titulo.rectTransform.anchoredPosition = Vector2.zero;
        titulo.rectTransform.sizeDelta = new Vector2(300, 40);
        titulo.transform.SetParent(panelTiendaCiudad.transform, false);
        
        Button btnVolver = CrearBotonConColor("BtnTiendaVolver", "VOLVER A LA CIUDAD", 18, new Color(0.3f, 0.6f, 0.8f, 1f));
        btnVolver.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.45f);
        btnVolver.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.45f);
        btnVolver.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        btnVolver.GetComponent<RectTransform>().sizeDelta = new Vector2(240, 40);
        btnVolver.transform.SetParent(panelTiendaCiudad.transform, false);
        btnVolver.onClick.AddListener(() => {
            MostrarCiudad();
        });
    }
    #endregion
    
    #region Helpers - UI y Efectos
    #region Helpers - Botones y texto
    private Text CrearTexto(string nombre, string texto, int fontSize, Color color)
    {
        GameObject obj = new GameObject(nombre);
        Text text = obj.AddComponent<Text>();
        text.text = texto;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;
        text.resizeTextForBestFit = false;
        
        RectTransform rect = obj.GetComponent<RectTransform>();
        if (rect == null) rect = obj.AddComponent<RectTransform>();
        
        return text;
    }
    
    private Button CrearBotonConIcono(string nombre, Sprite icono, string textoFallback)
    {
        GameObject obj = new GameObject(nombre);
        Button btn = obj.AddComponent<Button>();
        Image img = obj.AddComponent<Image>();
        img.color = Color.white;
        img.sprite = icono;
        img.preserveAspect = true;
        
        if (icono == null && !string.IsNullOrEmpty(textoFallback))
        {
            Text text = CrearTexto("Text", textoFallback, 20, Color.white);
            text.transform.SetParent(obj.transform, false);
            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = new Vector2(200f, 40f);
        }
        
        return btn;
    }
    
    private IEnumerator FlashBoton(Button boton)
    {
        Image img = boton.GetComponent<Image>();
        if (img == null) yield break;
        
        Color original = img.color;
        Vector3 escalaOriginal = boton.transform.localScale;
        Vector3 escalaGrande = escalaOriginal * 1.08f;
        
        float duracion = 0.15f;
        float t = 0f;
        while (t < duracion)
        {
            float p = t / duracion;
            boton.transform.localScale = Vector3.Lerp(escalaOriginal, escalaGrande, p);
            img.color = Color.Lerp(original, Color.white, p);
            t += Time.deltaTime;
            yield return null;
        }
        
        t = 0f;
        while (t < duracion)
        {
            float p = t / duracion;
            boton.transform.localScale = Vector3.Lerp(escalaGrande, escalaOriginal, p);
            img.color = Color.Lerp(Color.white, original, p);
            t += Time.deltaTime;
            yield return null;
        }
        
        boton.transform.localScale = escalaOriginal;
        img.color = original;
    }
    
    private IEnumerator EjecutarAccionConFeedback(Button boton, Action accion, GameObject dialog)
    {
        if (boton != null)
        {
            boton.interactable = false;
        }
        yield return StartCoroutine(FlashBoton(boton));
        accion?.Invoke();
        if (dialog != null)
        {
            Destroy(dialog);
        }
    }
    
    private Button CrearBoton(string nombre, string texto, int fontSize)
    {
        GameObject obj = new GameObject(nombre);
        Button btn = obj.AddComponent<Button>();
        Image img = obj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.4f, 0.6f, 1f);
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(obj.transform, false);
        Text text = textObj.AddComponent<Text>();
        text.text = texto;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.resizeTextForBestFit = false;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        RectTransform rect = obj.GetComponent<RectTransform>();
        if (rect == null) rect = obj.AddComponent<RectTransform>();
        
        return btn;
    }
    
    private Button CrearBotonConColor(string nombre, string texto, int fontSize, Color colorBoton)
    {
        GameObject obj = new GameObject(nombre);
        Button btn = obj.AddComponent<Button>();
        Image img = obj.AddComponent<Image>();
        img.color = colorBoton;
        
        ColorBlock colors = btn.colors;
        colors.normalColor = colorBoton;
        colors.highlightedColor = new Color(Mathf.Min(1f, colorBoton.r * 1.3f), Mathf.Min(1f, colorBoton.g * 1.3f), Mathf.Min(1f, colorBoton.b * 1.3f), 1f);
        colors.pressedColor = new Color(colorBoton.r * 0.7f, colorBoton.g * 0.7f, colorBoton.b * 0.7f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(colorBoton.r * 0.5f, colorBoton.g * 0.5f, colorBoton.b * 0.5f, 0.5f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.1f;
        btn.colors = colors;
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(obj.transform, false);
        Text text = textObj.AddComponent<Text>();
        text.text = texto;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.resizeTextForBestFit = false;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        RectTransform rect = obj.GetComponent<RectTransform>();
        if (rect == null) rect = obj.AddComponent<RectTransform>();
        
        AgregarAnimacionesBoton(btn);
        
        return btn;
    }
    
    private void AgregarAnimacionesBoton(Button btn)
    {
        EventTrigger trigger = btn.gameObject.AddComponent<EventTrigger>();
        
        EventTrigger.Entry entryEnter = new EventTrigger.Entry();
        entryEnter.eventID = EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((data) => {
            StartCoroutine(AnimarBotonEscala(btn.transform, 1.05f, 0.1f));
        });
        trigger.triggers.Add(entryEnter);
        
        EventTrigger.Entry entryExit = new EventTrigger.Entry();
        entryExit.eventID = EventTriggerType.PointerExit;
        entryExit.callback.AddListener((data) => {
            StartCoroutine(AnimarBotonEscala(btn.transform, 1f, 0.1f));
        });
        trigger.triggers.Add(entryExit);
        
        EventTrigger.Entry entryDown = new EventTrigger.Entry();
        entryDown.eventID = EventTriggerType.PointerDown;
        entryDown.callback.AddListener((data) => {
            StartCoroutine(AnimarBotonEscala(btn.transform, 0.95f, 0.05f));
        });
        trigger.triggers.Add(entryDown);
        
        EventTrigger.Entry entryUp = new EventTrigger.Entry();
        entryUp.eventID = EventTriggerType.PointerUp;
        entryUp.callback.AddListener((data) => {
            StartCoroutine(AnimarBotonEscala(btn.transform, 1.05f, 0.05f));
        });
        trigger.triggers.Add(entryUp);
    }
    
    private IEnumerator AnimarBotonEscala(Transform transform, float escalaObjetivo, float duracion)
    {
        if (transform == null) yield break;
        
        Vector3 escalaInicial = transform.localScale;
        Vector3 escalaFinal = new Vector3(escalaObjetivo, escalaObjetivo, 1f);
        float tiempo = 0f;
        
        while (tiempo < duracion)
        {
            if (transform == null) yield break;
            
            tiempo += Time.deltaTime;
            float t = tiempo / duracion;
            transform.localScale = Vector3.Lerp(escalaInicial, escalaFinal, t);
            yield return null;
        }
        
        if (transform != null)
        {
            transform.localScale = escalaFinal;
        }
    }
    #endregion
    
    #region Helpers - Sliders
    private Slider CrearSlider(string nombre, Color color)
    {
        GameObject obj = new GameObject(nombre);
        Slider slider = obj.AddComponent<Slider>();
        
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(obj.transform, false);
        Image bg = bgObj.AddComponent<Image>();
        bg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        slider.targetGraphic = bg;
        
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(obj.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = Vector2.zero;
        
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillArea.transform, false);
        Image fill = fillObj.AddComponent<Image>();
        fill.color = color;
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.sizeDelta = Vector2.zero;
        
        slider.fillRect = fillRect;
        slider.maxValue = 100;
        slider.minValue = 0;
        slider.value = 100;
        slider.wholeNumbers = true;
        
        RectTransform rect = obj.GetComponent<RectTransform>();
        if (rect == null) rect = obj.AddComponent<RectTransform>();
        
        return slider;
    }
    #endregion
    #endregion
    
    #region UI - Paneles (Mostrar / Ocultar)
    public void MostrarPanelMenu()
    {
        OcultarTodosLosPaneles();
        panelMenu.SetActive(true);
    }
    
    public void MostrarPanelJuego()
    {
        OcultarTodosLosPaneles();
        panelJuego.SetActive(true);
        
        if (animacionAtaque != null)
        {
            animacionAtaque.gameObject.SetActive(false);
            animacionAtaque.color = new Color(1f, 1f, 1f, 0f);
            animacionAtaque.sprite = null;
        }
        
        Sprite spriteJugador = gameManager.GetSpriteJugador();
        Sprite spriteEnemigo = gameManager.GetSpriteEnemigo();
        
        string tipoJugador = gameManager.GetTipoJugador();
        string tipoEnemigo = gameManager.GetTipoEnemigo();
        bool usarCaballeroFrames = tipoJugador == "Guerrero" && HasCaballeroFramesReady();
        if (!string.IsNullOrEmpty(tipoJugador))
        {
            IniciarAnimacionIdle(tipoJugador, true);
        }
        if (!string.IsNullOrEmpty(tipoEnemigo))
        {
            IniciarAnimacionIdle(tipoEnemigo, false);
        }
        
        if (!usarCaballeroFrames && spriteJugador != null && imgJugador != null)
        {
            imgJugador.sprite = spriteJugador;
            imgJugador.color = Color.white;
            imgJugador.gameObject.SetActive(true);
        }
        else if (!usarCaballeroFrames && imgJugador != null)
        {
            imgJugador.gameObject.SetActive(false);
        }
        else if (usarCaballeroFrames && imgJugador != null)
        {
            imgJugador.gameObject.SetActive(true);
            imgJugador.color = Color.white;
        }
        
        if (spriteEnemigo != null && imgEnemigo != null)
        {
            imgEnemigo.sprite = spriteEnemigo;
            imgEnemigo.color = Color.white;
            imgEnemigo.gameObject.SetActive(true);
        }
        else if (imgEnemigo != null)
        {
            imgEnemigo.gameObject.SetActive(false);
        }
    }
    
    public void MostrarPanelDescanso()
    {
        // Panel eliminado
    }
    
    public void MostrarPanelTienda(int oro)
    {
        // Panel eliminado
    }
    
    public void ActualizarContadoresPostBatalla(int clemencias, int muertes)
    {
        if (lblClemencias != null) lblClemencias.text = $"Clemencia: {clemencias}";
        if (lblMuertes != null) lblMuertes.text = $"Muertes: {muertes}";
    }
    
    public void MostrarMenuPostPelea()
    {
        OcultarTodosLosPaneles();
        if (panelPostPelea != null)
        {
            panelPostPelea.SetActive(true);
        }
    }
    
    public void MostrarMapa()
    {
        OcultarTodosLosPaneles();
        if (panelMapa != null) panelMapa.SetActive(true);
    }
    
    public void MostrarCiudad()
    {
        OcultarTodosLosPaneles();
        if (panelCiudad != null) panelCiudad.SetActive(true);
    }
    
    public void MostrarMorada()
    {
        OcultarTodosLosPaneles();
        if (panelMorada != null) panelMorada.SetActive(true);
    }
    
    public void MostrarTiendaCiudad()
    {
        OcultarTodosLosPaneles();
        if (panelTiendaCiudad != null) panelTiendaCiudad.SetActive(true);
    }
    
    private void OcultarTodosLosPaneles()
    {
        if (panelMenu != null) panelMenu.SetActive(false);
        if (panelJuego != null) panelJuego.SetActive(false);
        if (panelPostPelea != null) panelPostPelea.SetActive(false);
        if (panelMapa != null) panelMapa.SetActive(false);
        if (panelCiudad != null) panelCiudad.SetActive(false);
        if (panelMorada != null) panelMorada.SetActive(false);
        if (panelTiendaCiudad != null) panelTiendaCiudad.SetActive(false);
    }
    #endregion
    
    #region UI - Actualizaciones
    public void ActualizarVidaJugador(int vida, int vidaMax)
    {
        if (lblVida1 != null) lblVida1.text = $"❤️ Vida: {vida}/{vidaMax}";
        if (pbVida1 != null)
        {
            pbVida1.maxValue = vidaMax;
            pbVida1.value = vida;
            ActualizarColorVida(pbVida1, vida, vidaMax);
        }
    }
    
    public void ActualizarEscudoJugador(int escudo, int escudoMax)
    {
        if (lblEscudo1 != null) lblEscudo1.text = $"🛡️ Escudo: {escudo}/{escudoMax}";
        if (pbEscudo1 != null)
        {
            pbEscudo1.maxValue = escudoMax;
            pbEscudo1.value = escudo;
        }
        SetDefensaDisponible(escudo > 0);
    }
    
    public void ActualizarVidaEnemigo(int vida, int vidaMax)
    {
        if (lblVida2 != null) lblVida2.text = $"❤️ Vida: {vida}/{vidaMax}";
        if (pbVida2 != null)
        {
            pbVida2.maxValue = vidaMax;
            pbVida2.value = vida;
            ActualizarColorVida(pbVida2, vida, vidaMax);
        }
    }
    
    public void ActualizarEscudoEnemigo(int escudo, int escudoMax)
    {
        if (lblEscudo2 != null) lblEscudo2.text = $"🛡️ Escudo: {escudo}/{escudoMax}";
        if (pbEscudo2 != null)
        {
            pbEscudo2.maxValue = escudoMax;
            pbEscudo2.value = escudo;
        }
    }
    
    public void AnimarDañoObjetivo(bool objetivoEsEnemigo, int dañoTotal, int vidaAntes, int vidaDespues, int vidaMax,
        int escudoAntes, int escudoDespues, int escudoMax)
    {
        Coroutine actual = objetivoEsEnemigo ? animacionDañoEnemigo : animacionDañoJugador;
        if (actual != null)
        {
            StopCoroutine(actual);
        }
        
        if (objetivoEsEnemigo)
        {
            animacionDañoEnemigo = StartCoroutine(AnimarDañoCoroutine(true, dañoTotal, vidaAntes, vidaDespues, vidaMax, escudoAntes, escudoDespues, escudoMax));
        }
        else
        {
            animacionDañoJugador = StartCoroutine(AnimarDañoCoroutine(false, dañoTotal, vidaAntes, vidaDespues, vidaMax, escudoAntes, escudoDespues, escudoMax));
        }
    }
    
    private IEnumerator AnimarDañoCoroutine(bool objetivoEsEnemigo, int dañoTotal, int vidaAntes, int vidaDespues, int vidaMax,
        int escudoAntes, int escudoDespues, int escudoMax)
    {
        Slider vidaSlider = objetivoEsEnemigo ? pbVida2 : pbVida1;
        Slider escudoSlider = objetivoEsEnemigo ? pbEscudo2 : pbEscudo1;
        Text vidaLabel = objetivoEsEnemigo ? lblVida2 : lblVida1;
        Text escudoLabel = objetivoEsEnemigo ? lblEscudo2 : lblEscudo1;
        
        if (vidaSlider != null)
        {
            vidaSlider.maxValue = vidaMax;
            vidaSlider.value = vidaAntes;
        }
        if (escudoSlider != null)
        {
            escudoSlider.maxValue = escudoMax;
            escudoSlider.value = escudoAntes;
        }
        
        bool mostrarEnVida = vidaAntes > vidaDespues;
        RectTransform referencia = null;
        if (mostrarEnVida && vidaLabel != null)
        {
            referencia = vidaLabel.rectTransform;
        }
        else if (!mostrarEnVida && escudoLabel != null)
        {
            referencia = escudoLabel.rectTransform;
        }
        else if (mostrarEnVida && vidaSlider != null)
        {
            referencia = vidaSlider.GetComponent<RectTransform>();
        }
        else if (!mostrarEnVida && escudoSlider != null)
        {
            referencia = escudoSlider.GetComponent<RectTransform>();
        }
        
        Text textoDaño = null;
        RectTransform textoRect = null;
        if (referencia != null && panelJuego != null)
        {
            textoDaño = CrearTexto("DañoFlotante", $"-{dañoTotal}", 28, mostrarEnVida ? colorDañoVida : colorDañoEscudo);
            textoRect = textoDaño.GetComponent<RectTransform>();
            textoRect.anchorMin = referencia.anchorMin;
            textoRect.anchorMax = referencia.anchorMax;
            textoRect.pivot = referencia.pivot;
            textoRect.anchoredPosition = referencia.anchoredPosition;
            textoRect.sizeDelta = new Vector2(120f, 40f);
            textoDaño.transform.SetParent(panelJuego.transform, false);
        }
        
        float duracion = Mathf.Max(0.01f, duracionAnimacionDaño);
        float tiempo = 0f;
        Vector2 posicionInicial = textoRect != null ? textoRect.anchoredPosition : Vector2.zero;
        
        while (tiempo < duracion)
        {
            float t = tiempo / duracion;
            float vidaActual = Mathf.Lerp(vidaAntes, vidaDespues, t);
            float escudoActual = Mathf.Lerp(escudoAntes, escudoDespues, t);
            
            if (vidaSlider != null)
            {
                vidaSlider.value = vidaActual;
                ActualizarColorVida(vidaSlider, Mathf.RoundToInt(vidaActual), vidaMax);
            }
            if (escudoSlider != null)
            {
                escudoSlider.value = escudoActual;
            }
            if (vidaLabel != null)
            {
                vidaLabel.text = $"❤️ Vida: {Mathf.RoundToInt(vidaActual)}/{vidaMax}";
            }
            if (escudoLabel != null)
            {
                escudoLabel.text = $"🛡️ Escudo: {Mathf.RoundToInt(escudoActual)}/{escudoMax}";
            }
            
            if (textoRect != null && textoDaño != null)
            {
                float desplazamiento = Mathf.Lerp(0f, -desplazamientoDaño, t);
                textoRect.anchoredPosition = posicionInicial + new Vector2(0f, desplazamiento);
                Color color = textoDaño.color;
                color.a = Mathf.Lerp(1f, 0f, t);
                textoDaño.color = color;
            }
            
            tiempo += Time.deltaTime;
            yield return null;
        }
        
        if (vidaSlider != null)
        {
            vidaSlider.value = vidaDespues;
            ActualizarColorVida(vidaSlider, vidaDespues, vidaMax);
        }
        if (escudoSlider != null)
        {
            escudoSlider.value = escudoDespues;
        }
        if (vidaLabel != null)
        {
            vidaLabel.text = $"❤️ Vida: {vidaDespues}/{vidaMax}";
        }
        if (escudoLabel != null)
        {
            escudoLabel.text = $"🛡️ Escudo: {escudoDespues}/{escudoMax}";
        }
        
        if (!objetivoEsEnemigo)
        {
            SetDefensaDisponible(escudoDespues > 0);
        }
        
        if (textoDaño != null)
        {
            Destroy(textoDaño.gameObject);
        }
    }
    
    private void ActualizarColorVida(Slider slider, int vida, int vidaMax)
    {
        if (slider.fillRect == null) return;
        Image fill = slider.fillRect.GetComponent<Image>();
        if (fill == null) return;
        
        float porcentaje = (float)vida / vidaMax;
        if (porcentaje <= 0.25f)
        {
            fill.color = new Color(1f, 0.2f, 0.2f); // Rojo
        }
        else if (porcentaje <= 0.5f)
        {
            fill.color = new Color(1f, 0.8f, 0f); // Amarillo
        }
        else
        {
            fill.color = new Color(0.2f, 0.8f, 0.2f); // Verde
        }
    }
    
    private GameObject CrearTarjetaStatDesdePrefab(GameObject parent, GameObject prefab, string nombre, Sprite icono, string texto)
    {
        if (prefab == null)
        {
            return CrearTarjetaStat(parent, nombre, icono, texto);
        }
        
        GameObject tarjeta = Instantiate(prefab, parent.transform, false);
        tarjeta.name = $"Tarjeta{nombre}";
        
        Image iconoImage = tarjeta.transform.Find("Icono")?.GetComponent<Image>();
        if (iconoImage != null && icono != null)
        {
            iconoImage.sprite = icono;
        }
        
        Text textoObj = tarjeta.transform.Find("Texto")?.GetComponent<Text>();
        if (textoObj != null)
        {
            textoObj.text = texto;
        }

        return tarjeta;
    }
    
    private GameObject CrearTarjetaStat(GameObject parent, string nombre, Sprite icono, string texto)
    {
        GameObject tarjeta = new GameObject($"Tarjeta{nombre}");
        tarjeta.transform.SetParent(parent.transform, false);
        RectTransform tarjetaRect = tarjeta.AddComponent<RectTransform>();
        tarjetaRect.sizeDelta = new Vector2(70, 80);
        tarjetaRect.anchorMin = new Vector2(0f, 0f);
        tarjetaRect.anchorMax = new Vector2(0f, 0f);
        tarjetaRect.pivot = new Vector2(0f, 0f);
        
        // LayoutElement es opcional si en el futuro se usa un LayoutGroup
        LayoutElement layoutElement = tarjeta.AddComponent<LayoutElement>();
        layoutElement.minWidth = 48f;
        layoutElement.preferredWidth = 70f;
        layoutElement.preferredHeight = 80f;
        layoutElement.flexibleWidth = 0f;
        layoutElement.flexibleHeight = 0f;
        
        GameObject iconoObj = new GameObject("Icono");
        iconoObj.transform.SetParent(tarjeta.transform, false);
        RectTransform iconoRect = iconoObj.AddComponent<RectTransform>();
        iconoRect.anchorMin = new Vector2(0.5f, 1f);
        iconoRect.anchorMax = new Vector2(0.5f, 1f);
        iconoRect.pivot = new Vector2(0.5f, 1f);
        iconoRect.anchoredPosition = new Vector2(0f, -6f);
        iconoRect.sizeDelta = new Vector2(48f, 48f);
        
        Image iconoImage = iconoObj.AddComponent<Image>();
        if (icono != null)
        {
            iconoImage.sprite = icono;
        }
        iconoImage.preserveAspect = true;
        
        Text textoObj = CrearTexto($"Texto{nombre}", texto, 12, Color.white);
        textoObj.rectTransform.anchorMin = new Vector2(0f, 0f);
        textoObj.rectTransform.anchorMax = new Vector2(1f, 0f);
        textoObj.rectTransform.pivot = new Vector2(0.5f, 0f);
        textoObj.rectTransform.anchoredPosition = new Vector2(0f, 6f);
        textoObj.rectTransform.sizeDelta = new Vector2(0f, 20f);
        textoObj.alignment = TextAnchor.MiddleCenter;
        textoObj.horizontalOverflow = HorizontalWrapMode.Overflow;
        textoObj.verticalOverflow = VerticalWrapMode.Overflow;
        textoObj.transform.SetParent(tarjeta.transform, false);
        
        return tarjeta;
    }
    
    private void SetTextoTarjeta(GameObject tarjeta, string texto)
    {
        if (tarjeta == null) return;
        Text textoObj = tarjeta.transform.Find("Texto")?.GetComponent<Text>();
        if (textoObj == null)
        {
            textoObj = tarjeta.GetComponentInChildren<Text>(true);
        }
        if (textoObj != null)
        {
            textoObj.text = texto;
        }
    }
    
    private void AjustarPosicionPanelJugador()
    {
        if (panelStatsJugadorRect == null) return;
        
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(panelStatsJugadorRect);
        
        panelStatsJugadorRect.anchorMin = new Vector2(0f, 0f);
        panelStatsJugadorRect.anchorMax = new Vector2(0f, 0f);
        panelStatsJugadorRect.pivot = new Vector2(0f, 0f);
        
        var pos = panelStatsJugadorRect.anchoredPosition;
        pos.y = 0f;
        panelStatsJugadorRect.anchoredPosition = pos;
    }
    
    private void AplicarOffsetHudUnaVez()
    {
        if (_offsetAplicado) return;
        _offsetAplicado = true;
        
        RectTransform panelJugador = panelStatsJugadorRectSerialized != null ? panelStatsJugadorRectSerialized : panelStatsJugadorRect;
        
        if (panelJugador != null)
        {
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(panelJugador);
            var p = panelJugador.anchoredPosition;
            panelJugador.anchoredPosition = new Vector2(p.x, 0f);
        }
        
        if (panelHudEnemigo != null)
        {
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(panelHudEnemigo);
            var e = panelHudEnemigo.anchoredPosition;
            panelHudEnemigo.anchoredPosition = new Vector2(e.x, 0f);
        }
    }
    
    private void ActualizarAnchoTarjeta(GameObject tarjeta, Text texto)
    {
        if (tarjeta == null || texto == null) return;
        
        Canvas.ForceUpdateCanvases();
        
        float anchoTexto = texto.preferredWidth;
        
        float extraPadding = 28f;
        
        float anchoTotal = Mathf.Max(anchoTexto + extraPadding, 48f);
        
        LayoutElement layoutElement = tarjeta.GetComponent<LayoutElement>();
        RectTransform rectTransform = tarjeta.GetComponent<RectTransform>();
        if (layoutElement != null)
        {
            layoutElement.preferredWidth = anchoTotal;
            layoutElement.flexibleWidth = 0f;
        }
        // Sin LayoutGroup, actualizar RectTransform directamente
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(anchoTotal, rectTransform.sizeDelta.y);
        }
    }
    
    private Sprite CargarIconoHUD(string tipo)
    {
        Dictionary<string, string> mapeoIconos = new Dictionary<string, string>
        {
            { "vida", "IconoVida" },
            { "escudo", "IconoEscudo" },
            { "oro", "IconoOro" },
            { "espada", "IconoDaño" }, // Daño usa IconoDaño
            { "reloj", "IconoBatalla" } // Batalla usa IconoBatalla
        };
        
        if (!mapeoIconos.ContainsKey(tipo))
        {
            Debug.LogWarning($"Tipo de icono no reconocido: {tipo}");
            return null;
        }
        
        string nombreArchivo = mapeoIconos[tipo];
        
        #if UNITY_EDITOR
        string[] guids = AssetDatabase.FindAssets(nombreArchivo);
        
        foreach (string guid in guids)
        {
            string ruta = AssetDatabase.GUIDToAssetPath(guid);
            
            string nombreSinExtension = System.IO.Path.GetFileNameWithoutExtension(ruta);
            if (nombreSinExtension == nombreArchivo)
            {
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
                if (sprite != null)
                {
                    return sprite;
                }
                
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(ruta);
                if (texture != null)
                {
                    Rect rect = new Rect(0, 0, texture.width, texture.height);
                    return Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 100f);
                }
            }
        }
        
        Debug.LogWarning($"No se encontró el icono: {nombreArchivo}");
        return null;
        #else
        Sprite sprite = Resources.Load<Sprite>(nombreArchivo);
        if (sprite != null) return sprite;
        
        sprite = Resources.Load<Sprite>($"Sprites/UI/{nombreArchivo}");
        if (sprite != null) return sprite;
        
        Texture2D texture = Resources.Load<Texture2D>(nombreArchivo);
        if (texture != null)
        {
            Rect rect = new Rect(0, 0, texture.width, texture.height);
            return Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 100f);
        }
        
        return null;
        #endif
    }
    
    public void ActualizarInfoJugador(int vida, int vidaMax, int escudo, int escudoMax, int oro, int daño, int batalla)
    {
        if (panelStatsJugador == null) return;
        
        if (tarjetasJugador.Count == 0)
        {
            Sprite iconoVida = CargarIconoHUD("vida");
            Sprite iconoEscudo = CargarIconoHUD("escudo");
            Sprite iconoOro = CargarIconoHUD("oro");
            Sprite iconoDaño = CargarIconoHUD("espada"); // Daño = espada
            Sprite iconoBatalla = CargarIconoHUD("reloj"); // Batalla = reloj de arena
            
            tarjetasJugador["Vida"] = CrearTarjetaStatDesdePrefab(panelStatsJugador, tarjetaVidaPrefab, "Vida", iconoVida, $"Vida: {vida}/{vidaMax}");
            tarjetasJugador["Escudo"] = CrearTarjetaStatDesdePrefab(panelStatsJugador, tarjetaEscudoPrefab, "Escudo", iconoEscudo, $"Escudo: {escudo}/{escudoMax}");
            tarjetasJugador["Oro"] = CrearTarjetaStatDesdePrefab(panelStatsJugador, tarjetaOroPrefab, "Oro", iconoOro, $"Oro: {oro}");
            tarjetasJugador["Daño"] = CrearTarjetaStatDesdePrefab(panelStatsJugador, tarjetaDañoPrefab, "Daño", iconoDaño, $"Daño: {daño}");
            tarjetasJugador["Batalla"] = CrearTarjetaStatDesdePrefab(panelStatsJugador, tarjetaBatallaPrefab, "Batalla", iconoBatalla, $"Batalla: {batalla}");
            
            // Configurar posiciones manuales para las tarjetas (sin LayoutGroup)
            if (tarjetaVidaPrefab == null && tarjetasJugador.ContainsKey("Vida"))
            {
                RectTransform vidaRect = tarjetasJugador["Vida"].GetComponent<RectTransform>();
                if (vidaRect != null) vidaRect.anchoredPosition = new Vector2(0f, 0f);
            }
            if (tarjetaEscudoPrefab == null && tarjetasJugador.ContainsKey("Escudo"))
            {
                RectTransform escudoRect = tarjetasJugador["Escudo"].GetComponent<RectTransform>();
                if (escudoRect != null) escudoRect.anchoredPosition = new Vector2(80f, 0f);
            }
            if (tarjetaOroPrefab == null && tarjetasJugador.ContainsKey("Oro"))
            {
                RectTransform oroRect = tarjetasJugador["Oro"].GetComponent<RectTransform>();
                if (oroRect != null) oroRect.anchoredPosition = new Vector2(160f, 0f);
            }
            if (tarjetaDañoPrefab == null && tarjetasJugador.ContainsKey("Daño"))
            {
                RectTransform dañoRect = tarjetasJugador["Daño"].GetComponent<RectTransform>();
                if (dañoRect != null) dañoRect.anchoredPosition = new Vector2(240f, 0f);
            }
            if (tarjetaBatallaPrefab == null && tarjetasJugador.ContainsKey("Batalla"))
            {
                RectTransform batallaRect = tarjetasJugador["Batalla"].GetComponent<RectTransform>();
                if (batallaRect != null) batallaRect.anchoredPosition = new Vector2(320f, 0f);
            }
            
            foreach (var kvp in tarjetasJugador)
            {
                string textoInicial = "";
                switch (kvp.Key)
                {
                    case "Vida": textoInicial = $"Vida: {vida}/{vidaMax}"; break;
                    case "Escudo": textoInicial = $"Escudo: {escudo}/{escudoMax}"; break;
                    case "Oro": textoInicial = $"Oro: {oro}"; break;
                    case "Daño": textoInicial = $"Daño: {daño}"; break;
                    case "Batalla": textoInicial = $"Batalla: {batalla}"; break;
                }
                SetTextoTarjeta(kvp.Value, textoInicial);
            }
            
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(panelStatsJugador.GetComponent<RectTransform>());
            
            AjustarPosicionPanelJugador();
            
            AplicarOffsetHudUnaVez();
        }
        else
        {
            if (tarjetasJugador.ContainsKey("Vida"))
            {
                SetTextoTarjeta(tarjetasJugador["Vida"], $"Vida: {vida}/{vidaMax}");
            }
            if (tarjetasJugador.ContainsKey("Escudo"))
            {
                SetTextoTarjeta(tarjetasJugador["Escudo"], $"Escudo: {escudo}/{escudoMax}");
            }
            if (tarjetasJugador.ContainsKey("Oro"))
            {
                SetTextoTarjeta(tarjetasJugador["Oro"], $"Oro: {oro}");
            }
            if (tarjetasJugador.ContainsKey("Daño"))
            {
                SetTextoTarjeta(tarjetasJugador["Daño"], $"Daño: {daño}");
            }
            if (tarjetasJugador.ContainsKey("Batalla"))
            {
                SetTextoTarjeta(tarjetasJugador["Batalla"], $"Batalla: {batalla}");
            }
            
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(panelStatsJugador.GetComponent<RectTransform>());
            
            AjustarPosicionPanelJugador();
            
            AplicarOffsetHudUnaVez();
        }
    }
    
    public void ActualizarDañoEnemigo(int daño, string nombre)
    {
        if (panelStatsEnemigo == null)
        {
            Debug.LogWarning("panelStatsEnemigo es null en ActualizarDañoEnemigo");
            return;
        }
        
        if (!panelStatsEnemigo.activeSelf)
        {
            panelStatsEnemigo.SetActive(true);
        }
        
        if (lblNombreEnemigo != null)
        {
            lblNombreEnemigo.text = nombre;
            if (!lblNombreEnemigo.gameObject.activeSelf)
            {
                lblNombreEnemigo.gameObject.SetActive(true);
            }
        }
        
        if (tarjetaDañoEnemigo == null)
        {
            Sprite iconoDaño = CargarIconoHUD("espada"); // Daño = espada
            if (iconoDaño == null)
            {
                Debug.LogWarning("No se pudo cargar el icono de daño para el enemigo");
            }
            tarjetaDañoEnemigo = CrearTarjetaStatDesdePrefab(panelStatsEnemigo, tarjetaDañoEnemigoPrefab, "DañoEnemigo", iconoDaño, $"Daño: {daño}");
            if (tarjetaDañoEnemigo != null)
            {
                tarjetaDañoEnemigo.SetActive(true);
                RectTransform tarjetaRect = tarjetaDañoEnemigo.GetComponent<RectTransform>();
                if (tarjetaDañoEnemigoPrefab == null && tarjetaRect != null)
                {
                    tarjetaRect.anchorMin = new Vector2(1f, 0f);
                    tarjetaRect.anchorMax = new Vector2(1f, 0f);
                    tarjetaRect.pivot = new Vector2(1f, 0f);
                    tarjetaRect.anchoredPosition = Vector2.zero;
                }
                Debug.Log($"Tarjeta de daño del enemigo creada: {daño}");
            }
            else
            {
                Debug.LogError("No se pudo crear la tarjeta de daño del enemigo");
            }
        }
        else
        {
            Text texto = tarjetaDañoEnemigo.transform.Find("TextoDañoEnemigo")?.GetComponent<Text>();
            if (texto == null)
            {
                texto = tarjetaDañoEnemigo.transform.Find("Texto")?.GetComponent<Text>();
            }
            if (texto != null)
            {
                texto.text = $"Daño: {daño}";
            }
            if (!tarjetaDañoEnemigo.activeSelf)
            {
                tarjetaDañoEnemigo.SetActive(true);
            }
            foreach (Transform child in tarjetaDañoEnemigo.transform)
            {
                if (child != null && !child.gameObject.activeSelf)
                {
                    child.gameObject.SetActive(true);
                }
            }
        }
    }
    
    public void ActualizarBotonHuir(int porcentaje)
    {
        if (btnHuir != null)
        {
            Text text = btnHuir.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.text = $"🏃 HUIR ({porcentaje}%) - 💰10";
            }
        }
    }
    
    public void SetBtnAtacarEnabled(bool enabled)
    {
        if (btnAtacar != null) btnAtacar.interactable = enabled;
    }
    
    public void ActualizarBotonAtaque(bool atacando)
    {
        if (btnAtacar == null) return;
        Image img = btnAtacar.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = atacando ? iconoParar : iconoAtaque;
            img.preserveAspect = true;
        }
    }
    
    public void SetDefensaDisponible(bool disponible)
    {
        if (btnDefensa == null) return;
        btnDefensa.interactable = disponible;
        Image img = btnDefensa.GetComponent<Image>();
        if (img != null)
        {
            if (disponible && iconoDefensa != null)
            {
                img.sprite = iconoDefensa;
            }
            else if (!disponible && iconoEscudoRoto != null)
            {
                img.sprite = iconoEscudoRoto;
            }
            img.preserveAspect = true;
            Color c = img.color;
            c.a = disponible ? 1f : 0.6f;
            img.color = c;
        }
    }
    
    public void MostrarFeedbackDefensa()
    {
        if (btnDefensa == null) return;
        StartCoroutine(FlashBoton(btnDefensa));
    }
    
    public void MostrarFeedbackAtaque()
    {
        if (btnAtacar == null) return;
        StartCoroutine(FlashBoton(btnAtacar));
    }
    
    public void ActualizarContadorAtaque(int segundos)
    {
        if (lblContadorAtaque != null)
        {
            lblContadorAtaque.text = segundos.ToString();
            lblContadorAtaque.gameObject.SetActive(true);
        }
    }
    
    public void OcultarContadorAtaque()
    {
        if (lblContadorAtaque != null)
        {
            lblContadorAtaque.gameObject.SetActive(false);
        }
    }
    #endregion
    
    #region UI - Ataque (Botones / Modos)
    public void ToggleOpcionesAtaque()
    {
        if (btnAutomatico != null && btnManual != null && btnAtacar != null)
        {
            if (btnAutomatico.gameObject.activeSelf || btnManual.gameObject.activeSelf)
            {
                VolverABotonAtaque();
            }
            else
            {
                MostrarOpcionesAtaque();
            }
        }
    }
    
    public void MostrarOpcionesAtaque()
    {
        if (btnAutomatico != null && btnManual != null && btnAtacar != null)
        {
            TransformarDetenerEnAutomatico();
            
            Text textoBoton = btnAtacar.GetComponentInChildren<Text>();
            if (textoBoton != null && !textoBoton.text.Contains("ATAQUE"))
            {
                textoBoton.text = "⚔️ ATAQUE";
                btnAtacar.onClick.RemoveAllListeners();
                btnAtacar.onClick.AddListener(() => gameManager.BtnAtaque_Click());
            }
            
            btnAtacar.gameObject.SetActive(false);
            btnAutomatico.gameObject.SetActive(true);
            btnManual.gameObject.SetActive(true);
        }
    }
    
    public void MostrarModoManual()
    {
        if (btnAutomatico != null && btnManual != null && btnAtacar != null)
        {
            btnAutomatico.gameObject.SetActive(false);
            btnManual.gameObject.SetActive(false);
            
            btnAtacar.gameObject.SetActive(true);
            
            Text textoBoton = btnAtacar.GetComponentInChildren<Text>();
            if (textoBoton != null)
            {
                textoBoton.text = "⚔️ ATACAR";
            }
            
            btnAtacar.onClick.RemoveAllListeners();
            btnAtacar.onClick.AddListener(() => gameManager.BtnAtacar_Click());
            
            RectTransform ataqueRect = btnAtacar.GetComponent<RectTransform>();
            if (ataqueRect != null)
            {
                ataqueRect.anchorMin = new Vector2(0.5f, 0.15f);
                ataqueRect.anchorMax = new Vector2(0.5f, 0.15f);
                ataqueRect.anchoredPosition = new Vector2(0, 0);
                ataqueRect.sizeDelta = new Vector2(250, 45);
            }
        }
    }
    
    public void VolverABotonAtaque()
    {
        if (btnAutomatico != null && btnManual != null && btnAtacar != null)
        {
            TransformarDetenerEnAutomatico();
            
            RectTransform autoRect = btnAutomatico.GetComponent<RectTransform>();
            if (autoRect != null)
            {
                autoRect.anchorMin = new Vector2(0.5f, 0.15f);
                autoRect.anchorMax = new Vector2(0.5f, 0.15f);
                autoRect.anchoredPosition = new Vector2(-130, 0);
                autoRect.sizeDelta = new Vector2(220, 40);
            }
            
            btnAutomatico.gameObject.SetActive(false);
            btnManual.gameObject.SetActive(false);
            btnAtacar.gameObject.SetActive(true);
            
            Text textoBoton = btnAtacar.GetComponentInChildren<Text>();
            if (textoBoton != null)
            {
                textoBoton.text = "⚔️ ATAQUE";
            }
            
            btnAtacar.onClick.RemoveAllListeners();
            btnAtacar.onClick.AddListener(() => gameManager.BtnAtaque_Click());
            
            RectTransform ataqueRect = btnAtacar.GetComponent<RectTransform>();
            if (ataqueRect != null)
            {
                ataqueRect.anchorMin = new Vector2(0.5f, 0.15f);
                ataqueRect.anchorMax = new Vector2(0.5f, 0.15f);
                ataqueRect.anchoredPosition = new Vector2(0, 0);
                ataqueRect.sizeDelta = new Vector2(250, 45);
            }
        }
    }
    
    public void MostrarBotonDetener()
    {
        if (btnAutomatico != null)
        {
            Text textoBoton = btnAutomatico.GetComponentInChildren<Text>();
            if (textoBoton != null)
            {
                textoBoton.text = "⏹️ DETENER";
            }
            
            ColorBlock colors = btnAutomatico.colors;
            colors.normalColor = new Color(0.9f, 0.5f, 0.2f, 1f);
            btnAutomatico.colors = colors;
            
            RectTransform detenerRect = btnAutomatico.GetComponent<RectTransform>();
            if (detenerRect != null)
            {
                detenerRect.anchorMin = new Vector2(0.85f, 0.95f);
                detenerRect.anchorMax = new Vector2(0.95f, 0.98f);
                detenerRect.anchoredPosition = Vector2.zero;
                detenerRect.sizeDelta = new Vector2(100, 30);
            }
            
            btnAutomatico.gameObject.SetActive(true);
        }
    }
    
    public void TransformarAutomaticoEnDetener()
    {
        MostrarBotonDetener();
    }
    
    public void OcultarOpcionesYMostrarDetener()
    {
        if (btnManual != null) btnManual.gameObject.SetActive(false);
        if (btnAtacar != null) btnAtacar.gameObject.SetActive(false);
    }
    
    public void TransformarDetenerEnAutomatico()
    {
        if (btnAutomatico != null)
        {
            Text textoBoton = btnAutomatico.GetComponentInChildren<Text>();
            if (textoBoton != null)
            {
                textoBoton.text = "🔄 AUTOMÁTICO";
            }
            ColorBlock colors = btnAutomatico.colors;
            colors.normalColor = new Color(0.2f, 0.6f, 0.8f, 1f);
            btnAutomatico.colors = colors;
        }
    }
    
    public void MostrarBotonesAtaque(bool modoAutomatico)
    {
    }
    
    public void MostrarBotónDetener(bool mostrar)
    {
    }
    
    public bool EsBotonDetener()
    {
        if (btnAutomatico != null)
        {
            Text textoBoton = btnAutomatico.GetComponentInChildren<Text>();
            if (textoBoton != null)
            {
                return textoBoton.text.Contains("DETENER");
            }
        }
        return false;
    }
    #endregion
    
    #region Animaciones (Ataque / Hurt / Idle)
    public void MostrarAnimacionAtaque(string tipo, bool esJugador, int daño, string atacante, string objetivo)
    {
        if (animacionAtaque != null)
        {
            animacionAtaque.gameObject.SetActive(false);
        }
        
        if (tipo == "Guerrero" && esJugador && caballeroSheet != null)
        {
            if (guerreroAtaqueEnCurso) return;
            guerreroAtaqueEnCurso = true;
            DetenerAnimacionesIdle();
            if (corrutinaAnimacion != null)
            {
                StopCoroutine(corrutinaAnimacion);
            }
            corrutinaAnimacion = StartCoroutine(ReproducirAtaqueGuerreroCompleto(daño));
        }
    }

    public void IniciarAtaqueGuerrero()
    {
        MostrarAnimacionAtaque("Guerrero", true, 10, "", "");
    }
    
    public void MostrarAnimacionHurt(string tipo, bool esJugador)
    {
        if (animacionAtaque != null)
        {
            animacionAtaque.gameObject.SetActive(false);
        }
    }
    
    public void IniciarAnimacionIdle(string tipo, bool esJugador)
    {
        if (animacionAtaque == null) return;
        
        if (tipo == "Guerrero" && esJugador)
        {
            MostrarAnimacionIdleCaballero();
            return;
        }
        
        int cantidadFrames = 0;
        string carpeta = "";
        string animacion = "idle";
        
        if (tipo == "Mago")
        {
            cantidadFrames = 3; // 3 frames para idle del mago
            carpeta = "Mago";
        }
        else
        {
            return;
        }
        
        if (cantidadFrames > 0)
        {
            if (esJugador)
            {
                if (corrutinaIdleJugador != null)
                {
                    StopCoroutine(corrutinaIdleJugador);
                }
                corrutinaIdleJugador = StartCoroutine(ReproducirAnimacionIdle(carpeta, animacion, cantidadFrames, esJugador));
            }
            else
            {
                if (corrutinaIdleEnemigo != null)
                {
                    StopCoroutine(corrutinaIdleEnemigo);
                }
                corrutinaIdleEnemigo = StartCoroutine(ReproducirAnimacionIdle(carpeta, animacion, cantidadFrames, esJugador));
            }
        }
    }
    
    public void MostrarAnimacionIdleCaballero()
    {
        if (caballeroSheet == null && !HasCaballeroFramesReady()) return;
        if (imgJugador == null) return;
        if (imgCaballero != null)
        {
            imgCaballero.gameObject.SetActive(false);
        }
        
        if (corrutinaIdleCaballero != null)
        {
            StopCoroutine(corrutinaIdleCaballero);
        }
        
        imgJugador.gameObject.SetActive(true);
        imgJugador.color = Color.white;
        imgJugador.preserveAspect = true;
        ApplyCaballeroSizeFromFrames();
        corrutinaIdleCaballero = StartCoroutine(ReproducirIdleCaballero());
    }

    private void DetenerAnimacionesIdle()
    {
        if (corrutinaIdleJugador != null)
        {
            StopCoroutine(corrutinaIdleJugador);
            corrutinaIdleJugador = null;
        }
        if (corrutinaIdleEnemigo != null)
        {
            StopCoroutine(corrutinaIdleEnemigo);
            corrutinaIdleEnemigo = null;
        }
        if (corrutinaIdleCaballero != null)
        {
            StopCoroutine(corrutinaIdleCaballero);
            corrutinaIdleCaballero = null;
        }
    }

    private IEnumerator ReproducirIdleCaballero()
    {
        Sprite[] sprites = ObtenerSpritesCaballeroIdle();
        if (sprites.Length == 0) yield break;
        
        int index = 0;
        while (true)
        {
            if (imgJugador == null) yield break;
            Sprite sprite = sprites[index];
            if (sprite != null)
            {
                ApplySpriteToImage(imgJugador, sprite);
            }
            index = (index + 1) % sprites.Length;
            yield return new WaitForSeconds(caballeroIdleFrameTime);
        }
    }

    private IEnumerator IniciarIdleCaballeroDespuesDeInit()
    {
        yield return null;
        if ((caballeroSheet != null || HasCaballeroFramesReady()) && imgJugador != null)
        {
            MostrarAnimacionIdleCaballero();
        }
    }
    
    private IEnumerator ReproducirAnimacion(string carpeta, string animacion, int cantidadFrames, int daño, bool esJugador)
    {
        if (animacionAtaque == null) yield break;
        
        animacionAtaque.gameObject.SetActive(true);
        
        Sprite[] sprites = new Sprite[cantidadFrames];
        
        for (int i = 0; i < cantidadFrames; i++)
        {
            #if UNITY_EDITOR
            string rutaNueva = $"Assets/Sprites/Personajes/{carpeta}/{animacion}/{i + 1}.png";
            sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(rutaNueva);
            
            if (sprites[i] == null)
            {
                string ruta = $"Assets/Sprites/Personajes/{carpeta}/{i + 1}.png";
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
            }
            
            if (sprites[i] == null)
            {
                sprites[i] = Resources.Load<Sprite>($"Sprites/Personajes/{carpeta}/{animacion}/{i + 1}");
            }
            
            if (sprites[i] == null)
            {
                sprites[i] = Resources.Load<Sprite>($"Sprites/Personajes/{carpeta}/{i + 1}");
            }
            
            if (sprites[i] == null)
            {
                sprites[i] = Resources.Load<Sprite>($"Sprites/Personajes/{carpeta}/{animacion}/{(i + 1).ToString()}");
            }
            #else
            sprites[i] = Resources.Load<Sprite>($"Sprites/Personajes/{carpeta}/{animacion}/{i + 1}");
            if (sprites[i] == null)
            {
                sprites[i] = Resources.Load<Sprite>($"Sprites/Personajes/{carpeta}/{i + 1}");
            }
            if (sprites[i] == null)
            {
                sprites[i] = Resources.Load<Sprite>($"Sprites/Personajes/{carpeta}/{animacion}/{(i + 1).ToString()}");
            }
            #endif
        }
        
        float tiempoPorFrame = 0.55f; // Velocidad de animación (0.10 segundos menos que antes)
        
        int spritesCargados = 0;
        for (int i = 0; i < cantidadFrames; i++)
        {
            if (sprites[i] != null)
            {
                spritesCargados++;
            }
        }
        
        Debug.Log($"Iniciando animación: {carpeta}, Frames esperados: {cantidadFrames}, Cargados: {spritesCargados}");
        
        Debug.Log($"Reproduciendo {cantidadFrames} frames para {carpeta}");
        for (int i = 0; i < cantidadFrames; i++)
        {
            if (sprites[i] != null)
            {
                animacionAtaque.sprite = sprites[i];
                animacionAtaque.color = new Color(1f, 1f, 1f, 1f); // Mantener opacidad completa para el sprite
                Debug.Log($"Mostrando frame {i + 1}/{cantidadFrames} de {carpeta}");
            }
            else
            {
                Debug.LogWarning($"Sprite {i + 1} de {carpeta} no se pudo cargar - continuando con siguiente frame");
            }
            
            yield return new WaitForSeconds(tiempoPorFrame);
        }
        
        Debug.Log($"Animación completada: {carpeta}");
        
        
        yield return new WaitForSeconds(1.5f);
        
        OcultarAnimacion();
        
        if (esJugador)
        {
            string tipoJugador = gameManager != null ? gameManager.GetTipoJugador() : "";
            if (!string.IsNullOrEmpty(tipoJugador))
            {
                IniciarAnimacionIdle(tipoJugador, true);
            }
        }
        else
        {
            string tipoEnemigo = gameManager != null ? gameManager.GetTipoEnemigo() : "";
            if (!string.IsNullOrEmpty(tipoEnemigo))
            {
                IniciarAnimacionIdle(tipoEnemigo, false);
            }
        }
    }

    private IEnumerator ReproducirAtaqueGuerreroCompleto(int daño)
    {
        if (imgJugador == null) yield break;
        if (!imgJugador.gameObject.activeInHierarchy)
        {
            imgJugador.gameObject.SetActive(true);
        }
        imgJugador.color = Color.white;
        imgJugador.preserveAspect = true;
        
        if (HasCaballeroFramesReady())
        {
            // Inicio de carrera: sprites 16..19 (sin movimiento)
            Sprite[] inicioCarreraFrames = caballeroFrames != null ? GetCaballeroFramesExact(Enumerable.Range(16, 4).ToArray(), "inicio carrera 16..19") : Array.Empty<Sprite>();
            // Corrida derecha: sprites 20..23 (con movimiento)
            Sprite[] runRightFrames = caballeroFrames != null ? GetCaballeroFramesExact(Enumerable.Range(20, 4).ToArray(), "corrida derecha 20..23") : Array.Empty<Sprite>();
            // Golpe: sprites 24..27 (sin movimiento, en el extremo)
            Sprite[] caballeroAttackFramesLocal = caballeroFrames != null ? GetCaballeroFramesExact(Enumerable.Range(24, 4).ToArray(), "golpe 24..27") : Array.Empty<Sprite>();
            // Corrida izquierda: sprites 28..31 (con movimiento)
            Sprite[] baseLeftFrames = caballeroFrames != null ? GetCaballeroFramesExact(Enumerable.Range(28, 4).ToArray(), "corrida izquierda 28..31") : Array.Empty<Sprite>();
            int targetRunCount = runRightFrames.Length > 0 ? runRightFrames.Length : 4;
            Sprite[] runLeftFrames = RepeatFrames(baseLeftFrames, targetRunCount);
            
            if (inicioCarreraFrames.Length == 0)
            {
                guerreroAtaqueEnCurso = false;
                yield break;
            }
            if (runRightFrames.Length == 0)
            {
                guerreroAtaqueEnCurso = false;
                yield break;
            }
            if (caballeroAttackFramesLocal.Length == 0)
            {
                guerreroAtaqueEnCurso = false;
                yield break;
            }
            if (runLeftFrames.Length == 0)
            {
                guerreroAtaqueEnCurso = false;
                yield break;
            }
            
            RectTransform caballeroRect = imgJugador.rectTransform;
            Vector2 caballeroInicio = caballeroRect.anchoredPosition;
            Vector2 caballeroObjetivo = caballeroInicio + new Vector2(400f, 0f);
            if (imgEnemigo != null)
            {
                RectTransform enemigoRect = imgEnemigo.rectTransform;
                float worldDelta = enemigoRect.position.x - caballeroRect.position.x;
                float moveRight = Mathf.Max(200f, Mathf.Abs(worldDelta) - 150f);
                caballeroObjetivo = caballeroInicio + new Vector2(moveRight, 0f);
            }
            
            // Fase 1: Inicio de carrera (16..19) - SIN MOVIMIENTO
            for (int i = 0; i < inicioCarreraFrames.Length; i++)
            {
                if (inicioCarreraFrames[i] != null)
                {
                    ApplySpriteToImage(imgJugador, inicioCarreraFrames[i]);
                }
                yield return new WaitForSeconds(caballeroIdleFrameTime);
            }
            
            // Fase 2: Corrida derecha (20..23) - CON MOVIMIENTO
            // Repetir los frames varias veces para que el movimiento dure más y se vea natural
            int ciclosCorrida = 3; // Repetir 3 veces el ciclo completo de 4 frames (20-23)
            List<Sprite> idaFramesRepetidos = new List<Sprite>();
            for (int ciclo = 0; ciclo < ciclosCorrida; ciclo++)
            {
                for (int i = 0; i < runRightFrames.Length; i++)
                {
                    if (runRightFrames[i] != null)
                    {
                        idaFramesRepetidos.Add(runRightFrames[i]);
                    }
                }
            }
            Sprite[] idaFrames = idaFramesRepetidos.ToArray();
            float runFrameTime = caballeroIdleFrameTime;
            yield return StartCoroutine(MoverConFrames(imgJugador, caballeroInicio, caballeroObjetivo, idaFrames, runFrameTime));
            caballeroRect.anchoredPosition = caballeroObjetivo;
            
            // Fase 3: Golpe en el extremo con frames 24..27
            for (int i = 0; i < caballeroAttackFramesLocal.Length; i++)
            {
                if (caballeroAttackFramesLocal[i] != null)
                {
                    ApplySpriteToImage(imgJugador, caballeroAttackFramesLocal[i]);
                }
                yield return new WaitForSeconds(caballeroAttackFrameTime);
            }
            // Forzar último frame en el extremo antes de aplicar daño
            Sprite lastAttackFrame = caballeroAttackFramesLocal[caballeroAttackFramesLocal.Length - 1];
            if (lastAttackFrame != null)
            {
                ApplySpriteToImage(imgJugador, lastAttackFrame);
            }
            caballeroRect.anchoredPosition = caballeroObjetivo;
            yield return new WaitForSeconds(caballeroAttackFrameTime);
            
            if (gameManager != null)
            {
                gameManager.IniciarDañoEnemigoDesdeUI(daño);
            }
            
            // Forzar al último frame en el extremo antes de volver
            if (lastAttackFrame != null)
            {
                ApplySpriteToImage(imgJugador, lastAttackFrame);
            }
            caballeroRect.anchoredPosition = caballeroObjetivo;
            yield return new WaitForSeconds(caballeroAttackFrameTime);
            
            // Fase 4: Volver usando frames de correr a la izquierda (28..31) - CON MOVIMIENTO
            // Repetir los frames varias veces para que el movimiento dure más y se vea natural
            int ciclosVuelta = 3; // Repetir 3 veces el ciclo completo de 4 frames (28-31)
            List<Sprite> vueltaFramesRepetidos = new List<Sprite>();
            for (int ciclo = 0; ciclo < ciclosVuelta; ciclo++)
            {
                for (int i = 0; i < baseLeftFrames.Length; i++)
                {
                    if (baseLeftFrames[i] != null)
                    {
                        vueltaFramesRepetidos.Add(baseLeftFrames[i]);
                    }
                }
            }
            Sprite[] returnFrames = vueltaFramesRepetidos.ToArray();
            if (returnFrames.Length == 0)
            {
                returnFrames = lastAttackFrame != null ? new Sprite[] { lastAttackFrame } : Array.Empty<Sprite>();
            }
            float returnFrameTime = caballeroIdleFrameTime;
            yield return StartCoroutine(MoverConFrames(imgJugador, caballeroObjetivo, caballeroInicio, returnFrames, returnFrameTime));
            
            // Asegurar que el personaje esté en la posición inicial antes de iniciar idle
            caballeroRect.anchoredPosition = caballeroInicio;
            
            // Iniciar idle después de que termine completamente la vuelta
            IniciarAnimacionIdle("Guerrero", true);
            guerreroAtaqueEnCurso = false;
            yield break;
        }
        
        RectTransform jugadorRect = imgJugador.rectTransform;
        Canvas canvas = jugadorRect.GetComponentInParent<Canvas>();
        // Guardamos la posición anchoredPosition original para volver después
        Vector2 inicio = jugadorRect.anchoredPosition;
        // Calculamos posiciones ABSOLUTAS en el Canvas (no relativas a anchors)
        Vector2 jugadorPosAbsoluta = RectTransformUtility.PixelAdjustPoint(jugadorRect.position, jugadorRect, canvas);
        Vector2 enemigoPosAbsoluta = Vector2.zero;
        if (imgEnemigo != null)
        {
            RectTransform enemigoRect = imgEnemigo.rectTransform;
            enemigoPosAbsoluta = RectTransformUtility.PixelAdjustPoint(enemigoRect.position, enemigoRect, canvas);
        }
        // La distancia que queremos recorrer hacia el enemigo
        float distanciaAvance = 0f;
        if (imgEnemigo != null)
        {
            // Calculamos cuánto tiene que avanzar para llegar cerca del enemigo
            distanciaAvance = enemigoPosAbsoluta.x - jugadorPosAbsoluta.x - 120f;
        }
        else
        {
            // Si no hay enemigo, avanza 400 píxeles por defecto
            distanciaAvance = 400f;
        }
        // El objetivo es la posición inicial más el avance
        Vector2 objetivo = inicio + new Vector2(distanciaAvance, 0f);
        
        Sprite[] attackFrames = ObtenerSpritesCaballeroAtaque();
        if (attackFrames.Length == 0)
        {
            guerreroAtaqueEnCurso = false;
            yield break;
        }
        
        yield return StartCoroutine(MoverConFrames(imgJugador, inicio, objetivo, attackFrames, caballeroAttackFrameTime));
        
        Debug.Log("[UIManager] Llamando a IniciarDañoEnemigoDesdeUI. daño=" + daño + ", gameManager=" + (gameManager != null));
        if (gameManager != null)
        {
            gameManager.IniciarDañoEnemigoDesdeUI(daño);
        }
        
        yield return StartCoroutine(MoverConFrames(imgJugador, objetivo, inicio, attackFrames, caballeroAttackFrameTime));
        
        jugadorRect.anchoredPosition = inicio;
        IniciarAnimacionIdle("Guerrero", true);
        guerreroAtaqueEnCurso = false;
    }
    
    private IEnumerator ReproducirAtaquePersonaje(string tipo, bool esJugador)
    {
        Image atacante = esJugador ? imgJugador : imgEnemigo;
        Image objetivoImg = esJugador ? imgEnemigo : imgJugador;
        if (atacante == null) yield break;
        
        if (!atacante.gameObject.activeInHierarchy)
        {
            atacante.gameObject.SetActive(true);
        }
        atacante.color = Color.white;
        atacante.preserveAspect = true;
        
        RectTransform atacanteRect = atacante.rectTransform;
        Vector2 inicio = atacanteRect.anchoredPosition;
        Vector2 objetivo = inicio + (esJugador ? new Vector2(260f, 0f) : new Vector2(-260f, 0f));
        if (objetivoImg != null)
        {
            Vector2 targetPos = objetivoImg.rectTransform.anchoredPosition;
            objetivo = esJugador ? new Vector2(targetPos.x - 150f, inicio.y) : new Vector2(targetPos.x + 150f, inicio.y);
        }
        
        Sprite[] runOut = ObtenerSpritesRun(tipo, esJugador ? true : false);
        Sprite[] attack = ObtenerSpritesAttack(tipo);
        Sprite[] runBack = ObtenerSpritesRun(tipo, esJugador ? false : true);
        
        int runOutCount = Mathf.Max(1, runOut.Length);
        for (int i = 0; i < runOutCount; i++)
        {
            float t = runOutCount == 1 ? 1f : (float)i / (runOutCount - 1);
            atacanteRect.anchoredPosition = Vector2.Lerp(inicio, objetivo, t);
            if (i < runOut.Length && runOut[i] != null)
            {
                ApplySpriteToImage(atacante, runOut[i]);
            }
            yield return new WaitForSeconds(guerreroRunFrameTime);
        }
        
        for (int i = 0; i < attack.Length; i++)
        {
            if (attack[i] != null)
            {
                ApplySpriteToImage(atacante, attack[i]);
            }
            yield return new WaitForSeconds(guerreroAttackFrameTime);
        }
        
        int runBackCount = Mathf.Max(1, runBack.Length);
        for (int i = 0; i < runBackCount; i++)
        {
            float t = runBackCount == 1 ? 1f : (float)i / (runBackCount - 1);
            atacanteRect.anchoredPosition = Vector2.Lerp(objetivo, inicio, t);
            if (i < runBack.Length && runBack[i] != null)
            {
                ApplySpriteToImage(atacante, runBack[i]);
            }
            yield return new WaitForSeconds(guerreroRunFrameTime);
        }
        
        atacanteRect.anchoredPosition = inicio;
        IniciarAnimacionIdle(tipo, esJugador);
    }
    
    /// <summary>
    /// Aplica un sprite a una imagen, respetando los pivots configurados en el Sprite Editor.
    /// IMPORTANTE: Los pivots de los sprites son CONSTANTES y solo se LEEN, NUNCA se modifican desde este código.
    /// Solo el usuario o el script AutoPivotCalculator pueden cambiar los pivots a través del Sprite Editor de Unity.
    /// Unity maneja automáticamente los pivots cuando se asigna el sprite.
    /// 
    /// NOTA: Los sprites 7, 8 y 9 (CABALLERO_7, CABALLERO_8, CABALLERO_9) pueden tener
    /// pivots diferentes a los demás, causando que se muevan cuando aparecen en la animación.
    /// Verificar en el Sprite Editor que estos 3 sprites tengan el mismo pivot que los demás.
    /// El script AutoPivotCalculator tiene permiso para modificar los pivots de los sprites.
    /// </summary>
    private void ApplySpriteToImage(Image imagen, Sprite sprite)
    {
        if (imagen == null || sprite == null) return;
        
        // ✅ MANTENER TAMAÑO FIJO: Calcular tamaño máximo ANTES de asignar el sprite
        // Esto evita que el personaje cambie de tamaño cuando se aplican sprites con diferentes rects
        Vector2 tamañoFijo = Vector2.zero;
        bool usarTamañoFijo = false;
        if (imagen == imgJugador && HasCaballeroFramesReady())
        {
            // Asegurar que el tamaño máximo esté calculado
            if (!caballeroMaxFrameSizeReady)
            {
                ApplyCaballeroSizeFromFrames();
            }
            // Guardar el tamaño fijo para aplicarlo después
            if (caballeroMaxFrameSizeReady)
            {
                tamañoFijo = caballeroMaxFrameSize;
                usarTamañoFijo = true;
            }
        }
        
        // ✅ Aplicar el nuevo sprite - Unity maneja automáticamente los pivots del Sprite Editor
        // Los pivots que cambies en el Sprite Editor se aplican automáticamente cuando asignas el sprite
        // IMPORTANTE: Después de cambiar pivots en Unity, asegurate de guardar y reimportar el asset
        Vector2 tamañoAntes = imagen.rectTransform.sizeDelta;
        Rect spriteRect = sprite.rect;
        
        imagen.sprite = sprite;
        imagen.color = Color.white;
        
        Vector2 tamañoDespuesSprite = imagen.rectTransform.sizeDelta;
        
        // ✅ MANTENER PROPORCIONES: Usar preserveAspect para evitar distorsión
        // Todos los sprites ocuparán el mismo espacio (tamañoFijo) pero mantendrán sus proporciones
        if (usarTamañoFijo)
        {
            // Guardar la posición antes de cambiar el tamaño
            Vector2 posicionAntes = imagen.rectTransform.anchoredPosition;
            Vector2 pivotSprite = sprite.pivot; // Valores absolutos en píxeles (solo para logs)
            
            // ✅ Establecer posición Y de referencia (usar el primer sprite como referencia)
            if (!caballeroPosicionYReferencia.HasValue)
            {
                caballeroPosicionYReferencia = posicionAntes.y; // Guardar posición Y inicial como referencia fija
                Debug.Log($"[UIManager] Posición Y de referencia establecida: {caballeroPosicionYReferencia.Value:F2} (sprite: {sprite.name}, pivot={pivotSprite})");
            }
            
            // Establecer el tamaño del contenedor al máximo
            // preserveAspect hará que el sprite se escale manteniendo sus proporciones dentro de ese espacio
            imagen.preserveAspect = true; // ✅ Activar preserveAspect para mantener proporciones sin distorsión
            imagen.rectTransform.sizeDelta = tamañoFijo; // Contenedor del tamaño máximo
            
            // ✅ MANTENER POSICIÓN Y CONSTANTE: Simplemente restaurar la posición Y de referencia
            // Si todos los sprites tienen el mismo pivot normalizado, Unity los posicionará igual
            // No necesitamos compensar basándonos en pivots absolutos
            Vector2 posicionDespues = imagen.rectTransform.anchoredPosition;
            float posicionYActual = posicionDespues.y;
            float posicionYObjetivo = caballeroPosicionYReferencia.Value;
            
            // Aplicar la posición Y de referencia directamente (sin compensación de pivots)
            imagen.rectTransform.anchoredPosition = new Vector2(posicionDespues.x, posicionYObjetivo);
            
            // Si hay una diferencia significativa, interpolar suavemente
            float diferenciaY = Mathf.Abs(posicionYActual - posicionYObjetivo);
            if (diferenciaY > 0.5f)
            {
                // Detener cualquier interpolación anterior
                if (corrutinaSuavizarY != null)
                {
                    StopCoroutine(corrutinaSuavizarY);
                }
                // Iniciar interpolación suave
                corrutinaSuavizarY = StartCoroutine(SuavizarPosicionY(imagen.rectTransform, posicionYActual, posicionYObjetivo, 0.1f));
                
                Debug.LogWarning($"[UIManager] ⚠️ Sprite '{sprite.name}': SALTO DETECTADO - Y actual={posicionYActual:F2}, objetivo={posicionYObjetivo:F2}, diferencia={diferenciaY:F2} (pivot={pivotSprite})");
            }
            
            // Log informativo para todos los sprites (solo si hay desplazamiento)
            if (diferenciaY > 0.01f)
            {
                Debug.Log($"[UIManager] Sprite '{sprite.name}': Y={posicionYActual:F2}->{posicionYObjetivo:F2}, pivot={pivotSprite}");
            }
        }
        else
        {
            imagen.preserveAspect = true;
            Debug.Log($"[UIManager] Sprite '{sprite.name}': rect={spriteRect.width}x{spriteRect.height}, tamaño={imagen.rectTransform.sizeDelta} (sin tamaño fijo)");
        }
        
        // ✅ Forzar actualización para que los cambios se reflejen inmediatamente
        Canvas.ForceUpdateCanvases();
        
        // ✅ Forzar el tamaño una vez más después de que Unity procese el layout
        if (usarTamañoFijo)
        {
            // Usar la posición Y actual (ya ajustada con compensación basada en la referencia fija)
            float posicionYParaForzar = imagen.rectTransform.anchoredPosition.y;
            StartCoroutine(ForzarTamañoFijo(imagen.rectTransform, tamañoFijo, posicionYParaForzar));
        }
    }
    
    private IEnumerator ForzarTamañoFijo(RectTransform rectTransform, Vector2 tamaño, float posicionYOriginal)
    {
        yield return null; // Esperar un frame para que Unity procese el layout
        if (rectTransform != null)
        {
            Vector2 tamañoAntes = rectTransform.sizeDelta;
            Vector2 posicionAntes = rectTransform.anchoredPosition;
            
            rectTransform.sizeDelta = tamaño;
            
            // ✅ Mantener la posición Y original para evitar desplazamientos verticales
            rectTransform.anchoredPosition = new Vector2(posicionAntes.x, posicionYOriginal);
            
            Canvas.ForceUpdateCanvases();
            
            Vector2 tamañoDespues = rectTransform.sizeDelta;
            Vector2 posicionDespues = rectTransform.anchoredPosition;
            
            float desplazamientoY = posicionDespues.y - posicionYOriginal;
            if (Mathf.Abs(desplazamientoY) > 0.01f)
            {
                rectTransform.anchoredPosition = new Vector2(posicionDespues.x, posicionYOriginal);
                // Log solo si el desplazamiento es significativo
                if (Mathf.Abs(desplazamientoY) > 0.5f)
                {
                    Debug.LogWarning($"[UIManager] ⚠️ ForzarTamañoFijo: desplazamiento Y={desplazamientoY:F2}, corregido");
                }
            }
        }
    }
    
    private IEnumerator SuavizarPosicionY(RectTransform rectTransform, float desdeY, float haciaY, float duracion)
    {
        if (rectTransform == null) yield break;
        
        float tiempoInicio = Time.time;
        float diferencia = haciaY - desdeY;
        
        while (Time.time - tiempoInicio < duracion)
        {
            if (rectTransform == null) yield break;
            
            float progreso = (Time.time - tiempoInicio) / duracion;
            // Usar curva de suavizado (ease-out)
            progreso = 1f - Mathf.Pow(1f - progreso, 3f);
            
            float posicionYActual = desdeY + diferencia * progreso;
            Vector2 posicionActual = rectTransform.anchoredPosition;
            rectTransform.anchoredPosition = new Vector2(posicionActual.x, posicionYActual);
            
            yield return null;
        }
        
        // Asegurar que llegue exactamente al objetivo
        if (rectTransform != null)
        {
            Vector2 posicionFinal = rectTransform.anchoredPosition;
            rectTransform.anchoredPosition = new Vector2(posicionFinal.x, haciaY);
        }
    }

    private bool HasCaballeroFramesReady()
    {
        return EnsureCaballeroFramesOrdered();
    }


    private void ApplyCaballeroSizeFromFrames()
    {
        if (imgJugador == null) return;
        if (!HasCaballeroFramesReady()) return;
        
        // ✅ VERIFICAR SPrites FALTANTES
        int spritesNullos = 0;
        List<int> indicesFaltantes = new List<int>();
        for (int i = 0; i < caballeroFrames.Length; i++)
        {
            if (caballeroFrames[i] == null)
            {
                spritesNullos++;
                indicesFaltantes.Add(i);
            }
        }
        
        if (spritesNullos > 0)
        {
            Debug.LogError($"[UIManager] ⚠️ FALTAN {spritesNullos} SPRITES en caballeroFrames! Índices faltantes: {string.Join(", ", indicesFaltantes.Take(10))}");
            if (indicesFaltantes.Count > 10)
            {
                Debug.LogError($"[UIManager] ... y {indicesFaltantes.Count - 10} más.");
            }
        }
        
        // ✅ Recalcular siempre para asegurar que tenemos el tamaño correcto
        // No usar el flag caballeroMaxFrameSizeReady para permitir recálculo si es necesario
        float maxW = 0f;
        float maxH = 0f;
        int spritesProcesados = 0;
        
        for (int i = 0; i < caballeroFrames.Length; i++)
        {
            Sprite sprite = caballeroFrames[i];
            if (sprite == null) continue;
            
            // ✅ Usar el tamaño del rect del sprite (recorte)
            Rect rect = sprite.rect;
            if (rect.width > maxW) maxW = rect.width;
            if (rect.height > maxH) maxH = rect.height;
            spritesProcesados++;
        }
        
        if (maxW > 0f && maxH > 0f)
        {
            caballeroMaxFrameSize = new Vector2(maxW, maxH);
            caballeroMaxFrameSizeReady = true;
            Debug.Log($"[UIManager] Tamaño máximo calculado: {maxW}x{maxH} (de {spritesProcesados} sprites de {caballeroFrames.Length} totales)");
        }
        else
        {
            Debug.LogWarning($"[UIManager] No se pudo calcular el tamaño máximo. Sprites procesados: {spritesProcesados} de {caballeroFrames.Length} totales");
        }
        
        // ✅ Aplicar el tamaño calculado
        if (caballeroMaxFrameSizeReady)
        {
            imgJugador.rectTransform.sizeDelta = caballeroMaxFrameSize;
            Debug.Log($"[UIManager] Tamaño aplicado a imgJugador: {caballeroMaxFrameSize}");
        }
    }

    private IEnumerator MoverConFrames(Image imagen, Vector2 inicio, Vector2 fin, Sprite[] frames, float frameTime)
    {
        if (imagen == null) yield break;
        int frameCount = Mathf.Max(1, frames.Length);
        float total = frameCount * frameTime;
        float tiempo = 0f;
        int frameIndex = -1;
        
        while (tiempo < total)
        {
            tiempo += Time.deltaTime;
            float t = total <= 0f ? 1f : Mathf.Clamp01(tiempo / total);
            float suavizado = t * t * (3f - 2f * t); // SmoothStep
            imagen.rectTransform.anchoredPosition = Vector2.Lerp(inicio, fin, suavizado);
            
            int nuevoFrame = frameCount == 0 ? 0 : Mathf.Clamp(Mathf.FloorToInt(tiempo / frameTime), 0, frameCount - 1);
            if (nuevoFrame != frameIndex)
            {
                frameIndex = nuevoFrame;
                if (frameIndex < frames.Length && frames[frameIndex] != null)
                {
                    ApplySpriteToImage(imagen, frames[frameIndex]);
                }
            }
            
            yield return null;
        }
        
        imagen.rectTransform.anchoredPosition = fin;
    }
    
    private Sprite[] ObtenerSpritesRun(string tipo, bool haciaDerecha)
    {
        return ObtenerSpritesAttack(tipo);
    }
    
    private Sprite[] ObtenerSpritesAttack(string tipo)
    {
        if (tipo == "Guerrero" && caballeroSheet != null)
        {
            return ObtenerSpritesCaballeroAtaque();
        }
        
        int cantidadFrames = 0;
        string carpeta = "";
        if (tipo == "Guerrero")
        {
            cantidadFrames = 8;
            carpeta = "Guerrero";
        }
        else if (tipo == "Mago")
        {
            cantidadFrames = 7;
            carpeta = "Mago";
        }
        else if (tipo == "Cazador")
        {
            cantidadFrames = 9;
            carpeta = "Cazador";
        }
        if (cantidadFrames == 0) return Array.Empty<Sprite>();
        return CargarSpritesDesdeRecursos(carpeta, "attack", cantidadFrames);
    }
    
    private Sprite[] CargarSpritesDesdeRecursos(string carpeta, string animacion, int cantidadFrames)
    {
        Sprite[] sprites = new Sprite[cantidadFrames];
        for (int i = 0; i < cantidadFrames; i++)
        {
            #if UNITY_EDITOR
            string rutaNueva = $"Assets/Sprites/Personajes/{carpeta}/{animacion}/{i + 1}.png";
            sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(rutaNueva);
            if (sprites[i] == null)
            {
                string ruta = $"Assets/Sprites/Personajes/{carpeta}/{i + 1}.png";
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
            }
            if (sprites[i] == null)
            {
                sprites[i] = Resources.Load<Sprite>($"Sprites/Personajes/{carpeta}/{animacion}/{i + 1}");
            }
            #else
            sprites[i] = Resources.Load<Sprite>($"Sprites/Personajes/{carpeta}/{animacion}/{i + 1}");
            #endif
        }
        return sprites;
    }
    
    private Sprite[] ObtenerSpritesCaballeroIdle()
    {
        if (TryGetCaballeroFrames(false, out Sprite[] framesFromArray))
        {
            return framesFromArray;
        }
        
        if (!TryGetCaballeroSheet(out Texture2D sheet, out float cellW, out float cellH)) return Array.Empty<Sprite>();
        
        int idleCount = Mathf.Clamp(caballeroIdleFrameCount, 1, 18);
        int[] frames = caballeroIdleFrames != null && caballeroIdleFrames.Length > 0
            ? caballeroIdleFrames
            : Enumerable.Range(0, idleCount).ToArray();
        List<Sprite> result = new List<Sprite>(frames.Length);
        float pixelsPerUnit = caballeroSheet.pixelsPerUnit;
        for (int i = 0; i < frames.Length; i++)
        {
            Sprite sprite = CrearSpriteDesdeSheetCaballero(sheet, cellW, cellH, frames[i], pixelsPerUnit);
            if (sprite != null)
            {
                result.Add(sprite);
            }
        }
        return result.ToArray();
    }
    
    private Sprite[] ObtenerSpritesCaballeroAtaque()
    {
        if (TryGetCaballeroFrames(true, out Sprite[] framesFromArray))
        {
            return framesFromArray;
        }
        
        if (!TryGetCaballeroSheet(out Texture2D sheet, out float cellW, out float cellH)) return Array.Empty<Sprite>();
        
        int attackStart = 23;
        int maxAttack = Mathf.Max(1, 30 - attackStart); // 23..29
        int attackCount = Mathf.Clamp(caballeroAttackFrameCount, 1, maxAttack);
        int[] frames = caballeroAttackFrames != null && caballeroAttackFrames.Length > 0
            ? caballeroAttackFrames
            : Enumerable.Range(attackStart, attackCount).ToArray();
        List<Sprite> result = new List<Sprite>(frames.Length);
        float pixelsPerUnit = caballeroSheet.pixelsPerUnit;
        for (int i = 0; i < frames.Length; i++)
        {
            Sprite sprite = CrearSpriteDesdeSheetCaballero(sheet, cellW, cellH, frames[i], pixelsPerUnit);
            if (sprite != null)
            {
                result.Add(sprite);
            }
        }
        return result.ToArray();
    }

    private bool TryGetCaballeroFrames(bool ataque, out Sprite[] result)
    {
        result = Array.Empty<Sprite>();
        if (caballeroFrames == null || caballeroFrames.Length < 32)
        {
            return false;
        }
        
        int idleCount = Mathf.Clamp(caballeroIdleFrameCount, 1, 18);
        int attackCount = Mathf.Clamp(caballeroAttackFrameCount, 1, 7);
        int[] frames = ataque
            ? (caballeroAttackFrames != null && caballeroAttackFrames.Length > 0
                ? caballeroAttackFrames
                : Enumerable.Range(23, attackCount).ToArray())
            : (caballeroIdleFrames != null && caballeroIdleFrames.Length > 0
                ? caballeroIdleFrames
                : Enumerable.Range(0, idleCount).ToArray());
        
        result = PickCaballeroFrames(frames);
        return result.Length > 0;
    }

    private Sprite[] PickCaballeroFrames(int[] indices)
    {
        if (indices == null || indices.Length == 0) return Array.Empty<Sprite>();
        List<Sprite> list = new List<Sprite>(indices.Length);
        for (int i = 0; i < indices.Length; i++)
        {
            int index = indices[i];
            if (index < 0 || index >= caballeroFrames.Length) continue;
            Sprite sprite = caballeroFrames[index];
            if (sprite != null)
            {
                list.Add(sprite);
            }
        }
        return list.ToArray();
    }

    private Sprite[] GetCaballeroFramesExact(int[] indices, string label)
    {
        if (indices == null || indices.Length == 0) return Array.Empty<Sprite>();
        if (!EnsureCaballeroFramesOrdered())
        {
            Debug.LogError($"[UIManager] caballeroFrames no está ordenado o le faltan sprites para {label}.");
            return Array.Empty<Sprite>();
        }
        List<Sprite> result = new List<Sprite>();
        for (int i = 0; i < indices.Length; i++)
        {
            int index = indices[i];
            if (index < 0 || index >= caballeroFramesOrdered.Length)
            {
                Debug.LogWarning($"[UIManager] Índice fuera de rango en {label}: índice {index}. Se usará el sprite anterior disponible.");
                // Usar el último sprite disponible si hay uno
                if (result.Count > 0)
                {
                    result.Add(result[result.Count - 1]);
                    continue;
                }
                // Si no hay sprites anteriores, buscar el sprite más cercano disponible
                for (int j = index - 1; j >= 0; j--)
                {
                    if (j < caballeroFramesOrdered.Length && caballeroFramesOrdered[j] != null)
                    {
                        result.Add(caballeroFramesOrdered[j]);
                        break;
                    }
                }
                continue;
            }
            Sprite sprite = caballeroFramesOrdered[index];
            if (sprite == null)
            {
                Debug.LogWarning($"[UIManager] Sprite faltante en {label}: índice {index}. Se usará el sprite anterior disponible.");
                // Usar el último sprite disponible si hay uno
                if (result.Count > 0)
                {
                    result.Add(result[result.Count - 1]);
                    continue;
                }
                // Si no hay sprites anteriores, buscar el sprite más cercano disponible
                for (int j = index - 1; j >= 0; j--)
                {
                    if (j < caballeroFramesOrdered.Length && caballeroFramesOrdered[j] != null)
                    {
                        result.Add(caballeroFramesOrdered[j]);
                        break;
                    }
                }
                continue;
            }
            result.Add(sprite);
        }
        if (result.Count == 0)
        {
            Debug.LogError($"[UIManager] No se pudo obtener ningún sprite para {label}.");
            return Array.Empty<Sprite>();
        }
        return result.ToArray();
    }

    private bool EnsureCaballeroFramesOrdered()
    {
        if (caballeroFramesOrderedReady && caballeroFramesOrdered != null && caballeroFramesOrdered.Length == 32)
        {
            return true;
        }
        if (caballeroFrames == null || caballeroFrames.Length < 32)
        {
            Debug.LogError("[UIManager] caballeroFrames debe tener 32 sprites (CABALLERO_0..CABALLERO_31).");
            return false;
        }
        Sprite[] ordered = new Sprite[32];
        for (int i = 0; i < caballeroFrames.Length; i++)
        {
            Sprite sprite = caballeroFrames[i];
            if (sprite == null) continue;
            int index = ExtractCaballeroIndex(sprite.name);
            if (index < 0 || index >= ordered.Length) continue;
            ordered[index] = sprite;
        }
        caballeroFramesOrdered = ordered;
        caballeroFramesOrderedReady = true;
        return true;
    }

    private int ExtractCaballeroIndex(string spriteName)
    {
        if (string.IsNullOrEmpty(spriteName)) return -1;
        string digits = new string(spriteName.Where(char.IsDigit).ToArray());
        if (int.TryParse(digits, out int index)) return index;
        return -1;
    }

    private Sprite[] RepeatFrames(Sprite[] baseFrames, int targetLength)
    {
        if (baseFrames == null || baseFrames.Length == 0 || targetLength <= 0) return Array.Empty<Sprite>();
        if (baseFrames.Length >= targetLength) return baseFrames;
        List<Sprite> repeated = new List<Sprite>(targetLength);
        int idx = 0;
        while (repeated.Count < targetLength)
        {
            repeated.Add(baseFrames[idx % baseFrames.Length]);
            idx++;
        }
        return repeated.ToArray();
    }
    
    private bool TryGetCaballeroSheet(out Texture2D sheet, out float cellW, out float cellH)
    {
        sheet = null;
        cellW = 0f;
        cellH = 0f;
        if (caballeroSheet == null) return false;
        sheet = caballeroSheet.texture;
        if (sheet == null) return false;
        sheet.filterMode = FilterMode.Point;
        sheet.wrapMode = TextureWrapMode.Clamp;
        cellW = sheet.width / 4f;
        cellH = sheet.height / 8f;
        return cellW > 0f && cellH > 0f;
    }
    
    private Sprite CrearSpriteDesdeSheetCaballero(Texture2D sheet, float cellW, float cellH, int index, float pixelsPerUnit)
    {
        const int columns = 4;
        const int rows = 8;
        int row = index / columns;
        int col = index % columns;
        if (row >= rows) return null;
        
        float x = col * cellW;
        float y = sheet.height - ((row + 1) * cellH);
        Rect rect = new Rect(x, y, cellW, cellH);
        return Sprite.Create(sheet, rect, new Vector2(0.5f, 0.5f), pixelsPerUnit);
    }
    
    
    private IEnumerator ReproducirAnimacionIdle(string carpeta, string animacion, int cantidadFrames, bool esJugador)
    {
        Image imagenObjetivo = esJugador ? imgJugador : imgEnemigo;
        if (imagenObjetivo == null || !imagenObjetivo.gameObject.activeInHierarchy) yield break;
        
        Sprite[] sprites = new Sprite[cantidadFrames];
        
        for (int i = 0; i < cantidadFrames; i++)
        {
            #if UNITY_EDITOR
            string rutaNueva = $"Assets/Sprites/Personajes/{carpeta}/{animacion}/{i + 1}.png";
            sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(rutaNueva);
            
            if (sprites[i] == null)
            {
                string ruta = $"Assets/Sprites/Personajes/{carpeta}/{i + 1}.png";
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
            }
            
            if (sprites[i] == null)
            {
                sprites[i] = Resources.Load<Sprite>($"Sprites/Personajes/{carpeta}/{animacion}/{i + 1}");
            }
            #else
            sprites[i] = Resources.Load<Sprite>($"Sprites/Personajes/{carpeta}/{animacion}/{i + 1}");
            if (sprites[i] == null)
            {
                sprites[i] = Resources.Load<Sprite>($"Sprites/Personajes/{carpeta}/{i + 1}");
            }
            #endif
        }
        
        float tiempoPorFrame = 0.5f; // Velocidad de animación idle
        
        while (true)
        {
            for (int i = 0; i < cantidadFrames; i++)
            {
                if (imagenObjetivo == null || !imagenObjetivo.gameObject.activeInHierarchy)
                {
                    yield break;
                }
                
                if (sprites[i] != null)
                {
                    imagenObjetivo.sprite = sprites[i];
                }
                yield return new WaitForSeconds(tiempoPorFrame);
            }
        }
    }
    
    private void MostrarTextoDaño(int daño, bool esJugador)
    {
        Transform parent = animacionAtaque != null ? animacionAtaque.transform.parent : null;
        if (parent != null)
        {
            Transform dañoExistente = parent.Find("DañoTexto");
            if (dañoExistente != null)
            {
                Destroy(dañoExistente.gameObject);
            }
        }
        
        GameObject dañoObj = new GameObject("DañoTexto");
        dañoObj.transform.SetParent(parent, false); // Parent al panel para posicionamiento absoluto
        Text dañoText = dañoObj.AddComponent<Text>();
        dañoText.text = daño.ToString(); // Sin signo negativo
        dañoText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        dañoText.fontSize = 56;
        dañoText.fontStyle = FontStyle.Bold;
        dañoText.color = esJugador ? Color.white : Color.red; // Blanco si es jugador, rojo si es enemigo
        dañoText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform dañoRect = dañoObj.GetComponent<RectTransform>();
        dañoRect.anchorMin = new Vector2(0.5f, 0.9f);
        dañoRect.anchorMax = new Vector2(0.5f, 0.9f);
        dañoRect.anchoredPosition = Vector2.zero;
        dañoRect.sizeDelta = new Vector2(250, 70);
        
    }
    
    private IEnumerator DestruirDañoTexto(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null) Destroy(obj);
    }
    
    public void OcultarAnimacion()
    {
        if (corrutinaAnimacion != null)
        {
            StopCoroutine(corrutinaAnimacion);
            corrutinaAnimacion = null;
        }
        
        if (animacionAtaque != null)
        {
            animacionAtaque.gameObject.SetActive(false);
            animacionAtaque.sprite = null; // Limpiar sprite
            animacionAtaque.color = new Color(1f, 1f, 1f, 0f); // Completamente transparente
            
            Transform parent = animacionAtaque.transform.parent;
            if (parent != null)
            {
                Transform dañoExistente = parent.Find("DañoTexto");
                if (dañoExistente != null)
                {
                    Destroy(dañoExistente.gameObject);
                }
            }
        }
    }
    #endregion
    
    #region Dialogos
    public void MostrarDialogoSeleccionClase(Action<string> callback)
    {
        GameObject dialogAnterior = GameObject.Find("DialogoClase");
        if (dialogAnterior != null)
        {
            Destroy(dialogAnterior);
        }
        
        GameObject dialog = CrearPanel("DialogoClase", new Color(0.1f, 0.1f, 0.15f, 0.95f));
        dialog.transform.SetParent(canvas.transform, false);
        dialog.transform.SetAsLastSibling(); // Asegurar que esté al frente
        
        Text titulo = CrearTexto("TituloClase", "🎭 SELECCIONA TU CLASE", 24, new Color(1f, 0.9f, 0.3f, 1f));
        titulo.rectTransform.anchorMin = new Vector2(0.5f, 0.7f);
        titulo.rectTransform.anchorMax = new Vector2(0.5f, 0.7f);
        titulo.rectTransform.anchoredPosition = Vector2.zero;
        titulo.rectTransform.sizeDelta = new Vector2(400, 40);
        titulo.transform.SetParent(dialog.transform, false);
        
        Button btnGuerrero = CrearBotonConColor("BtnGuerrero", "⚔️ GUERRERO", 20, new Color(0.8f, 0.3f, 0.3f, 1f));
        RectTransform guerreroRect = btnGuerrero.GetComponent<RectTransform>();
        guerreroRect.anchorMin = new Vector2(0.5f, 0.5f);
        guerreroRect.anchorMax = new Vector2(0.5f, 0.5f);
        guerreroRect.anchoredPosition = new Vector2(0, 60);
        guerreroRect.sizeDelta = new Vector2(200, 45);
        guerreroRect.pivot = new Vector2(0.5f, 0.5f);
        btnGuerrero.transform.SetParent(dialog.transform, false);
        btnGuerrero.onClick.AddListener(() => {
            callback("Guerrero");
            Destroy(dialog);
        });
        
        Button btnCazador = CrearBotonConColor("BtnCazador", "🏹 CAZADOR", 20, new Color(0.3f, 0.7f, 0.4f, 1f));
        RectTransform cazadorRect = btnCazador.GetComponent<RectTransform>();
        cazadorRect.anchorMin = new Vector2(0.5f, 0.5f);
        cazadorRect.anchorMax = new Vector2(0.5f, 0.5f);
        cazadorRect.anchoredPosition = new Vector2(0, 0);
        cazadorRect.sizeDelta = new Vector2(200, 45);
        cazadorRect.pivot = new Vector2(0.5f, 0.5f);
        btnCazador.transform.SetParent(dialog.transform, false);
        btnCazador.onClick.AddListener(() => {
            callback("Cazador");
            Destroy(dialog);
        });
        
        Button btnMago = CrearBotonConColor("BtnMago", "🔮 MAGO", 20, new Color(0.5f, 0.3f, 0.8f, 1f));
        RectTransform magoRect = btnMago.GetComponent<RectTransform>();
        magoRect.anchorMin = new Vector2(0.5f, 0.5f);
        magoRect.anchorMax = new Vector2(0.5f, 0.5f);
        magoRect.anchoredPosition = new Vector2(0, -60);
        magoRect.sizeDelta = new Vector2(200, 45);
        magoRect.pivot = new Vector2(0.5f, 0.5f);
        btnMago.transform.SetParent(dialog.transform, false);
        btnMago.onClick.AddListener(() => {
            callback("Mago");
            Destroy(dialog);
        });
    }
    
    public void MostrarDialogoDistribucionSkill(int puntosTotal, int maxVida, int maxArmadura, int maxDaño, Action<int, int, int> callback)
    {
        GameObject dialog = CrearPanel("DialogoSkill", new Color(0.1f, 0.1f, 0.15f, 1f));
        dialog.transform.SetParent(canvas.transform, false);
        dialog.transform.SetAsLastSibling(); // Asegurar que esté al frente
        
        Text titulo = CrearTexto("TituloSkill", $"⭐ DISTRIBUYE {puntosTotal} PUNTOS ⭐", 48, new Color(1f, 0.9f, 0.3f, 1f)); // Tamaño al doble (era 24, ahora 48)
        titulo.rectTransform.anchorMin = new Vector2(0.5f, 0.85f);
        titulo.rectTransform.anchorMax = new Vector2(0.5f, 0.85f);
        titulo.rectTransform.anchoredPosition = Vector2.zero;
        titulo.rectTransform.sizeDelta = new Vector2(800, 80); // Tamaño al doble (era 400x40, ahora 800x80)
        titulo.transform.SetParent(dialog.transform, false);
        
        int[] valores = new int[3];
        valores[0] = 1; // Vida: 1 punto inicial
        valores[1] = 1; // Armadura: 1 punto inicial
        valores[2] = 1; // Daño: 1 punto inicial
        
        int puntosUsados = valores[0] + valores[1] + valores[2];
        int puntosRestantes = puntosTotal - puntosUsados;
        
        Text lblPuntosRestantes = CrearTexto("LblPuntosRestantes", $"Puntos disponibles: {puntosRestantes}", 32, new Color(0.8f, 0.8f, 0.2f, 1f)); // Era 16, ahora 32
        lblPuntosRestantes.rectTransform.anchorMin = new Vector2(0.5f, 0.75f);
        lblPuntosRestantes.rectTransform.anchorMax = new Vector2(0.5f, 0.75f);
        lblPuntosRestantes.rectTransform.anchoredPosition = Vector2.zero;
        lblPuntosRestantes.rectTransform.sizeDelta = new Vector2(600, 50); // Tamaño al doble (era 300x25, ahora 600x50)
        lblPuntosRestantes.transform.SetParent(dialog.transform, false);
        
        InputField[] inputFields = new InputField[3];
        
        Sprite iconoVida = CargarIcono("vida");
        Sprite iconoEscudo = CargarIcono("escudo");
        Sprite iconoDaño = CargarIcono("daño");
        
        inputFields[0] = CrearControlStat(dialog, "Vida", iconoVida, valores, 0, maxVida, 0.65f, puntosTotal, lblPuntosRestantes);
        
        inputFields[1] = CrearControlStat(dialog, "Escudo", iconoEscudo, valores, 1, maxArmadura, 0.5f, puntosTotal, lblPuntosRestantes);
        
        inputFields[2] = CrearControlStat(dialog, "Daño", iconoDaño, valores, 2, maxDaño, 0.35f, puntosTotal, lblPuntosRestantes);
        
        Button btnAleatorio = CrearBotonConColor("BtnAleatorio", "🎲 ALEATORIO", 36, new Color(0.9f, 0.6f, 0.2f, 1f)); // Fuente al doble (era 18, ahora 36)
        btnAleatorio.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.1f); // Bajado (era 0.2f, ahora 0.1f)
        btnAleatorio.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.1f); // Bajado (era 0.2f, ahora 0.1f)
        btnAleatorio.GetComponent<RectTransform>().anchoredPosition = new Vector2(-220, 0); // Ajustado proporcionalmente
        btnAleatorio.GetComponent<RectTransform>().sizeDelta = new Vector2(360, 80); // Tamaño al doble (era 180x40, ahora 360x80)
        btnAleatorio.transform.SetParent(dialog.transform, false);
        btnAleatorio.onClick.AddListener(() => {
            System.Random rand = new System.Random();
            
            int[] maximos = { maxVida, maxArmadura, maxDaño };
            int sumaObjetivo = puntosTotal;
            
            int puntosRestantesParaDistribuir = sumaObjetivo - 3;
            
            int valorExtra1 = rand.Next(0, Mathf.Min(maxVida - 1, puntosRestantesParaDistribuir) + 1);
            int valorExtra2 = rand.Next(0, Mathf.Min(maxArmadura - 1, puntosRestantesParaDistribuir - valorExtra1) + 1);
            int valorExtra3 = rand.Next(0, Mathf.Min(maxDaño - 1, puntosRestantesParaDistribuir - valorExtra1 - valorExtra2) + 1);
            
            valores[0] = 1 + valorExtra1;
            valores[1] = 1 + valorExtra2;
            valores[2] = 1 + valorExtra3;
            
            int sumaActual = valores[0] + valores[1] + valores[2];
            
            int diferencia = sumaObjetivo - sumaActual;
            
            while (diferencia != 0)
            {
                int statElegida = rand.Next(3);
                
                if (diferencia > 0)
                {
                    if (statElegida == 0 && valores[0] < maxVida)
                    {
                        valores[0]++;
                        diferencia--;
                    }
                    else if (statElegida == 1 && valores[1] < maxArmadura)
                    {
                        valores[1]++;
                        diferencia--;
                    }
                    else if (statElegida == 2 && valores[2] < maxDaño)
                    {
                        valores[2]++;
                        diferencia--;
                    }
                }
                else
                {
                    if (statElegida == 0 && valores[0] > 1)
                    {
                        valores[0]--;
                        diferencia++;
                    }
                    else if (statElegida == 1 && valores[1] > 1)
                    {
                        valores[1]--;
                        diferencia++;
                    }
                    else if (statElegida == 2 && valores[2] > 1)
                    {
                        valores[2]--;
                        diferencia++;
                    }
                }
                
                if (Mathf.Abs(diferencia) > 100) break;
            }
            
            valores[0] = Mathf.Clamp(valores[0], 1, maxVida);
            valores[1] = Mathf.Clamp(valores[1], 1, maxArmadura);
            valores[2] = Mathf.Clamp(valores[2], 1, maxDaño);
            
            if (inputFields[0] != null) inputFields[0].text = valores[0].ToString();
            if (inputFields[1] != null) inputFields[1].text = valores[1].ToString();
            if (inputFields[2] != null) inputFields[2].text = valores[2].ToString();
            
            ActualizarPuntosRestantes(lblPuntosRestantes, valores, puntosTotal);
        });
        
        Button btnAceptar = CrearBotonConColor("BtnAceptar", "✅ ACEPTAR", 36, new Color(0.2f, 0.7f, 0.3f, 1f)); // Fuente al doble (era 18, ahora 36)
        btnAceptar.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.1f); // Bajado (era 0.2f, ahora 0.1f)
        btnAceptar.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.1f); // Bajado (era 0.2f, ahora 0.1f)
        btnAceptar.GetComponent<RectTransform>().anchoredPosition = new Vector2(220, 0); // Ajustado proporcionalmente
        btnAceptar.GetComponent<RectTransform>().sizeDelta = new Vector2(360, 80); // Tamaño al doble (era 180x40, ahora 360x80)
        btnAceptar.transform.SetParent(dialog.transform, false);
        btnAceptar.onClick.AddListener(() => {
            callback(valores[0], valores[1], valores[2]);
            Destroy(dialog);
        });
    }
    
    private Sprite CargarIcono(string nombre)
    {
        #if UNITY_EDITOR
        string[] extensiones = { ".png", ".PNG", ".jpg", ".JPG" };
        
        foreach (string ext in extensiones)
        {
            string ruta = $"Assets/Sprites/UI/{nombre}{ext}";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
            if (sprite != null)
            {
                return sprite;
            }
            
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(ruta);
            if (texture != null)
            {
                Rect rect = new Rect(0, 0, texture.width, texture.height);
                return Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 100f);
            }
        }
        
        Debug.LogWarning($"No se encontró el icono {nombre} en Assets/Sprites/UI/");
        return null;
        #else
        Sprite sprite = Resources.Load<Sprite>($"Sprites/UI/{nombre}");
        if (sprite != null) return sprite;
        
        Texture2D texture = Resources.Load<Texture2D>($"Sprites/UI/{nombre}");
        if (texture != null)
        {
            Rect rect = new Rect(0, 0, texture.width, texture.height);
            return Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 100f);
        }
        
        return null;
        #endif
    }
    
    private InputField CrearControlStat(GameObject parent, string nombre, Sprite iconoSprite, int[] valores, int indice, int maximo, float posicionY, int puntosTotal, Text lblPuntosRestantes)
    {
        GameObject contenedorIcono = new GameObject($"Icono{nombre}");
        contenedorIcono.transform.SetParent(parent.transform, false);
        RectTransform iconoRect = contenedorIcono.AddComponent<RectTransform>();
        iconoRect.anchorMin = new Vector2(0.5f, posicionY);
        iconoRect.anchorMax = new Vector2(0.5f, posicionY);
        iconoRect.anchoredPosition = new Vector2(-370, 0); // 1 cm más a la izquierda (aproximadamente 50 píxeles más)
        iconoRect.sizeDelta = new Vector2(80, 80); // Tamaño reducido para evitar superposición (era 100, ahora 80)
        
        if (iconoSprite != null)
        {
            Image iconoImage = contenedorIcono.AddComponent<Image>();
            iconoImage.sprite = iconoSprite;
            iconoImage.preserveAspect = true;
        }
        
        Text lblNombre = CrearTexto($"Lbl{nombre}", $"{nombre} (Máx: {maximo})", 32, Color.white); // Era 16, ahora 32
        lblNombre.rectTransform.anchorMin = new Vector2(0.5f, posicionY);
        lblNombre.rectTransform.anchorMax = new Vector2(0.5f, posicionY);
        lblNombre.rectTransform.anchoredPosition = new Vector2(-100, 0); // Ajustado para que no se superponga con el icono
        lblNombre.rectTransform.sizeDelta = new Vector2(400, 50); // Tamaño aumentado proporcionalmente
        lblNombre.alignment = TextAnchor.MiddleLeft;
        lblNombre.transform.SetParent(parent.transform, false);
        
        GameObject inputObj = new GameObject($"Input{nombre}");
        inputObj.transform.SetParent(parent.transform, false);
        
        RectTransform inputRect = inputObj.AddComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0.5f, posicionY);
        inputRect.anchorMax = new Vector2(0.5f, posicionY);
        inputRect.anchoredPosition = new Vector2(160, 0); // Ajustado para el nuevo tamaño
        inputRect.sizeDelta = new Vector2(160, 60); // Tamaño al doble (era 80x30, ahora 160x60)
        
        InputField inputField = inputObj.AddComponent<InputField>();
        
        Image inputBg = inputObj.AddComponent<Image>();
        inputBg.color = new Color(0.3f, 0.3f, 0.3f, 1f); // Un poco más claro para mejor contraste con el texto
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(inputObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.offsetMin = new Vector2(5, 5); // Menos padding para que el texto tenga más espacio
        textRect.offsetMax = new Vector2(-5, -5);
        
        Text inputText = textObj.AddComponent<Text>();
        inputText.text = valores[indice].ToString();
        inputText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        inputText.fontSize = 36; // Tamaño al doble (era 18, ahora 36)
        inputText.color = new Color(1f, 0.9f, 0.2f, 1f); // Amarillo brillante para mejor visibilidad
        inputText.alignment = TextAnchor.MiddleCenter;
        inputText.fontStyle = FontStyle.Bold; // Negrita para mejor visibilidad
        inputText.horizontalOverflow = HorizontalWrapMode.Overflow; // Permitir que el texto se vea completo
        inputText.verticalOverflow = VerticalWrapMode.Overflow;
        
        inputField.textComponent = inputText;
        inputField.contentType = InputField.ContentType.IntegerNumber;
        inputField.inputType = InputField.InputType.Standard;
        
        inputField.onEndEdit.AddListener((string texto) => {
            int nuevoValor;
            if (int.TryParse(texto, out nuevoValor))
            {
                // Calcular suma de otros valores (excluyendo el actual)
                int sumOtros = valores[0] + valores[1] + valores[2] - valores[indice];
                
                // Calcular máximo permitido por el total de puntos
                int maxPermitidoPorTotal = puntosTotal - sumOtros;
                
                // El máximo final es el menor entre el máximo del stat y el máximo permitido por el total
                int maxFinal = Mathf.Min(maximo, maxPermitidoPorTotal);
                
                // Clamp entre 1 y maxFinal (mínimo 1 punto)
                nuevoValor = Mathf.Clamp(nuevoValor, 1, maxFinal);
                
                valores[indice] = nuevoValor;
                inputField.text = nuevoValor.ToString();
                ActualizarPuntosRestantes(lblPuntosRestantes, valores, puntosTotal);
            }
            else
            {
                inputField.text = valores[indice].ToString();
            }
        });
        
        inputField.onValueChanged.AddListener((string texto) => {
            int nuevoValor;
            if (int.TryParse(texto, out nuevoValor))
            {
                int sumOtros = valores[0] + valores[1] + valores[2] - valores[indice];
                int maxPermitidoPorTotal = puntosTotal - sumOtros;
                int maxFinal = Mathf.Min(maximo, maxPermitidoPorTotal);
                
                if (nuevoValor >= 1 && nuevoValor <= maxFinal)
                {
                    valores[indice] = nuevoValor;
                    ActualizarPuntosRestantes(lblPuntosRestantes, valores, puntosTotal);
                }
            }
        });
        
        // Agregar botones + y - SOLO para Vida
        if (nombre == "Vida")
        {
            // Botón "-" a la izquierda del input
            Button btnMenos = CrearBotonConColor("BtnVidaMenos", "-", 44, new Color(0.7f, 0.3f, 0.3f, 1f));
            RectTransform menosRect = btnMenos.GetComponent<RectTransform>();
            menosRect.anchorMin = new Vector2(0.5f, posicionY);
            menosRect.anchorMax = new Vector2(0.5f, posicionY);
            menosRect.anchoredPosition = new Vector2(60, 0);
            menosRect.sizeDelta = new Vector2(70, 60);
            btnMenos.transform.SetParent(parent.transform, false);
            btnMenos.onClick.AddListener(() => {
                if (valores[indice] > 0)
                {
                    valores[indice]--;
                    inputField.text = valores[indice].ToString();
                    ActualizarPuntosRestantes(lblPuntosRestantes, valores, puntosTotal);
                }
            });
            
            // Botón "+" a la derecha del input
            Button btnMas = CrearBotonConColor("BtnVidaMas", "+", 44, new Color(0.3f, 0.7f, 0.3f, 1f));
            RectTransform masRect = btnMas.GetComponent<RectTransform>();
            masRect.anchorMin = new Vector2(0.5f, posicionY);
            masRect.anchorMax = new Vector2(0.5f, posicionY);
            masRect.anchoredPosition = new Vector2(260, 0);
            masRect.sizeDelta = new Vector2(70, 60);
            btnMas.transform.SetParent(parent.transform, false);
            btnMas.onClick.AddListener(() => {
            int sumaActual = valores[0] + valores[1] + valores[2];
                int puntosDisponibles = puntosTotal - sumaActual;
                if (valores[indice] < maximo && puntosDisponibles > 0)
                {
                    valores[indice]++;
                    inputField.text = valores[indice].ToString();
                    ActualizarPuntosRestantes(lblPuntosRestantes, valores, puntosTotal);
                }
            });
        }
        
        // Agregar botones + y - SOLO para Escudo
        if (nombre == "Escudo")
        {
            // Botón "-" a la izquierda del input
            Button btnMenos = CrearBotonConColor("BtnEscudoMenos", "-", 44, new Color(0.7f, 0.3f, 0.3f, 1f));
            RectTransform menosRect = btnMenos.GetComponent<RectTransform>();
            menosRect.anchorMin = new Vector2(0.5f, posicionY);
            menosRect.anchorMax = new Vector2(0.5f, posicionY);
            menosRect.anchoredPosition = new Vector2(60, 0);
            menosRect.sizeDelta = new Vector2(70, 60);
            btnMenos.transform.SetParent(parent.transform, false);
            btnMenos.onClick.AddListener(() => {
                if (valores[indice] > 0)
                {
                    valores[indice]--;
                    inputField.text = valores[indice].ToString();
                    ActualizarPuntosRestantes(lblPuntosRestantes, valores, puntosTotal);
                }
            });
            
            // Botón "+" a la derecha del input
            Button btnMas = CrearBotonConColor("BtnEscudoMas", "+", 44, new Color(0.3f, 0.7f, 0.3f, 1f));
            RectTransform masRect = btnMas.GetComponent<RectTransform>();
            masRect.anchorMin = new Vector2(0.5f, posicionY);
            masRect.anchorMax = new Vector2(0.5f, posicionY);
            masRect.anchoredPosition = new Vector2(260, 0);
            masRect.sizeDelta = new Vector2(70, 60);
            btnMas.transform.SetParent(parent.transform, false);
            btnMas.onClick.AddListener(() => {
          int sumaActual = valores[0] + valores[1] + valores[2];
                int puntosDisponibles = puntosTotal - sumaActual;
                if (valores[indice] < maximo && puntosDisponibles > 0)
                {
                    valores[indice]++;
                    inputField.text = valores[indice].ToString();
                    ActualizarPuntosRestantes(lblPuntosRestantes, valores, puntosTotal);
                }
            });
        }
        if (nombre == "Daño")
        {
            // Botón "-" a la izquierda del input
            Button btnMenos = CrearBotonConColor("BtnDañoMenos", "-", 44, new Color(0.7f, 0.3f, 0.3f, 1f));
            RectTransform menosRect = btnMenos.GetComponent<RectTransform>();
            menosRect.anchorMin = new Vector2(0.5f, posicionY);
            menosRect.anchorMax = new Vector2(0.5f, posicionY);
            menosRect.anchoredPosition = new Vector2(60, 0);
            menosRect.sizeDelta = new Vector2(70, 60);
            btnMenos.transform.SetParent(parent.transform, false);
            btnMenos.onClick.AddListener(() => {
                if (valores[indice] > 0)
                {
                    valores[indice]--;
                    inputField.text = valores[indice].ToString();
                    ActualizarPuntosRestantes(lblPuntosRestantes, valores, puntosTotal);
                }
            });
            
            // Botón "+" a la derecha del input
            Button btnMas = CrearBotonConColor("BtnDañoMas", "+", 44, new Color(0.3f, 0.7f, 0.3f, 1f));
            RectTransform masRect = btnMas.GetComponent<RectTransform>();
            masRect.anchorMin = new Vector2(0.5f, posicionY);
            masRect.anchorMax = new Vector2(0.5f, posicionY);
            masRect.anchoredPosition = new Vector2(260, 0);
            masRect.sizeDelta = new Vector2(70, 60);
            btnMas.transform.SetParent(parent.transform, false);
            btnMas.onClick.AddListener(() => {
          int sumaActual = valores[0] + valores[1] + valores[2];
                int puntosDisponibles = puntosTotal - sumaActual;
                if (valores[indice] < maximo && puntosDisponibles > 0)
                {
                    valores[indice]++;
                    inputField.text = valores[indice].ToString();
                    ActualizarPuntosRestantes(lblPuntosRestantes, valores, puntosTotal);
                }
            });
        }
        
        return inputField;
    }
    
    private void ActualizarPuntosRestantes(Text lbl, int[] valores, int puntosTotal)
    {
        if (lbl != null)
        {
            int puntosUsados = valores[0] + valores[1] + valores[2];
            int puntosRestantes = puntosTotal - puntosUsados;
            lbl.text = $"Puntos disponibles: {puntosRestantes}";
        }
    }
    
    public void MostrarOpcionesDespuesBatalla(int oroGanado, bool puedeRevancha, Action<string> callback)
    {
        MostrarOpcionesDespuesBatalla(oroGanado, 0, puedeRevancha, callback);
    }
    
    public void MostrarOpcionesDespuesBatalla(int oroBase, int expBase, Action<string> callback)
    {
        MostrarOpcionesDespuesBatalla(oroBase, expBase, true, callback);
    }
    
    public void MostrarOpcionesDespuesBatalla(int oroBase, int expBase, bool puedeRevancha, Action<string> callback)
    {
        GameObject dialog = CrearPanel("DialogoOpciones", new Color(0.1f, 0.1f, 0.15f, 0.95f));
        dialog.transform.SetParent(canvas.transform, false);
        
        Text titulo = CrearTexto("TituloOpciones", "🎉 ¡VICTORIA! 🎉", 24, new Color(1f, 0.9f, 0.2f, 1f));
        titulo.rectTransform.anchorMin = new Vector2(0.5f, 0.85f);
        titulo.rectTransform.anchorMax = new Vector2(0.5f, 0.85f);
        titulo.rectTransform.anchoredPosition = Vector2.zero;
        titulo.rectTransform.sizeDelta = new Vector2(400, 40);
        titulo.transform.SetParent(dialog.transform, false);
        
        Button btnMatar = CrearBotonConColor("BtnMatar", "MATAR - Oro x1 / Exp x1", 18, new Color(0.7f, 0.2f, 0.2f, 1f));
        btnMatar.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.6f);
        btnMatar.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.6f);
        btnMatar.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        btnMatar.GetComponent<RectTransform>().sizeDelta = new Vector2(320, 45);
        btnMatar.transform.SetParent(dialog.transform, false);
        btnMatar.onClick.AddListener(() => {
            StartCoroutine(EjecutarAccionConFeedback(btnMatar, () => callback("Matar"), dialog));
        });
        
        Button btnRevancha = null;
        if (puedeRevancha)
        {
            btnRevancha = CrearBotonConColor("BtnRevancha", "REVANCHA (sin escudo) - Oro x0 / Exp x2", 18, new Color(0.9f, 0.6f, 0.2f, 1f));
            btnRevancha.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
            btnRevancha.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
            btnRevancha.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            btnRevancha.GetComponent<RectTransform>().sizeDelta = new Vector2(360, 45);
            btnRevancha.transform.SetParent(dialog.transform, false);
            btnRevancha.onClick.AddListener(() => {
                StartCoroutine(EjecutarAccionConFeedback(btnRevancha, () => callback("Revancha"), dialog));
            });
        }
        
        Button btnClemencia = CrearBotonConColor("BtnClemencia", "CLEMENCIA - Oro x2 / Exp x0", 18, new Color(0.2f, 0.7f, 0.3f, 1f));
        btnClemencia.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.4f);
        btnClemencia.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.4f);
        btnClemencia.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        btnClemencia.GetComponent<RectTransform>().sizeDelta = new Vector2(320, 45);
        btnClemencia.transform.SetParent(dialog.transform, false);
        btnClemencia.onClick.AddListener(() => {
            StartCoroutine(EjecutarAccionConFeedback(btnClemencia, () => callback("Clemencia"), dialog));
        });
        
        Text infoMatar = CrearTexto("InfoMatar", "Mata al enemigo.\nOro x1 y Exp x1", 14, Color.white);
        infoMatar.rectTransform.anchorMin = new Vector2(0.85f, 0.6f);
        infoMatar.rectTransform.anchorMax = new Vector2(0.85f, 0.6f);
        infoMatar.rectTransform.anchoredPosition = Vector2.zero;
        infoMatar.rectTransform.sizeDelta = new Vector2(240, 40);
        infoMatar.transform.SetParent(dialog.transform, false);
        
        if (btnRevancha != null)
        {
            Text infoRevancha = CrearTexto("InfoRevancha", "El enemigo se cura.\nOro x0 y Exp x2", 14, Color.white);
            infoRevancha.rectTransform.anchorMin = new Vector2(0.85f, 0.5f);
            infoRevancha.rectTransform.anchorMax = new Vector2(0.85f, 0.5f);
            infoRevancha.rectTransform.anchoredPosition = Vector2.zero;
            infoRevancha.rectTransform.sizeDelta = new Vector2(240, 40);
            infoRevancha.transform.SetParent(dialog.transform, false);
        }
        
        Text infoClemencia = CrearTexto("InfoClemencia", "Se perdona la vida del enemigo.\nOro x2 y Exp x0", 14, Color.white);
        infoClemencia.rectTransform.anchorMin = new Vector2(0.85f, 0.4f);
        infoClemencia.rectTransform.anchorMax = new Vector2(0.85f, 0.4f);
        infoClemencia.rectTransform.anchoredPosition = Vector2.zero;
        infoClemencia.rectTransform.sizeDelta = new Vector2(240, 40);
        infoClemencia.transform.SetParent(dialog.transform, false);
    }
    
    public void MostrarGameOver(Action callback)
    {
        GameObject dialog = CrearPanel("DialogoGameOver", new Color(0.2f, 0.1f, 0.1f, 0.95f));
        dialog.transform.SetParent(canvas.transform, false);
        
        Text titulo = CrearTexto("TituloGameOver", "💀 GAME OVER 💀", 48, new Color(1f, 0.2f, 0.2f, 1f));
        titulo.rectTransform.anchorMin = new Vector2(0.5f, 0.6f);
        titulo.rectTransform.anchorMax = new Vector2(0.5f, 0.6f);
        titulo.rectTransform.anchoredPosition = Vector2.zero;
        titulo.rectTransform.sizeDelta = new Vector2(400, 60);
        titulo.transform.SetParent(dialog.transform, false);
        
        Button btnMenu = CrearBotonConColor("BtnMenuGameOver", "🏠 VOLVER AL MENÚ", 20, new Color(0.5f, 0.5f, 0.5f, 1f));
        btnMenu.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.4f);
        btnMenu.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.4f);
        btnMenu.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        btnMenu.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 45);
        btnMenu.transform.SetParent(dialog.transform, false);
        btnMenu.onClick.AddListener(() => {
            callback();
            Destroy(dialog);
        });
    }
    
    public void MostrarMensajeVictoriaFinal()
    {
        GameObject dialog = CrearPanel("DialogoVictoriaFinal", new Color(0.1f, 0.2f, 0.1f, 0.95f));
        dialog.transform.SetParent(canvas.transform, false);
        
        Text titulo = CrearTexto("TituloVictoriaFinal", "🏆 ¡FELICITACIONES! 🏆\n🎊 ¡HAS GANADO! 🎊", 36, new Color(0.2f, 0.9f, 0.3f, 1f));
        titulo.rectTransform.anchorMin = new Vector2(0.5f, 0.6f);
        titulo.rectTransform.anchorMax = new Vector2(0.5f, 0.6f);
        titulo.rectTransform.anchoredPosition = Vector2.zero;
        titulo.rectTransform.sizeDelta = new Vector2(500, 100);
        titulo.transform.SetParent(dialog.transform, false);
        
        Button btnMenu = CrearBotonConColor("BtnMenuVictoria", "🏠 VOLVER AL MENÚ", 20, new Color(0.2f, 0.7f, 0.3f, 1f));
        btnMenu.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.4f);
        btnMenu.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.4f);
        btnMenu.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        btnMenu.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 45);
        btnMenu.transform.SetParent(dialog.transform, false);
        btnMenu.onClick.AddListener(() => {
            gameManager.ResetearJuego();
            MostrarPanelMenu();
            Destroy(dialog);
        });
    }
    
    public void MostrarMensaje(string mensaje)
    {
        GameObject dialog = CrearPanel("DialogoMensaje", new Color(0.1f, 0.1f, 0.15f, 0.95f));
        dialog.transform.SetParent(canvas.transform, false);
        
        Text texto = CrearTexto("TextoMensaje", mensaje, 18, Color.white);
        texto.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        texto.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        texto.rectTransform.anchoredPosition = Vector2.zero;
        texto.rectTransform.sizeDelta = new Vector2(400, 100);
        texto.transform.SetParent(dialog.transform, false);
        
        Button btnOk = CrearBotonConColor("BtnOkMensaje", "✅ OK", 18, new Color(0.2f, 0.6f, 0.8f, 1f));
        btnOk.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.3f);
        btnOk.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.3f);
        btnOk.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        btnOk.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 40);
        btnOk.transform.SetParent(dialog.transform, false);
        btnOk.onClick.AddListener(() => Destroy(dialog));
    }
    
    public void MostrarMensajeConfirmacion(string mensaje, Action onAceptar, Action onCancelar)
    {
        GameObject dialog = CrearPanel("DialogoConfirmacion", new Color(0.1f, 0.1f, 0.15f, 0.95f));
        dialog.transform.SetParent(canvas.transform, false);
        
        Text texto = CrearTexto("TextoConfirmacion", mensaje, 18, Color.white);
        texto.rectTransform.anchorMin = new Vector2(0.5f, 0.6f);
        texto.rectTransform.anchorMax = new Vector2(0.5f, 0.6f);
        texto.rectTransform.anchoredPosition = Vector2.zero;
        texto.rectTransform.sizeDelta = new Vector2(520, 120);
        texto.transform.SetParent(dialog.transform, false);
        
        Button btnAceptar = CrearBotonConColor("BtnAceptar", "ACEPTAR", 18, new Color(0.2f, 0.7f, 0.3f, 1f));
        RectTransform aceptarRect = btnAceptar.GetComponent<RectTransform>();
        aceptarRect.anchorMin = new Vector2(0.35f, 0.35f);
        aceptarRect.anchorMax = new Vector2(0.35f, 0.35f);
        aceptarRect.anchoredPosition = Vector2.zero;
        aceptarRect.sizeDelta = new Vector2(160, 40);
        btnAceptar.transform.SetParent(dialog.transform, false);
        btnAceptar.onClick.AddListener(() => {
            Destroy(dialog);
            onAceptar?.Invoke();
        });
        
        Button btnCancelar = CrearBotonConColor("BtnCancelar", "CANCELAR", 18, new Color(0.8f, 0.2f, 0.2f, 1f));
        RectTransform cancelarRect = btnCancelar.GetComponent<RectTransform>();
        cancelarRect.anchorMin = new Vector2(0.65f, 0.35f);
        cancelarRect.anchorMax = new Vector2(0.65f, 0.35f);
        cancelarRect.anchoredPosition = Vector2.zero;
        cancelarRect.sizeDelta = new Vector2(160, 40);
        btnCancelar.transform.SetParent(dialog.transform, false);
        btnCancelar.onClick.AddListener(() => {
            Destroy(dialog);
            onCancelar?.Invoke();
        });
    }
    #endregion
}


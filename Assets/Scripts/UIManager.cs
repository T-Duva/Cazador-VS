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
    private Image imgJugador;
    private Image imgEnemigo;
    private Image animacionAtaque;
    private Coroutine corrutinaAnimacion;
    private Coroutine corrutinaIdleJugador;
    private Coroutine corrutinaIdleEnemigo;
    private Button btnAtacar;
    private Button btnHuir;
    private Button btnAutomatico;
    private Button btnManual;
    private Button btnDetener;
    private Text lblContadorAtaque;
    private Text lblInfoJugador;
    private Text lblInfoEnemigo;
    private GameObject panelStatsJugador;
    private GameObject panelStatsEnemigo;
    private Text lblNombreEnemigo;
    private Dictionary<string, GameObject> tarjetasJugador = new Dictionary<string, GameObject>();
    private GameObject tarjetaDañoEnemigo;
    private GameObject spacerVidaEscudo;
    
    [Header("Ajustes de Posición HUD")]
    [SerializeField] private RectTransform panelStatsJugadorRectSerialized;
    [SerializeField] private RectTransform panelHudEnemigo;
    [SerializeField] private Canvas hudCanvas;
    [SerializeField] private float subirHudJugadorCm = 0f;
    [SerializeField] private float fallbackDpi = 96f;
    [SerializeField] private float playerBottomMargin = 0f;
    [SerializeField] private float escudoOffsetX = 30f;
    private RectTransform panelStatsJugadorRect;
    private bool _offsetAplicado = false;
    #endregion
    
    #region UI - Descanso
    private Button btnDescansoBatalla;
    private Button btnDescansoIrDescanso;
    private Button btnDescansoAbandonar;
    #endregion
    
    #region UI - Tienda
    private Button btnTiendaPocion;
    private Button btnTiendaArmadura;
    private Button btnTiendaVolver;
    private Text lblOroTienda;
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
    }
    #endregion
    
    #region Paneles - Creacion
    private void CrearPaneles()
    {
        panelMenu = CrearPanel("PanelMenu", new Color(0.1f, 0.1f, 0.15f, 1f));
        CrearPanelMenu();
        
        panelJuego = CrearPanel("PanelJuego", new Color(0.15f, 0.15f, 0.2f, 1f));
        CrearPanelJuego();
        
        panelDescanso = CrearPanel("PanelDescanso", new Color(0.1f, 0.15f, 0.1f, 1f));
        CrearPanelDescanso();
        
        panelTienda = CrearPanel("PanelTienda", new Color(0.1f, 0.1f, 0.15f, 1f));
        CrearPanelTienda();
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
        
        CrearMarcoRojo(animObj, 10f, 300, 300);
        
        Transform marcoRojo = animObj.transform.Find("Marco");
        if (marcoRojo != null)
        {
            marcoRojo.gameObject.SetActive(false);
        }
        
        lblContadorAtaque = CrearTexto("LblContadorAtaque", "", 36, Color.yellow);
        lblContadorAtaque.rectTransform.anchorMin = new Vector2(0.5f, 0.2f);
        lblContadorAtaque.rectTransform.anchorMax = new Vector2(0.5f, 0.2f);
        lblContadorAtaque.rectTransform.anchoredPosition = Vector2.zero;
        lblContadorAtaque.rectTransform.sizeDelta = new Vector2(100, 50);
        lblContadorAtaque.transform.SetParent(panelJuego.transform, false);
        lblContadorAtaque.gameObject.SetActive(false);
        
        btnAtacar = CrearBotonConColor("BtnAtaque", "⚔️ ATAQUE", 22, new Color(0.2f, 0.7f, 0.3f, 1f));
        RectTransform ataqueRect = btnAtacar.GetComponent<RectTransform>();
        ataqueRect.anchorMin = new Vector2(0.5f, 0.15f);
        ataqueRect.anchorMax = new Vector2(0.5f, 0.15f);
        ataqueRect.anchoredPosition = new Vector2(0, 0);
        ataqueRect.sizeDelta = new Vector2(250, 45);
        btnAtacar.transform.SetParent(panelJuego.transform, false);
        btnAtacar.gameObject.SetActive(true);
        btnAtacar.onClick.AddListener(() => gameManager.BtnAtaque_Click());
        
        btnAutomatico = CrearBotonConColor("BtnAutomatico", "🔄 AUTOMÁTICO", 20, new Color(0.2f, 0.6f, 0.8f, 1f));
        RectTransform autoRect = btnAutomatico.GetComponent<RectTransform>();
        autoRect.anchorMin = new Vector2(0.5f, 0.15f);
        autoRect.anchorMax = new Vector2(0.5f, 0.15f);
        autoRect.anchoredPosition = new Vector2(-130, 0);
        autoRect.sizeDelta = new Vector2(220, 40);
        btnAutomatico.transform.SetParent(panelJuego.transform, false);
        btnAutomatico.gameObject.SetActive(false);
        btnAutomatico.onClick.AddListener(() => gameManager.BtnAutomatico_Click());
        
        btnManual = CrearBotonConColor("BtnManual", "⚔️ MANUAL", 20, new Color(0.8f, 0.2f, 0.2f, 1f));
        RectTransform manualRect = btnManual.GetComponent<RectTransform>();
        manualRect.anchorMin = new Vector2(0.5f, 0.15f);
        manualRect.anchorMax = new Vector2(0.5f, 0.15f);
        manualRect.anchoredPosition = new Vector2(130, 0);
        manualRect.sizeDelta = new Vector2(220, 40);
        btnManual.transform.SetParent(panelJuego.transform, false);
        btnManual.gameObject.SetActive(false);
        btnManual.onClick.AddListener(() => gameManager.BtnManual_Click());
        
        btnDetener = btnAutomatico;
        
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
        
        HorizontalLayoutGroup layoutJugador = panelStatsJugador.AddComponent<HorizontalLayoutGroup>();
        layoutJugador.spacing = 0f;
        layoutJugador.padding = new RectOffset(0, 0, 0, 0);
        layoutJugador.childAlignment = TextAnchor.LowerLeft;
        layoutJugador.childControlWidth = true;
        layoutJugador.childControlHeight = false;
        layoutJugador.childForceExpandWidth = false;
        layoutJugador.childForceExpandHeight = false;
        
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
    }
    
    private void CrearPanelDescanso()
    {
        Text titulo = CrearTexto("TituloDescanso", "💤 DESCANSO", 36, new Color(0.5f, 0.8f, 1f, 1f));
        titulo.rectTransform.anchorMin = new Vector2(0.5f, 0.7f);
        titulo.rectTransform.anchorMax = new Vector2(0.5f, 0.7f);
        titulo.rectTransform.anchoredPosition = Vector2.zero;
        titulo.rectTransform.sizeDelta = new Vector2(300, 50);
        titulo.transform.SetParent(panelDescanso.transform, false);
        
        btnDescansoBatalla = CrearBotonConColor("BtnDescansoBatalla", "⚔️ PRÓXIMA BATALLA", 20, new Color(0.2f, 0.7f, 0.3f, 1f));
        btnDescansoBatalla.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
        btnDescansoBatalla.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        btnDescansoBatalla.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 60);
        btnDescansoBatalla.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 45);
        btnDescansoBatalla.transform.SetParent(panelDescanso.transform, false);
        btnDescansoBatalla.onClick.AddListener(() => gameManager.BtnDescansoBatalla_Click());
        
        btnDescansoIrDescanso = CrearBotonConColor("BtnDescansoIrDescanso", "🛏️ IR A DESCANSO", 20, new Color(0.3f, 0.6f, 0.8f, 1f));
        btnDescansoIrDescanso.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
        btnDescansoIrDescanso.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        btnDescansoIrDescanso.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        btnDescansoIrDescanso.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 45);
        btnDescansoIrDescanso.transform.SetParent(panelDescanso.transform, false);
        btnDescansoIrDescanso.onClick.AddListener(() => gameManager.BtnDescansoIrDescanso_Click());
        
        btnDescansoAbandonar = CrearBotonConColor("BtnDescansoAbandonar", "🚪 ABANDONAR", 20, new Color(0.7f, 0.2f, 0.2f, 1f));
        btnDescansoAbandonar.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
        btnDescansoAbandonar.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        btnDescansoAbandonar.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -60);
        btnDescansoAbandonar.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 45);
        btnDescansoAbandonar.transform.SetParent(panelDescanso.transform, false);
        btnDescansoAbandonar.onClick.AddListener(() => {
            gameManager.ResetearJuego();
            MostrarPanelMenu();
        });
    }
    
    private void CrearPanelTienda()
    {
        Text titulo = CrearTexto("TituloTienda", "🏪 TIENDA", 36, new Color(1f, 0.8f, 0.2f, 1f));
        titulo.rectTransform.anchorMin = new Vector2(0.5f, 0.85f);
        titulo.rectTransform.anchorMax = new Vector2(0.5f, 0.85f);
        titulo.rectTransform.anchoredPosition = Vector2.zero;
        titulo.rectTransform.sizeDelta = new Vector2(200, 50);
        titulo.transform.SetParent(panelTienda.transform, false);
        
        lblOroTienda = CrearTexto("LblOroTienda", "💰 Oro: 0", 24, Color.yellow);
        lblOroTienda.rectTransform.anchorMin = new Vector2(0.5f, 0.75f);
        lblOroTienda.rectTransform.anchorMax = new Vector2(0.5f, 0.75f);
        lblOroTienda.rectTransform.anchoredPosition = Vector2.zero;
        lblOroTienda.rectTransform.sizeDelta = new Vector2(200, 40);
        lblOroTienda.transform.SetParent(panelTienda.transform, false);
        
        btnTiendaPocion = CrearBotonConColor("BtnTiendaPocion", "🧪 POCIÓN (+50 ❤️) - 💰50", 18, new Color(0.8f, 0.3f, 0.3f, 1f));
        btnTiendaPocion.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.6f);
        btnTiendaPocion.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.6f);
        btnTiendaPocion.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        btnTiendaPocion.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 45);
        btnTiendaPocion.transform.SetParent(panelTienda.transform, false);
        btnTiendaPocion.onClick.AddListener(() => gameManager.BtnTiendaPocion_Click());
        
        btnTiendaArmadura = CrearBotonConColor("BtnTiendaArmadura", "🛡️ ARMADURA (Restaurar Escudo) - 💰75", 18, new Color(0.3f, 0.5f, 0.8f, 1f));
        btnTiendaArmadura.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
        btnTiendaArmadura.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        btnTiendaArmadura.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        btnTiendaArmadura.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 45);
        btnTiendaArmadura.transform.SetParent(panelTienda.transform, false);
        btnTiendaArmadura.onClick.AddListener(() => gameManager.BtnTiendaArmadura_Click());
        
        btnTiendaVolver = CrearBotonConColor("BtnTiendaVolver", "⚔️ VOLVER A BATALLA", 20, new Color(0.2f, 0.7f, 0.3f, 1f));
        btnTiendaVolver.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.3f);
        btnTiendaVolver.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.3f);
        btnTiendaVolver.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        btnTiendaVolver.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 45);
        btnTiendaVolver.transform.SetParent(panelTienda.transform, false);
        btnTiendaVolver.onClick.AddListener(() => {
            gameManager.BtnDescansoBatalla_Click();
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
    
    #region Helpers - Marcos y decoracion
    private void CrearMarcoEnemigoSegunTipo(GameObject parentObj, string tipoEnemigo, float ancho, float alto)
    {
        Transform marcoAnterior = parentObj.transform.Find("Marco");
        if (marcoAnterior != null)
        {
            #if UNITY_EDITOR
            DestroyImmediate(marcoAnterior.gameObject);
            #else
            Destroy(marcoAnterior.gameObject);
            #endif
        }
        
        Color colorEnemigo;
        if (tipoEnemigo == "Cazador")
        {
            colorEnemigo = new Color(0.2f, 0.8f, 0.4f, 1f); // Verde
        }
        else if (tipoEnemigo == "Guerrero")
        {
            colorEnemigo = new Color(0.8f, 0.2f, 0.2f, 1f); // Rojo
        }
        else if (tipoEnemigo == "Mago")
        {
            colorEnemigo = new Color(0.2f, 0.2f, 0.8f, 1f); // Azul
        }
        else
        {
            colorEnemigo = new Color(0.6f, 0.6f, 0.6f, 1f); // Gris
        }
        
        int tipoMarco = UnityEngine.Random.Range(0, 6); // 6 tipos diferentes
        
        GameObject marcoObj = new GameObject("Marco");
        marcoObj.transform.SetParent(parentObj.transform, false);
        
        marcoObj.transform.SetAsFirstSibling();
        
        marcoObj.SetActive(true);
        
        switch (tipoMarco)
        {
            case 0: // Marco estilo "gótico" con puntas
                CrearMarcoGotico(marcoObj, colorEnemigo, 8f, ancho, alto);
                break;
            case 1: // Marco estilo "energía" con rayos
                CrearMarcoEnergia(marcoObj, colorEnemigo, 7f, ancho, alto);
                break;
            case 2: // Marco estilo "piedra preciosa" con múltiples capas
                CrearMarcoPiedraPreciosa(marcoObj, colorEnemigo, 9f, ancho, alto);
                break;
            case 3: // Marco estilo "espada" con líneas afiladas
                CrearMarcoEspada(marcoObj, colorEnemigo, 8f, ancho, alto);
                break;
            case 4: // Marco estilo "cristal" con brillo
                CrearMarcoCristal(marcoObj, colorEnemigo, 7f, ancho, alto);
                break;
            case 5: // Marco estilo "fuego" con efectos
                CrearMarcoFuego(marcoObj, colorEnemigo, 8f, ancho, alto);
                break;
        }
    }
    
    private void CrearMarcoGotico(GameObject parent, Color color, float grosor, float ancho, float alto)
    {
        CrearBordeRectangulo(parent, "Borde", color, grosor, ancho, alto);
        CrearPuntaGotic(parent, "Punta1", color, grosor * 2.5f, -ancho/2, alto/2, 0f);
        CrearPuntaGotic(parent, "Punta2", color, grosor * 2.5f, ancho/2, alto/2, 90f);
        CrearPuntaGotic(parent, "Punta3", color, grosor * 2.5f, -ancho/2, -alto/2, -90f);
        CrearPuntaGotic(parent, "Punta4", color, grosor * 2.5f, ancho/2, -alto/2, 180f);
        CrearDecoracionesGotico(parent, color, grosor, ancho, alto);
    }
    
    private void CrearMarcoEnergia(GameObject parent, Color color, float grosor, float ancho, float alto)
    {
        Color energiaBrillante = new Color(color.r, color.g, color.b, 1f);
        CrearBordeRectangulo(parent, "BordePrincipal", energiaBrillante, grosor, ancho, alto);
        for (int i = 0; i < 4; i++)
        {
            float angle = i * 90f;
            float x = (i % 2 == 0 ? -1 : 1) * ancho / 2;
            float y = (i < 2 ? 1 : -1) * alto / 2;
            CrearRayoEnergia(parent, "Rayo" + i, color, grosor * 1.5f, x, y, angle);
        }
        CrearLineasEnergia(parent, color, grosor, ancho, alto);
    }
    
    private void CrearMarcoPiedraPreciosa(GameObject parent, Color color, float grosor, float ancho, float alto)
    {
        CrearBordeRectangulo(parent, "Capa1", new Color(color.r * 0.4f, color.g * 0.4f, color.b * 0.4f, 1f), grosor + 1f, ancho + 2f, alto + 2f);
        CrearBordeRectangulo(parent, "Capa2", new Color(color.r * 0.7f, color.g * 0.7f, color.b * 0.7f, 1f), grosor, ancho, alto);
        CrearBordeRectangulo(parent, "Capa3", new Color(Mathf.Min(1f, color.r * 1.3f), Mathf.Min(1f, color.g * 1.3f), Mathf.Min(1f, color.b * 1.3f), 1f), grosor * 0.5f, ancho - grosor * 1.5f, alto - grosor * 1.5f);
        CrearGema(parent, "Gema1", color, grosor * 2f, -ancho/2, alto/2);
        CrearGema(parent, "Gema2", color, grosor * 2f, ancho/2, alto/2);
        CrearGema(parent, "Gema3", color, grosor * 2f, -ancho/2, -alto/2);
        CrearGema(parent, "Gema4", color, grosor * 2f, ancho/2, -alto/2);
    }
    
    private void CrearMarcoEspada(GameObject parent, Color color, float grosor, float ancho, float alto)
    {
        CrearBordeRectangulo(parent, "Borde", color, grosor, ancho, alto);
        CrearEspadaDecorativa(parent, "Espada1", color, grosor * 2f, -ancho/2, alto/2, 45f);
        CrearEspadaDecorativa(parent, "Espada2", color, grosor * 2f, ancho/2, alto/2, 135f);
        CrearEspadaDecorativa(parent, "Espada3", color, grosor * 2f, -ancho/2, -alto/2, -45f);
        CrearEspadaDecorativa(parent, "Espada4", color, grosor * 2f, ancho/2, -alto/2, -135f);
        CrearLineasCorte(parent, color, grosor * 0.5f, ancho, alto);
    }
    
    private void CrearMarcoCristal(GameObject parent, Color color, float grosor, float ancho, float alto)
    {
        Color cristal = new Color(color.r, color.g, color.b, 0.7f);
        CrearBordeRectangulo(parent, "BordeCristal", cristal, grosor, ancho, alto);
        Color brillo = new Color(1f, 1f, 1f, 0.8f);
        CrearBrillo(parent, "Brillo1", brillo, grosor * 1.5f, -ancho/2, alto/2);
        CrearBrillo(parent, "Brillo2", brillo, grosor * 1.5f, ancho/2, alto/2);
        CrearBrillo(parent, "Brillo3", brillo, grosor * 1.5f, -ancho/2, -alto/2);
        CrearBrillo(parent, "Brillo4", brillo, grosor * 1.5f, ancho/2, -alto/2);
        CrearReflejos(parent, color, grosor, ancho, alto);
    }
    
    private void CrearMarcoFuego(GameObject parent, Color color, float grosor, float ancho, float alto)
    {
        Color fuegoBase = new Color(color.r, color.g * 0.5f, 0f, 1f);
        CrearBordeRectangulo(parent, "BordeFuego", fuegoBase, grosor, ancho, alto);
        CrearLlama(parent, "Llama1", color, grosor * 2f, -ancho/2, alto/2);
        CrearLlama(parent, "Llama2", color, grosor * 2f, ancho/2, alto/2);
        CrearLlama(parent, "Llama3", color, grosor * 2f, -ancho/2, -alto/2);
        CrearLlama(parent, "Llama4", color, grosor * 2f, ancho/2, -alto/2);
        CrearChispas(parent, color, grosor, ancho, alto);
    }
    
    private void CrearMarcoBlanco(GameObject parentObj, float grosor, float ancho, float alto)
    {
        Transform marcoAnterior = parentObj.transform.Find("Marco");
        if (marcoAnterior != null)
        {
            #if UNITY_EDITOR
            DestroyImmediate(marcoAnterior.gameObject);
            #else
            Destroy(marcoAnterior.gameObject);
            #endif
        }
        
        GameObject marcoObj = new GameObject("Marco");
        marcoObj.transform.SetParent(parentObj.transform, false);
        
        RectTransform marcoRect = marcoObj.GetComponent<RectTransform>();
        if (marcoRect == null)
        {
            marcoRect = marcoObj.AddComponent<RectTransform>();
        }
        marcoRect.anchorMin = new Vector2(0.5f, 0.5f);
        marcoRect.anchorMax = new Vector2(0.5f, 0.5f);
        marcoRect.anchoredPosition = Vector2.zero;
        marcoRect.sizeDelta = new Vector2(ancho, alto);
        
        CrearBordeRectangulo(marcoObj, "BordeBlanco", Color.white, grosor, ancho, alto);
        
        marcoObj.SetActive(true);
    }
    
    private void CrearMarcoRojo(GameObject parentObj, float grosor, float ancho, float alto)
    {
        Transform marcoAnterior = parentObj.transform.Find("Marco");
        if (marcoAnterior != null)
        {
            #if UNITY_EDITOR
            DestroyImmediate(marcoAnterior.gameObject);
            #else
            Destroy(marcoAnterior.gameObject);
            #endif
        }
        
        GameObject marcoObj = new GameObject("Marco");
        marcoObj.transform.SetParent(parentObj.transform, false);
        
        RectTransform marcoRect = marcoObj.GetComponent<RectTransform>();
        if (marcoRect == null)
        {
            marcoRect = marcoObj.AddComponent<RectTransform>();
        }
        marcoRect.anchorMin = new Vector2(0.5f, 0.5f);
        marcoRect.anchorMax = new Vector2(0.5f, 0.5f);
        marcoRect.anchoredPosition = Vector2.zero;
        marcoRect.sizeDelta = new Vector2(ancho, alto);
        
        Color rojo = new Color(1f, 0.2f, 0.2f, 1f);
        CrearBordeRectangulo(marcoObj, "BordeRojo", rojo, grosor, ancho, alto);
        
        marcoObj.SetActive(true);
    }
    
    private void CrearMarcoFijo(GameObject parentObj, Color colorMarco, float grosor, float ancho, float alto)
    {
        Transform marcoAnterior = parentObj.transform.Find("Marco");
        if (marcoAnterior != null)
        {
            #if UNITY_EDITOR
            DestroyImmediate(marcoAnterior.gameObject);
            #else
            Destroy(marcoAnterior.gameObject);
            #endif
        }
        
        GameObject marcoObj = new GameObject("Marco");
        marcoObj.transform.SetParent(parentObj.transform, false);
        
        RectTransform marcoRect = marcoObj.GetComponent<RectTransform>();
        if (marcoRect == null)
        {
            marcoRect = marcoObj.AddComponent<RectTransform>();
        }
        marcoRect.anchorMin = new Vector2(0.5f, 0.5f);
        marcoRect.anchorMax = new Vector2(0.5f, 0.5f);
        marcoRect.anchoredPosition = Vector2.zero;
        marcoRect.sizeDelta = new Vector2(ancho, alto);
        
        marcoObj.SetActive(true); // Asegurar que el marco sea visible
        
        Color colorSombra = new Color(colorMarco.r * 0.3f, colorMarco.g * 0.3f, colorMarco.b * 0.3f, colorMarco.a * 0.5f);
        CrearBordeRectangulo(marcoObj, "Sombra", colorSombra, grosor + 2f, ancho + 4f, alto + 4f);
        
        CrearBordeRectangulo(marcoObj, "BordeExterior", colorMarco, grosor, ancho, alto);
        
        Color colorInterior = new Color(
            Mathf.Min(1f, colorMarco.r * 1.2f),
            Mathf.Min(1f, colorMarco.g * 1.2f),
            Mathf.Min(1f, colorMarco.b * 1.2f),
            colorMarco.a
        );
        CrearBordeRectangulo(marcoObj, "BordeInterior", colorInterior, grosor * 0.5f, ancho - grosor * 1.5f, alto - grosor * 1.5f);
        
        float tamañoEsquina = grosor * 2f;
        CrearEsquinaDecorativa(marcoObj, "Esquina1", colorMarco, tamañoEsquina, -ancho/2, alto/2);
        CrearEsquinaDecorativa(marcoObj, "Esquina2", colorMarco, tamañoEsquina, ancho/2, alto/2);
        CrearEsquinaDecorativa(marcoObj, "Esquina3", colorMarco, tamañoEsquina, -ancho/2, -alto/2);
        CrearEsquinaDecorativa(marcoObj, "Esquina4", colorMarco, tamañoEsquina, ancho/2, -alto/2);
        
        CrearPuntosDecorativos(marcoObj, colorMarco, grosor, ancho, alto);
    }
    
    private void CrearBordeRectangulo(GameObject parent, string nombre, Color color, float grosor, float ancho, float alto)
    {
        CrearRectangulo(parent, nombre + "_Top", color, ancho + grosor * 2, grosor, 0, alto / 2 + grosor / 2);
        CrearRectangulo(parent, nombre + "_Bottom", color, ancho + grosor * 2, grosor, 0, -alto / 2 - grosor / 2);
        CrearRectangulo(parent, nombre + "_Left", color, grosor, alto, -ancho / 2 - grosor / 2, 0);
        CrearRectangulo(parent, nombre + "_Right", color, grosor, alto, ancho / 2 + grosor / 2, 0);
    }
    
    private void CrearRectangulo(GameObject parent, string nombre, Color color, float ancho, float alto, float posX, float posY)
    {
        GameObject rectObj = new GameObject(nombre);
        rectObj.transform.SetParent(parent.transform, false);
        Image rectImg = rectObj.AddComponent<Image>();
        
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        rectImg.sprite = sprite;
        rectImg.color = color;
        rectImg.raycastTarget = false; // No bloquear raycasts
        
        RectTransform rect = rectObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(posX, posY);
        rect.sizeDelta = new Vector2(ancho, alto);
        
        rectObj.SetActive(true);
    }
    
    private void CrearEsquina(GameObject parent, string nombre, Color color, float tamaño, float posX, float posY)
    {
        GameObject esquinaObj = new GameObject(nombre);
        esquinaObj.transform.SetParent(parent.transform, false);
        Image esquinaImg = esquinaObj.AddComponent<Image>();
        
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        esquinaImg.sprite = sprite;
        esquinaImg.color = color;
        esquinaImg.raycastTarget = false; // No bloquear raycasts
        
        RectTransform rect = esquinaObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(posX, posY);
        rect.sizeDelta = new Vector2(tamaño, tamaño);
        
        esquinaObj.SetActive(true);
    }
    
    private void CrearEsquinaDecorativa(GameObject parent, string nombre, Color color, float tamaño, float posX, float posY)
    {
        CrearEsquina(parent, nombre + "_Base", color, tamaño, posX, posY);
        Color brillo = new Color(1f, 1f, 1f, 0.6f);
        CrearEsquina(parent, nombre + "_Brillo", brillo, tamaño * 0.5f, posX, posY);
    }
    
    private void CrearPuntosDecorativos(GameObject parent, Color color, float grosor, float ancho, float alto)
    {
        float puntoSize = grosor * 0.5f;
        int puntosPorLado = 4;
        float espacio = ancho / (puntosPorLado + 1);
        
        for (int i = 1; i <= puntosPorLado; i++)
        {
            CrearPunto(parent, "PuntoTop" + i, color, puntoSize, -ancho/2 + espacio * i, alto/2);
            CrearPunto(parent, "PuntoBot" + i, color, puntoSize, -ancho/2 + espacio * i, -alto/2);
        }
        
        espacio = alto / (puntosPorLado + 1);
        for (int i = 1; i <= puntosPorLado; i++)
        {
            CrearPunto(parent, "PuntoLeft" + i, color, puntoSize, -ancho/2, -alto/2 + espacio * i);
            CrearPunto(parent, "PuntoRight" + i, color, puntoSize, ancho/2, -alto/2 + espacio * i);
        }
    }
    
    private void CrearPunto(GameObject parent, string nombre, Color color, float tamaño, float posX, float posY)
    {
        GameObject puntoObj = new GameObject(nombre);
        puntoObj.transform.SetParent(parent.transform, false);
        Image puntoImg = puntoObj.AddComponent<Image>();
        
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        puntoImg.sprite = sprite;
        puntoImg.color = color;
        puntoImg.raycastTarget = false; // No bloquear raycasts
        
        RectTransform rect = puntoObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(posX, posY);
        rect.sizeDelta = new Vector2(tamaño, tamaño);
        
        puntoObj.SetActive(true);
    }
    
    private void CrearPuntaGotic(GameObject parent, string nombre, Color color, float tamaño, float posX, float posY, float rotacion)
    {
        CrearEsquina(parent, nombre, color, tamaño, posX, posY);
    }
    
    private void CrearDecoracionesGotico(GameObject parent, Color color, float grosor, float ancho, float alto)
    {
        float margenExterior = grosor * 1.5f;
        for (int i = 0; i < 3; i++)
        {
            float offsetX = (i - 1) * (ancho * 0.3f);
            CrearRectangulo(parent, "DecorGoticTop" + i, color, grosor * 0.5f, grosor * 1.5f, offsetX, alto/2 + margenExterior);
            CrearRectangulo(parent, "DecorGoticBot" + i, color, grosor * 0.5f, grosor * 1.5f, offsetX, -alto/2 - margenExterior);
        }
    }
    
    private void CrearRayoEnergia(GameObject parent, string nombre, Color color, float tamaño, float posX, float posY, float angulo)
    {
        CrearEsquina(parent, nombre, color, tamaño, posX, posY);
    }
    
    private void CrearLineasEnergia(GameObject parent, Color color, float grosor, float ancho, float alto)
    {
        Color energia = new Color(color.r, color.g, color.b, 0.5f);
        float margenExterior = grosor * 2f;
        CrearRectangulo(parent, "LineaEnergia1", energia, ancho * 1.2f, grosor * 0.3f, 0, alto/2 + margenExterior);
        CrearRectangulo(parent, "LineaEnergia2", energia, ancho * 1.2f, grosor * 0.3f, 0, -alto/2 - margenExterior);
        CrearRectangulo(parent, "LineaEnergia3", energia, grosor * 0.3f, alto * 1.2f, ancho/2 + margenExterior, 0);
        CrearRectangulo(parent, "LineaEnergia4", energia, grosor * 0.3f, alto * 1.2f, -ancho/2 - margenExterior, 0);
    }
    
    private void CrearGema(GameObject parent, string nombre, Color color, float tamaño, float posX, float posY)
    {
        Color gemaBrillo = new Color(Mathf.Min(1f, color.r * 1.5f), Mathf.Min(1f, color.g * 1.5f), Mathf.Min(1f, color.b * 1.5f), 1f);
        CrearEsquina(parent, nombre + "_Base", color, tamaño, posX, posY);
        CrearEsquina(parent, nombre + "_Brillo", gemaBrillo, tamaño * 0.6f, posX, posY);
    }
    
    private void CrearEspadaDecorativa(GameObject parent, string nombre, Color color, float tamaño, float posX, float posY, float angulo)
    {
        CrearRectangulo(parent, nombre, color, tamaño * 0.3f, tamaño, posX, posY);
    }
    
    private void CrearLineasCorte(GameObject parent, Color color, float grosor, float ancho, float alto)
    {
        Color corte = new Color(color.r, color.g, color.b, 0.7f);
        float margenExterior = grosor * 1.5f;
        CrearRectangulo(parent, "Corte1", corte, grosor * 0.5f, grosor * 2f, -ancho/2 - margenExterior, alto/2 + margenExterior);
        CrearRectangulo(parent, "Corte2", corte, grosor * 0.5f, grosor * 2f, ancho/2 + margenExterior, alto/2 + margenExterior);
        CrearRectangulo(parent, "Corte3", corte, grosor * 0.5f, grosor * 2f, -ancho/2 - margenExterior, -alto/2 - margenExterior);
        CrearRectangulo(parent, "Corte4", corte, grosor * 0.5f, grosor * 2f, ancho/2 + margenExterior, -alto/2 - margenExterior);
    }
    
    private void CrearBrillo(GameObject parent, string nombre, Color color, float tamaño, float posX, float posY)
    {
        CrearEsquina(parent, nombre, color, tamaño, posX, posY);
    }
    
    private void CrearReflejos(GameObject parent, Color color, float grosor, float ancho, float alto)
    {
        Color reflejo = new Color(1f, 1f, 1f, 0.3f);
        float margenExterior = grosor * 1.5f;
        CrearRectangulo(parent, "Reflejo1", reflejo, ancho * 0.4f, grosor * 0.3f, 0, alto/2 + margenExterior);
        CrearRectangulo(parent, "Reflejo2", reflejo, grosor * 0.3f, alto * 0.4f, ancho/2 + margenExterior, 0);
    }
    
    private void CrearLlama(GameObject parent, string nombre, Color color, float tamaño, float posX, float posY)
    {
        Color llama = new Color(1f, UnityEngine.Random.Range(0.3f, 0.7f), 0f, 0.9f);
        CrearEsquina(parent, nombre, llama, tamaño, posX, posY);
        Color llamaBrillo = new Color(1f, 1f, 0.5f, 0.6f);
        CrearEsquina(parent, nombre + "_Brillo", llamaBrillo, tamaño * 0.5f, posX, posY);
    }
    
    private void CrearChispas(GameObject parent, Color color, float grosor, float ancho, float alto)
    {
        float margenExterior = grosor * 1.5f;
        for (int i = 0; i < 6; i++)
        {
            bool enBordeX = UnityEngine.Random.value > 0.5f;
            float x, y;
            if (enBordeX)
            {
                x = UnityEngine.Random.value > 0.5f ? ancho/2 + margenExterior : -ancho/2 - margenExterior;
                y = UnityEngine.Random.Range(-alto/2 - margenExterior, alto/2 + margenExterior);
            }
            else
            {
                x = UnityEngine.Random.Range(-ancho/2 - margenExterior, ancho/2 + margenExterior);
                y = UnityEngine.Random.value > 0.5f ? alto/2 + margenExterior : -alto/2 - margenExterior;
            }
            Color chispa = new Color(1f, UnityEngine.Random.Range(0.5f, 1f), 0f, 0.8f);
            CrearPunto(parent, "Chispa" + i, chispa, grosor * 0.4f, x, y);
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
            
            if (animacionAtaque.transform.parent != null)
            {
                Transform marcoRojo = animacionAtaque.transform.parent.Find("Marco");
                if (marcoRojo != null)
                {
                    marcoRojo.gameObject.SetActive(false);
                }
            }
        }
        
        Sprite spriteJugador = gameManager.GetSpriteJugador();
        Sprite spriteEnemigo = gameManager.GetSpriteEnemigo();
        
        string tipoJugador = gameManager.GetTipoJugador();
        string tipoEnemigo = gameManager.GetTipoEnemigo();
        if (!string.IsNullOrEmpty(tipoJugador))
        {
            IniciarAnimacionIdle(tipoJugador, true);
        }
        if (!string.IsNullOrEmpty(tipoEnemigo))
        {
            IniciarAnimacionIdle(tipoEnemigo, false);
        }
        
        if (spriteJugador != null && imgJugador != null)
        {
            imgJugador.sprite = spriteJugador;
            imgJugador.color = Color.white;
            imgJugador.gameObject.SetActive(true);
            
            GameObject imgObjJugador = imgJugador.gameObject;
            
            CrearMarcoBlanco(imgObjJugador, 8f, 150, 150);
            
            Transform marcoJugador = imgObjJugador.transform.Find("Marco");
            if (marcoJugador != null)
            {
                marcoJugador.gameObject.SetActive(true);
                
                for (int i = 0; i < marcoJugador.childCount; i++)
                {
                    Transform child = marcoJugador.GetChild(i);
                    if (child != null) child.gameObject.SetActive(true);
                }
                
                marcoJugador.SetAsFirstSibling();
                imgJugador.transform.SetAsLastSibling();
            }
        }
        else if (imgJugador != null)
        {
            imgJugador.gameObject.SetActive(false);
        }
        
        if (spriteEnemigo != null && imgEnemigo != null)
        {
            imgEnemigo.sprite = spriteEnemigo;
            imgEnemigo.color = Color.white;
            imgEnemigo.gameObject.SetActive(true);
            
            GameObject imgObjEnemigo = imgEnemigo.gameObject;
            
            CrearMarcoEnemigoSegunTipo(imgObjEnemigo, tipoEnemigo, 150, 150);
            
            Transform marcoEnemigo = imgObjEnemigo.transform.Find("Marco");
            if (marcoEnemigo != null)
            {
                marcoEnemigo.gameObject.SetActive(true);
                
                for (int i = 0; i < marcoEnemigo.childCount; i++)
                {
                    Transform child = marcoEnemigo.GetChild(i);
                    if (child != null) child.gameObject.SetActive(true);
                }
                
                marcoEnemigo.SetAsFirstSibling();
                imgEnemigo.transform.SetAsLastSibling();
            }
        }
        else if (imgEnemigo != null)
        {
            imgEnemigo.gameObject.SetActive(false);
        }
    }
    
    public void MostrarPanelDescanso()
    {
        OcultarTodosLosPaneles();
        panelDescanso.SetActive(true);
    }
    
    public void MostrarPanelTienda(int oro)
    {
        OcultarTodosLosPaneles();
        panelTienda.SetActive(true);
        lblOroTienda.text = $"💰 Oro: {oro}";
    }
    
    private void OcultarTodosLosPaneles()
    {
        if (panelMenu != null) panelMenu.SetActive(false);
        if (panelJuego != null) panelJuego.SetActive(false);
        if (panelDescanso != null) panelDescanso.SetActive(false);
        if (panelTienda != null) panelTienda.SetActive(false);
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
    
    private GameObject CrearTarjetaStat(GameObject parent, string nombre, Sprite icono, string texto)
    {
        GameObject tarjeta = new GameObject($"Tarjeta{nombre}");
        tarjeta.transform.SetParent(parent.transform, false);
        RectTransform tarjetaRect = tarjeta.AddComponent<RectTransform>();
        tarjetaRect.sizeDelta = new Vector2(70, 80);
        
        LayoutElement layoutElement = tarjeta.AddComponent<LayoutElement>();
        layoutElement.minWidth = 48f;
        layoutElement.preferredHeight = 80f;
        layoutElement.flexibleWidth = 0f;
        layoutElement.flexibleHeight = 0f;
        StatCardAutoWidth autoWidth = tarjeta.AddComponent<StatCardAutoWidth>();
        
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
        
        autoWidth.label = textoObj;
        autoWidth.layoutElement = layoutElement;
        
        return tarjeta;
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
        if (layoutElement != null)
        {
            layoutElement.preferredWidth = anchoTotal;
            layoutElement.flexibleWidth = 0f;
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
            
            tarjetasJugador["Vida"] = CrearTarjetaStat(panelStatsJugador, "Vida", iconoVida, $"Vida: {vida}/{vidaMax}");
            
            spacerVidaEscudo = new GameObject("SpacerVidaEscudo");
            spacerVidaEscudo.transform.SetParent(panelStatsJugador.transform, false);
            RectTransform spacerRect = spacerVidaEscudo.AddComponent<RectTransform>();
            spacerRect.sizeDelta = Vector2.zero;
            LayoutElement spacerLayout = spacerVidaEscudo.AddComponent<LayoutElement>();            
            spacerLayout.flexibleWidth = 0f;
            spacerLayout.flexibleHeight = 0f;
            spacerVidaEscudo.transform.SetSiblingIndex(1); // Después de Vida (índice 0)
            
            tarjetasJugador["Escudo"] = CrearTarjetaStat(panelStatsJugador, "Escudo", iconoEscudo, $"Escudo: {escudo}/{escudoMax}");
            tarjetasJugador["Oro"] = CrearTarjetaStat(panelStatsJugador, "Oro", iconoOro, $"Oro: {oro}");
            tarjetasJugador["Daño"] = CrearTarjetaStat(panelStatsJugador, "Daño", iconoDaño, $"Daño: {daño}");
            tarjetasJugador["Batalla"] = CrearTarjetaStat(panelStatsJugador, "Batalla", iconoBatalla, $"Batalla: {batalla}");
            
            foreach (var kvp in tarjetasJugador)
            {
                StatCardAutoWidth autoWidth = kvp.Value.GetComponent<StatCardAutoWidth>();
                if (autoWidth != null)
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
                    autoWidth.SetText(textoInicial);
                }
            }
            
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(panelStatsJugador.GetComponent<RectTransform>());
            
            AjustarPosicionPanelJugador();
            
            AplicarOffsetHudUnaVez();
        }
        else
        {
            if (tarjetasJugador.ContainsKey("Vida"))
            {
                StatCardAutoWidth autoWidth = tarjetasJugador["Vida"].GetComponent<StatCardAutoWidth>();
                if (autoWidth != null)
                    autoWidth.SetText($"Vida: {vida}/{vidaMax}");
            }
            if (tarjetasJugador.ContainsKey("Escudo"))
            {
                StatCardAutoWidth autoWidth = tarjetasJugador["Escudo"].GetComponent<StatCardAutoWidth>();
                if (autoWidth != null)
                    autoWidth.SetText($"Escudo: {escudo}/{escudoMax}");
            }
            if (tarjetasJugador.ContainsKey("Oro"))
            {
                StatCardAutoWidth autoWidth = tarjetasJugador["Oro"].GetComponent<StatCardAutoWidth>();
                if (autoWidth != null)
                    autoWidth.SetText($"Oro: {oro}");
            }
            if (tarjetasJugador.ContainsKey("Daño"))
            {
                StatCardAutoWidth autoWidth = tarjetasJugador["Daño"].GetComponent<StatCardAutoWidth>();
                if (autoWidth != null)
                    autoWidth.SetText($"Daño: {daño}");
            }
            if (tarjetasJugador.ContainsKey("Batalla"))
            {
                StatCardAutoWidth autoWidth = tarjetasJugador["Batalla"].GetComponent<StatCardAutoWidth>();
                if (autoWidth != null)
                    autoWidth.SetText($"Batalla: {batalla}");
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
            tarjetaDañoEnemigo = CrearTarjetaStat(panelStatsEnemigo, "DañoEnemigo", iconoDaño, $"Daño: {daño}");
            if (tarjetaDañoEnemigo != null)
            {
                tarjetaDañoEnemigo.SetActive(true);
                RectTransform tarjetaRect = tarjetaDañoEnemigo.GetComponent<RectTransform>();
                if (tarjetaRect != null)
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
            int cantidadFrames = 0;
            string carpeta = "";
            string animacion = "attack"; // Usar la nueva estructura de carpetas
            
            if (tipo == "Guerrero")
            {
                cantidadFrames = 8;
                carpeta = "Guerrero";
            }
            else if (tipo == "Mago")
            {
                cantidadFrames = 7; // 7 frames para attack del mago
                carpeta = "Mago";
            }
            else if (tipo == "Cazador")
            {
                cantidadFrames = 9;
                carpeta = "Cazador";
            }
            
            if (cantidadFrames > 0)
            {
                DetenerAnimacionesIdle();
                
                if (corrutinaAnimacion != null)
                {
                    StopCoroutine(corrutinaAnimacion);
                }
                corrutinaAnimacion = StartCoroutine(ReproducirAnimacion(carpeta, animacion, cantidadFrames, daño, esJugador));
            }
            else
            {
                animacionAtaque.gameObject.SetActive(true);
                MostrarTextoDaño(daño, esJugador);
            }
        }
    }
    
    public void MostrarAnimacionHurt(string tipo, bool esJugador)
    {
        if (animacionAtaque != null)
        {
            int cantidadFrames = 2; // 2 frames para hurt
            string carpeta = "";
            string animacion = "hurt";
            
            if (tipo == "Mago")
            {
                carpeta = "Mago";
            }
            else
            {
                MostrarAnimacionAtaque(tipo, esJugador, 0, "", "");
                return;
            }
            
            DetenerAnimacionesIdle();
            
            if (corrutinaAnimacion != null)
            {
                StopCoroutine(corrutinaAnimacion);
            }
            corrutinaAnimacion = StartCoroutine(ReproducirAnimacion(carpeta, animacion, cantidadFrames, 0, esJugador, false));
        }
    }
    
    public void IniciarAnimacionIdle(string tipo, bool esJugador)
    {
        if (animacionAtaque == null) return;
        
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
    }
    
    private IEnumerator ReproducirAnimacion(string carpeta, string animacion, int cantidadFrames, int daño, bool esJugador, bool mostrarMarco = true)
    {
        if (animacionAtaque == null) yield break;
        
        animacionAtaque.gameObject.SetActive(true);
        
        if (mostrarMarco && animacionAtaque.transform.parent != null)
        {
            Transform marcoRojo = animacionAtaque.transform.parent.Find("Marco");
            if (marcoRojo != null)
            {
                marcoRojo.gameObject.SetActive(true);
                foreach (Transform child in marcoRojo)
                {
                    if (child != null)
                    {
                        child.gameObject.SetActive(true);
                    }
                }
            }
        }
        
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
        
        if (daño > 0)
        {
            MostrarTextoDaño(daño, esJugador);
        }
        
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
                Transform marcoRojo = parent.Find("Marco");
                if (marcoRojo != null)
                {
                    marcoRojo.gameObject.SetActive(false);
                }
                
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
    // Diálogos
    public void MostrarDialogoSeleccionClase(Action<string> callback)
    {
        // Destruir cualquier diálogo anterior que pueda existir
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
        // Fondo completamente opaco para evitar imágenes residuales
        GameObject dialog = CrearPanel("DialogoSkill", new Color(0.1f, 0.1f, 0.15f, 1f));
        dialog.transform.SetParent(canvas.transform, false);
        dialog.transform.SetAsLastSibling(); // Asegurar que esté al frente
        
        Text titulo = CrearTexto("TituloSkill", $"⭐ DISTRIBUYE {puntosTotal} PUNTOS ⭐", 48, new Color(1f, 0.9f, 0.3f, 1f)); // Tamaño al doble (era 24, ahora 48)
        titulo.rectTransform.anchorMin = new Vector2(0.5f, 0.85f);
        titulo.rectTransform.anchorMax = new Vector2(0.5f, 0.85f);
        titulo.rectTransform.anchoredPosition = Vector2.zero;
        titulo.rectTransform.sizeDelta = new Vector2(800, 80); // Tamaño al doble (era 400x40, ahora 800x80)
        titulo.transform.SetParent(dialog.transform, false);
        
        // Variables para almacenar los valores (usando array para poder modificarlos en los listeners)
        int[] valores = new int[3];
        valores[0] = 1; // Vida: 1 punto inicial
        valores[1] = 1; // Armadura: 1 punto inicial
        valores[2] = 1; // Daño: 1 punto inicial
        
        int puntosUsados = valores[0] + valores[1] + valores[2];
        int puntosRestantes = puntosTotal - puntosUsados;
        
        // Texto de puntos restantes - tamaño al doble
        Text lblPuntosRestantes = CrearTexto("LblPuntosRestantes", $"Puntos disponibles: {puntosRestantes}", 32, new Color(0.8f, 0.8f, 0.2f, 1f)); // Era 16, ahora 32
        lblPuntosRestantes.rectTransform.anchorMin = new Vector2(0.5f, 0.75f);
        lblPuntosRestantes.rectTransform.anchorMax = new Vector2(0.5f, 0.75f);
        lblPuntosRestantes.rectTransform.anchoredPosition = Vector2.zero;
        lblPuntosRestantes.rectTransform.sizeDelta = new Vector2(600, 50); // Tamaño al doble (era 300x25, ahora 600x50)
        lblPuntosRestantes.transform.SetParent(dialog.transform, false);
        
        // Referencias a los InputFields para poder actualizarlos desde el botón aleatorio
        InputField[] inputFields = new InputField[3];
        
        // Cargar iconos desde imágenes separadas
        Sprite iconoVida = CargarIcono("vida");
        Sprite iconoEscudo = CargarIcono("escudo");
        Sprite iconoDaño = CargarIcono("daño");
        
        // Crear controles para Vida (con más separación vertical)
        inputFields[0] = CrearControlStat(dialog, "Vida", iconoVida, valores, 0, maxVida, 0.65f, puntosTotal, lblPuntosRestantes);
        
        // Crear controles para Armadura (con más separación vertical)
        inputFields[1] = CrearControlStat(dialog, "Escudo", iconoEscudo, valores, 1, maxArmadura, 0.5f, puntosTotal, lblPuntosRestantes);
        
        // Crear controles para Daño (con más separación vertical)
        inputFields[2] = CrearControlStat(dialog, "Daño", iconoDaño, valores, 2, maxDaño, 0.35f, puntosTotal, lblPuntosRestantes);
        
        // Botón Aleatorio - tamaño al doble
        Button btnAleatorio = CrearBotonConColor("BtnAleatorio", "🎲 ALEATORIO", 36, new Color(0.9f, 0.6f, 0.2f, 1f)); // Fuente al doble (era 18, ahora 36)
        btnAleatorio.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.1f); // Bajado (era 0.2f, ahora 0.1f)
        btnAleatorio.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.1f); // Bajado (era 0.2f, ahora 0.1f)
        btnAleatorio.GetComponent<RectTransform>().anchoredPosition = new Vector2(-220, 0); // Ajustado proporcionalmente
        btnAleatorio.GetComponent<RectTransform>().sizeDelta = new Vector2(360, 80); // Tamaño al doble (era 180x40, ahora 360x80)
        btnAleatorio.transform.SetParent(dialog.transform, false);
        btnAleatorio.onClick.AddListener(() => {
            // Distribución aleatoria REAL: valores completamente aleatorios para cada stat
            // La suma debe ser exactamente igual a puntosTotal
            System.Random rand = new System.Random();
            
            int[] maximos = { maxVida, maxArmadura, maxDaño };
            int sumaObjetivo = puntosTotal;
            
            // Cada stat ya tiene 1 punto asignado, así que tenemos 297 puntos para distribuir (300-3)
            int puntosRestantesParaDistribuir = sumaObjetivo - 3;
            
            // Generar valores aleatorios adicionales para cada stat (puede ser 0 o más)
            // La suma de estos valores adicionales debe ser puntosRestantesParaDistribuir
            int valorExtra1 = rand.Next(0, Mathf.Min(maxVida - 1, puntosRestantesParaDistribuir) + 1);
            int valorExtra2 = rand.Next(0, Mathf.Min(maxArmadura - 1, puntosRestantesParaDistribuir - valorExtra1) + 1);
            int valorExtra3 = rand.Next(0, Mathf.Min(maxDaño - 1, puntosRestantesParaDistribuir - valorExtra1 - valorExtra2) + 1);
            
            // Asignar los valores (1 base + extras)
            valores[0] = 1 + valorExtra1;
            valores[1] = 1 + valorExtra2;
            valores[2] = 1 + valorExtra3;
            
            int sumaActual = valores[0] + valores[1] + valores[2];
            
            // Ajustar para que la suma sea exactamente puntosTotal
            int diferencia = sumaObjetivo - sumaActual;
            
            // Si hay diferencia, distribuirla aleatoriamente
            while (diferencia != 0)
            {
                int statElegida = rand.Next(3);
                
                if (diferencia > 0)
                {
                    // Aumentar
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
                    // Reducir (pero nunca menos de 1)
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
                
                // Evitar loops infinitos
                if (Mathf.Abs(diferencia) > 100) break;
            }
            
            // Asegurar que no excedan los máximos y que tengan al menos 1
            valores[0] = Mathf.Clamp(valores[0], 1, maxVida);
            valores[1] = Mathf.Clamp(valores[1], 1, maxArmadura);
            valores[2] = Mathf.Clamp(valores[2], 1, maxDaño);
            
            // Actualizar los campos de texto directamente
            if (inputFields[0] != null) inputFields[0].text = valores[0].ToString();
            if (inputFields[1] != null) inputFields[1].text = valores[1].ToString();
            if (inputFields[2] != null) inputFields[2].text = valores[2].ToString();
            
            // Actualizar puntos restantes
            ActualizarPuntosRestantes(lblPuntosRestantes, valores, puntosTotal);
        });
        
        // Botón Aceptar
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
        // Cargar el icono desde la imagen separada
        // Intentar diferentes extensiones posibles
        string[] extensiones = { ".png", ".PNG", ".jpg", ".JPG" };
        
        foreach (string ext in extensiones)
        {
            string ruta = $"Assets/Sprites/UI/{nombre}{ext}";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
            if (sprite != null)
            {
                return sprite;
            }
            
            // Si no se carga como Sprite, intentar como Texture2D y crear el sprite
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(ruta);
            if (texture != null)
            {
                // Crear sprite desde la textura completa
                Rect rect = new Rect(0, 0, texture.width, texture.height);
                return Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 100f);
            }
        }
        
        Debug.LogWarning($"No se encontró el icono {nombre} en Assets/Sprites/UI/");
        return null;
        #else
        // En runtime, cargar desde Resources
        Sprite sprite = Resources.Load<Sprite>($"Sprites/UI/{nombre}");
        if (sprite != null) return sprite;
        
        // Si no se carga como Sprite, intentar como Texture2D
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
        // Crear contenedor para el icono (más cerca del texto)
        GameObject contenedorIcono = new GameObject($"Icono{nombre}");
        contenedorIcono.transform.SetParent(parent.transform, false);
        RectTransform iconoRect = contenedorIcono.AddComponent<RectTransform>();
        iconoRect.anchorMin = new Vector2(0.5f, posicionY);
        iconoRect.anchorMax = new Vector2(0.5f, posicionY);
        iconoRect.anchoredPosition = new Vector2(-370, 0); // 1 cm más a la izquierda (aproximadamente 50 píxeles más)
        iconoRect.sizeDelta = new Vector2(80, 80); // Tamaño reducido para evitar superposición (era 100, ahora 80)
        
        // Agregar imagen del icono si existe
        if (iconoSprite != null)
        {
            Image iconoImage = contenedorIcono.AddComponent<Image>();
            iconoImage.sprite = iconoSprite;
            iconoImage.preserveAspect = true;
        }
        
        // Label del nombre con máximo permitido (sin emoji) - tamaño de fuente al doble
        Text lblNombre = CrearTexto($"Lbl{nombre}", $"{nombre} (Máx: {maximo})", 32, Color.white); // Era 16, ahora 32
        lblNombre.rectTransform.anchorMin = new Vector2(0.5f, posicionY);
        lblNombre.rectTransform.anchorMax = new Vector2(0.5f, posicionY);
        lblNombre.rectTransform.anchoredPosition = new Vector2(-100, 0); // Ajustado para que no se superponga con el icono
        lblNombre.rectTransform.sizeDelta = new Vector2(400, 50); // Tamaño aumentado proporcionalmente
        lblNombre.alignment = TextAnchor.MiddleLeft;
        lblNombre.transform.SetParent(parent.transform, false);
        
        // Campo de texto editable (InputField) - crear primero para poder referenciarlo
        GameObject inputObj = new GameObject($"Input{nombre}");
        inputObj.transform.SetParent(parent.transform, false);
        
        RectTransform inputRect = inputObj.AddComponent<RectTransform>();
        inputRect.anchorMin = new Vector2(0.5f, posicionY);
        inputRect.anchorMax = new Vector2(0.5f, posicionY);
        inputRect.anchoredPosition = new Vector2(160, 0); // Ajustado para el nuevo tamaño
        inputRect.sizeDelta = new Vector2(160, 60); // Tamaño al doble (era 80x30, ahora 160x60)
        
        InputField inputField = inputObj.AddComponent<InputField>();
        
        // Crear background para el input (más claro para mejor contraste)
        Image inputBg = inputObj.AddComponent<Image>();
        inputBg.color = new Color(0.3f, 0.3f, 0.3f, 1f); // Un poco más claro para mejor contraste con el texto
        
        // Crear texto del input
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
        
        // Configurar InputField
        inputField.textComponent = inputText;
        inputField.contentType = InputField.ContentType.IntegerNumber;
        inputField.inputType = InputField.InputType.Standard;
        
        // Botón -
        Button btnMenos = CrearBotonConColor($"Btn{nombre}Menos", "-", 40, new Color(0.7f, 0.3f, 0.3f, 1f)); // Fuente al doble (era 20, ahora 40)
        RectTransform menosRect = btnMenos.GetComponent<RectTransform>();
        menosRect.anchorMin = new Vector2(0.5f, posicionY);
        menosRect.anchorMax = new Vector2(0.5f, posicionY);
        menosRect.anchoredPosition = new Vector2(60, 0); // Ajustado para el nuevo tamaño
        menosRect.sizeDelta = new Vector2(70, 60); // Tamaño al doble (era 35x30, ahora 70x60)
        btnMenos.transform.SetParent(parent.transform, false);
        btnMenos.onClick.AddListener(() => {
            if (valores[indice] > 0)
            {
                valores[indice]--;
                if (inputField != null) inputField.text = valores[indice].ToString();
                ActualizarPuntosRestantes(lblPuntosRestantes, valores, puntosTotal);
            }
        });
        
        // Actualizar cuando cambie el texto
        inputField.onEndEdit.AddListener((string texto) => {
            int nuevoValor;
            if (int.TryParse(texto, out nuevoValor))
            {
                int puntosUsados = valores[0] + valores[1] + valores[2];
                int puntosActuales = valores[indice];
                int puntosDisponibles = puntosTotal - puntosUsados + puntosActuales;
                
                // Validar que esté dentro del rango
                if (nuevoValor < 0)
                {
                    nuevoValor = 0;
                }
                else if (nuevoValor > maximo)
                {
                    nuevoValor = maximo;
                }
                else if (nuevoValor > puntosDisponibles)
                {
                    nuevoValor = puntosDisponibles;
                }
                
                valores[indice] = nuevoValor;
                inputField.text = nuevoValor.ToString();
                ActualizarPuntosRestantes(lblPuntosRestantes, valores, puntosTotal);
            }
            else
            {
                // Si no es un número válido, restaurar el valor anterior
                inputField.text = valores[indice].ToString();
            }
        });
        
        // Validar al escribir
        inputField.onValueChanged.AddListener((string texto) => {
            // Actualizar puntos restantes en tiempo real
            int nuevoValor;
            if (int.TryParse(texto, out nuevoValor))
            {
                int puntosUsados = valores[0] + valores[1] + valores[2];
                int puntosActuales = valores[indice];
                int puntosDisponibles = puntosTotal - puntosUsados + puntosActuales;
                
                if (nuevoValor >= 0 && nuevoValor <= maximo && nuevoValor <= puntosDisponibles)
                {
                    valores[indice] = nuevoValor;
                    ActualizarPuntosRestantes(lblPuntosRestantes, valores, puntosTotal);
                }
            }
        });
        
        // Botón +
        Button btnMas = CrearBotonConColor($"Btn{nombre}Mas", "+", 40, new Color(0.3f, 0.7f, 0.3f, 1f)); // Fuente al doble (era 20, ahora 40)
        RectTransform masRect = btnMas.GetComponent<RectTransform>();
        masRect.anchorMin = new Vector2(0.5f, posicionY);
        masRect.anchorMax = new Vector2(0.5f, posicionY);
        masRect.anchoredPosition = new Vector2(260, 0); // Ajustado para el nuevo tamaño
        masRect.sizeDelta = new Vector2(70, 60); // Tamaño al doble (era 35x30, ahora 70x60)
        btnMas.transform.SetParent(parent.transform, false);
        btnMas.onClick.AddListener(() => {
            int puntosUsados = valores[0] + valores[1] + valores[2];
            int puntosActuales = valores[indice];
            int puntosDisponibles = puntosTotal - puntosUsados + puntosActuales;
            
            if (valores[indice] < maximo && puntosDisponibles > 0)
            {
                valores[indice]++;
                if (inputField != null) inputField.text = valores[indice].ToString();
                ActualizarPuntosRestantes(lblPuntosRestantes, valores, puntosTotal);
            }
        });
        
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
        GameObject dialog = CrearPanel("DialogoOpciones", new Color(0.1f, 0.1f, 0.15f, 0.95f));
        dialog.transform.SetParent(canvas.transform, false);
        
        Text titulo = CrearTexto("TituloOpciones", $"🎉 ¡VICTORIA! 🎉\n💰 Oro ganado: {oroGanado}", 20, new Color(1f, 0.9f, 0.2f, 1f));
        titulo.rectTransform.anchorMin = new Vector2(0.5f, 0.82f);
        titulo.rectTransform.anchorMax = new Vector2(0.5f, 0.82f);
        titulo.rectTransform.anchoredPosition = Vector2.zero;
        titulo.rectTransform.sizeDelta = new Vector2(400, 60);
        titulo.transform.SetParent(dialog.transform, false);
        
        Button btnSiguiente = CrearBotonConColor("BtnSiguiente", "⚔️ PRÓXIMA BATALLA", 18, new Color(0.2f, 0.7f, 0.3f, 1f));
        btnSiguiente.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
        btnSiguiente.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        btnSiguiente.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 80);
        btnSiguiente.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 45);
        btnSiguiente.transform.SetParent(dialog.transform, false);
        btnSiguiente.onClick.AddListener(() => {
            callback("Siguiente");
            Destroy(dialog);
        });
        
        if (puedeRevancha)
        {
            Button btnRevancha = CrearBotonConColor("BtnRevancha", "🔄 REVANCHA", 18, new Color(0.9f, 0.6f, 0.2f, 1f));
            btnRevancha.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
            btnRevancha.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
            btnRevancha.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 20);
            btnRevancha.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 45);
            btnRevancha.transform.SetParent(dialog.transform, false);
            btnRevancha.onClick.AddListener(() => {
                callback("Revancha");
                Destroy(dialog);
            });
        }
        
        Button btnDescanso = CrearBotonConColor("BtnDescanso", "🛏️ IR A DESCANSO", 18, new Color(0.3f, 0.6f, 0.8f, 1f));
        btnDescanso.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
        btnDescanso.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        btnDescanso.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -40);
        btnDescanso.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 45);
        btnDescanso.transform.SetParent(dialog.transform, false);
        btnDescanso.onClick.AddListener(() => {
            callback("Descanso");
            Destroy(dialog);
        });
        
        Button btnAbandonar = CrearBotonConColor("BtnAbandonar", "🚪 ABANDONAR", 18, new Color(0.7f, 0.2f, 0.2f, 1f));
        btnAbandonar.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
        btnAbandonar.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        btnAbandonar.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -100);
        btnAbandonar.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 45);
        btnAbandonar.transform.SetParent(dialog.transform, false);
        btnAbandonar.onClick.AddListener(() => {
            callback("Abandonar");
            Destroy(dialog);
        });
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
    #endregion
}


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
// using TMPro; // TextMeshPro no instalado - usando Text normal

public class GameManager : MonoBehaviour
{
    #region Estado del juego
    // Variables del juego
    private int OroGuardado = 0;
    private int experienciaGuardada = 0;
    private int contadorClemencias = 0;
    private int contadorMuertes = 0;
    private string nombreJugador1 = "";
    private string nombreJugador2 = "";
    private int vidaJugador1 = 100;
    private int vidaJugador2 = 100;
    private int escudoJugador1 = 100;
    private int escudoJugador2 = 100;
    private int escudoMaximoJugador1 = 100;
    private int dañoJugador1 = 0;
    private int dañoJugador2 = 0;
    private System.Random random = new System.Random();
    private int vidaInicialCPU = 0;
    private int numeroPelea = 0;
    private int numeroRounds = 0;
    private int golpesAcertados = 0;
    private int nivelEnemigo = 1;
    private int vidaInicialEnemigo = 0;
    private int nivelEnemigoActual = 1;
    private bool revanchaUsada = false;
    private int vidaMaximaJugador1 = 100;
    private int vidaMaximaEnemigo = 100;
    private int escudoMaximoEnemigo = 100;
    private string tipoJugador = "";
    private int segundosRestantes = 0;
    private bool ataqueEnProceso = false;
    private bool modoAutomatico = false; // Modo automático desactivado por defecto
    private bool defensaPendiente = false;
    private bool detenerSolicitado = false;
    private bool infoDefensaMostrada = false;
    private bool saltarProximoAtaqueJugador = false;
    private string tipoEnemigo = "";
    private bool revanchaActiva = false;
    private int escudoJugador1AntesRevancha = -1;
    #endregion

    #region Referencias UI
    public Canvas canvas;
    public UIManager uiManager;
    #endregion
    
    #region Sprites de personajes
    public Sprite guerreroJugadorSprite;
    public Sprite magoJugadorSprite;
    public Sprite cazadorJugadorSprite;
    public Sprite guerreroEnemigoSprite;
    public Sprite magoEnemigoSprite;
    public Sprite cazadorEnemigoSprite;
    #endregion

    // Estados del juego (enum definido pero no utilizado actualmente)
    // Se puede usar en el futuro para controlar el flujo del juego
    // private enum EstadoJuego
    // {
    //     Menu,
    //     Juego,
    //     Descanso,
    //     Tienda
    // }

    #region Unity
    private void Awake()
    {
        // ✅ INICIALIZAR REFERENCIAS VACÍAS para limpiar warnings de Missing Reference
        if (canvas == null)
        {
            canvas = GetComponentInChildren<Canvas>();
            if (canvas == null)
            {
                canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            }
        }
        
        if (uiManager == null)
        {
            uiManager = GetComponentInChildren<UIManager>();
            if (uiManager == null)
            {
                uiManager = UnityEngine.Object.FindFirstObjectByType<UIManager>();
            }
        }
        
        Debug.Log($"[GameManager] Awake: Referencias inicializadas - canvas: {canvas != null}, uiManager: {uiManager != null}");
    }
    
    void Start()
    {
        if (canvas == null)
        {
            canvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
        }
        
        if (uiManager == null)
        {
            uiManager = UnityEngine.Object.FindFirstObjectByType<UIManager>();
            if (uiManager == null)
            {
                GameObject uiObj = new GameObject("UIManager");
                uiManager = uiObj.AddComponent<UIManager>();
            }
        }
        
        uiManager.Initialize(this);
        
        // Cargar sprites automáticamente si no están asignados (solo funciona en editor)
        CargarSpritesAutomaticamente();
        MostrarMenu();
    }
    #endregion

    #region Botones - Menu / Inicio
    public void BtnJugar_Click()
    {
        MostrarDialogoSeleccionClase();
    }

    private void MostrarDialogoSeleccionClase()
    {
        uiManager.MostrarDialogoSeleccionClase((string tipo) => {
            tipoJugador = tipo;
            nombreJugador1 = tipo; // Usar el tipo como nombre del jugador
            // NO generar el nombre del enemigo aquí, se hará en IniciarNuevaBatalla después de generar el tipo
            IniciarJuego();
        });
    }
    #endregion

    #region Enemigo - Nombre
    private string GenerarNombreEnemigo()
    {
        // El tipo ya fue generado aleatoriamente en GenerarTipoEnemigoAleatorio()
        string[] nombresGuerrero = { "Thorin", "Grom", "Korg", "Drak", "Borg", "Thane", "Gorth", "Krom" };
        string[] nombresMago = { "Merlin", "Gandalf", "Zephyr", "Arcane", "Mystic", "Sage", "Wizard", "Mage" };
        string[] nombresCazador = { "Legolas", "Robin", "Hawkeye", "Arrow", "Hunter", "Ranger", "Tracker", "Scout" };
        
        string[] nombres = tipoEnemigo == "Guerrero" ? nombresGuerrero : 
                          tipoEnemigo == "Mago" ? nombresMago : nombresCazador;
        
        return $"{tipoEnemigo} {nombres[random.Next(nombres.Length)]}";
    }
    #endregion

    #region Juego - Setup inicial
    private void IniciarJuego()
    {
        numeroPelea++;
        revanchaUsada = false;
        
        MostrarDialogoDistribucionSkillInicial();
    }

    private void MostrarDialogoDistribucionSkillInicial()
    {
        uiManager.MostrarDialogoDistribucionSkill(300, 200, 100, 50, (int vida, int armadura, int daño) => {
            vidaJugador1 = vida;
            vidaMaximaJugador1 = vida;
            escudoJugador1 = armadura;
            escudoMaximoJugador1 = armadura;
            dañoJugador1 = daño;
            
            IniciarNuevaBatalla();
        });
    }
    #endregion

    #region Batalla - Setup
    private void IniciarNuevaBatalla()
    {
        numeroRounds = 0;
        golpesAcertados = 0;
        revanchaUsada = false;
        ataqueEnProceso = false;
        revanchaActiva = false;
        escudoJugador1AntesRevancha = -1;
        
        // Generar tipo de enemigo aleatorio en cada batalla (diferente al jugador)
        GenerarTipoEnemigoAleatorio();
        
        // Generar nombre del enemigo después de tener el tipo
        nombreJugador2 = GenerarNombreEnemigo();
        
        // Calcular nivel del enemigo según número de pelea
        CalcularNivelEnemigo();
        
        // Generar stats del enemigo (aleatorio en cada batalla)
        GenerarStatsEnemigo();
        
        // Configurar UI de batalla
        uiManager.MostrarPanelJuego();
        ActualizarInterfaz();
        ActualizarTextoBotonHuir();
        uiManager.ActualizarContadoresPostBatalla(contadorClemencias, contadorMuertes);
        
        // Configurar modo de ataque
        modoAutomatico = false;
        uiManager.ActualizarBotonAtaque(false);
        
        // No iniciar automáticamente, esperar a que el jugador presione ATAQUE
    }
    
    private void IniciarRevancha()
    {
        numeroRounds = 0;
        golpesAcertados = 0;
        ataqueEnProceso = false;
        
        // Mantener tipo y nombre del enemigo
        vidaJugador2 = vidaInicialEnemigo;
        escudoJugador2 = 0;
        escudoMaximoEnemigo = 0;
        
        uiManager.MostrarPanelJuego();
        ActualizarInterfaz();
        ActualizarTextoBotonHuir();
        
        modoAutomatico = false;
        uiManager.ActualizarBotonAtaque(false);
    }
    #endregion
    
    #region Enemigo - Generacion y stats
    private void GenerarTipoEnemigoAleatorio()
    {
        string[] tipos = { "Guerrero", "Mago", "Cazador" };
        
        // Asegurar que el enemigo sea diferente al jugador
        string tipoEnemigoAleatorio;
        do
        {
            tipoEnemigoAleatorio = tipos[random.Next(tipos.Length)];
        } while (tipoEnemigoAleatorio == tipoJugador && !string.IsNullOrEmpty(tipoJugador));
        
        tipoEnemigo = tipoEnemigoAleatorio;
        nombreJugador2 = GenerarNombreEnemigo();
    }

    private void CalcularNivelEnemigo()
    {
        if (numeroPelea >= 40)
        {
            nivelEnemigo = 250;
        }
        else if (numeroPelea >= 35)
        {
            nivelEnemigo = random.Next(90, 101);
        }
        else if (numeroPelea >= 30)
        {
            nivelEnemigo = random.Next(75, 101);
        }
        else if (numeroPelea >= 25)
        {
            nivelEnemigo = random.Next(50, 101);
        }
        else if (numeroPelea >= 20)
        {
            nivelEnemigo = random.Next(20, 81);
        }
        else if (numeroPelea >= 15)
        {
            nivelEnemigo = random.Next(15, 41);
        }
        else if (numeroPelea >= 10)
        {
            nivelEnemigo = random.Next(10, 21);
        }
        else if (numeroPelea >= 5)
        {
            nivelEnemigo = random.Next(5, 11);
        }
        else
        {
            nivelEnemigo = random.Next(numeroPelea, 6);
        }
        
        nivelEnemigoActual = nivelEnemigo;
    }

    private void GenerarStatsEnemigo()
    {
        // Distribuir puntos del enemigo según su nivel
        DistribuirPuntosEnemigo(nivelEnemigo);
        
        vidaInicialEnemigo = vidaJugador2;
        vidaInicialCPU = vidaJugador2;
        vidaMaximaEnemigo = vidaJugador2;
        escudoMaximoEnemigo = escudoJugador2;
    }

    private void DistribuirPuntosEnemigo(int nivel)
    {
        // Resetear valores base completamente aleatorios
        int puntosVida = 0;
        int puntosEscudo = 0;
        int puntosDaño = 0;
        
        // Calcular puntos totales según nivel (más variación aleatoria)
        int puntosBasePorNivel = Mathf.Max(1, nivel / 3); // Aproximadamente 1 punto cada 3 niveles
        int variacionAleatoria = random.Next(-2, 3); // Variación de -2 a +2
        int puntosTotales = (puntosBasePorNivel * 3) + variacionAleatoria;
        puntosTotales = Mathf.Max(1, puntosTotales); // Mínimo 1 punto
        
        // Distribuir puntos de forma aleatoria pero con preferencia según tipo
        for (int i = 0; i < puntosTotales; i++)
        {
            int rand = random.Next(100);
            
            if (tipoEnemigo == "Guerrero")
            {
                if (rand < 55) puntosVida++;
                else if (rand < 80) puntosEscudo++;
                else puntosDaño++;
            }
            else if (tipoEnemigo == "Mago")
            {
                if (rand < 55) puntosEscudo++;
                else if (rand < 80) puntosVida++;
                else puntosDaño++;
            }
            else // Cazador
            {
                if (rand < 55) puntosDaño++;
                else if (rand < 80) puntosVida++;
                else puntosEscudo++;
            }
        }
        
        // Valores base aleatorios también
        int vidaBase = random.Next(95, 106); // 95-105 base
        int escudoBase = random.Next(45, 56); // 45-55 base
        int dañoBase = random.Next(8, 13); // 8-12 base
        
        vidaJugador2 = vidaBase + puntosVida;
        escudoJugador2 = escudoBase + puntosEscudo;
        dañoJugador2 = dañoBase + puntosDaño;
        
        // Asegurar valores mínimos
        vidaJugador2 = Mathf.Max(50, vidaJugador2);
        escudoJugador2 = Mathf.Max(20, escudoJugador2);
        dañoJugador2 = Mathf.Max(5, dañoJugador2);
    }
    #endregion

    #region Ataque
    #region Ataque - Botones / Modo
    public void BtnAtaque_Click()
    {
        uiManager.MostrarFeedbackAtaque();
        if (ataqueEnProceso || modoAutomatico)
        {
            detenerSolicitado = true; // esperar a terminar animación
            return;
        }
        
        modoAutomatico = true;
        detenerSolicitado = false;
        uiManager.ActualizarBotonAtaque(true);
        IniciarAtaqueAutomatico();
    }
    
    public void BtnDefensa_Click()
    {
        if (escudoJugador1 <= 0)
        {
            uiManager.MostrarMensaje("Necesitas escudo");
            return;
        }
        if (!infoDefensaMostrada)
        {
            uiManager.MostrarMensajeConfirmacion(
                "El ESCUDO te permite reducir 50% del daño del próximo ataque (siempre que tenga durabilidad).",
                () => {
                    infoDefensaMostrada = true;
                    ActivarDefensaYAvanzarTurno();
                },
                () => { }
            );
            return;
        }
        
        ActivarDefensaYAvanzarTurno();
    }
    
    private void IniciarAtaqueAutomatico()
    {
        if (ataqueEnProceso) return;
        
        if (timerAtaque != null)
        {
            StopCoroutine(timerAtaque);
        }
        
        // Iniciar ataque automático inmediatamente
        IniciarAtaqueJugador();
    }
    
    public void BtnAtacar_Click()
    {
        // Ataque manual (se llama cuando se presiona el botón ATACAR en modo manual)
        if (ataqueEnProceso || modoAutomatico) return;
        
        if (timerAtaque != null)
        {
            StopCoroutine(timerAtaque);
        }
        
        IniciarAtaqueJugador();
        
        // Después del ataque, volverá al botón principal ATAQUE (se maneja en IniciarCicloAtaques)
    }
    #endregion

    #region Ataque - Ciclo / Timer / Corrutinas
    private Coroutine timerAtaque;

    private void IniciarAtaqueJugador()
    {
        if (ataqueEnProceso) return;
        
        if (timerAtaque != null)
        {
            StopCoroutine(timerAtaque);
        }
        
        ataqueEnProceso = true;
        
        // Si está en modo manual, deshabilitar el botón ATACAR
        if (!modoAutomatico)
        {
            uiManager.SetBtnAtacarEnabled(false);
        }
        
        IniciarCicloAtaques();
    }
    
    private void DetenerAtaqueAuto()
    {
        ataqueEnProceso = false;
        modoAutomatico = false;
        
        StopAllCoroutines();
        
        if (timerAtaque != null)
        {
            StopCoroutine(timerAtaque);
            timerAtaque = null;
        }
        
        uiManager.OcultarContadorAtaque();
        uiManager.OcultarAnimacion();
        uiManager.ActualizarBotonAtaque(false);
    }
    
    private int AplicarDefensaSiCorresponde(int daño)
    {
        if (!defensaPendiente) return daño;
        defensaPendiente = false;
        return Mathf.CeilToInt(daño * 0.5f);
    }
    
    private void ActivarDefensaYAvanzarTurno()
    {
        defensaPendiente = true;
        uiManager.MostrarFeedbackDefensa();
        
        if (!ataqueEnProceso)
        {
            ataqueEnProceso = true;
            saltarProximoAtaqueJugador = false;
            StartCoroutine(EjecutarAtaqueEnemigoSolo());
            return;
        }
        
        saltarProximoAtaqueJugador = true;
    }
    
    private IEnumerator EjecutarAtaqueEnemigoSolo()
    {
        Debug.Log("[GameManager] EjecutarAtaqueEnemigoSolo iniciado. ataqueEnProceso=" + ataqueEnProceso + ", modoAutomatico=" + modoAutomatico);
        // Ataque del enemigo después de 1.5 segundos
        yield return new WaitForSeconds(1.5f);
        
        if (!ataqueEnProceso) yield break;
        
        int dañoEnemigo = random.Next(Math.Max(1, dañoJugador2 - 3), dañoJugador2 + 4);
        dañoEnemigo = AplicarDefensaSiCorresponde(dañoEnemigo);
        uiManager.MostrarAnimacionAtaque(tipoEnemigo, false, dañoEnemigo, nombreJugador2, nombreJugador1);
        
        int cantidadFramesEnemigo = tipoEnemigo == "Guerrero" ? 8 : tipoEnemigo == "Mago" ? 7 : 9;
        float tiempoAnimacionEnemigo = (cantidadFramesEnemigo * 0.55f) + 1.5f;
        yield return new WaitForSeconds(tiempoAnimacionEnemigo);
        
        if (!ataqueEnProceso) yield break;
        
        int vidaAntesJugador = vidaJugador1;
        int escudoAntesJugador = escudoJugador1;
        AplicarDaño(ref escudoJugador1, ref vidaJugador1, dañoEnemigo);
        int vidaDespuesJugador = vidaJugador1;
        int escudoDespuesJugador = escudoJugador1;
        uiManager.AnimarDañoObjetivo(false, dañoEnemigo, vidaAntesJugador, vidaDespuesJugador, vidaMaximaJugador1, escudoAntesJugador, escudoDespuesJugador, escudoMaximoJugador1);
        
        if (dañoEnemigo > 0 && tipoJugador == "Mago")
        {
            uiManager.MostrarAnimacionHurt(tipoJugador, true);
        }
        uiManager.OcultarAnimacion();
        
        uiManager.ActualizarInfoJugador(vidaJugador1, vidaMaximaJugador1, escudoJugador1, escudoMaximoJugador1,
                                        OroGuardado, dañoJugador1, numeroPelea);
        uiManager.ActualizarDañoEnemigo(dañoJugador2, nombreJugador2);
        ActualizarTextoBotonHuir();
        
        if (vidaJugador1 <= 0)
        {
            ataqueEnProceso = false;
            detenerSolicitado = false;
            if (modoAutomatico)
            {
                uiManager.ActualizarBotonAtaque(false);
            }
            ProcesarGameOver();
            yield break;
        }
        
        if (detenerSolicitado)
        {
            detenerSolicitado = false;
            modoAutomatico = false;
            ataqueEnProceso = false;
            uiManager.ActualizarBotonAtaque(false);
            yield break;
        }
        
        if (modoAutomatico && ataqueEnProceso)
        {
            yield return new WaitForSeconds(1.5f);
            if (modoAutomatico && ataqueEnProceso)
            {
                IniciarCicloAtaques();
            }
        }
        else if (!modoAutomatico)
        {
            ataqueEnProceso = false;
            uiManager.ActualizarBotonAtaque(false);
            uiManager.SetBtnAtacarEnabled(true);
        }
    }

    private void IniciarCicloAtaques()
    {
        if (!ataqueEnProceso) return;
        
        numeroRounds++;
        golpesAcertados++;
        
        if (saltarProximoAtaqueJugador)
        {
            saltarProximoAtaqueJugador = false;
            StartCoroutine(EjecutarAtaqueEnemigoSolo());
            return;
        }
        
        // Ataque del jugador
        int daño = random.Next(Math.Max(1, dañoJugador1 - 3), dañoJugador1 + 4);
        
        uiManager.MostrarAnimacionAtaque(tipoJugador, true, daño, nombreJugador1, nombreJugador2);
        if (tipoJugador == "Guerrero")
        {
            return;
        }
        
        StartCoroutine(EsperarYAplicarDañoJugador(daño));
    }

    private IEnumerator EsperarYAplicarDañoJugador(int daño)
    {
        // Calcular tiempo necesario para la animación completa
        int cantidadFrames = tipoJugador == "Guerrero" ? 8 : tipoJugador == "Mago" ? 7 : 9; // Mago = 7, Cazador = 9
        float tiempoAnimacion = (cantidadFrames * 0.55f) + 1.5f; // tiempo por frame + 1.5 segundos después
        
        // Esperar a que la animación termine completamente
        yield return new WaitForSeconds(tiempoAnimacion);
        
        if (!ataqueEnProceso) yield break; // Si se detuvo, salir
        
        // Aplicar daño (sin iniciar turno del enemigo todavía)
        yield return StartCoroutine(AplicarDañoEnemigo(daño));
        
        if (!ataqueEnProceso) yield break; // Si se detuvo después de aplicar daño (victoria), salir
        
        // Iniciar turno del enemigo después de que termine la animación
        IniciarTurnoEnemigo();
        yield break;
    }

    /// <summary>
    /// Aplica el daño del jugador al enemigo SIN iniciar el turno del enemigo.
    /// El turno del enemigo debe iniciarse manualmente llamando a IniciarTurnoEnemigo() después.
    /// </summary>
    public void IniciarDañoEnemigoDesdeUI(int daño)
    {
        Debug.Log("[GameManager] IniciarDañoEnemigoDesdeUI recibido. daño=" + daño + ", ataqueEnProceso=" + ataqueEnProceso);
        if (!ataqueEnProceso) return;
        StartCoroutine(AplicarDañoEnemigo(daño));
    }

    /// <summary>
    /// Solo aplica el daño al enemigo y actualiza la UI. NO inicia el turno del enemigo.
    /// </summary>
    private IEnumerator AplicarDañoEnemigo(int daño)
    {
        Debug.Log("[GameManager] AplicarDañoEnemigo iniciado. daño=" + daño + ", ataqueEnProceso=" + ataqueEnProceso);
        if (!ataqueEnProceso) yield break;
        int vidaAntes = vidaJugador2;
        int escudoAntes = escudoJugador2;
        AplicarDaño(ref escudoJugador2, ref vidaJugador2, daño);
        int vidaDespues = vidaJugador2;
        int escudoDespues = escudoJugador2;
        uiManager.AnimarDañoObjetivo(true, daño, vidaAntes, vidaDespues, vidaMaximaEnemigo, escudoAntes, escudoDespues, escudoMaximoEnemigo);
        
        if (daño > 0 && tipoEnemigo == "Mago")
        {
            uiManager.MostrarAnimacionHurt(tipoEnemigo, false);
        }
        uiManager.OcultarAnimacion();
        
        uiManager.ActualizarInfoJugador(vidaJugador1, vidaMaximaJugador1, escudoJugador1, escudoMaximoJugador1,
                                        OroGuardado, dañoJugador1, numeroPelea);
        uiManager.ActualizarDañoEnemigo(dañoJugador2, nombreJugador2);
        ActualizarTextoBotonHuir();
        
        if (vidaJugador2 <= 0)
        {
            ataqueEnProceso = false;
            detenerSolicitado = false;
            if (modoAutomatico)
            {
                uiManager.ActualizarBotonAtaque(false);
            }
            ProcesarVictoria();
            yield break;
        }
        
        Debug.Log("[GameManager] ✅ Daño aplicado al enemigo. Esperando a que termine la animación del jugador antes de iniciar turno del enemigo.");
        yield break;
    }

    /// <summary>
    /// Inicia el turno del enemigo después de que el jugador complete su animación de ataque.
    /// Debe llamarse DESPUÉS de que termine la Fase 4 (vuelta) del ataque del caballero.
    /// </summary>
    public void IniciarTurnoEnemigo()
    {
        Debug.Log("[GameManager] IniciarTurnoEnemigo llamado. ataqueEnProceso=" + ataqueEnProceso);
        if (!ataqueEnProceso) return;
        StartCoroutine(ContinuarDespuesDañoJugador());
    }

    /// <summary>
    /// Continúa el flujo después del daño del jugador: espera y luego ejecuta el ataque del enemigo.
    /// </summary>
    private IEnumerator ContinuarDespuesDañoJugador()
    {
        Debug.Log("[GameManager] ContinuarDespuesDañoJugador iniciado. ataqueEnProceso=" + ataqueEnProceso);
        if (!ataqueEnProceso) yield break;
        
        yield return new WaitForSeconds(1.5f);
        
        if (!ataqueEnProceso) yield break;
        
        Debug.Log("[GameManager] A punto de llamar a EjecutarAtaqueEnemigoSolo");
        yield return StartCoroutine(EjecutarAtaqueEnemigoSolo());
        yield break;
    }

    private void IniciarTimerAtaque()
    {
        segundosRestantes = 3; // Cambiar a 3 segundos
        timerAtaque = StartCoroutine(TimerAtaqueCoroutine());
        uiManager.SetBtnAtacarEnabled(true);
    }

    private IEnumerator TimerAtaqueCoroutine()
    {
        while (segundosRestantes > 0 && !ataqueEnProceso)
        {
            uiManager.ActualizarContadorAtaque(segundosRestantes);
            yield return new WaitForSeconds(1f);
            segundosRestantes--;
        }
        
        if (segundosRestantes <= 0 && !ataqueEnProceso)
        {
            uiManager.OcultarContadorAtaque();
            IniciarAtaqueJugador();
        }
    }

    private void AplicarDaño(ref int escudo, ref int vida, int daño)
    {
        if (escudo > 0)
        {
            int dañoEscudo = Math.Min(daño, escudo);
            escudo -= dañoEscudo;
            daño -= dañoEscudo;
        }
        
        if (daño > 0)
        {
            vida -= daño;
            if (vida < 0) vida = 0;
        }
    }
    #endregion
    #endregion

    #region Resultados - Victoria / Derrota
    private void ProcesarVictoria()
    {
        if (numeroPelea >= 40)
        {
            uiManager.MostrarMensajeVictoriaFinal();
            return;
        }
        
        if (revanchaActiva && escudoJugador1AntesRevancha >= 0)
        {
            escudoJugador1 = escudoJugador1AntesRevancha;
            revanchaActiva = false;
            escudoJugador1AntesRevancha = -1;
        }
        
        int oroBase = (numeroPelea * golpesAcertados) + (nivelEnemigo * 2);
        if (oroBase < 1) oroBase = 1;
        int expBase = Mathf.Max(1, nivelEnemigo);
        
        uiManager.MostrarOpcionesDespuesBatalla(oroBase, expBase, !revanchaUsada, (string opcion) => {
            if (opcion == "Matar")
            {
                OroGuardado += oroBase;
                experienciaGuardada += expBase;
                contadorMuertes++;
                uiManager.ActualizarContadoresPostBatalla(contadorClemencias, contadorMuertes);
                uiManager.MostrarMenuPostPelea();
            }
            else if (opcion == "Revancha")
            {
                revanchaUsada = true;
                experienciaGuardada += expBase * 2;
                revanchaActiva = true;
                escudoJugador1AntesRevancha = escudoJugador1;
                escudoJugador1 = 0;
                vidaJugador2 = vidaInicialEnemigo;
                escudoJugador2 = 0;
                escudoMaximoEnemigo = 0;
                IniciarRevancha();
            }
            else if (opcion == "Clemencia")
            {
                OroGuardado += oroBase * 2;
                contadorClemencias++;
                uiManager.ActualizarContadoresPostBatalla(contadorClemencias, contadorMuertes);
                uiManager.MostrarMenuPostPelea();
            }
        });
    }

    private void ProcesarGameOver()
    {
        uiManager.MostrarGameOver(() => {
            ResetearJuego();
            MostrarMenu();
        });
    }
    #endregion

    #region Huir
    public void BtnHuir_Click()
    {
        if (OroGuardado < 10)
        {
            uiManager.MostrarMensaje("No tienes suficiente oro para huir (necesitas 10).");
            return;
        }
        
        OroGuardado -= 10;
        double probabilidad = CalcularProbabilidadHuir();
        
        if (random.NextDouble() < probabilidad)
        {
            uiManager.MostrarMensaje("¡Lograste huir!");
            MostrarMenu();
        }
        else
        {
            uiManager.MostrarMensaje("No pudiste huir. El enemigo te ataca.");
            // El enemigo ataca automáticamente
            StartCoroutine(AtaqueEnemigoPorHuir());
        }
        
        uiManager.ActualizarInfoJugador(vidaJugador1, vidaMaximaJugador1, escudoJugador1, escudoMaximoJugador1,
                                        OroGuardado, dañoJugador1, numeroPelea);
        uiManager.ActualizarDañoEnemigo(dañoJugador2, nombreJugador2);
    }

    private IEnumerator AtaqueEnemigoPorHuir()
    {
        yield return new WaitForSeconds(1f);
        
        int dañoEnemigo = random.Next(Math.Max(1, dañoJugador2 - 3), dañoJugador2 + 4);
        dañoEnemigo = AplicarDefensaSiCorresponde(dañoEnemigo);
        uiManager.MostrarAnimacionAtaque(tipoEnemigo, false, dañoEnemigo, nombreJugador2, nombreJugador1);
        
        yield return new WaitForSeconds(1f);
        
        AplicarDaño(ref escudoJugador1, ref vidaJugador1, dañoEnemigo);
        
        // Mostrar animación hurt en el jugador
        if (dañoEnemigo > 0 && tipoJugador == "Mago")
        {
            uiManager.MostrarAnimacionHurt(tipoJugador, true);
        }
        
        ActualizarInterfaz();
        
        if (vidaJugador1 <= 0)
        {
            ProcesarGameOver();
        }
    }

    private double CalcularProbabilidadHuir()
    {
        double vidaJugadorPorcentaje = (double)vidaJugador1 / vidaMaximaJugador1;
        double vidaEnemigoPorcentaje = (double)vidaJugador2 / vidaMaximaEnemigo;
        
        double diferenciaVida = vidaEnemigoPorcentaje - vidaJugadorPorcentaje;
        
        double probabilidadBase = 0.5;
        probabilidadBase += diferenciaVida * 0.3; // Si el enemigo tiene más vida, es más fácil huir
        
        // Si el enemigo tiene muy poca vida, es muy fácil huir
        if (vidaEnemigoPorcentaje < 0.2)
        {
            probabilidadBase = 0.9;
        }
        
        probabilidadBase = Mathf.Clamp((float)probabilidadBase, 0.01f, 0.99f);
        return probabilidadBase;
    }
    #endregion

    #region UI - Actualizacion
    private void ActualizarInterfaz()
    {
        uiManager.ActualizarVidaJugador(vidaJugador1, vidaMaximaJugador1);
        uiManager.ActualizarEscudoJugador(escudoJugador1, escudoMaximoJugador1);
        uiManager.ActualizarVidaEnemigo(vidaJugador2, vidaMaximaEnemigo);
        uiManager.ActualizarEscudoEnemigo(escudoJugador2, escudoMaximoEnemigo);
        uiManager.ActualizarInfoJugador(vidaJugador1, vidaMaximaJugador1, escudoJugador1, escudoMaximoJugador1, 
                                        OroGuardado, dañoJugador1, numeroPelea);
        uiManager.ActualizarDañoEnemigo(dañoJugador2, nombreJugador2);
    }

    private void ActualizarTextoBotonHuir()
    {
        double probabilidad = CalcularProbabilidadHuir();
        int porcentaje = (int)(probabilidad * 100);
        uiManager.ActualizarBotonHuir(porcentaje);
    }
    #endregion

    #region Paneles - Navegacion
    private void MostrarMenu()
    {
        uiManager.MostrarPanelMenu();
    }
    #endregion

    #region Reset
    public void ResetearJuego()
    {
        OroGuardado = 0;
        experienciaGuardada = 0;
        contadorClemencias = 0;
        contadorMuertes = 0;
        nombreJugador1 = "";
        nombreJugador2 = "";
        vidaJugador1 = 100;
        vidaJugador2 = 100;
        escudoJugador1 = 100;
        escudoJugador2 = 100;
        escudoMaximoJugador1 = 100;
        dañoJugador1 = 0;
        dañoJugador2 = 0;
        // experienciaGuardada y oro se resetean con el juego
        numeroPelea = 0;
        numeroRounds = 0;
        golpesAcertados = 0;
        nivelEnemigo = 1;
        nivelEnemigoActual = 1;
        vidaInicialEnemigo = 0;
        vidaInicialCPU = 0;
        revanchaUsada = false;
        tipoEnemigo = "";
        vidaMaximaJugador1 = 100;
        vidaMaximaEnemigo = 100;
        escudoMaximoEnemigo = 100;
        tipoJugador = "";
        ataqueEnProceso = false;
    }
    #endregion

    #region Getters para UI
    public string GetNombreJugador1() => nombreJugador1;
    public string GetNombreJugador2() => nombreJugador2;
    public string GetTipoJugador() => tipoJugador;
    public string GetTipoEnemigo() => tipoEnemigo;
    public Sprite GetSpriteJugador() 
    {
        if (tipoJugador == "Guerrero") return guerreroJugadorSprite;
        if (tipoJugador == "Mago") return magoJugadorSprite;
        if (tipoJugador == "Cazador") return cazadorJugadorSprite;
        return null;
    }
    public Sprite GetSpriteEnemigo()
    {
        if (tipoEnemigo == "Guerrero") return guerreroEnemigoSprite;
        if (tipoEnemigo == "Mago") return magoEnemigoSprite;
        if (tipoEnemigo == "Cazador") return cazadorEnemigoSprite;
        return null;
    }
    #endregion
    
    #region Editor - Carga automatica de sprites
    // Método para cargar sprites automáticamente (solo se ejecuta en editor)
    private void CargarSpritesAutomaticamente()
    {
        #if UNITY_EDITOR
        // Cargar sprite del Guerrero (usar el primer frame de idle)
        if (guerreroJugadorSprite == null)
        {
            guerreroJugadorSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Personajes/Guerrero/1.png");
        }
        
        if (guerreroEnemigoSprite == null)
        {
            guerreroEnemigoSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Personajes/Guerrero/1.png");
            if (guerreroEnemigoSprite == null && guerreroJugadorSprite != null)
            {
                guerreroEnemigoSprite = guerreroJugadorSprite;
            }
        }
        
        // Cargar sprite del Mago
        if (magoJugadorSprite == null)
        {
            magoJugadorSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Personajes/Mago/1.png");
        }
        
        if (magoEnemigoSprite == null)
        {
            magoEnemigoSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Personajes/Mago/1.png");
            if (magoEnemigoSprite == null && magoJugadorSprite != null)
            {
                magoEnemigoSprite = magoJugadorSprite;
            }
        }
        
        // Cargar sprite del Cazador
        if (cazadorJugadorSprite == null)
        {
            cazadorJugadorSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Personajes/Cazador/1.png");
        }
        
        if (cazadorEnemigoSprite == null)
        {
            cazadorEnemigoSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Personajes/Cazador/1.png");
            if (cazadorEnemigoSprite == null && cazadorJugadorSprite != null)
            {
                cazadorEnemigoSprite = cazadorJugadorSprite;
            }
        }
        #endif
        // En el build, los sprites deben estar asignados manualmente en el Inspector
    }
    #endregion
}


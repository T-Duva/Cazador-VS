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
    private string nombreJugador1 = "";
    private string nombreJugador2 = "";
    private int vidaJugador1 = 100;
    private int vidaJugador2 = 100;
    private int escudoJugador1 = 100;
    private int escudoJugador2 = 100;
    private int escudoMaximoJugador1 = 100;
    private int dañoJugador1 = 0;
    private int dañoJugador2 = 0;
    private bool dobleOro = false;
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
    private bool modoAutomatico = true; // Modo automático activado por defecto
    private int puntosSkillDisponibles = 0;
    private string tipoEnemigo = "";
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
        
        // Configurar modo de ataque
        modoAutomatico = true;
        uiManager.MostrarBotonesAtaque(true); // Mostrar automático, ocultar manual
        
        // No iniciar automáticamente, esperar a que el jugador presione AUTOMÁTICO
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
        // Alternar entre mostrar/ocultar las opciones Manual/Automático
        uiManager.ToggleOpcionesAtaque();
    }
    
    public void BtnAutomatico_Click()
    {
        // Si el botón dice DETENER, detener el ataque
        if (uiManager.EsBotonDetener())
        {
            BtnDetener_Click();
            return;
        }
        
        // Si dice AUTOMÁTICO, iniciar ataque automático
        modoAutomatico = true;
        
        // Ocultar opciones y el botón principal, mostrar solo DETENER
        uiManager.OcultarOpcionesYMostrarDetener();
        
        // Transformar el botón automático en DETENER y mostrarlo arriba
        uiManager.MostrarBotonDetener();
        IniciarAtaqueAutomatico();
    }
    
    public void BtnDetener_Click()
    {
        // Detener el ataque automático inmediatamente
        ataqueEnProceso = false;
        modoAutomatico = false;
        
        // Detener todas las corrutinas de ataque
        StopAllCoroutines();
        
        // Limpiar el timer
        if (timerAtaque != null)
        {
            StopCoroutine(timerAtaque);
            timerAtaque = null;
        }
        
        uiManager.OcultarContadorAtaque();
        uiManager.OcultarAnimacion();
        
        // Volver al botón principal ATAQUE (no mostrar opciones, solo el botón principal)
        uiManager.VolverABotonAtaque();
    }
    
    public void BtnManual_Click()
    {
        modoAutomatico = false;
        
        // Detener cualquier ataque en curso
        if (timerAtaque != null)
        {
            StopCoroutine(timerAtaque);
            timerAtaque = null;
        }
        uiManager.OcultarContadorAtaque();
        ataqueEnProceso = false;
        
        // Ocultar opciones y volver al botón principal (que ahora será ATACAR en modo manual)
        uiManager.MostrarModoManual();
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

    private void IniciarCicloAtaques()
    {
        if (!ataqueEnProceso) return;
        
        numeroRounds++;
        golpesAcertados++;
        
        // Ataque del jugador
        int daño = random.Next(Math.Max(1, dañoJugador1 - 3), dañoJugador1 + 4);
        
        uiManager.MostrarAnimacionAtaque(tipoJugador, true, daño, nombreJugador1, nombreJugador2);
        
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
        
        AplicarDaño(ref escudoJugador2, ref vidaJugador2, daño);
        
        // Mostrar animación hurt en el enemigo
        if (daño > 0 && tipoEnemigo == "Mago")
        {
            uiManager.MostrarAnimacionHurt(tipoEnemigo, false);
        }
        
        ActualizarInterfaz();
        ActualizarTextoBotonHuir();
        
        // NO llamar OcultarAnimacion aquí - la animación se oculta automáticamente al terminar
        
        if (vidaJugador2 <= 0)
        {
            ataqueEnProceso = false;
            if (modoAutomatico)
            {
                uiManager.TransformarDetenerEnAutomatico();
            }
            ProcesarVictoria();
            yield break;
        }
        
        if (!ataqueEnProceso) yield break; // Si se detuvo, salir
        
        // Ataque del enemigo después de 3 segundos
        yield return new WaitForSeconds(1.5f);
        
        if (!ataqueEnProceso) yield break; // Si se detuvo, salir
        
        int dañoEnemigo = random.Next(Math.Max(1, dañoJugador2 - 3), dañoJugador2 + 4);
        uiManager.MostrarAnimacionAtaque(tipoEnemigo, false, dañoEnemigo, nombreJugador2, nombreJugador1);
        
        // Calcular tiempo necesario para la animación completa del enemigo
        int cantidadFramesEnemigo = tipoEnemigo == "Guerrero" ? 8 : tipoEnemigo == "Mago" ? 7 : 9; // Mago = 7, Cazador = 9
        float tiempoAnimacionEnemigo = (cantidadFramesEnemigo * 0.55f) + 1.5f; // tiempo por frame + 1.5 segundos después
        
        // Esperar a que la animación termine completamente
        yield return new WaitForSeconds(tiempoAnimacionEnemigo);
        
        if (!ataqueEnProceso) yield break; // Si se detuvo, salir
        
        AplicarDaño(ref escudoJugador1, ref vidaJugador1, dañoEnemigo);
        
        // Mostrar animación hurt en el jugador
        if (dañoEnemigo > 0 && tipoJugador == "Mago")
        {
            uiManager.MostrarAnimacionHurt(tipoJugador, true);
        }
        
        ActualizarInterfaz();
        ActualizarTextoBotonHuir();
        
        // NO llamar OcultarAnimacion aquí - la animación se oculta automáticamente al terminar
        
        if (vidaJugador1 <= 0)
        {
            ataqueEnProceso = false;
            if (modoAutomatico)
            {
                uiManager.TransformarDetenerEnAutomatico();
            }
            ProcesarGameOver();
            yield break;
        }
        
        if (!ataqueEnProceso) yield break; // Si se detuvo, salir
        
        // Continuar ciclo solo si está en modo automático
        if (modoAutomatico && ataqueEnProceso)
        {
            yield return new WaitForSeconds(1.5f);
            if (modoAutomatico && ataqueEnProceso)
            {
                IniciarCicloAtaques(); // Continuar automáticamente
            }
        }
        else if (!modoAutomatico)
        {
            // En modo manual, detener y volver al botón ATAQUE principal
            ataqueEnProceso = false;
            // IMPORTANTE: Resetear modoAutomatico para permitir elegir de nuevo
            modoAutomatico = false; // Ya está en false, pero lo dejamos explícito
            uiManager.VolverABotonAtaque();
            // Habilitar el botón para que pueda volver a atacar o cambiar de modo
            uiManager.SetBtnAtacarEnabled(true);
        }
        else
        {
            // Si terminó el ataque, reiniciar timer
            if (modoAutomatico)
            {
                uiManager.TransformarDetenerEnAutomatico();
                uiManager.MostrarOpcionesAtaque();
            }
            IniciarTimerAtaque();
        }
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
        
        // Calcular oro ganado
        int oroGanado = (numeroPelea * golpesAcertados) + (nivelEnemigo * 2);
        if (dobleOro) oroGanado *= 2;
        if (oroGanado < 1) oroGanado = 1;
        
        OroGuardado += oroGanado;
        dobleOro = false;
        
        // Verificar si hay puntos de skill
        if (numeroPelea % 5 == 0)
        {
            puntosSkillDisponibles += 10;
        }
        
        uiManager.MostrarOpcionesDespuesBatalla(oroGanado, !revanchaUsada, (string opcion) => {
            if (opcion == "Siguiente")
            {
                dobleOro = true;
                IniciarNuevaBatalla();
            }
            else if (opcion == "Revancha")
            {
                revanchaUsada = true;
                vidaJugador2 = vidaInicialEnemigo;
                escudoJugador2 = 0; // Sin escudo en revancha
                escudoMaximoEnemigo = 0; // Asegurar que el escudo máximo también sea 0
                numeroPelea++;
                IniciarNuevaBatalla();
            }
            else if (opcion == "Descanso")
            {
                MostrarDescanso();
            }
            else if (opcion == "Abandonar")
            {
                ResetearJuego();
                MostrarMenu();
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
            MostrarDescanso();
        }
        else
        {
            uiManager.MostrarMensaje("No pudiste huir. El enemigo te ataca.");
            // El enemigo ataca automáticamente
            StartCoroutine(AtaqueEnemigoPorHuir());
        }
        
        ActualizarInterfaz();
    }

    private IEnumerator AtaqueEnemigoPorHuir()
    {
        yield return new WaitForSeconds(1f);
        
        int dañoEnemigo = random.Next(Math.Max(1, dañoJugador2 - 3), dañoJugador2 + 4);
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

    private void MostrarDescanso()
    {
        uiManager.MostrarPanelDescanso();
    }

    public void BtnDescansoBatalla_Click()
    {
        IniciarNuevaBatalla();
    }

    public void BtnDescansoIrDescanso_Click()
    {
        vidaJugador1 = Mathf.Min(vidaJugador1 + (int)(vidaMaximaJugador1 * 0.1f), vidaMaximaJugador1);
        ActualizarInterfaz();
        MostrarTienda();
    }

    private void MostrarTienda()
    {
        uiManager.MostrarPanelTienda(OroGuardado);
    }
    #endregion

    #region Botones - Descanso / Tienda
    public void BtnTiendaPocion_Click()
    {
        if (OroGuardado >= 50)
        {
            OroGuardado -= 50;
            vidaJugador1 = Mathf.Min(vidaJugador1 + 50, vidaMaximaJugador1);
            ActualizarInterfaz();
            uiManager.MostrarPanelTienda(OroGuardado);
        }
    }

    public void BtnTiendaArmadura_Click()
    {
        if (OroGuardado >= 75)
        {
            OroGuardado -= 75;
            escudoJugador1 = escudoMaximoJugador1;
            ActualizarInterfaz();
            uiManager.MostrarPanelTienda(OroGuardado);
        }
    }
    #endregion

    #region Reset
    public void ResetearJuego()
    {
        OroGuardado = 0;
        nombreJugador1 = "";
        nombreJugador2 = "";
        vidaJugador1 = 100;
        vidaJugador2 = 100;
        escudoJugador1 = 100;
        escudoJugador2 = 100;
        escudoMaximoJugador1 = 100;
        dañoJugador1 = 0;
        dañoJugador2 = 0;
        dobleOro = false;
        numeroPelea = 0;
        numeroRounds = 0;
        golpesAcertados = 0;
        nivelEnemigo = 1;
        nivelEnemigoActual = 1;
        vidaInicialEnemigo = 0;
        vidaInicialCPU = 0;
        revanchaUsada = false;
        puntosSkillDisponibles = 0;
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


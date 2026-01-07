using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Componente que ajusta automáticamente el ancho de una StatCard basado en el contenido del texto.
/// No usa ContentSizeFitter para evitar loops con LayoutGroups.
/// </summary>
public class StatCardAutoWidth : MonoBehaviour
{
    [Header("Referencias")]
    public LayoutElement layoutElement;
    public Text label; // Usando Text normal ya que TMP no está disponible
    
    [Header("Parámetros")]
    [SerializeField] private float minWidth = 32f;
    [SerializeField] private float horizontalPadding = 16f; // Padding total izq+der
    [SerializeField] private float extraSafety = 2f;
    
    private float lastWidth = -1f;
    
    private void Start()
    {
        // Auto-buscar componentes si no están asignados (Start se ejecuta después de Awake, cuando ya se asignaron las referencias)
        if (layoutElement == null)
            layoutElement = GetComponent<LayoutElement>();
        
        if (label == null)
        {
            // Buscar el componente Text en los hijos (puede estar en cualquier hijo)
            label = GetComponentInChildren<Text>(true); // true = incluir inactivos
            
            // Si aún no se encuentra, buscar manualmente en todos los hijos
            if (label == null)
            {
                Transform[] hijos = GetComponentsInChildren<Transform>(true);
                foreach (Transform hijo in hijos)
                {
                    Text texto = hijo.GetComponent<Text>();
                    if (texto != null)
                    {
                        label = texto;
                        break;
                    }
                }
            }
        }
        
        if (layoutElement == null)
        {
            Debug.LogError($"StatCardAutoWidth en {gameObject.name}: No se encontró LayoutElement");
            enabled = false;
            return;
        }
        
        if (label == null)
        {
            Debug.LogError($"StatCardAutoWidth en {gameObject.name}: No se encontró Text component");
            enabled = false;
            return;
        }
    }
    
    /// <summary>
    /// Establece el texto y recalcula el ancho de la tarjeta.
    /// </summary>
    public void SetText(string text)
    {
        if (label == null || layoutElement == null) return;
        
        // Establecer el texto
        label.text = text;
        
        // Forzar actualización del texto para calcular preferredWidth correctamente
        // Usamos Canvas.ForceUpdateCanvases() solo una vez antes de medir
        Canvas.ForceUpdateCanvases();
        
        // Calcular el ancho preferido del texto
        float anchoTexto = label.preferredWidth;
        
        // Calcular ancho total: máximo entre el ancho del texto + padding y el ancho mínimo
        float targetWidth = Mathf.Max(minWidth, anchoTexto + horizontalPadding + extraSafety);
        
        // Solo actualizar si cambió significativamente (tolerancia 0.1)
        if (Mathf.Abs(targetWidth - lastWidth) > 0.1f)
        {
            layoutElement.preferredWidth = targetWidth;
            layoutElement.flexibleWidth = 0f;
            lastWidth = targetWidth;
            
            // Marcar el layout para rebuild (pero no forzar inmediatamente)
            UnityEngine.UI.LayoutRebuilder.MarkLayoutForRebuild(GetComponent<RectTransform>());
        }
    }
    
    /// <summary>
    /// Obtiene el ancho actual preferido.
    /// </summary>
    public float GetPreferredWidth()
    {
        return layoutElement != null ? layoutElement.preferredWidth : minWidth;
    }
}


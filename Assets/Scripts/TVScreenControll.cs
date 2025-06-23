// TVScreenController.cs - VERSÃO COM AJUSTE MANUAL DE TILING E OFFSET
using UnityEngine;
using System.Collections;

public class TVScreenController : MonoBehaviour
{
    [Header("Configuração da Tela")]
    public Renderer screenRenderer;
    [Tooltip("O índice do material da tela na lista de materiais do Renderer. Começa em 0.")]
    public int screenMaterialIndex = 0;

    [Header("Imagens")]
    public Texture[] screenImages;

    [Header("Configuração de Tempo")]
    public float changeInterval = 3.0f;

    // --- NOVOS CONTROLOS MANUAIS ---
    [Header("Ajuste Manual da Textura")]
    [Tooltip("Ajuste a escala (tiling) da textura. Valores menores 'aumentam o zoom' da imagem.")]
    public Vector2 manualTiling = Vector2.one; // Começa com (1, 1) por padrão

    [Tooltip("Ajuste a posição (offset) da textura para a centrar.")]
    public Vector2 manualOffset = Vector2.zero; // Começa com (0, 0) por padrão
    // --- FIM DOS NOVOS CONTROLOS ---

    private int currentImageIndex = 0;
    private Material screenMaterial;

    void Start()
    {
        if (screenRenderer == null || screenImages == null || screenImages.Length == 0)
        {
            Debug.LogError("Renderer ou lista de imagens não configurados!", this);
            this.enabled = false;
            return;
        }

        if (screenMaterialIndex < 0 || screenMaterialIndex >= screenRenderer.materials.Length)
        {
            Debug.LogError($"Índice de material ({screenMaterialIndex}) inválido!", this);
            this.enabled = false;
            return;
        }

        // Pega uma instância do material para não alterar o ficheiro original do projeto
        screenMaterial = screenRenderer.materials[screenMaterialIndex];

        StartCoroutine(CycleImages());
    }

    private IEnumerator CycleImages()
    {
        while (true)
        {
            Texture currentTexture = screenImages[currentImageIndex];

            screenMaterial.mainTexture = currentTexture;
            screenMaterial.SetTexture("_EmissionMap", currentTexture);

            // --- APLICA OS AJUSTES MANUAIS ---
            // Em vez de forçar para (1,1), usamos os valores do Inspector.
            screenMaterial.mainTextureScale = manualTiling;
            screenMaterial.mainTextureOffset = manualOffset;
            
            // Aplica também ao mapa de emissão para consistência
            screenMaterial.SetTextureScale("_EmissionMap", manualTiling);
            screenMaterial.SetTextureOffset("_EmissionMap", manualOffset);
            // --- FIM DA APLICAÇÃO ---

            currentImageIndex = (currentImageIndex + 1) % screenImages.Length;
            yield return new WaitForSeconds(changeInterval);
        }
    }
}
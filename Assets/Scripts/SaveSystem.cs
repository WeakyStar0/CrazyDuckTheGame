using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;


[System.Serializable]
class SaveData
{
    public List<NivelData> niveis = new List<NivelData>();
    public List<string> tpDesbloqueados = new List<string>();
}

[System.Serializable]
class NivelData
{
    public string nomeNivel;
    public List<string> coletaveisApanhados = new List<string>();
}

public class SaveSystem : MonoBehaviour
{
    private string path;

    private void Awake()
    {
        path = Application.persistentDataPath + "/save.json";
    }

    public void SaveGame()
    {
        SaveData save = new SaveData();
        save.tpDesbloqueados = GameManager.Instance.tpDesbloqueados;

        foreach (var nivel in GameManager.Instance.coletaveisApanhados)
        {
            NivelData nivelData = new NivelData
            {
                nomeNivel = nivel.Key,
                coletaveisApanhados = nivel.Value
            };
            save.niveis.Add(nivelData);
        }

        string json = JsonUtility.ToJson(save, true);
        File.WriteAllText(path, json);
        Debug.Log("Jogo guardado.");
    }

    public void LoadGame()
    {
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveData save = JsonUtility.FromJson<SaveData>(json);

            GameManager.Instance.tpDesbloqueados = save.tpDesbloqueados;

            GameManager.Instance.coletaveisApanhados.Clear();
            foreach (var nivel in save.niveis)
            {
                GameManager.Instance.coletaveisApanhados[nivel.nomeNivel] = nivel.coletaveisApanhados;
            }

            Debug.Log("Jogo carregado.");
        }
    }
}

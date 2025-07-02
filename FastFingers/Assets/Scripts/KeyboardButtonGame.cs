using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class KeyboardButtonGame : MonoBehaviour
{
    public GameObject[] ArrowsMod;
    public GameObject[] LetterMod;
    public GameObject[] NumbersMod;
    public GameObject[] AllButtons;

    public Material blackMaterial; // Dışarıdan atanacak Black materyali
    public Color blackTextColor = Color.black; // Text için siyah renk
    private Dictionary<GameObject, Material> originalMaterials = new Dictionary<GameObject, Material>();

    public Color activeEmissionColor;
    public float activeEmissionIntensity;
    public Color inactiveEmissionColor;

    private int activeButtonIndex = -1;
    private bool gameActive = true;
    private GameObject[] currentButtons;
    private Material[] buttonMaterials;

    public enum GameMode { Numbers, Letters, Arrows, All }
    public GameMode currentMode = GameMode.All;

    void Start()
    {
        SaveOriginalMaterials();
        SetGameMode(currentMode);
        StartCoroutine(StartNewRound());
    }

    void SaveOriginalMaterials()
    {
        foreach (GameObject button in AllButtons)
        {
            SaveMaterialRecursively(button);
        }
    }

    void SaveMaterialRecursively(GameObject obj)
    {
        Renderer objRenderer = obj.GetComponent<Renderer>();
        if (objRenderer != null && !originalMaterials.ContainsKey(obj))
        {
            originalMaterials[obj] = objRenderer.material;
        }

        foreach (Transform child in obj.transform)
        {
            SaveMaterialRecursively(child.gameObject);
        }
    }

    public void SetGameMode(GameMode mode)
    {
        currentMode = mode;

        switch (mode)
        {
            case GameMode.Numbers:
                currentButtons = NumbersMod;
                break;
            case GameMode.Letters:
                currentButtons = LetterMod;
                break;
            case GameMode.Arrows:
                currentButtons = ArrowsMod;
                break;
            case GameMode.All:
                currentButtons = AllButtons;
                break;
        }

        foreach (GameObject button in AllButtons)
        {
            if (!ArrayContains(currentButtons, button))
            {
                SetMaterialRecursively(button, blackMaterial);
            }
            else
            {
                RestoreMaterialRecursively(button);
            }
        }

        buttonMaterials = new Material[currentButtons.Length];
        for (int i = 0; i < currentButtons.Length; i++)
        {
            buttonMaterials[i] = currentButtons[i].GetComponent<Renderer>().material;
        }
    }

    void SetMaterialRecursively(GameObject obj, Material material)
    {
        Renderer objRenderer = obj.GetComponent<Renderer>();
        if (objRenderer != null)
        {
            objRenderer.material = material;
        }

        foreach (Transform child in obj.transform)
        {
            SetMaterialRecursively(child.gameObject, material);
        }
    }

    void RestoreMaterialRecursively(GameObject obj)
    {
        if (originalMaterials.ContainsKey(obj))
        {
            Renderer objRenderer = obj.GetComponent<Renderer>();
            if (objRenderer != null)
            {
                objRenderer.material = originalMaterials[obj];
            }
        }

        foreach (Transform child in obj.transform)
        {
            RestoreMaterialRecursively(child.gameObject);
        }
    }

    private bool ArrayContains(GameObject[] array, GameObject item)
    {
        foreach (GameObject obj in array)
        {
            if (obj == item) return true;
        }
        return false;
    }

    IEnumerator StartNewRound()
    {
        while (gameActive)
        {
            if (currentButtons == null || currentButtons.Length == 0)
            {
                yield break;
            }

            activeButtonIndex = Random.Range(0, currentButtons.Length);
            Material buttonMaterial = buttonMaterials[activeButtonIndex];

            buttonMaterial.SetColor("_EmissionColor", activeEmissionColor * activeEmissionIntensity);
            buttonMaterial.EnableKeyword("_EMISSION");

            yield return StartCoroutine(WaitForKeyPress(currentButtons[activeButtonIndex].name));

            if (!gameActive)
                yield break;

            buttonMaterial.SetColor("_EmissionColor", inactiveEmissionColor);

            yield return new WaitForSeconds(0.5f);
        }
    }
    private Dictionary<string, KeyCode> specialKeys = new Dictionary<string, KeyCode>()
    {
        { "space", KeyCode.Space }, { "enter", KeyCode.Return }, { "backspace", KeyCode.Backspace },
        { "tab", KeyCode.Tab }, { "escape", KeyCode.Escape },

        { "leftarrow", KeyCode.LeftArrow }, { "rightarrow", KeyCode.RightArrow },
        { "uparrow", KeyCode.UpArrow }, { "downarrow", KeyCode.DownArrow },

        { "0", KeyCode.Alpha0 }, { "1", KeyCode.Alpha1 }, { "2", KeyCode.Alpha2 },
        { "3", KeyCode.Alpha3 }, { "4", KeyCode.Alpha4 }, { "5", KeyCode.Alpha5 },
        { "6", KeyCode.Alpha6 }, { "7", KeyCode.Alpha7 }, { "8", KeyCode.Alpha8 },
        { "9", KeyCode.Alpha9 },

        { "[", KeyCode.LeftBracket }, { "]", KeyCode.RightBracket }, { ";", KeyCode.Semicolon },
        { "'", KeyCode.Quote }, { ",", KeyCode.Comma }, { ".", KeyCode.Period },
        { "/", KeyCode.Slash }, { "-", KeyCode.Minus }, { "=", KeyCode.Equals }
    };
    IEnumerator WaitForKeyPress(string correctKey)
    {
        float timer = 3f;
        while (timer > 0)
        {
            KeyCode keyCode;

            // Eğer özel tuşlardan biri ise Dictionary'den çek
            if (specialKeys.TryGetValue(correctKey.ToLower(), out keyCode))
            {
                if (Input.GetKeyDown(keyCode))
                {
                    yield break; // Doğru tuşa basıldı, devam et
                }
            }
            else
            {
                // Sayı olup olmadığını kontrol et
                if (correctKey.Length == 1 && char.IsDigit(correctKey[0]))
                {
                    keyCode = (KeyCode)System.Enum.Parse(typeof(KeyCode), "Alpha" + correctKey);
                }
                else
                {
                    keyCode = (KeyCode)System.Enum.Parse(typeof(KeyCode), correctKey.ToUpper());
                }

                if (Input.GetKeyDown(keyCode))
                {
                    yield break; // Doğru tuşa basıldı
                }
            }

            timer -= Time.deltaTime;
            yield return null;
        }

        Debug.Log("Yanlış! Oyun bitti.");
        gameActive = false;
    }
}


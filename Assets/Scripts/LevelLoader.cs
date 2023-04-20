using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

    public class LevelLoader : MonoBehaviour
    {
        [SerializeField]
        private Slider progressBar;

        [SerializeField]
        private TextMeshProUGUI errorText;

        [SerializeField]
        private CanvasGroup fadeCanvasGroup;

        [SerializeField]
        private float fakeDurationLength = 0.15f;

        private static int sceneToLoad;

        static LevelLoader()
        {
            sceneToLoad = -1;
        }

        private void Awake()
        {
            errorText.text = "";
            if (sceneToLoad < 0)
            {
                errorText.text = "Error finding scene to load!";
                return;
            }
            StartCoroutine(LoadScene_Coroutine());
        }

        public static void LoadScene(int sceneId)
        {
            sceneToLoad = sceneId;
            SceneManager.LoadScene("LoadingScene");
        }

        public static void LoadScene(string sceneName)
        {
            LoadScene(SceneUtility.GetBuildIndexByScenePath(sceneName));
        }

        private IEnumerator LoadScene_Coroutine()
        {
            progressBar.value = 0;
            yield return new WaitForSeconds(fakeDurationLength);
            AsyncOperation newScene = SceneManager.LoadSceneAsync(sceneToLoad);
            newScene.completed += (_) =>
            {
                sceneToLoad = -1;
            };
            while (!newScene.isDone)
            {
                progressBar.value = Mathf.Clamp01(newScene.progress / 0.9f);
                if (progressBar.value >= 0.9f)
                {
                    progressBar.value = 1;
                    break;
                }
                yield return null;
            }
        }
    }

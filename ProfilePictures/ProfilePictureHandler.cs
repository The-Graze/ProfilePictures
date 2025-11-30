using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace ProfilePictures
{
    public class ProfilePictureHandler : MonoBehaviour
    {
        private string? _cachedURL;
        private Texture2D? _playerPicture;

        private Material? _runtimeMaterial;
        private Image? _cloneImage;
        private GameObject? _cloneGo;

        private GorillaPlayerScoreboardLine? _line;
        private bool _initialized;

        private void Awake()
        {
            TryInitialize();
        }

        private void OnEnable()
        {
            TryInitialize();
            Refresh();
        }

        private void TryInitialize()
        {
            if (_initialized) return;

            if (!_line && !TryGetComponent(out _line))
                return;
            
            _cloneGo = Instantiate(_line?.playerSwatch.gameObject, _line?.playerSwatch.transform.parent);
            _cloneImage = _cloneGo?.GetComponent<Image>();
            _cloneGo?.SetActive(false);
            
            _runtimeMaterial = new Material(_cloneImage?.material);

            _initialized = true;
        }

        public void Refresh()
        {
            if (!_initialized)
            {
                TryInitialize();
                if (!_initialized) return;
            }

            var props = _line?.linePlayer.GetPlayerRef().CustomProperties;

            if (!props!.TryGetValue(Constants.PropName, out var val))
            {
                _cloneGo?.SetActive(false);
                return;
            }

            var newUrl = val as string;
            if (string.IsNullOrEmpty(newUrl))
            {
                _cloneGo?.SetActive(false);
                return;
            }
            
            if (_cachedURL == newUrl)
            {
                ApplyTexture();
                return;
            }
            
            _cachedURL = newUrl;
            _playerPicture = null;
            _cloneGo?.SetActive(false);

            StartCoroutine(DownloadImage(newUrl));
        }

        private IEnumerator DownloadImage(string url)
        {
            using var req = UnityWebRequestTexture.GetTexture(url);
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                _playerPicture = DownloadHandlerTexture.GetContent(req);
                _playerPicture.filterMode = FilterMode.Point;
                ApplyTexture();
            }
            else
            {
                _cloneGo?.SetActive(false);
            }
        }

        private void ApplyTexture()
        {
            if (!_playerPicture) return;

            _cloneGo?.SetActive(false);
            _cloneImage?.color = Color.white;

            _runtimeMaterial?.mainTexture = _playerPicture;
            _cloneImage?.material = _runtimeMaterial;

            _cloneGo?.SetActive(true);
        }

        private void OnDisable()
        {
            if (_cloneGo) _cloneGo.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_runtimeMaterial) Destroy(_runtimeMaterial);
            if (_cloneGo) Destroy(_cloneGo);
        }
    }
}

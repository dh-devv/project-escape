using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Carten
{
    public class LastSparkApiClient : MonoBehaviour
    {
        [SerializeField]
        private string baseUrl = "http://localhost:8080/api/v1";

        [SerializeField]
        private int timeoutSeconds = 10;

        public string BaseUrl => (baseUrl ?? string.Empty).TrimEnd('/');

        public void CreateUser(
            string username,
            Action<UserResponse> onSuccess,
            Action<string> onError = null)
        {
            UserRequest request = new UserRequest { username = username };

            StartCoroutine(SendRequest(
                UnityWebRequest.kHttpVerbPOST,
                "/users",
                JsonUtility.ToJson(request),
                response => DecodeResponse(response, JsonUtility.FromJson<UserResponse>,
                    value => value != null && value.userId > 0, onSuccess, onError),
                onError
            ));
        }

        public void SaveScore(
            int userId,
            int bossId,
            double clearTime,
            int score,
            int maxPhase,
            Action<ScoreResponse> onSuccess,
            Action<string> onError = null)
        {
            ScoreRequest request = new ScoreRequest
            {
                userId = userId,
                bossId = bossId,
                clearTime = clearTime,
                score = score,
                maxPhase = maxPhase
            };

            StartCoroutine(SendRequest(
                UnityWebRequest.kHttpVerbPOST,
                "/scores",
                JsonUtility.ToJson(request),
                response => DecodeResponse(response, JsonUtility.FromJson<ScoreResponse>,
                    value => value != null && value.recordId > 0, onSuccess, onError),
                onError
            ));
        }

        public void GetRanking(
            int limit,
            Action<ScoreResponse[]> onSuccess,
            Action<string> onError = null)
        {
            StartCoroutine(SendRequest(
                UnityWebRequest.kHttpVerbGET,
                "/ranks?limit=" + Mathf.Clamp(limit, 1, 100),
                null,
                response => DecodeResponse(response, JsonHelper.FromJson<ScoreResponse>,
                    value => value != null && Array.TrueForAll(value, item => item != null && item.recordId > 0),
                    onSuccess, onError),
                onError
            ));
        }

        public void GetBestScore(
            int userId,
            Action<ScoreResponse> onSuccess,
            Action<string> onError = null)
        {
            StartCoroutine(SendRequest(
                UnityWebRequest.kHttpVerbGET,
                "/users/" + userId + "/best",
                null,
                response => DecodeResponse(response, JsonUtility.FromJson<ScoreResponse>,
                    value => value != null && value.recordId > 0, onSuccess, onError),
                onError
            ));
        }

        private IEnumerator SendRequest(
            string method,
            string path,
            string json,
            Action<string> onSuccess,
            Action<string> onError)
        {
            if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out Uri address) ||
                (address.Scheme != Uri.UriSchemeHttp && address.Scheme != Uri.UriSchemeHttps))
            {
                onError?.Invoke("API URL must be an absolute HTTP or HTTPS URL");
                yield break;
            }
            using UnityWebRequest request = new UnityWebRequest(BaseUrl + path, method);

            if (json != null)
            {
                request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
                request.SetRequestHeader("Content-Type", "application/json");
            }
            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = Mathf.Max(1, timeoutSeconds);

            UnityWebRequestAsyncOperation operation = null;
            string startError = null;
            try { operation = request.SendWebRequest(); }
            catch (Exception error) when (error is InvalidOperationException || error is ArgumentException)
            {
                startError = error.Message;
            }
            if (operation == null)
            {
                onError?.Invoke("API request could not start: " + startError);
                yield break;
            }
            yield return operation;
            HandleResponse(request, onSuccess, onError);
        }

        private static void DecodeResponse<T>(string json, Func<string, T> decode,
            Func<T, bool> isValid, Action<T> onSuccess, Action<string> onError)
        {
            T value;
            try
            {
                value = decode(json);
                if (!isValid(value)) throw new ArgumentException("Missing response fields");
            }
            catch (ArgumentException)
            {
                onError?.Invoke("API returned an invalid JSON response");
                return;
            }
            onSuccess?.Invoke(value);
        }

        private static void HandleResponse(
            UnityWebRequest request,
            Action<string> onSuccess,
            Action<string> onError)
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke(request.downloadHandler.text);
                return;
            }

            string message = string.IsNullOrEmpty(request.error)
                ? "API request failed with status " + request.responseCode
                : request.error;
            try
            {
                ApiError error = JsonUtility.FromJson<ApiError>(request.downloadHandler.text);
                if (error != null && !string.IsNullOrWhiteSpace(error.message)) message = error.message;
            }
            catch (ArgumentException) { /* A proxy may return an HTML error page. */ }
            onError?.Invoke(message);
        }

        [Serializable]
        private class ApiError { public string message; }
    }

    [Serializable]
    public class UserRequest
    {
        public string username;
    }

    [Serializable]
    public class UserResponse
    {
        public int userId;
        public string username;
    }

    [Serializable]
    public class ScoreRequest
    {
        public int userId;
        public int bossId;
        public double clearTime;
        public int score;
        public int maxPhase;
    }

    [Serializable]
    public class ScoreResponse
    {
        public int recordId;
        public int userId;
        public int bossId;
        public double clearTime;
        public int score;
        public int maxPhase;
    }

    public static class JsonHelper
    {
        [Serializable]
        private class Wrapper<T>
        {
            public T[] items;
        }

        public static T[] FromJson<T>(string json)
        {
            string wrappedJson = "{\"items\":" + json + "}";
            return JsonUtility.FromJson<Wrapper<T>>(wrappedJson).items;
        }
    }
}

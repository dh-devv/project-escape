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

        public string BaseUrl => baseUrl.TrimEnd('/');

        public void CreateUser(
            string username,
            Action<UserResponse> onSuccess,
            Action<string> onError = null)
        {
            UserRequest request = new UserRequest
            {
                username = username
            };

            StartCoroutine(SendJsonRequest(
                UnityWebRequest.kHttpVerbPOST,
                "/users",
                JsonUtility.ToJson(request),
                response => onSuccess?.Invoke(JsonUtility.FromJson<UserResponse>(response)),
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

            StartCoroutine(SendJsonRequest(
                UnityWebRequest.kHttpVerbPOST,
                "/scores",
                JsonUtility.ToJson(request),
                response => onSuccess?.Invoke(JsonUtility.FromJson<ScoreResponse>(response)),
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
                response => onSuccess?.Invoke(JsonHelper.FromJson<ScoreResponse>(response)),
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
                response => onSuccess?.Invoke(JsonUtility.FromJson<ScoreResponse>(response)),
                onError
            ));
        }

        private IEnumerator SendJsonRequest(
            string method,
            string path,
            string json,
            Action<string> onSuccess,
            Action<string> onError)
        {
            using UnityWebRequest request = new UnityWebRequest(
                BaseUrl + path,
                method
            );

            byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = timeoutSeconds;

            yield return request.SendWebRequest();
            HandleResponse(request, onSuccess, onError);
        }

        private IEnumerator SendRequest(
            string method,
            string path,
            string json,
            Action<string> onSuccess,
            Action<string> onError)
        {
            using UnityWebRequest request = new UnityWebRequest(
                BaseUrl + path,
                method
            );

            request.downloadHandler = new DownloadHandlerBuffer();
            request.timeout = timeoutSeconds;

            yield return request.SendWebRequest();
            HandleResponse(request, onSuccess, onError);
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
            onError?.Invoke(message);
        }
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

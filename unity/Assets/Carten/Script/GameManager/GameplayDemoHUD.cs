using UnityEngine;
using UnityEngine.SceneManagement;

namespace Carten
{
    /// <summary>Small test-scene UI; production scenes do not use this component.</summary>
    public sealed class GameplayDemoHUD : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private BossController boss;
        [SerializeField] private BossResultReporter reporter;
        [SerializeField] private LastSparkSession session;
        private string username = "";
        private void Start()
        {
            if (LastSparkSession.Instance != null) session = LastSparkSession.Instance;
        }
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(12f, 12f, 460f, 245f), GUI.skin.box);
            GUILayout.Label("Move: A/D or arrows | Jump: Space | Dash: Shift");
            GUILayout.Label("Attack: Keypad 3 | Skills: Q/W/E | Interact: F");
            GUILayout.Label("Yellow: attack warning. Red: active damage. Use the switch to open the gate.");
            if (player != null)
                GUILayout.Label($"Player HP {player.CurrentHealth:F0}/{player.MaxHealth:F0} | {player.CurrentPhase} | Stunned: {player.IsStunned}");
            if (boss != null)
                GUILayout.Label($"{boss.Kind} HP {boss.CurrentHealth:F0}/{boss.MaxHealth:F0} | {boss.CurrentPhase}");
            if (session != null)
            {
                GUILayout.BeginHorizontal();
                username = GUILayout.TextField(username, 50, GUILayout.Width(200f));
                GUI.enabled = !session.IsConnecting && session.UserId == 0;
                if (GUILayout.Button("Connect new username")) session.Connect(username);
                GUI.enabled = true;
                GUILayout.EndHorizontal();
                GUILayout.Label("API: " + session.Status);
            }
            if (reporter != null) GUILayout.Label("Result: " + reporter.Status);
            if (GUILayout.Button("Restart demo"))
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            GUILayout.EndArea();
        }
    }
}

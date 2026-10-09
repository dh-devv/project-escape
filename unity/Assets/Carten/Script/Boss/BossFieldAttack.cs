using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Carten
{
    /// <summary>A visible warning, then a world-space hit box. Owned by its boss.</summary>
    public sealed class BossFieldAttack : MonoBehaviour
    {
        private BossController owner;
        private Vector2 size;
        private LayerMask playerLayer;
        private float damage;
        private float stun;
        private LineRenderer outline;
        private Material material;
        private bool cancelled;
        public bool IsArmed { get; private set; }

        public static BossFieldAttack Create(BossController boss, Vector2 center, Vector2 size,
            LayerMask playerLayer, float damage, float stun, float warning, float active)
        {
            GameObject go = new GameObject("Boss attack warning");
            go.transform.position = center;
            BossFieldAttack field = go.AddComponent<BossFieldAttack>();
            field.owner = boss;
            field.size = size;
            field.playerLayer = playerLayer;
            field.damage = damage;
            field.stun = stun;
            field.DrawWarning();
            field.StartCoroutine(field.Lifetime(Mathf.Max(0.1f, warning), Mathf.Max(0.05f, active)));
            return field;
        }

        private bool OwnerAlive => owner != null && !owner.IsDead && owner.isActiveAndEnabled;

        private void DrawWarning()
        {
            outline = gameObject.AddComponent<LineRenderer>();
            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                material = new Material(shader);
                outline.sharedMaterial = material;
            }
            outline.useWorldSpace = false;
            outline.loop = true;
            outline.positionCount = 4;
            outline.widthMultiplier = 0.12f;
            outline.sortingOrder = 20;
            Vector2 half = size * 0.5f;
            outline.SetPositions(new[] {
                new Vector3(-half.x, -half.y), new Vector3(-half.x, half.y),
                new Vector3(half.x, half.y), new Vector3(half.x, -half.y) });
            outline.startColor = outline.endColor = Color.yellow;
        }

        private IEnumerator Lifetime(float warning, float active)
        {
            yield return new WaitForSeconds(warning);
            if (cancelled || !OwnerAlive)
            {
                Destroy(gameObject);
                yield break;
            }
            IsArmed = true;
            outline.startColor = outline.endColor = Color.red;
            float expires = Time.time + active;
            HashSet<PlayerController> hitPlayers = new HashSet<PlayerController>();
            while (!cancelled && OwnerAlive && Time.time < expires)
            {
                foreach (Collider2D collider in Physics2D.OverlapBoxAll(transform.position, size, 0f, playerLayer))
                {
                    PlayerController player = collider.GetComponentInParent<PlayerController>();
                    if (player != null && !player.IsDead && hitPlayers.Add(player))
                        player.TakeDamage(damage, stun);
                }
                yield return new WaitForFixedUpdate();
            }
            IsArmed = false;
            Destroy(gameObject);
        }

        public void Cancel()
        {
            cancelled = true;
            IsArmed = false;
            StopAllCoroutines();
            Destroy(gameObject);
        }
        private void OnDisable() { cancelled = true; IsArmed = false; }
        private void OnDestroy() { if (material != null) Destroy(material); }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Carten
{
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

        private GameObject warningPrefab;
        private GameObject attackPrefab;
        private Sprite warningSprite;
        private Sprite attackSprite;
        private bool scaleToFit;
        private bool showOutline;

        private GameObject currentVisualInstance;
        private SpriteRenderer fallbackSpriteRenderer;

        public bool IsArmed { get; private set; }

        public static BossFieldAttack Create(
            BossController boss, Vector2 center, Vector2 size,
            LayerMask playerLayer, float damage, float stun, float warning, float active,
            GameObject warningPrefab = null, GameObject attackPrefab = null,
            Sprite warningSprite = null, Sprite attackSprite = null,
            bool scaleToFit = true, bool showOutline = true)
        {
            GameObject go = new GameObject("Boss attack warning");
            go.transform.position = center;
            BossFieldAttack field = go.AddComponent<BossFieldAttack>();
            field.owner = boss;
            field.size = size;
            field.playerLayer = playerLayer;
            field.damage = damage;
            field.stun = stun;
            field.warningPrefab = warningPrefab;
            field.attackPrefab = attackPrefab;
            field.warningSprite = warningSprite;
            field.attackSprite = attackSprite;
            field.scaleToFit = scaleToFit;
            field.showOutline = showOutline;

            if (showOutline)
                field.DrawWarning();

            field.ShowVisual(warningPrefab, warningSprite, sortingOrder: 18);
            field.StartCoroutine(field.Lifetime(Mathf.Max(0.1f, warning), Mathf.Max(0.05f, active)));
            return field;
        }

        private bool OwnerAlive => owner != null && !owner.IsDead && owner.isActiveAndEnabled;

        private void ShowVisual(GameObject prefab, Sprite sprite, int sortingOrder)
        {
            if (currentVisualInstance != null)
                Destroy(currentVisualInstance);

            if (prefab != null)
            {
                currentVisualInstance = Instantiate(prefab, transform.position, Quaternion.identity, transform);
                if (scaleToFit)
                {
                    SpriteRenderer sr = currentVisualInstance.GetComponentInChildren<SpriteRenderer>();
                    if (sr != null && sr.sprite != null)
                    {
                        Vector2 spriteSize = sr.sprite.bounds.size;
                        currentVisualInstance.transform.localScale = new Vector3(
                            spriteSize.x > 0 ? size.x / spriteSize.x : 1f,
                            spriteSize.y > 0 ? size.y / spriteSize.y : 1f,
                            1f);
                    }
                }
            }
            else if (sprite != null)
            {
                if (fallbackSpriteRenderer == null)
                    fallbackSpriteRenderer = gameObject.AddComponent<SpriteRenderer>();

                fallbackSpriteRenderer.sprite = sprite;
                fallbackSpriteRenderer.sortingOrder = sortingOrder;

                if (scaleToFit)
                {
                    Vector2 spriteSize = sprite.bounds.size;
                    transform.localScale = new Vector3(
                        spriteSize.x > 0 ? size.x / spriteSize.x : 1f,
                        spriteSize.y > 0 ? size.y / spriteSize.y : 1f,
                        1f);
                }
            }
        }

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
            if (outline != null)
                outline.startColor = outline.endColor = Color.red;

            if (attackPrefab != null || attackSprite != null)
            {
                transform.localScale = Vector3.one;
                ShowVisual(attackPrefab, attackSprite, sortingOrder: 19);
            }

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

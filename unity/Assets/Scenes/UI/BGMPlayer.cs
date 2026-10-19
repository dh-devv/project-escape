using UnityEngine;

namespace Carten
{
    public class BGMPlayer : MonoBehaviour
    {
        private static BGMPlayer instance;

        private void Awake()
        {
            // 중복 생성 방지 및 씬 전환 시 파괴되지 않음 처리
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}

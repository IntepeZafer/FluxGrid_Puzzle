using UnityEngine;
using System.Collections;
using PolarityGrid.Core;
using TMPro; // TextMeshPro için şart

namespace PolarityGrid.Blocks
{
    public class Block : MonoBehaviour
    {
        [SerializeField] private BlockType blockType;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private TextMeshPro symbolText; // Block içindeki TMP objesi
        [SerializeField] private float moveSpeed = 15f; 

        public BlockType Type => blockType;
        public bool IsMoving { get; private set; } 

        public void SetType(BlockType type)
        {
            blockType = type;
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            
            switch (blockType)
            {
                case BlockType.Positive: 
                    spriteRenderer.color = Color.blue; 
                    if(symbolText != null) symbolText.text = "+";
                    break;
                case BlockType.Negative: 
                    spriteRenderer.color = Color.red; 
                    if(symbolText != null) symbolText.text = "-";
                    break;
                case BlockType.Obstacle: 
                    spriteRenderer.color = Color.gray; 
                    if(symbolText != null) symbolText.text = ""; 
                    break;
            }
        }

        public void MoveTo(Vector3 targetPos)
        {
            StopAllCoroutines();
            StartCoroutine(MoveRoutine(targetPos));
        }

        private IEnumerator MoveRoutine(Vector3 target)
        {
            IsMoving = true;
            
            // Hareket başlarken hafif esneme (Squash)
            transform.localScale = new Vector3(1.1f, 0.9f, 1f);

            while (Vector3.Distance(transform.position, target) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = target;
            
            // Hedefe ulaştığında zıplama (Juice)
            float timer = 0;
            while (timer < 0.2f)
            {
                timer += Time.deltaTime;
                float scale = 1f + Mathf.Sin(timer * Mathf.PI / 0.2f) * 0.2f;
                transform.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }
            transform.localScale = Vector3.one;

            IsMoving = false;
        }
    }
}
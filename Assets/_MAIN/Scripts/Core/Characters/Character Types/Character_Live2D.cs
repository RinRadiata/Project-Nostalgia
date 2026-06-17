using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Live2D.Cubism.Framework.Expression;
using Live2D.Cubism.Rendering;
using System.Linq;

namespace CHARACTERS
{
    public class Character_Live2D : Character
    {
        public const float DEFAULT_TRANSITION_SPEED = 3f;
        public const int CHARACTER_SORTING_DEPTH_SIZE = 250;

        private CubismRenderController renderController;
        private CubismExpressionController expressionController;
        private Animator motionAnimator;

        private List<CubismRenderController> oldRenderers = new List<CubismRenderController>();

        private float xScale = 1f;

        public string activeExpression { get; private set; } = string.Empty;
        public string activeMotion { get; private set; } = string.Empty;

        public override bool isVisible
        {
            get
            {
                if (renderController == null)
                    return false;

                return isRevealing || renderController.Opacity == 1;
            }
            set
            {
                if (renderController == null)
                    return;

                renderController.Opacity = value ? 1 : 0;
            }
        }

        public Character_Live2D(string name, CharacterConfigData config, GameObject prefab, string rootAssetsFolder) : base(name, config, prefab)
        {
            Debug.Log($"Created a live2D character name: '{name}'");

            if (animator == null)
            {
                Debug.LogWarning($"Live2D character '{name}' has no base Animator.");
                return;
            }

            if (animator.transform.childCount > 0)
                motionAnimator = animator.transform.GetChild(0).GetComponentInChildren<Animator>();

            if (motionAnimator == null)
                motionAnimator = animator.GetComponentInChildren<Animator>();

            if (motionAnimator == null)
            {
                Debug.LogWarning($"Live2D character '{name}' has no motion Animator.");
                return;
            }

            renderController = motionAnimator.GetComponent<CubismRenderController>();

            if (renderController == null)
                renderController = motionAnimator.GetComponentInChildren<CubismRenderController>();

            expressionController = motionAnimator.GetComponent<CubismExpressionController>();

            if (expressionController == null)
                expressionController = motionAnimator.GetComponentInChildren<CubismExpressionController>();

            if (renderController != null)
                xScale = renderController.transform.localScale.x;
            else
                Debug.LogWarning($"Live2D character '{name}' has no CubismRenderController.");
        }

        public void SetMotion(string animationName)
        {
            if (string.IsNullOrWhiteSpace(animationName))
                return;

            if (motionAnimator == null)
            {
                Debug.LogWarning($"Live2D character '{name}' has no motion Animator.");
                return;
            }

            motionAnimator.Play(animationName);
            activeMotion = animationName;
        }

        public void SetExpression(int expressionIndex)
        {
            if (expressionController == null ||
                expressionController.ExpressionsList == null ||
                expressionController.ExpressionsList.CubismExpressionObjects == null)
            {
                Debug.LogWarning($"Live2D character '{name}' has no expression list.");
                return;
            }

            CubismExpressionData[] expressions = expressionController.ExpressionsList.CubismExpressionObjects;

            if (expressionIndex < 0 || expressionIndex >= expressions.Length)
            {
                Debug.LogWarning($"Live2D expression index '{expressionIndex}' does not exist on '{name}'.");
                return;
            }

            expressionController.CurrentExpressionIndex = expressionIndex;
            activeExpression = expressionIndex.ToString();
        }

        public void SetExpression(string expressionName)
        {
            if (string.IsNullOrWhiteSpace(expressionName))
                return;

            if (int.TryParse(expressionName, out int expressionIndex))
            {
                SetExpression(expressionIndex);
                return;
            }

            int index = GetExpressionIndexByName(expressionName);

            if (index < 0)
            {
                Debug.LogWarning($"Live2D expression '{expressionName}' does not exist on '{name}'.");
                return;
            }

            expressionController.CurrentExpressionIndex = index;
            activeExpression = expressionName;
        }

        private int GetExpressionIndexByName(string expressionName)
        {
            if (string.IsNullOrWhiteSpace(expressionName))
                return -1;

            if (expressionController == null ||
                expressionController.ExpressionsList == null ||
                expressionController.ExpressionsList.CubismExpressionObjects == null)
                return -1;

            string targetName = expressionName.ToLower();

            for (int i = 0; i < expressionController.ExpressionsList.CubismExpressionObjects.Length; i++)
            {
                CubismExpressionData expr = expressionController.ExpressionsList.CubismExpressionObjects[i];

                if (expr == null || string.IsNullOrWhiteSpace(expr.name))
                    continue;

                if (expr.name.Split('.')[0].ToLower() == targetName)
                    return i;
            }

            return -1;
        }

        public override IEnumerator ShowingOrHiding(bool show, float speedMultiplier = 1f)
        {
            if (renderController == null)
            {
                co_revealing = null;
                co_hiding = null;
                yield break;
            }

            float targetAlpha = show ? 1f : 0f;

            while (renderController.Opacity != targetAlpha)
            {
                renderController.Opacity = Mathf.MoveTowards(
                    renderController.Opacity,
                    targetAlpha,
                    DEFAULT_TRANSITION_SPEED * Time.deltaTime * speedMultiplier
                );

                yield return null;
            }

            co_revealing = null;
            co_hiding = null;
        }

        public override void SetColor(Color color)
        {
            base.SetColor(color);

            if (renderController == null || renderController.Renderers == null)
                return;

            foreach (CubismRenderer renderer in renderController.Renderers)
            {
                if (renderer != null)
                    renderer.Color = color;
            }
        }

        public override IEnumerator ChangingColor(Color color, float speed)
        {
            yield return ChangingColor2D(color, speed);

            co_changingColor = null;
        }

        public override IEnumerator Highlighting(bool highlight, float speedMultiplier, bool immediate = false)
        {
            if (renderController == null || renderController.Renderers == null)
            {
                co_highlighting = null;
                yield break;
            }

            if (!isChangingColor)
            {
                if (immediate)
                {
                    foreach (var renderer in renderController.Renderers)
                    {
                        if (renderer != null)
                            renderer.Color = displayColor;
                    }
                }
                else
                {
                    yield return ChangingColor2D(displayColor, speedMultiplier);
                }
            }

            co_highlighting = null;
        }

        public IEnumerator ChangingColor2D(Color targetColor, float speed)
        {
            if (renderController == null || renderController.Renderers == null || renderController.Renderers.Length == 0)
                yield break;

            CubismRenderer[] renderers = renderController.Renderers;
            Color startColors = renderers[0].Color;

            float colorPercent = 0;

            while (colorPercent != 1)
            {
                colorPercent = Mathf.Clamp01(colorPercent + (DEFAULT_TRANSITION_SPEED * Time.deltaTime * speed));
                Color currentColor = Color.Lerp(startColors, targetColor, colorPercent);

                foreach (CubismRenderer renderer in renderController.Renderers)
                {
                    if (renderer != null)
                        renderer.Color = currentColor;
                }

                yield return null;
            }
        }

        public override IEnumerator FaceDirection(bool faceleft, float speedMultiplier, bool immediate)
        {
            if (renderController == null)
            {
                co_flipping = null;
                yield break;
            }

            if (immediate)
            {
                renderController.transform.localScale = new Vector3(
                    faceleft ? xScale : -xScale,
                    renderController.transform.localScale.y,
                    renderController.transform.localScale.z
                );

                co_flipping = null;
                yield break;
            }

            GameObject newLive2DCharacter = CreateNewCharacterController();

            if (newLive2DCharacter == null)
            {
                co_flipping = null;
                yield break;
            }

            newLive2DCharacter.transform.localScale = new Vector3(
                faceleft ? xScale : -xScale,
                newLive2DCharacter.transform.localScale.y,
                newLive2DCharacter.transform.localScale.z
            );

            renderController.Opacity = 0;
            float transitionSpeed = DEFAULT_TRANSITION_SPEED * speedMultiplier * Time.deltaTime;

            while (renderController.Opacity < 1 || oldRenderers.Any(r => r != null && r.Opacity > 0))
            {
                renderController.Opacity = Mathf.MoveTowards(renderController.Opacity, 1, transitionSpeed);

                foreach (CubismRenderController oldRenderer in oldRenderers)
                {
                    if (oldRenderer != null)
                        oldRenderer.Opacity = Mathf.MoveTowards(oldRenderer.Opacity, 0, transitionSpeed);
                }

                yield return null;
            }

            foreach (CubismRenderController r in oldRenderers)
            {
                if (r != null)
                    Object.Destroy(r.gameObject);
            }

            oldRenderers.Clear();

            co_flipping = null;
        }

        private GameObject CreateNewCharacterController()
        {
            if (renderController == null)
                return null;

            oldRenderers.Add(renderController);

            GameObject newLive2DCharacter = Object.Instantiate(renderController.gameObject, renderController.transform.parent);
            newLive2DCharacter.name = name;

            renderController = newLive2DCharacter.GetComponent<CubismRenderController>();
            expressionController = newLive2DCharacter.GetComponent<CubismExpressionController>();
            motionAnimator = newLive2DCharacter.GetComponent<Animator>();

            return newLive2DCharacter;
        }

        public override void OnSort(int sortingIndex)
        {
            if (renderController == null)
                return;

            renderController.SortingOrder = sortingIndex * CHARACTER_SORTING_DEPTH_SIZE;
        }

        public override void OnReceiveCastingExpression(int layer, string expression)
        {
            SetExpression(expression);
        }
    }
}
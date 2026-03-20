using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Linq;

namespace CHARACTERS
{
    public class Character_Sprite : Character
    {
        private const string SPRITE_RENDERER_PARENT_NAME = "Renderers";
        private const string SPRITESHEET_DEFAULT_SHEETNAME = "Default";
        private const char SPRITESHEET_TEX_SPRITE_DELIMITER = '-';
        private CanvasGroup rootCG => root.GetComponent<CanvasGroup>();

        public List<CharacterSpriteLayer> layers = new List<CharacterSpriteLayer>();

        private string artAssetDirectory = "";
        public override bool isVisible
        {
            get { return isRevealing || rootCG.alpha == 1; }
            set { rootCG.alpha = value ? 1 : 0; }
        }
        public Character_Sprite(string name, CharacterConfigData config, GameObject prefab, string rootAssetsFolder) : base(name, config, prefab)
        {
            rootCG.alpha = ENABLE_ON_START ? 1 : 0;
            artAssetDirectory = rootAssetsFolder + "/Images";

            GetLayers();

            Debug.Log($"Created Sprite character: '{name}'");
        }

        private void GetLayers()
        {
            // get the animator component from the root object
            //Animator animator = root.GetC<Animator>();

            //if (animator == null)
            //    return;

            animator = root.GetComponent<Animator>();
            if (animator == null)
                animator = root.GetComponentInChildren<Animator>();


            Transform rendererRoot = animator.transform.Find(SPRITE_RENDERER_PARENT_NAME);
            if (rendererRoot == null)
                return;

            // get all the children of the renderer root
            for (int i = 0; i < rendererRoot.transform.childCount; i++)
            {
                Transform child = rendererRoot.transform.GetChild(i);

                Image rendererImage = child.GetComponentInChildren<Image>();


                if (rendererImage != null)
                {
                    // i = image layer that is being working on
                    CharacterSpriteLayer layer = new CharacterSpriteLayer(rendererImage, i);
                    layers.Add(layer);
                    child.name = $"Layer: {i}";

                }
            }
        }

        public void SetSprite(Sprite sprite, int layer = 0)
        {
            layers[layer].SetSprite(sprite);
        }

        public Sprite GetSprite(string spriteName)
        {
            //get sprite form the dictionary assigned to the character
            if (config.sprites.Count > 0)
            {
                if (config.sprites.TryGetValue(spriteName, out Sprite sprite))
                    return sprite;
            }

            // get the sprite from the art asset directory, if the sprite name is not found, return ull and log a warning
            if (config.characterType == CharacterType.SpriteSheet)
            {
                string[] data = spriteName.Split(SPRITESHEET_TEX_SPRITE_DELIMITER);
                Sprite[] spriteArray = new Sprite[0];

                if (data.Length == 2)
                {
                    string textureName = data[0];
                    spriteName = data[1];
                    spriteArray = Resources.LoadAll<Sprite>($"{artAssetDirectory}/{textureName}");
                }
                else
                {
                    spriteArray = Resources.LoadAll<Sprite>($"{artAssetDirectory}/{SPRITESHEET_DEFAULT_SHEETNAME}");
                }
                if (spriteArray.Length == 0)
                    Debug.LogWarning($"character '{name}' does not have a default art asset called '{SPRITESHEET_DEFAULT_SHEETNAME}'");

                return Array.Find(spriteArray, sprite => sprite.name == spriteName);
            }
            else
            {
                return Resources.Load<Sprite>($"{artAssetDirectory}/{spriteName}");
            }
        }

        // Transition the sprite of the layer to a new sprite. This will stop any current transition and start a new one.
        public Coroutine TransitionSrpite(Sprite sprite, int layer = 0, float speed = 1)
        {
            if (layer < 0 || layer >= layers.Count)
            {
                Debug.LogError($"Invalid layer index {layer} for character '{name}'. Layer count: {layers.Count}.");
                return null;
            }
            CharacterSpriteLayer spriteLayer = layers[layer];

            return spriteLayer.TransitionSprite(sprite, speed);
        }

        public override IEnumerator ShowingOrHiding(bool show, float speedMultiplier = 1f)
        {
            float targetAlpha = show ? 1f : 0;
            CanvasGroup self = rootCG;

            while (self.alpha != targetAlpha)
            {
                // remove spd multiplier if smthing is wrong
                self.alpha = Mathf.MoveTowards(self.alpha, targetAlpha, Time.deltaTime * 3f * speedMultiplier);
                yield return null;
            }

            co_revealing = null;
            co_hiding = null;
        }

        public override void SetColor(Color color)
        {
            base.SetColor(color);

            color = displayColor;

            foreach (CharacterSpriteLayer layer in layers)
            {
                layer.StopChangingColor();
                layer.SetColor(color);
            }
        }

        public override IEnumerator ChangingColor(Color color, float speed)
        {
            foreach (CharacterSpriteLayer layer in layers)
                layer.TransitionColor(color, speed);

            yield return null;

            // wait for all layers to finish changing color
            while (layers.Any(l => l.isChangingColor))
                yield return null;

            co_changingColor = null;
        }

        public override IEnumerator Highlighting(bool highlight, float speedMultiplier, bool immediate = false)
        {
            Color targetColor = displayColor;

            foreach (CharacterSpriteLayer layer in layers)
            {
                if (immediate)
                    layer.SetColor(displayColor);
                else
                    layer.TransitionColor(targetColor, speedMultiplier);
            } 

            yield return null;

            while (layers.Any(l => l.isChangingColor))
                yield return null;

            co_highlighting = null;
        }

        public override IEnumerator FaceDirection(bool faceLeft, float speedMultiplier, bool immediate)
        {
            foreach (CharacterSpriteLayer layer in layers)
            {
                if (faceLeft)
                    layer.FaceLeft(speedMultiplier, immediate);
                else
                    layer.FaceRight(speedMultiplier, immediate);
            }

            yield return null;

            while (layers.Any(l => l.isFlipping))
                yield return null;

            co_flipping = null;
        }

        public override void OnReceiveCastingExpression(int layer, string expression)
        {
            Sprite sprite = GetSprite(expression);

            if (sprite == null)
            {
                Debug.LogWarning($"Sprite '{expression}' could not be found for Character '{name}'");
                return;
            }

            TransitionSrpite(sprite, layer);
        }

    }
}
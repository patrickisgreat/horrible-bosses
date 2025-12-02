using UnityEngine;

namespace HorribleBosses.Boss
{
    /// <summary>
    /// Handles visual customization of boss appearance.
    /// Modifies mesh, materials, and scale based on BossData.
    /// </summary>
    public class BossCustomization : MonoBehaviour
    {
        [Header("Body Parts")]
        [SerializeField] private Transform bodyRoot;
        [SerializeField] private SkinnedMeshRenderer bodyRenderer;
        [SerializeField] private SkinnedMeshRenderer headRenderer;

        [Header("Material Slots")]
        [SerializeField] private int skinMaterialIndex = 0;
        [SerializeField] private int primaryClothingMaterialIndex = 1;
        [SerializeField] private int secondaryClothingMaterialIndex = 2;
        [SerializeField] private int hairMaterialIndex = 3;

        [Header("Body Type Scaling")]
        [SerializeField] private Vector3 skinnyScale = new Vector3(0.8f, 1f, 0.8f);
        [SerializeField] private Vector3 averageScale = Vector3.one;
        [SerializeField] private Vector3 largeScale = new Vector3(1.2f, 1.1f, 1.2f);
        [SerializeField] private Vector3 hugeScale = new Vector3(1.5f, 1.3f, 1.5f);

        [Header("Outfit Meshes")]
        [SerializeField] private Mesh businessSuitMesh;
        [SerializeField] private Mesh casualMesh;
        [SerializeField] private Mesh hawaiianShirtMesh;
        [SerializeField] private Mesh golfAttireMesh;
        [SerializeField] private Mesh powerSuitMesh;
        [SerializeField] private Mesh suspendersMesh;

        [Header("Head Meshes")]
        [SerializeField] private Mesh defaultHeadMesh;
        [SerializeField] private Mesh baldHeadMesh;
        [SerializeField] private Mesh comboverHeadMesh;
        [SerializeField] private Mesh slickedHeadMesh;
        [SerializeField] private Mesh messyHeadMesh;
        [SerializeField] private Mesh toupeeHeadMesh;

        [Header("Name Display")]
        [SerializeField] private TextMesh nameText;
        [SerializeField] private TextMesh titleText;

        private Material[] bodyMaterials;
        private Material[] headMaterials;

        private void Awake()
        {
            if (bodyRenderer != null)
                bodyMaterials = bodyRenderer.materials;
            if (headRenderer != null)
                headMaterials = headRenderer.materials;
        }

        public void ApplyCustomization(BossData data)
        {
            if (data == null) return;

            ApplyBodyType(data.bodyType);
            ApplyOutfit(data.outfit);
            ApplyHeadType(data.headType);
            ApplyColors(data);
            ApplyNameDisplay(data);
        }

        public void ApplyBodyType(BodyType bodyType)
        {
            if (bodyRoot == null) return;

            Vector3 scale = bodyType switch
            {
                BodyType.Skinny => skinnyScale,
                BodyType.Average => averageScale,
                BodyType.Large => largeScale,
                BodyType.Huge => hugeScale,
                _ => averageScale
            };

            bodyRoot.localScale = scale;
        }

        public void ApplyOutfit(OutfitType outfit)
        {
            if (bodyRenderer == null) return;

            Mesh mesh = outfit switch
            {
                OutfitType.BusinessSuit => businessSuitMesh,
                OutfitType.Casual => casualMesh,
                OutfitType.HawaiianShirt => hawaiianShirtMesh,
                OutfitType.GolfAttire => golfAttireMesh,
                OutfitType.PowerSuit => powerSuitMesh,
                OutfitType.Suspenders => suspendersMesh,
                _ => businessSuitMesh
            };

            if (mesh != null)
                bodyRenderer.sharedMesh = mesh;
        }

        public void ApplyHeadType(HeadType headType)
        {
            if (headRenderer == null) return;

            Mesh mesh = headType switch
            {
                HeadType.Default => defaultHeadMesh,
                HeadType.Bald => baldHeadMesh,
                HeadType.Combover => comboverHeadMesh,
                HeadType.Slicked => slickedHeadMesh,
                HeadType.Messy => messyHeadMesh,
                HeadType.Toupee => toupeeHeadMesh,
                _ => defaultHeadMesh
            };

            if (mesh != null)
                headRenderer.sharedMesh = mesh;
        }

        public void ApplyColors(BossData data)
        {
            SetMaterialColor(bodyMaterials, skinMaterialIndex, "_Color", data.skinTone);
            SetMaterialColor(headMaterials, skinMaterialIndex, "_Color", data.skinTone);
            SetMaterialColor(bodyMaterials, primaryClothingMaterialIndex, "_Color", data.primaryClothingColor);
            SetMaterialColor(bodyMaterials, secondaryClothingMaterialIndex, "_Color", data.secondaryClothingColor);
            SetMaterialColor(headMaterials, hairMaterialIndex, "_Color", data.hairColor);
        }

        private void SetMaterialColor(Material[] materials, int index, string propertyName, Color color)
        {
            if (materials == null || index < 0 || index >= materials.Length) return;
            if (materials[index] == null) return;

            if (materials[index].HasProperty(propertyName))
            {
                materials[index].SetColor(propertyName, color);
            }
            else if (materials[index].HasProperty("_BaseColor"))
            {
                materials[index].SetColor("_BaseColor", color);
            }
            else if (materials[index].HasProperty("_MainColor"))
            {
                materials[index].SetColor("_MainColor", color);
            }
        }

        public void ApplyNameDisplay(BossData data)
        {
            if (nameText != null)
                nameText.text = data.bossName;
            if (titleText != null)
                titleText.text = data.title;
        }

        public void SetSkinTone(Color color)
        {
            SetMaterialColor(bodyMaterials, skinMaterialIndex, "_Color", color);
            SetMaterialColor(headMaterials, skinMaterialIndex, "_Color", color);
        }

        public void SetPrimaryClothingColor(Color color)
        {
            SetMaterialColor(bodyMaterials, primaryClothingMaterialIndex, "_Color", color);
        }

        public void SetSecondaryClothingColor(Color color)
        {
            SetMaterialColor(bodyMaterials, secondaryClothingMaterialIndex, "_Color", color);
        }

        public void SetHairColor(Color color)
        {
            SetMaterialColor(headMaterials, hairMaterialIndex, "_Color", color);
        }
    }
}

﻿using System;
using SharpDX;
using WoWEditor6.Scene;
using WoWEditor6.Utils;

namespace WoWEditor6.Editing
{
    enum TerrainChangeType
    {
        Elevate,
        Flatten,
        Blur,
        Shading
    }

    enum TerrainAlgorithm
    {
        Flat,
        Linear,
        Quadratic,
        Trigonometric
    }

    class TerrainChangeParameters
    {
        public TerrainChangeType Method;
        public TerrainAlgorithm Algorithm;
        public Vector3 Center;
        public float OuterRadius;
        public float InnerRadius;
        public TimeSpan TimeDiff;
        public Vector3 Shading;
        public float Amount;
        public bool Inverted;
        public bool AlignModels;
    }

    class TerrainChangeManager
    {
        public static TerrainChangeManager Instance { get; private set; }

        private float mInnerRadius = 45.0f;
        private float mOuterRadius = 55.0f;

        public TerrainChangeType ChangeType { get; set; }
        public TerrainAlgorithm ChangeAlgorithm { get; set; }
        public Vector3 MousePosition { get; set; }
        public bool IsTerrainHovered { get; set; }
        public Vector3 ShadingMultiplier { get; set; }
        public float Amount { get; set; }
        public bool AlignModelsToGround { get; set; }

        public float InnerRadius
        {
            get { return mInnerRadius; }
            set
            {
                mInnerRadius = value;
                WorldFrame.Instance.UpdateTerrainBrush(mInnerRadius, mOuterRadius);
            }
        }

        public float OuterRadius
        {
            get { return mOuterRadius; }
            set
            {
                mOuterRadius = value;
                WorldFrame.Instance.UpdateTerrainBrush(mInnerRadius, mOuterRadius);
            }
        }

        static TerrainChangeManager()
        {
            Instance = new TerrainChangeManager();
        }

        public TerrainChangeManager()
        {
            ChangeType = TerrainChangeType.Elevate;
            ChangeAlgorithm = TerrainAlgorithm.Linear;
            ShadingMultiplier = Vector3.One;
            Amount = 15.0f;
            AlignModelsToGround = false;
        }

        public void OnChange(TimeSpan diff)
        {
            bool inverted;
            if (CheckRequirements(out inverted) == false)
                return;

            var parameters = new TerrainChangeParameters()
            {
                Algorithm = ChangeAlgorithm,
                Center = MousePosition,
                InnerRadius = mInnerRadius,
                OuterRadius = mOuterRadius,
                Method = ChangeType,
                TimeDiff = diff,
                Shading = ShadingMultiplier,
                Amount = Amount,
                Inverted = inverted,
                AlignModels = AlignModelsToGround
            };

            WorldFrame.Instance.MapManager.OnEditTerrain(parameters);
        }

        private bool CheckRequirements(out bool isInverted)
        {
            isInverted = false;
            if (IsTerrainHovered == false)
                return false;

            var bindings = Settings.KeyBindings.Instance;
            var state = new byte[256];
            UnsafeNativeMethods.GetKeyboardState(state);

            if (KeyHelper.AreKeysDown(state, bindings.Interaction.Edit) == false &&
                KeyHelper.AreKeysDown(state, bindings.Interaction.EditInverse) == false)
                return false;

            if (KeyHelper.AreKeysDown(state, bindings.Interaction.EditInverse))
                isInverted = true;

            return true;
        }
    }
}
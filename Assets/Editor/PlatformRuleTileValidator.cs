using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace ProjectWoods.Editor
{
    /// <summary>
    /// Verifies that the paired red-border and pink-background Rule Tiles cover
    /// the complete 47-tile blob topology used by Terrain (32x32).png.
    /// </summary>
    public static class PlatformRuleTileValidator
    {
        private const string BorderPath = "Assets/Sprites/Platform_Border_Final.asset";
        private const string BackgroundPath = "Assets/Sprites/Platform_Inner_Pink_Final.asset";
        private const string TerrainTexturePath = "Assets/Sprites/Terrain (32x32).png";

        // Clockwise from north-west. Bit order only needs to stay consistent here.
        private static readonly Vector3Int[] NeighborOffsets =
        {
            new(-1, 1, 0), new(0, 1, 0), new(1, 1, 0), new(1, 0, 0),
            new(1, -1, 0), new(0, -1, 0), new(-1, -1, 0), new(-1, 0, 0),
        };

        [MenuItem("Tools/Platform Rule Tiles/Validate")]
        public static void ValidateFromMenu()
        {
            Validate();
            Debug.Log("Platform Rule Tile validation passed: all 256 neighbor layouts are covered.");
        }

        [MenuItem("Tools/Platform Rule Tiles/Rebuild Red Border From Pink Topology")]
        public static void RebuildBorderFromBackground()
        {
            RuleTile border = LoadRuleTile(BorderPath);
            RuleTile background = LoadRuleTile(BackgroundPath);
            Dictionary<int, Sprite> sprites = AssetDatabase.LoadAllAssetsAtPath(TerrainTexturePath)
                .OfType<Sprite>()
                .ToDictionary(SpriteIndex);
            Dictionary<string, RuleTile.TilingRule> backgroundRulesBySignature =
                background.m_TilingRules.ToDictionary(rule => RuleSignature(rule, false));

            int centerIndex = SpriteIndex(background.m_DefaultSprite);
            var rebuiltRules = new List<RuleTile.TilingRule>();

            foreach (RuleTile.TilingRule source in background.m_TilingRules)
            {
                int sourceBackgroundIndex = SpriteIndex(source.m_Sprites[0]);
                string oppositeSignature = RuleSignature(source, true);
                Require(backgroundRulesBySignature.TryGetValue(oppositeSignature, out RuleTile.TilingRule opposite),
                    $"Pink topology has no 180-degree counterpart for rule {source.m_Id}.");

                int oppositeBackgroundIndex = SpriteIndex(opposite.m_Sprites[0]);
                if (oppositeBackgroundIndex == centerIndex)
                    continue;

                // The red artwork is inward-facing: for a topologically open north edge,
                // use the art paired with the pink south edge (and likewise for corners).
                int borderIndex = ExpectedBorderIndex(sourceBackgroundIndex, oppositeBackgroundIndex);
                Require(sprites.TryGetValue(borderIndex, out Sprite borderSprite),
                    $"Could not find paired red sprite Terrain (32x32)_{borderIndex}.");

                rebuiltRules.Add(new RuleTile.TilingRule
                {
                    m_Id = source.m_Id,
                    m_Sprites = new[] { borderSprite },
                    m_GameObject = null,
                    m_MinAnimationSpeed = 1f,
                    m_MaxAnimationSpeed = 1f,
                    m_PerlinScale = 0.5f,
                    m_Output = RuleTile.TilingRuleOutput.OutputSprite.Single,
                    m_ColliderType = Tile.ColliderType.Grid,
                    m_RandomTransform = RuleTile.TilingRuleOutput.Transform.Fixed,
                    m_Neighbors = new List<int>(source.m_Neighbors),
                    m_NeighborPositions = new List<Vector3Int>(source.m_NeighborPositions),
                    m_RuleTransform = RuleTile.TilingRuleOutput.Transform.Fixed,
                });
            }

            Require(rebuiltRules.Count == 46,
                $"Expected 46 non-center border rules, generated {rebuiltRules.Count}.");

            Undo.RecordObject(border, "Rebuild platform border Rule Tile");
            border.m_DefaultSprite = background.m_DefaultSprite;
            border.m_DefaultGameObject = null;
            border.m_DefaultColliderType = Tile.ColliderType.None;
            border.m_TilingRules = rebuiltRules;
            EditorUtility.SetDirty(border);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Rebuilt Platform_Border_Final from the pink tile's verified 47-tile topology.");
        }

        // Entry point for: Unity -batchmode -executeMethod
        public static void ValidateBatchMode()
        {
            try
            {
                Validate();
                Debug.Log("PASS: Platform Rule Tile validation OK (256/256 neighbor layouts).");
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        public static void RebuildAndValidateBatchMode()
        {
            try
            {
                RebuildBorderFromBackground();
                Validate();
                Debug.Log("PASS: Rebuilt and validated Platform Rule Tiles (256/256 neighbor layouts).");
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        public static void Validate()
        {
            RuleTile border = LoadRuleTile(BorderPath);
            RuleTile background = LoadRuleTile(BackgroundPath);

            Require(border.m_TilingRules.Count == 46,
                $"Border should contain 46 visible edge cases, but has {border.m_TilingRules.Count}.");
            Require(background.m_TilingRules.Count == 47,
                $"Background should contain all 47 blob cases, but has {background.m_TilingRules.Count}.");
            Require(border.m_DefaultSprite == background.m_DefaultSprite,
                "Platform default must use the pink center sprite for fully surrounded cells.");
            Require(background.m_DefaultSprite != null,
                "Background needs a visible default/center sprite.");
            Require(border.m_DefaultColliderType == Tile.ColliderType.None,
                "Platform center must not create a collider; only red boundary rules are solid.");
            Require(background.m_DefaultColliderType == Tile.ColliderType.None,
                "Background must not duplicate platform colliders.");

            ValidateRuleContents(border, 0, 46, Tile.ColliderType.Grid, "border");
            ValidateRuleContents(background, 47, 93, Tile.ColliderType.None, "background");
            ValidateEveryNeighborhood(border, background);
        }

        private static RuleTile LoadRuleTile(string path)
        {
            RuleTile tile = AssetDatabase.LoadAssetAtPath<RuleTile>(path);
            Require(tile != null, $"Could not load Rule Tile at {path}.");
            return tile;
        }

        private static void ValidateRuleContents(
            RuleTile tile,
            int minimumSpriteIndex,
            int maximumSpriteIndex,
            Tile.ColliderType expectedCollider,
            string label)
        {
            var seenSprites = new HashSet<int>();

            foreach (RuleTile.TilingRule rule in tile.m_TilingRules)
            {
                Require(rule.m_Output == RuleTile.TilingRuleOutput.OutputSprite.Single,
                    $"The {label} tile contains a non-single sprite output.");
                Require(rule.m_RuleTransform == RuleTile.TilingRuleOutput.Transform.Fixed,
                    $"The {label} tile must not rotate asymmetric pixel art.");
                Require(rule.m_ColliderType == expectedCollider,
                    $"The {label} tile contains an incorrect collider override.");
                Require(rule.m_Sprites != null && rule.m_Sprites.Length == 1 && rule.m_Sprites[0] != null,
                    $"The {label} tile contains a missing sprite reference.");

                int index = SpriteIndex(rule.m_Sprites[0]);
                Require(index >= minimumSpriteIndex && index <= maximumSpriteIndex,
                    $"The {label} rule references unexpected sprite {rule.m_Sprites[0].name}.");
                bool added = seenSprites.Add(index);
                if (minimumSpriteIndex == 47)
                {
                    Require(added,
                        $"The {label} tile uses sprite index {index} more than once.");
                }
            }

            int expectedUniqueCount = minimumSpriteIndex == 0 ? 42 : 47;
            Require(seenSprites.Count == expectedUniqueCount,
                $"The {label} tile uses {seenSprites.Count}/{expectedUniqueCount} expected unique sprites.");
        }

        private static void ValidateEveryNeighborhood(RuleTile border, RuleTile background)
        {
            var gridObject = new GameObject("RuleTileValidationGrid", typeof(Grid));
            var borderObject = new GameObject("Border", typeof(Tilemap), typeof(TilemapRenderer));
            var backgroundObject = new GameObject("Background", typeof(Tilemap), typeof(TilemapRenderer));
            borderObject.transform.SetParent(gridObject.transform);
            backgroundObject.transform.SetParent(gridObject.transform);

            try
            {
                Tilemap borderMap = borderObject.GetComponent<Tilemap>();
                Tilemap backgroundMap = backgroundObject.GetComponent<Tilemap>();
                var borderSpriteUse = new HashSet<int>();
                var backgroundSpriteUse = new HashSet<int>();

                for (int mask = 0; mask < 256; mask++)
                {
                    Sprite borderSprite = Resolve(border, borderMap, mask);
                    Sprite backgroundSprite = Resolve(background, backgroundMap, mask);

                    Require(backgroundSprite != null,
                        $"Background has no output for neighbor mask 0x{mask:X2}.");

                    int backgroundIndex = SpriteIndex(backgroundSprite);
                    backgroundSpriteUse.Add(backgroundIndex);

                    if (mask == 255)
                    {
                        Require(borderSprite == backgroundSprite,
                            "A fully surrounded platform cell should use the pink center sprite.");
                        continue;
                    }

                    Require(borderSprite != null,
                        $"Border has no output for neighbor mask 0x{mask:X2}.");
                    int borderIndex = SpriteIndex(borderSprite);
                    borderSpriteUse.Add(borderIndex);

                    int oppositeMask = RotateMask180(mask);
                    Sprite oppositeBackgroundSprite = Resolve(background, backgroundMap, oppositeMask);
                    int expectedBorderIndex = ExpectedBorderIndex(
                        backgroundIndex,
                        SpriteIndex(oppositeBackgroundSprite));
                    Require(borderIndex == expectedBorderIndex,
                        $"Red rim faces the wrong way for mask 0x{mask:X2}: " +
                        $"got _{borderIndex}, expected _{expectedBorderIndex}.");
                }

                Require(borderSpriteUse.Count == 42,
                    $"Topology simulation reached {borderSpriteUse.Count}/42 expected border sprites.");
                Require(backgroundSpriteUse.Count == 47,
                    $"Topology simulation reached {backgroundSpriteUse.Count}/47 background sprites.");

                // Visual contract from the target reference: red is outside and cream is inside.
                Require(SpriteIndex(Resolve(border, borderMap, 0xF8)) == 24, "Top edge must use red-above/cream-below sprite _24.");
                Require(SpriteIndex(Resolve(border, borderMap, 0x8F)) == 1, "Bottom edge must use cream-above/red-below sprite _1.");
                Require(SpriteIndex(Resolve(border, borderMap, 0x3E)) == 13, "Left edge must use red-left/cream-right sprite _13.");
                Require(SpriteIndex(Resolve(border, borderMap, 0xE3)) == 11, "Right edge must use cream-left/red-right sprite _11.");
                Require(SpriteIndex(Resolve(border, borderMap, 0x38)) == 4, "Top-left outer corner must use sprite _4.");
                Require(SpriteIndex(Resolve(border, borderMap, 0xE0)) == 5, "Top-right outer corner must use sprite _5.");
                Require(SpriteIndex(Resolve(border, borderMap, 0x0E)) == 15, "Bottom-left outer corner must use sprite _15.");
                Require(SpriteIndex(Resolve(border, borderMap, 0x83)) == 16, "Bottom-right outer corner must use sprite _16.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gridObject);
            }
        }

        private static Sprite Resolve(RuleTile tile, Tilemap map, int mask)
        {
            map.ClearAllTiles();
            map.SetTile(Vector3Int.zero, tile);

            for (int bit = 0; bit < NeighborOffsets.Length; bit++)
            {
                if ((mask & (1 << bit)) != 0)
                    map.SetTile(NeighborOffsets[bit], tile);
            }

            var tileData = new TileData();
            tile.GetTileData(Vector3Int.zero, map, ref tileData);
            return tileData.sprite;
        }

        private static int SpriteIndex(Sprite sprite)
        {
            string suffix = sprite.name.Split('_').Last();
            Require(int.TryParse(suffix, out int index),
                $"Sprite name does not end in an index: {sprite.name}.");
            return index;
        }

        private static string RuleSignature(RuleTile.TilingRule rule, bool rotate180)
        {
            return string.Join(";", rule.m_NeighborPositions
                .Select((position, index) => new
                {
                    Position = rotate180 ? new Vector3Int(-position.x, -position.y, position.z) : position,
                    Neighbor = rule.m_Neighbors[index],
                })
                .OrderBy(item => item.Position.x)
                .ThenBy(item => item.Position.y)
                .ThenBy(item => item.Position.z)
                .Select(item => $"{item.Position.x},{item.Position.y},{item.Position.z}:{item.Neighbor}"));
        }

        private static int RotateMask180(int mask)
        {
            int rotated = 0;
            for (int bit = 0; bit < 8; bit++)
            {
                if ((mask & (1 << bit)) != 0)
                    rotated |= 1 << ((bit + 4) % 8);
            }
            return rotated;
        }

        private static int ExpectedBorderIndex(int sourceBackgroundIndex, int oppositeBackgroundIndex)
        {
            // The red family has dedicated compact convex corners. The superficially
            // corresponding thick corners (_25/_23/_2/_0) extend cream strips to the
            // outer edge and create the protrusions visible in the failed screenshot.
            return sourceBackgroundIndex switch
            {
                47 => 4,  // top-left
                49 => 5,  // top-right
                70 => 15, // bottom-left
                72 => 16, // bottom-right
                _ => oppositeBackgroundIndex - 47,
            };
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }
    }
}

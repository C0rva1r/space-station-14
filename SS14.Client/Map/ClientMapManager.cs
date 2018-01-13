﻿using SS14.Shared.Map;
using System.Collections.Generic;
using SS14.Shared.Interfaces.Map;
using SS14.Client.Interfaces.Map;
using SS14.Shared.IoC;
using SS14.Client.Interfaces;
using SS14.Shared.Log;

namespace SS14.Client.Map
{
    public class ClientMapManager : MapManager
    {
        [Dependency]
        private IClientTileDefinitionManager tileDefinitionManager;
        [Dependency]
        private ISceneTreeHolder sceneTree;

        private Dictionary<(int mapId, int gridId), Godot.TileMap> RenderTileMaps = new Dictionary<(int mapId, int gridId), Godot.TileMap>();

        public ClientMapManager()
        {
            OnTileChanged += UpdateTileMapOnUpdate;
            OnGridCreated += UpdateOnGridCreated;
            OnGridRemoved += UpdateOnGridRemoved;
        }

        private void UpdateTileMapOnUpdate(TileRef tileRef, Tile oldTile)
        {
            var tilemap = RenderTileMaps[(tileRef.MapIndex, tileRef.GridIndex)];
            tilemap.SetCell(tileRef.X, tileRef.Y, tileRef.Tile.TileId);
        }

        private void UpdateOnGridCreated(int mapId, int gridId)
        {
            var tilemap = new Godot.TileMap
            {
                TileSet = tileDefinitionManager.TileSet,
                // TODO: Unhardcode this cell size.
                CellSize = new Godot.Vector2(32, 32),
                ZIndex = -10,
                // Fiddle with this some more maybe. Increases lighting performance a TON.
                CellQuadrantSize = 4,
                //Visible = false,
            };
            tilemap.SetName($"Grid {mapId}.{gridId}");
            sceneTree.WorldRoot.AddChild(tilemap);
            RenderTileMaps[(mapId, gridId)] = tilemap;
        }

        private void UpdateOnGridRemoved(int mapId, int gridId)
        {
            Logger.Debug($"Removing grid {mapId}.{gridId}");
            var tilemap = RenderTileMaps[(mapId, gridId)];
            tilemap.QueueFree();
            tilemap.Dispose();
            RenderTileMaps.Remove((mapId, gridId));
        }
    }
}

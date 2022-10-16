using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Sirenix.OdinInspector;

public class BuildingSystem : Singleton<BuildingSystem>
{
    [Title("Traps")]
    public List<TrapsContainer> Traps = new List<TrapsContainer>();

    [Title("Grid")]
    [SerializeField]
    private GridLayout _gridLayout;

    [SerializeField]
    private Tilemap _tileMap;
    private Grid _grid;

    private TypeTrap _currentTypeSelected;
    private TileBase _currentTileSelected;
    private List<TileBase> _occupiedTiles = new List<TileBase>();

    #region GETTERS AND SETTERS
    public GridLayout GetGridLayout { get { return _gridLayout; } }
    #endregion

    #region METHODS PLACE TRAP
    public void PlaceTrap(TileBase tile, TypeTrap type)
    {
        TrapBehaviour trap;
        int length = Traps.Count;
        for (int i = 0; i < length; ++i)
        {
            if (Traps[i].IsMyType(type))
            {
                trap = Traps[i].Trap;
                break;
            }
        }
        // TODO:
    }
    #endregion

    #region UTILS
    public Vector3 SnapCoordinateToGrid(Vector3 position)
    {
        return _gridLayout.WorldToCell(position);
    }
    public bool IsThisTileAvaliable(Vector3Int posTile)
    {
        var tile = _tileMap.GetTile(posTile);
        return !_occupiedTiles.Contains(tile);
    }
    public bool IsThisTileAvaliable(TileBase tile)
    {
        return !_occupiedTiles.Contains(tile);
    }
    #endregion

    #region UNITY METHODS
    private void OnValidate()
    {
        Utils.ValidationUtility.SafeOnValidate(() =>
        {
            if (this == null) return;
            if (_grid == null && _gridLayout != null) _grid = _gridLayout.gameObject.GetComponent<Grid>();
        });
    }
    #endregion
}

[System.Serializable]
public class TrapsContainer
{
    public TypeTrap Type;
    public TrapBehaviour Trap;

    public bool IsMyType(TypeTrap type) => Type == type;
}

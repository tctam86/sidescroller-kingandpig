using UnityEngine;

[CreateAssetMenu(fileName = "EnemyConfig", menuName = "Scriptable Objects/EnemyConfig")]
public class EnemyConfig : ScriptableObject
{
    [Header("Health")]
    [Min(1)]
    public int maximumHeart = 3;

    [Header("Combat")]
    [Min(0)]
    public int contactDamageUnits = 1;

    [Min(0)]
    public int attackDamageUnits = 1;


    [Header("Movement")]
    [Min(0f)]
    public float moveSpeed = 2f;


    [Header("Diamond Drop")]
    public GameObject droppedDiamondPrefab;
    [Min(0)]
    public int minimumDiamondDrop = 1;
    [Min(0)]
    public int maximumDiamondDrop = 3;
    [Min(0f)]
    public float horizontalDropForce = 2f;
    public Vector2 verticalDropForceRange = new Vector2(3f, 5f);
}

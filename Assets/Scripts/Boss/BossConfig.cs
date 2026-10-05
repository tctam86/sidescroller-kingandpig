using UnityEngine;

[CreateAssetMenu(fileName = "BossConfig", menuName = "Scriptable Objects/BossConfig")]
public class BossConfig : ScriptableObject
{
    [Header("Health")]
    [Min(1)]
    public int maximumHeart = 20;

    [Header("Combat")]
    [Min(0)]
    public int contactDamageUnits = 1;
    [Min(0)]
    public int attackDamageUnits = 1;


    [Header("Movement")]
    public float moveSpeed = 2f;

    [Header("Diamond Drop")]
    public GameObject droppedDiamondPrefab;
    [Min(0)]
    public int diamondDrop = 10;
    [Min(0f)]
    public float horizontalDropForce = 2f;
    public Vector2 verticalDropForceRange = new Vector2(3f, 5f);
}

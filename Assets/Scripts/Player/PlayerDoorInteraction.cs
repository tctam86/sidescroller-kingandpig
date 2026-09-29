using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDoorInteraction : MonoBehaviour
{
    private LevelDoor currentDoor;

    private Animator animator;


    private void OnInteract(InputValue value)
    {

        if (!value.isPressed)
        {
            return;
        }

        if (currentDoor == null)
        {
            return;
        }
        currentDoor.TryEnter(gameObject);


    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out LevelDoor door))
        {
            currentDoor = door;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out LevelDoor door) &&
            door == currentDoor)
        {
            currentDoor = null;
        }
    }
}


